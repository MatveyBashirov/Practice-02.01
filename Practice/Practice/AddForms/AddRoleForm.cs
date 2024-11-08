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

namespace Practice.AddForms
{
    public partial class AddRoleForm : Form
    {
        private Database database = new Database();
        public string RoleName { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public AddRoleForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            RoleName = roleNameTextBox.Text;
            Login = loginTextBox.Text;
            Password = passwordTextBox.Text;

            if (string.IsNullOrWhiteSpace(RoleName) || string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            try
            {
                string insertQuery = "INSERT INTO ROLES (NAME, LOGIN, PASSWORD) VALUES (@Name, @Login, @Password)";
                database.openConnection();
                SqlCommand command = new SqlCommand(insertQuery, database.getConnection());
                command.Parameters.AddWithValue("@Name", RoleName);
                command.Parameters.AddWithValue("@Login", Login);
                command.Parameters.AddWithValue("@Password", Password);
                command.ExecuteNonQuery();
                database.closeConnection();

                DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка при добавлении роли: {ex.Message}");
                database.closeConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
                database.closeConnection();
            }
        }
    }
}
