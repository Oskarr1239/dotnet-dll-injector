using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace TestProcess {
    public static class Program {
        public static String SecretValue = "secret-value";
        static void Main(string[] args) {
            Process process = Process.GetCurrentProcess();
            Console.WriteLine("My name is: " + process.ProcessName);
            Console.WriteLine("My PID is:  " + process.Id);
            
            do {
                Thread.Sleep(100);
            } while (true);
        }
    }
}
