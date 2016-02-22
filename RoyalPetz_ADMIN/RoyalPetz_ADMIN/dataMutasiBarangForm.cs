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
    public partial class dataMutasiBarangForm : Form
    {
        private int originModuleID = 0;
        private int selectedROID = 0;

        private Data_Access DS = new Data_Access();

        public dataMutasiBarangForm()
        {
            InitializeComponent();
        }

        public dataMutasiBarangForm(int moduleID)
        {
            InitializeComponent();
            originModuleID = moduleID;

            if ((moduleID != globalConstants.PERMINTAAN_BARANG) && (moduleID != globalConstants.CEK_DATA_MUTASI))
                newButton.Visible = false;
        }

        private void displaySpecificForm()
        {
            switch (originModuleID)
            {
                case globalConstants.CEK_DATA_MUTASI:
                        dataMutasiBarangDetailForm displayedForm = new dataMutasiBarangDetailForm();
                        displayedForm.ShowDialog(this);
                    break;

                case globalConstants.PERMINTAAN_BARANG:
                        permintaanProdukForm permintaanProdukDisplayedForm = new permintaanProdukForm(globalConstants.NEW_REQUEST_ORDER);
                        permintaanProdukDisplayedForm.ShowDialog(this);
                    break;

                case globalConstants.PENERIMAAN_BARANG:
                        penerimaanBarangForm penerimaanBarangDisplayedForm = new penerimaanBarangForm();
                        penerimaanBarangDisplayedForm.ShowDialog(this);
                    break;
            }
        }

        private void dataSalesDataGridView_DoubleClick(object sender, EventArgs e)
        {
            displaySpecificForm();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            displaySpecificForm();
        }

        private void loadROdata()
        {
            MySqlDataReader rdr;
            DataTable dt = new DataTable();
            string sqlCommand;

            DS.mySqlConnect();

            sqlCommand = "SELECT * FROM REQUEST_ORDER_HEADER WHERE RO_ACTIVE = 1 AND RO_EXPIRED > " + DateTime.Now;

            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    dt.Load(rdr);
                    dataRequestOrderGridView.DataSource = dt;

                    dataRequestOrderGridView.Columns["ID"].Visible = false;
                    //dataPelangganDataGridView.Columns["CUSTOMER_ID"].Visible = false;
                    //dataPelangganDataGridView.Columns["NAMA PELANGGAN"].Width = 300;
                    //dataPelangganDataGridView.Columns["TANGGAL BERGABUNG"].Width = 200;
                    //dataPelangganDataGridView.Columns["GROUP CUSTOMER"].Width = 200;
                }
            }


        }

        private void dataMutasiBarangForm_Load(object sender, EventArgs e)
        {

        }
    }
}
