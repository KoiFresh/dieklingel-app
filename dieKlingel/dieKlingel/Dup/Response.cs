using System;
using System.Collections.Generic;
using System.Text;

namespace dieKlingel
{
    namespace Dup
    {
        public class Response
        {
            #region public variables
            public int StatusCode { get; set; } = -1;
            public bool Ok 
            { 
                get 
                {
                    return (this.StatusCode == 200);
                } 
            }
            public string Message { get; set; }
            public Notification Notification { get; set; }
            #endregion
            
            public Response()
            {

            }
        }
    }
}
