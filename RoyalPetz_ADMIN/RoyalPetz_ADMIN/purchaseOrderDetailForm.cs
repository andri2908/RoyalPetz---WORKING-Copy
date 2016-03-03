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
    public partial class purchaseOrderDetailForm : Form
    {
        private Data_Access DS = new Data_Access();
        private globalUtilities gUtil = new globalUtilities();

        private bool isLoading = false;
        private double globalTotalValue = 0;
        private List<string> detailQty = new List<string>();
        private List<string> detailHpp = new List<string>();
        string previousInput = "";
        
        private int selectedSupplierID = 0;

        public purchaseOrderDetailForm()
        {
            InitializeComponent();
        }

        private void fillInSupplierCombo()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT SUPPLIER_ID, SUPPLIER_FULL_NAME FROM MASTER_SUPPLIER WHERE SUPPLIER_ACTIVE = 1";

            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    supplierCombo.Items.Clear();
                    supplierHiddenCombo.Items.Clear();

                    while (rdr.Read())
                    {
                        supplierCombo.Items.Add(rdr.GetString("SUPPLIER_FULL_NAME"));
                        supplierHiddenCombo.Items.Add(rdr.GetString("SUPPLIER_ID"));
                    }
                }
            }
        }

        private bool isPOInvoiceExist()
        {
            bool result = false;

            if (Convert.ToInt32(DS.getDataSingleValue("SELECT COUNT(1) FROM PURCHASE_HEADER WHERE PURCHASE_INVOICE = '"+POinvoiceTextBox.Text+"'")) > 0)
                result = true;

            return result;
        }

        private void addDataGridColumn()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DataGridViewComboBoxColumn productNameCmb = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn stockQtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn basePriceColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn subTotalColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn productIdColumn = new DataGridViewTextBoxColumn();

            sqlCommand = "SELECT PRODUCT_ID, PRODUCT_NAME FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1 ORDER BY PRODUCT_NAME ASC";

            productIDComboHidden.Items.Clear();
            productNameComboHidden.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    productNameCmb.Items.Add(rdr.GetString("PRODUCT_NAME"));
                    productIDComboHidden.Items.Add(rdr.GetString("PRODUCT_ID"));
                    productNameComboHidden.Items.Add(rdr.GetString("PRODUCT_NAME"));
                }
            }

            rdr.Close();

            // PRODUCT NAME COLUMN
            productNameCmb.HeaderText = "NAMA PRODUK";
            productNameCmb.Name = "productName";
            productNameCmb.Width = 300;
            detailPODataGridView.Columns.Add(productNameCmb);

            basePriceColumn.HeaderText = "HARGA POKOK";
            basePriceColumn.Name = "HPP";
            basePriceColumn.Width = 200;
            detailPODataGridView.Columns.Add(basePriceColumn);

            stockQtyColumn.HeaderText = "QTY";
            stockQtyColumn.Name = "qty";
            stockQtyColumn.Width = 100;
            detailPODataGridView.Columns.Add(stockQtyColumn);

            subTotalColumn.HeaderText = "SUBTOTAL";
            subTotalColumn.Name = "subTotal";
            subTotalColumn.Width = 200;
            subTotalColumn.ReadOnly = true;
            detailPODataGridView.Columns.Add(subTotalColumn);

            productIdColumn.HeaderText = "PRODUCT_ID";
            productIdColumn.Name = "productID";
            productIdColumn.Width = 200;
            productIdColumn.Visible = false;
            detailPODataGridView.Columns.Add(productIdColumn);
        }

        private void detailPODataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (detailPODataGridView.CurrentCell.ColumnIndex == 0 && e.Control is ComboBox)
            {
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }

            if ((detailPODataGridView.CurrentCell.ColumnIndex == 1 || detailPODataGridView.CurrentCell.ColumnIndex == 2)
                && e.Control is TextBox)
            {
                TextBox textBox = e.Control as TextBox;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private string getProductID(int selectedIndex)
        {
            string productID = "";
            productID = productIDComboHidden.Items[selectedIndex].ToString();
            return productID;
        }

        private double getHPPValue(string productID)
        {
            double result = 0;

            DS.mySqlConnect();

            result = Convert.ToDouble(DS.getDataSingleValue("SELECT IFNULL(PRODUCT_BASE_PRICE, 0) FROM MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'"));

            return result;
        }

        private void calculateTotal()
        {
            double total = 0;

            for (int i = 0; i < detailPODataGridView.Rows.Count; i++)
            {
                total = total + Convert.ToDouble(detailPODataGridView.Rows[i].Cells["subTotal"].Value);
            }

            globalTotalValue = total;
            totalLabel.Text = "Rp. " + total.ToString();
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = 0;
            int rowSelectedIndex = 0;
            string selectedProductID = "";
            double hpp = 0;
            double productQty = 0;
            double subTotal = 0;

            if (isLoading)
                return;

            DataGridViewComboBoxEditingControl dataGridViewComboBoxEditingControl = sender as DataGridViewComboBoxEditingControl;

            selectedIndex = dataGridViewComboBoxEditingControl.SelectedIndex;
            selectedProductID = getProductID(selectedIndex);
            hpp = getHPPValue(selectedProductID);

            rowSelectedIndex = detailPODataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailPODataGridView.Rows[rowSelectedIndex];

            selectedRow.Cells["hpp"].Value = hpp;

            if (null == selectedRow.Cells["qty"].Value)
                selectedRow.Cells["qty"].Value = 0;

            selectedRow.Cells["productId"].Value = selectedProductID;

            if (null != selectedRow.Cells["qty"].Value)
            {
                productQty = Convert.ToDouble(selectedRow.Cells["qty"].Value);
                subTotal = Math.Round((hpp * productQty), 2);

                selectedRow.Cells["subTotal"].Value = subTotal;
            }

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

            rowSelectedIndex = detailPODataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailPODataGridView.Rows[rowSelectedIndex];

            previousInput = "";
            if (detailQty.Count < rowSelectedIndex + 1)
            {
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL)
                    && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                {
                    if (detailPODataGridView.CurrentCell.ColumnIndex == 2)
                    {
                        detailQty.Add(dataGridViewTextBoxEditingControl.Text);
                        detailHpp.Add(selectedRow.Cells["hpp"].Value.ToString());
                    }
                    else
                    { 
                        detailHpp.Add(dataGridViewTextBoxEditingControl.Text);
                        detailQty.Add(selectedRow.Cells["qty"].Value.ToString());
                    }
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
                    if (detailPODataGridView.CurrentCell.ColumnIndex == 1)
                        detailHpp[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                    else
                        detailQty[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                }
                else
                {
                    if (detailPODataGridView.CurrentCell.ColumnIndex == 1)
                        dataGridViewTextBoxEditingControl.Text = detailHpp[rowSelectedIndex];
                    else
                        dataGridViewTextBoxEditingControl.Text = detailQty[rowSelectedIndex];
                }
            }

            try
            {
                if (detailPODataGridView.CurrentCell.ColumnIndex == 1)
                {
                    //changes on hpp
                    hppValue = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);
                    productQty = Convert.ToDouble(selectedRow.Cells["qty"].Value);
                }
                else
                {
                    //changes on qty
                    productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);
                    hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
                }

                subTotal = Math.Round((hppValue * productQty), 2);

                selectedRow.Cells["subtotal"].Value = subTotal;

                calculateTotal();
            }
            catch (Exception ex)
            {
                //dataGridViewTextBoxEditingControl.Text = previousInput;
            }
        }

        private void purchaseOrderDetailForm_Load(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            fillInSupplierCombo();

            addDataGridColumn();

            detailPODataGridView.EditingControlShowing += detailPODataGridView_EditingControlShowing;

            gUtil.reArrangeTabOrder(this);
        }

        private void POinvoiceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (isPOInvoiceExist())
            {
                errorLabel.Text = "NO PO SUDAH ADA";
            }
            else
            {
                errorLabel.Text = "";
            }
        }

        private bool dataValidated()
        {
            if (POinvoiceTextBox.Text.Length <= 0)
            {
                errorLabel.Text = "NO PURCHASE TIDAK BOLEH KOSONG";
                return false;
            }

            return true;
        }

        private bool saveDataTransaction()
        {
            bool result = false;

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
                errorLabel.Text = "";
                gUtil.showSuccess(gUtil.INS);
            }
        }
    }
}
