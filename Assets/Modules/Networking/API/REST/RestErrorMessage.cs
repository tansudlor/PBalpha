using System;

namespace com.playbux.network
{
    [Serializable]
    public class RestErrorMessage
    {
        public int statusCode;
        public string error;
        public string message;

        public string ToString()
        {
            return $"Status Code:{statusCode}\nError:{error}\nMessage:{message}";
        }
        public RestErrorMessage(){}
        public RestErrorMessage(RestErrorMessage restErrorMessage)
        {
            statusCode = restErrorMessage.statusCode;
            error = restErrorMessage.error;
            message = restErrorMessage.message;
        }
    }
}