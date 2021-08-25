using System;
using System.Collections.Generic;
using System.Text;

namespace dieKlingel.Controls.Editor
{
    interface IBase
    {
        EditorType Type { get; }
        string Text { get; set; }
        virtual void ResizeRequest(int width, int heigth)
        {

        }

        event EventHandler<CustomCommandEventArgs> CustomCommand;
    }

    public class CustomCommandEventArgs : EventArgs
    {
        public string Command { get; set; }
    }

    public enum EditorType
    {
        Plain,
        Ini,
        Json,
        Cmd
    }
}
