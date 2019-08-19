using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DJI.WindowsSDK;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;


namespace _24S
{
    class DJIComponentManager
    {
        public static DJIComponentManager Instance { get; } = new DJIComponentManager(); // Singleton

        private LocationCoordinate2D _aircraftLocation = new LocationCoordinate2D() { latitude = 0, longitude = 0 };
        public LocationCoordinate2D AircraftLocation
        {
            get
            {
                return _aircraftLocation;
            }
            set
            {
                _aircraftLocation = value;
            }
        }


        private double _aircraftAltitude = 0;
        public double AircraftAltitude
        {
            get
            {
                return _aircraftAltitude;
            }
            set
            {
                _aircraftAltitude = value;
            }
        }

        private double _aircraftBattery = 0;
        public double AircraftBattery
        {
            get
            {
                return _aircraftBattery;
            }
            set
            {
                _aircraftBattery = value;
            }
        }

        private bool _aircraftConnection = false;
        public bool AircraftConnection
        {
            get
            {
                return _aircraftConnection;
            }
            set
            {
                _aircraftConnection = value;
            }
        }

        private double _aircraftVelocity = 0.0;
        public double AircraftVelocity
        {
            get
            {
                return _aircraftVelocity;
            }
            set
            {
                _aircraftVelocity = value;
            }
        }

        private Attitude _aircraftGimbalAttitude = new Attitude() { pitch = 0, yaw = 0, roll = 0 };
        public Attitude AircraftGimbalAttitude
        {
            get
            {
                return _aircraftGimbalAttitude;
            }
            set
            {
                _aircraftGimbalAttitude = value;
            }
        }

        private Attitude _aircraftAttitude = new Attitude() { pitch = 0, yaw = 0, roll = 0 };
        public Attitude AircraftAttitude
        {
            get
            {
                return _aircraftAttitude;
            }
            set
            {
                _aircraftAttitude = value;
            }
        }

        private FCGPSSignalLevelMsg _aircraftSignalLevel = new FCGPSSignalLevelMsg() { value = FCGPSSignalLevel.UNKNOWN };
        public FCGPSSignalLevelMsg AircraftSignalLevel
        {
            get
            {
                return _aircraftSignalLevel;
            }
            set
            {
                _aircraftSignalLevel = value;
            }
        }

        private DJIComponentManager()
        {
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += AircraftLocationChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += AircraftAltitudeChanged;
            DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += AircraftBatteryPercentChanged;
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ConnectionChanged += AircraftConnectionChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).VelocityChanged += AircraftVelocityChanged;
            DJISDKManager.Instance.ComponentManager.GetGimbalHandler(0, 0).GimbalAttitudeChanged += GimbalAttitudeChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AttitudeChanged += AttitudeChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GPSSignalLevelChanged += GPSSignalLevelChanged;

        }

        private async void AircraftLocationChanged(object sender, LocationCoordinate2D? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftLocation = value.Value;
                }
            });
        }

        private async void AircraftAltitudeChanged(object sender, DoubleMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftAltitude = value.Value.value;
                }
            });
        }

        private async void AircraftBatteryPercentChanged(object sender, IntMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftBattery = value.Value.value;
                }
            });
        }

        private async void AircraftConnectionChanged(object sender, BoolMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftConnection = value.Value.value;
                }
            });
        }

        private async void AircraftVelocityChanged(object sender, Velocity3D? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    double x = value.Value.x;
                    double y = value.Value.y;
                    double z = value.Value.z;
                    AircraftVelocity = Math.Sqrt(x*x + y*y + z*z);
                }
            });
        }

        private async void GimbalAttitudeChanged(object sender, Attitude? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftGimbalAttitude = value.Value;
                }
            });
        }

        private async void AttitudeChanged(object sender, Attitude? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftAttitude = value.Value;
                }
            });
        }

        private async void GPSSignalLevelChanged(object sender, FCGPSSignalLevelMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftSignalLevel = value.Value;
                }
            });
        }

        public async Task<bool> AircraftConnected()
        {
            //If you want to get the latest product connection state manually, you can use the following code
            var productType = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value;
            if (productType != null && productType?.value != ProductType.UNRECOGNIZED)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<SDKError> SetPrecisionLanding(bool value)
        {
            SDKError errSetPrecisionLanding = await DJISDKManager.Instance.ComponentManager.GetFlightAssistantHandler(0, 0).SetPrecisionLandingEnabledAsync(new BoolMsg() { value = value });
            System.Diagnostics.Debug.WriteLine(String.Format("Set precision landing to {0}: {1}", value, errSetPrecisionLanding.ToString()));
            return errSetPrecisionLanding;
        }

        public async Task<SDKError> SetGroundStationModeEnabled(bool value)
        {
            SDKError errSetGroundStationModeEnabled = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetGroundStationModeEnabledAsync(new BoolMsg() { value = true });
            System.Diagnostics.Debug.WriteLine(String.Format("Set Ground Station Mode Enabled to {0}: {1}", value, errSetGroundStationModeEnabled.ToString()));
            return errSetGroundStationModeEnabled;
        }

        public async Task<SDKError> RotateGimbalByAngle(double pitch, double roll, double yaw, double duration)
        {
            SDKError errRotateGimbalByAngle = await DJISDKManager.Instance.ComponentManager.GetGimbalHandler(0, 0).RotateByAngleAsync(new GimbalAngleRotation() { pitch = pitch, roll = roll, yaw = yaw, duration = duration });
            System.Diagnostics.Debug.WriteLine(String.Format("Rotate by angle: {0}", errRotateGimbalByAngle.ToString()));
            return errRotateGimbalByAngle;
        }
    }
}
