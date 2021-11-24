using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Linphone;

namespace dieKlingel
{
    public class Doorunit
    {
        #region private const
        private const string DEFAULT_CT_SERVER = "https://ct.dieklingel.com/";
        private const string GATEWAY_PATH = "/gateway/post";
        private const string PREVIEW_PATH = "/client/preview";
        private const string REQ_CONTENT_TYPE = "text/plain";
        private const string NONE = "none";
        private const int KEY_LENGTH = 32;
        private const int IV_LENGTH = 16;
        private const int REQ_TIMEOUT = 6000;
        #endregion

        #region private variables
        private bool isReachable = false;
        private Uri url = new Uri(DEFAULT_CT_SERVER);
        private Core core = null;
        #endregion

        #region public variables
        public event EventHandler SaveStateChanged;
        [JsonProperty]
        public string Username { get; private set; }
        [JsonProperty]
        public string Registry { get; private set; }
        [JsonProperty]
        public string Password { get; private set; }
        public string Key
        {
            get
            {
                return Cryptonia.Normalize(Password, KEY_LENGTH);
            }
        }
        [JsonProperty]
        public Uri Url { get; private set; }
        [JsonProperty]
        public Account Account { get; private set; }
        public bool IsReachable { get { return this.isReachable; } }
        [JsonProperty]
        public bool IsRegisterd { get; private set; }
        public static string Id
        {
            get
            {
                string raw = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id + Plugin.DeviceInfo.CrossDeviceInfo.Current.Model;
                return raw.Replace(" ", "_");
            }
        }
        #endregion
        public Doorunit()
        {

        }

        public Doorunit(Core core)
        {
            this.core = core;
        }

        public Doorunit(Core core, string username, string password, string url)
        {
            this.core = core;
            SetAuth(username, password, url);
            SaveStateChanged?.Invoke(this, EventArgs.Empty);
        }

        #region private methods
        private void OnRegistrationStateChanged(Core ls, ProxyConfig config, RegistrationState state, string message)
        {
            switch (state)
            {
                case RegistrationState.Cleared:
                case RegistrationState.None:
                    this.isReachable = false;
                    break;
                default:
                    this.isReachable = true;
                    break;
            }
        }

        /**
         * register a new sip account, either the given one or the existing one
         * @param account to register
         */
        private void SipRegister(Account account = null)
        {
            this.Account = account ?? this.Account;
            if (null == this.Account)
            {
                System.Diagnostics.Debug.WriteLine("Cannot register a sip account, when there is no sip account in the given doorunit");
                return;
            }
            if (null == this.core)
            {
                System.Diagnostics.Debug.WriteLine("Cannot register a sip account, when there is no linphone core in the given doorunit");
                return;
            }
            this.core.ClearCallLogs();
            this.core.ClearProxyConfig();
            AuthInfo authInfo = Factory.Instance.CreateAuthInfo(this.Account.Sip.Username, null, this.Account.Sip.Password, null, null, this.Account.Sip.Domain);
            this.core.AddAuthInfo(authInfo);
            ProxyConfig proxyConfig = this.core.CreateProxyConfig();
            Address identity = Factory.Instance.CreateAddress("sip:sample@domain.tld;transport=udp");
            identity.Port = this.Account.Sip.Port;
            identity.Domain = this.Account.Sip.Domain;
            identity.Transport = TransportType.Udp;
            identity.Username = this.Account.Sip.Username;
            proxyConfig.Edit();
            proxyConfig.IdentityAddress = identity;
            proxyConfig.ServerAddr = this.Account.Sip.Domain + ":" + this.Account.Sip.Port.ToString();
            proxyConfig.Route = this.Account.Sip.Domain + ":" + this.Account.Sip.Port.ToString();
            //proxyConfig.Route = identity.Domain; // 2020-12-27, 14:17 by Kai Mayer //  EntryDomain.Text; // 2020-12-27, 10:15 by Kai Mayer //  + ":5061";
            proxyConfig.RegisterEnabled = true;
            proxyConfig.Done();
            this.core.AddProxyConfig(proxyConfig);
            this.core.DefaultProxyConfig = proxyConfig;
            this.core.RefreshRegisters();
            Transports transports = this.core.TransportsUsed;
            transports.TlsPort = -1;
            transports.TcpPort = -1;
            transports.UdpPort = -1;
            this.core.Transports = transports;
        }

        /**
         * unregister the current sip account
         */
        private void SipClear()
        {
            if (null == this.core)
            {
                System.Diagnostics.Debug.WriteLine("Cannot clear a sip account, when there is no linphone core in the given doorunit");
            }
            ProxyConfig proxyConfig = this.core.DefaultProxyConfig;
            proxyConfig.Edit();
            proxyConfig.PublishEnabled = true;
            proxyConfig.PublishExpires = 0;
            proxyConfig.RegisterEnabled = false;
            proxyConfig.Done();
            this.core.AddProxyConfig(proxyConfig);
            this.core.DefaultProxyConfig = proxyConfig;

            this.core.RefreshRegisters();
            Transports transports = this.core.TransportsUsed;
            transports.TlsPort = -1;
            transports.TcpPort = -1;
            transports.UdpPort = -1;
            this.core.Transports = transports;
        }
        #endregion

        #region public methods
        /**
         * sets the linphone core, which the doorunit is using
         * @param core to use
         * */
        public void SetCore(Core core)
        {
            this.core = core;
            this.core.Listener.OnRegistrationStateChanged += OnRegistrationStateChanged;
        }

        /**
         * sets the auth information, to build a connection to the doorunit
         * @param username for the ct-server
         * @param password to create a ct-server key
         * @param url the base url of the ct-server e.g. https://ct.dieklingel.com/
         */
        public void SetAuth(string username, string password, string url)
        {
            this.Username = username;
            this.Password = password;
            this.Url = new Uri(url);
            string iv = Key.Substring(0, IV_LENGTH);
            this.Registry = Cryptonia.Encrypt(this.Username, this.Key, iv);
            SaveStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// set the user to register on the sip server by an full account
        /// and registers the user immedeatly
        /// </summary>
        /// <param name="account">the acccount with full information about auth and the sip
        /// server</param>
        public void SetUser(Account account)
        {
            this.IsRegisterd = true;
            this.Account = account;
            this.SipRegister();
            SaveStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// refreshes the registration state on the sip server
        /// with the account set by <code>SetUser(Account);</code>
        /// </summary>
        public void RefreshRegister()
        {
            if(null != this.Account)
            {
                this.IsRegisterd = true;
                this.SipRegister();
            }
        }

        /**
         * removes the user associated to the doorunit
         */
        public void RemoveUser()
        {
            this.IsRegisterd = false;
            this.Account = null;
            this.SipClear();
            SaveStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /**
         * Task to send a request and await the result from the server
         * @param notification to send
         * @return response from the server
         */
        public async Task<Dup.Response> SendAsync(Dup.Notification notification)
        {
            // wait until the task is ready
            return await Task.Run(() =>
            {
                Dup.Response result = new Dup.Response();
                string iv;
                try
                {
                    RestClient client = new RestClient(new Uri(Url, GATEWAY_PATH))
                    {
                        Timeout = REQ_TIMEOUT
                    };
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", REQ_CONTENT_TYPE);
                    request.AddHeader("Token", Registry);
                    iv = Cryptonia.RandomIV(IV_LENGTH);
                    string raw = JsonConvert.SerializeObject(notification, new JsonSerializerSettings
                    {
                        // ignor values wich are null, this is often/alway the case, because of the multiple interface implementaion of Dup.Data
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    string body = iv + Cryptonia.Encrypt(raw , Key, iv);
                    request.AddParameter(REQ_CONTENT_TYPE, body, ParameterType.RequestBody);
                    RestResponse response = (RestResponse)client.Execute(request);
                    result.StatusCode = (int)response.StatusCode;
                    result.Message = response.ErrorMessage;
                    isReachable = (result.StatusCode == 200); // only for 200 the doorunit is fully reachable, some parts could work at any response 
                    switch (result.StatusCode)
                    {
                        case 200:
                            string plain = response.Content[IV_LENGTH..];
                            iv = response.Content.Substring(0, IV_LENGTH);
                            result.Notification = JsonConvert.DeserializeObject<Dup.Notification>(Cryptonia.Decrypt(plain, Key, iv));
                            break;
                        case 404:
                            result.Message = "Der gesuchte Account konnte nicht gefunden werden.";
                            break;
                        case 503:
                            result.Message = "Der Server ist Temporär nicht Verfügbar";
                            break;
                        default:
                            result.Message = "Somethin went wrong, but there was no status code declared for this possibility: " + result.StatusCode;
                            break;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("An Error occured while send a dup notification to the ct-server; " + e.Message);
                }
                return result;
            });
        }




        /*
         * generate data payload for a device update
         * @return a fully useable device-update dup data payload 
         */
        public Dup.IDataDeviceUpdate GetDeviceUpdateDataPayload(string pushtoken)
        {
            Dup.IDataDeviceUpdate data = new Dup.Data();
            data.Devicename = Doorunit.Id;
#if __IOS__
            data.Os = "iOS";
            data.Server = "apple.com";
#else
            data.Os = "android";
            data.Server = "googleapis.com";
#endif
            if(null != this.Account?.Sip)
            {
                data.Username = this.Account.Sip.ToString();
            }
            else
            {
                data.Username = NONE;
            }
            data.Token = pushtoken;
            return data;
        }
        #endregion
    }
}
