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

namespace Practice.UpdateForms
{
    public partial class UpdateRoleForm : Form
    {
        private Database database = new Database();
        private int roleId;

        public string RoleName { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        public UpdateRoleForm(int roleId, string roleName, string login, string password)
        {
            InitializeComponent();
            this.roleId = roleId;
            roleNameTextBox.Text = roleName;
            loginTextBox.Text = login;
            passwordTextBox.Text = password;
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
                string updateQuery = "UPDATE ROLES SET NAME = @Name, LOGIN = @Login, PASSWORD = @Password WHERE ID = @Id";
                database.openConnection();
                SqlCommand command = new SqlCommand(updateQuery, database.getConnection());
                command.Parameters.AddWithValue("@Name", RoleName);
                command.Parameters.AddWithValue("@Login", Login);
                command.Parameters.AddWithValue("@Password", Password);
                command.Parameters.AddWithValue("@Id", roleId);
                command.ExecuteNonQuery();
                database.closeConnection();

                DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка при обновлении роли: {ex.Message}");
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
