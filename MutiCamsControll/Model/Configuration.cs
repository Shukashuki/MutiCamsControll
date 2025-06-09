using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MultiCamsControl.Model
{
    public class StationName
    {
        public string CameraName { get; set; }
        public string Station { get; set; }
        public string ItemNameType { get; set; }
        public string TestType { get; set; }
        public string FacilityID { get; set; }
    }

    public class AIplatform
    {
        public string Port { get; set; }
    }

    public class AIModel
    {
        public string Model1 { get; set; }
    }

    public class Configuration
    {
        public StationName StationName { get; set; }
        public AIplatform AIplatform { get; set; }
        public AIModel AIModel { get; set; }
    }
}
