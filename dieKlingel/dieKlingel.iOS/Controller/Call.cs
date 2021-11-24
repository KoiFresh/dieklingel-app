using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using CallKit;

namespace Controller
{
    /// <summary>
    /// provides a api for ios to handle system calls with the ios callkit
    /// Autho
    /// </summary>
    class Call
    {
        #region private vars
        private CXCallController callController = new CXCallController();
        #endregion

        public Call()
        {

        }

        #region public methods

        /// <summary>
        /// tells the system call kit to start a call
        /// <para>number</para> sip number to call <code>sip:example@dieklingel.com:6050</code>
        /// </summary>
        public void StartCall(string number)
        {
            throw new NotImplementedException("Start Call is not Implemented on iOS");
        }

        /// <summary>
        /// ends the call if there is an active call
        /// </summary>
        public void EndCall()
        {
            throw new NotImplementedException("End Call is not Implemented on iOS");
        }

        /// <summary>
        /// pauses the call if there is an active call
        /// </summary>
        public void PauseCall()
        {
            throw new NotImplementedException("Pause Call is not Implemented on iOS");
        }

        /// <summary>
        /// resumes the call wich was paused before with <code>PauseCall();</code>
        /// </summary>

        public void ResumeCall()
        {
            throw new NotImplementedException("Resume Call is not Implemented on iOS");
        }

        #endregion
    }
}