using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCamsControl.Model
{
    public class DialogHostAsyncResult : IDialogHostAsyncResult
    {
        private object thisLock = new object();
        private object _State;
        private bool _IsCompleted;
        private EventWaitHandle _Event = new EventWaitHandle(false, EventResetMode.AutoReset);
        private bool _DialogResult;
        private object _Content;
        private object _Payload;
        public DialogHostAsyncResult()
        {

        }
        #region IDialogHostAsyncResult
        public void SignalWaitHandle()
        {
            if (null != _Event)
                _Event.Set();
        }
        public object AsyncState
        {
            get
            {
                return _State;
            }
            set
            {
                _State = value;
            }
        }
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_IsCompleted && null != _Event)
                {
                    _Event.Set();
                }
                return _Event;
            }
        }
        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }
        public bool IsCompleted
        {
            get
            {
                return _IsCompleted;
            }
            set
            {
                _IsCompleted = value;
            }
        }
        public bool DialogResult
        {
            get
            {
                return _DialogResult;
            }
            set
            {
                _DialogResult = value;
            }
        }
        public object Payload
        {
            get
            {
                return _Payload;
            }
            set
            {
                _Payload = value;
            }
        }
        #endregion
    }
}
