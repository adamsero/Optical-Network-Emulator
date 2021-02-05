using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Cloud {
    static class Program { 

        public static LinkedList<Tuple<int, String, int, String, int>> connectionTable; 
        public static LinkedList<Tuple<int, String, int, String, int>> brokenConnections = new LinkedList<Tuple<int, String, int, String, int>>();

        [STAThread]
        static void Main() {

            connectionTable = new ConfigLoader().loadConfig(); 

            new Thread(() => {
                Thread.Sleep(300);
                new ConnectionListener();
            }).Start();          
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIWindow());
        }
    }
}
