using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientNode {
    public partial class PopOutQuestionBox : Form {
        public PopOutQuestionBox() {
            InitializeComponent();
            button1.ForeColor = Color.FromArgb(125, 166, 125);
            button1.BackColor = Color.FromArgb(192, 255, 192);
            button1.FlatAppearance.BorderColor = Color.FromArgb(163, 217, 163);
            button2.ForeColor = Color.FromArgb(166, 125, 125);
            button2.BackColor = Color.FromArgb(255, 192, 192);
            button2.FlatAppearance.BorderColor = Color.FromArgb(217, 163, 163);
        }

        //nie
        private void button2_Click(object sender, EventArgs e) {

        }


        
        private void label1_Click(object sender, EventArgs e) {

        }

        public void labelText(String new_text) {
            label1.Text = new_text;
        }


        //tak
        private void button1_Click(object sender, EventArgs e) {
            GUIWindow.AcceptCall();

        }
    }
}
