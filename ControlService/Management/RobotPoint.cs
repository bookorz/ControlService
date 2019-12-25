using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Management
{
    public class RobotPoint
    {
        public string NodeName { get; set; }
        public string Position { get; set; }
        public string MappingPoint { get; set; }
        public string PreMappingPoint { get; set; }
        public string PositionType { get; set; }
        public string Point { get; set; }
        public int Offset { get; set; }
    }
}
