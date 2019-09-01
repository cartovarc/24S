using System;
using Windows.UI.Xaml.Controls;
using DJI.WindowsSDK;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace _24S
{

    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            // Log Application Start
            LoggingServices.Instance.WriteLine<MainPage>("Application starting...", MetroLog.LogLevel.Info);

            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            DJISDKManager.Instance.RegisterApp("979d9e5b7053c25f2d50c2f9");

            //Initialize Socket Server
            Task
                .Factory
                .StartNew(() => {
                    SocketServer.Instance.ExecuteServer();
                    SocketServer.Instance.DataReceived += MessageManager.Instance.OnSocketDataReceivedAsync;
                });
        }


        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                string registerSuccessfullyMessage = "Register app successfully.";
                LoggingServices.Instance.WriteLine<MainPage>("Register app successfully.", MetroLog.LogLevel.Info);

                //The product connection state will be updated when it changes here.
                DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (value != null && value?.value != ProductType.UNRECOGNIZED)
                        {
                            LoggingServices.Instance.WriteLine<MainPage>("The Aircraft is connected now.", MetroLog.LogLevel.Info);

                            DJIVideoManager.Instance.setSwapChainPanel(swapChainPanel);
                            DJIVideoManager.Instance.InitializeVideoFeedModule(); //Initialize video streaming when aircraft is connected
                            DJISDKManager.Instance.ComponentManager.GetFlightAssistantHandler(0, 0).SetVisionAssistedPositioningEnabledAsync(new BoolMsg() { value = true });
                        }
                        else
                        {
                            LoggingServices.Instance.WriteLine<MainPage>("The Aircraft is disconnected now.", MetroLog.LogLevel.Info);
                            //You can hide your pages according to the aircraft connection state here, or show the connection tips to the users.
                        }
                    });
                };

            }
            else
            {
                string registerFailedMessage = "Register SDK failed, the error is: " + resultCode.ToString();
                LoggingServices.Instance.WriteLine<MainPage>(registerFailedMessage, MetroLog.LogLevel.Error);
            }
        }

        #region debug_buttons
        private async void TakeOffClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
            var dialog = new MessageDialog("take off: " + err.ToString(), "Info");
            var result = await dialog.ShowAsync();
        }

        private async void BottonLightONClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightAssistantHandler(0, 0).SetBottomAuxiliaryLightModeAsync(new BottomAuxiliaryLightModeMsg() { value = BottomAuxiliaryLightMode.ON });
            var dialog = new MessageDialog("light on: " + err.ToString(), "Info");
            var result = await dialog.ShowAsync();
        }

        private async void BottonLightOFFClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightAssistantHandler(0, 0).SetBottomAuxiliaryLightModeAsync(new BottomAuxiliaryLightModeMsg() { value = BottomAuxiliaryLightMode.OFF });
            var dialog = new MessageDialog("light off: " + err.ToString(), "Info");
            var result = await dialog.ShowAsync();
        }

        private async void LandingClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartAutoLandingAsync();
            var dialog = new MessageDialog("landing: " + err.ToString(), "Info");
            var result = await dialog.ShowAsync();
        }

        private async void MissionStateClick(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("mission state: " + DJIMissionManager.Instance.WaypointMissionCurrentState(), "Info");
            var result = await dialog.ShowAsync();
        }

        private async void ZoomInClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).CameraStartContinuousOpticalZoomAsync(new CameraContinuousOpticalZoomParam() { direction = CameraZoomDirection.ZOOM_IN, speed = CameraZoomSpeed.NORMAL });
            var dialog = new MessageDialog("zoom in: " + err.ToString(), "Info");
            var result = await dialog.ShowAsync();
        }

        private async void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).CameraStartContinuousOpticalZoomAsync(new CameraContinuousOpticalZoomParam() { direction = CameraZoomDirection.ZOOM_OUT, speed = CameraZoomSpeed.NORMAL });
            var dialog = new MessageDialog("zoom out: " + err.ToString(), "Info");
            var result = await dialog.ShowAsync();
        }
        #endregion
    }
}
