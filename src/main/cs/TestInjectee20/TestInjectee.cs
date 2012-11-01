using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Test {
    
    class Program {
    
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);
        
        public static int Main(string arg) {
           
            Process process = Process.GetCurrentProcess();
            int pid = process.Id;
            ProcessModule main = process.MainModule;
            MessageBox.Show("Hello World from Injectee, PID " + pid + " running " + main.FileName);

            String name = System.Reflection.Assembly.GetCallingAssembly().GetName().Name + ".dll";

            IntPtr pointer = GetModuleHandle(name);
            if (IntPtr.Zero == pointer) {
                String msg = "Cannot get " + name + " module handle.";
                MessageBox.Show(msg);
                return -1;
            }

            FreeLibrary(pointer);

            MessageBox.Show("Deallocated " + name);
            return 0;
        }
    }
}