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

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace IoT.WeMo.UPnP
{
    class SSDP
    {
        private const string LocationPrefix = "location: ";
        private const string BelkinWemoUserAgent = "X-User-Agent: redsonic";

        private Timer _discoveryTimer;
        private Timer _scanningNetworkTimer;
        private Action<string> _onDataCallback;
        private Action<bool> _scanningNetworkCallback;
        private DatagramSocket _socket;
        private readonly HostName _broadcastHostname = new HostName("239.255.255.250");

        public SSDP(Action<string> onDataCallback, Action<bool> scanningNetworkCallback, int refreshIntervalInSeconds)
        {
            this._onDataCallback = onDataCallback;
            this._scanningNetworkCallback = scanningNetworkCallback;
            StartDiscovery(refreshIntervalInSeconds);
            _scanningNetworkTimer = new Timer(Timer_DiscoveryCompleted, null, Timeout.Infinite, Timeout.Infinite);
        }

        private async Task SendMessage(string message)
        {
            if (_socket == null)
            {
                _socket = new DatagramSocket();
                _socket.MessageReceived += OnMessageReceived;
            }
            try
            {
                using (var os = await _socket.GetOutputStreamAsync(_broadcastHostname, "1900"))
                {
                    using (var writer = new DataWriter(os))
                    {
                        writer.WriteBytes(Encoding.UTF8.GetBytes(message));
                        await writer.StoreAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _socket = null;
                throw ex;
            }
        }

        private async void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            using (var result = args.GetDataStream())
            {
                using (var resultStream = result.AsStreamForRead(1024))
                {
                    using (var reader = new StreamReader(resultStream))
                    {
                        var response = await reader.ReadToEndAsync();

                        // keep out non-Belkin WeMo responses
                        if (response.IndexOf(BelkinWemoUserAgent, System.StringComparison.Ordinal) == -1)
                        {
                            return;
                        }

                        // get the LOCATION url
                        var locationStartingIndex = response.ToLower().IndexOf(LocationPrefix, System.StringComparison.Ordinal);
                        if (locationStartingIndex == -1)
                        {
                            return;
                        }
                        locationStartingIndex = locationStartingIndex + LocationPrefix.Length;
                        var locationEndingIndex = response.IndexOf("\r", locationStartingIndex + 1);
                        if (locationEndingIndex == -1)
                        {
                            return;
                        }
                        var location = response.Substring(locationStartingIndex, locationEndingIndex - locationStartingIndex);
                        _onDataCallback(location);
                    }
                }
            }
        }

        public void StartDiscovery(int refreshIntervalInSeconds)
        {
            _discoveryTimer = new Timer(Timer_DiscoveryTick, null, 0, refreshIntervalInSeconds * 1000);
        }

        private async void Timer_DiscoveryTick(object stateInfo)
        {
            await Discover();
        }

        private async void Timer_DiscoveryCompleted(object stateInfo)
        {
            if (_scanningNetworkCallback != null)
            {
                _scanningNetworkCallback(false);
            }
        }

        public async Task Discover()
        {
            if (_scanningNetworkCallback != null)
            {
                _scanningNetworkCallback(true);
                _scanningNetworkTimer.Change(4000, Timeout.Infinite);
            }
            string req = "M-SEARCH * HTTP/1.1\r\n" +
                         "HOST: 239.255.255.250:1900\r\n" +
                         "ST:upnp:rootdevice\r\n" +
                         "MAN:\"ssdp:discover\"\r\n" +
                         "MX:3\r\n\r\n";
            await SendMessage(req);
        }
    }
}
