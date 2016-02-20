﻿using System;
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
    public partial class dataInvoiceForm : Form
    {
        private int originModuleID = 0;

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

                default:
                    dataReturPenjualanForm displayedForm = new dataReturPenjualanForm();
                    displayedForm.ShowDialog(this);
                    break;

            }



        }
    }
}
