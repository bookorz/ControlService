using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace ControlService.CommandConvert
{
    public class EncoderOCR
    {
        private string Supplier;
       

        /// <summary>
        /// OCR Encoder
        /// </summary>
        /// <param name="supplier"> 設備供應商 </param>
        public EncoderOCR(string supplier)
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
                case "HST":
                    result = "";
                    break;
                case "COGNEX":
                    result = "\r\n";
                    break;
            }
            return result;
        }
        public enum OnlineStatus
        {
            Offline,
            Online
        }

        public string Read()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "COGNEX":
                case "HST":
                    commandStr = string.Format("{0}{1}{2}{3}{4}", "SM", ((char)34).ToString(), "READ", ((char)34).ToString(), "0");
                    break;
                default:
                    throw new NotSupportedException();
            }
            return commandStr + EndCode();
        }

        public string SetOnline(OnlineStatus online)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "COGNEX":
                case "HST":
                    commandStr = string.Format("SO{0}", ((int)online).ToString());
                    break;
                default:
                    throw new NotSupportedException();
            }
            return commandStr + EndCode();
        }

        public string GetOnline()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "COGNEX":
                case "HST":
                    commandStr = "GO";
                    break;
                default:
                    throw new NotSupportedException();
            }
            return commandStr + EndCode();
        }
        public string SetConfigEnable(string val)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "COGNEX":
                case "HST":
                    commandStr = "Ev SetConfigEnable(A4," + val + ")";
                    break;
                default:
                    throw new NotSupportedException();
            }
            return commandStr + EndCode();
        }


    }
}
