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
    public partial class pembayaranHutangForm : Form
    {
        private globalUtilities gutil = new globalUtilities();
        private Data_Access DS = new Data_Access();
        private CultureInfo culture = new CultureInfo("id-ID");

        private string selectedPOInvoice = "";
        private int selectedSupplierID = 0;
        private double globalTotalValue = 0;
        private int selectedDebtID = 0;
        private bool isLoading = false;
        private bool isPaymentExceed = false;
        private int purchasePaid = 0;

        public pembayaranHutangForm()
        {
            InitializeComponent();
        }

        public pembayaranHutangForm(string poInvoice)
        {
            InitializeComponent();
            selectedPOInvoice = poInvoice;
        }

        private void fillInPaymentMethod()
        {
            MySqlDataReader rdr;
            string sqlCommand = "";

            sqlCommand = "SELECT PM_NAME FROM PAYMENT_METHOD";

            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    paymentCombo.Items.Clear();
                    while (rdr.Read())
                        paymentCombo.Items.Add(rdr.GetString("PM_NAME"));

                    paymentCombo.Text = paymentCombo.Items[0].ToString();
                }
            }
        }

        private void loadDataHeaderPO()
        {
            MySqlDataReader rdr;
            string sqlCommand;
            DateTime poDate;

            sqlCommand = "SELECT H.*, M.SUPPLIER_FULL_NAME FROM PURCHASE_HEADER H, MASTER_SUPPLIER M WHERE PURCHASE_INVOICE = '" + selectedPOInvoice + "' AND H.SUPPLIER_ID = M.SUPPLIER_ID";

            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        poInvoiceTextBox.Text = rdr.GetString("PURCHASE_INVOICE");
                        selectedSupplierID = rdr.GetInt32("SUPPLIER_ID");
                        supplierNameTextBox.Text = rdr.GetString("SUPPLIER_FULL_NAME");
                        poDate = rdr.GetDateTime("PURCHASE_DATETIME");
                        poDateTextBox.Text = String.Format(culture, "{0:dd MMM yyyy}", poDate);
                        globalTotalValue = rdr.GetDouble("PURCHASE_TOTAL");
                        purchasePaid = rdr.GetInt32("PURCHASE_PAID");
                    }
                }
            }
        }

        private int getDebtID()
        {
            int result = 0;

            result = Convert.ToInt32(DS.getDataSingleValue("SELECT IFNULL(DEBT_ID, 0) FROM DEBT WHERE PURCHASE_INVOICE = '" + selectedPOInvoice + "'"));

            return result;
        }

        private void loadDataDetailPO()
        {
            MySqlDataReader rdr;
            DataTable dt = new DataTable();
            string sqlCommand = "";

            sqlCommand = "SELECT D.PRODUCT_NAME AS 'NAMA PRODUK', PRODUCT_PRICE AS 'HARGA BELI', PRODUCT_QTY AS 'QTY', PURCHASE_SUBTOTAL AS 'SUBTOTAL' FROM PURCHASE_DETAIL S, MASTER_PRODUCT D WHERE S.PRODUCT_ID = D.PRODUCT_ID AND S.PURCHASE_INVOICE = '" + selectedPOInvoice + "'";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    dt.Load(rdr);
                    detailPurchaseOrderDataGridView.DataSource = dt;

                    detailPurchaseOrderDataGridView.Columns["NAMA PRODUK"].Width = 300;
                    detailPurchaseOrderDataGridView.Columns["HARGA BELI"].Width = 200;
                    detailPurchaseOrderDataGridView.Columns["QTY"].Width = 200;
                    detailPurchaseOrderDataGridView.Columns["SUBTOTAL"].Width = 200;
                }
            }

        }

        private void loadDataDetailPayment()
        {
            MySqlDataReader rdr;
            DataTable dt = new DataTable();
            string sqlCommand = "";

            sqlCommand = "SELECT PAYMENT_INVALID, PAYMENT_ID, PM_NAME AS 'TIPE', IF(PAYMENT_CONFIRMED = 1, 'Y', 'N') AS STATUS, DATE_FORMAT(PAYMENT_DATE, '%d-%M-%Y') AS 'TANGGAL', PAYMENT_NOMINAL AS 'NOMINAL', PAYMENT_DESCRIPTION AS 'DESKRIPSI' FROM PAYMENT_DEBT PC, PAYMENT_METHOD PM WHERE PC.PM_ID = PM.PM_ID AND DEBT_ID = " + selectedDebtID;
            using (rdr = DS.getData(sqlCommand))
            {
                detailPaymentDataGridView.DataSource = null;
                if (rdr.HasRows)
                {
                    dt.Load(rdr);
                    detailPaymentDataGridView.DataSource = dt;

                    detailPaymentDataGridView.Columns["PAYMENT_ID"].Visible = false;
                    detailPaymentDataGridView.Columns["PAYMENT_INVALID"].Visible = false;
                    detailPaymentDataGridView.Columns["TIPE"].Visible = false;
                    //detailPaymentDataGridView.Columns["STATUS"].Visible = false;
                    detailPaymentDataGridView.Columns["TANGGAL"].Width = 200;
                    detailPaymentDataGridView.Columns["NOMINAL"].Width = 200;
                    detailPaymentDataGridView.Columns["DESKRIPSI"].Width = 300;

                    for (int i = 0; i < detailPaymentDataGridView.Rows.Count; i++)
                    {
                        if (detailPaymentDataGridView.Rows[i].Cells["STATUS"].Value.ToString().Equals("N") && detailPaymentDataGridView.Rows[i].Cells["PAYMENT_INVALID"].Value.ToString().Equals("0"))
                            detailPaymentDataGridView.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                }
            }
        }

        private void calculateTotalDebt(bool globalCalculation = false)
        {
            double totalPayment = 0;

            if (globalCalculation)
            {
                globalTotalValue = Convert.ToDouble(DS.getDataSingleValue("SELECT PURCHASE_TOTAL FROM PURCHASE_HEADER WHERE PURCHASE_INVOICE = '" + selectedPOInvoice + "'"));
            }

            totalPayment = Convert.ToDouble(DS.getDataSingleValue("SELECT IFNULL(SUM(PAYMENT_NOMINAL), 0) AS PAYMENT FROM PAYMENT_DEBT WHERE DEBT_ID = " + selectedDebtID + " AND PAYMENT_INVALID = 0"));

            globalTotalValue = globalTotalValue - totalPayment;

            totalLabel.Text = globalTotalValue.ToString("C", culture);

            if (globalTotalValue <= 0)
                saveButton.Enabled = false;
        }

        private void totalPaymentMaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            totalPaymentMaskedTextBox.Text = gutil.allTrim(totalPaymentMaskedTextBox.Text);
        }
        
        private bool dataValidated()
        {
            double nominalPayment;

            nominalPayment = Convert.ToDouble(totalPaymentMaskedTextBox.Text);
            if (nominalPayment > globalTotalValue)
            {
                if (DialogResult.Yes == MessageBox.Show("PEMBAYARAN MELEBIHI JUMLAH HUTANG, LANJUTKAN ?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    isPaymentExceed = true;
                }
                else
                    return false;
            }

            return true;
        }

        public bool saveDataTransaction()
        {
            bool result = false;
            string sqlCommand = "";
            string paymentDateTime = "";
            DateTime selectedPaymentDate;
            double paymentNominal = 0;
            int paymentMethod = 0;

            string paymentDescription = "";
            int paymentConfirmed = 0;

            MySqlException internalEX = null;

            selectedPaymentDate = paymentDateTimePicker.Value;
            paymentDateTime = String.Format(culture, "{0:dd-MM-yyyy}", selectedPaymentDate);
            paymentNominal = Convert.ToDouble(totalPaymentMaskedTextBox.Text);
            paymentDescription = descriptionTextBox.Text;
            paymentMethod = paymentCombo.SelectedIndex;

            if (paymentNominal > globalTotalValue)
                paymentNominal = globalTotalValue;

            if (paymentMethod < 3)
                paymentConfirmed = 1;

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                // SAVE HEADER TABLE
                sqlCommand = "INSERT INTO PAYMENT_DEBT (DEBT_ID, PAYMENT_DATE, PM_ID, PAYMENT_NOMINAL, PAYMENT_DESCRIPTION, PAYMENT_CONFIRMED) VALUES " +
                                    "(" + selectedDebtID+ ", STR_TO_DATE('" + paymentDateTime + "', '%d-%m-%Y'), 1, " + paymentNominal + ", '" + paymentDescription + "', " + paymentConfirmed + ")";

                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                    throw internalEX;

                if (paymentNominal == globalTotalValue && paymentConfirmed == 1)
                {
                    // UPDATE CREDIT TABLE
                    sqlCommand = "UPDATE DEBT SET DEBT_PAID = 1 WHERE DEBT_ID = " + selectedDebtID;

                    if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                        throw internalEX;

                    // UPDATE SALES HEADER TABLE
                    sqlCommand = "UPDATE PURCHASE_HEADER SET PURCHASE_PAID = 1 WHERE PURCHASE_INVOICE = '" + selectedPOInvoice + "'";

                    if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                        throw internalEX;
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

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveData())
            {
                gutil.showSuccess(gutil.INS);

                if (isPaymentExceed)
                {
                    MessageBox.Show("UANG KEMBALI SEBESAR " + (Convert.ToDouble(totalPaymentMaskedTextBox.Text) - globalTotalValue).ToString("C", culture));
                }

                loadDataDetailPayment();
                calculateTotalDebt(true);
            }
        }

        private void pembayaranHutangForm_Load(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            paymentDateTimePicker.CustomFormat = globalUtilities.CUSTOM_DATE_FORMAT;

            fillInPaymentMethod();

            isLoading = true;

            loadDataHeaderPO();
            loadDataDetailPO();

            selectedDebtID = getDebtID();
            loadDataDetailPayment();

            calculateTotalDebt();

            isLoading = false;

            gutil.reArrangeTabOrder(this);
        }

        private void pembayaranHutangForm_Activated(object sender, EventArgs e)
        {
            //if need something
        }

        private bool checkDebtStatus()
        {
            bool result = true;
            string sqlCommand;
            int numOfUnconfirmedPayment;
            MySqlException internalEX = null;

            sqlCommand = "SELECT COUNT(1) FROM PAYMENT_DEBT WHERE DEBT_ID = " + selectedDebtID + " AND PAYMENT_CONFIRMED = 0";
            numOfUnconfirmedPayment = Convert.ToInt32(DS.getDataSingleValue(sqlCommand));

            if (numOfUnconfirmedPayment <= 0)
            {
                if (globalTotalValue <= 0)
                {
                    DS.beginTransaction();

                    try
                    {
                        DS.mySqlConnect();

                        // UPDATE DEBT TABLE
                        sqlCommand = "UPDATE DEBT SET DEBT_PAID = 1 WHERE DEBT_ID = " + selectedDebtID;

                        if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            throw internalEX;

                        // UPDATE PURCHASE HEADER TABLE
                        sqlCommand = "UPDATE PURCHASE_HEADER SET PURCHASE_PAID = 1 WHERE PURCHASE_INVOICE = '" + selectedPOInvoice + "'";

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
                }
            }

            return result;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (detailPaymentDataGridView.Rows.Count <= 0)
                return;

            int rowSelectedIndex = detailPaymentDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailPaymentDataGridView.Rows[rowSelectedIndex];

            if (!selectedRow.Cells["STATUS"].Value.ToString().Equals("N") || !selectedRow.Cells["PAYMENT_INVALID"].Value.ToString().Equals("0"))
            {
                invalidPayment.Enabled = false;
                confirmBayar.Enabled = false;
            }
            else
            {
                invalidPayment.Enabled = true;
                confirmBayar.Enabled = true;
            }
        }

        private bool invalidPembayaran(string paymentID)
        {
            bool result = false;
            string sqlCommand;
            MySqlException internalEX = null;

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                // SAVE HEADER TABLE
                sqlCommand = "UPDATE PAYMENT_DEBT SET PAYMENT_INVALID = 1 WHERE PAYMENT_ID = " + paymentID;

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

        private void invalidPayment_Click(object sender, EventArgs e)
        {
            string selectedPaymentID = "";

            if (detailPaymentDataGridView.Rows.Count <= 0)
                return;

            int rowSelectedIndex = detailPaymentDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailPaymentDataGridView.Rows[rowSelectedIndex];

            selectedRow.DefaultCellStyle.BackColor = Color.Red;

            if (DialogResult.Yes == MessageBox.Show("PEMBAYARAN TIDAK VALID ? ", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
            {
                selectedPaymentID = selectedRow.Cells["PAYMENT_ID"].Value.ToString();

                if (invalidPembayaran(selectedPaymentID))
                {
                    calculateTotalDebt(true);
                    if (checkDebtStatus())
                    {
                        gutil.showSuccess(gutil.INS);
                    }

                    loadDataDetailPayment();
                }
            }

            selectedRow.DefaultCellStyle.BackColor = Color.White;
        }

        private bool confirmPembayaran(string paymentID)
        {
            bool result = false;
            string sqlCommand;
            MySqlException internalEX = null;
            DateTime selectedPaymentDate;
            string paymentDateTime;

            selectedPaymentDate = paymentDateTimePicker.Value;
            paymentDateTime = String.Format(culture, "{0:dd-MM-yyyy}", selectedPaymentDate);

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                // SAVE HEADER TABLE
                sqlCommand = "UPDATE PAYMENT_DEBT SET PAYMENT_CONFIRMED = 1, PAYMENT_CONFIRMED_DATE = STR_TO_DATE('" + paymentDateTime + "', '%d-%m-%Y') WHERE PAYMENT_ID = " + paymentID;

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

        private void confirmBayar_Click(object sender, EventArgs e)
        {
            string selectedPaymentID = "";

            if (detailPaymentDataGridView.Rows.Count <= 0)
                return;

            int rowSelectedIndex = detailPaymentDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = detailPaymentDataGridView.Rows[rowSelectedIndex];

            if (selectedRow.Cells["STATUS"].Value.ToString().Equals("N"))
            {
                selectedRow.DefaultCellStyle.BackColor = Color.Red;

                if (DialogResult.Yes == MessageBox.Show("KONFIRMASI PEMBAYARAN ? ", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    selectedPaymentID = selectedRow.Cells["PAYMENT_ID"].Value.ToString();

                    if (confirmPembayaran(selectedPaymentID))
                    {
                        calculateTotalDebt(true);
                        if (checkDebtStatus())
                        {
                            gutil.showSuccess(gutil.INS);
                        }

                        loadDataDetailPayment();
                    }
                }
                selectedRow.DefaultCellStyle.BackColor = Color.White;
            }
        }
    }
}
