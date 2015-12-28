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
using System.ComponentModel;

namespace IoT.WeMo.Model
{
    public class DeviceModel : INotifyPropertyChanged, IEquatable<DeviceModel>
    {
        private String _deviceName;
        private int _port;
        private string _ipAddress;
        private DateTime _timeStamp;
        private string _uri;
        private bool _state;

        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceModel(string deviceName, string ipAddress, int port, string uri, bool state)
        {
            this._deviceName = deviceName;
            this._ipAddress = ipAddress;
            this._port = port;
            this._uri = uri;
            this._state = state;
            this._timeStamp = DateTime.Now;
        }

        public string ButtonTitle
        {
            get
            {
                return _state ? "Turn Off" : "Turn On";
            }
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
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("State"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ButtonTitle"));
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

        public void ToggleState()
        {
            this.State = !this.State;
        }

        public bool Equals(DeviceModel other)
        {
            if (other == null) return false;
            return (this.DeviceName.Equals(other.DeviceName));
        }
    }
}
