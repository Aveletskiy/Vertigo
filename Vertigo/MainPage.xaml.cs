using System;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Vertigo
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Geolocator Geolocator { get; set; }
        private Geoposition Geoposition { get; set; }
        public static MainPage Current;
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            NotifyUser(String.Empty, NotifyType.StatusMessage);
            frameMain.Navigate(typeof(Coords));
            textBlockTitle.Text = "Координаты";
            Geolocator = new Geolocator() { ReportInterval = 1000 };
            Geolocator.DesiredAccuracy = PositionAccuracy.High;
            Geolocator.StatusChanged += Geolocator_StatusChanged;
            Geolocator.PositionChanged += Geolocator_PositionChanged;
        }

        private async void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Geoposition == null || args.Position.Coordinate.Accuracy <= Geoposition.Coordinate.Accuracy)
                {
                    Geoposition = args.Position;
                    TextBlockCoords.Text = Geoposition.Coordinate.Latitude.ToString() + ' ' 
                    + Geoposition.Coordinate.Longitude.ToString()+':'+ Geoposition.Coordinate.Accuracy.ToString();
                }
                
            });
        }

        internal void RewriteStatus(StatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                case PositionStatus.Ready:
                    // Location platform is providing valid data.
                    StatusLabel.Text = "Status: Ready";
                    break;

                case PositionStatus.Initializing:
                    // Location platform is attempting to acquire a fix. 
                    StatusLabel.Text = "Status: Initializing";
                    break;

                case PositionStatus.NoData:
                    // Location platform could not obtain location data.
                    StatusLabel.Text = "Status: No data";
                    break;

                case PositionStatus.Disabled:
                    // The permission to access location data is denied by the user or other policies.
                    StatusLabel.Text = "Status: Disabled";
                    break;

                case PositionStatus.NotInitialized:
                    // The location platform is not initialized. This indicates that the application 
                    // has not made a request for location data.
                    StatusLabel.Text = "Status: Not initialized";
                    break;

                case PositionStatus.NotAvailable:
                    // The location platform is not available on this version of the OS.
                    StatusLabel.Text = "Status: Not available";
                    break;

                default:
                    StatusLabel.Text = "Status: Unknown";
                    break;
            }
        }

        /// <summary>
        /// Event handler for StatusChanged events. It is raised when the 
        /// location status in the system changes.
        /// </summary>
        /// <param name="sender">Geolocator instance</param>
        /// <param name="args">Statu data</param>
        async private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the location setting message only if status is disabled.


            });
        }





         private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Messages.IsSelected)
            {
                frameMain.Navigate(typeof(Messages));
                textBlockTitle.Text = "Сообщения";
            }
            else if (Coords.IsSelected)
            {
                frameMain.Navigate(typeof(Coords));
                textBlockTitle.Text = "Координаты";
            }
            else if (Payment.IsSelected)
            {
                frameMain.Navigate(typeof(Payment));
                textBlockTitle.Text = "Оплата";
            }
            else if (Settings.IsSelected)
            {
                frameMain.Navigate(typeof(Settings));
                textBlockTitle.Text = "Настройки";
            }

        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }
        private string GetStatusString(PositionStatus status)
        {
            var strStatus = "";

            switch (status)
            {
                case PositionStatus.Ready:
                    strStatus = "Location is available.";
                    break;

                case PositionStatus.Initializing:
                    strStatus = "Geolocation service is initializing.";
                    break;

                case PositionStatus.NoData:
                    strStatus = "Location service data is not available.";
                    break;

                case PositionStatus.Disabled:
                    strStatus = "Location services are disabled. Use the " +
                                "Settings charm to enable them.";
                    break;

                case PositionStatus.NotInitialized:
                    strStatus = "Location status is not initialized because " +
                                "the app has not yet requested location data.";
                    break;

                case PositionStatus.NotAvailable:
                    strStatus = "Location services are not supported on your system.";
                    break;

                default:
                    strStatus = "Unknown PositionStatus value.";
                    break;
            }

            return (strStatus);

        }


        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }


    }
}
