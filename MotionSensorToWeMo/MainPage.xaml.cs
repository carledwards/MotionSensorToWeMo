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

using IoT.WeMo.Model;
using IoT.WeMo.Service;
using Windows.UI.Xaml.Controls;

namespace MotionSensorToWeMo
{
    public sealed partial class MainPage : Page
    {
        public WeMoViewModel ViewModel { get; set; }
        private WeMoService _wemo = new WeMoService(); 

        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = _wemo.Model;
        }
    }
}
