using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.CommandConvert
{
    public class Encoder_CTU
    {
        private string Supplier;
        public Encoder_CTU(string supplier)
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
        public string Pick(string Address, string StationNo, string Mode,string Act)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:CTGET:{0},{1},{2},{3}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode, Act);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Place(string Address, string StationNo, string Mode, string Act)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:CTPUT:{0},{1},{2},{3}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode, Act);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Hold(string Address, string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:CTHLD:{0},{1}";
                    commandStr = string.Format(commandStr, Address, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Release(string Address, string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:CTRLS:{0},{1}";
                    commandStr = string.Format(commandStr, Address, Mode);
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

                    commandStr = "$3MCR:CTHOM:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string OrgSearch(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3MCR:CTINI:{0}";
                    commandStr = string.Format(commandStr, Address);
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
        public string Pause()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3SET:PAUSE";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Continue()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3SET:CONT_";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Stop()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3SET:STOP_:0";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Initial_IO()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3SET:TGEVT";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Get_DIO(string Type)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3GET:BRDIO:1,{0},64";
                    commandStr = string.Format(commandStr, Type);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Set_DIO(string Name, string Value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$3SET:NMEIO:{0},{1}";
                    commandStr = string.Format(commandStr, Name, Value);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
