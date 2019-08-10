using DJI.WindowsSDK;

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
    }
}
