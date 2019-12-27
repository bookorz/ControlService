using log4net;
using ControlService.Config;
using ControlService.Controller;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlService.Comm
{
    class ComPortClient : IConnection
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ComPortClient));
        private SerialPort port;
        IConnectionReport ConnReport;
        IController cfg;
        public bool _WaitForData = false;

        public ComPortClient(IController _Config, IConnectionReport _ConnReport)
        {
            cfg = _Config;
            ConnReport = _ConnReport;
       

            port = new SerialPort(_Config.GetPortName(), _Config.GetBaudRate(), Parity.None, 8, StopBits.One);
            if (_Config.GetVendor().Equals("SMARTTAG"))
            {
                port.Handshake = Handshake.None;
                port.RtsEnable = true;
                port.ReadTimeout = 5000;
                port.WriteTimeout = 5000;
            }
        }

        public void WaitForData(bool Enable)
        {
            _WaitForData = Enable;
        }


        public void Reconnect()
        {
            Start();
        }

        public void Start()
        {
            if (!port.IsOpen)
            {
                Thread ComTd = new Thread(ConnectServer);
                ComTd.IsBackground = true;
                ComTd.Start();
            }
        }

        public void Close()
        {
            try
            {
                port.Close();
                port = new SerialPort(cfg.GetPortName(), cfg.GetBaudRate(), Parity.None, 8, StopBits.One);
            }
            catch (Exception e)
            {
                ConnReport.On_Connection_Error("(Close )" + e.Message + "\n" + e.StackTrace);
            }
        }

        public bool Send(object Message)
        {
            try
            {
                if (cfg.GetVendor().ToUpper().Equals("ACDT"))
                {
                    string hexString = Message.ToString().Replace("-", "");
                    byte[] byteOUT = new byte[hexString.Length / 2];
                    for (int i = 0; i < hexString.Length; i = i + 2)
                    {
                        //每2位16進位數字轉換為一個10進位整數
                        byteOUT[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                    }
                    port.Write(byteOUT, 0, byteOUT.Length);
                }
                else
                {
                    port.Write(Message.ToString());
                }
                return true;
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        public bool SendHexData(object Message)
        {
            try
            {
                byte[] buf = HexStringToByteArray(Message.ToString());

                port.Write(buf, 0, buf.Length);
                return true;
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        private void ConnectServer()
        {
            try
            {
                ConnReport.On_Connection_Connecting("Connecting to ");
                port.Open();
                ConnReport.On_Connection_Connected("Connected! ");
                switch (cfg.GetVendor().ToUpper())
                {
                    case "ACDT":
                        port.DataReceived += new SerialDataReceivedEventHandler(ACDT_DataReceived);
                        break;
                    case "TDK":

                        port.DataReceived += new SerialDataReceivedEventHandler(TDK_DataReceived);
                        break;
                    case "ATEL_NEW":
                    case "SANWA":
                        port.DataReceived += new SerialDataReceivedEventHandler(Sanwa_DataReceived);
                        break;
                    case "ASYST":
                        port.DataReceived += new SerialDataReceivedEventHandler(ASYST_DataReceived);
                        break;
                    case "SMARTTAG":
                        port.DataReceived += new SerialDataReceivedEventHandler(SMARTTAG_DataReceived);
                        break;
                }
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                SpinWait.SpinUntil(() => false, 3000);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
            }
        }

        private void ASYST_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";

                data = port.ReadTo("\r\n");


                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ASYST_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        private void SMARTTAG_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SpinWait.SpinUntil(() => !_WaitForData, 1000);

                byte[] buf = new byte[250];
                port.Read(buf, 0, buf.Length);
                string data = ByteArrayToString(buf);
                ConnReport.On_Connection_Message(data.Trim());

            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ASYST_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        private void TDK_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";
                //switch (cfg.DeviceType)
                //{
                //    case "TDKController":
                //data = port.ReadTo("\r");

                //        break;
                //    case "SanwaController":
                data = port.ReadTo(((char)3).ToString());
                //        break;
                //}

                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(TDK_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        int currentIdx = 0;
        byte[] readB = new byte[100];
        private void ACDT_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";
                //switch (cfg.DeviceType)
                //{
                //    case "TDKController":
                //data = port.ReadTo("\r");

                //        break;
                //    case "SanwaController":
                //data = port.ReadTo(((char)3).ToString());
                while (((readB[currentIdx] = Convert.ToByte(port.ReadByte())) != 3) || (readB[1] == 105 && currentIdx < 7))
                {

                    currentIdx++;
                }
                data = BitConverter.ToString(readB.Take(currentIdx + 1).ToArray());
                //        break;
                //}
                currentIdx = 0;
                readB = new byte[100];
                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(TDK_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        private void Sanwa_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";

                data = port.ReadTo("\r");


                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(Sanwa_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);


            foreach (byte b in ba)
            {
                if (b == 0)
                {
                    //continue;
                }
                hex.AppendFormat("{0:X2}", b);
                hex.Append(" ");
            }
            return hex.ToString();
        }


        private byte[] HexStringToByteArray(string s)
        {

            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public void Dispose()
        {

        }
    }
}
