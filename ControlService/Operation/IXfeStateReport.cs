using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Management;

namespace ControlService.Operation
{
    public interface IXfeStateReport
    {
        void On_Transfer_Complete(XfeCrossZone xfe);
        void On_LoadPort_Selected(Node Port);
        void On_UnLoadPort_Selected(Node Port);
        void On_LoadPort_Complete(Node Port);
        void On_UnLoadPort_Complete(Node Port);
    }
}
