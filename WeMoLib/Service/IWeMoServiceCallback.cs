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

using IoT.WeMo.Data;

namespace IoT.WeMo.Service
{
    public interface IWeMoServiceCallback
    {
        void OnDeviceFound(WeMoDevice device);
        void OnNetworkScanningChange(bool active);
    }
}
