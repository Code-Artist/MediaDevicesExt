﻿using PortableDeviceApiLib;

namespace MediaDevices
{
    internal class EventCallback : IPortableDeviceEventCallback
    {
        private readonly MediaDevice device;

        public EventCallback(MediaDevice device)
        {
            this.device = device;
        }

        public void OnEvent(IPortableDeviceValues pEventParameters)
        {
            this.device.CallEvent(pEventParameters);
        }
    }
}
