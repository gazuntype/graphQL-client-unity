using System;

namespace GraphQlClient.EventCallbacks
{
    #region Network

    public class OnRequestBegin : Event<OnRequestBegin>
    {
        public OnRequestBegin(){
            
        }
    }

    public class OnRequestEnded : Event<OnRequestEnded>
    {
        public string data;
        public bool success;
        public Exception exception;
        public OnRequestEnded(string data){
            this.data = data;
            success = true;
        }

        public OnRequestEnded(Exception exception){
            this.exception = exception;
            success = false;
        }
    }

    #endregion
}
