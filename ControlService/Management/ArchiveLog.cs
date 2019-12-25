using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Threading;
using log4net;

namespace ControlService.Management
{
    public static class ArchiveLog
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ArchiveLog));
        static List<string> listFile = new List<string>();
        static Boolean isRun = false;
        static string filePath = "";
        static string bakPath = "";

        public static void doWork(string srcPath, string destPath)
        {
            try
            {
                if (isRun)
                    return;//如果正在執行中，跳過此次執行
                           //存放日誌文件的目錄 
                           //string filePath = @"D:\log\";
                           //string bakPath = @"D:\log_backup\";
                filePath = srcPath;
                bakPath = destPath;


                ThreadPool.QueueUserWorkItem(new WaitCallback(ArchivePlan));
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        private static void ArchivePlan(object obj)
        {
            RunZip();//被呼叫後先執行一次，之後每天12點做一次
            while (true)
            {
                try
                {
                    //DateTime formatDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    ////指定的備份時間(例如:當天下午12點1分1秒 時開始執行備份操作)
                    //DateTime startTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 12:mm:ss"));
                    ////Console.WriteLine(formatDate + " " + startTime);
                    ////定時執行時間判斷 
                    //double ts = (formatDate. - startTime).TotalSeconds;
                    //if (ts == 0)//符合JOB 該執行的時段: 12點
                    //{
                    //    RunZip();//執行壓縮檔案
                    //}
                    if (DateTime.Now.Hour == 12)
                    {
                        RunZip();//執行壓縮檔案
                    }

                }
                catch (Exception e)
                {
                    logger.Error(e.StackTrace);
                }
                finally
                {
                    Thread.Sleep(60000 * 60);//60秒 * 60 => 一小時確認一次時間
                }
            }

        }

        private static void RunZip()
        {
            try
            {
                isRun = true;//設定鎖定
                GetAllFiles(filePath);//設定 listFile, 將一個月前的檔案加入此LIST
                //搬移檔案至備份目錄
                foreach (string file in listFile)
                {
                    FileInfo srcFileInfo = new FileInfo(file);
                    string bkForder = bakPath + srcFileInfo.LastWriteTime.ToString("yyyy-MM-dd").ToString() + "\\";
                    //Console.WriteLine(DateTime.Now + "-=|備份:" + file + " to " + bkForder);
                    string toFileName = file.Replace(filePath, bkForder);
                    FileInfo newFileInfo = new FileInfo(toFileName);
                    if (newFileInfo.Directory.Exists == false)
                    {
                        Console.WriteLine("Create " + newFileInfo.Directory);
                        newFileInfo.Directory.Create();
                    }
                    File.Move(file, toFileName);
                }
                //壓縮檔案並刪除
                if (!(Directory.Exists(bakPath)))
                {
                    Directory.CreateDirectory(bakPath);
                }
                string[] dir = Directory.GetDirectories(bakPath);
                for (int n = 0; n < dir.Length; n++)
                {
                    if (dir[n].EndsWith("zip"))
                        continue; //ZIP 資料夾不處理
                                  //處理壓縮
                    FileInfo srcDirInfo = new FileInfo(dir[n]);
                    FileInfo zipFileInfo = new FileInfo(bakPath + "zip\\" + srcDirInfo.Name.Substring(0, 7) + "\\" + srcDirInfo.Name + "-" + DateTime.Now.ToString("MMddmmssfff") + ".zip");
                    if (zipFileInfo.Directory.Exists == false)
                    {
                        zipFileInfo.Directory.Create();
                    }
                    //如果您收到組建錯誤「名稱 'ZipFile' 不存在於目前的內容中」，
                    //請將 System.IO.Compression.FileSystem 組件的參考新增至您的專案。
                    logger.Debug("dir[n]:" + dir[n] + " zipFileInfo.FullName:" + zipFileInfo.FullName);
                    ZipFile.CreateFromDirectory(dir[n], zipFileInfo.FullName);
                    DeleteDirectory(srcDirInfo.FullName);//必須要變更權限才能刪除檔案，所以獨立寫個 function
                }
                Console.WriteLine(DateTime.Now + "任務結束");
                isRun = false;//解除鎖定
            }
            catch (Exception e)
            {
                logger.Error(e.Message + " bakPath:" + bakPath + "\n" + e.StackTrace);
                isRun = false;//解除鎖定
            }
        }

        private static void GetAllFiles(string filePath)
        {
            try
            {

                if (!System.IO.File.Exists("D:\\log\\log.txt"))
                {
                    System.IO.FileStream f = System.IO.File.Create("D:\\log\\log.txt");
                    f.Close();
                }
                //遞歸遍歷所有子文件夾 
                string[] dir = Directory.GetDirectories(filePath);
                for (int n = 0; n < dir.Length; n++)
                {
                    GetAllFiles(dir[n]);
                }


                //保存文件夾內的文件路徑 log文件 
                string[] file = Directory.GetFiles(filePath, "*.*");//*.txt
                for (int i = 0; i < file.Length; i++)
                {
                    if (file[i].EndsWith("zip"))
                        continue;
                    FileInfo fi = new FileInfo(file[i]);
                    if (fi.LastWriteTime < DateTime.Now.AddMonths(-1))//將一個月前的檔案加入文件列表
                    {
                        listFile.Add(file[i]);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }
        public static void DeleteDirectory(string targetDir)
        {
            try
            {
                File.SetAttributes(targetDir, FileAttributes.Normal);

                string[] files = Directory.GetFiles(targetDir);
                string[] dirs = Directory.GetDirectories(targetDir);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }

                Directory.Delete(targetDir, false);
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

    }
}
