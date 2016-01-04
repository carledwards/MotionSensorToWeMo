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
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace WindowsDevices.Gpio
{
    class TimerState
    {
        public Timer Timer { get; set; }
        public GpioPinValue ResetPinValue { get; set; }
    }

    class MemoryGpioPin : IGpioPin, IGpioPinImpersonator
    {
        private int _pinNumber;
        private GpioSharingMode _sharingMode;
        private GpioPinDriveMode _driveMode;
        private GpioPinValue _pinValue;

        public MemoryGpioPin(int pinNumber, GpioSharingMode sharingMode)
        {
            _pinNumber = pinNumber;

            if (sharingMode != GpioSharingMode.Exclusive)
            {
                throw new NotImplementedException();
            }
            _sharingMode = sharingMode;
            SetDriveMode(GpioPinDriveMode.Input);
        }

        public TimeSpan DebounceTimeout
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int PinNumber
        {
            get
            {
                return _pinNumber;
            }
        }

        public GpioSharingMode SharingMode
        {
            get
            {
                return _sharingMode;
            }
        }

        public event TypedEventHandler<IGpioPin, IGpioPinValueChangedEventArgs> ValueChanged;

        public void Dispose()
        {
        }

        public GpioPinDriveMode GetDriveMode()
        {
            return _driveMode;
        }

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            switch (driveMode)
            {
                case GpioPinDriveMode.InputPullDown:
                case GpioPinDriveMode.InputPullUp:
                    return true;
                default:
                    return false;
            }
        }

        public GpioPinValue Read()
        {
            return PinValue;
        }

        public void SetDriveMode(GpioPinDriveMode value)
        {
            switch (value)
            {
                case GpioPinDriveMode.Input:
                    PinValue = GpioPinValue.Low;
                    break;
                case GpioPinDriveMode.InputPullDown:
                    PinValue = GpioPinValue.Low;
                    break;
                case GpioPinDriveMode.InputPullUp:
                    PinValue = GpioPinValue.High;
                    break;
                default:
                    throw new NotImplementedException();
            }
            _driveMode = value;
        }

        public void Write(GpioPinValue value)
        {
            throw new NotImplementedException();
        }

        public void SetPinValue(GpioPinValue value)
        {
            PinValue = value;
        }

        public void SetPinValue(GpioPinValue value, int resetTime)
        {
            PinValue = value;
            TimerState state = new TimerState();
            Timer t = new Timer(ResetPinBack, state, Timeout.Infinite, Timeout.Infinite);
            state.Timer = t;
            state.ResetPinValue = value == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
            t.Change(resetTime, Timeout.Infinite);
        }

        private void ResetPinBack(Object stateInfo)
        {
            TimerState state = (TimerState)stateInfo;
            PinValue = state.ResetPinValue;
            state.Timer.Dispose();
        }

        private GpioPinValue PinValue
        {
            get
            {
                return _pinValue;
            }
            set
            {
                if (_pinValue != value)
                {
                    System.Diagnostics.Debug.WriteLine("pinValue Changing from {0} to {1}", _pinValue, value);
                    _pinValue = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new GpioPinValueChangedEventArgs(
                            value == GpioPinValue.High ? GpioPinEdge.RisingEdge : GpioPinEdge.FallingEdge));
                    }
                }
            }
        }
    }
}
