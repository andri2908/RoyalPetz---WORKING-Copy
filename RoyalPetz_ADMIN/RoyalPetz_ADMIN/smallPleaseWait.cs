using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoyalPetz_ADMIN
{
    public partial class smallPleaseWait : Form
    {
        globalUtilities gUtil = new globalUtilities();
        public smallPleaseWait()
        {
            InitializeComponent();
            //label1.Text = "PLEASE WAIT";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = label1.Text + ".";
            gUtil.saveSystemDebugLog(0, "[MESSAGING] label1.text = " + label1.Text);

            if (label1.Text.Length > 20)
            { 
                label1.Text = "PLEASE WAIT";
                gUtil.saveSystemDebugLog(0, "[MESSAGING] reset label1.text");
            }

            //label1.Invalidate();
            //label1.Update();
            //label1.Refresh();
            //Application.DoEvents();
        }

        private void smallPleaseWait_Activated(object sender, EventArgs e)
        {
        }

        private void smallPleaseWait_Deactivate(object sender, EventArgs e)
        {
            //timer1.Stop();
        }

        private void smallPleaseWait_Load(object sender, EventArgs e)
        {
            //timer1.Start();
        }
    }
}
