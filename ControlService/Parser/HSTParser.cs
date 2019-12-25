using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Management;

namespace ControlService.Parser
{
    class HSTParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.OCRType.Read:
                case Transaction.Command.OCRType.ReadM12:
                case Transaction.Command.OCRType.ReadT7:
                    return ParseRead(Message);
                default:
                    throw new Exception(Command + " Not support");
            }
        }

        private Dictionary<string, string> ParseRead(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] OCRResult;

            OCRResult = Message.Replace("[", "").Replace("]", "").Split(',');
            result.Add("WAFER_ID", OCRResult[0]);
            result.Add("SCORE", OCRResult[1]);
            result.Add("PASS", Convert.ToInt32(Convert.ToDouble(OCRResult[2])).ToString());
            return result;
        }
    }
}
