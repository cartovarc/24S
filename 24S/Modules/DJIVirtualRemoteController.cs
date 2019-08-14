using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DJI.WindowsSDK;
using System.Threading.Tasks;


namespace _24S
{
    class DJIVirtualRemoteController
    {
        public static DJIVirtualRemoteController Instance { get; } = new DJIVirtualRemoteController(); // Singleton

        private DJIVirtualRemoteController()
        {
        }

        public void UpdateJoystickValue(float pitch, float roll, float yaw, float throttle)
        {
            System.Diagnostics.Debug.WriteLine("Pitch: {0}", pitch);
            System.Diagnostics.Debug.WriteLine("roll: {0}", roll);
            System.Diagnostics.Debug.WriteLine("yaw: {0}", yaw);
            System.Diagnostics.Debug.WriteLine("throttle: {0}", throttle);
            DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(throttle, roll, pitch, yaw);
        }

        public async Task<SDKError> Landing()
        {
            SDKError makeLandingError = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartAutoLandingAsync();
            return makeLandingError;
        }

        public async Task<SDKError> GoHome()
        {
            SDKError makeLandingError = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartGoHomeAsync();
            return makeLandingError;
        }
    }
}
