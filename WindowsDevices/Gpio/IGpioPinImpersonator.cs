﻿//*********************************************************
//
// Copyright (c) Carl Edwards. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Windows.Devices.Gpio;

namespace WindowsDevices.Gpio
{
    public interface IGpioPinImpersonator
    {
        void SetPinValue(GpioPinValue value);
        void SetPinValue(GpioPinValue value, int delay);
    }
}
