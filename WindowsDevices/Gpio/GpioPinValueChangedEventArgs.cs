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

namespace WindowsDevices.Gpio
{
    class GpioPinValueChangedEventArgs : IGpioPinValueChangedEventArgs
    {
        private GpioPinEdge _edge;

        public GpioPinValueChangedEventArgs(GpioPinEdge edge)
        {
            _edge = edge;
        }

        public GpioPinEdge Edge
        {
            get
            {
                return _edge;
            }
        }
    }
}
