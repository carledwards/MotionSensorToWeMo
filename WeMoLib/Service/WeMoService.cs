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
using IoT.WeMo.UPnP;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.System.Threading;

namespace IoT.WeMo.Service
{
    public class WeMoService
    {
        private const int DeviceRefreshIntervalInSeconds = 15;
        private const string SetBinaryStatePayloadTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">"
        + "\n <s:Body>"
        + "\n  <u:SetBinaryState xmlns:u=\"urn:Belkin:service:basicevent:1\">"
        + "\n   <BinaryState>{0}</BinaryState>"
        + "\n  </u:SetBinaryState>"
        + "\n </s:Body>"
        + "\n</s:Envelope>";
        private const string GetBinaryStatePayload = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">"
        + "\n <s:Body>"
        + "\n  <u:GetBinaryState xmlns:u=\"urn:Belkin:service:basicevent:1\">"
        + "\n  </u:GetBinaryState>"
        + "\n </s:Body>"
        + "\n</s:Envelope>";
        private const string WemoUrlTemplate = "http://{0}:{1}/upnp/control/basicevent1";

        private SSDP _ssdp = null;

        public WeMoService()
        {
            _ssdp = new SSDP(OnDeviceFound, OnScanningNetwork, DeviceRefreshIntervalInSeconds);
        }

        public IWeMoServiceCallback ServiceCallback { get; set; }

        private static Dictionary<string, string> DictionaryFromXDocument(XDocument doc)
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            foreach (XElement element in doc.Descendants())
            {
                if (element.HasElements)
                {
                    continue;
                }
                int keyInt = 0;
                string keyName = element.Name.LocalName;
                while (dataDictionary.ContainsKey(keyName))
                {
                    keyName = element.Name.LocalName + "_" + keyInt++;
                }
                dataDictionary.Add(keyName, element.Value);
            }
            return dataDictionary;
        }

        public async void ResolveDevice(string location)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Motion Sensor To WeMo / C#");
                    string response = await httpClient.GetStringAsync(location);
                    XDocument doc = XDocument.Parse(response);
                    Dictionary<string, string> dictionary = DictionaryFromXDocument(doc);

                    // ensure this is a device we are interested in

                    // verify we have all the fields we are interested in
                    if (!(dictionary.ContainsKey("friendlyName")
                        && dictionary.ContainsKey("deviceType")
                        && dictionary.ContainsKey("UDN")
                        && dictionary.ContainsKey("binaryState")))
                    {
                        return;
                    }

                    if (!dictionary["UDN"].StartsWith("uuid:Lightswitch"))
                    {
                        return;
                    }

                    if (!dictionary["deviceType"].Equals("urn:Belkin:device:lightswitch:1"))
                    {
                        return;
                    }

                    Uri wemoUri = new Uri(location);
                    string friendlyName = dictionary["friendlyName"];

                    // get the binary state of this device
                    Dictionary<string, string> result = await MakeApiRequest(wemoUri.Host, wemoUri.Port, "GetBinaryState",
                        GetBinaryStatePayload);
                    bool state = result.ContainsKey("BinaryState") && result["BinaryState"] == "1";

                    IWeMoServiceCallback callback = ServiceCallback;
                    if (callback != null)
                    {
                        callback.OnDeviceFoundAsync(new WeMoDevice(friendlyName, wemoUri.Host, wemoUri.Port, location, state));
                    }
                }
            }
            catch (Exception)
            {
                // ignore 
                // TODO LOG
            }
        }

        public async void OnDeviceFound(string location)
        {
            await ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) => ResolveDevice(location)));
        }

        public void OnScanningNetwork(bool active)
        {
            IWeMoServiceCallback callback = ServiceCallback;
            if (callback != null)
            {
                callback.OnNetworkScanningChangeAsync(active);
            }
        }

        private async Task<Dictionary<string, string>> MakeApiRequest(string host, int port, string soapActionName, string payload)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Motion Sensor To WeMo / C#");

                // must send quotes around the value!!
                httpClient.DefaultRequestHeaders.Add("SOAPACTION",
                    String.Format("\"urn:Belkin:service:basicevent:1#{0}\"", soapActionName));

                using (HttpResponseMessage response = await httpClient.PostAsync(
                    String.Format(WemoUrlTemplate, host, port),
                    new StringContent(payload, Encoding.UTF8, "text/xml")))
                {
                    // must read as bytes as the "Content.ReadAsStringAsync" cannot handle utf-8
                    byte[] responseAsBytes = await response.Content.ReadAsByteArrayAsync();
                    // assuming UTF-8 as the encoding
                    string responseAsString = Encoding.UTF8.GetString(responseAsBytes);
                    XDocument doc = XDocument.Parse(responseAsString);
                    return DictionaryFromXDocument(doc);
                }
            }
        }

        public async void SendDeviceStateAsync(WeMoDevice device)
        {
            if (device == null)
            {
                return;
            }
            await ThreadPool.RunAsync(new WorkItemHandler(async (IAsyncAction) =>
                 await MakeApiRequest(device.IpAddress, device.Port, "SetBinaryState",
                     String.Format(SetBinaryStatePayloadTemplate, device.State ? "1" : "0"))));
        }
    }
}
