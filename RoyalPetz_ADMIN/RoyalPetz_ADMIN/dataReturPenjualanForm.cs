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
    public partial class dataReturPenjualanForm : Form
    {
        private int originModuleID;
        private int selectedCustomerID;
        private string selectedProductID;
        private string selectedPurchaseInvoice;
        private double globalTotalValue = 0;
        private List<string> returnQty = new List<string>();

        private Data_Access DS = new Data_Access();
        private globalUtilities gutil = new globalUtilities();

        public dataReturPenjualanForm()
        {
            InitializeComponent();
        }

        public dataReturPenjualanForm(int moduleID, string purchaseInvoice = "", int customerID = 0)
        {
            InitializeComponent();

            originModuleID = moduleID;

            if (originModuleID == globalConstants.RETUR_PENJUALAN_STOCK_ADJUSTMENT)
            {
                invoiceDateLabel.Visible = false;
                invoiceDateTextBox.Visible = false;
                invoiceTotalLabel.Visible = false;
                invoiceTotalLabelValue.Visible = false;
                invoiceSignLabel.Visible = false;
                selectedCustomerID = customerID;

                invoiceInfoLabel.Text = "NAMA PELANGGAN";
            }
            else
            {
                selectedPurchaseInvoice = purchaseInvoice;
            }
        }

        private void addDataGridColumn()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DataGridViewComboBoxColumn productNameCmb = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn stockQtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn purchaseQtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn retailPriceColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn subtotalColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn productIdColumn = new DataGridViewTextBoxColumn();

            sqlCommand = "SELECT M.PRODUCT_ID, M.PRODUCT_NAME FROM MASTER_PRODUCT M, PURCHASE_DETAIL PD " +
                                "WHERE PD.PURCHASE_INVOICE = '" + selectedPurchaseInvoice + "' AND PD.PRODUCT_ID = M.PRODUCT_ID";

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

            // PRODUCT NAME COLUMN
            productNameCmb.HeaderText = "NAMA PRODUK";
            productNameCmb.Name = "productName";
            productNameCmb.Width = 300;
            detailReturDataGridView.Columns.Add(productNameCmb);

            retailPriceColumn.HeaderText = "RETAIL PRICE";
            retailPriceColumn.Name = "productPrice";
            retailPriceColumn.Width = 100;
            detailReturDataGridView.Columns.Add(retailPriceColumn);

            if (originModuleID == globalConstants.RETUR_PENJUALAN)
            {
                purchaseQtyColumn.HeaderText = "PO QTY";
                purchaseQtyColumn.Name = "POqty";
                purchaseQtyColumn.Width = 100;
                detailReturDataGridView.Columns.Add(purchaseQtyColumn);
            }
            
            stockQtyColumn.HeaderText = "QTY";
            stockQtyColumn.Name = "qty";
            stockQtyColumn.Width = 100;
            detailReturDataGridView.Columns.Add(stockQtyColumn);

            subtotalColumn.HeaderText = "SUBTOTAL";
            subtotalColumn.Name = "subtotal";
            subtotalColumn.Width = 100;
            subtotalColumn.Visible = false;
            detailReturDataGridView.Columns.Add(subtotalColumn);

            productIdColumn.HeaderText = "PRODUCT_ID";
            productIdColumn.Name = "productID";
            productIdColumn.Width = 200;
            productIdColumn.Visible = false;
            detailReturDataGridView.Columns.Add(productIdColumn);
        }

        private bool noReturExist()
        {
            bool result = false;

            if (Convert.ToInt32(DS.getDataSingleValue("SELECT COUNT(1) FROM RETURN_SALES_HEADER WHERE RS_INVOICE = '" + noReturTextBox.Text + "'")) > 0)
                result = true;

            return result;
        }

        private void detailReturDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (detailReturDataGridView.CurrentCell.ColumnIndex == 0 && e.Control is ComboBox)
            {
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }

            if ((detailReturDataGridView.CurrentCell.ColumnIndex == 1) && e.Control is TextBox)
            {
                TextBox textBox = e.Control as TextBox;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private string getProductID(int selectedIndex)
        {
            string productID = "";
            productID = productComboHidden.Items[selectedIndex].ToString();
            return productID;
        }

        private double getProductPriceValue(string productID)
        {
            double result = 0;
            string sqlCommand = "";
            DS.mySqlConnect();

            sqlCommand = "SELECT PRODUCT_PRICE FROM PURCHASE_DETAIL WHERE PURCHASE_INVOICE = '" + selectedPurchaseInvoice + "' AND PRODUCT_ID = '" + productID + "'";
            result = Convert.ToDouble(DS.getDataSingleValue(sqlCommand));

            return result;
        }

        private double getPOQty(string productID)
        {
            double result = 0;
            string sqlCommand = "";
            DS.mySqlConnect();

            sqlCommand = "SELECT PRODUCT_QTY FROM PURCHASE_DETAIL WHERE PURCHASE_INVOICE = '" + selectedPurchaseInvoice + "' AND PRODUCT_ID = '" + productID + "'";
            result = Convert.ToDouble(DS.getDataSingleValue(sqlCommand));

            return result;
        }
        
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = 0;
            int rowSelectedIndex = 0;
            string selectedProductID = "";
            double hpp = 0;

            DataGridViewComboBoxEditingControl dataGridViewComboBoxEditingControl = sender as DataGridViewComboBoxEditingControl;

            selectedIndex = dataGridViewComboBoxEditingControl.SelectedIndex;
            selectedProductID = getProductID(selectedIndex);
            hpp = getProductPriceValue(selectedProductID);

            rowSelectedIndex = detailReturDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailReturDataGridView.Rows[rowSelectedIndex];

            selectedRow.Cells["productPrice"].Value = hpp;

            if (null == selectedRow.Cells["qty"].Value)
                selectedRow.Cells["qty"].Value = 0;

            if (originModuleID == globalConstants.RETUR_PENJUALAN)
                selectedRow.Cells["POqty"].Value = getPOQty(selectedProductID);

            selectedRow.Cells["productId"].Value = selectedProductID;
        }

        private void calculateTotal()
        {
            double total = 0;

            for (int i = 0; i < detailReturDataGridView.Rows.Count; i++)
            {
                if (null != detailReturDataGridView.Rows[i].Cells["subtotal"].Value)
                    total = total + Convert.ToDouble(detailReturDataGridView.Rows[i].Cells["subtotal"].Value);
            }

            globalTotalValue = total;
            totalLabel.Text = "Rp. " + total.ToString();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            int rowSelectedIndex = 0;
            double subTotal = 0;
            double productPrice = 0;
            string productID = "";

            DataGridViewTextBoxEditingControl dataGridViewTextBoxEditingControl = sender as DataGridViewTextBoxEditingControl;

            rowSelectedIndex = detailReturDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailReturDataGridView.Rows[rowSelectedIndex];

            if (null != selectedRow.Cells["productID"].Value)
                productID = selectedRow.Cells["productID"].Value.ToString();

            if (gutil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL)
                && (dataGridViewTextBoxEditingControl.Text.Length > 0)
                )
            {
                returnQty[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
            }
            else
            {
                dataGridViewTextBoxEditingControl.Text = returnQty[rowSelectedIndex];
            }

            productPrice = Convert.ToDouble(selectedRow.Cells["productPrice"].Value);

            subTotal = Math.Round((productPrice * Convert.ToDouble(returnQty[rowSelectedIndex])), 2);
            selectedRow.Cells["subtotal"].Value = subTotal;

            calculateTotal();
        }

        private void noReturTextBox_TextChanged(object sender, EventArgs e)
        {
            if (noReturExist())
            {
                errorLabel.Text = "NO RETUR SUDAH ADA";
                noReturTextBox.Focus();
            }
            else
                errorLabel.Text = "";
        }

        private void detailReturDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            returnQty.Add("0");
            detailReturDataGridView.Rows[e.RowIndex].Cells["qty"].Value = "0";
        }
        
        private string getInvoiceTotalValue()
        {
            string result = "";

            // GLOBAL PURCHASE TOTAL VALUE WITHOUT ANY PAYMENT / RETURN
            result = DS.getDataSingleValue("SELECT PURCHASE_TOTAL FROM PURCHASE_HEADER WHERE PURCHASE_INVOICE = '" + selectedPurchaseInvoice + "'").ToString();

            return result;
        }

        private string getPelangganName()
        {
            string result = "";
            
            result = DS.getDataSingleValue("SELECT CUSTOMER_FULL_NAME FROM MASTER_CUSTOMER WHERE CUSTOMER_ID = "+selectedCustomerID).ToString();

            return result;
        }

        private void dataReturPenjualanForm_Load(object sender, EventArgs e)
        {
            rsDateTimePicker.CustomFormat = globalUtilities.CUSTOM_DATE_FORMAT;
            invoiceTotalLabelValue.Text = "Rp. " + getInvoiceTotalValue();

            if (originModuleID == globalConstants.RETUR_PENJUALAN_STOCK_ADJUSTMENT)
                invoiceInfoTextBox.Text = getPelangganName();

            addDataGridColumn();

            gutil.reArrangeTabOrder(this);
        }
    }
}
