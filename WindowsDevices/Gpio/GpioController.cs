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

namespace WindowsDevices.Gpio
{
    public class GpioController
    {
        private static MemoryGpioController _memoryGpioController;

        public static IGpioController GetNativeDefaultGpioController()
        {
            return WindowsIoTGpioController.GetDefaultGpioController();
        }

        public static IGpioController GetMemoryGpioController()
        {
            if (_memoryGpioController == null)
            {
                _memoryGpioController = new MemoryGpioController(15);
            }
            return _memoryGpioController;
        }
    }
}
