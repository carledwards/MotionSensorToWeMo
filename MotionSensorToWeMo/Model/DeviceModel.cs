﻿//*********************************************************
//
// Copyright (c) Carl Edwards. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using IoT.WeMo.Data;
using System;
using System.ComponentModel;
using Windows.UI.Core;

namespace MotionSensorToWeMo.Model
{
    public class DeviceModel : INotifyPropertyChanged, IWeMoDeviceCallback
    {
        private WeMoDevice _device;
        private WeMoServiceModel _wemoServiceModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceModel(WeMoDevice device, WeMoServiceModel wemoServiceModel)
        {
            _device = device;
            _wemoServiceModel = wemoServiceModel;
        }

        public string DeviceName
        {
            get
            {
                return _device.DeviceName;
            }
        }

        public bool State
        {
            get
            {
                return _device.State;
            }
            set
            {
                if (_device.State != value)
                {
                    _device.State = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("State"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ButtonStateTitle"));
                    }
                    _wemoServiceModel.WeMoService.SendDeviceStateAsync(_device);
                }
            }
        }

        public async void OnStateChangeAsync(WeMoDevice device, bool state)
        {
            if (device != _device)
            {
                return;
            }
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        () =>
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("State"));
                            PropertyChanged(this, new PropertyChangedEventArgs("ButtonStateTitle"));
                        }
                        );
        }
    }
}
