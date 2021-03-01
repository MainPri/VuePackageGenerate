using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CreateVueApp
{
    class CmdUtility
    {

        public string ExecuteInCMD(string commandText, string directory)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                //工作目录
                if (!string.IsNullOrEmpty(directory))
                {
                    p.StartInfo.WorkingDirectory = directory;
                }
                p.Start();//启动程序

                p.StandardInput.WriteLine(commandText + "&exit");

                p.StandardInput.AutoFlush = true;


                string output = p.StandardOutput.ReadToEnd();

               

                //获取cmd窗口的输出信息
                StreamReader reader = p.StandardOutput;//截取输出流
                StreamReader error = p.StandardError;//截取错误信息
                string str = reader.ReadToEnd() + error.ReadToEnd();
                if(!string.IsNullOrEmpty(str) && !str.Contains("Building") && !str.Contains("production") && !str.Contains("npm"))
                {
                    p.WaitForExit();
                    p.Close();
                    return "bad";
                }
                else
                {
                    p.WaitForExit();
                    p.Close();
                    return output;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception Occurred:{0},{1}", e.Message, e.StackTrace.ToString());
                return null;
            }
        }

        /// <summary>
        /// 启动外部windows应用程序，隐藏程序界面
        /// </summary>
        /// <param name="appname">应用程序路径名称</param>
        /// <returns>true表示成功，false表示失败</returns>
        public string Startapp(string appname, string directroy)
        {
            return Startapp(appname, directroy, ProcessWindowStyle.Hidden);
        }
        /// <summary>
        /// 启动外部应用程序
        /// </summary>
        /// <param name="appname">应用程序路径名称</param>
        /// <param name="style">进程窗口模式</param>
        /// <returns>true表示成功，false表示失败</returns>
        public string Startapp(string appname, string directroy, ProcessWindowStyle style)
        {
            return Startapp(appname, null, directroy, style);
        }
        /// <summary>
        /// 启动外部应用程序，隐藏程序界面
        /// </summary>
        /// <param name="appname">应用程序路径名称</param>
        /// <param name="arguments">启动参数</param>
        /// <returns>true表示成功，false表示失败</returns>
        public string Startapp(string appname, string arguments, string directroy)
        {
            return Startapp(appname, arguments, directroy, ProcessWindowStyle.Maximized);
        }
        /// <summary>
        /// 启动外部应用程序
        /// </summary>
        /// <param name="appname">应用程序路径名称</param>
        /// <param name="arguments">启动参数</param>
        /// <param name="style">进程窗口模式</param>
        /// <returns>true表示成功，false表示失败</returns>
        public string Startapp(string appname, string arguments, string directory, ProcessWindowStyle style)
        {
            string result;
            var start = new ProcessStartInfo();
            if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(arguments))
            {
                start = new ProcessStartInfo
                {
                    FileName = appname,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    //RedirectStandardInput = true,
                    //RedirectStandardError = true,
                    WindowStyle = style,
                    WorkingDirectory = directory
                };
            }
            else
            {
                start = new ProcessStartInfo
                {
                    FileName = appname,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    //RedirectStandardInput = true,
                    //RedirectStandardError = true,
                    WindowStyle = style
                };
            }
            
            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                    process.WaitForExit();
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
            //string result = null;
            //Process p = new Process();
            //p.StartInfo.FileName = appname;//exe,bat and so on
            //p.StartInfo.WindowStyle = style;
            //p.StartInfo.Arguments = arguments;
            //try
            //{
            //    p.Start();
            //    p.WaitForExit();
            //    Console.WriteLine(p.StandardOutput.ReadToEnd());
            //    p.Close();
            //}
            //catch(Exception e)
            //{
            //    throw e;
            //}
        }
    }
}
