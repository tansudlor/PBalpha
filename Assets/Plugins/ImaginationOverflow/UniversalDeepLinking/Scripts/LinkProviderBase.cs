using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking
{
    public abstract class LinkProviderBase : ILinkProvider
    {
        protected bool IsConnectedToUnity { get;  }
        protected LinkProviderBase()
        {
            //use unity implementation as fallback
            try
            {
                Delegate handler = new Action<string>(OnLinkReceived);
                typeof(Application).GetEvent("deepLinkActivated").AddEventHandler(null, handler);

                if (string.IsNullOrEmpty(Application.absoluteURL) == false)
                    OnLinkReceived(Application.absoluteURL);

                IsConnectedToUnity = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error Setting handler");
                Debug.LogError(e);
            }
        }


        public abstract bool Initialize();
        public abstract event Action<string> LinkReceived;
        public abstract void PollInfoAfterPause();

        protected abstract void OnLinkReceived(string s);
    }
}