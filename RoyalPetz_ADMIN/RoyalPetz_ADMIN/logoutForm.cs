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
    public partial class logoutForm : Form
    {
        public logoutForm()
        {
            InitializeComponent();
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            loginForm displayLoginForm = new loginForm(globalConstants.LOGOUT_FORM);
            displayLoginForm.ShowDialog(this);
        }

        private void endShiftButton_Click(object sender, EventArgs e)
        {
            loginForm displayLoginForm = new loginForm(globalConstants.LOGOUT_FORM);
            displayLoginForm.ShowDialog(this);
        }
    }
}
