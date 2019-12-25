using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Management;

namespace ControlService.Parser
{
    class KawasakiParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.RobotType.GetStatus:
                    return ParseStatus(Message);
                
                default:
                    throw new Exception(Command + " Not support");
            }
        }
        private Dictionary<string, string> ParseStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] parameters = Message.Split(' ');
            for (int i = 0; i < parameters.Length; i++)
            {

                switch (i + 1)
                {
                    case 4:
                        result.Add("L_Hold_Status", parameters[i].ToString());
                        break;
                    case 7:
                        result.Add("R_Hold_Status", parameters[i].ToString());
                        break;
                }
            }
            return result;
        }
    }
}
