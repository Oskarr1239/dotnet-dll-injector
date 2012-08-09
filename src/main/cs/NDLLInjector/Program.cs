using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NDLLInjector {

    class Program {

        static void Main(string[] args) {

            if (args.Length != 5) {
                PrintUsage();
                Environment.Exit(-1);
            }

            String procName = args[0];
            Process[] processes = Process.GetProcessesByName(procName);
            if (processes.Length == 0) {
                Console.WriteLine("No processes with name " + procName);
                return;
            }

            Process process = processes[0];

            int pid = process.Id;
            bool is64BitCurrentProcess = ProcessInjector.Is64BitProcess(Process.GetCurrentProcess().Id);
            bool is64BitTargetProcess = ProcessInjector.Is64BitProcess(pid);

            if (is64BitCurrentProcess ^ is64BitTargetProcess) {

                StringBuilder sb = new StringBuilder(1024);

                foreach(string arg in Concat(Process.GetCurrentProcess().MainModule.FileName, args)) {
                    sb.AppendFormat(arg.Contains(" ") ? "\"{0}\" " : "{0} ", arg);
                }

                Process proc = Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "x86runner.exe"), sb.ToString());
                proc.WaitForExit();
                Environment.ExitCode = proc.ExitCode;
            } else {
                try {
                    ProcessInjector pi = new ProcessInjector();
                    String bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), string.Format(@"bootstrap{0}.bin", (is64BitCurrentProcess ? "64" : "32")));
                    Environment.ExitCode = pi.Inject(pid, bootstrapPath, args[1], args[2], args[3], args[4]);
                } catch( Exception e) {
                    Console.WriteLine("Error occured: ", e.StackTrace);
                    Environment.ExitCode = -1;
                }
            }
        }

        private static void PrintUsage() {
            String name = System.AppDomain.CurrentDomain.FriendlyName.Split(new char[] { '.' })[0];
            Console.WriteLine("Usage: " + name + " [procname] [runtime] [dllpath] [class] [function]");
            Console.WriteLine("  [procname] - process name");
            Console.WriteLine("  [runtime]  - framework runtime version");
            Console.WriteLine("  [dllpath]  - path to injectee DLL file");
            Console.WriteLine("  [class]    - injectee class name togehter with namespace (e.g. Test.Program)");
            Console.WriteLine("  [function] - injectee function to run (e.g. Main)");
        }

        private static IEnumerable<T> Concat<T>(T first, IEnumerable<T> other ) {
            yield return first;
            foreach (T t in other) {
                yield return t;
            }
        } 
    }
}
