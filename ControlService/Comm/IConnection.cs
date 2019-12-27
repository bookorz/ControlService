using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Comm
{
    interface IConnection
    {
        bool Send(object Message);
        bool SendHexData(object Message);
        void Start();
        void Close();
        void WaitForData(bool Enable);
        void Reconnect();
    }
}
