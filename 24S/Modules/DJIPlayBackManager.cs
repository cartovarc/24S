using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;


namespace _24S
{

    class DJIPlayBackManager : Page
    {
        public static DJIPlayBackManager Instance { get; } = new DJIPlayBackManager(); // Singleton

        private TaskModel taskModel = new TaskModel();
        private MediaTaskManager taskManager = new MediaTaskManager(0, 0);
        public ObservableCollection<MediaItem> files = new ObservableCollection<MediaItem>();
        private CameraHandler cameraHandler = DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0);

        private DJIPlayBackManager()
        {
            DJIVideoManager.Instance.MissionRecorded += ProcessDownloadFirst; 
        }

        /// <summary>
        /// Developer should change the work mode to transcode or playback in order to enhance download speed
        /// </summary>
        private async void Mode()
        {
            var current = await cameraHandler.GetCameraWorkModeAsync();
            var currMode = current.value?.value;
            if (currMode != CameraWorkMode.PLAYBACK && currMode != CameraWorkMode.TRANSCODE)
            {
                var msg = new CameraWorkModeMsg
                {
                    value = CameraWorkMode.TRANSCODE
                };
                SDKError err = await cameraHandler.SetCameraWorkModeAsync(msg);
                System.Diagnostics.Debug.WriteLine("Mode {0})", err.ToString());
            }
            else
            {
                var msg = new CameraWorkModeMsg
                {
                    value = CameraWorkMode.SHOOT_PHOTO
                };
                SDKError err = await cameraHandler.SetCameraWorkModeAsync(msg);
                System.Diagnostics.Debug.WriteLine("Mode {0})", err.ToString());
            }

        }

        /// <summary>
        /// Developer should get file list first, in case developer gets the file index for download
        /// </summary>
        private async void Reload()
        {
            var result = await cameraHandler.GetCameraWorkModeAsync();
            if (result.value == null)
            {
                return;
            }
            var mode = result.value?.value;
            if (mode != CameraWorkMode.TRANSCODE && mode != CameraWorkMode.PLAYBACK)
            {
                return;
            }
            this.files.Clear();
            var fileListTask = MediaTask.FromRequest(new MediaFileListRequest
            {
                count = -1,
                index = 1,
                subType = MediaRequestType.ORIGIN,
                isAllList = true,
                location = MediaFileListLocation.SD_CARD,
            });
            fileListTask.OnListReqResponse += async (fileSender, files) =>
            {
                taskModel.Sync = String.Format("LaunchFileDataTask get files : {0}", files.Count);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    files.ForEach(obj => this.files.Add(new MediaItem(obj)));
                });
            };
            fileListTask.OnRequestTearDown += (fileSender, retCode, response) =>
            {
                if (retCode == SDKError.NO_ERROR)
                {
                    return;
                }
                taskModel.Sync = String.Format("LaunchFileDataTask get files : {0}. Switch Mode or try again", retCode);
            };
            taskManager.PushBack(fileListTask);
        }

        private void ProcessDownloadFirst()
        {
            Mode(); // Download Mode
            Reload(); // List Files from aircraft
            DownloadFirst();
            //TODO delete files from aircraft
        }

        /// <summary>
        /// Download first video from aircraft
        /// </summary>
        private void DownloadFirst()
        {
            if (files.Count == 0)
            {
                taskModel.Sync = "Should reload before download";
                return;
            }
            if (taskModel.CompleteCount != 0)
            {
                taskModel.Sync = "Downloading is transfer, Cancel First";
                return;
            }
            DownloadSingle(0);
        }


        private async void DownloadSingle(int index)
        {
            if (index >= taskModel.CompleteCount)
            {
                taskModel.Reset();
                return;
            }
            var file = files[index].file;
            var request = new MediaFileDownloadRequest
            {
                index = file.fileIndex,
                count = 1,
                dataSize = -1,
                offSet = 0,
                segSubIndex = 0,
                subIndex = 0,
                type = MediaRequestType.ORIGIN
            };

            var task = MediaTask.FromRequest(request);
            var storageFile = await KnownFolders.VideosLibrary.CreateFileAsync(file.fileName, CreationCollisionOption.GenerateUniqueName);
            var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            var outputStream = stream.GetOutputStreamAt(0);
            var fileWriter = new DataWriter(outputStream);
            task.OnDataReqResponse += (sender, req, data, speed) =>
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    taskModel.cachedByte += data.Length;
                    taskModel.MBSpeed = (speed / 8388608);
                    fileWriter.WriteBytes(data);
                    await fileWriter.StoreAsync();
                    await outputStream.FlushAsync();
                });
            };

            task.OnRequestTearDown += (sender, retCode, res) =>
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    taskModel.Sync = String.Format("DownloadMediaFile index {0} complete {1}", res?.dataReq.index, retCode);
                    //this.DownloadSingle(this.taskModel.runTasks.Count);
                });
            };
            taskModel.runTasks.Add(task);
            taskManager.PushBack(task);

        }
    }
}
