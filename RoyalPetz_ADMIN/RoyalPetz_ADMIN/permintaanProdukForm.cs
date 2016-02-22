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
    public partial class permintaanProdukForm : Form
    {
        private int originModuleID = 0;
        private int selectedBranchFromID = 0;
        private int selectedBranchToID = 0;
        private Data_Access DS = new Data_Access();
        List<string> detailRequestQty = new List<string>();
        string previousInput = "";
        private globalUtilities gUtil = new globalUtilities();
        
        public permintaanProdukForm()
        {
            InitializeComponent();
        }

        public permintaanProdukForm(int moduleID)
        {
            InitializeComponent();
            originModuleID = moduleID;
        }

        private int getProductID(int selectedIndex)
        {
            string productID = "";
            productID = productIDComboHidden.Items[selectedIndex].ToString();
            return Convert.ToInt32(productID);
        }

        private void calculateTotal()
        {
            double total = 0;

            for (int i = 0; i<detailRequestOrderDataGridView.Rows.Count;i++)
            {
                total = total + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value);
            }

            totalLabel.Text = "Rp. " + total.ToString();
        }

        private void detailRequestOrderDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (detailRequestOrderDataGridView.CurrentCell.ColumnIndex == 0 && e.Control is ComboBox)
            {
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }

            if (detailRequestOrderDataGridView.CurrentCell.ColumnIndex == 1 && e.Control is TextBox)
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

            DataGridViewTextBoxEditingControl dataGridViewTextBoxEditingControl = sender as DataGridViewTextBoxEditingControl;
            
            rowSelectedIndex = detailRequestOrderDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailRequestOrderDataGridView.Rows[rowSelectedIndex];

            previousInput = "";
            if ( detailRequestQty.Count < rowSelectedIndex+1 )
            {
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL))
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
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL))
                {
                    detailRequestQty[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                }
                else
                {
                    dataGridViewTextBoxEditingControl.Text = detailRequestQty[rowSelectedIndex];
                }
            }

            productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);

            if (null != selectedRow.Cells["hpp"].Value)
            {
                hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
                subTotal = Math.Round((hppValue * productQty), 2);

                selectedRow.Cells["subTotal"].Value = subTotal;
            }
  
            calculateTotal();
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = 0;
            int rowSelectedIndex = 0;
            int selectedProductID = 0;
            double hpp = 0;
            double productQty = 0;
            double subTotal = 0;

            DataGridViewComboBoxEditingControl dataGridViewComboBoxEditingControl = sender as DataGridViewComboBoxEditingControl;

            selectedIndex = dataGridViewComboBoxEditingControl.SelectedIndex;
            selectedProductID = getProductID(selectedIndex);
            hpp = getHPPValue(selectedProductID);

            rowSelectedIndex = detailRequestOrderDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailRequestOrderDataGridView.Rows[rowSelectedIndex];

            selectedRow.Cells["hpp"].Value = hpp;

            if (null != selectedRow.Cells["qty"].Value)
            {
                productQty = Convert.ToDouble(selectedRow.Cells["qty"].Value);
                subTotal = Math.Round((hpp * productQty),2);
 
                selectedRow.Cells["subTotal"].Value = subTotal;
            }

            calculateTotal();
        }
        
        private void exportButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            MessageBox.Show(saveFileDialog1.FileName);
        }

        private double getHPPValue(int productID)
        {
            double result = 0;

            DS.mySqlConnect();

            result = Convert.ToDouble(DS.getDataSingleValue("SELECT IFNULL(PRODUCT_BASE_PRICE, 0) FROM MASTER_PRODUCT WHERE ID = " + productID));

            return result;
        }

        private void fillInProductNameCombo()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            DataGridViewComboBoxColumn productNameCmb = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn stockQtyColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn basePriceColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn subTotalColumn = new DataGridViewTextBoxColumn();

            sqlCommand = "SELECT ID, PRODUCT_NAME FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1 ORDER BY PRODUCT_NAME ASC";

            productIDComboHidden.Items.Clear();
            productNameComboHidden.Items.Clear();

            using (rdr = DS.getData(sqlCommand))
            {
                while (rdr.Read())
                {
                    productNameCmb.Items.Add(rdr.GetString("PRODUCT_NAME"));
                    productIDComboHidden.Items.Add(rdr.GetString("ID"));
                    productNameComboHidden.Items.Add(rdr.GetString("PRODUCT_NAME"));
                }
            }

            // PRODUCT NAME COLUMN

            productNameCmb.HeaderText = "NAMA PRODUK";
            productNameCmb.Name = "productName";
            productNameCmb.Width = 300;
            //productNameCmb.ReadOnly = true;

            detailRequestOrderDataGridView.Columns.Add(productNameCmb);

            stockQtyColumn.HeaderText = "QTY";
            stockQtyColumn.Name = "qty";
            stockQtyColumn.Width = 100;

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
            errorLabel.Text = "";
            fillInBranchFromCombo();
            fillInProductNameCombo();

            detailRequestOrderDataGridView.EditingControlShowing += detailRequestOrderDataGridView_EditingControlShowing;
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
            ROinvoiceTextBox.Text = ROinvoiceTextBox.Text.Trim();

            if (invoiceExist())
            {
                errorLabel.Text = "NO PERMINTAAN SUDAH ADA";
            }
            else
            {
                errorLabel.Text = "";
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
        }
        
        private void fillInBranchToCombo()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT BRANCH_ID, BRANCH_NAME FROM MASTER_BRANCH WHERE BRANCH_ACTIVE = 1 AND BRANCH_ID <> " + selectedBranchFromID + " ORDER BY BRANCH_NAME ASC";

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
        }

        private void branchFromCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = branchFromCombo.SelectedIndex;

            selectedBranchFromID = Convert.ToInt32(branchFromComboHidden.Items[selectedIndex]);

            fillInBranchToCombo();
        }

        private void detailRequestOrderDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void branchToCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedBranchToID = Convert.ToInt32(branchToComboHidden.Items[branchToCombo.SelectedIndex].ToString());
        }

        private void detailRequestOrderDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
        }

        private bool dataValidated()
        {
            return true;
        }

        private bool saveDataTransaction()
        {
            bool result = false;
            string sqlCommand = "";

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                
                DS.executeNonQueryCommand(sqlCommand);

                DS.commit();
            }
            catch (Exception e)
            {
                try
                {
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
                result = true;
            }

            return result;
        }

        private bool saveData()
        {
            if (dataValidated())
            {
                return savedataTransaction();
            }

            return false;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveData())
            {
                MessageBox.Show("SUCCESS");
            }
        }
    }
}
