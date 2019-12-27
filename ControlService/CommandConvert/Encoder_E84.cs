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
        public string Reset(string AddrNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},550400bb";
                    commandStr = string.Format(commandStr,AddrNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string AutoMode(string AddrNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},550100bb";
                    commandStr = string.Format(commandStr, AddrNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string ManualMode(string AddrNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},550200bb";
                    commandStr = string.Format(commandStr, AddrNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP1(string AddrNo, string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},5570{1}bb";
                    commandStr = string.Format(commandStr, AddrNo, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP2(string AddrNo,string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},5571{1}bb";
                    commandStr = string.Format(commandStr, AddrNo, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP3(string AddrNo,string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},5572{1}bb";
                    commandStr = string.Format(commandStr, AddrNo, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP4(string AddrNo,string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},5573{1}bb";
                    commandStr = string.Format(commandStr, AddrNo, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP5(string AddrNo,string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},5574{1}bb";
                    commandStr = string.Format(commandStr, AddrNo, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP6(string AddrNo,string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:{0},5575{1}bb";
                    commandStr = string.Format(commandStr, AddrNo, Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
