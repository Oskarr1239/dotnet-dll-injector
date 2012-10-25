using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace Test {
    class Program {
        public static int Main(string arg) {
            Process process = Process.GetCurrentProcess();
            int pid = process.Id;
            ProcessModule main = process.MainModule;
            MessageBox.Show("Hello World from Injectee, PID " + pid + " running " + main.FileName);
            return 0;
        }
    }
}