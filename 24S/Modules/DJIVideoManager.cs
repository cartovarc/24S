using System;
using System.IO;
using Windows.UI.Xaml.Controls;
using DJI.WindowsSDK;
using DJIVideoParser;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace _24S
{
    
    class DJIVideoManager : Page
    {
        public static DJIVideoManager Instance { get; } = new DJIVideoManager(); // Singleton

        private DJIVideoParser.Parser videoParser; //use videoParser to decode raw data.
        public Stream videoClient { get; set; } = null; // stream for client of video
        SwapChainPanel swapChainPanel = null;
        public bool videoTest = false; //change to test video without aircraft
        Stopwatch clock = new Stopwatch();

        public delegate void VideoMissionRecordedEventHandler();
        public event VideoMissionRecordedEventHandler MissionRecorded;

        private OrderTaskScheduler _scheduler_video = new OrderTaskScheduler("scheduler_video");

        private DJIVideoManager()
        {
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StateChanged += OnExecutionFinish;
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
                    
                    int width = 1280;
                    int height = 720;

                    ArrayList pixels = new ArrayList();

                    int index = 0;
                   
                    for(int i=0; i<24; i++) {
                        Random rnd = new Random();
                        byte[] pixelData = new byte[4 * width * height];
                        for (int y = 0; y < height; ++y) {
                            for (int x = 0; x < width; ++x)
                            {

                                pixelData[index++] = (byte)rnd.Next(0, 255);  // B
                                pixelData[index++] = (byte)rnd.Next(0, 255); ;  // G
                                pixelData[index++] = (byte)rnd.Next(0, 255); ;  // R
                                pixelData[index++] = (byte)rnd.Next(0, 255); ;  // A
                            }
                        }
                        index = 0;
                        pixels.Add(pixelData);
                    }
                    clock.Start();
                    index = 0;
                    while (true)
                    {
                        ReceiveDecodedData((byte[]) pixels[index++], width, height);
                        index = index % 24;
                        countFrame++;
                        Thread.Sleep(25);
                    }
                });

                return;
            }

            //Must in UI Thread
            //Page pageAux = new Page();
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
                DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated -= OnVideoPush;
                videoParser = null;
            }
        }

        //raw data
        void OnVideoPush(VideoFeed sender, byte[] bytes)
        {
            videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        private int countFrame = 0; // use to inspect fps on video test
        public Task sendVideoToClientInOrder(byte[] data, int width, int height)
        {
            return Task.Factory.StartNew(
                () => {
                    try
                    {
                        if (videoClient != null)
                        {
                            if (countFrame % 100 == 0 && videoTest)
                            {
                                int fps = (int)(countFrame / (clock.ElapsedMilliseconds / 1000.0));
                                System.Diagnostics.Debug.WriteLine("fps: {0}", fps);

                            }
                            videoClient.Write(data, 0, data.Length); // send bytes to the client
                        }
                    }
                    catch (System.IO.IOException e)
                    {
                        videoClient = null; //restart unique consumer
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                this._scheduler_video);
        }


        //Decode data. Do nothing here. This function would return a bytes array with image data in RGBA format.
        async void ReceiveDecodedData(byte[] data, int width, int height)
        {

            //System.Diagnostics.Debug.WriteLine("W {0} H {1}", width, height);

            if (videoClient != null)
            {
                //System.Diagnostics.Debug.WriteLine("tamano: {0} {1} {2}", width, height, data.Length);
                if (countFrame % 2 == 0)
                {
                    sendVideoToClientInOrder(data, width, height);
                }
                countFrame++;
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

        public void setClientStream(Stream clientStream)
        {
            videoClient = clientStream;
            if (videoTest)
            {
                InitializeVideoFeedModule();
            }
        }

        private async void OnExecutionFinish(object sender, WaypointMissionStateTransition? value)
        {
            if (value != null && value.Value.previous == WaypointMissionState.EXECUTING && value.Value.current == WaypointMissionState.READY_TO_UPLOAD)
            {
                Task
                .Factory
                .StartNew(() => {
                    while (DJIComponentManager.Instance.AircraftAltitude > 0)
                    {
                        Thread.Sleep(15000);

                    }
                    //TODO: Send stop video request
                    videoClient = null; // close connection with video client
                    System.Diagnostics.Debug.WriteLine("STOP VIDEO CLIENT");

                });

            }
        }

    }
}
