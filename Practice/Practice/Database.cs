using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practice
{
    public class Database
    {
        ////На ВЦ
        //SqlConnection SqlConnection = new SqlConnection(@"Data Source=ADCLG1;Initial Catalog=Bashirov;Integrated Security=True");

        //Собственная БД
        SqlConnection SqlConnection = new SqlConnection(@"Data Source=DESKTOP-HD1UESQ\SQLEXPRESS;Initial Catalog=Bashirov;Integrated Security=True");

        public void openConnection()
        {
            if (SqlConnection.State == System.Data.ConnectionState.Closed)
            {
                SqlConnection.Open();
            }
        }

        public void closeConnection()
        {
            if (SqlConnection.State == System.Data.ConnectionState.Open)
            {
                SqlConnection.Close();
            }
        }

        public SqlConnection getConnection()
        {
            return SqlConnection;
        }
    }
}
