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
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace WindowsDevices.Gpio
{
    public interface IGpioPin : IDisposable
    {
        event TypedEventHandler<IGpioPin, IGpioPinValueChangedEventArgs> ValueChanged;
        GpioPinDriveMode GetDriveMode();
        bool IsDriveModeSupported(GpioPinDriveMode driveMode);
        GpioPinValue Read();
        void SetDriveMode(GpioPinDriveMode value);
        void Write(GpioPinValue value);
        TimeSpan DebounceTimeout { get; set; }
        int PinNumber { get; }
        GpioSharingMode SharingMode { get; }
    }
}
