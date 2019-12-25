namespace ControlService.Digital_IO.Controller
{
    interface IDIOController
    {
        void SetOut(string Address,string Value);
        void SetOutWithoutUpdate(string Address, string Value);
        void UpdateOut();
        string GetIn(string Address);
        string GetOut(string Address);
        void Connect();
        void Close();
    }
}
