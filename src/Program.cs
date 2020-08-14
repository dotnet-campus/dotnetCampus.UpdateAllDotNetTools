using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
            var self = @"dotnetCampus.UpdateAllDotNetTools";
            foreach (var temp in Parse(Command("dotnet", "tool list -g")))
            {
                if (temp.Equals(self, StringComparison.OrdinalIgnoreCase))
                {
                    // 最后才更新工具自身，解决 https://github.com/dotnet-campus/dotnetCampus.UpdateAllDotNetTools/issues/4
                    // 如果上在 MAC 下，是可以删除运行的软件
                    // 但是删除之后，调用 Process.Start 将会抛 Win32Exception 找不到自身
                }
                else
                {
                    TryUpdate(temp);
                }
            }

            // 尝试更新自身，在 MAC 设备下可以更新，在 Windows 下不能
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // https://github.com/dotnet-campus/dotnetCampus.UpdateAllDotNetTools/issues/4
                TryUpdate(self);
            }

            Console.WriteLine("Update finished");
        }

        private static void TryUpdate(string toolName)
        {
            try
            {
                UpdateTool(toolName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void UpdateTool(string tool)
        {
            Console.WriteLine(Command("dotnet", $"tool update {tool} -g"));
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

            // 输入是 dotnetcampus.officedocumentzipper           1.0.2              OfficeDocumentZipper
            // 下面代码需要返回 dotnetcampus.officedocumentzipper 用来升级
            line = stringReader.ReadLine();
            var regex = new Regex(@"(\S+)\s+", RegexOptions.Compiled);
            // Fix https://github.com/dotnet-campus/dotnetCampus.UpdateAllDotNetTools/issues/3
            while (!string.IsNullOrWhiteSpace(line))
            {
                var match = regex.Match(line);
                yield return match.Groups[1].Value;
                line = stringReader.ReadLine();
            }
        }

        private static string Command(string fileName, string argument, string workingDirectory = null)
        {
            Console.WriteLine($"{fileName} {argument}");
            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Environment.CurrentDirectory;
            }

            Console.WriteLine($"FileName={fileName}");
            Console.WriteLine($"Argument={argument}");
            Console.WriteLine($"WorkingDirectory={workingDirectory}");

            var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = argument,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false, //是否使用操作系统shell启动
                    //RedirectStandardInput = true, //接受来自调用程序的输入信息
                    RedirectStandardOutput = true, //由调用程序获取输出信息
                    RedirectStandardError = true, //重定向标准错误输出
                    CreateNoWindow = true, //不显示程序窗口
                    //StandardOutputEncoding = Encoding.GetEncoding("GBK") //Encoding.UTF8
                    //Encoding.GetEncoding("GBK");//乱码
                }
            };

            process.Start(); //启动程序

            const string breakLine = "\r\n";
            string output = "";
            process.OutputDataReceived += (sender, args) =>
            {
                output += args.Data + breakLine;
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                output += args.Data + breakLine;
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 如果应用程序没有退出，那么 ReadToEnd 将会等待，也就是当前进程将会等待这个进程退出
            ////获取输出信息
            //string output = p.StandardOutput.ReadToEnd();
            //output += p.StandardError.ReadToEnd();

            // 等待程序执行完退出进程
            process.WaitForExit(TimeSpan.FromMinutes(1).Milliseconds); 

            // 第二次等待解决 OutputDataReceived 调用的坑
            // 请看官方代码 https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.outputdatareceived?view=netcore-3.1
            process.WaitForExit(); 
            process.Close();

            return output + breakLine;
        }
    }
}
