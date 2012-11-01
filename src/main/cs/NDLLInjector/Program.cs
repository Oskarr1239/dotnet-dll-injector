using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Globalization;
using System.Reflection;

namespace NDLLInjector {

    class Program {

        static String checkArg(String a, String name) {
            if (a.StartsWith(name)) {
                return a.Substring(name.Length);
            }
            return null;
        }

        static void Main(string[] args) {

            bool verbose = false;
            for (int i = 0; i < args.Length; i++) {
                if (args[i].StartsWith("--verbose")) {
                    verbose = true;
                    break;
                }
            }

            if (args.Length == 0) {
                PrintUsage();
                Environment.Exit(-1);
            }

            if (verbose) {
                for (int i = 0; i < args.Length; i++) {
                    Console.WriteLine("argument " + i + " " + args[i]);
                }
            }

            // required 
            String argProcID = null;
            String argRuntime = null;
            String argDllPath = null;
            String argSignature = null;

            // optional
            String argX86Runner = null;
            String argX64Bootstrap = null;
            String argX32Bootstrap = null;

            // parse arguments
            for (int i = 0; i < args.Length; i++) {

                String a = args[i];

                if (argProcID == null && (argProcID = checkArg(a, "--proc-id=")) != null) continue;
                if (argRuntime == null && (argRuntime = checkArg(a, "--runtime=")) != null) continue;
                if (argDllPath == null && (argDllPath = checkArg(a, "--dll-path=")) != null) continue;
                if (argSignature == null && (argSignature = checkArg(a, "--signature=")) != null) continue;
                if (argX86Runner == null && (argX86Runner = checkArg(a, "--x86-runner-path=")) != null) continue;
                if (argX64Bootstrap == null && (argX64Bootstrap = checkArg(a, "--x64-bootstrap-path=")) != null) continue;
                if (argX32Bootstrap == null && (argX32Bootstrap = checkArg(a, "--x32-bootstrap-path=")) != null) continue;

                if (a.StartsWith("--help")) {
                    PrintUsage();
                    Environment.Exit(0);
                }
            }

            if (verbose) {
                Console.WriteLine("parsed proc id: " + argProcID);
                Console.WriteLine("parsed runtime: " + argRuntime);
                Console.WriteLine("parsed dll: " + argDllPath);
                Console.WriteLine("parsed signature: " + argSignature);
                Console.WriteLine("parsed x86runner: " + argX86Runner);
                Console.WriteLine("parsed x64boot: " + argX64Bootstrap);
                Console.WriteLine("parsed x32boot: " + argX32Bootstrap);
            }

            if (argProcID == null) {
                Console.WriteLine("ERROR: Missing --proc-id argument");
                Environment.Exit(-1);
            }
            if (argDllPath == null) {
                Console.WriteLine("ERROR: Missing --dll-path argument");
                Environment.Exit(-1);
            }
            if (argSignature == null) {
                Console.WriteLine("ERROR: Missing --signature argument");
                Environment.Exit(-1);
            }

            int pid = Int32.Parse(argProcID);

            try {
                Process process = Process.GetProcessById(pid);
            } catch (ArgumentException e) {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }

            bool is64BitCurrentProcess = ProcessInjector.Is64BitProcess(Process.GetCurrentProcess().Id);
            bool is64BitTargetProcess = ProcessInjector.Is64BitProcess(pid);

            if (is64BitCurrentProcess ^ is64BitTargetProcess) {

                StringBuilder sb = new StringBuilder(1024);

                foreach(string arg in Concat(Process.GetCurrentProcess().MainModule.FileName, args)) {
                    sb.AppendFormat(arg.Contains(" ") ? "\"{0}\" " : "{0} ", arg);
                }

                String x86runner = null;
                if (argX86Runner == null) {
                    x86runner = Path.Combine(Directory.GetCurrentDirectory(), "x86runner.exe");
                } else {
                    x86runner = argX86Runner;
                }
                if (verbose) {
                    Console.WriteLine("Use x64 runner from: " + x86runner);
                }

                Process proc = Process.Start(x86runner, sb.ToString());
                proc.WaitForExit();
                Environment.ExitCode = proc.ExitCode;

            } else {

                // bootstrap path

                String bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), string.Format(@"bootstrap{0}.bin", (is64BitCurrentProcess ? "64" : "32")));
                if (is64BitCurrentProcess && argX64Bootstrap != null) {
                    bootstrapPath = argX64Bootstrap;
                } else if (!is64BitCurrentProcess && argX32Bootstrap != null) {
                    bootstrapPath = argX32Bootstrap;
                }

                // find suitable runtime version

                if (argRuntime == null) {
                    argRuntime = findFrameworkVersion(argDllPath, verbose);
                    if (argRuntime == null) {
                        Console.WriteLine("ERROR: Cannot find appropriate runtime version to use");
                        Environment.Exit(-1);
                    }
                }

                // find namespace / class / method signature

                int p = argSignature.LastIndexOf(".");
                String method = argSignature.Substring(p + 1);
                String clazz = argSignature.Substring(0, p);

                if (verbose) {
                    Console.WriteLine("Use bootstrap x" + (is64BitCurrentProcess ? "64" : "32") + ": " + bootstrapPath);
                    Console.WriteLine("Use runtime: " + argRuntime);
                    Console.WriteLine("Use DLL injectee: " + argDllPath);
                    Console.WriteLine("Run class: " + clazz);
                    Console.WriteLine("Run method: " + method);
                }


                try {
                    ProcessInjector pi = new ProcessInjector();
                    Environment.ExitCode = pi.Inject(pid, bootstrapPath, argRuntime, argDllPath, clazz, method);
                } catch( Exception e) {
                    Console.WriteLine("Error occured: ", e.StackTrace);
                    Environment.ExitCode = -1;
                }
            }
        }

        private static String findFrameworkVersion(String dllPath, bool verbose) {

            // read assembly information from manifest

            Assembly a = Assembly.ReflectionOnlyLoadFrom(dllPath);
            String v = a.ImageRuntimeVersion.Remove(0, 1);
            String search = v.Substring(0, v.LastIndexOf("."));
            RegistryKey installed = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
            String[] versions = installed.GetSubKeyNames();

            if (verbose) {
                Console.WriteLine("DLL assembly version: " + v);
            }

            for (int i = 0; i < versions.Length; i++) {

                v = versions[i];

                if (v.StartsWith("v")) {

                    if (verbose) {
                        Console.WriteLine("Registry framework version: " + versions[i]);
                    }

                    v = v.Remove(0, 1);
                    int k = v.LastIndexOf(".");
                    if (k != -1) {
                        String found = v.Substring(0, k);
                        if (found.Equals(search)) {
                            return versions[i];
                        }
                    }
                }
            }

            return null;
        }

        private static void PrintUsage() {
            String name = System.AppDomain.CurrentDomain.FriendlyName.Split(new char[] { '.' })[0];
            Console.WriteLine("Usage: " + name + " --proc-id=<procid> --dll-path=<dllpath> --signature=<signature> --runtime=[runtime] --x64-bootstrap-path=[x64-boot] --x32-bootstrap-path=[x32-boot] --x86-runner-path=[x86runner]");
            Console.WriteLine("  <procid>    - ID of process to inject DLL into (required)");
            Console.WriteLine("  <dllpath>   - path to injectee DLL file (required)");
            Console.WriteLine("  <signature> - signature of method to be run, e.g. Namespace.Clazz.Main (required)");
            Console.WriteLine("  [runtime]   - framework runtime version (optional)");
            Console.WriteLine("  [x86runner] - path to the x86 runner executable (optional)");
            Console.WriteLine("  [x64-boot]  - path to the x64 bootstrap (optional)");
            Console.WriteLine("  [x32-boot]  - path to the x32 bootstrap (optional)");
        }

        private static IEnumerable<T> Concat<T>(T first, IEnumerable<T> other ) {
            yield return first;
            foreach (T t in other) {
                yield return t;
            }
        } 
    }
}
