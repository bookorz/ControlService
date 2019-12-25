using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Management;

namespace ControlService.Parser
{
    class SanwaParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.RobotType.GetRIO:
                    return ParseRIO(Message);
                case Transaction.Command.RobotType.GetPosition:
                    return ParsePosition(Message);
                case Transaction.Command.RobotType.GetStatus:
                    return ParseStatus(Message);
                case Transaction.Command.RobotType.GetSV:
                    return ParseSV(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }
        private Dictionary<string, string> ParseSV(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            switch (MsgAry[0])
            {
                case "01":
                    result.Add("R_Vacuum_Solenoid", MsgAry[1]);
                    break;
                case "02":
                    result.Add("L_Vacuum_Solenoid", MsgAry[1]);
                    break;

            }
            return result;
        }
        private Dictionary<string, string> ParseRIO(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            switch (MsgAry[0])
            {
                case "004":
                    result.Add("R_Hold_Status", MsgAry[1]);                   
                    break;
                case "005":
                    result.Add("L_Hold_Status", MsgAry[1]);
                    break;
                case "008":
                    result.Add("R_Present", MsgAry[1]);
                    break;
                case "009":
                    result.Add("L_Present", MsgAry[1]);
                    break;
            }
            return result;
        }

        private Dictionary<string, string> ParsePosition(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            for (int i = 0; i < MsgAry.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        result.Add("R_Position", MsgAry[i]);
                        break;
                    case 1:
                        result.Add("L_Position", MsgAry[i]);
                        break;
                    case 4:
                        result.Add("X_Position", MsgAry[i]);
                        break;

                }
            }
            return result;
        }

        private Dictionary<string, string> ParseStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 0; i < Message.Length; i++)
            {

                switch (i + 1)
                {
                    case 10:
                        result.Add("Servo", Message[i].ToString());
                        break;
                }
            }
            return result;
        }
    }
}
