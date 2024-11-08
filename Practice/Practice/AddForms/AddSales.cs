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
    public partial class AddSalesForm : Form
    {
        private Database database = new Database();

        public int SelectedClientId { get; set; }
        public DateTime SellDate { get; set; }
        public List<SellDetail> SellDetails { get; set; } = new List<SellDetail>();

        public AddSalesForm()
        {
            InitializeComponent();
            LoadProducts();
            InitializeForm();
        }

        private void InitializeForm()
        {
            sellDatePicker.Value = DateTime.Now;
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

                // Привязываем данные к ComboBox
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

        private void addDetailButton_Click(object sender, EventArgs e)
        {
            // Получаем данные из элементов управления
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

            // Проверяем, есть ли уже этот товар в списке деталей
            var existingDetail = SellDetails.FirstOrDefault(detail => detail.ProductId == productId);
            if (existingDetail != null)
            {
                existingDetail.Quantity = quantity;
                MessageBox.Show("Количество товара обновлено!");
            }
            else
            {
                // Создаем новый объект SellDetail и добавляем его в список
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

            AddSale(SelectedClientId, SellDate, SellDetails);
            MessageBox.Show("Запись успешно сохранена!");
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AddSale(int clientId, DateTime sellDate, List<SellDetail> sellDetails)
        {
            string insertSellQuery = @"
        INSERT INTO SELLS (CLIENT, SELL_DATE) 
        VALUES (@ClientId, @SellDate);
        SELECT SCOPE_IDENTITY();";

            database.openConnection();
            SqlCommand command = new SqlCommand(insertSellQuery, database.getConnection());
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@SellDate", sellDate);

            try
            {
                int newSellId = Convert.ToInt32(command.ExecuteScalar());
                foreach (var detail in sellDetails)
                {
                    string insertDetailQuery = @"
                INSERT INTO SELL_DETAILS (SELL, PRODUCT, QUANTITY) 
                VALUES (@SellId, @ProductId, @Quantity)";

                    SqlCommand detailCommand = new SqlCommand(insertDetailQuery, database.getConnection());
                    detailCommand.Parameters.AddWithValue("@SellId", newSellId);
                    detailCommand.Parameters.AddWithValue("@ProductId", detail.ProductId);
                    detailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    detailCommand.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении продажи: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }
        }
    }
    public class SellDetail
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
