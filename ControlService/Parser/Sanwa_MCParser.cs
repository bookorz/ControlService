using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Config;
using ControlService.Management;

namespace ControlService.Parser
{
    class Sanwa_MCParser : IParser
    {

        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.ILPT.Load:
                case Transaction.Command.PTZ.Transfer:
                case Transaction.Command.PTZ.Home:
                    return ParseMap(Message);
                case Transaction.Command.Shelf.GetFOUPPresence:
                    return ParsePresent(Message);
                case Transaction.Command.WHR.Get_DIO:
                    return ParseIO(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }

        private Dictionary<string, string> ParseIO(string Message)
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
           
            string[] valAry= Message.Split(',');
            int Count = Convert.ToInt32(valAry[2]);
            int idx = 3;
            int Type = Convert.ToInt32(valAry[1]);
            int BoardId = Convert.ToInt32(valAry[0]);
            for (int x = 0; x < Count; x++)
            {

                string hexStr = valAry[idx];
                string binary = String.Join(String.Empty, hexStr.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
                int IO_Idx = 1;
                for (int i = binary.Length-1; i > -1; i--)
                {
                    result.Add(BoardId.ToString()+"_"+ Type.ToString()+"_"+ IO_Idx.ToString(), binary[i].ToString());

                    IO_Idx++;
                }

                
                idx++;
                BoardId++;
            }

            
            return result;
        }
        private Dictionary<string, string> ParseMap(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Mapping", Message.Replace(",", ""));
            return result;
        }
        private Dictionary<string, string> ParsePresent(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] tmp = Message.Split(',');
            for (int i = 0; i < tmp.Length; i++)
            {
                int idx = i + 1;
                switch (idx)
                {
                    case 1:
                        result.Add("FOUP_ROBOT", tmp[i]);
                        break;
                    case 2:
                        result.Add("ELPT1", tmp[i]);
                        break;
                    case 3:
                        result.Add("ELPT2", tmp[i]);
                        break;
                    case 4:
                        result.Add("ILPT1", tmp[i]);
                        break;
                    case 5:
                        result.Add("ILPT2", tmp[i]);
                        break;
                    default:
                        result.Add("SHELF" + (idx - 5).ToString(), tmp[i]);
                        break;
                }
            }



            return result;
        }
    }
}
