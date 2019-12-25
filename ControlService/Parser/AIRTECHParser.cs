using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Management;

namespace ControlService.Parser
{
    class AIRTECHParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.FFUType.GetStatus:
                    return ParseStatus(Message);
                default:
                    throw new Exception(Command + " Not support");
            }
        }
        private Dictionary<string, string> ParseStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] MsgAry = Message.Split(',');
            for (int i = 0;i< MsgAry.Length;i++)
            {
                switch (i)
                {
                    case 4:
                        string status = Convert.ToString(Convert.ToUInt16(MsgAry[i]), 2).PadLeft(16, '0');
                        result.Add("Alarm", status.Substring(2, 1));
                        break;
                    case 6:
                        result.Add("Target_RPM",MsgAry[i]);
                        break;
                }
            }
            return result;
        }
    }
}
