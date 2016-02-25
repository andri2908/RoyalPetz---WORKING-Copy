﻿using System;
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
    public partial class dataMutasiBarangDetailForm : Form
    {
        private int originModuleID = 0;
        private int subModuleID = 0;
        private int selectedROID = 0;
        private int selectedBranchFromID = 0;
        private int selectedBranchToID = 0;
        private string selectedROInvoice = "";
        private bool isLoading = false;
        private double globalTotalValue = 0;
        private bool directMutasiBarang = false;
        private string previousInput = "";

        private Data_Access DS = new Data_Access();
        private List<string> detailRequestQtyApproved = new List<string>();

        private globalUtilities gUtil = new globalUtilities();
        private CultureInfo culture = new CultureInfo("id-ID");

        public dataMutasiBarangDetailForm()
        {
            InitializeComponent();
        }

        public dataMutasiBarangDetailForm(int moduleID, int roID = 0)
        {
            InitializeComponent();

            originModuleID = moduleID;
            selectedROID = roID;

            switch (originModuleID)
            {
                case globalConstants.CEK_DATA_MUTASI:
                    reprintButton.Visible = false;
                    selectedROID = roID;
                    break;

                case globalConstants.REPRINT_PERMINTAAN_BARANG:
                    approveButton.Visible = false;
                    rejectButton.Visible = false;
                    detailRequestOrderDataGridView.ReadOnly = true;
                    break;

                case globalConstants.MUTASI_BARANG:
                    approveButton.Text = "SAVE MUTASI";
                    reprintButton.Text = "REPRINT DATA MUTASI";
                    rejectButton.Visible = false;

                    directMutasiBarang = true;
                    break;
            }           
        }

        private void calculateTotal()
        {
            double total = 0;

            for (int i = 0; i < detailRequestOrderDataGridView.Rows.Count; i++)
            {
                total = total + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value);
            }

            globalTotalValue = total;

            if (!directMutasiBarang)
                totalApproved.Text = "Rp. " + total.ToString();
            else
                totalLabel.Text = "Rp. " + total.ToString();
        }

        private bool stockIsEnough(string productID, double qtyRequested)
        {
            bool result = false;
            double stockQty = 0;

            stockQty = Convert.ToDouble(DS.getDataSingleValue("SELECT (PRODUCT_STOCK_QTY - PRODUCT_LIMIT_STOCK) FROM MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'"));

            if (stockQty >= qtyRequested)
                result = true;

            return result;
        }

        private void detailRequestOrderDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int qtyPosition;

            if (!directMutasiBarang)
                qtyPosition = 2;
            else
                qtyPosition = 1;

            if (detailRequestOrderDataGridView.CurrentCell.ColumnIndex == 0 && e.Control is ComboBox)
            {
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }

            if (detailRequestOrderDataGridView.CurrentCell.ColumnIndex == qtyPosition && e.Control is TextBox)
            {
                TextBox textBox = e.Control as TextBox;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private double getHPP(string productID)
        {
            double result = 0;

            result = Convert.ToDouble(DS.getDataSingleValue("SELECT PRODUCT_BASE_PRICE FROM MASTER_PRODUCT WHERE PRODUCT_ID = '"+productID+"'"));
            return result;
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rowSelectedIndex = 0;
            double productQty = 0;
            double hppValue = 0;
            double subTotal = 0;
            int cmbSelectedIndex = 0;
            string productID = "";

            if (isLoading)
                return;

            DataGridViewComboBoxEditingControl dataGridViewComboBoxEditingControl = sender as DataGridViewComboBoxEditingControl;

            rowSelectedIndex = detailRequestOrderDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailRequestOrderDataGridView.Rows[rowSelectedIndex];
            cmbSelectedIndex = dataGridViewComboBoxEditingControl.SelectedIndex;

            // get product id
            productID = productIDHiddenCombo.Items[cmbSelectedIndex].ToString();
            selectedRow.Cells["productID"].Value = productID;

            // get hpp

            
            productQty = Convert.ToDouble(selectedRow.Cells["qty"].Value);
            hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
            subTotal = Math.Round((hppValue * productQty), 2);

            selectedRow.Cells["subTotal"].Value = subTotal;

            calculateTotal();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            int rowSelectedIndex = 0;
            double productQty = 0;
            double hppValue = 0;
            double subTotal = 0;

            if (isLoading)
                return;

            DataGridViewTextBoxEditingControl dataGridViewTextBoxEditingControl = sender as DataGridViewTextBoxEditingControl;

            rowSelectedIndex = detailRequestOrderDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailRequestOrderDataGridView.Rows[rowSelectedIndex];

            if (!directMutasiBarang)
            {
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL) && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                {
                    productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);

                    if (stockIsEnough(selectedRow.Cells["productID"].Value.ToString(), productQty))
                    {
                        detailRequestQtyApproved[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                    }
                    else
                    {
                        dataGridViewTextBoxEditingControl.Text = detailRequestQtyApproved[rowSelectedIndex];
                    }
                }
                else
                {
                    dataGridViewTextBoxEditingControl.Text = detailRequestQtyApproved[rowSelectedIndex];
                }
            }
            else
            {
                previousInput = "";

                if (detailRequestQtyApproved.Count < rowSelectedIndex + 1)
                {
                    if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL) && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                    {
                        productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);

                        if (stockIsEnough(selectedRow.Cells["productID"].Value.ToString(), productQty))
                        {
                            detailRequestQtyApproved[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                        }
                        else
                        {
                            dataGridViewTextBoxEditingControl.Text = detailRequestQtyApproved[rowSelectedIndex];
                        }

                        //detailRequestQtyApproved.Add(dataGridViewTextBoxEditingControl.Text);
                    }
                    else
                    {
                        dataGridViewTextBoxEditingControl.Text = previousInput;
                    }
                }
                else
                {
                    if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL))
                    {
                        detailRequestQtyApproved[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                    }
                    else
                    {
                        dataGridViewTextBoxEditingControl.Text = detailRequestQtyApproved[rowSelectedIndex];
                    }
                }

            }

            productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);

            hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
            subTotal = Math.Round((hppValue * productQty), 2);

            selectedRow.Cells["subTotal"].Value = subTotal;

            calculateTotal();
        }

        private void loadDataHeaderRO()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DS.mySqlConnect();

            sqlCommand = "SELECT * FROM REQUEST_ORDER_HEADER WHERE ID = " + selectedROID;

            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        ROInvoiceTextBox.Text = rdr.GetString("RO_INVOICE");
                        RODateTimePicker.Value = rdr.GetDateTime("RO_DATETIME");
                        ROExpiredDateTimePicker.Value = rdr.GetDateTime("RO_EXPIRED");
                        selectedBranchFromID = rdr.GetInt32("RO_BRANCH_ID_FROM");
                        selectedBranchToID = rdr.GetInt32("RO_BRANCH_ID_TO");

                        selectedROInvoice = rdr.GetString("RO_INVOICE");

                        totalLabel.Text = "Rp. " + rdr.GetString("RO_TOTAL");
                        totalApproved.Text = "Rp. " + rdr.GetString("RO_TOTAL");
                        globalTotalValue = rdr.GetDouble("RO_TOTAL");
                    }

                    rdr.Close();
                }
            }
        }

        private void loadDataDetailRO()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";
            string productName = "";

            sqlCommand = "SELECT R.*, M.PRODUCT_NAME FROM REQUEST_ORDER_DETAIL R, MASTER_PRODUCT M WHERE R.RO_INVOICE = '" + selectedROInvoice + "' AND R.PRODUCT_ID = M.PRODUCT_ID";

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    productName = rdr.GetString("PRODUCT_NAME");
                    detailRequestOrderDataGridView.Rows.Add(productName, rdr.GetString("RO_QTY"), rdr.GetString("RO_QTY"), rdr.GetString("PRODUCT_BASE_PRICE"), rdr.GetString("RO_SUBTOTAL"), rdr.GetString("PRODUCT_ID"));
                    detailRequestQtyApproved.Add(rdr.GetString("RO_QTY"));
                }

                rdr.Close();
            }
        }

        private string getBranchName(int branchID)
        {
            string result = "";

            result = DS.getDataSingleValue("SELECT BRANCH_NAME FROM MASTER_BRANCH WHERE BRANCH_ID = " + branchID).ToString();

            return result;
        }

        private bool isNewRORequest()
        {
            bool result = false;

            if (1 == Convert.ToInt32(DS.getDataSingleValue("SELECT RO_ACTIVE FROM REQUEST_ORDER_HEADER WHERE RO_INVOICE = '" + selectedROInvoice + "'")))
                result = true;

            return result;
        }

        private string getNoMutasi()
        {
            string result = "";

            result = DS.getDataSingleValue("SELECT PM_INVOICE FROM PRODUCTS_MUTATION_HEADER WHERE RO_INVOICE = '" + selectedROInvoice + "'").ToString();
            return result;
        }

        private DateTime getPMDateTimeValue()
        {
            DateTime result;

            result = Convert.ToDateTime(DS.getDataSingleValue("SELECT PM_DATETIME FROM PRODUCTS_MUTATION_HEADER WHERE RO_INVOICE = '" + selectedROInvoice + "'"));
            return result;
        }

        private void fillInBranchCombo(ComboBox comboToFill, ComboBox hiddenComboToFill)
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DS.mySqlConnect();

            sqlCommand = "SELECT BRANCH_ID, BRANCH_NAME FROM MASTER_BRANCH WHERE BRANCH_ACTIVE = 1";

            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    comboToFill.Items.Clear();
                    comboToFill.Text = "";

                    hiddenComboToFill.Items.Clear();
                    hiddenComboToFill.Text = "";
                    while (rdr.Read())
                    {
                        hiddenComboToFill.Items.Add(rdr.GetString("BRANCH_ID"));
                        comboToFill.Items.Add(rdr.GetString("BRANCH_NAME"));
                    }

                    rdr.Close();
                }
            }
        }

        private void addDataToProductNameCombo(DataGridViewComboBoxColumn comboColumn)
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT PRODUCT_ID, PRODUCT_NAME FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1 ORDER BY PRODUCT_NAME ASC";

            productIDHiddenCombo.Items.Clear();
            comboColumn.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    comboColumn.Items.Add(rdr.GetString("PRODUCT_NAME"));
                    productIDHiddenCombo.Items.Add(rdr.GetString("PRODUCT_ID"));
                }
            }

            rdr.Close();

        }

        private void addColumnToDetailDataGrid()
        {
            DataGridViewTextBoxColumn productNameColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn qtyReqColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn qtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn hppColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn subtotalColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn productIDColumn = new DataGridViewTextBoxColumn();

            DataGridViewComboBoxColumn productNameComboColumn = new DataGridViewComboBoxColumn();

            if (!directMutasiBarang)
            {
                productNameColumn.Name = "productName";
                productNameColumn.HeaderText = "NAMA PRODUK";
                productNameColumn.ReadOnly = true;
                productNameColumn.Width = 300;
                detailRequestOrderDataGridView.Columns.Add(productNameColumn);

                qtyReqColumn.Name = "qtyRequest";
                qtyReqColumn.HeaderText = "QTY REQ";
                qtyReqColumn.ReadOnly = true;
                qtyReqColumn.Width = 150;
                detailRequestOrderDataGridView.Columns.Add(qtyReqColumn);

            }
            else
            {
                productNameComboColumn.Name = "productName";
                productNameComboColumn.HeaderText = "NAMA PRODUK";
                productNameComboColumn.Width = 300;
                addDataToProductNameCombo(productNameComboColumn);

                detailRequestOrderDataGridView.Columns.Add(productNameComboColumn);
            }

            qtyColumn.Name = "qty";
            qtyColumn.HeaderText = "QTY";
            qtyColumn.Width = 150;
            detailRequestOrderDataGridView.Columns.Add(qtyColumn);

            hppColumn.Name = "hpp";
            hppColumn.HeaderText = "HARGA POKOK";
            hppColumn.Width = 200;
            hppColumn.ReadOnly = true;
            detailRequestOrderDataGridView.Columns.Add(hppColumn);

            subtotalColumn.Name = "subtotal";
            subtotalColumn.HeaderText = "SUBTOTAL";
            subtotalColumn.Width = 200;
            subtotalColumn.ReadOnly = true;
            detailRequestOrderDataGridView.Columns.Add(subtotalColumn);

            productIDColumn.Name = "productID";
            productIDColumn.HeaderText = "productID";
            productIDColumn.Width = 100;
            productIDColumn.Visible = false;
            detailRequestOrderDataGridView.Columns.Add(productIDColumn);
        }

        private void dataMutasiBarangDetailForm_Load(object sender, EventArgs e)
        {
            errorLabel.Text = "";

            isLoading = true;

            addColumnToDetailDataGrid();

            if (!directMutasiBarang)
            { 
                loadDataHeaderRO();
                loadDataDetailRO();

                if (isNewRORequest())
                {
                    subModuleID = globalConstants.NEW_PRODUCT_MUTATION;

                    approveButton.Visible = true;
                    rejectButton.Visible = true;
                    reprintButton.Visible = false;
                }
                else
                {
                    //subModuleID = globalConstants.EDIT_PRODUCT_MUTATION;
            
                    noMutasiTextBox.ReadOnly = true;
                    noMutasiTextBox.Text = getNoMutasi();
                
                    PMDateTimePicker.Enabled = false;
                    PMDateTimePicker.Value = getPMDateTimeValue();

                    detailRequestOrderDataGridView.ReadOnly = true;

                    approveButton.Visible = false;
                    rejectButton.Visible = false;
                    reprintButton.Visible = true;

                    totalApproved.Visible = false;
                    totalApprovedLabel.Visible = false;
                    label13.Visible = false;
                }

                branchFromCombo.Text = getBranchName(selectedBranchFromID);
                branchToCombo.Text = getBranchName(selectedBranchToID);
                branchFromCombo.Enabled = false;
                branchToCombo.Enabled = false;
            }
            else
            {
                subModuleID = globalConstants.NEW_PRODUCT_MUTATION;

                branchFromCombo.Enabled = true;
                branchToCombo.Enabled = true;

                fillInBranchCombo(branchFromCombo, branchFromComboHidden);
                fillInBranchCombo(branchToCombo, branchToComboHidden);

                detailRequestOrderDataGridView.AllowUserToAddRows = true;
            }
            
            isLoading = false;

            detailRequestOrderDataGridView.EditingControlShowing += detailRequestOrderDataGridView_EditingControlShowing;

            gUtil.reArrangeTabOrder(this);
        }

        private bool saveDataTransaction()
        {
            bool result = false;
            string sqlCommand = "";

            string roInvoice = "0";
            string noMutasi = "";
            int branchIDFrom = 0;
            int branchIDTo = 0;
            string PMDateTime = "";
            double PMTotal = 0;
            DateTime selectedPMDate;

            noMutasi = noMutasiTextBox.Text;
            roInvoice = ROInvoiceTextBox.Text;
            branchIDFrom = selectedBranchFromID;
            branchIDTo = selectedBranchToID;
            selectedPMDate = PMDateTimePicker.Value;
            PMDateTime = String.Format(culture, "{0:dd-MM-yyyy}", selectedPMDate);

            PMTotal = globalTotalValue;

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                switch (subModuleID)
                {
                    case globalConstants.NEW_PRODUCT_MUTATION:
                        // SAVE HEADER TABLE
                        sqlCommand = "INSERT INTO PRODUCTS_MUTATION_HEADER (PM_INVOICE, BRANCH_ID_FROM, BRANCH_ID_TO, PM_DATETIME, PM_TOTAL, RO_INVOICE) VALUES " +
                                            "('" + noMutasi + "', " + branchIDFrom + ", " + branchIDTo + ", STR_TO_DATE('" + PMDateTime + "', '%d-%m-%Y'), " + PMTotal + ", '" + roInvoice + "')";
                        DS.executeNonQueryCommand(sqlCommand);

                        // SAVE DETAIL TABLE
                        for (int i = 0; i < detailRequestOrderDataGridView.Rows.Count; i++)
                        {
                            sqlCommand = "INSERT INTO PRODUCTS_MUTATION_DETAIL (PM_INVOICE, PRODUCT_ID, PRODUCT_BASE_PRICE, PRODUCT_QTY, PM_SUBTOTAL) VALUES " +
                                                "('" + noMutasi + "', '" + detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value.ToString() + "', " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["hpp"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["qty"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value) + ")";

                            DS.executeNonQueryCommand(sqlCommand);
                        }

                        if (!directMutasiBarang)
                        { 
                            // UPDATE REQUEST ORDER HEADER TABLE
                            sqlCommand = "UPDATE REQUEST_ORDER_HEADER SET RO_ACTIVE = 0 WHERE RO_INVOICE = '" + roInvoice + "'";
                            DS.executeNonQueryCommand(sqlCommand);
                        }
                        break;
                }

                DS.commit();
            }
            catch (Exception e)
            {
                try
                {
                    DS.rollBack();
                }
                catch (MySqlException ex)
                {
                    if (DS.getMyTransConnection() != null)
                    {
                        MessageBox.Show("An exception of type " + ex.GetType() +
                                          " was encountered while attempting to roll back the transaction.");
                    }
                }

                MessageBox.Show("An exception of type " + e.GetType() +
                                  " was encountered while inserting the data.");
                MessageBox.Show("Neither record was written to database.");
            }
            finally
            {
                DS.mySqlClose();
                result = true;
            }

            return result;
        }

        private bool dataValidated()
        {
            if (noMutasiTextBox.Text.Length <= 0)
            {
                errorLabel.Text = "NO MUTASI TIDAK BOLEH KOSONG";
                return false;
            }

            return true;
        }

        private bool saveData()
        {
            if (dataValidated())
                return saveDataTransaction();

            return false;
        }

        private void approveButton_Click(object sender, EventArgs e)
        {
            if (saveData())
            {
                MessageBox.Show("SUCCESS");

                detailRequestOrderDataGridView.ReadOnly = true;
                approveButton.Visible = false;
                rejectButton.Visible = false;

                reprintButton.Visible = true;
            }
        }

        private bool noMutasiExist()
        {
            bool result = false;

            if (0 < Convert.ToInt32(DS.getDataSingleValue("SELECT COUNT(1) FROM PRODUCTS_MUTATION_HEADER WHERE PM_INVOICE = '" + noMutasiTextBox.Text + "'")))
                result = true;

            return result;
        }

        private void noMutasiTextBox_TextChanged(object sender, EventArgs e)
        {
            noMutasiTextBox.Text = noMutasiTextBox.Text.Trim();

            if (noMutasiExist() && (subModuleID == globalConstants.NEW_PRODUCT_MUTATION))
            {
                errorLabel.Text = "NO MUTASI SUDAH ADA";
                noMutasiTextBox.Focus();
            }
        }
    }
}
