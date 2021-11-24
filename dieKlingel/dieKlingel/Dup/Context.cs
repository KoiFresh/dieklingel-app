using System;
using System.Collections.Generic;
using System.Text;

namespace dieKlingel
{
    namespace Dup
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
            SecureUnlock,
            EnterPasscode,
            Call,
            Register,
            Unknown
        }

        class ContextConverter
        {
            private ContextConverter() { }

            #region public methods
            /**
             * Converts a Context to a readable string, for use in Dup V1, V2 should migrate to numbers
             * @param context to convert
             * @return string to the context
             */
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
                    case Context.SecureUnlock: return "secure-unlock";
                    case Context.EnterPasscode: return "enter-passcode";
                    case Context.Call: return "call";
                    case Context.Register: return "register";
                    default: return "unknown";
                }
            }

            /**
             * Coverts a string into a Context 
             * @param string from readable context
             * @return context associated to the string 
             */
            public static Context StringAsContext(String context)
            {
                Context result = Context.Unknown;
                for (int i = (int)Context.Response; i != (int)Context.Unknown; i++)
                {
                    Context ctx = (Context)i;
                    if (ContextAsString(ctx) == context)
                    {
                        result = ctx;
                    }
                }
                return result;
            }
            #endregion
        }
    }
}
