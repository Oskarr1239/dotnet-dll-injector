using System;
using System.Windows.Forms;

namespace X86Runner {
    
    class Program {

        static void Main(string[] args) {

            if (args.Length == 0) {
                MessageBox.Show("Too few arguments");
                return;
            }

            string[] arguments = new string[args.Length - 1];
            for (int i = 1; i < args.Length; i++) {
                arguments[i - 1] = args[i];
            }

            Environment.ExitCode = AppDomain.CurrentDomain.ExecuteAssembly(args[0], AppDomain.CurrentDomain.Evidence, arguments);
        }
    }
}
