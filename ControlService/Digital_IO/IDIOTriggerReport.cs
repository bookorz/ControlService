using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Digital_IO
{
    public interface IDIOTriggerReport
    {
        void On_Data_Chnaged(string Parameter, string Value,string Type);
        void On_Connection_Error(string DIOName, string ErrorMsg);
        void On_Connection_Status_Report(string DIOName, string Status);
        void On_DIO_Alarm_Happen(string DIOName, string ErrorCode);
    }
}
