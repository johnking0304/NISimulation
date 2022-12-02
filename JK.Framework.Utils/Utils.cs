using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Management;

namespace JK.Framework.Utils
{
    public static class Utils
    {
        public static System.Diagnostics.Process RunningInstance()
        {
            System.Diagnostics.Process current = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process process in processes) //查找相同名称的进程 
            {
                if (process.Id != current.Id) //忽略当前进程 
                { //确认相同进程的程序运行位置是否一样. 
                    if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", @"/") == current.MainModule.FileName)
                    {
                        return process;
                    }
                }
            } //No other instance was found, return null. 
            return null;
        }


        public static byte SetBit(byte data, int index, bool flag)
        {
            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte)(data | v) : (byte)(data & ~v);
        }


        public static void KillProcess(string processName)
        {
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            //得到所有打开的进程   
            try
            {
                foreach (Process thisproc in Process.GetProcessesByName(processName))
                {
                    //找到程序进程,kill之。
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                    }
                }

            }
            catch (Exception)
            {
                //  MessageBox.Show(Exc.Message);
            }
        }


        public static void StopProcess(string processName)
        {
            try
            {
                System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (System.Diagnostics.Process p in ps)
                {
                    p.Kill();
                }
            }
            catch (Exception)
            {
                //throw ex;
            }
        }

        /// <summary>
        /// 处理未捕获异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            SaveExceptionLog("-----------------------begin--------------------------");
            SaveExceptionLog("CurrentDomain_UnhandledException" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SaveExceptionLog("IsTerminating : " + e.IsTerminating.ToString());
            SaveExceptionLog(e.ExceptionObject.ToString());
            SaveExceptionLog("-----------------------end----------------------------");
        }

        /// <summary>
        /// 处理UI主线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            SaveExceptionLog("-----------------------begin--------------------------");
            SaveExceptionLog("Application_ThreadException" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SaveExceptionLog("Application_ThreadException:" + e.Exception.Message);
            SaveExceptionLog(e.Exception.StackTrace);
            SaveExceptionLog("-----------------------end----------------------------");
        }


        public static void SaveLogMessage(string log)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\log\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = path + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            //采用using关键字，会自动释放
            using (FileStream fs = new FileStream(filePath, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                {

                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " " + log);
                }
            }
        }
        public static void SaveExceptionLog(string log)
        {


            string path = AppDomain.CurrentDomain.BaseDirectory + @"\exception\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = path + "exception.txt";

            //采用using关键字，会自动释放
            using (FileStream fs = new FileStream(filePath, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                {
                    sw.WriteLine(log);
                }
            }
        }
        public static void MessageBoxError(string message)
        {
            if (message.Length > 0)
            {
                MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void MessageBoxWarning(string message)
        {
            if (message.Length > 0)
            {
                MessageBox.Show(message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void DeleteDirectoryFile(string directory)
        {
            FileAttributes attr = File.GetAttributes(directory);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(directory, true);
            }
        }

        public static void MessageBoxInformation(string message)
        {
            if (message.Length > 0)
            {
                MessageBox.Show(message, "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static Boolean MessageBoxQuestion(string message)
        {
            if (message.Length > 0)
            {
                return MessageBox.Show(message, "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
            }
            return false;
        }


        public static void InitializeTableLayoutPanel(TableLayoutPanel container, int rowCount, int columnCount)
        {
            if (rowCount != 0 && columnCount != 0)
            {
                container.ColumnCount = columnCount;
                container.RowCount = rowCount;
                container.RowStyles.Clear();
                int percent = 100 / container.RowCount;
                for (int i = 0; i < container.RowCount; i++)
                {
                    container.RowStyles.Add(new RowStyle(SizeType.Percent, percent));
                }
                container.ColumnStyles.Clear();
                percent = 100 / container.ColumnCount;
                for (int i = 0; i < container.ColumnCount; i++)
                {
                    container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percent));
                }
                container.Dock = System.Windows.Forms.DockStyle.Fill;
                container.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic).SetValue(container, true, null);
            }

        }
        public static void AddFormToContainer(Form form, Control container)
        {
            if (form != null && container != null)
            {
                form.FormBorderStyle = FormBorderStyle.None;
                form.TopLevel = false;
                form.Dock = System.Windows.Forms.DockStyle.Fill;
                container.Controls.Add(form);
                form.Show();
            }
        }

        public static void AddFormToTableLayoutPanel(Form form, TableLayoutPanel container, int row, int column)
        {
            form.FormBorderStyle = FormBorderStyle.None;
            form.TopLevel = false;
            form.Dock = System.Windows.Forms.DockStyle.Fill;
            container.Controls.Add(form, column, row);
            form.Show();
        }
        // 摘要: 
        //将List转换为TXT文件
        public static void WriteListToFile(List<string> list, string filePathName)
        {
            try
            {
                //创建一个文件流，用以写入或者创建一个StreamWriter           
                FileStream fileStream = new FileStream(filePathName, FileMode.OpenOrCreate, FileAccess.Write);
                fileStream.SetLength(0);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.Flush();
                // 使用StreamWriter来往文件中写入内容 
                try
                {
                    streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                    for (int i = 0; i < list.Count; i++)
                    {
                        streamWriter.WriteLine(list[i]);
                    }
                    //关闭此文件 
                    streamWriter.Flush();
                }
                finally
                {
                    streamWriter.Close();
                    fileStream.Close();
                }
            }
            catch (System.IO.IOException)
            {

            }
        }

        public static Boolean DoPython(string StartFileName, string StartFileArg, Boolean hidden)
        {
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = StartFileName;      // 命令  
            CmdProcess.StartInfo.Arguments = StartFileArg;      // 参数  

            CmdProcess.StartInfo.CreateNoWindow = hidden;        // 不创建新窗口  
            CmdProcess.StartInfo.UseShellExecute = false;
            return CmdProcess.Start();
        }

        /// <summary>
        /// 读取文件中所有字符
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string LoadFromFile(string filename)
        {
            string content = "";
            if (System.IO.File.Exists(filename))
            {
                StreamReader reader = new StreamReader(filename, Encoding.UTF8);
                content = reader.ReadToEnd();
                reader.Close();
            }
            return content;
        }

        public static void SaveToFile(string filename, string content)
        {
            List<string> contents = new List<string>();
            contents.Add(content);
            WriteListToFile(contents, filename);
        }

        /// <summary>
        /// 日志部分
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        public static void WriteLogs(string fileName, string type, string content)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory + fileName;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + "\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);
                    fs.Close();
                }
                if (File.Exists(path))
                {
                    StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + type + "-->" + content);
                    sw.Close();
                }
            }
        }


        public static Boolean ContainChinese(string input)
        {
            string pattern = "[\u4e00-\u9fbb]";
            return Regex.IsMatch(input, pattern);
        }


        public static Boolean DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// 获取指定驱动器的空间总大小(单位为GB)  
        /// </summary>  
        /// <param name=”str_HardDiskName”>只需输入代表驱动器的字母即可 </param>  
        /// <returns> </returns>  

        public static long GetHardDiskSpace(string str_HardDiskName)
        {
            long totalSize = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    totalSize = drive.TotalSize;//    /(1024*1024*1024);  
                }
            }
            return totalSize;
        }

        /// <summary>  
        /// 获取指定驱动器的剩余空间总大小(单位为GB)    
        /// </summary>  
        /// <param name=”str_HardDiskName”>只需输入代表驱动器的字母即可 </param>  
        /// <returns> </returns>  

        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace;//    /(1024*1024*1024);  
                }
            }

            return freeSpace;
        }


        public static ushort[] StringToUShort(String inString)
        {
            if (inString.Length % 2 == 1)
            {
                inString += " ";
            };
            char[] bufChar = inString.ToCharArray();
            byte[] bufByte = new byte[2];
            ushort[] outShort = new ushort[bufChar.Length / 2];
            for (int i = 0, j = 0; i < bufChar.Length; i += 2, j++)
            {
                bufByte[1] = (byte)bufChar[i];
                bufByte[0] = (byte)bufChar[i + 1];
                outShort[j] = BitConverter.ToUInt16(bufByte, 0);
            }
            return outShort;
        }


        public static bool IsCorrenctIP(string ip)
        {
            string pattrn = @"(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])";
            return System.Text.RegularExpressions.Regex.IsMatch(ip, pattrn);
        }



    }












}
