namespace Cloud {
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
            this.label1 = new System.Windows.Forms.Label();
            this.RoutingTable = new System.Windows.Forms.DataGridView();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.Logs = new System.Windows.Forms.TabPage();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.ConnectonTable = new System.Windows.Forms.TabPage();
            this.ClearButton = new System.Windows.Forms.Button();
            this.PauseLogsCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.RoutingTable)).BeginInit();
            this.TabControl.SuspendLayout();
            this.Logs.SuspendLayout();
            this.ConnectonTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Roboto Light", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 42);
            this.label1.TabIndex = 0;
            this.label1.Text = "Cloud";
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
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(241)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.RoutingTable.DefaultCellStyle = dataGridViewCellStyle1;
            this.RoutingTable.Location = new System.Drawing.Point(0, 0);
            this.RoutingTable.Name = "RoutingTable";
            this.RoutingTable.ReadOnly = true;
            this.RoutingTable.RowHeadersVisible = false;
            this.RoutingTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.RoutingTable.Size = new System.Drawing.Size(429, 352);
            this.RoutingTable.TabIndex = 1;
            //this.RoutingTable.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.RoutingTable_CellEnter);
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.Logs);
            this.TabControl.Controls.Add(this.ConnectonTable);
            this.TabControl.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.TabControl.Location = new System.Drawing.Point(12, 55);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(366, 424);
            this.TabControl.TabIndex = 4;
            // 
            // Logs
            // 
            this.Logs.Controls.Add(this.LogBox);
            this.Logs.Location = new System.Drawing.Point(4, 24);
            this.Logs.Name = "Logs";
            this.Logs.Padding = new System.Windows.Forms.Padding(3);
            this.Logs.Size = new System.Drawing.Size(358, 396);
            this.Logs.TabIndex = 0;
            this.Logs.Text = "Logs";
            this.Logs.UseVisualStyleBackColor = true;
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
            this.LogBox.Size = new System.Drawing.Size(352, 390);
            this.LogBox.TabIndex = 8;
            // 
            // ConnectonTable
            // 
            this.ConnectonTable.Controls.Add(this.RoutingTable);
            this.ConnectonTable.Location = new System.Drawing.Point(4, 24);
            this.ConnectonTable.Name = "ConnectonTable";
            this.ConnectonTable.Padding = new System.Windows.Forms.Padding(3);
            this.ConnectonTable.Size = new System.Drawing.Size(358, 352);
            this.ConnectonTable.TabIndex = 1;
            this.ConnectonTable.Text = "Connection Table";
            this.ConnectonTable.UseVisualStyleBackColor = true;
            // 
            // ClearButton
            // 
            this.ClearButton.Font = new System.Drawing.Font("Roboto Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ClearButton.Location = new System.Drawing.Point(268, 43);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(110, 30);
            this.ClearButton.TabIndex = 13;
            this.ClearButton.Text = "CLEAR LOGS";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // PauseLogsCheckBox
            // 
            this.PauseLogsCheckBox.AutoSize = true;
            this.PauseLogsCheckBox.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PauseLogsCheckBox.Location = new System.Drawing.Point(279, 12);
            this.PauseLogsCheckBox.Name = "PauseLogsCheckBox";
            this.PauseLogsCheckBox.Size = new System.Drawing.Size(93, 19);
            this.PauseLogsCheckBox.TabIndex = 14;
            this.PauseLogsCheckBox.Text = "Pause Logs";
            this.PauseLogsCheckBox.UseVisualStyleBackColor = true;
            this.PauseLogsCheckBox.CheckedChanged += new System.EventHandler(this.PauseLogsCheckBox_CheckedChanged);
            // 
            // GUIWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(384, 491);
            this.Controls.Add(this.PauseLogsCheckBox);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "GUIWindow";
            this.Text = "Cloud";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            ((System.ComponentModel.ISupportInitialize)(this.RoutingTable)).EndInit();
            this.TabControl.ResumeLayout(false);
            this.Logs.ResumeLayout(false);
            this.Logs.PerformLayout();
            this.ConnectonTable.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView RoutingTable;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage Logs;
        private System.Windows.Forms.TabPage ConnectonTable;
        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.CheckBox PauseLogsCheckBox;
    }
}

