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
    public partial class penerimaanBarangForm : Form
    {
        string selectedPMInvoice;
        int originModuleId = 0;
        int selectedFromID = 0;
        int selectedToID = 0;
        double globalTotalValue = 0;
        bool isLoading = false;
        private List<string> detailRequestQty = new List<string>();
        private List<string> detailHpp = new List<string>();
        string previousInput = "";
        globalUtilities gUtil = new globalUtilities();
        Data_Access DS = new Data_Access();

        public penerimaanBarangForm()
        {
            InitializeComponent();
        }

        public penerimaanBarangForm(int moduleID, string pmInvoice)
        {
            InitializeComponent();

            originModuleId = moduleID;
            selectedPMInvoice = pmInvoice;
        }

        private void initializeScreen()
        {
            switch (originModuleId)
            {
                case globalConstants.PENERIMAAN_BARANG_DARI_MUTASI:
                    labelNo.Text = "NO MUTASI";
                    labelTanggal.Text = "TANGGAL MUTASI";
                    labelAsal.Text = "ASAL MUTASI";
                    labelTujuan.Text = "TUJUAN MUTASI";
                    break;

                case globalConstants.PENERIMAAN_BARANG_DARI_PO:
                    break;
            }
        }

        private void loadDataHeader()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            switch (originModuleId)
            {
                case globalConstants.PENERIMAAN_BARANG_DARI_MUTASI:
                    sqlCommand = "SELECT * FROM PRODUCTS_MUTATION_HEADER WHERE PM_INVOICE = '" + selectedPMInvoice + "'";
                    using (rdr = DS.getData(sqlCommand))
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                noInvoiceTextBox.Text = rdr.GetString("PM_INVOICE");
                                invoiceDtPicker.Value = rdr.GetDateTime("PM_DATETIME");
                                selectedFromID = rdr.GetInt32("BRANCH_ID_FROM");
                                selectedToID = rdr.GetInt32("BRANCH_ID_TO");
                                labelTotalValue.Text = "Rp. " + rdr.GetString("PM_TOTAL");
                                labelAcceptValue.Text = "Rp. " + rdr.GetString("PM_TOTAL");

                                globalTotalValue = rdr.GetDouble("PM_TOTAL");
                            }
                        }
                    }
                    break;
            }
        }

        private void loadDataDetail()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            switch (originModuleId)
            {
                case globalConstants.PENERIMAAN_BARANG_DARI_MUTASI:
                    sqlCommand = "SELECT PM.*, M.PRODUCT_NAME FROM PRODUCTS_MUTATION_DETAIL PM, MASTER_PRODUCT M WHERE PM_INVOICE = '" + selectedPMInvoice + "' AND PM.PRODUCT_ID = M.PRODUCT_ID";
                    using (rdr = DS.getData(sqlCommand))
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                detailGridView.Rows.Add(rdr.GetString("PRODUCT_NAME"), rdr.GetString("PRODUCT_QTY"), rdr.GetString("PRODUCT_BASE_PRICE"), rdr.GetString("PRODUCT_QTY"), rdr.GetString("PM_SUBTOTAL"));

                                detailRequestQty.Add(rdr.GetString("PRODUCT_QTY"));
                                detailHpp.Add(rdr.GetString("PRODUCT_BASE_PRICE"));
                            }
                        }
                    }
                    break;
            }
        }

        private string getBranchName(int branchID)
        {
            string result = "";

            result = DS.getDataSingleValue("SELECT BRANCH_NAME FROM MASTER_BRANCH WHERE BRANCH_ID = " + branchID).ToString();

            return result;
        }

        private void penerimaanBarangForm_Load(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            initializeScreen();

            detailGridView.EditingControlShowing += detailGridView_EditingControlShowing;

            isLoading = true;
            
            loadDataHeader();
            loadDataDetail();

            branchFromTextBox.Text = getBranchName(selectedFromID);
            branchToTextBox.Text = getBranchName(selectedToID);

            isLoading = false;

            gUtil.reArrangeTabOrder(this);
        }

        private void detailGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if ((detailGridView.CurrentCell.ColumnIndex == 2 || detailGridView.CurrentCell.ColumnIndex == 3) && e.Control is TextBox)
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

            rowSelectedIndex = detailGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailGridView.Rows[rowSelectedIndex];

            previousInput = "";
            if (detailRequestQty.Count < rowSelectedIndex + 1)
            {
                if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL)
                    && (dataGridViewTextBoxEditingControl.Text.Length > 0))
                {
                    if (detailGridView.CurrentCell.ColumnIndex == 2 )
                        detailHpp.Add(dataGridViewTextBoxEditingControl.Text);
                    else
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
                    if (detailGridView.CurrentCell.ColumnIndex == 2)
                        detailHpp[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                    else
                        detailRequestQty[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                }
                else
                {
                    if (detailGridView.CurrentCell.ColumnIndex == 2)
                        dataGridViewTextBoxEditingControl.Text = detailHpp[rowSelectedIndex];
                    else
                        dataGridViewTextBoxEditingControl.Text = detailRequestQty[rowSelectedIndex];
                }
            }

            try
            {
                if (detailGridView.CurrentCell.ColumnIndex == 2)
                {
                    //changes on hpp
                    hppValue = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);
                    productQty = Convert.ToDouble(selectedRow.Cells["qtyReceived"].Value);
                }
                else
                {
                    //changes on qty
                    productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);
                    hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
                }

                subTotal = Math.Round((hppValue * productQty), 2);

                selectedRow.Cells["subtotal"].Value = subTotal;
                //if (detailGridView.CurrentCell.ColumnIndex == 2)
                //{
                //    productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);
                //    hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
                //}
                //else
                //    productQty = Convert.ToDouble(selectedRow.Cells["qtyReceived"].Value);

                //if (null != selectedRow.Cells["hpp"].Value)
                //{
                //    hppValue = Convert.ToDouble(selectedRow.Cells["hpp"].Value);
                //    subTotal = Math.Round((hppValue * productQty), 2);

                //    selectedRow.Cells["subtotal"].Value = subTotal;
                //}

                calculateTotal();
            }
            catch (Exception ex)
            {
                //dataGridViewTextBoxEditingControl.Text = previousInput;
            }
        }

        private void calculateTotal()
        {
            double total = 0;
            for (int i =0;i<detailGridView.Rows.Count;i++)
            {
                total = total + Convert.ToDouble(detailGridView.Rows[i].Cells["subtotal"].Value);
            }

            globalTotalValue = total;
            labelAcceptValue.Text = "Rp. " + globalTotalValue;
        }

        private bool isNoPRExist()
        {
            bool result = false;

            if (Convert.ToInt32(DS.getDataSingleValue("SELECT COUNT(1) FROM PRODUCTS_RECEIVED_HEADER WHERE PR_INVOICE = '" + prInvoiceTextBox.Text + "'")) > 0)
                result = true;

            return result;
        }

        private void prInvoiceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (isNoPRExist())
            {
                errorLabel.Text = "NO PENERIMAAN SUDAH ADA";
            }
        }

        private bool dataValidated()
        {
            return true;
        }

        private bool saveDataTransaction()
        {
            return true;
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
                gUtil.showSuccess(gUtil.INS);
            }
        }
    }
}