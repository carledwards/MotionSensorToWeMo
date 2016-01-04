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
    class WindowsIoTGpioPin : IGpioPin
    {
        private Windows.Devices.Gpio.GpioPin _nativeGpioPin;

        public WindowsIoTGpioPin(Windows.Devices.Gpio.GpioPin nativeGpioPin)
        {
            _nativeGpioPin = nativeGpioPin;
            nativeGpioPin.ValueChanged += NativeGpioPin_ValueChanged;
        }

        private void NativeGpioPin_ValueChanged(GpioPin sender, Windows.Devices.Gpio.GpioPinValueChangedEventArgs args)
        {
            ValueChanged(this, new WindowsDevices.Gpio.GpioPinValueChangedEventArgs(args.Edge));
        }

        public TimeSpan DebounceTimeout
        {
            get
            {
                return _nativeGpioPin.DebounceTimeout;
            }

            set
            {
                _nativeGpioPin.DebounceTimeout = value;
            }
        }

        public int PinNumber
        {
            get
            {
                return _nativeGpioPin.PinNumber;
            }
        }

        public GpioSharingMode SharingMode
        {
            get
            {
                return _nativeGpioPin.SharingMode;
            }
        }

        public event TypedEventHandler<IGpioPin, IGpioPinValueChangedEventArgs> ValueChanged;

        public void Dispose()
        {
            _nativeGpioPin.ValueChanged -= NativeGpioPin_ValueChanged;
            _nativeGpioPin.Dispose();
        }

        public GpioPinDriveMode GetDriveMode()
        {
            return _nativeGpioPin.GetDriveMode();
        }

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            return _nativeGpioPin.IsDriveModeSupported(driveMode);
        }

        public GpioPinValue Read()
        {
            return _nativeGpioPin.Read();
        }

        public void SetDriveMode(GpioPinDriveMode value)
        {
            _nativeGpioPin.SetDriveMode(value);
        }

        public void Write(GpioPinValue value)
        {
            _nativeGpioPin.Write(value);
        }
    }
}
