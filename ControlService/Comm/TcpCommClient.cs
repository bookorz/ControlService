using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ControlService.Controller;

namespace ControlService.Comm
{
    class TcpCommClient : IConnection
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TcpCommClient));
        IConnectionReport ConnReport;
        IController Config;

        //先建立一個TcpClient;
        TcpClient tcpClient = new TcpClient();

        public TcpCommClient(IController _Config, IConnectionReport _ConnReport)
        {
            Config = _Config;

            ConnReport = _ConnReport;
        }
        public bool Send(object Message)
        {
            try
            {
                NetworkStream ns = tcpClient.GetStream();
                if (ns.CanWrite)
                {
                    byte[] msgByte = Encoding.Default.GetBytes(Message.ToString());
                    ns.Write(msgByte, 0, msgByte.Length);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                return false;
            }
            return true;
        }

        public bool SendHexData(object Message)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            try
            {
                tcpClient.Client.Shutdown(SocketShutdown.Both);
                tcpClient.Client.Disconnect(false);
            }
            catch
            {

            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnectServer));
        }

        private void ConnectServer(object input)
        {
            ConnReport.On_Connection_Connecting("Connecting");
            //先建立IPAddress物件,IP為欲連線主機之IP
            IPAddress ipa = IPAddress.Parse(Config.GetIPAdress());

            //建立IPEndPoint
            IPEndPoint ipe = new IPEndPoint(ipa, Config.GetPort());
            
            //開始連線
            try
            { 
                //tcpClient.ReceiveTimeout = 1000;
                tcpClient.Connect(ipe);
               

                //bool success = false;
                //while (!success)
                //{
                //    var result = tcpClient.BeginConnect("remotehost", Config.Port, null, null);
                //    success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                //    if (!success)
                //    {
                //        logger.Error(Config.DeviceName+" ip="+Config.IPAdress+":"+Config.Port.ToString()+" connect timeout!");
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}

                if (tcpClient.Connected)
                {
                    ConnReport.On_Connection_Connected("Connected");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Receive));
                }
                else
                {
                    ConnReport.On_Connection_Error("Error");
                }
        
            }
            catch (Exception e)
            {
                tcpClient.Close();
                logger.Error(e.StackTrace);
            }

        }

        private void Receive(object input)
        {
            string receiveMsg = string.Empty;
            
           
            int numberOfBytesRead = 0;
            NetworkStream ns = tcpClient.GetStream();

            while (tcpClient.Connected)
            {

                if (ns.CanRead)
                {
                    do
                    {
                        
                        byte[] receiveBytes = new byte[tcpClient.ReceiveBufferSize];
                        numberOfBytesRead = ns.Read(receiveBytes, 0, tcpClient.ReceiveBufferSize);

                        byte[] bytesRead = new byte[numberOfBytesRead];
                        Array.Copy(receiveBytes, bytesRead, numberOfBytesRead);
                        //receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                        socketDataArrivalHandler(bytesRead);
                    }
                    while (ns.DataAvailable);
                }

               
            }
        }

        string S = "";

        private void socketDataArrivalHandler(byte[] OrgData)
        {

            string data = "";
            switch (Config.GetVendor().ToUpper())
            {
                case "TDK":


                    S += Encoding.Default.GetString(OrgData, 0, OrgData.Length);
                    if (S.IndexOf(Convert.ToChar(3)) != -1)
                    {
                        //logger.Debug("s:" + S);
                        data = S.Substring(0, S.IndexOf(Convert.ToChar(3)) + 1);
                        //logger.Debug("data:" + data);

                        S = S.Substring(S.IndexOf(Convert.ToChar(3)) + 1);
                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        break;
                    }


                    break;
                case "SANWA":

                    S += Encoding.Default.GetString(OrgData, 0, OrgData.Length);

                    if (S.IndexOf("\r") != -1)
                    {
                        //logger.Debug("s:" + S);
                        data = S.Substring(0, S.IndexOf("\r"));
                        //logger.Debug("data:" + data);

                        S = S.Substring(S.IndexOf("\r") + 1);
                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        break;
                    }


                    break;
                case "HST":

                    S += Encoding.Default.GetString(OrgData, 0, OrgData.Length);
                    if (S.IndexOf("1\r\n") != -1)
                    {
                        data = S.Substring(S.IndexOf("1\r\n"), 3);
                        //logger.Debug("data:" + data);
                        S = S.Substring(S.IndexOf("1\r\n") + 3);

                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        //break;

                    }

                    if (S.IndexOf("-2\r\n") != -1)
                    {
                        data = S.Substring(S.IndexOf("-2\r\n"), 4);
                        //logger.Debug("data:" + data);
                        S = S.Substring(S.IndexOf("-2\r\n") + 4);

                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        //break;

                    }
                    if (S.IndexOf("]\r\n") != -1)
                    {
                        data = S.Substring(0, S.IndexOf("]\r\n") + 3).Substring(S.IndexOf("["));
                        //logger.Debug("data:" + data);

                        S = S.Substring(S.IndexOf("]\r\n") + 3);
                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        //break;

                    }
                    else if (S.IndexOf("</Result>\r\n") != -1)
                    {
                        //logger.Debug("s:" + S);
                        data = S.Substring(0, S.IndexOf("</Result>\r\n") + 11);
                        //logger.Debug("data:" + data);

                        S = S.Substring(S.IndexOf("</Result>\r\n") + 11);
                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        break;
                    }
                    if (S.IndexOf("Welcome to e-Reader8000") != -1 || S.IndexOf("User:") != -1)
                    {
                        S = "";
                        //logger.Debug("s:" + S);

                        //break;

                    }
                    if (S.IndexOf("0\r\n") != -1)
                    {
                        data = S.Substring(0, S.IndexOf("0\r\n"));
                        //logger.Debug("data:" + data);
                        S = S.Substring(S.IndexOf("0\r\n") + 3);

                        //logger.Debug("s:" + S);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                        //break;

                    }

                    break;
                default:
                    data = Encoding.Default.GetString(OrgData, 0, OrgData.Length);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);

                    break;
            }
        }
        public void WaitForData(bool Enable)
        {
            //throw new NotImplementedException();
        }

        public void Reconnect()
        {
            throw new NotImplementedException();
        }
    }
}
