using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dieKlingel
{
    public class Account
    {
        #region public variables
        [JsonProperty]
        public string Displayname { get; set; }
        [JsonProperty]
        public string Password { get; set;}
        [JsonProperty]
        public string Number { get; set; }
        [JsonProperty]
        public SipConfig Sip { get; set; }
        #endregion
    }

    public class SipConfig
    {
        #region public variables
        [JsonProperty]
        public string Username { get; set; }
        [JsonProperty]
        public string Password { get; set; }
        [JsonProperty]
        public string Domain { get; set; }
        [JsonProperty]
        public int Port { get; set; }
        #endregion

        #region public methods
        public override string ToString()
        {
            return Username + "@" + Domain + ":" + Port.ToString();
        }
        #endregion
    }
}
