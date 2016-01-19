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
        private const string APPLICATION_DATA_FILENAME = "app_data_v2.json";

        public WeMoServiceModel WeMoServiceModel { get; set; }

        private readonly TimeSpan MaxUptime = new TimeSpan(6, 0, 0);
        private WeMoService _wemo = new WeMoService();
        private ModelAppData ModelAppData { get; set; }
        private DispatcherTimer _timer;
        private DateTime _applicationStartTime;

        public MainPage()
        {
            _applicationStartTime = DateTime.Now;
            Initialize();
            this.InitializeComponent();
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

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            _timer.Tick += RefreshTime_Tick;
            _timer.Start();

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

        void RefreshTime_Tick(object sender, object e)
        {
            DateTime dt = DateTime.Now;
            currentTime.Text = dt.ToString();

            // Windows IoT isn't stable for long periods.  We try and force shutdown the system after 6 hours.
            // I could use Environment.TickCount to see if the system was up too long, but I wanted it to be easier
            // for deploying and testing.  The system can actually last for almost 24 hours, but 6 hours is a simple
            // and frequent number.
            TimeSpan ts = dt - _applicationStartTime;
            timeUntilRestart.Text = (MaxUptime - ts).ToString();
            if (ts > MaxUptime)
            {
                if (ModelAppData.ProgramModel.Halt())
                {
                    try
                    {
                        Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.Zero);
                    }
                    catch (Exception)
                    {
                        // shutdown is only allowed on IoT devices, we will just reset the start time
                        _applicationStartTime = DateTime.Now;
                        ModelAppData.ProgramModel.Resume();
                    }
                }
            }
        }
    }
}
