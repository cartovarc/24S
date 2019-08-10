using System;
using Windows.UI.Xaml.Controls;
using DJI.WindowsSDK;
using Windows.UI.Xaml;

namespace _24S
{

    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            DJISDKManager.Instance.RegisterApp("c3155620d9c96af31b741f64");

            //Initialize Socket Server
            SocketServer.Instance.ExecuteServer();
            SocketServer.Instance.DataReceived += MessageManager.Instance.OnSocketDataReceivedAsync;
        }


        private void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                System.Diagnostics.Debug.WriteLine("Register app successfully.");

                //The product connection state will be updated when it changes here.
                DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (value != null && value?.value != ProductType.UNRECOGNIZED)
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");

                            DJIVideoManager.Instance.setSwapChainPanel(swapChainPanel);
                            DJIVideoManager.Instance.InitializeVideoFeedModule(); //Initialize video streaming when aircraft is connected
                            DJISDKManager.Instance.ComponentManager.GetFlightAssistantHandler(0, 0).SetVisionAssistedPositioningEnabledAsync(new BoolMsg() { value = true});
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is disconnected now.");
                            //You can hide your pages according to the aircraft connection state here, or show the connection tips to the users.
                        }
                    });
                };

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Register SDK failed, the error is: ");
                System.Diagnostics.Debug.WriteLine(resultCode.ToString());
            }
        }

        private async void TakeOffClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
            System.Diagnostics.Debug.WriteLine("TAKE OFF: %s", err.ToString());
        }

        private async void EnableGSMClick(object sender, RoutedEventArgs e)
        {
            SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetGroundStationModeEnabledAsync(new BoolMsg() { value = true });
            System.Diagnostics.Debug.WriteLine("TAKE OFF: %s", err.ToString());
        }
    }
}
