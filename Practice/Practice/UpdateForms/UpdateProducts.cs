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
    public partial class UpdateProducts : Form
    {
        private int id;
        private Database database = new Database();


        public string Name { get; private set; }
        public double Price { get; private set; }
        public int Quantity { get; private set; }
        public int Category { get; private set; }
        public UpdateProducts(object productID)
        {
            InitializeComponent();
            this.id = Convert.ToInt32(productID);
            LoadProducts();
        }

        private void LoadProducts()
        {
            string query = "SELECT [NAME], PRICE, QUANTITY, CATEGORY FROM PRODUCTS WHERE ID = @Id";

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
                            productNameTextBox.Text = reader["NAME"].ToString();
                            priceTextBox.Text = reader["PRICE"].ToString();
                            quantityTextBox.Text = reader["QUANTITY"].ToString();
                            categoryTextBox.Text = reader["CATEGORY"].ToString();
                        }
                    }
                }
            }
        }

        private void changeProduct_Click(object sender, EventArgs e)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(productNameTextBox.Text)
                    || string.IsNullOrWhiteSpace(priceTextBox.Text)
                    || string.IsNullOrWhiteSpace(quantityTextBox.Text)
                    || string.IsNullOrWhiteSpace(quantityTextBox.Text))

                {
                    MessageBox.Show("Введите все данные для обновления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Получение данных из формы
                Name = productNameTextBox.Text;
                Price = Convert.ToDouble(priceTextBox.Text);
                Quantity = Convert.ToInt32(quantityTextBox.Text);
                Category = Convert.ToInt32(categoryTextBox.Text);

                // Закрытие формы с результатом OK
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) {
                MessageBox.Show("Ошибка изменения данных:" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }
    }
}
