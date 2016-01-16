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

namespace WindowsDevices.Gpio
{
    class WindowsIoTGpioController : IGpioController
    {
        private Windows.Devices.Gpio.GpioController _nativeController;

        public static IGpioController GetDefaultGpioController()
        {
            Windows.Devices.Gpio.GpioController defaultController = Windows.Devices.Gpio.GpioController.GetDefault();
            if (defaultController == null)
            {
                return null;
            }
            return new WindowsIoTGpioController(defaultController);
        }

        private WindowsIoTGpioController(Windows.Devices.Gpio.GpioController nativeController)
        {
            _nativeController = nativeController;
        }

        public int PinCount
        {
            get
            {
                return _nativeController.PinCount;
            }
        }

        public IGpioPin OpenPin(int pinNumber)
        {
            return new WindowsIoTGpioPin(_nativeController.OpenPin(pinNumber));
        }
    }
}
