using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Practice
{
    public partial class Login : Form
    {
        Database database = new Database();

        public Login()
        {
            InitializeComponent();
        }

        public static bool IsUser(string login, string password, SqlConnection connection)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            //Запрос на поиск записи по логину и паролю
            string GetUser = $"select [LOGIN], [PASSWORD] from ROLES where [LOGIN] = '{login}'";

            SqlCommand command = new SqlCommand(GetUser, connection);

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                if (table.Rows[0][1].ToString() == password) return true;
                else return false;
            }
            else return false;
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            database.openConnection();
            string UserLogin = textBox1.Text;
            string UserPassword = textBox2.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            //Запрос на поиск записи по логину и паролю
            string GetUser = $"SELECT [NAME], [LOGIN], [PASSWORD] FROM ROLES WHERE [LOGIN] = '{UserLogin}' AND [PASSWORD] = '{UserPassword}'";

            SqlCommand command = new SqlCommand(GetUser, database.getConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                string userName = table.Rows[0]["NAME"].ToString();  
                ShopBase form1 = new ShopBase(userName);
                this.Hide();
                form1.ShowDialog();
                this.Show();
            }
            else MessageBox.Show("Введены неверный логин или пароль!", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            database.closeConnection();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                button1.Enabled = false;
            }
            else button1.Enabled = true;
        }
    }
}
