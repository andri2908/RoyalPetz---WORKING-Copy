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
    public partial class IPPusatForm : Form
    {
        private globalUtilities gutil = new globalUtilities();
        private Data_Access DS = new Data_Access();

        public IPPusatForm()
        {
            InitializeComponent();
        }

        private void loadIPPusatInfo()
        {
            string ipPusat;

            ipPusat = DS.getDataSingleValue("SELECT IFNULL(IP_PUSAT, ' ') FROM SYS_CONFIG").ToString();

            ipAddressMaskedTextbox.Text = ipPusat;
        }

        private void IPPusatForm_Load(object sender, EventArgs e)
        {
            loadIPPusatInfo();

            gutil.reArrangeTabOrder(this);
        }

        private bool saveData()
        {
            string ipPusat;
            string sqlCommand;
            bool result = false;
            MySqlException internalEX = null;

            ipPusat = ipAddressMaskedTextbox.Text;

            DS.beginTransaction();

            try
            {
                DS.mySqlConnect();

                sqlCommand = "UPDATE SYS_CONFIG SET IP_PUSAT = '" + ipPusat + "'";

                if (!DS.executeNonQueryCommand(sqlCommand, ref internalEX))
                    throw internalEX;

                DS.commit();
                result = true;
            }
            catch (Exception ex)
            {
                try
                {
                    DS.rollBack();
                }
                catch (MySqlException exc)
                {
                    if (DS.getMyTransConnection() != null)
                    {
                        gutil.showDBOPError(exc, "ROLLBACK");
                    }
                }

                gutil.showDBOPError(ex, "ROLLBACK");
                result = false;
            }
            finally
            {
                DS.mySqlClose();
            }

            return result;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveData())
                gutil.showSuccess(gutil.UPD);
        }
    }
}
