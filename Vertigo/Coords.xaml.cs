using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Vertigo
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Coords : Page
    {
        private Geolocator _geolocator { get; set; }
        private Geoposition Geoposition { get; set; }
        private MapIcon MapIcon { get; set; }

        //// A pointer to the main page
        private MainPage _rootPage = MainPage.Current;

        public Coords()
        {
            this.InitializeComponent();



            //            _geolocator = new Geolocator() { ReportInterval = 1000 };
            //          _geolocator.DesiredAccuracy = PositionAccuracy.High;
            //            _geolocator.StatusChanged += Geolocator_StatusChanged;
            //          _geolocator.PositionChanged += Geolocator_PositionChanged;


            mapControl.MapServiceToken = "nD18JaojG92g4lqLyhzI~wo1ajtwOwbPAoUAUUxu6Sg~ArEdm5wChACupRSXm50wPw8v7drL6dNMRMWztobTJOj1rrcNoh0uIch5I_XHnOvp";
            zoomLevel.Minimum = mapControl.MinZoomLevel;
            zoomLevel.Maximum = mapControl.MaxZoomLevel;
            mapControl.ZoomLevel = 17;
            UpdateLocationData();
        }

        private void TrackingInit(object sender, RoutedEventArgs e)
        {
            if ((bool)TrackingButton.IsChecked) StartTracking(sender, e); else StopTracking(sender, e);
        }

        private async void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Geoposition == null || args.Position.Coordinate.Accuracy <= Geoposition.Coordinate.Accuracy)
                {
                    Geoposition = args.Position;
                    if (MapIcon != null)
                    {
                        RemoveMapIcon(MapIcon);
                    }
                    MapIcon = AddGeopoint(args.Position.Coordinate.Point, "Вы");
                    //mapControl.Center = Geoposition.Coordinate.Point;
                    UpdateLocationData();
                }
            });
        }


        private async void StartTracking(object sender, RoutedEventArgs e)
        {
            // Request permission to access location
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // You should set MovementThreshold for distance-based tracking
                    // or ReportInterval for periodic-based tracking before adding event
                    // handlers. If none is set, a ReportInterval of 1 second is used
                    // as a default and a position will be returned every 1 second.
                    //
                    // Value of 2000 milliseconds (2 seconds) 
                    // isn't a requirement, it is just an example.
                    _geolocator = new Geolocator { ReportInterval = 1000 };

                    // Subscribe to PositionChanged event to get updated tracking positions
                    _geolocator.PositionChanged += Geolocator_PositionChanged;


                    // Subscribe to StatusChanged event to get updates of location status changes
                    _geolocator.StatusChanged += Geolocator_StatusChanged;

                    _rootPage.NotifyUser("Waiting for update...", MainPage.NotifyType.StatusMessage);
                    LocationDisabledMessage.Visibility = Visibility.Collapsed;
                    //TrackingButton.IsChecked = true;

                    break;

                case GeolocationAccessStatus.Denied:
                    _rootPage.NotifyUser("Access to location is denied.", MainPage.NotifyType.ErrorMessage);
                    LocationDisabledMessage.Visibility = Visibility.Visible;
                    break;

                case GeolocationAccessStatus.Unspecified:
                    _rootPage.NotifyUser("Unspecificed error!", MainPage.NotifyType.ErrorMessage);
                    LocationDisabledMessage.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void StopTracking(object sender, RoutedEventArgs e)
        {
            _geolocator.PositionChanged -= Geolocator_PositionChanged;
            _geolocator.StatusChanged -= Geolocator_StatusChanged;
            _geolocator.DesiredAccuracy = PositionAccuracy.High;
            _geolocator = null;

            //TrackingButton.IsEnabled = false;

            // Clear status
            _rootPage.NotifyUser("Tracking stoped", MainPage.NotifyType.StatusMessage);
        }

        async private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the location setting message only if status is disabled.
                LocationDisabledMessage.Visibility = Visibility.Collapsed;

                switch (args.Status)
                {
                    case PositionStatus.Ready:
                        // Location platform is providing valid data.
                        _rootPage.RewriteStatus (args);
                        _rootPage.NotifyUser("Location platform is ready.", MainPage.NotifyType.StatusMessage);
                        break;

                    case PositionStatus.Initializing:
                        // Location platform is attempting to acquire a fix. 
                        _rootPage.RewriteStatus(args);
                        _rootPage.NotifyUser("Location platform is attempting to obtain a position.", MainPage.NotifyType.StatusMessage);
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        _rootPage.RewriteStatus(args);
                        _rootPage.NotifyUser("Not able to determine the location.", MainPage.NotifyType.ErrorMessage);
                        break;

                    case PositionStatus.Disabled:
                        // The permission to access location data is denied by the user or other policies.
                        _rootPage.RewriteStatus(args);
                        _rootPage.NotifyUser("Access to location is denied.", MainPage.NotifyType.ErrorMessage);

                        // Show message to the user to go to location settings
                        LocationDisabledMessage.Visibility = Visibility.Visible;



                        // Clear cached location data if any
                        //UpdateLocationData(null);
                        break;

                    case PositionStatus.NotInitialized:
                        // The location platform is not initialized. This indicates that the application 
                        // has not made a request for location data.
                        _rootPage.RewriteStatus(args);
                        _rootPage.NotifyUser("No request for location is made yet.", MainPage.NotifyType.StatusMessage);
                        break;

                    case PositionStatus.NotAvailable:
                        // The location platform is not available on this version of the OS.
                        _rootPage.RewriteStatus(args);
                        _rootPage.NotifyUser("Location is not available on this version of the OS.", MainPage.NotifyType.ErrorMessage);
                        break;

                    default:
                        _rootPage.RewriteStatus(args);
                        _rootPage.NotifyUser(string.Empty, MainPage.NotifyType.StatusMessage);
                        break;
                }
            });
        }

        public void RemoveMapIcon(MapIcon mapIcon)
        {
            mapControl.MapElements.Remove(mapIcon);
        }

        public void UpdateLocationData()
        {
            if (_geolocator != null && Geoposition!=null)
            {
               if (!(bool)CenterPin.IsChecked)
                    mapControl.Center = Geoposition.Coordinate.Point;
                mapControl.LandmarksVisible = true;
            }
        }

        public MapIcon AddGeopoint(Geopoint geopoint, string name)
        {
            MapIcon mapIcon = new MapIcon();
            mapIcon.Location = geopoint;
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = name;
            mapIcon.ZIndex = 0;
            mapControl.MapElements.Add(mapIcon);
            mapIcon.Visible = true;
            return mapIcon;
        }

        private void ZoomLevelHendler(object sender, RangeBaseValueChangedEventArgs e)
        {
            mapControl.ZoomLevel = zoomLevel.Value;
        }

        private void ZoomLevelChanged(MapControl sender, object args)
        {
            zoomLevel.Value = mapControl.ZoomLevel;
        }

        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            zoomLevel.Value++;
        }

        private void ClickMinus(object sender, RoutedEventArgs e)
        {
            zoomLevel.Value--;
        }

        private void KeyDownPin(object sender, KeyRoutedEventArgs e)
        {

        }
    }

   
}
