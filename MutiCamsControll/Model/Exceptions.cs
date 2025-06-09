using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCamsControl.Model
{
    public class CameraNotStartedException : Exception
    {
        public CameraNotStartedException()
            : base("Camera has not been started.") { }

    }

    public class CameraStartAfterStarted : Exception
    {
        public CameraStartAfterStarted()
            : base("Camera has been started!!!.") { }

    }

    public class CameraStopBeforeStarted : Exception
    {
        public CameraStopBeforeStarted():base("Camera hasn't opened") { }
    }



}
