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

        private Data_Access DS = new Data_Access();
        private List<string> detailRequestQtyApproved = new List<string>();

        private globalUtilities gUtil = new globalUtilities();
        private CultureInfo culture = new CultureInfo("id-ID");

        public dataMutasiBarangDetailForm()
        {
            InitializeComponent();
        }

        public dataMutasiBarangDetailForm(int moduleID, int roID)
        {
            InitializeComponent();

            originModuleID = moduleID;
            
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
            totalApproved.Text = "Rp. " + total.ToString();
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
            if (detailRequestOrderDataGridView.CurrentCell.ColumnIndex == 2 && e.Control is TextBox)
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

            if (gUtil.matchRegEx(dataGridViewTextBoxEditingControl.Text, globalUtilities.REGEX_NUMBER_WITH_2_DECIMAL) && (dataGridViewTextBoxEditingControl.Text.Length>0))
            {
                productQty = Convert.ToDouble(dataGridViewTextBoxEditingControl.Text);

                if (stockIsEnough(selectedRow.Cells["productID"].Value.ToString(), productQty))
                {
                    detailRequestQtyApproved[rowSelectedIndex] = dataGridViewTextBoxEditingControl.Text;
                    errorLabel.Text = "";
                    dataGridViewTextBoxEditingControl.BackColor = Color.White;
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

            if (0 == Convert.ToInt32(DS.getDataSingleValue("SELECT RO_ACTIVE FROM REQUEST_ORDER_HEADER WHERE RO_INVOICE = '" + selectedROInvoice + "'")))
                result = true;

            return result;
        }

        private void dataMutasiBarangDetailForm_Load(object sender, EventArgs e)
        {
            errorLabel.Text = "";

            isLoading = true;            

            if (isNewRORequest())
            {
                subModuleID = globalConstants.NEW_PRODUCT_MUTATION;
                loadDataHeaderRO();            
                loadDataDetailRO();
            }
            else
            {
                subModuleID = globalConstants.EDIT_PRODUCT_MUTATION;
                noMutasiTextBox.ReadOnly = true;
            }

            branchFromTextBox.Text = getBranchName(selectedBranchFromID);
            branchToTextBox.Text = getBranchName(selectedBranchToID);

            isLoading = false;

            detailRequestOrderDataGridView.EditingControlShowing += detailRequestOrderDataGridView_EditingControlShowing;

            gUtil.reArrangeTabOrder(this);
        }

        private bool saveDataTransaction()
        {
            bool result = false;
            string sqlCommand = "";

            string roInvoice = "";
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

                        // UPDATE REQUEST ORDER HEADER TABLE
                        sqlCommand = "UPDATE REQUEST_ORDER_HEADER SET RO_ACTIVE = 0 WHERE RO_INVOICE = '" + roInvoice + "'";
                        DS.executeNonQueryCommand(sqlCommand);
                        break;

                    case globalConstants.EDIT_PRODUCT_MUTATION:
                        // UPDATE HEADER TABLE
                        //sqlCommand = "UPDATE REQUEST_ORDER_HEADER SET " +
                        //                    "RO_BRANCH_ID_FROM = " + branchIDFrom + ", " +
                        //                    "RO_BRANCH_ID_TO = " + branchIDTo + ", " +
                        //                    "RO_DATETIME = STR_TO_DATE('" + roDateTime + "', '%d-%m-%Y'), " +
                        //                    "RO_TOTAL = " + roTotal + ", " +
                        //                    "RO_EXPIRED = STR_TO_DATE('" + roDateExpired + "', '%d-%m-%Y') " +
                        //                    "WHERE RO_INVOICE = '" + roInvoice + "'";

                       // DS.executeNonQueryCommand(sqlCommand);

                        // DELETE DETAIL TABLE
                        //sqlCommand = "DELETE FROM REQUEST_ORDER_DETAIL WHERE RO_INVOICE = '" + roInvoice + "'";
                        //DS.executeNonQueryCommand(sqlCommand);

                        // RE-INSERT DETAIL TABLE
                        for (int i = 0; i < detailRequestOrderDataGridView.Rows.Count; i++)
                        {
                            //if (null != detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value)
                            //{
                            //    sqlCommand = "INSERT INTO REQUEST_ORDER_DETAIL (RO_INVOICE, PRODUCT_ID, PRODUCT_BASE_PRICE, RO_QTY, RO_SUBTOTAL) VALUES " +
                            //                        "('" + roInvoice + "', '" + detailRequestOrderDataGridView.Rows[i].Cells["productID"].Value.ToString() + "', " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["hpp"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["qty"].Value) + ", " + Convert.ToDouble(detailRequestOrderDataGridView.Rows[i].Cells["subTotal"].Value) + ")";

                            //    DS.executeNonQueryCommand(sqlCommand);
                            //}
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

            if (noMutasiExist())
            {
                errorLabel.Text = "NO MUTASI SUDAH ADA";
                noMutasiTextBox.Focus();
            }
        }
    }
}
