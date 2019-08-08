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
            DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(throttle, roll, pitch, yaw);
        }
    }
}
