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

using System.Collections.ObjectModel;

namespace WeMoLib.Util
{
    public class ObservableSetCollection<T> : ObservableCollection<T>
    {
        public void Append(T item)
        {
            // avoid duplicates
            if (Contains(item)) return;
            base.Add(item);
        }
    }
}
