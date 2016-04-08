﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.IO;

using MySql.Data;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace RoyalPetz_ADMIN
{
    public partial class sinkronisasiInformasiForm : Form
    {
        private globalUtilities gutil = new globalUtilities();
        private Data_Access DS = new Data_Access();
        private CultureInfo culture = new CultureInfo("id-ID");

        public sinkronisasiInformasiForm()
        {
            InitializeComponent();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            string fileName = "";
            openFileDialog1.Filter = "SQL File (.sql)|*.sql";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    fileName = openFileDialog1.FileName;
                    fileNameTextbox.Text = fileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
       
        private void exportDataMasterProduk(string fileName)
        {
            //string localDate = "";
            //string strCmdText = "";
            //string ipServer;
            //System.Diagnostics.Process proc = new System.Diagnostics.Process();
            MySqlDataReader rdr;
            string sqlCommand = "";
            string insertStatement;
            StreamWriter sw = null;

            // EXPORT MASTER PRODUCT DATA
            string strCmdText =  "USE `sys_pos`; " + "\n" +
                                        "DROP TABLE IF EXISTS `temp_master_product`;" + "\n" +
                                        "CREATE TABLE `temp_master_product` (" + "\n" +
                                        "`ID` int(10) unsigned NOT NULL AUTO_INCREMENT," + "\n" +
                                        "`PRODUCT_ID` varchar(50) DEFAULT NULL," + "\n" +
                                        "`PRODUCT_BARCODE` int(10) unsigned DEFAULT NULL," + "\n" +
                                        "`PRODUCT_NAME` varchar(50) DEFAULT NULL," + "\n" +
                                        "`PRODUCT_DESCRIPTION` varchar(100) DEFAULT NULL," + "\n" +
                                        "`PRODUCT_BASE_PRICE` double DEFAULT NULL," + "\n" +
                                        "`PRODUCT_RETAIL_PRICE` double DEFAULT NULL," + "\n" +
                                        "`PRODUCT_BULK_PRICE` double DEFAULT NULL," + "\n" +
                                        "`PRODUCT_WHOLESALE_PRICE` double DEFAULT NULL," + "\n" +
                                        "`UNIT_ID` smallint(5) unsigned DEFAULT '0'," + "\n" +
                                        "`PRODUCT_IS_SERVICE` tinyint(3) unsigned DEFAULT NULL," + "\n" +
                                        "PRIMARY KEY(`ID`)," + "\n" +
                                        "UNIQUE KEY `PRODUCT_ID_UNIQUE` (`PRODUCT_ID`)" + "\n" +
                                        ") ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;" + "\n";

            //localDate = String.Format(culture, "{0:ddMMyyyy}", DateTime.Now);
            //fileName = "SYNCINFO_PRODUCT_" + localDate + ".sql";

            sqlCommand = "SELECT PRODUCT_ID, PRODUCT_BARCODE, PRODUCT_NAME, PRODUCT_DESCRIPTION, PRODUCT_BASE_PRICE, PRODUCT_RETAIL_PRICE, PRODUCT_BULK_PRICE, PRODUCT_WHOLESALE_PRICE, UNIT_ID, PRODUCT_IS_SERVICE FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    if (!File.Exists(fileName))
                        sw = File.CreateText(fileName);
                    else
                    {
                        File.Delete(fileName);
                        sw = File.CreateText(fileName);
                    }

                    sw.WriteLine(strCmdText);

                    while (rdr.Read())
                    {
                        insertStatement = "INSERT INTO TEMP_MASTER_PRODUCT (PRODUCT_ID, PRODUCT_BARCODE, PRODUCT_NAME, PRODUCT_DESCRIPTION, PRODUCT_BASE_PRICE, PRODUCT_RETAIL_PRICE, PRODUCT_BULK_PRICE, PRODUCT_WHOLESALE_PRICE, UNIT_ID, PRODUCT_IS_SERVICE) VALUES (" +
                                                 "'" + rdr.GetString("PRODUCT_ID") + "', " + rdr.GetString("PRODUCT_BARCODE") + ", '" + rdr.GetString("PRODUCT_NAME") + "', '" + rdr.GetString("PRODUCT_DESCRIPTION") + "', " + rdr.GetString("PRODUCT_BASE_PRICE") + ", " + rdr.GetString("PRODUCT_RETAIL_PRICE") + ", " + rdr.GetString("PRODUCT_BULK_PRICE") + ", " + rdr.GetString("PRODUCT_WHOLESALE_PRICE") + ", " + rdr.GetString("UNIT_ID") + ", " + rdr.GetString("PRODUCT_IS_SERVICE") + ");";
                        sw.WriteLine(insertStatement);
                    }
                }
                rdr.Close();
            }
            sw.WriteLine("");

            // EXPORT MASTER KATEGORI DATA
            sw.WriteLine("");
            sw.WriteLine("DELETE FROM MASTER_CATEGORY;");
            sqlCommand = "SELECT CATEGORY_ID, CATEGORY_NAME, CATEGORY_DESCRIPTION FROM MASTER_CATEGORY WHERE CATEGORY_ACTIVE = 1";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        insertStatement = "INSERT INTO MASTER_CATEGORY (CATEGORY_ID, CATEGORY_NAME, CATEGORY_DESCRIPTION, CATEGORY_ACTIVE) VALUES (" +
                                                 rdr.GetString("CATEGORY_ID") + ", '" + rdr.GetString("CATEGORY_NAME") + "', '" + rdr.GetString("CATEGORY_DESCRIPTION") + "', 1);";
                        sw.WriteLine(insertStatement);
                    }
                }
                rdr.Close();
            }
            sw.WriteLine("");

            // EXPORT MASTER UNIT DATA
            sw.WriteLine("");
            sw.WriteLine("DELETE FROM MASTER_UNIT;");
            sqlCommand = "SELECT UNIT_ID, UNIT_NAME, UNIT_DESCRIPTION FROM MASTER_UNIT WHERE UNIT_ACTIVE = 1";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        insertStatement = "INSERT INTO MASTER_UNIT (UNIT_ID, UNIT_NAME, UNIT_DESCRIPTION, UNIT_ACTIVE) VALUES (" +
                                                 rdr.GetString("UNIT_ID") + ", '" + rdr.GetString("UNIT_NAME") + "', '" + rdr.GetString("UNIT_DESCRIPTION") + "', 1);";
                        sw.WriteLine(insertStatement);
                    }
                }
                rdr.Close();
            }
            sw.WriteLine("");

            // EXPORT MASTER UNIT KONVERSI DATA
            sw.WriteLine("");
            sw.WriteLine("DELETE FROM UNIT_CONVERT;");
            sqlCommand = "SELECT CONVERT_UNIT_ID_1, CONVERT_UNIT_ID_2, CONVERT_MULTIPLIER FROM UNIT_CONVERT";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        insertStatement = "INSERT INTO UNIT_CONVERT (CONVERT_UNIT_ID_1, CONVERT_UNIT_ID_2, CONVERT_MULTIPLIER) VALUES (" +
                                                 rdr.GetString("CONVERT_UNIT_ID_1") + ", " + rdr.GetString("CONVERT_UNIT_ID_2") + ", " + rdr.GetString("CONVERT_MULTIPLIER") + ");";
                        sw.WriteLine(insertStatement);
                    }
                }
                rdr.Close();
            }
            sw.WriteLine("");

            sw.Close();
            //ipServer = DS.getIPServer();
            ////strCmdText = "/C mysqldump -h " + ipServer + " -u SYS_POS_ADMIN -ppass123 sys_pos MASTER_PRODUCT > \"" + fileName + "\"";

            //proc.StartInfo.FileName = "CMD.exe";
            //proc.StartInfo.Arguments = "/C " + "mysqldump -h " + ipServer + " -u SYS_POS_ADMIN -ppass123 sys_pos > \"" + fileName + "\"";
            //proc.Exited += new EventHandler(ProcessExited);
            //proc.EnableRaisingEvents = true;
            //proc.Start();


            //System.Diagnostics.Process.Start("CMD.exe", strCmdText);
        }

        private void exportDataButton_Click(object sender, EventArgs e)
        {
            string localDate = "";
            string fileName = "";
            localDate = String.Format(culture, "{0:ddMMyyyy}", DateTime.Now);
            fileName = "SYNCINFO_PRODUCT_" + localDate + ".sql";

            saveFileDialog1.FileName = fileName;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = "sql";
            saveFileDialog1.Filter = "SQL File (.sql)|*.sql";
            saveFileDialog1.ShowDialog();
           
            exportDataMasterProduk(saveFileDialog1.FileName);

            MessageBox.Show("DONE");
        }

        private void sinkronisasiInformasiForm_Load(object sender, EventArgs e)
        {
            gutil.reArrangeTabOrder(this);
        }

        private void sinkronisasiInformasiForm_Activated(object sender, EventArgs e)
        {
            //if need something
        }

        private void ProcessExited(Object source, EventArgs e)
        {
            var proc = (System.Diagnostics.Process)source;
            if (updateLocalData())
            {
                MessageBox.Show("DONE");
            }
            
        }

        public void saveCurrentDataProduct()
        {

        }

        private void syncInformation(string fileName)
        {
            string ipServer = "";
            //string strCmdText = "";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            Directory.SetCurrentDirectory(Application.StartupPath);

            ipServer = DS.getIPServer();
            //strCmdText = "/C " + "mysql -h " + ipServer + " -u SYS_POS_ADMIN -ppass123 sys_pos < \"" + fileName + "\"";

            //System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            proc.StartInfo.FileName = "CMD.exe";
            proc.StartInfo.Arguments = "/C " + "mysql -h " + ipServer + " -u SYS_POS_ADMIN -ppass123 sys_pos < \"" + fileName + "\"";
            proc.Exited += new EventHandler(ProcessExited);
            proc.EnableRaisingEvents = true;
            proc.Start();
            

        }

        private bool updateLocalData()
        {
            bool result = false;
            string sqlCommand = "";
            string productID;
            string productBarcode;
            string productName;
            string productDescription;
            string productBasePrice;
            string productRetailPrice;
            string productBulkPrice;
            string productWholesalePrice;
            string productService;

            MySqlException internalEX = null;
            MySqlDataReader rdr;
            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            int i = 0;
            DS.beginTransaction();


            sqlCommand = "SELECT PRODUCT_ID FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1";
            try
            {
                DS.mySqlConnect();

                using (rdr = DS.getData(sqlCommand))
                {
                    if (rdr.HasRows)
                    {
                        dt.Load(rdr);
                        rdr.Close();

                        dataGridView1.DataSource = dt;
                        i = 0;
                        // UPDATE CURRENT DATA IN LOCAL DATABASE    
                        while (i < dataGridView1.Rows.Count)
                        {
                            productID = dataGridView1.Rows[i].Cells["PRODUCT_ID"].Value.ToString();

                            productBasePrice = DS.getDataSingleValue("SELECT PRODUCT_BASE_PRICE FROM TEMP_MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'").ToString();
                            productRetailPrice = DS.getDataSingleValue("SELECT PRODUCT_RETAIL_PRICE FROM TEMP_MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'").ToString();
                            productBulkPrice = DS.getDataSingleValue("SELECT PRODUCT_BULK_PRICE FROM TEMP_MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'").ToString();
                            productWholesalePrice = DS.getDataSingleValue("SELECT PRODUCT_WHOLESALE_PRICE FROM TEMP_MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'").ToString();

                            sqlCommand = "UPDATE MASTER_PRODUCT SET PRODUCT_BASE_PRICE = " + productBasePrice + ", PRODUCT_RETAIL_PRICE = " + productRetailPrice + ", PRODUCT_BULK_PRICE = " + productBulkPrice + ", PRODUCT_WHOLESALE_PRICE = " + productWholesalePrice + " WHERE PRODUCT_ID = '" + productID + "'";

                            if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                                throw internalEX;

                            //sqlCommand = "DELETE FROM TEMP_MASTER_PRODUCT WHERE PRODUCT_ID = '" + productID + "'";

                            //if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                            //    throw internalEX; 

                            i++;
                        }
                        dataGridView1.DataSource = null;
                        // INSERT NEW DATA
                        sqlCommand = "SELECT * FROM TEMP_MASTER_PRODUCT WHERE PRODUCT_ID NOT IN (SELECT PRODUCT_ID FROM MASTER_PRODUCT)";

                        using (rdr = DS.getData(sqlCommand))
                        {
                            if (rdr.HasRows)
                            {
                                dt2.Load(rdr);
                                rdr.Close();

                                dataGridView1.DataSource = dt2;
                                i = 0;
                                while (i < dataGridView1.Rows.Count)
                                {
                                    productID = dataGridView1.Rows[i].Cells["PRODUCT_ID"].Value.ToString();
                                    productBarcode = dataGridView1.Rows[i].Cells["PRODUCT_BARCODE"].Value.ToString();
                                    productName = dataGridView1.Rows[i].Cells["PRODUCT_NAME"].Value.ToString();
                                    productDescription = dataGridView1.Rows[i].Cells["PRODUCT_DESCRIPTION"].Value.ToString();
                                    productBasePrice = dataGridView1.Rows[i].Cells["PRODUCT_BASE_PRICE"].Value.ToString();
                                    productRetailPrice = dataGridView1.Rows[i].Cells["PRODUCT_RETAIL_PRICE"].Value.ToString();
                                    productBulkPrice = dataGridView1.Rows[i].Cells["PRODUCT_BULK_PRICE"].Value.ToString();
                                    productWholesalePrice = dataGridView1.Rows[i].Cells["PRODUCT_WHOLESALE_PRICE"].Value.ToString();
                                    productService = dataGridView1.Rows[i].Cells["PRODUCT_IS_SERVICE"].Value.ToString();
                                    sqlCommand = "INSERT INTO MASTER_PRODUCT (PRODUCT_ID, PRODUCT_BARCODE, PRODUCT_NAME, PRODUCT_DESCRIPTION, PRODUCT_BASE_PRICE, PRODUCT_RETAIL_PRICE, PRODUCT_BULK_PRICE, PRODUCT_WHOLESALE_PRICE, PRODUCT_STOCK_QTY, PRODUCT_LIMIT_STOCK, PRODUCT_SHELVES, PRODUCT_ACTIVE, PRODUCT_IS_SERVICE) VALUES (" +
                                                        "'" + productID + "', '" + productBarcode + "', '" + productName + "', '" + productDescription + "', " + productBasePrice + ", " + productRetailPrice + ", " + productBulkPrice + ", " + productWholesalePrice + ", 0, 0, '--00', 1, " + productService + ")";

                                    if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                                        throw internalEX;

                                    i++;
                                }
                            }
                        }

                        DS.commit();
                    }
                }

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

                gutil.showDBOPError(e, "ROLLBACK");
                result = false;
            }
            finally
            {
                DS.mySqlClose();
            }

            return result;
        }

        private void importFromFileButton_Click(object sender, EventArgs e)
        {
            if (fileNameTextbox.Text != "")
            {
                //this.Cursor = Cursors.WaitCursor;
                
                //restore database from file
                syncInformation(fileNameTextbox.Text);
                
            }
            else
            {
                String errormessage = "Filename is blank." + Environment.NewLine + "Please find the appropriate file!";
                gutil.showError(errormessage);
            }
        }
    }
}
