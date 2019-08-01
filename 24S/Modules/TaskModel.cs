
using DJI.WindowsSDK;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using Windows.UI.Core;

namespace _24S
{
    /// <summary>
    /// ViewMolde for the PlaybackPage
    /// </summary>
    public sealed class TaskModel
    {
        private Windows.UI.Core.CoreDispatcher dispatcher = null;
        public List<MediaTask> runTasks = new List<MediaTask>();
        int completeCount = 0;
        string process = "0.00%";
        string count = "Count: ";
        string speed = "Speed: ";
        string sync = "Turn workmode green first";
        public long totalByte;
        public long cachedByte;
        public double MBSpeed;
        Timer timer;

        public string Process { get => process; set => process = value; }
        public string Count { get => count; set => count = value; }
        public string Speed { get => speed; set => speed = value; }
        public string Sync
        {
            get => sync; set
            {
                sync = value;
                System.Diagnostics.Debug.WriteLine("SYNC: {0}", sync);
            }
        }
        public int CompleteCount
        {
            get => completeCount;
            set
            {
                completeCount = value;
            }
        }

        public CoreDispatcher Dispatcher { get => dispatcher; set => dispatcher = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Reset()
        {
            StopTimer();
            totalByte = 0;
            cachedByte = 0;
            MBSpeed = 0;
            runTasks.Clear();
            completeCount = 0;
        }

        public void StartTimer()
        {
            if (timer == null)
            {
                timer = new Timer(1000);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
            }
            if (timer.Enabled)
            {
                return;
            }
            timer.Enabled = true;
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process = (((double)cachedByte / (double)totalByte) * 100).ToString("0.00") + "%";
            Count = "Count: " + runTasks.Count.ToString() + "/" + CompleteCount.ToString();
            Speed = "Speed: " + MBSpeed.ToString("0.00");
            InvokeUpdateText();
        }

        void StopTimer()
        {
            if (timer != null && timer.Enabled)
            {
                timer.Stop();
                Process = " ";
                Count = "Count : ";
                Speed = "Speed: ";
                InvokeUpdateText();
            }
        }

        private void InvokeUpdateText()
        {
            System.Diagnostics.Debug.WriteLine("Process: {0}", Process);
            System.Diagnostics.Debug.WriteLine("Count: {0}", Count);
            System.Diagnostics.Debug.WriteLine("Speed: {0}", Speed);
        }
    }

    /// <summary>
    /// ItemModel for the GridView render 
    /// </summary>
    public sealed class MediaItem
    {
        public MediaItem(MediaFile file)
        {
            this.file = file;
        }

        public readonly MediaFile file;
        public int Index { get => file.fileIndex; }

    }
}
