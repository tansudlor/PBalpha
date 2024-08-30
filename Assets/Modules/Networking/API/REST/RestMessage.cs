using System;
using System.Collections.Generic;

namespace com.playbux.network
{
    public class RestMessage<T>
    {
        public T data;
        public RestErrorMessage errorMessage;

        public RestMessage(T data, RestErrorMessage errorMessage = null)
        {
            this.data = data;
            this.errorMessage = errorMessage;
        }
    }

    [Serializable]
    public class Empty
    {
        
    }
}