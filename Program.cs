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
            foreach (var temp in Parse(Command("dotnet", "tool list -g")))
            {
                UpdateTool(temp);
            }

            Console.WriteLine("Update finished");
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

            line = stringReader.ReadLine();
            var regex = new Regex(@"(\S+)\s+", RegexOptions.Compiled);
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

            var p = new Process
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

            p.Start(); //启动程序

            //获取输出信息
            string output = p.StandardOutput.ReadToEnd();
            output += p.StandardError.ReadToEnd();

            p.WaitForExit(TimeSpan.FromMinutes(1).Milliseconds); //等待程序执行完退出进程
            p.Close();

            return output + "\r\n";
        }
    }
}
