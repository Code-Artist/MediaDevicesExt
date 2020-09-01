using MediaDevices;
using System;
using System.Diagnostics;
using System.Management;

namespace CodeArtEng.RemoteScheduler.Utility
{
    public class MediaDeviceWatcher : IDisposable
    {
        readonly ManagementEventWatcher watcher;
        private bool disposedValue;

        public string DeviceName { get; private set; }

        public event EventHandler DeviceConnected;
        public event EventHandler DeviceDisconnected;

        private bool Connected { get; set; } = false;
        public MediaDeviceWatcher(string deviceName)
        {
            DeviceName = deviceName;
            watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
        }

        public void Start()
        {
            Connected = false;
            if (IsDeviceConnected())
            {
                Debug.WriteLine("Device [" + DeviceName + "] connected.");
                Connected = true;
            }
            watcher.Start();
        }

        public void Stop() { watcher.Stop(); Connected = false; }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            bool connected = IsDeviceConnected();
            if (!Connected && connected)
            {
                Trace.WriteLine("Device Connected: " + DeviceName);
                Connected = true;
                DeviceConnected?.Invoke(this, null);
            }
            else if (Connected && !connected)
            {
                Trace.WriteLine("Device Disconnected: " + DeviceName);
                Connected = false;
                DeviceDisconnected?.Invoke(this, null);
            }
        }

        private bool IsDeviceConnected()
        {
            if (string.IsNullOrEmpty(DeviceName)) return false;

            using (MediaManager manager = new MediaManager())
            {
                return manager.IsDeviceConnected(DeviceName);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    watcher.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
