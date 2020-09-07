using MediaDevices;
using System;
using System.Diagnostics;
using System.Management;

namespace CodeArtEng.RemoteScheduler.Utility
{
    /// <summary>
    /// Media Device Watcher, monitor on device connection status changed
    /// </summary>
    public class MediaDeviceWatcher : IDisposable
    {
        readonly ManagementEventWatcher watcher;
        private bool disposedValue;

        /// <summary>
        /// Media Device's Description
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Device connected event. Event raised from worker thread.
        /// </summary>
        public event EventHandler DeviceConnected;
        /// <summary>
        /// Device disconeected event. Event raised from worker thread.
        /// </summary>
        public event EventHandler DeviceDisconnected;

        private bool Connected { get; set; } = false;
        /// <summary>
        /// Create instance to monitor on connection status of device with defined description.
        /// </summary>
        /// <param name="deviceName"></param>
        public MediaDeviceWatcher(string deviceName)
        {
            DeviceName = deviceName;
            watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
        }

        /// <summary>
        /// Start monitoring thread.
        /// </summary>
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

        /// <summary>
        /// Stop monitoring thread.
        /// </summary>
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

        /// <summary>
        /// Disponse instance and stop monitoring thread.
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// Disponse instance and stop monitoring thread.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
