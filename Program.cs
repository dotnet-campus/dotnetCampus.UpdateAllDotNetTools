using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace dotnetCampus.UpdateAllDotNetTools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting update all dotnet tools");
            Console.WriteLine("Finding installed tools");
            foreach (var temp in Parse(Command("dotnet tool list -g")))
            {
                UpdateTool(temp);
            }

            Console.WriteLine("Update finished");
        }

        private static void UpdateTool(string tool)
        {
            Console.WriteLine(Command($"dotnet tool update {tool} -g"));
        }

        private static IEnumerable<string> Parse(string command)
        {
            Console.WriteLine(command);

            var stringReader = new StringReader(command);
            var line = stringReader.ReadLine();
            while (line != null)
            {
                if (line.Contains("-------------------------"))
                {
                    break;
                }
                line = stringReader.ReadLine();
            }

            line = stringReader.ReadLine();
            var regex = new Regex(@"(\S+)\s+", RegexOptions.Compiled);
            while (line != null)
            {
                var match = regex.Match(line);
                yield return match.Groups[1].Value;
                line = stringReader.ReadLine();
            }
        }


        private static string Command(string str, string workingDirectory = null)
        {
            Console.WriteLine(str);
            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Environment.CurrentDirectory;
            }

            var p = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false, //是否使用操作系统shell启动
                    RedirectStandardInput = true, //接受来自调用程序的输入信息
                    RedirectStandardOutput = true, //由调用程序获取输出信息
                    RedirectStandardError = true, //重定向标准错误输出
                    CreateNoWindow = true, //不显示程序窗口
                    //StandardOutputEncoding = Encoding.GetEncoding("GBK") //Encoding.UTF8
                    //Encoding.GetEncoding("GBK");//乱码
                }
            };

            p.Start(); //启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            bool exited = false;

            //// 超时
            //Task.Run(() =>
            //{
            //    Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ =>
            //    {
            //        if (exited)
            //        {
            //            return;
            //        }

            //        try
            //        {
            //            if (!p.HasExited)
            //            {
            //                Console.WriteLine($"{str} 超时");
            //                p.Kill();
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine(e);
            //        }
            //    });
            //});

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();
            //Console.WriteLine(output);
            output += p.StandardError.ReadToEnd();
            //Console.WriteLine(output);

            //StreamReader reader = p.StandardOutput;
            //string line=reader.ReadLine();
            //while (!reader.EndOfStream)
            //{
            //    str += line + "  ";
            //    line = reader.ReadLine();
            //}

            p.WaitForExit(TimeSpan.FromMinutes(1).Milliseconds); //等待程序执行完退出进程
            p.Close();

            exited = true;

            return output + "\r\n";
        }
    }
}
