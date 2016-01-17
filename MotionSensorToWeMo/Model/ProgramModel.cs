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
        public enum ProgramStatus { Error, Waiting, Sleeping, Running, Finished };

        private ObservableSetCollection<string> _deviceNames = new ObservableSetCollection<string>();
        private string _durationInSeconds;
        private string _statusMessageResourceKey;
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

        [DataMember]
        public string DurationInSeconds
        {
            get
            {
                return _durationInSeconds;
            }
            set
            {
                _durationInSeconds = value;
                StatusMessage = ValidateProgramAndGetStatusMessageResourceKey();
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DurationInSeconds"));
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
                StatusMessage = ValidateProgramAndGetStatusMessageResourceKey();
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
                StatusMessage = ValidateProgramAndGetStatusMessageResourceKey();
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        public ProgramStatus Status { get; set; }

        private string ValidateProgramAndGetStatusMessageResourceKey()
        {
            if (Status == ProgramStatus.Running)
            {
                return "Running";
            }

            if (this._deviceNames.Count == 0)
            {
                Status = ProgramStatus.Error;
                return "NoDevices";
            }
            try
            {
                int duration = Int32.Parse(_durationInSeconds);
                if (duration <= 0)
                {
                    Status = ProgramStatus.Error;
                    return "InvalidDuration";
                }
            }
            catch (FormatException)
            {
                Status = ProgramStatus.Error;
                return "InvalidDuration";
            }
            catch (ArgumentNullException)
            {
                Status = ProgramStatus.Error;
                return "InvalidDuration";
            }
            if (_sunrise == null)
            {
                Status = ProgramStatus.Error;
                return "SunriseNotSet";
            }
            if (_sunset == null)
            {
                Status = ProgramStatus.Error;
                return "SunsetNotSet";
            }

            if (_sunrise >= _sunset)
            {
                Status = ProgramStatus.Error;
                return "SunriseAfterSunset";
            }

            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            if (currentTime > _sunrise && currentTime < _sunset)
            {
                Status = ProgramStatus.Sleeping;
                return "Sleeping";
            }

            Status = ProgramStatus.Waiting;
            return "Waiting";
        }

        public string StatusMessage
        {
            get
            {
                _statusMessageResourceKey = ValidateProgramAndGetStatusMessageResourceKey();
                if (_statusMessageResourceKey == null)
                {
                    return null;
                }
                var message = _resourceLoader.GetString(_statusMessageResourceKey);
                return message;
            }
            set
            {
                if (_statusMessageResourceKey != value)
                {
                    _statusMessageResourceKey = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("StatusMessage"));
                    }
                }
            }
        }

        public void RunProgram()
        {
            StatusMessage = ValidateProgramAndGetStatusMessageResourceKey();
            if (Status != ProgramStatus.Waiting)
            {
                return;
            }

            _devicesTriggered.Clear();
            Status = ProgramStatus.Running;
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
            StatusMessage = ValidateProgramAndGetStatusMessageResourceKey();
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
                            Status = ProgramStatus.Finished;
                        });
        }
    }
}
