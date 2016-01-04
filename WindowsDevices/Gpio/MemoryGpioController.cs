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
    public class MemoryGpioController : IGpioController
    {
        private int _pinCount;

        public MemoryGpioController(int pinCount)
        {
            _pinCount = pinCount;
        }

        public int PinCount
        {
            get
            {
                return _pinCount;
            }
        }

        public IGpioPin OpenPin(int pinNumber)
        {
            return new MemoryGpioPin(pinNumber, Windows.Devices.Gpio.GpioSharingMode.Exclusive);
        }
    }
}
