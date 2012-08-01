using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

namespace TestInjectee {
    public static class Program {
        public static void Main() {
            MessageBox.Show("Secret value is " + TestProcess.Program.SecretValue);
        }
    }
}
