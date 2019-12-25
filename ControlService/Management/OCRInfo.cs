using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Management
{
    public class OCRInfo
    {
        public string Passed = "";
        public string ResultID = "";
        public string Result = "";
        public string Score = "";

        public OCRInfo(string Msg)
        {
            if (Msg.IndexOf("Result Id=\"") != -1)
            {
                this.ResultID = Msg.Substring(Msg.IndexOf("Result Id=\"") + 11, 1);
            }
            if (Msg.IndexOf("<String>") != -1)
            {
                this.Result = Msg.Substring(Msg.IndexOf("<String>") + 8, Msg.IndexOf("</String>") - (Msg.IndexOf("<String>")+8));
            }
            if (Msg.IndexOf("<Score>") != -1)
            {
                this.Score = Msg.Substring(Msg.IndexOf("<Score>") + 7, Msg.IndexOf("</Score>") - (Msg.IndexOf("<Score>") + 7));
            }
            if (Msg.IndexOf("<Passed>") != -1)
            {
                this.Passed = Msg.Substring(Msg.IndexOf("<Passed>") + 8, Msg.IndexOf("</Passed>") - (Msg.IndexOf("<Passed>") + 8));
            }
        }
    }
}
