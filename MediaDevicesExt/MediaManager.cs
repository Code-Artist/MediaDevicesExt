﻿using PortableDeviceApiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MediaDevices
{
    /// <summary>
    /// This class provides functions for creating and searching for portable devices.
    /// <para>In oposite to previous version this manager provides possibility to use
    /// MTP from separate STA threads, just create once manager instance in each thread.</para>
    /// </summary>
    public class MediaManager : IDisposable
    {
        private readonly PortableDeviceManager portableDeviceManager;
        private List<MediaDevice> devices;
        private List<MediaDevice> privateDevices;

        /// <summary>
        /// Creates a new instance of media manager.
        /// <para>Remember that this instance is tidly connected with <see cref="PortableDeviceManager"/>
        /// instance and its parent STA thread. Do not try to call this object outside it's apartment,
        /// it will not work. You will get <see cref="COMException"/>.</para>
        /// <para><see cref="MediaManager"/> creates its own instance of COM portable device manager
        /// so that you are able to fork MTP connection from any STA thread.</para>
        /// <para>To use it, create a new instance of this manager and start fetching devices.</para>
        /// </summary>
        public MediaManager()
        {
            this.portableDeviceManager = new PortableDeviceManager();
        }

        /// <summary>
        /// Returns an enumerable collection of currently available portable devices.
        /// </summary>
        /// <returns>>An enumerable collection of portable devices currently available.</returns>
        public IEnumerable<MediaDevice> GetDevices()
        {
            portableDeviceManager.RefreshDeviceList();

            // get number of devices
            uint count = 0;
            portableDeviceManager.GetDevices(null, ref count);

            if (count == 0)
            {
                return new List<MediaDevice>();
            }

            // get device IDs
            var deviceIds = new string[count];
            portableDeviceManager.GetDevices(deviceIds, ref count);

            if (devices == null)
            {
                devices = deviceIds.Select(d => new MediaDevice(d, this.portableDeviceManager)).ToList();
            }
            else
            {
                UpdateDeviceList(devices, deviceIds);
            }
            return devices;
        }

        private void UpdateDeviceList(List<MediaDevice> deviceList, string[] deviceIdList)
        {
            var idList = deviceIdList.ToList();

            // remove
            var remove = deviceList.Where(d => !idList.Contains(d.DeviceId)).Select(d => d.DeviceId).ToList();
            deviceList.RemoveAll(d => remove.Contains(d.DeviceId));

            // add
            var add = idList.Where(id => !deviceList.Select(d => d.DeviceId).Contains(id)).ToList();
            deviceList.AddRange(add.Select(id => new MediaDevice(id, this.portableDeviceManager)));
        }

        /// <summary>
        /// Returns an enumerable collection of currently available private portable devices.
        /// </summary>
        /// <returns>>An enumerable collection of private portable devices currently available.</returns>
        public IEnumerable<MediaDevice> GetPrivateDevices()
        {
            portableDeviceManager.RefreshDeviceList();

            // get number of devices
            uint count = 0;
            portableDeviceManager.GetPrivateDevices(null, ref count);

            if (count == 0)
            {
                return new List<MediaDevice>();
            }

            // get device IDs
            var deviceIds = new string[count];
            portableDeviceManager.GetPrivateDevices(deviceIds, ref count);

            if (privateDevices == null)
            {
                privateDevices = deviceIds.Select(d => new MediaDevice(d, this.portableDeviceManager)).ToList();
            }
            else
            {
                UpdateDeviceList(privateDevices, deviceIds);
            }
            return privateDevices;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (devices != null)
            {
                foreach (var dev in devices)
                {
                    dev.DestroyDevice();
                }

                devices = null;
            }
        }

        /// <summary>
        /// Check if a device with given description is connected with MTP / PTP enabled.
        /// By default, some Android device is connected as USB charging only.
        /// </summary>
        /// <param name="deviceDescription">Device description</param>
        /// <returns>true if device connected with MTP / PTP capability.</returns>
        public bool IsDeviceConnected(string deviceDescription)
        {
            MediaDevice ptrDevice = GetDevices().FirstOrDefault(x => x.Description == deviceDescription);
            if (ptrDevice == null) return false;

            //Enumerate Directory to ensure Media Device in MTP or PTP Mode.
            //By default, Android device is connected as USB Charging only with no file access.
            ptrDevice.Connect();
            MediaDirectoryInfo rootInfo = ptrDevice.GetRootDirectory();
            try
            {
                var dirs = rootInfo.EnumerateDirectories();
                if (rootInfo.EnumerateDirectories().Count() > 0)
                {
                    ptrDevice.Dispose(); ptrDevice = null;
                    return true;
                }
            }
            catch { ptrDevice.Dispose(); ptrDevice = null; }
            return false;
        }

        /// <summary>
        /// Find and return device by description. Only return device which connected as MTP / PTP.
        /// </summary>
        /// <param name="deviceDescription">Device description</param>
        /// <returns></returns>
        public MediaDevice GetDevice(string deviceDescription)
        {
            MediaDevice ptrDevice = GetDevices().FirstOrDefault(x => x.Description == deviceDescription);
            if (ptrDevice == null) return null;

            //Enumerate Directory to ensure Media Device in MTP or PTP Mode.
            //By default, Android device is connected as USB Charging only with no file access.
            ptrDevice.Connect();
            MediaDirectoryInfo rootInfo = ptrDevice.GetRootDirectory();
            try
            {
                var dirs = rootInfo.EnumerateDirectories();
                if (rootInfo.EnumerateDirectories().Count() > 0)
                {
                    return ptrDevice;
                }
            }
            catch { ptrDevice.Dispose(); ptrDevice = null; }
            return null;
        }
    }
}
