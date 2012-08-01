using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace TestProcess {

    public static class Program {

        public static String SecretValue = "secret-value";    

        static void Main(string[] args) {

            MessageBox.Show("Testing");

            String name = System.AppDomain.CurrentDomain.FriendlyName.Split(new char[] { '.' })[0];
            Process[] processes = Process.GetProcessesByName(name);

            Console.WriteLine("My name is: " + name);
            Console.WriteLine("My PID is:  " + processes[0].Id);
            Console.ReadKey();
        }
    }
}
