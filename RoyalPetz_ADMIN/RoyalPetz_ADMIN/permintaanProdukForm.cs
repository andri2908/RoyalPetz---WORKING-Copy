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
using System.IO;

namespace RoyalPetz_ADMIN
{
    public partial class permintaanProdukForm : Form
    {
        private int originModuleID = 0;
        private int selectedBranchFromID = 0;
        private int selectedBranchToID = 0;
        private string previousInput = "";
        private double globalTotalValue = 0;
        private bool isLoading = false;

        private int selectedROID = 0;
        private string selectedROInvoice = "";

        private Data_Access DS = new Data_Access();
        private List<string> detailRequestQty = new List<string>();
        
        private globalUtilities gUtil = new globalUtilities();
        private CultureInfo culture = new CultureInfo("id-ID");

        public permintaanProdukForm()
        {
            InitializeComponent();
        }

        public permintaanProdukForm(int moduleID)
        {
            InitializeComponent();
            originModuleID = moduleID;
        }

        public permintaanProdukForm(int moduleID, int roID)
        {
            InitializeComponent();
            originModuleID = moduleID;
            selectedROID = roID;
        }

        private string getBranchName(int branchID)
        {
            string result = "";

            Data_Access tempDS = new Data_Access();
            result = tempDS.getDataSingleValue("SELECT ifnull(BRANCH_NAME, '') FROM MASTER_BRANCH WHERE BRANCH_ID = " + branchID).ToString();
            tempDS.mySqlClose();
            
            return result;
        }

        private string getProductName(int productID)
        {
            string result = "";

            result = DS.getDataSingleValue("SELECT ifnull(PRODUCT_NAME, '') FROM MASTER_PRODUCT WHERE PRODUCT_ID = " + productID).ToString();

            return result;
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
                        ROinvoiceTextBox.Text = rdr.GetString("RO_INVOICE");
                        RODateTimePicker.Value = rdr.GetDateTime("RO_DATETIME");
                        
                        //branchFromCombo.Text = getBranchName(rdr.GetInt32("RO_BRANCH_ID_FROM"));
                        selectedBranchFromID = rdr.GetInt32("RO_BRANCH_ID_FROM");
                        
                        //branchToCombo.Text = getBranchName(rdr.GetInt32("RO_BRANCH_ID_TO"));
                        selectedBranchToID = rdr.GetInt32("RO_BRANCH_ID_TO");
                        
                        durationTextBox.Text = Convert.ToInt32((rdr.GetDateTime("RO_EXPIRED") - rdr.GetDateTime("RO_DATETIME")).TotalDays).ToString();
                        totalLabel.Text = rdr.GetDouble("RO_TOTAL").ToString("C", culture);
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
                detailRequestOrderDataGridView.Rows.Clear();
                while (rdr.Read())
                {
                    productName = rdr.GetString("PRODUCT_NAME");
                    detailRequestOrderDataGridView.Rows.Add(rdr.GetString("PRODUCT_ID"), productName, rdr.GetString("RO_QTY"), rdr.GetString("PRODUCT_BASE_PRICE"), rdr.GetString("RO_SUBTOTAL"));
                }

                rdr.Close();
            }
        }

        private void calculateTotal()
        {
            double total = 0;

            for (int i = 0; i<detailRequestOrderDataGridView.Rows.Count;i++)
            {
                total = total + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value);
            }

            globalTotalValue = total;
            totalLabel.Text = total.ToString("C2", culture);
        }

        private void detailRequestOrderDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if ((detailRequestOrderDataGridView.CurrentCell.OwningColumn.Name == "productID" || detailRequestOrderDataGridView.CurrentCell.OwningColumn.Name == "productName") && e.Control is ComboBox)
            {
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }

            if (detailRequestOrderDataGridView.CurrentCell.OwningColumn.Name == "qty" && e.Control is TextBox)
            {
                TextBox textBox = e.Control as TextBox;
                textBox.TextChanged += TextBox_TextChanged;
            }
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

            previousInput = "";
            if ( detailRequestQty.Count < rowSelectedIndex+1 )
            {
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL)
                    && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                {
                    detailRequestQty.Add(dataGridViewTextBoxEditingControl.Text);
                }
                else
                {
                    dataGridViewTextBoxEditingControl.Text = previousInput;
                }
            }
            else
            {
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL) 
                    && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                {
                    detailRequestQty[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                }
                else
                {
                    dataGridViewTextBoxEditingControl.Text = detailRequestQty[rowSelectedIndex];
                }
            }

            try
            {
                productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);

                if (null != selectedRow.Cells["hpp"].Value)
                {
                    hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
                    subTotal = Math.Round((hppValue * productQty), 2);

                    selectedRow.Cells["subTotal"].Value = subTotal;
                }

                calculateTotal();
            }
            catch (Exception ex)
            {
                //dataGridViewTextBoxEditingControl.Text = previousInput;
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rowSelectedIndex = 0;
            double productQty = 0;
            double subTotal = 0;
            double hppValue = 0;
            int cmbSelectedIndex = 0;
            string productID = "";

            if (isLoading)
                return;
            
            DataGridViewComboBoxEditingControl dataGridViewComboBoxEditingControl = sender as DataGridViewComboBoxEditingControl;

            rowSelectedIndex = detailRequestOrderDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailRequestOrderDataGridView.Rows[rowSelectedIndex];
            cmbSelectedIndex = dataGridViewComboBoxEditingControl.SelectedIndex;

            if (cmbSelectedIndex < 0)
                return;

            // get product id
            DataGridViewComboBoxCell productIDComboCell = (DataGridViewComboBoxCell)selectedRow.Cells["productID"];
            DataGridViewComboBoxCell productNameComboCell = (DataGridViewComboBoxCell)selectedRow.Cells["productName"];

            productID = productIDComboCell.Items[cmbSelectedIndex].ToString();
            productIDComboCell.Value = productIDComboCell.Items[cmbSelectedIndex];
            productNameComboCell.Value = productNameComboCell.Items[cmbSelectedIndex];

            // get hpp
            hppValue = getHPPValue(productID);
            selectedRow.Cells["hpp"].Value = hppValue;

            productQty = Convert.ToDouble(selectedRow.Cells["qty"].Value);
            hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
            subTotal = Math.Round((hppValue * productQty), 2);

            selectedRow.Cells["subTotal"].Value = subTotal;

            calculateTotal();
        }
        
        private bool exportDataRO(string exportedFileName)
        {
            bool result = false;

            string sqlCommand = "";
            MySqlException internalEX = null;
            string roInvoice = "";
            int branchIDFrom = 0;
            int branchIDTo = 0;
            string roDateTime = "";
            double roTotal = 0;
            string roDateExpired = "";
            DateTime selectedRODate;
            DateTime expiredRODate;

            string selectedDate = RODateTimePicker.Value.ToShortDateString();
            selectedRODate = RODateTimePicker.Value;
            expiredRODate = selectedRODate.AddDays(Convert.ToDouble(durationTextBox.Text));

            roInvoice = ROinvoiceTextBox.Text;
            branchIDFrom = selectedBranchFromID;
            branchIDTo = selectedBranchToID;

            roDateTime = String.Format(culture, "{0:dd-MM-yyyy}", Convert.ToDateTime(selectedDate));
            roDateExpired = String.Format(culture, "{0:dd-MM-yyyy}", expiredRODate);
            roTotal = globalTotalValue;


            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                //WRITE RO INVOICE
                using (StreamWriter outputFile = new StreamWriter(exportedFileName))
                {
                    outputFile.WriteLine(roInvoice);
                }

                // WRITE HEADER TABLE SQL
                sqlCommand = "INSERT INTO REQUEST_ORDER_HEADER (RO_INVOICE, RO_BRANCH_ID_FROM, RO_BRANCH_ID_TO, RO_DATETIME, RO_TOTAL, RO_EXPIRED, RO_ACTIVE) VALUES " +
                                    "('" + roInvoice + "', " + branchIDFrom + ", " + branchIDTo + ", STR_TO_DATE('" + roDateTime + "', '%d-%m-%Y'), " + roTotal + ", STR_TO_DATE('" + roDateExpired + "', '%d-%m-%Y'), 1)";

                using (StreamWriter outputFile = new StreamWriter(exportedFileName, true))
                {
                    outputFile.WriteLine(sqlCommand);
                }

                // WRITE DETAIL TABLE SQL
                for (int i = 0; i < detailRequestOrderDataGridView.Rows.Count; i++)
                {
                    if (null != detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value)
                    {
                        sqlCommand = "INSERT INTO REQUEST_ORDER_DETAIL (RO_INVOICE, PRODUCT_ID, PRODUCT_BASE_PRICE, RO_QTY, RO_SUBTOTAL) VALUES " +
                                            "('" + roInvoice + "', '" + detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value.ToString() + "', " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["hpp"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["qty"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value) + ")";

                        using (StreamWriter outputFile = new StreamWriter(exportedFileName, true))
                        {
                            outputFile.WriteLine(sqlCommand);
                        }

                    }
                }

                sqlCommand = "UPDATE REQUEST_ORDER_HEADER SET RO_EXPORTED = 1 WHERE RO_INVOICE = '" + roInvoice + "'";
                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                    throw internalEX;

                DS.commit();

                result = true;
            }
            catch (Exception e)
            {
                result = false;
                try
                {
                    if (System.IO.File.Exists(exportedFileName))
                        System.IO.File.Delete(exportedFileName);

                    //myTrans.Rollback();
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
            }

            return result;
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            string exportedFileName = "";
            string roInvoice = "";

            if (saveData())
            {
                string selectedDate = RODateTimePicker.Value.ToShortDateString();
                roInvoice = ROinvoiceTextBox.Text;

                exportedFileName = "RO_" + roInvoice + "_" + String.Format(culture, "{0:ddMMyyyy}", Convert.ToDateTime(selectedDate)) + ".exp";

                saveFileDialog1.FileName = exportedFileName;
                saveFileDialog1.Filter = "Export File (.exp)|*.exp";

                //saveFileDialog1.ShowDialog();

                if (DialogResult.OK == saveFileDialog1.ShowDialog())
                    if (exportDataRO(saveFileDialog1.FileName))
                    { 
                        gUtil.ResetAllControls(this);
                        originModuleID = globalConstants.NEW_REQUEST_ORDER;
                        detailRequestOrderDataGridView.Rows.Clear();
                        totalLabel.Text = "Rp. 0";

                        selectedBranchFromID = 0;
                        selectedBranchToID = 0;

                        gUtil.showSuccess(gUtil.UPD);

                        ROinvoiceTextBox.ReadOnly = false;
                        ROinvoiceTextBox.Focus();
                    }
            }
        }

        private double getHPPValue(string productID)
        {
            double result = 0;
            
            result = Convert.ToDouble(DS.getDataSingleValue("SELECT PRODUCT_BASE_PRICE FROM MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'"));

            return result;
        }

        private void addColumnToDataGrid()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DataGridViewComboBoxColumn productIdCmb = new DataGridViewComboBoxColumn();
            DataGridViewComboBoxColumn productNameCmb = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn stockQtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn basePriceColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn subTotalColumn = new DataGridViewTextBoxColumn();
            
            sqlCommand = "SELECT PRODUCT_ID, PRODUCT_NAME FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1 ORDER BY PRODUCT_NAME ASC";

            //productIDComboHidden.Items.Clear();
            //productNameComboHidden.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                        productNameCmb.Items.Add(rdr.GetString("PRODUCT_NAME"));
                        productIdCmb.Items.Add(rdr.GetString("PRODUCT_ID"));
                        //productIDComboHidden.Items.Add(rdr.GetString("PRODUCT_ID"));
                        //productNameComboHidden.Items.Add(rdr.GetString("PRODUCT_NAME"));
                }
            }

            rdr.Close();

            productIdCmb.HeaderText = "KODE PRODUK";
            productIdCmb.Name = "productID";
            productIdCmb.Width = 200;
            productIdCmb.DefaultCellStyle.BackColor = Color.LightBlue;
            detailRequestOrderDataGridView.Columns.Add(productIdCmb);

            productNameCmb.HeaderText = "NAMA PRODUK";
            productNameCmb.Name = "productName";
            productNameCmb.Width = 300;
            productNameCmb.DefaultCellStyle.BackColor = Color.LightBlue;
            detailRequestOrderDataGridView.Columns.Add(productNameCmb);

            stockQtyColumn.HeaderText = "QTY";
            stockQtyColumn.Name = "qty";
            stockQtyColumn.Width = 100;
            stockQtyColumn.DefaultCellStyle.BackColor = Color.LightBlue;
            detailRequestOrderDataGridView.Columns.Add(stockQtyColumn);

            basePriceColumn.HeaderText = "HARGA POKOK";
            basePriceColumn.Name = "HPP";
            basePriceColumn.Width = 200;
            basePriceColumn.ReadOnly = true;
            detailRequestOrderDataGridView.Columns.Add(basePriceColumn);

            subTotalColumn.HeaderText = "SUBTOTAL";
            subTotalColumn.Name = "subTotal";
            subTotalColumn.Width = 200;
            subTotalColumn.ReadOnly = true;
            detailRequestOrderDataGridView.Columns.Add(subTotalColumn);


        }

        private void permintaanProdukForm_Load(object sender, EventArgs e)
        {
            int userAccessOption = 0;
            RODateTimePicker.CustomFormat = globalUtilities.CUSTOM_DATE_FORMAT;

            //fillInBranchFromCombo();
            //fillInProductNameCombo();
            addColumnToDataGrid();

            // ALL REQUEST WILL GO TO PUSAT 
            selectedBranchFromID = 0; // SET BRANCH_FROM TO PUSAT 
            selectedBranchToID = getCurrentBranchID(); // SET BRANCH_TO TO CURRENT BRANCH

            detailRequestOrderDataGridView.EditingControlShowing += detailRequestOrderDataGridView_EditingControlShowing;

            userAccessOption = DS.getUserAccessRight(globalConstants.MENU_REQUEST_ORDER, gUtil.getUserGroupID());

            if (originModuleID == globalConstants.NEW_REQUEST_ORDER)
            {
                if (userAccessOption != 2 && userAccessOption != 6)
                {
                    gUtil.setReadOnlyAllControls(this);
                }
            }
            else if (originModuleID == globalConstants.EDIT_REQUEST_ORDER)
            {
                if (userAccessOption != 4 && userAccessOption != 6)
                {
                    gUtil.setReadOnlyAllControls(this);
                }
            }

            gUtil.reArrangeTabOrder(this);            
        }

        private bool invoiceExist()
        {
            bool result = false;

            DS.mySqlConnect();

            if (Convert.ToInt32(DS.getDataSingleValue("SELECT COUNT(1) FROM REQUEST_ORDER_HEADER WHERE RO_INVOICE = '" + ROinvoiceTextBox.Text + "'")) > 0)
                result = true;

            return result;
        }

        private void ROinvoiceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (isLoading)
                return;

            ROinvoiceTextBox.Text = ROinvoiceTextBox.Text.Trim();
            if (invoiceExist())
            {
                errorLabel.Text = "NO PERMINTAAN SUDAH ADA";
                //ROinvoiceTextBox.BackColor = Color.Red;
                ROinvoiceTextBox.Focus();
            }
            else
            {
                errorLabel.Text = "";
               // ROinvoiceTextBox.BackColor = Color.White;
            }
        }

        private void fillInBranchFromCombo()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT BRANCH_ID, BRANCH_NAME FROM MASTER_BRANCH WHERE BRANCH_ACTIVE = 1 ORDER BY BRANCH_NAME ASC";

            branchFromCombo.Items.Clear();
            branchFromComboHidden.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    branchFromCombo.Items.Add(rdr.GetString("BRANCH_NAME"));
                    branchFromComboHidden.Items.Add(rdr.GetString("BRANCH_ID"));
                }
            }

            rdr.Close();
        }
        
        private void fillInBranchToCombo()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT BRANCH_ID, BRANCH_NAME FROM MASTER_BRANCH WHERE BRANCH_ACTIVE = 1 AND BRANCH_ID <> " + selectedBranchFromID + " ORDER BY BRANCH_NAME ASC";

            branchToCombo.Text = "";
            branchToCombo.Items.Clear();
            branchToComboHidden.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    branchToCombo.Items.Add(rdr.GetString("BRANCH_NAME"));
                    branchToComboHidden.Items.Add(rdr.GetString("BRANCH_ID"));
                }
            }

            rdr.Close();
        }

        private void branchFromCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = branchFromCombo.SelectedIndex;

            if (!isLoading)
                selectedBranchFromID = Convert.ToInt32(branchFromComboHidden.Items[selectedIndex]);

            fillInBranchToCombo();
        }

        private void branchToCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isLoading)
                selectedBranchToID = Convert.ToInt32(branchToComboHidden.Items[branchToCombo.SelectedIndex].ToString());
        }

        private bool dataValidated()
        {
            int i = 0;
            bool dataExist = false;

            if (selectedBranchToID == 0)
            {
                errorLabel.Text = "INFORMASI CABANG BELUM DI ISI";
                return false;
            }

            if (ROinvoiceTextBox.Text.Equals(""))
            {
                errorLabel.Text = "NO PERMINTAAN TIDAK BOLEH KOSONG";
                return false;
            }

            if (detailRequestOrderDataGridView.Rows.Count <= 0)
            {
                errorLabel.Text = "TIDAK ADA BARANG YANG DIMINTA";
                return false;
            }

            while (i<detailRequestOrderDataGridView.Rows.Count && !dataExist)
            {
                if (null != detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value)
                    dataExist = true;

                i++;
            }
            if (!dataExist)
            {
                errorLabel.Text = "TIDAK ADA BARANG YANG DIMINTA";
                return false;
            }

            return true;
        }

        private int getCurrentBranchID()
        {
            int result = 0;

            result = Convert.ToInt32(DS.getDataSingleValue("SELECT IFNULL(BRANCH_ID, 0) FROM SYS_CONFIG WHERE ID = 2"));

            return result;
        }

        private bool saveDataTransaction()
        {
            bool result = false;
            string sqlCommand = "";
            MySqlException internalEX = null;

            string roInvoice = "";
            int branchIDFrom = 0;
            int branchIDTo = 0;
            string roDateTime = "";
            double roTotal = 0;
            string roDateExpired = "";
            DateTime selectedRODate;
            DateTime expiredRODate;

            string selectedDate = RODateTimePicker.Value.ToShortDateString();
            selectedRODate = RODateTimePicker.Value;
            expiredRODate = selectedRODate.AddDays(Convert.ToDouble(durationTextBox.Text));

            roInvoice = ROinvoiceTextBox.Text;
            branchIDFrom = selectedBranchFromID;
            branchIDTo = selectedBranchToID;

            roDateTime = String.Format(culture, "{0:dd-MM-yyyy}", Convert.ToDateTime(selectedDate));
            roDateExpired = String.Format(culture, "{0:dd-MM-yyyy}", expiredRODate);
            roTotal = globalTotalValue;
            
            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                switch(originModuleID)
                {
                    case globalConstants.NEW_REQUEST_ORDER:
                        // SAVE HEADER TABLE
                        sqlCommand = "INSERT INTO REQUEST_ORDER_HEADER (RO_INVOICE, RO_BRANCH_ID_FROM, RO_BRANCH_ID_TO, RO_DATETIME, RO_TOTAL, RO_EXPIRED, RO_ACTIVE) VALUES " +
                                            "('" + roInvoice + "', " + branchIDFrom + ", " + branchIDTo + ", STR_TO_DATE('" + roDateTime + "', '%d-%m-%Y'), " + roTotal + ", STR_TO_DATE('" + roDateExpired + "', '%d-%m-%Y'), 1)";

                        if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            throw internalEX;

                        // SAVE DETAIL TABLE
                        for (int i = 0; i < detailRequestOrderDataGridView.Rows.Count; i++)
                        {
                            if (null != detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value)
                            {
                                sqlCommand = "INSERT INTO REQUEST_ORDER_DETAIL (RO_INVOICE, PRODUCT_ID, PRODUCT_BASE_PRICE, RO_QTY, RO_SUBTOTAL) VALUES " +
                                                    "('" + roInvoice + "', '" + detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value.ToString() + "', " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["hpp"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["qty"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value) + ")";

                                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                                    throw internalEX;
                            }
                        }
                        break;

                    case globalConstants.EDIT_REQUEST_ORDER:
                        // UPDATE HEADER TABLE
                        sqlCommand = "UPDATE REQUEST_ORDER_HEADER SET " +
                                            "RO_BRANCH_ID_FROM = " + branchIDFrom + ", " +
                                            "RO_BRANCH_ID_TO = " + branchIDTo + ", " +
                                            "RO_DATETIME = STR_TO_DATE('" + roDateTime + "', '%d-%m-%Y'), " +
                                            "RO_TOTAL = " + roTotal + ", " +
                                            "RO_EXPIRED = STR_TO_DATE('" + roDateExpired + "', '%d-%m-%Y') " +
                                            "WHERE RO_INVOICE = '" + roInvoice + "'";

                        if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            throw internalEX;

                        // DELETE DETAIL TABLE
                        sqlCommand = "DELETE FROM REQUEST_ORDER_DETAIL WHERE RO_INVOICE = '" + roInvoice + "'";
                        if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            throw internalEX;

                        // RE-INSERT DETAIL TABLE
                        for (int i = 0; i < detailRequestOrderDataGridView.Rows.Count; i++)
                        {
                            if (null != detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value)
                            {
                                sqlCommand = "INSERT INTO REQUEST_ORDER_DETAIL (RO_INVOICE, PRODUCT_ID, PRODUCT_BASE_PRICE, RO_QTY, RO_SUBTOTAL) VALUES " +
                                                    "('" + roInvoice + "', '" + detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value.ToString() + "', " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["hpp"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["qty"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value) + ")";

                                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                                    throw internalEX;
                            }
                        }
                        break;
                }
                
                DS.commit();
                result = true;
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
                        gUtil.showDBOPError(ex, "ROLLBACK");
                    }
                }

                gUtil.showDBOPError(e, "INSERT");
                result = false;
            }
            finally
            {
                DS.mySqlClose();
            }

            return result;
        }

        private bool saveData()
        {
            if (dataValidated())
            {
                return saveDataTransaction();
            }

            return false;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveData())
            {
                //MessageBox.Show("SUCCESS");
                gUtil.ResetAllControls(this);
                originModuleID = globalConstants.NEW_REQUEST_ORDER;
                detailRequestOrderDataGridView.Rows.Clear();
                totalLabel.Text = "Rp. 0";

                //selectedBranchFromID = 0;
                //selectedBranchToID = 0;

                if (originModuleID == globalConstants.NEW_REQUEST_ORDER)
                    gUtil.showSuccess(gUtil.INS);
                else if (originModuleID == globalConstants.EDIT_REQUEST_ORDER)
                    gUtil.showSuccess(gUtil.UPD);
                
                ROinvoiceTextBox.Focus();
                
                /*ROinvoiceTextBox.ReadOnly = true;
                RODateTimePicker.Enabled = false;
                branchFromCombo.Enabled = false;
                branchToCombo.Enabled = false;
                durationTextBox.ReadOnly = true;
                detailRequestOrderDataGridView.ReadOnly = true;
                detailRequestOrderDataGridView.AllowUserToAddRows = false;

                //saveButton.Enabled = false;
                //generateButton.Enabled = false;
                //exportButton.Enabled = false;

                saveButton.Visible = false;
                generateButton.Visible = false;
                exportButton.Visible = false;*/
            }
        }


        private void deleteCurrentRow()
        {
            if (detailRequestOrderDataGridView.SelectedCells.Count > 0)
            {
                int rowSelectedIndex = detailRequestOrderDataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = detailRequestOrderDataGridView.Rows[rowSelectedIndex];    

                detailRequestOrderDataGridView.Rows.Remove(selectedRow);
            }
        }

        private void detailRequestOrderDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (DialogResult.Yes == MessageBox.Show("DELETE CURRENT ROW?","WARNING", MessageBoxButtons.YesNo))
                {
                    deleteCurrentRow();
                    calculateTotal();
                }
            }
        }

        private bool isExportedRO()
        {
            bool result = false;

            if (0!=Convert.ToInt32(DS.getDataSingleValue("SELECT RO_EXPORTED FROM REQUEST_ORDER_HEADER WHERE RO_INVOICE = '"+ROinvoiceTextBox.Text+"'")))
            {
                result = true;
            }

            return result;
        }

        private void permintaanProdukForm_Activated(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            deactivateButton.Visible = false;

            if (originModuleID == globalConstants.EDIT_REQUEST_ORDER)
            {
                isLoading = true;

                loadDataHeaderRO();
                selectedROInvoice = ROinvoiceTextBox.Text;
                ROinvoiceTextBox.ReadOnly = true;
                //branchFromCombo.Text = getBranchName(selectedBranchFromID);
                //branchToCombo.Text = getBranchName(selectedBranchToID);

                loadDataDetailRO();

                if (isExportedRO())
                {
                    ROinvoiceTextBox.ReadOnly = true;
                    RODateTimePicker.Enabled = false;
                    branchFromCombo.Enabled = false;
                    branchToCombo.Enabled = false;
                    durationTextBox.ReadOnly = true;
                    detailRequestOrderDataGridView.ReadOnly = true;
                    detailRequestOrderDataGridView.AllowUserToAddRows = false;

                    //saveButton.Enabled = false;
                    //generateButton.Enabled = false;
                    //exportButton.Enabled = false;

                    saveButton.Visible = false;
                    generateButton.Visible = false;
                    //exportButton.Visible = false;
                    deactivateButton.Visible = true;
                }
                
                isLoading = false;
            }
        }

        private void generateButton_Click(object sender, EventArgs e)
        {

        }

        private void branchFromCombo_Validated(object sender, EventArgs e)
        {
            if (!branchFromCombo.Items.Contains(branchFromCombo.Text))
                branchFromCombo.Focus();
        }

        private void branchToCombo_Validated(object sender, EventArgs e)
        {
            if (!branchToCombo.Items.Contains(branchToCombo.Text))
                branchToCombo.Focus();
        }

        private bool setNonActiveRO()
        {
            bool result = false;
            string sqlCommand = "";
            MySqlException internalEX = null;

            string roInvoice = "";
            roInvoice = ROinvoiceTextBox.Text;
          
            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                sqlCommand = "UPDATE REQUEST_ORDER_HEADER SET RO_ACTIVE = 0 WHERE RO_INVOICE = '" + roInvoice + "'";

                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                    throw internalEX;
          
                DS.commit();
                result = true;
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
                        gUtil.showDBOPError(ex, "ROLLBACK");
                    }
                }

                gUtil.showDBOPError(e, "INSERT");
                result = false;
            }
            finally
            {
                DS.mySqlClose();
            }

            return result;
        }

        private void deactivateButton_Click(object sender, EventArgs e)
        {
            if (setNonActiveRO())
            {
                MessageBox.Show("SUCCESS");
                deactivateButton.Visible = false;
            }
        }
    }
}
