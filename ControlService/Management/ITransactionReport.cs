using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Management
{
    public interface ITransactionReport
    {
        void On_Transaction_TimeOut(Transaction Txn);
        void On_Transaction_BypassTimeOut(Transaction Txn);
    }
}
