//*********************************************************
//
// Copyright (c) Carl Edwards. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;

namespace IoT.WeMo.Data
{
    public class WeMoDevice
    {
        private String _deviceName;
        private int _port;
        private string _ipAddress;
        private DateTime _timeStamp;
        private string _uri;
        private bool _state;

        public WeMoDevice(string deviceName, string ipAddress, int port, string uri, bool state)
        {
            this._deviceName = deviceName;
            this._ipAddress = ipAddress;
            this._port = port;
            this._uri = uri;
            this._state = state;
            this._timeStamp = DateTime.Now;
        }

        public IWeMoDeviceCallback Callback
        {
            get; set;
        }

        public string DeviceName
        {
            get
            {
                return _deviceName;
            }
        }

        public string IpAddress
        {
            get
            {
                return _ipAddress;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != (bool)value)
                {
                    _state = (bool)value;
                    IWeMoDeviceCallback callback = Callback;
                    if (callback != null)
                    {
                        callback.OnStateChange(this, value);
                    }
                }
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }
        }

        public string URI
        {
            get
            {
                return _uri;
            }
        }
    }
}
