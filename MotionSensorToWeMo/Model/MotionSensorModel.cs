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
using Windows.ApplicationModel.Resources;
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
        private string _errorMessageKey;
        private IGpioController _controller;
        private ResourceLoader _resourceLoader;


        public event PropertyChangedEventHandler PropertyChanged;

        public MotionSensorModel()
        {
            Initialize();
        }

        private string ErrorMessage
        {
            get
            {
                if (_errorMessageKey == null)
                {
                    return null;
                }
                return _resourceLoader.GetString(_errorMessageKey);
            }
            set
            {
                _errorMessageKey = value;
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
                    ErrorMessage = "GpioNotAvailable";
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
            catch (FormatException)
            {
                ErrorMessage = "PinNumberInvalid";
                return;
            }
            catch (ArgumentNullException)
            {
                ErrorMessage = "PinNumberNotSet";
                return;
            }

            try
            {
                _sensorPin = _controller.OpenPin(pinAsNumber);
            }
            catch (Exception)
            {
                ErrorMessage = "PinNotAvailable";
                return;
            }

            _sensorPin.SetDriveMode(Windows.Devices.Gpio.GpioPinDriveMode.Input);
            _sensorPin.ValueChanged += _sensorPin_ValueChangedAsync;
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
            _resourceLoader = new ResourceLoader();
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
                    return ErrorMessage != null ? ErrorMessage : _resourceLoader.GetString("Unknown");
                }
                else
                {
                    return _sensorPin.Read() == Windows.Devices.Gpio.GpioPinValue.High ? _resourceLoader.GetString("High") : _resourceLoader.GetString("Low");
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

        private async void _sensorPin_ValueChangedAsync(IGpioPin sender, IGpioPinValueChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
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
