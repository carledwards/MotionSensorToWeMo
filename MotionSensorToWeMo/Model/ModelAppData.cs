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

using System.Runtime.Serialization;

namespace MotionSensorToWeMo.Model
{
    [DataContract]
    public class ModelAppData
    {
        [DataMember]
        public ProgramModel ProgramModel { get; set; }
        [DataMember]
        public MotionSensorModel MotionSensorModel { get; set; }
    }
}
