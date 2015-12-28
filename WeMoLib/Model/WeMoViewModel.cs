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

using IoT.WeMo.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Windows.UI.Core;

namespace IoT.WeMo.Model
{
    public class ObservableSetCollection<T> : ObservableCollection<T>
    {
        public void Append(T item)
        {
            // avoid duplicates
            if (Contains(item)) return;
            base.Add(item);
        }
    }

    public class WeMoViewModel
    {
        private ObservableCollection<DeviceModel> _devices = new ObservableSetCollection<DeviceModel>();
        private Dictionary<string, DeviceModel> _devicesByName = new Dictionary<string, DeviceModel>();
        private ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private WeMoService _service;

        public WeMoViewModel(WeMoService service)
        {
            this._service = service;
        }

        public ObservableCollection<DeviceModel> Devices
        {
            get
            {
                return this._devices;
            }
        }

        public void Add(string deviceName, string host, int port, string location, bool state)
        {
            // perform the update on the UI thread
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        () =>
                        {
                            DeviceModel device = new DeviceModel(deviceName, host, port, location, state);
                            InternalAdd(device);
                        });
        }

        void InternalAdd(DeviceModel device)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                if (!Devices.Contains(device))
                {
                    device.PropertyChanged += new PropertyChangedEventHandler(device_PropertyChanged);
                    _devices.Add(device);
                }
                _devicesByName[device.DeviceName] = device;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        private void device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("State"))
            {
                _service.SendDeviceState((DeviceModel)sender);
            }
        }
    }
}
