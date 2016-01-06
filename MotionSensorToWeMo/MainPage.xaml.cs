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
using System.Runtime.Serialization.Json;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;

namespace MotionSensorToWeMo
{
    public sealed partial class MainPage : Page
    {
        private const string APPLICATION_DATA_FILENAME = "app_data_v1_a.json";

        public WeMoServiceModel WeMoServiceModel { get; set; }
        private WeMoService _wemo = new WeMoService();
        private ModelAppData ModelAppData { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
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
                this.ModelAppData.ProgramModel.Devices.Append(name);
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
                ModelAppData.ProgramModel.RunProgram();
            }
            else if (e.PropertyName.Equals("DataContractMemberChanged"))
            {
                WriteApplicationData();
            }
        }

        private void ProgramModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("DataContractMemberChanged"))
            {
                WriteApplicationData();
            }
        }

        private async void WriteApplicationData()
        {
            DataContractJsonSerializer madSer = new DataContractJsonSerializer(typeof(ModelAppData));
            MemoryStream madStream = new MemoryStream();
            madSer.WriteObject(madStream, ModelAppData);
            madStream.Position = 0;
            StreamReader sr = new StreamReader(madStream);
            StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
                APPLICATION_DATA_FILENAME, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, sr.ReadToEnd());
        }

        private async void Initialize()
        {
            this.WeMoServiceModel = new WeMoServiceModel(_wemo);

            // read from persistence
            ModelAppData mad = null;
            try
            {
                StorageFile file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(APPLICATION_DATA_FILENAME);
                string text = await FileIO.ReadTextAsync(file);

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                DataContractJsonSerializer madSer = new DataContractJsonSerializer(typeof(ModelAppData));
                mad = (ModelAppData)madSer.ReadObject(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            if (mad == null)
            {
                mad = new ModelAppData();
            }

            if (mad.ProgramModel == null)
            {
                mad.ProgramModel = new ProgramModel();
            }

            if (mad.MotionSensorModel == null)
            {
                mad.MotionSensorModel = new MotionSensorModel();
            }

            mad.ProgramModel.Initialize(WeMoServiceModel);
            mad.ProgramModel.PropertyChanged += ProgramModel_PropertyChanged;
            mad.MotionSensorModel.PropertyChanged += MotionSensorModel_PropertyChanged;

            this.ModelAppData = mad;
        }

        private void programDeviceList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void FlyoutDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            if (menuFlyoutItem != null)
            {
                var deviceName = menuFlyoutItem.DataContext as string;
                if (deviceName != null)
                {
                    ModelAppData.ProgramModel.Devices.Remove(deviceName);
                }
            }
        }
    }
}
