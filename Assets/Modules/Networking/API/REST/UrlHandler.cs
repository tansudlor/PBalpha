using System;
using com.playbux.utilis.network;
using System.Collections.Specialized;

namespace com.playbux.network.api.rest
{
    public class UrlHandler
    {
        private readonly APISettings settings;

        public UrlHandler(APISettings settings)
        {
            this.settings = settings;
        }

        public string Handle(string segments, NameValueCollection query = null)
        {
            if (query == null)
                query = HttpUtility.ParseQueryString(string.Empty);
            
            string format = !string.IsNullOrEmpty(settings.apiVersion.Value) ? "/{0}/{1}" : "/{0}";
            string formattedSegments = !string.IsNullOrEmpty(settings.apiVersion.Value)? string.Format(format, settings.apiVersion.Value, segments) : string.Format(format, segments);
            var uriBuilder = new UriBuilder(settings.domain.Value);
            uriBuilder.Path = formattedSegments;
            uriBuilder.Query = query.ToString();
            
            if (settings.apiPort.Value != -1)
                uriBuilder.Port = settings.apiPort.Value;

            // FIXME: replacing "ws" with "http" is too hacky!
            uriBuilder.Scheme = uriBuilder.Scheme.Replace("ws", "http");

            return uriBuilder.ToString();
        }
    }
}