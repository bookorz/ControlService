using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.CommandConvert
{
    public class Encoder_PTZ
    {
        private string Supplier;
        public Encoder_PTZ(string supplier)
        {
            try
            {
                Supplier = supplier;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        private string EndCode()
        {
            string result = "";
            switch (Supplier)
            {
                case "SANWA_MC":
                case "ATEL":
                case "ATEL_NEW":
                    result = "\r";
                    break;

                case "KAWASAKI":
                    result = "\r\n";
                    break;
            }
            return result;
        }
        public string Rotate(string Address, string Direction)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:PTRAT:{0},{1}";
                    commandStr = string.Format(commandStr, Address, Direction);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Home(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:PTHME:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetPath(string Address, string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:PTMOD:{0},{1}";
                    commandStr = string.Format(commandStr, Address, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Transfer(string Address, string Position, string Mode, string Direction)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:PTTSF:{0},{1},{2},{3}";
                    commandStr = string.Format(commandStr, Address, Position, Direction, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Reset(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3SET:RESET";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetSpeed(string Address, string Value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":
                    if (Value.Equals("0"))
                    {
                        Value = "1";
                    }
                    else if (Value.Equals("100"))
                    {
                        Value = "0";
                    }
                    commandStr = "$3SET:SP___:{0},{1}";
                    commandStr = string.Format(commandStr, Value, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
