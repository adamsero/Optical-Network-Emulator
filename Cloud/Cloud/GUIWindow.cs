using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Cloud {
    public partial class GUIWindow : Form {

        private static GUIWindow instance;
        private readonly List<string> logBuffer = new List<string>();

        public GUIWindow() {
            InitializeComponent();
            instance = this;
            FillConnectionTable();
        }

        private void OnClosing(Object sender, FormClosingEventArgs e) {
            Environment.Exit(0);
        }

        private void FillConnectionTable() {

            RoutingTable.ColumnCount = 5;
            RoutingTable.Columns[0].Name = "Address A";
            RoutingTable.Columns[1].Name = "Port A";
            RoutingTable.Columns[2].Name = "Address B";
            RoutingTable.Columns[3].Name = "Port B";
            RoutingTable.Columns[4].Name = "Working";

            foreach(Tuple<int, String, int, String, int> tuple in Program.connectionTable) {
                String[] row = { tuple.Item2, tuple.Item3.ToString(), tuple.Item4, tuple.Item5.ToString(), "True" };
                RoutingTable.Rows.Add(row);
            }

            int index = 1;
            foreach (DataGridViewColumn column in RoutingTable.Columns) {
                if (index == 1 || index == 3) {
                    column.Width = RoutingTable.Size.Width / 4;
                }
                else {
                    column.Width = RoutingTable.Size.Width / 6;
                }
                index++;
            }
        }

        //private void RoutingTable_CellEnter(object sender, DataGridViewCellEventArgs e) {
        //    int selectedCellCount = RoutingTable.GetCellCount(DataGridViewElementStates.Selected);
        //    if (selectedCellCount != 1) {
        //        DisableButton.Enabled = false;
        //        EnableButton.Enabled = false;
        //        return;
        //    }

        //    DataGridViewCell selectedCell = RoutingTable.SelectedCells[0];
        //    if(selectedCell.ColumnIndex != 4) {
        //        DisableButton.Enabled = false;
        //        EnableButton.Enabled = false;
        //    } else {
        //        if (selectedCell.Value.Equals("True")) {
        //            DisableButton.Enabled = true;
        //            EnableButton.Enabled = false;
        //        } else {
        //            DisableButton.Enabled = false;
        //            EnableButton.Enabled = true;
        //        }
        //    }
        //}

        //private void DisableButton_Click(object sender, EventArgs e) {
        //    DataGridViewCell selectedCell = RoutingTable.SelectedCells[0];
        //    selectedCell.Value = "False";
        //    DisableButton.Enabled = false;
        //    EnableButton.Enabled = true;

        //    foreach (Tuple<int, String, int, String, int> connection in Program.connectionTable) {
        //        if (connection.Item1 == selectedCell.RowIndex + 1) {
        //            Program.brokenConnections.AddLast(connection);
        //            break;
        //        }
        //    }
        //}

        //private void EnableButton_Click(object sender, EventArgs e) {
        //    DataGridViewCell selectedCell = RoutingTable.SelectedCells[0];
        //    selectedCell.Value = "True";
        //    DisableButton.Enabled = true;
        //    EnableButton.Enabled = false;

        //    foreach (Tuple<int, String, int, String, int> brokenConnection in Program.brokenConnections) {
        //        if (brokenConnection.Item1 == selectedCell.RowIndex + 1) {
        //            Program.brokenConnections.Remove(brokenConnection);
        //            break;
        //        }
        //    }
        //}

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
