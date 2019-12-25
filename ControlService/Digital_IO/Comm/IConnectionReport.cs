namespace ControlService.Digital_IO.Comm
{
    public interface IConnectionReport
    {
        void On_Connection_Message(object Msg);
        void On_Connection_Connecting(string Msg);
        void On_Connection_Connected(object Msg);
        void On_Connection_Disconnected(string Msg);
        void On_Connection_Error(string Msg);
    }
}