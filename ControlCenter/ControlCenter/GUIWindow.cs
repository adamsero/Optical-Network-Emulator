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

namespace ControlCenter {
    public partial class GUIWindow : Form {

        private static GUIWindow instance;
        private readonly List<string> logBuffer1 = new List<string>();

        public GUIWindow() {
            InitializeComponent();
            CreateChannelTable();
            //UpdateChannelTable();
            instance = this;
        }

        private void CreateChannelTable() {

            ChannelTable.Rows.Clear();
            ChannelTable.Refresh();

            ChannelTable.ColumnCount = 2;
            ChannelTable.Columns[0].Name = "Connection ID";
            ChannelTable.Columns[1].Name = "EON Channels 12.5GHz";

            //foreach (DataGridViewColumn column in ChannelTable.Columns) {
            //    column.Width = ChannelTable.Size.Width / 2;
            //}
            ChannelTable.Columns[0].Width = 88;
            ChannelTable.Columns[1].Width = 700;
        }

        public static void UpdateChannelTable() {
            instance.ChannelTable.Invoke((MethodInvoker)delegate {
                instance.ChannelTable.Rows.Clear();
                instance.ChannelTable.Refresh();
                foreach (int key in ConfigLoader.connections.Keys) {
                    ConfigLoader.connections.TryGetValue(key, out Connection conn);
                    string result = string.Join("", conn.slot);
                    instance.ChannelTable.Rows.Add(new string[] { key.ToString(), result });
                }
            }); 
        }

        private void OnClosing(Object sender, FormClosingEventArgs e) {
            Environment.Exit(0);
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
                    instance.logBuffer1.Add(instance.TimeStamp() + message);
                return;
            }

            instance.LogBox1.Invoke((MethodInvoker)delegate {
                if (!instance.LogBox1.Text.Equals(""))
                    instance.LogBox1.AppendText(Environment.NewLine);
                instance.LogBox1.AppendText(instance.TimeStamp() + message);
            });
        }

        public static void PrintLogNoTimeStamp(String message) {

            if (instance.PauseLogsCheckBox.Checked) {
                    instance.logBuffer1.Add(instance.TimeStamp() + message);
                return;
            }

            instance.LogBox1.Invoke((MethodInvoker)delegate {
                if (!instance.LogBox1.Text.Equals(""))
                    instance.LogBox1.AppendText(Environment.NewLine);
                instance.LogBox1.AppendText(message);
            });
        }

        private void ClearButton_Click(object sender, EventArgs e) {
            LogBox1.Text = "";
            logBuffer1.Clear();
        }

        private void PauseLogsCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (!PauseLogsCheckBox.Checked) {
                foreach (string log in logBuffer1)
                    PrintLogNoTimeStamp(log);
                logBuffer1.Clear();
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

        public static bool ShowKeepAlive() {
            return instance.KeepAliveCheckBox.Checked;
        }
      
        private void LogBox1_TextChanged(object sender, EventArgs e) {

        }

        private void ChannelTable_CellContentClick(object sender, DataGridViewCellEventArgs e) {
        }
    }
}
