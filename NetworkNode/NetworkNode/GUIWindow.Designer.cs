namespace NetworkNode {
    partial class GUIWindow {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent() {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.NameLabel = new System.Windows.Forms.Label();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.LogsTab = new System.Windows.Forms.TabPage();
            this.RoutingTableTab = new System.Windows.Forms.TabPage();
            this.RoutingTable = new System.Windows.Forms.DataGridView();
            this.ClearButton = new System.Windows.Forms.Button();
            this.PauseLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.ownIP = new System.Windows.Forms.Label();
            this.TabControl.SuspendLayout();
            this.LogsTab.SuspendLayout();
            this.RoutingTableTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RoutingTable)).BeginInit();
            this.SuspendLayout();
            // 
            // NameLabel
            // 
            this.NameLabel.Font = new System.Drawing.Font("Roboto Light", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.NameLabel.Location = new System.Drawing.Point(0, 0);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(147, 42);
            this.NameLabel.TabIndex = 0;
            this.NameLabel.Text = "Router";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LogBox
            // 
            this.LogBox.BackColor = System.Drawing.Color.White;
            this.LogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LogBox.Location = new System.Drawing.Point(3, 3);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogBox.Size = new System.Drawing.Size(357, 400);
            this.LogBox.TabIndex = 7;
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.LogsTab);
            this.TabControl.Controls.Add(this.RoutingTableTab);
            this.TabControl.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.TabControl.Location = new System.Drawing.Point(7, 45);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(371, 434);
            this.TabControl.TabIndex = 8;
            // 
            // LogsTab
            // 
            this.LogsTab.Controls.Add(this.LogBox);
            this.LogsTab.Location = new System.Drawing.Point(4, 24);
            this.LogsTab.Name = "LogsTab";
            this.LogsTab.Padding = new System.Windows.Forms.Padding(3);
            this.LogsTab.Size = new System.Drawing.Size(363, 406);
            this.LogsTab.TabIndex = 0;
            this.LogsTab.Text = "Logs";
            this.LogsTab.UseVisualStyleBackColor = true;
            // 
            // RoutingTableTab
            // 
            this.RoutingTableTab.Controls.Add(this.RoutingTable);
            this.RoutingTableTab.Location = new System.Drawing.Point(4, 24);
            this.RoutingTableTab.Name = "RoutingTableTab";
            this.RoutingTableTab.Padding = new System.Windows.Forms.Padding(3);
            this.RoutingTableTab.Size = new System.Drawing.Size(363, 406);
            this.RoutingTableTab.TabIndex = 1;
            this.RoutingTableTab.Text = "Connection Table";
            this.RoutingTableTab.UseVisualStyleBackColor = true;
            // 
            // RoutingTable
            // 
            this.RoutingTable.AllowUserToAddRows = false;
            this.RoutingTable.AllowUserToDeleteRows = false;
            this.RoutingTable.AllowUserToResizeColumns = false;
            this.RoutingTable.AllowUserToResizeRows = false;
            this.RoutingTable.BackgroundColor = System.Drawing.Color.White;
            this.RoutingTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.RoutingTable.DefaultCellStyle = dataGridViewCellStyle1;
            this.RoutingTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RoutingTable.Location = new System.Drawing.Point(3, 3);
            this.RoutingTable.Name = "RoutingTable";
            this.RoutingTable.ReadOnly = true;
            this.RoutingTable.RowHeadersVisible = false;
            this.RoutingTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.RoutingTable.Size = new System.Drawing.Size(357, 400);
            this.RoutingTable.TabIndex = 0;
            // 
            // ClearButton
            // 
            this.ClearButton.Font = new System.Drawing.Font("Roboto Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ClearButton.Location = new System.Drawing.Point(268, 31);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(104, 30);
            this.ClearButton.TabIndex = 12;
            this.ClearButton.Text = "CLEAR LOGS";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // PauseLogsCheckBox
            // 
            this.PauseLogsCheckBox.AutoSize = true;
            this.PauseLogsCheckBox.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PauseLogsCheckBox.Location = new System.Drawing.Point(279, 7);
            this.PauseLogsCheckBox.Name = "PauseLogsCheckBox";
            this.PauseLogsCheckBox.Size = new System.Drawing.Size(93, 19);
            this.PauseLogsCheckBox.TabIndex = 13;
            this.PauseLogsCheckBox.Text = "Pause Logs";
            this.PauseLogsCheckBox.UseVisualStyleBackColor = true;
            this.PauseLogsCheckBox.CheckedChanged += new System.EventHandler(this.PauseLogsCheckBox_CheckedChanged);
            // 
            // ownIP
            // 
            this.ownIP.Font = new System.Drawing.Font("Roboto Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ownIP.Location = new System.Drawing.Point(137, 14);
            this.ownIP.Name = "ownIP";
            this.ownIP.Size = new System.Drawing.Size(125, 24);
            this.ownIP.TabIndex = 16;
            this.ownIP.Text = "255.255.255.255";
            this.ownIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GUIWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(384, 491);
            this.Controls.Add(this.ownIP);
            this.Controls.Add(this.PauseLogsCheckBox);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.NameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GUIWindow";
            this.Text = "Router";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.TabControl.ResumeLayout(false);
            this.LogsTab.ResumeLayout(false);
            this.LogsTab.PerformLayout();
            this.RoutingTableTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RoutingTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage LogsTab;
        private System.Windows.Forms.TabPage RoutingTableTab;
        private System.Windows.Forms.DataGridView RoutingTable;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.CheckBox PauseLogsCheckBox;
        private System.Windows.Forms.Label ownIP;
    }
}

