using Practice.AddForms;
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
    public partial class UpdateSalesForm : Form
    {
        private Database database = new Database();
        private int sellId;

        public int SelectedClientId { get; set; }
        public DateTime SellDate { get; set; }
        public List<SellDetail> SellDetails { get; set; } = new List<SellDetail>();

        public UpdateSalesForm(object sellId)
        {
            InitializeComponent();
            this.sellId = Convert.ToInt32(sellId);
            sellDatePicker.Enabled = false;
            clientIdTextBox.Enabled = false;
            LoadProducts();
            LoadExistingSaleData();
        }

        private void LoadProducts()
        {
            string query = "SELECT ID, NAME FROM PRODUCTS";

            database.openConnection();
            SqlCommand command = new SqlCommand(query, database.getConnection());

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable productsTable = new DataTable();
                adapter.Fill(productsTable);

                productComboBox.DataSource = productsTable;
                productComboBox.DisplayMember = "NAME";
                productComboBox.ValueMember = "ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке товаров: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }
        }

        private void LoadExistingSaleData()
        {
            string saleQuery = "SELECT CLIENT, SELL_DATE FROM SELLS WHERE ID = @SellId";
            database.openConnection();
            SqlCommand saleCommand = new SqlCommand(saleQuery, database.getConnection());
            saleCommand.Parameters.AddWithValue("@SellId", sellId);

            try
            {
                using (SqlDataReader reader = saleCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        SelectedClientId = reader.GetInt32(0);
                        SellDate = reader.GetDateTime(1);
                        clientIdTextBox.Text = SelectedClientId.ToString();
                        sellDatePicker.Value = SellDate;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных продажи: " + ex.Message);
            }

            string detailsQuery = "SELECT PRODUCT, QUANTITY FROM SELL_DETAILS WHERE SELL = @SellId";
            SqlCommand detailsCommand = new SqlCommand(detailsQuery, database.getConnection());
            detailsCommand.Parameters.AddWithValue("@SellId", sellId);

            try
            {
                using (SqlDataReader reader = detailsCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SellDetails.Add(new SellDetail
                        {
                            ProductId = reader.GetInt32(0),
                            Quantity = reader.GetInt32(1)
                        });
                    }
                }
                UpdateDetailsListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке деталей продажи: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }
        }

        private void addDetailButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(clientIdTextBox.Text, out int clientId))
            {
                SelectedClientId = clientId;
            }
            else
            {
                MessageBox.Show("Введите корректный ID клиента.");
                return;
            }

            SellDate = sellDatePicker.Value;

            int productId = (int)productComboBox.SelectedValue;
            int quantity = (int)quantityNumericUpDown.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше нуля.");
                return;
            }

            var existingDetail = SellDetails.FirstOrDefault(detail => detail.ProductId == productId);
            if (existingDetail != null)
            {
                existingDetail.Quantity = quantity;
                MessageBox.Show("Количество товара обновлено!");
            }
            else
            {
                SellDetail detail = new SellDetail
                {
                    ProductId = productId,
                    Quantity = quantity
                };
                SellDetails.Add(detail);
            }
            UpdateDetailsListBox();
        }

        private void UpdateDetailsListBox()
        {
            detailsListBox.Items.Clear();

            foreach (var detail in SellDetails)
            {
                string productName = GetProductNameById(detail.ProductId);
                string displayText = $"{productName} в количестве {detail.Quantity}";
                detailsListBox.Items.Add(displayText);
            }
        }

        private string GetProductNameById(int productId)
        {
            string productName = string.Empty;
            string query = "SELECT NAME FROM PRODUCTS WHERE ID = @ProductId";

            database.openConnection();
            SqlCommand command = new SqlCommand(query, database.getConnection());
            command.Parameters.AddWithValue("@ProductId", productId);

            try
            {
                productName = command.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении названия товара: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }

            return productName;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (SellDetails.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар перед сохранением.");
                return;
            }

            UpdateSale(SelectedClientId, SellDate, SellDetails);
            MessageBox.Show("Запись успешно обновлена!");
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void UpdateSale(int clientId, DateTime sellDate, List<SellDetail> sellDetails)
        {
            string updateSellQuery = @"
        UPDATE SELLS SET CLIENT = @ClientId, SELL_DATE = @SellDate 
        WHERE ID = @SellId";

            database.openConnection();
            SqlCommand command = new SqlCommand(updateSellQuery, database.getConnection());
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@SellDate", sellDate);
            command.Parameters.AddWithValue("@SellId", sellId);

            try
            {
                command.ExecuteNonQuery();

                // Удаляем существующие детали
                string deleteDetailsQuery = "DELETE FROM SELL_DETAILS WHERE SELL = @SellId";
                SqlCommand deleteCommand = new SqlCommand(deleteDetailsQuery, database.getConnection());
                deleteCommand.Parameters.AddWithValue("@SellId", sellId);
                deleteCommand.ExecuteNonQuery();

                // Вставляем обновленные детали
                foreach (var detail in sellDetails)
                {
                    string insertDetailQuery = @"
                INSERT INTO SELL_DETAILS (SELL, PRODUCT, QUANTITY) 
                VALUES (@SellId, @ProductId, @Quantity)";

                    SqlCommand detailCommand = new SqlCommand(insertDetailQuery, database.getConnection());
                    detailCommand.Parameters.AddWithValue("@SellId", sellId);
                    detailCommand.Parameters.AddWithValue("@ProductId", detail.ProductId);
                    detailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);

                    detailCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении продажи: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }
        }
    }
}
