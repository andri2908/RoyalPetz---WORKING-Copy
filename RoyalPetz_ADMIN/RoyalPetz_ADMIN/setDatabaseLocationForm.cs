using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace RoyalPetz_ADMIN
{
    public partial class setDatabaseLocationForm : Form
    {
        private globalUtilities gutil = new globalUtilities();
        private Data_Access DS = new Data_Access();

        public setDatabaseLocationForm()
        {
            InitializeComponent();
        }

        private void serverIPRadioButton_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void localhostRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void serverIPRadioButton_Click(object sender, EventArgs e)
        {
            if (serverIPRadioButton.Checked)
            {
                ipAddressMaskedTextbox.Enabled = true;
                localhostRadioButton.Checked = false;
                serverIPRadioButton.Checked = true;
            }
            else
            {
                ipAddressMaskedTextbox.Enabled = false;
                localhostRadioButton.Checked = true;
                serverIPRadioButton.Checked = false;
            }
        }

        private void localhostRadioButton_Click(object sender, EventArgs e)
        {
            if (localhostRadioButton.Checked)
            {
                ipAddressMaskedTextbox.Enabled = false;
                localhostRadioButton.Checked = true;
                serverIPRadioButton.Checked = false;
            }
            else
            {
                ipAddressMaskedTextbox.Enabled = true;
                localhostRadioButton.Checked = false;
                serverIPRadioButton.Checked = true;
            }
        }

        private bool saveData()
        {
            bool result = true;

            return result;
        }

        private void loadData()
        {

        }

        private void setDatabaseLocationForm_Load(object sender, EventArgs e)
        {
            gutil.reArrangeTabOrder(this);
        }

        private void setDatabaseLocationForm_Activated(object sender, EventArgs e)
        {
            //if need something
        }
    }
}
