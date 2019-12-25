using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace ControlService.Comm
{
    public class FTP
    {
        private string FTPAddress;
        private string RemotePath;
        private string UserID;
        private string UserPW;

        /// <summary>
        /// FTP
        /// </summary>
        /// <param name="ipAddress"> FTP IP </param>
        /// <param name="port"> FTP Port </param>
        /// <param name="remotePath"> FTP remote Path </param>
        /// <param name="fileName"> FTP get file </param>
        /// <param name="account"> FTP login user </param>
        /// <param name="password"> FTP login user password </param>
        /// <param name="savePath"> save path </param>
        public FTP(string ipAddress, string port, string remotePath, string account, string password)
        {
            FTPAddress = string.Format(@"ftp://{0}:{1}/", ipAddress, port);
            RemotePath = remotePath;
            UserID = account;
            UserPW = password;
        }

        /// <summary>
        /// Get ftp file
        /// </summary>
        /// <returns></returns>
        public string Get(string remotefileName, string savefileName, string savePath)
        {
            string ftpFilePath = string.Empty;
            string StoragePath = string.Empty;
            FtpWebRequest ftpRequest = null;
            NetworkCredential ftpCredential = null;
            FtpWebResponse ftpResponse = null;
            Stream ftpStream = null;

            try
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                ftpFilePath = FTPAddress + RemotePath + remotefileName;
                StoragePath = savePath + "/" + savefileName;
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFilePath);
                ftpCredential = new NetworkCredential(UserID, UserPW);
                ftpRequest.Credentials = ftpCredential;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();

                using (FileStream fileStream = new FileStream(StoragePath, FileMode.Create))
                {
                    int bufferSize = 2048;
                    int readCount;
                    byte[] buffer = new byte[bufferSize];

                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    while (readCount > 0)
                    {
                        fileStream.Write(buffer, 0, readCount);
                        readCount = ftpStream.Read(buffer, 0, bufferSize);
                    }
                }

                ftpStream.Close();
                ftpResponse.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Close();

                if (ftpResponse != null)
                    ftpResponse.Close();
            }

            return StoragePath;
        }
    }
}
