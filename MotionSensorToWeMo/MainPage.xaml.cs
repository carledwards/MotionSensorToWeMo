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

using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using MotionSensorToWeMo.Model;
using IoT.WeMo.Service;
using Windows.ApplicationModel.DataTransfer;
using System.ComponentModel;

namespace MotionSensorToWeMo
{
    public sealed partial class MainPage : Page
    {
        public WeMoServiceModel WeMoServiceModel { get; set; }
        public MotionSensorModel MotionSensorModel { get; set; }
        public ProgramModel ProgramModel { get; set; }
        private WeMoService _wemo = new WeMoService();

        public MainPage()
        {
            this.InitializeComponent();
            this.WeMoServiceModel = new WeMoServiceModel(_wemo);
            this.ProgramModel = new ProgramModel(this.WeMoServiceModel);
            this.MotionSensorModel = new MotionSensorModel();
            this.MotionSensorModel.PropertyChanged += MotionSensorModel_PropertyChanged;
        }

        private void ToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void programDeviceList_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        private async void programDeviceList_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                string name = await e.DataView.GetTextAsync();
                this.ProgramModel.Devices.Append(name);
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        private void foundDeviceList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var deviceModel = e.Items[0];
            if (deviceModel is DeviceModel)
            {
                e.Data.SetText((deviceModel as DeviceModel).DeviceName);
                e.Data.RequestedOperation = DataPackageOperation.Copy;
            }
            else
            {
                e.Data.RequestedOperation = DataPackageOperation.None;
            }
        }

        private void MotionSensorModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Triggered"))
            {
                ProgramModel.RunProgram();
            }
        }
    }
}
