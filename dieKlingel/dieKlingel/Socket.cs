using Linphone;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace dieKlingel
{
    public static class Socket
    {
        public enum Context
        {
            Response,
            DeviceUpdate,
            ConnectionState,
            Movement,
            DisplayState,
            Log,
            Unlock,
            EnterPasscode,
            Call,
            Register,
            Unknown
        }
        public static string ContextAsString(Context context)
        {
            switch (context)
            {
                case Context.Response: return "response";
                case Context.DeviceUpdate: return "device-update";
                case Context.ConnectionState: return "connection-state";
                case Context.Movement: return "movement";
                case Context.DisplayState: return "display-state";
                case Context.Log: return "log";
                case Context.Unlock: return "unlock";
                case Context.EnterPasscode: return "enter-passcode";
                case Context.Call: return "call";
                case Context.Register: return "register";
                default: return "unknown";
            }
        }
        public static Context StringAsContext(String context)
        {
            Context res = Context.Unknown;
            for (int i = (int)Context.Response; i != (int)Context.Unknown; i++)
            {
                Context ctx = (Context)i;
                if (ContextAsString(ctx) == context)
                {
                    res = ctx;
                }
            }
            return res;
        }

        public static JObject Send(Context context, JObject obj)
        {
            JObject result = new JObject();
            result["status_code"] = 503;
            // check for null
            if (Global.Registry == null) return new JObject();
            if (Global.CtDomain == null) return new JObject();
            if (Global.Key == null) return new JObject();
            // Format Object
            obj["header"] = obj["header"] ?? new JObject();
            obj["header"]["time"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
            obj["body"] = obj["body"] ?? new JObject();
            obj["body"]["context"] = ContextAsString(context);
            // Encrypt object
            string iv = Cryptonia.RandomIV(16);
            string crypted = iv + Cryptonia.Encrypt(JsonConvert.SerializeObject(obj), Global.Key, iv);
            // Send to Server
            try
            {
                RestClient client = new RestClient(Global.CtDomain);
                client.Timeout = 500;
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Token", Global.Registry);
                request.AddParameter("application/json", crypted, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if((int)response.StatusCode == 200)
                {
                    string text = response.Content.Substring(16);
                    iv = response.Content.Substring(0, 16);
                    string plainres = Cryptonia.Decrypt(text, Global.Key, iv);
                    result = JObject.Parse(plainres);
                }
                result["status_code"] = (int)response.StatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            } 
            return result;
        }
        public static void SipUpdate(string username, string password, string domain, int port)
        {
            Global.Core.ClearAllAuthInfo();
            Global.Core.ClearProxyConfig();
            // 2020-12-27, 19:50 by Kai Mayer // register with settings
            AuthInfo authInfo = Factory.Instance.CreateAuthInfo(username, null, password, null, null, domain);
            Global.Core.AddAuthInfo(authInfo);
            ProxyConfig proxyConfig = Global.Core.CreateProxyConfig();
            Address identity = Factory.Instance.CreateAddress("sip:sample@domain.tld;transport=udp");
            identity.Port = port;
            identity.Domain = domain;
            identity.Transport = TransportType.Udp;
            identity.Username = username;
            proxyConfig.Edit();
            proxyConfig.IdentityAddress = identity;
            proxyConfig.ServerAddr = domain + ":" + port.ToString();
            proxyConfig.Route = domain + ":" + port.ToString();
            //proxyConfig.Route = identity.Domain; // 2020-12-27, 14:17 by Kai Mayer //  EntryDomain.Text; // 2020-12-27, 10:15 by Kai Mayer //  + ":5061";
            proxyConfig.RegisterEnabled = true;
            proxyConfig.Done();
            Global.Core.AddProxyConfig(proxyConfig);
            Global.Core.DefaultProxyConfig = proxyConfig;

            Global.Core.RefreshRegisters();
            Transports transports = Global.Core.TransportsUsed;
            transports.TlsPort = -1;
            transports.TcpPort = -1;
            transports.UdpPort = -1;
            Global.Core.Transports = transports; 
        }

        public static void SipClear()
        {
            ProxyConfig proxyConfig = Global.Core.DefaultProxyConfig;
            proxyConfig.Edit();
            proxyConfig.PublishEnabled = true;
            proxyConfig.PublishExpires = 0;
            proxyConfig.RegisterEnabled = false;
            proxyConfig.Done();
            Global.Core.AddProxyConfig(proxyConfig);
            Global.Core.DefaultProxyConfig = proxyConfig;


            Global.Core.RefreshRegisters();
            Transports transports = Global.Core.TransportsUsed;
            transports.TlsPort = -1;
            transports.TcpPort = -1;
            transports.UdpPort = -1;
            Global.Core.Transports = transports;
        }
    }
}
