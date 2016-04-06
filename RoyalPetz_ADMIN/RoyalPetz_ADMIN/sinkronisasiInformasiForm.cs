using System;
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
       
        private void exportDataMasterProduk()
        {
            string localDate = "";
            string fileName = "";
            //string ipServer;
            MySqlDataReader rdr;
            string sqlCommand = "";
            string insertStatement;
            StreamWriter sw = null;

            string strCmdText = "USE `sys_pos`; "+ "\n" +
                                        "DROP TABLE IF EXISTS `temp_master_product`;" + "\n" +
                                        "CREATE TABLE `temp_master_product` (" + "\n" +
                                        "`ID` int(10) unsigned NOT NULL AUTO_INCREMENT," + "\n" +
                                        "`PRODUCT_ID` varchar(50) DEFAULT NULL," + "\n" +
                                        "`PRODUCT_BASE_PRICE` double DEFAULT NULL," + "\n" +
                                        "`PRODUCT_RETAIL_PRICE` double DEFAULT NULL," + "\n" +
                                        "`PRODUCT_BULK_PRICE` double DEFAULT NULL," + "\n" +
                                        "`PRODUCT_WHOLESALE_PRICE` double DEFAULT NULL," + "\n" +
                                        "PRIMARY KEY(`ID`)," + "\n" +
                                        "UNIQUE KEY `PRODUCT_ID_UNIQUE` (`PRODUCT_ID`)" + "\n" +
                                        ") ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;" + "\n";
            
            localDate = String.Format(culture, "{0:ddMMyyyy}", DateTime.Now);
            fileName = "SYNCINFO_PRODUCT_" + localDate + ".sql";

            sqlCommand = "SELECT PRODUCT_ID, PRODUCT_BASE_PRICE, PRODUCT_RETAIL_PRICE, PRODUCT_BULK_PRICE, PRODUCT_WHOLESALE_PRICE FROM MASTER_PRODUCT WHERE PRODUCT_ACTIVE = 1";
            using (rdr = DS.getData(sqlCommand))
            {
                if (rdr.HasRows)
                {
                    if (!File.Exists(fileName))
                        sw = File.CreateText(Application.StartupPath + "\\" + fileName);
                    else
                    {
                        File.Delete(fileName);
                        sw = File.CreateText(Application.StartupPath + "\\" + fileName);
                    }

                    sw.WriteLine(strCmdText);

                    while (rdr.Read())
                    {
                        insertStatement = "INSERT INTO TEMP_MASTER_PRODUCT (PRODUCT_ID, PRODUCT_BASE_PRICE, PRODUCT_RETAIL_PRICE, PRODUCT_BULK_PRICE, PRODUCT_WHOLESALE_PRICE) VALUES (" +
                                                 "'" + rdr.GetString("PRODUCT_ID") + "', " + rdr.GetString("PRODUCT_BASE_PRICE") + ", " + rdr.GetString("PRODUCT_RETAIL_PRICE") + ", " + rdr.GetString("PRODUCT_BULK_PRICE") + ", " + rdr.GetString("PRODUCT_WHOLESALE_PRICE") + ");";
                        sw.WriteLine(insertStatement);
                    }
                }
            }

            sw.Close();
            //ipServer = DS.getIPServer();
            //strCmdText = "/C mysqldump -h " + ipServer + " -u SYS_POS_ADMIN -ppass123 sys_pos MASTER_PRODUCT > " + fileName;
            //System.Diagnostics.Process.Start("CMD.exe", strCmdText);
        }

        private void exportDataButton_Click(object sender, EventArgs e)
        {
            //saveFileDialog1.ShowDialog();
            //MessageBox.Show(saveFileDialog1.FileName);
        //    this.Cursor = Cursors.WaitCursor;
            exportDataMasterProduk();
          //  this.Cursor = Cursors.Default;
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
            //Console.WriteLine(proc.Id.ToString());
            if (updateLocalData())
            {
                MessageBox.Show("DONE");
            }
            
        }

        private void syncInformation(string fileName)
        {
            string ipServer = "";
            string strCmdText = "";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            Directory.SetCurrentDirectory(Application.StartupPath);

            ipServer = DS.getIPServer();
            strCmdText = "/C " + "mysql -h " + ipServer + " -u SYS_POS_ADMIN -ppass123 sys_pos < \"" + fileName + "\"";

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
            string productBasePrice;
            string productRetailPrice;
            string productBulkPrice;
            string productWholesalePrice;
            MySqlException internalEX = null;
            MySqlDataReader rdr;
            DataTable dt = new DataTable();
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

                            i++;
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
