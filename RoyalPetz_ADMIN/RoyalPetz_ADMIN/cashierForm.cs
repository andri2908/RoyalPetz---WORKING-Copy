using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotkeys;

using MySql.Data;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace RoyalPetz_ADMIN
{
    public partial class cashierForm : Form
    {
        public static int objCounter = 1;
        private DateTime localDate = DateTime.Now;
        private double globalTotalValue = 0;
        private int selectedPelangganID = 0;
        private int selectedPelangganIDCustomerType = 0;
        private bool isLoading = false;

        private Data_Access DS = new Data_Access();

        private globalUtilities gutil = new globalUtilities();
        private CultureInfo culture = new CultureInfo("id-ID");
        private List<string> salesQty = new List<string>();
        private List<string> disc1 = new List<string>();
        private List<string> disc2 = new List<string>();
        private List<string> discRP = new List<string>();
        private string previousInput = "";

        private Hotkeys.GlobalHotkey ghk_F1;
        private Hotkeys.GlobalHotkey ghk_F2;
        private Hotkeys.GlobalHotkey ghk_F3;
        private Hotkeys.GlobalHotkey ghk_F4;
        private Hotkeys.GlobalHotkey ghk_F5;
        private Hotkeys.GlobalHotkey ghk_F7;
        private Hotkeys.GlobalHotkey ghk_F8;
        private Hotkeys.GlobalHotkey ghk_F9;
        private Hotkeys.GlobalHotkey ghk_F10;
        private Hotkeys.GlobalHotkey ghk_F11;
        private Hotkeys.GlobalHotkey ghk_F12;
        
        private Hotkeys.GlobalHotkey ghk_CTRL_DEL;
        private Hotkeys.GlobalHotkey ghk_CTRL_C;
        private Hotkeys.GlobalHotkey ghk_CTRL_U;

        private Hotkeys.GlobalHotkey ghk_ALT_F4;

        private adminForm parentForm;

        public cashierForm()
        {
            InitializeComponent();
        }

        public cashierForm(int counter)
        {
            InitializeComponent();
            label1.Text = "Struk # : " + counter;

            objCounter = counter + 1;
        }

        public void setCustomerID(int ID)
        {
            selectedPelangganID = ID;
            setCustomerProfile();

            refreshProductPrice();
        }
        
        private void updateLabel()
        {
            localDate = DateTime.Now;
            dateTimeStampLabel.Text = String.Format(culture, "{0:dddd, dd-MM-yyyy - HH:mm}", localDate);
        }

        private void updateRowNumber()
        {
            for (int i = 0;i<cashierDataGridView.Rows.Count;i++)
                cashierDataGridView.Rows[i].Cells["F8"].Value = i + 1;
        }

        private void addNewRow()
        {
            int prevValue = 0;
                
            if (cashierDataGridView.Rows.Count >0 )
            {
                prevValue = Convert.ToInt32(cashierDataGridView.Rows[cashierDataGridView.Rows.Count-1].Cells["F8"].Value);
            }

            cashierDataGridView.Rows.Add();

            salesQty.Add("0");
            disc1.Add("0");
            disc2.Add("0");
            discRP.Add("0");

            cashierDataGridView.Rows[cashierDataGridView.Rows.Count - 1].Cells["F8"].Value = prevValue + 1;
        }

        private void captureAll(Keys key)
        {
            switch (key)
            {
                case Keys.F3:
                    cashierForm displayForm = new cashierForm(objCounter);
                    displayForm.Show();
                    break;
            
                case Keys.F4:
                    //MessageBox.Show("F4");
                    dataPelangganForm pelangganForm = new dataPelangganForm(globalConstants.CASHIER_MODULE, this);
                    pelangganForm.ShowDialog(this);
                    break;
            
                case Keys.F8:
                    addNewRow();
                    break;

                case Keys.F9:
                    saveAndPrintOutInvoice();
                    break;

                
                
                case Keys.F1:
                    MessageBox.Show("F1");
                    break;
                case Keys.F2:
                    MessageBox.Show("F2");
                    break;
                case Keys.F5:
                    MessageBox.Show("F5");
                    break;
                case Keys.F7:
                    MessageBox.Show("F7");
                    break;
                case Keys.F10:
                    MessageBox.Show("F10");
                    break;
                case Keys.F11:
                    MessageBox.Show("F11");
                    break;
                case Keys.F12:
                    MessageBox.Show("F12");
                    break;
            }
        }

        private void captureAltModifier(Keys key)
        {
            switch (key)
            {
                case Keys.F4: // ALT + F4
                    MessageBox.Show("ALT+F4");
                    this.Close();
                    break;
            }
        }

        private void captureCtrlModifier(Keys key)
        {
            switch (key)
            {
                case Keys.Delete: // CTRL + DELETE
                    MessageBox.Show("CTRL+DELETE");
                    break;
                case Keys.C: // CTRL + C
                    MessageBox.Show("CTRL+C");
                    break;
                case Keys.U: // CTRL + U
                    MessageBox.Show("CTRL+U");
                    break;
            }
        }

        protected override void WndProc(ref Message m)
        {
           if (m.Msg == Constants.WM_HOTKEY_MSG_ID) {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                int modifier = (int)m.LParam & 0xFFFF;

               if (modifier == Constants.NOMOD)
                    captureAll(key);
                else if (modifier == Constants.ALT)
                   captureAltModifier(key);
                else if (modifier == Constants.CTRL)
                   captureCtrlModifier(key);
            }

            base.WndProc(ref m);
        }

        private void registerGlobalHotkey()
        {
            ghk_F3 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F3, this);
            ghk_F3.Register();

            ghk_F4 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F4, this);
            ghk_F4.Register();
            
            ghk_F8 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F8, this);
            ghk_F8.Register();

            ghk_F9 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F9, this);
            ghk_F9.Register();
            
            
            //ghk_F1 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F1, this);
            //ghk_F1.Register();
            
            //ghk_F2 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F2, this);
            //ghk_F2.Register();
            
            //ghk_F5 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F5, this);
            //ghk_F5.Register();

            //ghk_F7 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F7, this);
            //ghk_F7.Register();
            
            
            //ghk_F10 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F10, this);
            //ghk_F10.Register();

            //ghk_F11 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F11, this);
            //ghk_F11.Register();

            //// ## F12 doesn't work yet ##
            ////ghk_F12 = new Hotkeys.GlobalHotkey(Constants.NOMOD, Keys.F12, this);
            ////ghk_F12.Register();

            //ghk_CTRL_DEL = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.Delete, this);
            //ghk_CTRL_DEL.Register();

            //ghk_CTRL_C = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.C, this);
            //ghk_CTRL_C.Register();

            //ghk_CTRL_U = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.U, this);
            //ghk_CTRL_U.Register();

            //ghk_ALT_F4 = new Hotkeys.GlobalHotkey(Constants.ALT, Keys.F4, this);
            //ghk_ALT_F4.Register();

        }

        private void unregisterGlobalHotkey()
        {
            ghk_F3.Unregister();
            ghk_F4.Unregister();
            ghk_F8.Unregister();
            ghk_F9.Unregister();

            //ghk_F1.Unregister();
            //ghk_F2.Unregister();
            //ghk_F5.Unregister();
            //ghk_F7.Unregister();
            
            //ghk_F10.Unregister();
            //ghk_F11.Unregister();
            ////ghk_F12.Unregister();

            //ghk_CTRL_DEL.Unregister();
            //ghk_CTRL_C.Unregister();
            //ghk_CTRL_U.Unregister();

            //ghk_ALT_F4.Unregister();
        }

        private void fillInDummyData()
        {
            for (int i = 1; i <= 150;i++ )
                cashierDataGridView.Rows.Add(i, "", "", "","", "", "");
        }

        private void setCustomerProfile()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            //DS.mySqlConnect();
            sqlCommand = "SELECT * FROM MASTER_CUSTOMER WHERE CUSTOMER_ID = " + selectedPelangganID;
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    rdr.Read();

                    pelangganTextBox.Text = rdr.GetString("CUSTOMER_FULL_NAME");
                    isLoading = true;
                    customerComboBox.SelectedIndex = rdr.GetInt32("CUSTOMER_GROUP") - 1;
                    customerComboBox.Text = customerComboBox.Items[customerComboBox.SelectedIndex].ToString();
                    isLoading = false;
                }
            }
            rdr.Close();
        }

        private void refreshProductPrice()
        {
            double productPrice = 0;
            for (int i =0;i<cashierDataGridView.Rows.Count;i++)
            {
                if (null != cashierDataGridView.Rows[i].Cells["productID"].Value)
                {
                    productPrice = getProductPriceValue(cashierDataGridView.Rows[i].Cells["productID"].Value.ToString(), customerComboBox.SelectedIndex);

                    cashierDataGridView.Rows[i].Cells["productPrice"].Value = productPrice;
                    cashierDataGridView.Rows[i].Cells["jumlah"].Value = calculateSubTotal(i, productPrice);
                }
            }

            calculateTotal();
        }
        
        private bool dataValidated()
        {
            return true;
        }

        private bool saveDataTransaction()
        {
            bool result = false;
            string sqlCommand = "";

            string salesInvoice = "0";
            bool newID = false;
            string salesInvPrefix = "";
            int currentCounter = 0;
            
            string SODateTime = "";
            DateTime SODueDateTimeValue;
            string SODueDateTime = "";
            string salesDiscountFinal = "0";
            int salesTop = 1;
            int salesPaid = 0;
            MySqlException internalEX = null;

            SODateTime = String.Format(culture, "{0:dd-MM-yyyy}", DateTime.Now);

            if (discJualMaskedTextBox.Text.Length > 0)
                salesDiscountFinal = discJualMaskedTextBox.Text;

            if (cashRadioButton.Checked)
            {
                salesTop = 1;
                salesPaid = 1;
                SODueDateTime = SODateTime;
            }
            else
            { 
                salesTop = 0;
                SODueDateTimeValue = DateTime.Now;
                SODueDateTimeValue.AddDays(Convert.ToInt32(tempoMaskedTextBox.Text));
                SODueDateTime = String.Format(culture, "{0:dd-MM-yyyy}", SODueDateTimeValue);
            }

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                // GENERATE NEW SALES INVOICE
                if (!getSalesInvoiceID(ref salesInvoice, ref newID, ref salesInvPrefix, ref currentCounter))
                    throw new Exception("CAN'T GENERATE SALES INVOICE");
            
                if (newID)
                    sqlCommand = "INSERT INTO SYS_SALESINV_AUTOGENERATE (SALES_INVOICE_PREFIX, SALES_INVOICE_COUNTER) VALUES ('" + salesInvPrefix + "', " + currentCounter + ")";
                else
                    sqlCommand = "UPDATE SYS_SALESINV_AUTOGENERATE SET SALES_INVOICE_COUNTER = " + currentCounter + " WHERE SALES_INVOICE_PREFIX = '" + salesInvPrefix + "'";

                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                    throw internalEX;


                // SAVE HEADER TABLE
                sqlCommand = "INSERT INTO SALES_HEADER (SALES_INVOICE, CUSTOMER_ID, SALES_DATE, SALES_TOTAL, SALES_DISCOUNT_FINAL, SALES_TOP, SALES_TOP_DATE, SALES_PAID) " +
                                    "VALUES " +
                                    "('" + salesInvoice + "', " + selectedPelangganID + ", STR_TO_DATE('" + SODateTime + "', '%d-%m-%Y'), " + globalTotalValue + ", " + salesDiscountFinal + ", " + salesTop + ", STR_TO_DATE('" + SODueDateTime + "', '%d-%m-%Y'), " + salesPaid + ")";
                
                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                    throw internalEX;

                // SAVE DETAIL TABLE
                for (int i = 0; i < cashierDataGridView.Rows.Count; i++)
                {
                    if (null != cashierDataGridView.Rows[i].Cells["qty"].Value)
                    {
                        sqlCommand = "INSERT INTO SALES_DETAIL (SALES_INVOICE, PRODUCT_ID, PRODUCT_SALES_PRICE, PRODUCT_QTY, PRODUCT_DISC1, PRODUCT_DISC2, PRODUCT_DISC_RP, SALES_SUBTOTAL) " +
                                            "VALUES " +
                                            "('" + salesInvoice + "', '" + cashierDataGridView.Rows[i].Cells["productID"].Value.ToString() + "', " + Convert.ToDouble(cashierDataGridView.Rows[i].Cells["productPrice"].Value) + ", " +
                                            Convert.ToDouble(cashierDataGridView.Rows[i].Cells["qty"].Value) + ", " + Convert.ToDouble(cashierDataGridView.Rows[i].Cells["disc1"].Value) +
                                            Convert.ToDouble(cashierDataGridView.Rows[i].Cells["disc2"].Value) + ", " + Convert.ToDouble(cashierDataGridView.Rows[i].Cells["discRP"].Value) +
                                            Convert.ToDouble(cashierDataGridView.Rows[i].Cells["jumlah"].Value) + ")";

                        if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            throw internalEX;

                        // REDUCE STOCK QTY AT MASTER PRODUCT
                        sqlCommand = "UPDATE MASTER_PRODUCT SET PRODUCT_STOCK_QTY = PRODUCT_STOCK_QTY - " + Convert.ToDouble(cashierDataGridView.Rows[i].Cells["qty"].Value) +
                                            " WHERE PRODUCT_ID = '" + cashierDataGridView.Rows[i].Cells["productID"].Value.ToString() + "'";

                        if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            throw internalEX;
                    }
                }

                DS.commit();
                result = true;
            }
            catch (Exception e)
            {
                try
                {
                    DS.rollBack(ref internalEX);
                }
                catch (MySqlException ex)
                {
                    if (DS.getMyTransConnection() != null)
                    {
                        gutil.showDBOPError(ex, "ROLLBACK");
                    }
                }

                gutil.showDBOPError(e, "INSERT");
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

        private void saveAndPrintOutInvoice()
        {
            if (DialogResult.Yes == MessageBox.Show("SAVE AND PRINT OUT ?", "WARNING", MessageBoxButtons.YesNo,MessageBoxIcon.Warning))
            {
                if (saveData())
                {
                    gutil.showSuccess(gutil.INS);
                    gutil.ResetAllControls(this);
                }
            }
        }

        private bool getSalesInvoiceID(ref string salesInvoice, ref bool newID, ref string salesInvPrefix, ref int currentCounter)
        {
            //string salesInvoice = "";
            DateTime localDate = DateTime.Now;
            //string salesInvPrefix = "";
            //int currentCounter = 0;
            string currentCounterStringValue;
            string sqlCommand = "";
            MySqlException internalEX = null;
            bool result = false;

            salesInvPrefix= String.Format(culture, "{0:yyyyMMdd}", localDate);

            //DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                sqlCommand = "SELECT IFNULL(SALES_INVOICE_COUNTER, 0) AS COUNTER FROM SYS_SALESINV_AUTOGENERATE WHERE SALES_INVOICE_PREFIX = '" + salesInvPrefix + "'";
                currentCounter = Convert.ToInt32(DS.getDataSingleValue(sqlCommand));

                if (0 == currentCounter)
                {
                    currentCounter = 1;
                    newID = true;
                    //sqlCommand = "INSERT INTO SYS_SALESINV_AUTOGENERATE (SALES_INVOICE_PREFIX, SALES_INVOICE_COUNTER) VALUES ('" + salesInvPrefix + "', " + currentCounter + ")";
                }
                else
                {
                    currentCounter += 1;
                    newID = false;
                    //sqlCommand = "UPDATE SYS_SALESINV_AUTOGENERATE SET SALES_INVOICE_COUNTER = " + currentCounter + " WHERE SALES_INVOICE_PREFIX = '" + salesInvPrefix + "'";
                }

                currentCounterStringValue = currentCounter.ToString();
                while (currentCounterStringValue.Length < 10)
                    currentCounterStringValue = "0" + currentCounterStringValue;


                //if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                //    throw internalEX;

                salesInvoice = salesInvPrefix + currentCounterStringValue;

                //DS.commit();
                result = true;
            }
            catch(Exception e)
            {
                //try
                //{
                //    DS.rollBack(ref internalEX);
                //}
                //catch (MySqlException ex)
                //{
                //    if (DS.getMyTransConnection() != null)
                //    {
                //        gutil.showDBOPError(ex, "ROLLBACK");
                //    }
                //}

                //gutil.showDBOPError(e, "INSERT");
                result = false;
            }
            finally
            {
                DS.mySqlClose();
            }

            return result;
        }

        private string getProductID(int selectedIndex)
        {
            string productID = "";
            productID = productComboHidden.Items[selectedIndex].ToString();
            return productID;
        }

        private double getProductPriceValue(string productID, int customerType = 0)
        {
            double result = 0;
            string priceType = "";

            DS.mySqlConnect();

            if (customerType == 0)
                priceType = "PRODUCT_RETAIL_PRICE";
            else if (customerType == 1)
                priceType = "PRODUCT_BULK_PRICE";
            else
                priceType = "PRODUCT_WHOLESALE_PRICE";

            result = Convert.ToDouble(DS.getDataSingleValue("SELECT IFNULL(" + priceType + ", 0) FROM MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'"));

            return result;
        }
        
        private void cashierDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (cashierDataGridView.CurrentCell.ColumnIndex == 1 && e.Control is ComboBox)
            {
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }

            if ((cashierDataGridView.CurrentCell.ColumnIndex == 3)
                && e.Control is TextBox)
            {
                TextBox textBox = e.Control as TextBox;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private double calculateSubTotal(int rowSelectedIndex, double productPrice)
        {
            double subTotal = 0;
            double productQty = 0;
            double hppValue = 0;
            double disc1Value = 0;
            double disc2Value = 0;
            double discRPValue = 0;

            try
            {
                productQty = Convert.ToDouble(salesQty[rowSelectedIndex]);

                hppValue = productPrice;

                disc1Value = Convert.ToDouble(disc1[rowSelectedIndex]);
                disc2Value = Convert.ToDouble(disc2[rowSelectedIndex]);
                discRPValue = Convert.ToDouble(discRP[rowSelectedIndex]);

                subTotal = Math.Round((hppValue * productQty), 2);

                if (disc1Value > 0)
                    subTotal = Math.Round(subTotal - (subTotal * disc1Value / 100), 2);

                if (disc2Value > 0)
                    subTotal = Math.Round(subTotal - (subTotal * disc2Value / 100), 2);

                if (discRPValue > 0)
                    subTotal = Math.Round(subTotal - discRPValue, 2);

            }
            catch (Exception ex)
            {
                subTotal = 0;
            }

            return subTotal;
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = 0;
            int rowSelectedIndex = 0;
            string selectedProductID = "";
            double hpp = 0;
            double productQty = 0;
            double subTotal = 0;

            DataGridViewComboBoxEditingControl dataGridViewComboBoxEditingControl = sender as DataGridViewComboBoxEditingControl;

            selectedIndex = dataGridViewComboBoxEditingControl.SelectedIndex;
            selectedProductID = getProductID(selectedIndex);
            hpp = getProductPriceValue(selectedProductID, customerComboBox.SelectedIndex);

            rowSelectedIndex = cashierDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = cashierDataGridView.Rows[rowSelectedIndex];

            selectedRow.Cells["productPrice"].Value = hpp;

            if (null == selectedRow.Cells["qty"].Value)
                selectedRow.Cells["qty"].Value = 0;

            selectedRow.Cells["productId"].Value = selectedProductID;

            subTotal = calculateSubTotal(rowSelectedIndex, hpp);

            //if (null != selectedRow.Cells["qty"].Value)
            //{
            //    productQty = Convert.ToDouble(selectedRow.Cells["qty"].Value);
            //    subTotal = Math.Round((hpp * productQty), 2);

            //    selectedRow.Cells["jumlah"].Value = subTotal;
            //}

            calculateTotal();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            int rowSelectedIndex = 0;
            double subTotal = 0;
            double productPrice = 0;
        
            DataGridViewTextBoxEditingControl dataGridViewTextBoxEditingControl = sender as DataGridViewTextBoxEditingControl;

            rowSelectedIndex = cashierDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = cashierDataGridView.Rows[rowSelectedIndex];

            previousInput = "";

            if (cashierDataGridView.CurrentCell.ColumnIndex != 3 && cashierDataGridView.CurrentCell.ColumnIndex != 4 && cashierDataGridView.CurrentCell.ColumnIndex != 5 && cashierDataGridView.CurrentCell.ColumnIndex != 6)
                return;

                if (gutil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL)
                    && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                {
                    switch (cashierDataGridView.CurrentCell.ColumnIndex)
                    {
                        case 3:
                            salesQty[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                            break;
                        case 4:
                            disc1[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                            break;
                        case 5:
                            disc2[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                            break;
                        case 6:
                            discRP[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                            break;
                    }
                }
                else
                {
                    switch (cashierDataGridView.CurrentCell.ColumnIndex)
                    {
                        case 3:
                            dataGridViewTextBoxEditingControl.Text = salesQty[rowSelectedIndex];
                            break;
                        case 4:
                            dataGridViewTextBoxEditingControl.Text = disc1[rowSelectedIndex];
                            break;
                        case 5:
                            dataGridViewTextBoxEditingControl.Text = disc2[rowSelectedIndex];
                            break;
                        case 6:
                            dataGridViewTextBoxEditingControl.Text = discRP[rowSelectedIndex];
                            break;
                    }                    
                }

                productPrice = Convert.ToDouble(selectedRow.Cells["productPrice"].Value);

                subTotal = calculateSubTotal(rowSelectedIndex, productPrice);
                selectedRow.Cells["jumlah"].Value = subTotal;

                calculateTotal();
        }

        private void cashierForm_Shown(object sender, EventArgs e)
        {
            registerGlobalHotkey();
        }

        private void cashierForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            unregisterGlobalHotkey();
        }

        private void creditRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (creditRadioButton.Checked == true)
            {
                paymentComboBox.Visible = false;
                tempoMaskedTextBox.Visible = true;
                
                labelCaraBayar.Text = "Tempo            :";
            }
        }

        private void cashierForm_Load(object sender, EventArgs e)
        {
            addColumnToDataGrid();

            customerComboBox.SelectedIndex = 0;
            customerComboBox.Text = customerComboBox.Items[0].ToString();

            cashierDataGridView.EditingControlShowing += cashierDataGridView_EditingControlShowing;

            gutil.reArrangeTabOrder(this);
        }

        private void cashierForm_Activated(object sender, EventArgs e)
        {
            //if need something
            updateLabel();
            timer1.Start();
        }

        private void cashierForm_Deactivate(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateLabel();
        }

        private void addColumnToDataGrid()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DataGridViewTextBoxColumn F8Column = new DataGridViewTextBoxColumn();
            DataGridViewComboBoxColumn productNameCmb = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn productPriceColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn stockQtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn disc1Column = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn disc2Column = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn discRPColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn subTotalColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn productIdColumn = new DataGridViewTextBoxColumn();

            sqlCommand = "SELECT PRODUCT_ID, PRODUCT_NAME FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1 ORDER BY PRODUCT_NAME ASC";

            productComboHidden.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    productNameCmb.Items.Add(rdr.GetString("PRODUCT_NAME"));
                    productComboHidden.Items.Add(rdr.GetString("PRODUCT_ID"));
                }
            }

            rdr.Close();

            // F8 COLUMN
            F8Column.HeaderText = "F8";
            F8Column.Name = "F8";
            F8Column.Width = 44;
            F8Column.ReadOnly = true;
            cashierDataGridView.Columns.Add(F8Column);

            // PRODUCT NAME COLUMN
            productNameCmb.HeaderText = "NAMA PRODUK";
            productNameCmb.Name = "productName";
            productNameCmb.Width = 320;
            cashierDataGridView.Columns.Add(productNameCmb);

            productPriceColumn.HeaderText = "HARGA";
            productPriceColumn.Name = "productPrice";
            productPriceColumn.Width = 200;
            productPriceColumn.ReadOnly = true;
            cashierDataGridView.Columns.Add(productPriceColumn);

            stockQtyColumn.HeaderText = "QTY";
            stockQtyColumn.Name = "qty";
            stockQtyColumn.Width = 100;
            cashierDataGridView.Columns.Add(stockQtyColumn);

            disc1Column.HeaderText = "DISC 1 (%)";
            disc1Column.Name = "disc1";
            disc1Column.Width = 150;
            disc1Column.MaxInputLength = 5;
            cashierDataGridView.Columns.Add(disc1Column);

            disc2Column.HeaderText = "DISC 2 (%)";
            disc2Column.Name = "disc2";
            disc2Column.Width = 150;
            disc2Column.MaxInputLength = 5;
            cashierDataGridView.Columns.Add(disc2Column);

            discRPColumn.HeaderText = "DISC RP";
            discRPColumn.Name = "discRP";
            discRPColumn.Width = 150;
            cashierDataGridView.Columns.Add(discRPColumn);

            subTotalColumn.HeaderText = "JUMLAH";
            subTotalColumn.Name = "jumlah";
            subTotalColumn.Width = 200;
            subTotalColumn.ReadOnly = true;
            cashierDataGridView.Columns.Add(subTotalColumn);

            productIdColumn.HeaderText = "PRODUCT_ID";
            productIdColumn.Name = "productID";
            productIdColumn.Width = 200;
            productIdColumn.Visible = false;
            cashierDataGridView.Columns.Add(productIdColumn);
        }

        private void deleteCurrentRow()
        {
            if (cashierDataGridView.SelectedCells.Count > 0)
            {
                int rowSelectedIndex = cashierDataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = cashierDataGridView.Rows[rowSelectedIndex];

                cashierDataGridView.Rows.Remove(selectedRow);
            }
        }

        private void cashierDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (DialogResult.Yes == MessageBox.Show("DELETE CURRENT ROW?", "WARNING", MessageBoxButtons.YesNo))
                {
                    deleteCurrentRow();
                    updateRowNumber();
                    calculateTotal();
                }
            }
        }

        private void calculateTotal()
        {
            double total = 0;

            for (int i = 0; i < cashierDataGridView.Rows.Count; i++)
            {
                if ( null != cashierDataGridView.Rows[i].Cells["jumlah"].Value )
                    total = total + Convert.ToDouble(cashierDataGridView.Rows[i].Cells["jumlah"].Value);
            }

            globalTotalValue = total;
            totalLabel.Text = "Rp. " + total.ToString();

            totalPenjualanTextBox.Text = total.ToString();
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void discJualMaskedTextBox_Validating(object sender, CancelEventArgs e)
        {
            double totalAfterDisc = 0;

            totalAfterDisc = globalTotalValue - Convert.ToDouble(discJualMaskedTextBox.Text);

            totalAfterDiscTextBox.Text = totalAfterDisc.ToString();
        }

        private void customerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading)
                return;

            refreshProductPrice();
            
        }

        private void cashRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (cashRadioButton.Checked)
            {
                tempoMaskedTextBox.Visible = false;
                labelCaraBayar.Text = "Cara Bayar       :";
                paymentComboBox.Visible = true;
            }
        }

    }
}
