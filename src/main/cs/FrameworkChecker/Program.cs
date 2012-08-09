using System;
using System.Reflection;

namespace FrameworkChecker {
    
    class Program {
        
        static void Main(string[] args) {

            if (args.Length == 0) {
                Console.WriteLine("Usage: frmvchk [dllpath]");
                Console.WriteLine("  [dllpath]   - path to the DLL library to check framework version");
                return;
            }

            Assembly a = Assembly.ReflectionOnlyLoadFrom(args[0]);
            Console.WriteLine(a.ImageRuntimeVersion);
        }
    }
}
