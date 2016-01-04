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
using System.ComponentModel;
using System.Threading;
using Windows.UI.Core;

namespace MotionSensorToWeMo.Model
{
    public class ProgramModel : INotifyPropertyChanged
    {
        private ObservableSetCollection<string> _deviceNames = new ObservableSetCollection<string>();
        private string _durationInSeconds;
        private string _status;
        private bool _isRunning = false;
        private Timer _endProgramTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        public ProgramModel()
        {
            _endProgramTimer = new Timer(Timer_ProgramComplete, null, Timeout.Infinite, Timeout.Infinite);
            _deviceNames.CollectionChanged += _deviceNames_CollectionChanged;
        }

        public ObservableSetCollection<string> Devices
        {
            get
            {
                return this._deviceNames;
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
            }
        }

        public bool ValidateAndUpdateStatus()
        {
            if (this._deviceNames.Count == 0)
            {
                Status = "No Devices";
                return false;
            }
            try
            {
                int duration = Int32.Parse(_durationInSeconds);
                if (duration <= 0)
                {
                    Status = "Invalid Duration";
                    return false;
                }
            }
            catch (FormatException e)
            {
                Status = "Invalid Duration";
                return false;
            }
            catch (ArgumentNullException e)
            {
                Status = "Invalid Duration";
                return false;
            }
            Status = IsRunning ? "Running" : "Idle";
            return true;
        }

        public string Status
        {
            get
            {
                ValidateAndUpdateStatus();
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                    }
                }
            }
        }

        public void RunProgram()
        {
            if (!ValidateAndUpdateStatus())
            {
                return;
            }
            IsRunning = true;
            _endProgramTimer.Change(Int32.Parse(DurationInSeconds)*1000, Timeout.Infinite);
        }

        private void _deviceNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ValidateAndUpdateStatus();
        }

        private void Timer_ProgramComplete(object stateInfo)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        () =>
                        {
                            IsRunning = false;
                        });
        }
    }
}
