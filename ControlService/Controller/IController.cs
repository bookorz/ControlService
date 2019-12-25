using ControlService.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.CommandConvert;

namespace ControlService.Controller
{
    public interface IController
    {

        void SetReport(ICommandReport ReportTarget);
        string GetDeviceName();
        bool GetEnable();

        string GetDeviceType();
        string GetConnectionType();
        string GetControllerType();
        string GetIPAdress();
        int GetPort();
        string GetVendor();
        string GetPortName();
        int GetBaudRate();
        string GetStatus();
        void SetStatus(string Status);
        void Start(object state);
        bool DoWork(Transaction Txn,bool WaitForData = false);
        string GetNextSeq();
        void Reconnect();
        CommandEncoder GetEncoder();
        
    }
}
