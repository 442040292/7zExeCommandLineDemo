using iBot.Activities.PDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _7zExeCommandLineDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var filelist = SevenZipBot.GetFileList(@"D:\用户目录\下载\dotnet451.zip");
            var filelist = SevenZipBot.Instance.GetFileList(@"D:\用户目录\下载\0723.zip");
            MessageBox.Show($" 文件列表 [{string.Join(";", filelist.Data.Select(x => x.InnerFullPath))}]");
            var duplicatefile = filelist.Data.Select(x => System.IO.Path.Combine(@"D:\TEMP\", x.InnerFullPath)).Where(x => File.Exists(x));
            if (duplicatefile.Count() <= 0)
            {
                //SevenZipBot.DeCompress(@"D:\用户目录\下载\0723.zip", @"D:\TEMP");
                MessageBox.Show($" 存在重复文件 [{string.Join(";", duplicatefile)}]");
            }
            else
            {
                MessageBox.Show($" 存在重复文件 [{string.Join(";", duplicatefile)}]");
                //throw new Exception($" 存在重复文件 [{string.Join(";", duplicatefile)}]");
            }

        }
    }
}
