namespace ControlService.Digital_IO.Comm
{
    interface IConnection
    {
        bool Send(object Message);
        bool SendHexData(object Message);
        void Start();
        void WaitForData(bool Enable);
        void Reconnect();
    }
}
