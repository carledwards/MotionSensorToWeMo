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
using IoT.WeMo.Service;
using MotionSensorToWeMo.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Windows.UI.Core;
using IoT.WeMo.Data;

namespace MotionSensorToWeMo.Model
{
    public class WeMoServiceModel : INotifyPropertyChanged, IWeMoServiceCallback
    {
        private ObservableCollection<DeviceModel> _devices = new ObservableSetCollection<DeviceModel>();
        private Dictionary<string, DeviceModel> _devicesByName = new Dictionary<string, DeviceModel>();
        private ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private bool _scanningNetwork;
        private WeMoService _wemoService;

        public WeMoService WeMoService
        {
            get
            {
                return _wemoService;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WeMoServiceModel(WeMoService service)
        {
            this._wemoService = service;
            this._wemoService.ServiceCallback = this;
        }

        public DeviceModel GetDeviceByName(string name)
        {
            return _devicesByName[name];
        }

        public bool ScanningNetwork
        {
            get
            {
                return _scanningNetwork;
            }
            set
            {
                _scanningNetwork = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ScanningNetwork"));
                }
            }
        }

        public ObservableCollection<DeviceModel> Devices
        {
            get
            {
                return this._devices;
            }
        }

        void InternalAdd(DeviceModel device)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                DeviceModel model = Devices.FirstOrDefault(x => x.DeviceName == device.DeviceName);
                if (model != null)
                {
                    model.State = device.State;
                }
                else
                {
                    _devices.Add(device);
                    _devicesByName[device.DeviceName] = device;
                }
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public async void OnDeviceFoundAsync(WeMoDevice device)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        () =>
                        {
                            InternalAdd(new DeviceModel(device, this));
                        }
            );
        }

        public async void OnNetworkScanningChangeAsync(bool active)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        () =>
                        {
                            this.ScanningNetwork = active;
                        }
            );
        }
    }
}
