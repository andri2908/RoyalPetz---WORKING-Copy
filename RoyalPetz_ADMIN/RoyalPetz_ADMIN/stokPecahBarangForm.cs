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
    public partial class stokPecahBarangForm : Form
    {
        private int newSelectedInternalProductID = 0;
        private int selectedInternalProductID = 0;
        private int selectedUnitID = 0;
        private List<int> selectedKategoriID = new List<int>();

        private Data_Access DS = new Data_Access();

        public stokPecahBarangForm()
        {
            InitializeComponent();
        }

        public stokPecahBarangForm(int productID)
        {
            InitializeComponent();
            selectedInternalProductID = productID;
        }

        public void setNewSelectedProductID(int productID)
        {
            newSelectedInternalProductID = productID;
        }

        private void newProduk_Click(object sender, EventArgs e)
        {
            dataProdukDetailForm displayForm = new dataProdukDetailForm(globalConstants.STOK_PECAH_BARANG);
            displayForm.ShowDialog(this);
        }

        private void browseProdukButton_Click(object sender, EventArgs e)
        {
            dataProdukForm displayForm = new dataProdukForm(globalConstants.BROWSE_STOK_PECAH_BARANG);
            displayForm.ShowDialog(this);
        }

        private void loadProductInformation()
        {
            MySqlDataReader rdr;
            DataTable dt = new DataTable();

            DS.mySqlConnect();

            // LOAD PRODUCT DATA
            using (rdr = DS.getData("SELECT * FROM MASTER_PRODUCT WHERE ID =  " + selectedInternalProductID))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        productIDTextBox.Text = rdr.GetString("PRODUCT_ID");
                        productNameTextBox.Text = rdr.GetString("PRODUCT_NAME");
                        hppTextBox.Text = rdr.GetString("PRODUCT_BASE_PRICE");
                        hargaEcerTextBox.Text = rdr.GetString("PRODUCT_RETAIL_PRICE");
                        hargaPartaiTextBox.Text = rdr.GetString("PRODUCT_BULK_PRICE");
                        hargaGrosirTextBox.Text = rdr.GetString("PRODUCT_WHOLESALE_PRICE"); ;
                        stockTextBox.Text = rdr.GetString("PRODUCT_STOCK_QTY");                        

                        selectedUnitID = rdr.GetInt32("UNIT_ID");
                    }
                }
            }

        }

        private void loadCategoryInformation()
        {
            MySqlDataReader rdr;
            DataTable dt = new DataTable();
            string kategoriInformation = "";

            DS.mySqlConnect();

            using (rdr = DS.getData("SELECT P.*, M.CATEGORY_NAME FROM PRODUCT_CATEGORY P, MASTER_CATEGORY M WHERE PRODUCT_ID =  '"+ productIDTextBox.Text +"' AND P.CATEGORY_ID = M.CATEGORY_ID" ))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        selectedKategoriID.Add(rdr.GetInt32("CATEGORY_ID"));

                        if (kategoriInformation.Equals(""))
                            kategoriInformation = rdr.GetString("CATEGORY_NAME");
                        else
                            kategoriInformation = kategoriInformation + ", "  + rdr.GetString("CATEGORY_NAME");
                    }
                }
            }

            productCategoryTextBox.Text = kategoriInformation;

        }

        private void loadUnitInformation()
        {
            MySqlDataReader rdr;
            DataTable dt = new DataTable();

            DS.mySqlConnect();

            using (rdr = DS.getData("SELECT * FROM MASTER_UNIT WHERE UNIT_ID =  " + selectedUnitID ))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        unitTextBox.Text = rdr.GetString("UNIT_NAME");
                    }
                }
            }
        }

        private void stokPecahBarangForm_Load(object sender, EventArgs e)
        {
            loadProductInformation();

            loadUnitInformation();

            loadCategoryInformation();
        }
    }
}
