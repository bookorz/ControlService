using System;

namespace ControlService.Digital_IO.Config
{
    public class ParamConfig
    {
        public string DeviceName { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string Parameter { get; set; }
        public string Status { get; set; }
        public string Normal { get; set; }
        public string Abnormal { get; set; }
        public string Error_Code { get; set; }
        public string hwid { get; set; }
        public DateTime LastErrorHappenTime { get; set; }
    }
}
