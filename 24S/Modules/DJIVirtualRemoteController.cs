using DJI.WindowsSDK;

namespace _24S
{
    class DJIVirtualRemoteController
    {
        public static DJIVirtualRemoteController Instance { get; } = new DJIVirtualRemoteController(); // Singleton

        public void UpdateJoystickValue(float throttle, float roll, float pitch, float yaw)
        {
            DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(throttle, roll, pitch, yaw);
        }

    }
}
