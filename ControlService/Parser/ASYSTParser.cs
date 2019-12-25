using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Management;

namespace ControlService.Parser
{
    class ASYSTParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.LoadPortType.ReadStatus:
                    return ParseStatus(Message);
                default:
                    throw new Exception(Command + " Not support");
            }
        }

        private Dictionary<string, string> ParseStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            foreach (string each in MsgAry)
            {
                string[] Kv = each.Split('=');
                if (Kv[0].Equals("SLOTPOS"))
                {
                    Kv[1] = Convert.ToInt32(Kv[1]).ToString("00");
                }
                result.Add(Kv[0], Kv[1]);
                //switch (Kv[0])
                //{
                //    case "ELDN":
                //        result.Add("Elevator down limit", Kv[1]);
                //        break;
                //    case "ELPOS":
                //        result.Add("Current elevator position", Kv[1]);
                //        break;
                //    case "ELSTAGE":
                //        result.Add("Elevator stage position", Kv[1]);
                //        break;
                //    case "ELUP":
                //        result.Add("Elevator up limit", Kv[1]);
                //        break;
                //    case "HOME":
                //        result.Add("HOME", Kv[1]);
                //        break;
                //    case "MODE":
                //        result.Add("MODE", Kv[1]);
                //        break;
                //    case "PIO":
                //        result.Add("Parallel interlocks", Kv[1]);
                //        break;
                //    case "PIP":
                //        result.Add("Pod present", Kv[1]);
                //        break;
                //    case "PRTST":
                //        result.Add("Port locked", Kv[1]);
                //        break;
                //    case "READY":
                //        result.Add("READY", Kv[1]);
                //        break;
                //    case "SEATER":
                //        result.Add("SEATER", Kv[1]);
                //        break;
                //    case "SLOTPOS":
                //        result.Add("Current elevator slot position", Kv[1]);
                //        break;
                //}
            }
            return result;
        }
    }
}
