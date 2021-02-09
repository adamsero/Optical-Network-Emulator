namespace ControlCenter {
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
            this.TabControl = new System.Windows.Forms.TabControl();
            this.Logs = new System.Windows.Forms.TabPage();
            this.LogBox1 = new System.Windows.Forms.TextBox();
            this.ChannelTab = new System.Windows.Forms.TabPage();
            this.ChannelTable = new System.Windows.Forms.DataGridView();
            this.ClearButton = new System.Windows.Forms.Button();
            this.PauseLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.KeepAliveCheckBox = new System.Windows.Forms.CheckBox();
            this.TabControl.SuspendLayout();
            this.Logs.SuspendLayout();
            this.ChannelTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChannelTable)).BeginInit();
            this.SuspendLayout();
            // 
            // NameLabel
            // 
            this.NameLabel.Font = new System.Drawing.Font("Roboto Light", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.NameLabel.Location = new System.Drawing.Point(0, 0);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(316, 40);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.Text = "Control Center";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.Logs);
            this.TabControl.Controls.Add(this.ChannelTab);
            this.TabControl.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.TabControl.Location = new System.Drawing.Point(9, 43);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(612, 436);
            this.TabControl.TabIndex = 5;
            // 
            // Logs
            // 
            this.Logs.Controls.Add(this.LogBox1);
            this.Logs.Location = new System.Drawing.Point(4, 24);
            this.Logs.Name = "Logs";
            this.Logs.Padding = new System.Windows.Forms.Padding(3);
            this.Logs.Size = new System.Drawing.Size(634, 408);
            this.Logs.TabIndex = 0;
            this.Logs.Text = "Logs";
            this.Logs.UseVisualStyleBackColor = true;
            // 
            // LogBox1
            // 
            this.LogBox1.BackColor = System.Drawing.Color.White;
            this.LogBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogBox1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LogBox1.Location = new System.Drawing.Point(3, 3);
            this.LogBox1.Multiline = true;
            this.LogBox1.Name = "LogBox1";
            this.LogBox1.ReadOnly = true;
            this.LogBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogBox1.Size = new System.Drawing.Size(628, 402);
            this.LogBox1.TabIndex = 8;
            this.LogBox1.TextChanged += new System.EventHandler(this.LogBox1_TextChanged);
            // 
            // ChannelTab
            // 
            this.ChannelTab.Controls.Add(this.ChannelTable);
            this.ChannelTab.Location = new System.Drawing.Point(4, 24);
            this.ChannelTab.Name = "ChannelTab";
            this.ChannelTab.Size = new System.Drawing.Size(604, 408);
            this.ChannelTab.TabIndex = 2;
            this.ChannelTab.Text = "Channels";
            this.ChannelTab.UseVisualStyleBackColor = true;
            // 
            // ChannelTable
            // 
            this.ChannelTable.AllowUserToAddRows = false;
            this.ChannelTable.AllowUserToDeleteRows = false;
            this.ChannelTable.AllowUserToResizeColumns = false;
            this.ChannelTable.AllowUserToResizeRows = false;
            this.ChannelTable.BackgroundColor = System.Drawing.Color.White;
            this.ChannelTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ChannelTable.DefaultCellStyle = dataGridViewCellStyle1;
            this.ChannelTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChannelTable.Location = new System.Drawing.Point(0, 0);
            this.ChannelTable.Name = "ChannelTable";
            this.ChannelTable.ReadOnly = true;
            this.ChannelTable.RowHeadersVisible = false;
            this.ChannelTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ChannelTable.Size = new System.Drawing.Size(604, 408);
            this.ChannelTable.TabIndex = 1;
            this.ChannelTable.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ChannelTable_CellContentClick);
            // 
            // ClearButton
            // 
            this.ClearButton.Font = new System.Drawing.Font("Roboto Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ClearButton.Location = new System.Drawing.Point(507, 31);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(110, 30);
            this.ClearButton.TabIndex = 14;
            this.ClearButton.Text = "CLEAR LOGS";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // PauseLogsCheckBox
            // 
            this.PauseLogsCheckBox.AutoSize = true;
            this.PauseLogsCheckBox.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PauseLogsCheckBox.Location = new System.Drawing.Point(519, 6);
            this.PauseLogsCheckBox.Name = "PauseLogsCheckBox";
            this.PauseLogsCheckBox.Size = new System.Drawing.Size(93, 19);
            this.PauseLogsCheckBox.TabIndex = 15;
            this.PauseLogsCheckBox.Text = "Pause Logs";
            this.PauseLogsCheckBox.UseVisualStyleBackColor = true;
            // 
            // KeepAliveCheckBox
            // 
            this.KeepAliveCheckBox.AutoSize = true;
            this.KeepAliveCheckBox.Font = new System.Drawing.Font("Roboto Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.KeepAliveCheckBox.Location = new System.Drawing.Point(392, 6);
            this.KeepAliveCheckBox.Name = "KeepAliveCheckBox";
            this.KeepAliveCheckBox.Size = new System.Drawing.Size(121, 19);
            this.KeepAliveCheckBox.TabIndex = 16;
            this.KeepAliveCheckBox.Text = "Show Keep-Alive";
            this.KeepAliveCheckBox.UseVisualStyleBackColor = true;
            // 
            // GUIWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(624, 491);
            this.Controls.Add(this.KeepAliveCheckBox);
            this.Controls.Add(this.PauseLogsCheckBox);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.NameLabel);
            this.Name = "GUIWindow";
            this.Text = "ControlCenter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.TabControl.ResumeLayout(false);
            this.Logs.ResumeLayout(false);
            this.Logs.PerformLayout();
            this.ChannelTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ChannelTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage Logs;
        private System.Windows.Forms.TextBox LogBox1;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.CheckBox PauseLogsCheckBox;
        private System.Windows.Forms.CheckBox KeepAliveCheckBox;
        private System.Windows.Forms.TabPage ChannelTab;
        private System.Windows.Forms.DataGridView ChannelTable;
    }
}

