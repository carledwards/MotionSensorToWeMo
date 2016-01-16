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

using MotionSensorToWeMo.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace MotionSensorToWeMo.Model
{
    [DataContract]
    public class ProgramModel : INotifyPropertyChanged
    {
        private ObservableSetCollection<string> _deviceNames = new ObservableSetCollection<string>();
        private string _durationInSeconds;
        private string _statusKey;
        private bool _isRunning = false;
        private Timer _endProgramTimer;
        private List<DeviceModel> _devicesTriggered;
        private WeMoServiceModel _serviceModel;
        private ResourceLoader _resourceLoader;
        private TimeSpan _sunrise;
        private TimeSpan _sunset;


        public event PropertyChangedEventHandler PropertyChanged;

        public ProgramModel()
        {
        }

        public void Initialize(WeMoServiceModel serviceModel)
        {
            _resourceLoader = new ResourceLoader();
            _serviceModel = serviceModel;
            _endProgramTimer = new Timer(Timer_ProgramCompleteAsync, null, Timeout.Infinite, Timeout.Infinite);
            _deviceNames.CollectionChanged += _deviceNames_CollectionChanged;
            _devicesTriggered = new List<DeviceModel>();
        }

        [DataMember]
        public ObservableSetCollection<string> Devices
        {
            get
            {
                return this._deviceNames;
            }
            set
            {
                this._deviceNames = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    ValidateAndUpdateStatus();
                }
            }
        }

        [DataMember]
        public string DurationInSeconds
        {
            get
            {
                return _durationInSeconds;
            }
            set
            {
                if (_durationInSeconds != value)
                {
                    _durationInSeconds = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("DurationInSeconds"));
                    }
                }
                ValidateAndUpdateStatus();
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        [DataMember]
        public TimeSpan Sunrise
        {
            get { return _sunrise; }
            set {
                _sunrise = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        [DataMember]
        public TimeSpan Sunset
        {
            get { return _sunset; }
            set {
                _sunset = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        public bool ValidateAndUpdateStatus()
        {
            if (this._deviceNames.Count == 0)
            {
                Status = "NoDevices";
                return false;
            }
            try
            {
                int duration = Int32.Parse(_durationInSeconds);
                if (duration <= 0)
                {
                    Status = "InvalidDuration";
                    return false;
                }
            }
            catch (FormatException)
            {
                Status = "InvalidDuration";
                return false;
            }
            catch (ArgumentNullException)
            {
                Status = "InvalidDuration";
                return false;
            }
            if (_sunrise == null)
            {
                Status = "SunriseNotSet";
            }
            if (_sunset == null)
            {
                Status = "SunsetNotSet";
            }
            Status = IsRunning ? "Running" : "Idle";
            return true;
        }

        public string Status
        {
            get
            {
                ValidateAndUpdateStatus();
                if (_statusKey == null)
                {
                    return null;
                }
                return _resourceLoader.GetString(_statusKey);
            }
            set
            {
                if (_statusKey != value)
                {
                    _statusKey = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                    }
                }
            }
        }

        public void RunProgram()
        {
            if (IsRunning)
            {
                return;
            }
            if (!ValidateAndUpdateStatus())
            {
                return;
            }
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            if (currentTime > _sunrise && currentTime < _sunset)
            {
                return;
            }

            _devicesTriggered.Clear();
            IsRunning = true;
            foreach (string deviceName in _deviceNames)
            {
                DeviceModel device = _serviceModel.GetDeviceByName(deviceName);
                if (device != null && !device.State)
                {
                    device.State = true;
                    _devicesTriggered.Add(device);
                }
            }
            _endProgramTimer.Change(Int32.Parse(DurationInSeconds)*1000, Timeout.Infinite);
        }

        private void _deviceNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ValidateAndUpdateStatus();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
            }
        }

        private async void Timer_ProgramCompleteAsync(object stateInfo)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        () =>
                        {
                            foreach (DeviceModel device in _devicesTriggered)
                            {
                                device.State = false;
                            }
                            _devicesTriggered.Clear();
                            IsRunning = false;
                        });
        }
    }
}
