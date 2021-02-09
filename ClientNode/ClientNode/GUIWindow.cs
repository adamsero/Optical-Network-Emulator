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
using FrameLib;

namespace ClientNode {
    public partial class GUIWindow : Form {

        public static GUIWindow instance;
        private List<string> logBuffer = new List<string>();
        private readonly Dictionary<string, string> hostIPs = new Dictionary<string, string>();
        private static int requestedBandwidth;
        private static String receiver;

        public GUIWindow() {
            InitializeComponent();

            instance = this;

            SetButtonGreen();

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

        public static void ManageCallButton(bool state) {
            if (state) instance.SetButtonGreen();
            else instance.SetButtonGrey();
            instance.ToggleButton.Enabled = state; 
        }

        public static void ManageMessageBox(bool state) { 

            instance.MessageBox.Enabled = state;
        }

        public static void ManageSendButton(bool state) {

            instance.SendButton.Enabled = state;
        }

        public Button GetToggleButton() {
            return ToggleButton;
        }

        private void ToggleButton_Click(object sender, EventArgs e) {
            ManageCall(true);

        }

        public static void AcceptCall() {
            instance.ManageCall(false);
        }

        public static void UnlockSendingMessages() {

            instance.ToggleButton.Text = "END";
            instance.SetButtonRed();
            instance.MessageBox.Enabled = true;
            instance.SendButton.Enabled = true;

        }

        public static void LockSendingMessages() {

            instance.ToggleButton.Text = "CALL";
            instance.SetButtonGreen();
            instance.MessageBox.Enabled = false;
            instance.SendButton.Enabled = false;
        }

        private void ManageCall(bool enableDestinationRestrain) {
            switch (ToggleButton.Text) {
                case "CALL":

                    ReturnWindowParams();
                    if (enableDestinationRestrain && (Destination.Text.Length == 0 || requestedBandwidth==0)) {
                        GUIWindow.PrintLog("Destination name and throughput must not be empty!");
                        break; 
                    }                                            
                    Program.cpcc.SendCallRequest("Host" + ConfigLoader.nodeID, Destination.Text, requestedBandwidth);
                    break;

                case "END":
                    Program.cpcc.SendCallTeardownCPCC("Host" + ConfigLoader.nodeID, CPCC.cachedDestination, CPCC.connectionID);

                    break;
            }
        }

        public void ReturnWindowParams() { 
            
            //GUIWindow.PrintLog(Throughput.GetItemText(this.Throughput.SelectedItem));
            if(!Int32.TryParse(Throughput.GetItemText(this.Throughput.SelectedItem).Trim(), out requestedBandwidth)) {

                //GUIWindow.PrintLog("formatowanie blad"); 
            }
                
            receiver = Destination.Text;
            
        
        }

        public static String GetReceiver() {

            return receiver;
        }

        public static int GetRequestedBandwidth() {

            return requestedBandwidth;
        }

        private void SendOutPacket() {
            //SendMessage();

            string destinationName = "";
            Destination.Invoke((MethodInvoker)delegate {
                destinationName = Destination.Text;
            });

            //GUIWindow.PrintLog("CONN ID: " + Program.cpcc.connectionID);
            MessageBox.Invoke((MethodInvoker)delegate {
                CloudConnection.SendMessage(CPCC.connectionID, MessageBox.Text, destinationName);
            });
        }

        private void ClearButton_Click(object sender, EventArgs e) {
            LogBox.Text = "";
        }

        private void SetButtonGreen() {
            ToggleButton.ForeColor = Color.FromArgb(125, 166, 125);
            ToggleButton.BackColor = Color.FromArgb(192, 255, 192);
            ToggleButton.FlatAppearance.BorderColor = Color.FromArgb(163, 217, 163);
        }

        private void SetButtonRed() {
            ToggleButton.ForeColor = Color.FromArgb(166, 125, 125);
            ToggleButton.BackColor = Color.FromArgb(255, 192, 192);
            ToggleButton.FlatAppearance.BorderColor = Color.FromArgb(217, 163, 163);
        }

        private void SetButtonGrey() {
            ToggleButton.ForeColor = Color.FromArgb(56,56,56);
            ToggleButton.BackColor = Color.FromArgb(169, 169, 169);
            ToggleButton.FlatAppearance.BorderColor = Color.FromArgb(56, 56, 56);
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

        private void PauseLogsCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (!PauseLogsCheckBox.Checked) {
                foreach (string log in logBuffer)
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

        private void SendButton_Click(object sender, EventArgs e) {
            
            SendOutPacket();
        }

        public static void SetDestinationValue(string val) {
            instance.Destination.Invoke((MethodInvoker)delegate {
                instance.Destination.Text = val;
            });
        }

        private void Throughput_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void Destination_TextChanged(object sender, EventArgs e) {

        }

        /*
        public static void AddDestinations() {
            instance.Destination.Invoke((MethodInvoker)delegate {
                //foreach (Tuple<int, string, int> t in ConfigLoader.otherHosts) {
                foreach (Tuple<int, string, int, int> t in ConfigLoader.otherAvailableHosts) {
                    string key = "Host" + (t.Item1) + " #" + (t.Item4);
                    instance.Destination.Items.Add(key);
                    instance.hostIPs.Add(key, t.Item2 + "/24");
                }

            });
        }
        */
    }
}
