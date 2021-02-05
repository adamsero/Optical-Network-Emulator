using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace NetworkNode {
    public partial class GUIWindow : Form {

        private static GUIWindow instance;
        private List<string> logBuffer = new List<string>();

        public GUIWindow() {
            InitializeComponent();
            CreateRoutingTable();
            instance = this;
            lock (Program.waiterConfig) {
                Monitor.Pulse(Program.waiterConfig);
            }
        }

        private void OnClosing(Object sender, FormClosingEventArgs e) {
            new Thread(() => {
                Thread.Sleep(10);
                Environment.Exit(0);
            }).Start();

            Program.cloudConnection.DisconnectFromCloud();
        }

        private void CreateRoutingTable() {

            RoutingTable.Rows.Clear();
            RoutingTable.Refresh();

            RoutingTable.ColumnCount = 2;
            RoutingTable.Columns[0].Name = "Connection ID";
            RoutingTable.Columns[1].Name = "Out Port";

            foreach (DataGridViewColumn column in RoutingTable.Columns) {
                column.Width = RoutingTable.Size.Width / 2;
            }
        }

        public static void UpdateRoutingTable(LinkedList<string[]> rows) {

            //instance.RoutingTable.Rows.Clear();
            instance.RoutingTable.Refresh();

            //usuwanie może nie działać do końca poprawnie ale na razie nie mam jak sprawdzić
            foreach (string[] row in rows) {
                if(row[1].Equals("-1")) {
                    //List<Tuple<int, int>> copyRoutingTable = new List<Tuple<int, int>>(Program.routingTable);

                    //foreach(Tuple<int, int> tuple in copyRoutingTable) {
                    //    if (tuple.Item1 == Convert.ToInt32(row[0]))
                    //        Program.routingTable.Remove(tuple);
                    //}
                    for(int i = Program.routingTable.Count - 1; i >= 0; i--) {
                        if (Program.routingTable[i].Item1 == Convert.ToInt32(row[0]))
                            Program.routingTable.Remove(Program.routingTable[i]);
                    }

                    bool deleted = false; 
                    
                    //foreach (DataGridViewRow dataRow in instance.RoutingTable.Rows) {
                    //    if(dataRow.Cells["Connection ID"].Value.Equals(row[0])) {
                    //        instance.RoutingTable.Rows.Remove(dataRow);
                    //        deleted = true;
                    //    }
                    //}

                    for(int i = instance.RoutingTable.RowCount - 1; i >=0; i--) {
                        DataGridViewRow DataRow = instance.RoutingTable.Rows[i];
                        if(DataRow.Cells["Connection ID"].Value.Equals(row[0])) {
                            instance.RoutingTable.Rows.Remove(DataRow);
                            deleted = true;
                        }
                    }

                    if(!deleted)
                        PrintLog("Failed to delete Connection Table row (wrong Connection ID)!");
                } else
                    instance.RoutingTable.Rows.Add(row);
            }

            instance.RoutingTable.Refresh();
        }

        private string TimeStamp() {
            string hms = DateTime.Now.ToString("HH:mm:ss");
            string millis = DateTime.Now.Millisecond.ToString();
            string prefix = "";
            for (int i = 0; i < 3 - millis.Length; i++)
                prefix += "0";
            return "[" + hms + "." + prefix + millis + "] ";
        }

        public static void PrintLog(String message) {

            if (instance.PauseLogsCheckBox.Checked) {
                instance.logBuffer.Add(instance.TimeStamp() + message);
                return;
            }

            instance.LogBox.Invoke((MethodInvoker)delegate {
                if (!instance.LogBox.Text.Equals(""))
                    instance.LogBox.AppendText(Environment.NewLine);
                instance.LogBox.AppendText(instance.TimeStamp() + message);
            });
        }

        public static void PrintLogNoTimeStamp(String message) {

            if (instance.PauseLogsCheckBox.Checked) {
                instance.logBuffer.Add(instance.TimeStamp() + message);
                return;
            }

            instance.LogBox.Invoke((MethodInvoker)delegate {
                if (!instance.LogBox.Text.Equals(""))
                    instance.LogBox.AppendText(Environment.NewLine);
                instance.LogBox.AppendText(message);
            });
        }

        private void ClearButton_Click(object sender, EventArgs e) {
            LogBox.Text = "";
            logBuffer.Clear();
        }

        private void PauseLogsCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (!PauseLogsCheckBox.Checked) {
                foreach(string log in logBuffer)
                    PrintLogNoTimeStamp(log);
                logBuffer.Clear();
            }
        }

        public static void ChangeWindowName(string newName) {
            instance.Invoke((MethodInvoker)delegate {
                instance.Text = newName;
            });

            instance.NameLabel.Invoke((MethodInvoker)delegate {
                instance.NameLabel.Text = newName;
            });
        }

        public static void ChangeIP(string ip) {
            instance.ownIP.Invoke((MethodInvoker)delegate {
                instance.ownIP.Text = ip;
            });
        }
    }
}
