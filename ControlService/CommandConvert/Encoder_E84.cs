using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.CommandConvert
{
    public class Encoder_E84
    {
        private string Supplier;
        public Encoder_E84(string supplier)
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
                case "SANWA":
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
        public string Reset()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,550400bb";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string AutoMode()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,550100bb";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string ManualMode()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,550200bb";
                    commandStr = string.Format(commandStr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP1(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5570{0}bb";
                    commandStr = string.Format(commandStr,Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP2(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5571{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP3(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5572{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP4(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5573{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP5(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5574{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP6(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5575{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
