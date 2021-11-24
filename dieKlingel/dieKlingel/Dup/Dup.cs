using System;
using System.Collections.Generic;
using System.Text;

namespace dieKlingel
{
    class BDup
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
    }
}
