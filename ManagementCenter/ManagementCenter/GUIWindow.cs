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

namespace ManagementCenter {
    public partial class GUIWindow : Form {

        private static GUIWindow instance;
        private readonly List<string> logBuffer = new List<string>();

        public GUIWindow() {
            InitializeComponent();
            instance = this;
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
                foreach (string log in logBuffer)
                    PrintLogNoTimeStamp(log);
                logBuffer.Clear();
            }
        }
    }
}
