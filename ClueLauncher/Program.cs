﻿// Copyright (c) 2024 Miguel Martins
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using EasyHook;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;

namespace ClueLauncher
{
    public class ClueInterface : MarshalByRefObject
    {
        public static Dictionary<string, string> AppSettings { get; set; }

        public void IsInstalled(int InClientPID)
        {
            Log.Information("Installed in target {0}.", InClientPID);
        }

        public void ReportException(Exception InInfo)
        {
            Log.Error("The target process has reported an error:\r\n" + InInfo.ToString());
        }

        public void Ping()
        {
        }

        public void LogInformation(string message)
        {
            Log.Information(message);
        }

        public void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public string GetSetting(string key)
        {
            return AppSettings[key];
        }
    }

    internal class Program
    {
        private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message:lj}{NewLine}{Exception}";
        private const string LogFile = "randomenhancerlog.txt";
        static string ChannelName = null;

        static void Main()
        {
            EmptyLogFile();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: OutputTemplate)
                .WriteTo.File(LogFile, outputTemplate: OutputTemplate).CreateLogger();
            ReadSettings();

            try
            {
                RemoteHooking.IpcCreateServer<ClueInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);
                string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "HookInject.dll");
                RemoteHooking.CreateAndInject(ClueInterface.AppSettings["PathToClueExe"], "", 0, InjectionOptions.DoNotRequireStrongName, injectionLibrary, injectionLibrary, out int TargetPID, ChannelName);
                Log.Information("Created and injected process {0}", TargetPID);
                Process process = Process.GetProcessById(TargetPID);
                process.WaitForExit();
            }
            catch (Exception ExtInfo)
            {
                Log.Error("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
                Console.WriteLine("<Press any key to exit>");
                Console.ReadKey();
            }
        }

        private static void ReadSettings()
        {
            ClueInterface.AppSettings = new Dictionary<string, string>();
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                ClueInterface.AppSettings[key] = ConfigurationManager.AppSettings[key];
            }
        }

        private static void EmptyLogFile()
        {
            using (FileStream fileStream = File.OpenWrite(LogFile))
            {
                fileStream.SetLength(0);
            }
        }
    }
}
