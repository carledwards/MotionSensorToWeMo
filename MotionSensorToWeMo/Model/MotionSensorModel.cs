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
using System.ComponentModel;
using System.Runtime.Serialization;
using Windows.UI.Core;
using WindowsDevices.Gpio;

namespace MotionSensorToWeMo.Model
{
    [DataContract]
    public class MotionSensorModel : INotifyPropertyChanged
    {
        private IGpioPin _sensorPin;
        private bool _emulateGpio = true;
        private string _pinNumber;
        private string _errorMessage;
        private IGpioController _controller;

        public event PropertyChangedEventHandler PropertyChanged;

        public MotionSensorModel()
        {
            Initialize();
        }

        private string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PinState"));
                }
            }
        }

        private IGpioController Controller
        {
            get
            {
                return _controller;
            }
            set
            {
                _controller = value;
                if (_controller == null)
                {
                    ErrorMessage = "Gpio not available";
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AllowImpersonatingMotionSensor"));
                }
            }
        }

        private void InitializePin()
        {
            if (_controller == null)
            {
                return;
            }

            if (_sensorPin != null)
            {
                _sensorPin.Dispose();
                _sensorPin = null;
            }

            int pinAsNumber;
            try
            {
                pinAsNumber = Int32.Parse(_pinNumber);
            }
            catch (FormatException e)
            {
                ErrorMessage = "Pin number invalid";
                return;
            }
            catch (ArgumentNullException e)
            {
                ErrorMessage = "Pin number not set";
                return;
            }

            try
            {
                _sensorPin = _controller.OpenPin(pinAsNumber);
            }
            catch (Exception e)
            {
                ErrorMessage = "Pin not available";
                return;
            }

            _sensorPin.SetDriveMode(Windows.Devices.Gpio.GpioPinDriveMode.Input);
            _sensorPin.ValueChanged += _sensorPin_ValueChanged;
            if (_sensorPin is IGpioPinImpersonator)
            {
                (_sensorPin as IGpioPinImpersonator).SetPinValue(Windows.Devices.Gpio.GpioPinValue.Low);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("PinState"));
                PropertyChanged(this, new PropertyChangedEventArgs("AllowImpersonatingMotionSensor"));
            }
            ErrorMessage = null;
        }

        private void Initialize()
        {
            Controller = _emulateGpio ? GpioController.GetMemoryGpioController() : GpioController.GetNativeDefaultGpioController();
            InitializePin();
        }

        [DataMember]
        public Boolean? EmulateGpio
        {
            get
            {
                return _emulateGpio;
            }

            set
            {
                if (_emulateGpio != value)
                {
                    _emulateGpio = (Boolean)value;
                    if (_sensorPin != null)
                    {
                        _sensorPin.ValueChanged -= _sensorPin_ValueChanged;
                        _sensorPin.Dispose();
                        _sensorPin = null;
                    }
                }
                Initialize();
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        [DataMember]
        public string PinNumber
        {
            get
            {
                return _pinNumber;
            }
            set
            {
                _pinNumber = value;
                InitializePin();
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DataContractMemberChanged"));
                }
            }
        }

        public string PinState
        {
            get
            {
                if (_sensorPin == null)
                {
                    return _errorMessage != null ? _errorMessage : "Unknown";
                }
                else
                {
                    return _sensorPin.Read() == Windows.Devices.Gpio.GpioPinValue.High ? "High" : "Low";
                }
            }
        }

        public bool AllowImpersonatingMotionSensor
        {
            get
            {
                return _sensorPin != null && _sensorPin is IGpioPinImpersonator;
            }
        }

        public void TriggerMotionSensor()
        {
            if (_sensorPin is IGpioPinImpersonator)
            {
                (_sensorPin as IGpioPinImpersonator).SetPinValue(Windows.Devices.Gpio.GpioPinValue.High, 1000);
            }
        }

        private void _sensorPin_ValueChanged(IGpioPin sender, IGpioPinValueChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.High,
                            () =>
                            {
                                PropertyChanged(this, new PropertyChangedEventArgs("PinState"));
                                if (args.Edge == Windows.Devices.Gpio.GpioPinEdge.RisingEdge)
                                {
                                    PropertyChanged(this, new PropertyChangedEventArgs("Triggered"));
                                }
                            });
            }
        }
    }
}
