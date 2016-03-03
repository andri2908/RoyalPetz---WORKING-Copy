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
    public partial class dataPOForm : Form
    {
        public dataPOForm()
        {
            InitializeComponent();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            purchaseOrderDetailForm displayedForm = new purchaseOrderDetailForm();
            displayedForm.ShowDialog(this);
        }
    }
}
