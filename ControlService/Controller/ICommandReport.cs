using ControlService.Management;
using ControlService.CommandConvert;

namespace ControlService.Controller
{
    public interface ICommandReport
    {
        void On_Command_Excuted(Node Node, Transaction Txn, CommandReturnMessage Msg);
        void On_Command_Error(Node Node, Transaction Txn, CommandReturnMessage Msg);
        void On_Command_Finished(Node Node, Transaction Txn, CommandReturnMessage Msg);
        void On_Command_TimeOut(Node Node, Transaction Txn);
        void On_Event_Trigger(Node Node, CommandReturnMessage Msg);
        void On_Node_State_Changed(Node Node, string Status);
        void On_Controller_State_Changed(string Device_ID, string Status);
        void On_Message_Log(string Type, string Message);
    }
}
