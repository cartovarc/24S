using System;
using System.IO;
using Windows.UI.Xaml.Controls;
using DJI.WindowsSDK;
using DJIVideoParser;
using System.Threading.Tasks;

namespace _24S
{
    class DJIVideoManager : Page
    {
        public static DJIVideoManager Instance { get; } = new DJIVideoManager(); // Singleton

        private DJIVideoParser.Parser videoParser; //use videoParser to decode raw data.
        public Stream videoClient { get; set; } = null; // stream for client of video
        SwapChainPanel swapChainPanel = null;
        private bool videoTest = false; //change to test video without aircraft

        public delegate void VideoMissionRecordedEventHandler();
        public event VideoMissionRecordedEventHandler MissionRecorded;

        private DJIVideoManager()
        {
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).ExecutionStateChanged += StartStopMissionVideoRecord;
        }

        public void setSwapChainPanel(SwapChainPanel swapChainPanel)
        {
            this.swapChainPanel = swapChainPanel;
        }

        public async void InitializeVideoFeedModule()
        {
            if (videoTest)
            {
                var t = Task.Run(() => {
                    while (true)
                    {
                        int width = 500;
                        int height = 500;
                        byte[] pixelData = new byte[4 * width * height];

                        int index = 0;
                        Random rnd = new Random();
                        for (int y = 0; y < height; ++y)
                            for (int x = 0; x < width; ++x)
                            {

                                pixelData[index++] = (byte)rnd.Next(0, 255);  // B
                                pixelData[index++] = (byte)rnd.Next(0, 255); ;  // G
                                pixelData[index++] = (byte)rnd.Next(0, 255); ;  // R
                                pixelData[index++] = (byte)rnd.Next(0, 255); ;  // A
                            }

                        ReceiveDecodedData(pixelData, width, height);

                        if (videoParser != null) break;
                    }
                });

                return;
            }

            //Must in UI Thread

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                //Raw data and decoded data listener
                if (videoParser == null)
                {
                    videoParser = new DJIVideoParser.Parser();
                    videoParser.Initialize(delegate (byte[] data)
                    {
                        //Note: This function must be called because we need DJI Windows SDK to help us to parse frame data.
                        return DJISDKManager.Instance.VideoFeeder.ParseAssitantDecodingInfo(0, data);
                    });
                    //Set the swapChainPanel to display and set the decoded data callback.
                    videoParser.SetSurfaceAndVideoCallback(0, 0, swapChainPanel, ReceiveDecodedData);
                    DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated += OnVideoPush;
                }
                //get the camera type and observe the CameraTypeChanged event.
                DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).CameraTypeChanged += OnCameraTypeChanged;
                var type = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetCameraTypeAsync();
                OnCameraTypeChanged(this, type.value);
            });
        }

        public void UninitializeVideoFeedModule()
        {
            if (DJISDKManager.Instance.SDKRegistrationResultCode == SDKError.NO_ERROR)
            {
                videoParser.SetSurfaceAndVideoCallback(0, 0, null, null);
                DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated -= OnVideoPush;
            }
        }

        //raw data
        void OnVideoPush(VideoFeed sender, byte[] bytes)
        {
            videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        //Decode data. Do nothing here. This function would return a bytes array with image data in RGBA format.
        async void ReceiveDecodedData(byte[] data, int width, int height)
        {

            //System.Diagnostics.Debug.WriteLine("W {0} H {1}", width, height);

            if (videoClient != null)
            {
                //System.Diagnostics.Debug.WriteLine("tamano: {0} {1} {2}", width, height, data.Length);
                try
                {
                    await Task.Run(() => {
                        videoClient.Write(data, 0, data.Length); // send bytes to the client
                    });

                }
                catch (System.IO.IOException)
                {
                    videoClient = null; //restart unique consumer
                }

            }
        }

        //We need to set the camera type of the aircraft to the DJIVideoParser. After setting camera type, DJIVideoParser would correct the distortion of the video automatically.
        private void OnCameraTypeChanged(object sender, CameraTypeMsg? value)
        {
            if (value != null)
            {
                switch (value.Value.value)
                {
                    case CameraType.MAVIC_2_ZOOM:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Zoom);
                        break;
                    case CameraType.MAVIC_2_PRO:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Pro);
                        break;
                    default:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Others);
                        break;
                }

            }
        }

        private async Task<SDKError> SetCameraModeToRecord()
        {
            SDKError err =  await SetCameraWorkMode(CameraWorkMode.RECORD_VIDEO);
            return err;
        }

        private async Task<SDKError> SetCameraWorkMode(CameraWorkMode mode)
        {
            CameraWorkModeMsg workMode = new CameraWorkModeMsg
            {
                value = mode,
            };
            SDKError retCode = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraWorkModeAsync(workMode);
            return retCode;
        }

        private async Task<SDKError> StartRecordVideo()
        {
            SDKError retCode = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).StartRecordAsync();
            return retCode;
        }

        private async Task<SDKError> StopRecordVideo()
        {
            SDKError retCode = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).StopRecordAsync();
            return retCode;
        }

        private async void StartStopMissionVideoRecord(object sender, WaypointMissionExecutionState? value)
        {
            if (value.Value.state == WaypointMissionExecuteState.INITIALIZING)
            {
                await SetCameraModeToRecord();
                await StartRecordVideo();
            }
            else if (value.Value.isExecutionFinish)
            {
                SDKError err = await StopRecordVideo();

                if(err == SDKError.NO_ERROR)
                {
                    OnVideoMissionRecorded();
                }
                
            }
        }

        private void OnVideoMissionRecorded()
        {
            MissionRecorded?.Invoke();
        }

    }
}
