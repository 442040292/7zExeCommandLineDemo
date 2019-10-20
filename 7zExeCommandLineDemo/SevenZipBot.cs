
using _7zExeCommandLineDemo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zExeCommandLineDemo
{

    internal sealed class SevenZipBot
    {
        private const int ProcessTimeOut = 10000;
        private const int ProcessCheckListTimeOut = 300;
        private const string SEVENZIPEXEFILEPATH = "7-Zip/7z.exe";

        public static SevenZipBot Instance = new SevenZipBot();
        private List<FileModel> fileModels = new List<FileModel>();
        private bool startRead = false;
        private bool isrunning = false;

        #region innerClass
        public enum DuplicateOperate
        {
            Overwrite,  // -aoa表示直接覆盖现有文件，且没有提示。类似的还有：
            Skip,       // -aos跳过现有文件不会覆盖
            Rename,     // -aou如果相同文件名的文件已存在，将自动重命名被释放的文件
            RenameOld   // -aot如果相同文件名的文件已存在，将自动重命名现有的文件
        }
        public class FeedBack<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
        public class FileModel
        {
            public DateTime FileDateTime { get; set; }

            public string Attr { get; set; }
            public long Size { get; set; }
            public long Compressed { get; set; }

            public string InnerFullPath { get; set; }

            public FileModel(string fromCommandLine)
            {
                var prop = fromCommandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                FileDateTime = DateTime.Parse($"{prop[0]} {prop[1]}");
                Attr = prop[2];
                Size = long.Parse(prop[3]);
                Compressed = long.Parse(prop[4]);
                InnerFullPath = prop[5];
            }
        }

        #endregion

        /// <summary>
        /// 解压文件
        /// <param> -aoa表示直接覆盖现有文件，且没有提示。类似的还有：</param>
        /// <param> -aos跳过现有文件不会覆盖</param>
        /// <param> -aou如果相同文件名的文件已存在，将自动重命名被释放的文件</param>
        /// <param> -aot如果相同文件名的文件已存在，将自动重命名现有的文件</param>
        /// </summary>
        /// <param name="zipFilePath">源文件</param>
        /// <param name="destPath">目标文件夹</param>
        /// <param name="Duplicateactor">覆盖时操作选项</param>
        /// <returns></returns>
        public bool DeCompress(string zipFilePath, string destPath, DuplicateOperate Duplicateactor = DuplicateOperate.Overwrite)
        {
            var operation = string.Empty;
            switch (Duplicateactor)
            {
                case DuplicateOperate.Overwrite:
                    operation = " -aoa";
                    break;
                case DuplicateOperate.Skip:
                    operation = " -aos";
                    break;
                case DuplicateOperate.Rename:
                    operation = " -aou";
                    break;
                case DuplicateOperate.RenameOld:
                    operation = " -aot";
                    break;
                default:
                    break;
            }

            Log.Debug($"DeCompress Start zipFilePath[{zipFilePath}]");
            Process process = new Process();
            bool succeeded = false;
            System.DateTime startTime = System.DateTime.Now;
            process.StartInfo.FileName = SEVENZIPEXEFILEPATH;
            process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.StartInfo.Arguments = string.Format(@"x ""{0}"" -o""{1}"" {2}", zipFilePath, destPath, operation);
            // "x -o""D:\用户目录\下载\0723.zip"" -aoa"
            process.ErrorDataReceived += ErrorDataReceived;
            process.OutputDataReceived += OutputDataReceived;
            process.Start();//启动程序
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            Log.Debug($"SevenZipBot Start ");
            process.WaitForExit(ProcessTimeOut);//等待程序执行完退出进程  
            succeeded = (startTime.AddMilliseconds(ProcessTimeOut) > System.DateTime.Now);

            process.Close();
            Log.Debug($"SevenZipBot End Arguments:" + zipFilePath + ", succeeded:" + succeeded);

            return succeeded;
        }

        /// <summary>
        /// 获取压缩文件的文件列表
        /// </summary>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        public FeedBack<List<FileModel>> GetFileList(string zipFilePath)
        {
            Log.Debug($"SevenZipBot End Arguments:" + zipFilePath);
            var result = new FeedBack<List<FileModel>>();
            startRead = false;
            fileModels.Clear();
            Process process = new Process();
            bool succeeded = false;
            System.DateTime startTime = System.DateTime.Now;
            process.StartInfo.FileName = SEVENZIPEXEFILEPATH;
            process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            //process.StartInfo.Arguments = @"l ""D:\用户目录\下载\0723.zip""";
            process.StartInfo.Arguments = string.Format(@"l ""{0}""", zipFilePath);
            process.ErrorDataReceived += ErrorDataReceived;
            process.OutputDataReceived += OutputDataReceived;
            process.Start();//启动程序
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            Log.Debug($"SevenZipBot Start ");
            process.WaitForExit(ProcessTimeOut);//等待程序执行完退出进程  
            succeeded = (startTime.AddMilliseconds(ProcessCheckListTimeOut) > System.DateTime.Now);
            isrunning = true;
            while (isrunning && startTime.AddMilliseconds(ProcessCheckListTimeOut) > System.DateTime.Now)
            {
                //这里要卡住 不然程序执行太快 获取不到列表 如果列表数量比较大 失败的情况会少一点 3秒足够输出
                System.Threading.Thread.Sleep(300);
            }
            result.Success = succeeded;
            result.Data = new List<FileModel>(fileModels);
            process.Close();
            Log.Debug($"SevenZipBot End Arguments:" + zipFilePath + ", succeeded:" + succeeded);

            return result;
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.Debug($"pdf 操作 错误信息： {e.Data}");
            isrunning = false;
            Process process = sender as Process;
            process.Close();
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.Debug($"pdf 操作 命令行输出： {e.Data}");
            Console.WriteLine(e.Data);
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (startRead)
                {
                    if (e.Data.StartsWith("-------------------"))
                    {
                        startRead = false;
                        isrunning = false;
                        return;
                    }
                    fileModels.Add(new FileModel(e.Data));
                }

                if (e.Data.Contains("-------------------"))
                {
                    startRead = true;
                    Process process = sender as Process;
                    process.Close();
                }
            }
        }


        //Scanning the drive for archives:
        //1 file, 51964594 bytes(50 MiB)

        //Listing archive: D:\用户目录\下载\dotnet451.zip

        //--
        //Path = D:\用户目录\下载\dotnet451.zip
        //Type = zip
        //Physical Size = 51964594

        //   Date Time    Attr Size   Compressed Name
        //------------------- ----- ------------ ------------  ------------------------
        //2014-01-21 16:15:00 .....        31529         4066  Source\wpf\WindowsBase.csproj
        //2014-01-21 16:15:00 .....        31529         4066  Source\wpf\WindowsBase.csproj
        //------------------- ----- ------------ ------------  ------------------------
        //2014-02-12 15:54:34          278324055     47665754  18556 files


    }
}
