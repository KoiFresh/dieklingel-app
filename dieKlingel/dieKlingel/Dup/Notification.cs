using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace dieKlingel
{
    namespace Dup
    {
        /**
         * Dup Notification
         */
        public class Notification
        {
            public Header Header { get; set; } = new Header();
            public Body Body { get; set; } = new Body();

            /*public static Notification Build(Context context, JObject data = null)
            {
                Notification notification = new Notification();
                notification.Body = new Body()
                {
                    Context = ContextConverter.ContextAsString(context),
                    Data = data ?? new JObject
  
                };
                return notification;
            }*/

            public static Notification Build(Context context, Data data)
            {
                Notification notification = new Notification();
                notification.Body = new Body()
                {
                    Context = ContextConverter.ContextAsString(context),
                    Data = data
                };
                return notification;
            }
        }

        /**
         * Header of a Dup Notification
         */
        public class Header
        {
            public string Version { get; set; } = "1.0";
        }

        /**
         * Body of a Dup Notification
         */
        public class Body
        {
            public string Context { get; set; } = ContextConverter.ContextAsString(Dup.Context.Unknown);
            public Data Data { get; set; }
         
        }

        public class Data : IDataUsers, IDataResponse, IDataDeviceUpdate
        {
            public List<Account> Users { get; set; }
            public string Devicename { get; set; }
            public string Os { get; set; }
            public string Server { get; set; }
            public string Token { get; set; }
            public string Username { get; set; }
        }

        public interface IDataUsers
        {
            public List<Account> Users { get; set; }
        }

        public interface IDataDeviceUpdate
        {
            public string Devicename { get; set; }
            public string Os { get; set; }
            public string Server { get; set; }
            public string Token { get; set; }
            public string Username { get; set; }
        }

        public interface IDataResponse
        {
            // This could be used, when no data is neccessar
        }

    }
}
