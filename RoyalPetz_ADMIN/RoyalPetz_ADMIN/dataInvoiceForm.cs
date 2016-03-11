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
using System.Globalization;


namespace RoyalPetz_ADMIN
{
    public partial class dataInvoiceForm : Form
    {
        private int originModuleID = 0;
        private globalUtilities gutil = new globalUtilities();

        public dataInvoiceForm()
        {
            InitializeComponent();
        }

        public dataInvoiceForm(int moduleID)
        {
            InitializeComponent();
            originModuleID = moduleID;
        }

        private void dataInvoiceDataGridView_DoubleClick(object sender, EventArgs e)
        {
            switch(originModuleID)
            {
                case globalConstants.PEMBAYARAN_PIUTANG:
                    pembayaranPiutangForm pembayaranForm = new pembayaranPiutangForm();
                    pembayaranForm.ShowDialog(this);
                    break;

                case globalConstants.RETUR_PENJUALAN:
                    dataReturPenjualanForm displayedForm = new dataReturPenjualanForm(originModuleID);
                    displayedForm.ShowDialog(this);
                    break;

            }

        }

        private void fillInPelangganCombo()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT * FROM MASTER_CUSTOMER WHERE CUSTOMER_ACTIVE = 1";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    pelangganCombo.Items.Clear();
                    pelangganComboHidden.Items.Clear();
                    while (rdr.Read())
                    {
                        pelangganCombo.Items.Add(rdr.GetString("CUSTOMER_FULL_NAME"));
                        pelangganComboHidden.Items.Add(rdr.GetString("CUSTOMER_ID"));
                    }
                }
            }
        }

        private void displayButton_Click(object sender, EventArgs e)
        {

        }

        private void dataInvoiceForm_Load(object sender, EventArgs e)
        {
            gutil.reArrangeTabOrder(this);
        }
    }
}
