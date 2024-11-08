using Practice.Classes;
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
    public partial class UpdateClientsForm : Form
    {

        public UpdateClientsForm(object clientID)
        {
            InitializeComponent();
            this.id = Convert.ToInt32(clientID);
            LoadClients();
        }

        private int id;
        private Database database = new Database();


        public string Name { get; private set; }
        public string Phone { get; private set; }
        public bool IsRegular { get; private set; }

        private void LoadClients()
        {
            string query = "SELECT [NAME], PHONE, IS_REGULAR FROM CLIENTS WHERE ID = @Id";

            using (SqlConnection connection = database.getConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Заполнение полей формы данными клиента
                            nameTextBox.Text = reader["NAME"].ToString();
                            contactTextBox.Text = reader["PHONE"].ToString();
                            checkBox1.Checked = Convert.ToBoolean(reader["IS_REGULAR"]);
                        }
                    }
                }
            }
        }

        public void changeProduct_Click(object sender, EventArgs e)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(nameTextBox.Text)
                    || string.IsNullOrWhiteSpace(contactTextBox.Text))

                {
                    MessageBox.Show("Введите все данные для обновления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
                // Получение данных из формы
                Name = nameTextBox.Text;
                Phone = contactTextBox.Text;
                IsRegular = checkBox1.Checked;

                // Закрытие формы с результатом OK
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения данных:" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }
    }
}
