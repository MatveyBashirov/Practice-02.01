using Practice.AddForms;
using Practice.UpdateForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Practice
{
    public partial class ShopBase : Form
    {
        public BindingSource productsBindingSource = new BindingSource();
        public BindingSource clientsBindingSource = new BindingSource();
        public BindingSource salesBindingSource = new BindingSource();
        public BindingSource categoriesBindingSource = new BindingSource();
        public BindingSource rolesBindingSource = new BindingSource();
        private Database database = new Database();

        public ShopBase(string userName)
        {
            InitializeComponent();
            access = userName;
            Products_Load();
            Clients_Load();
            LoadSalesData();
            Categoty_Load();
            Roles_Load();
            this.Load += ShopBase_Load;
        }

        public ShopBase()
        {
            InitializeComponent();
            Products_Load();
            Clients_Load();
            LoadSalesData();
            Categoty_Load();
            Roles_Load();
        }

        string access;
        private void ShopBase_Load(object sender, EventArgs e)
        {
            SetTabPageAccess(access);
        }

        public void SetTabPageAccess(string userName)
        {
            tabControl1.TabPages.Clear();
            if (userName == "Admin")
            {
                // Admin имеет доступ ко всем вкладкам
                tabControl1.TabPages.Add(tabPage1);
                tabControl1.TabPages.Add(tabPage2);
                tabControl1.TabPages.Add(tabPage3);
                tabControl1.TabPages.Add(tabPage4);
                tabControl1.TabPages.Add(tabPage5);
                tabControl1.TabPages.Add(tabPage6);
            }
            else if (userName == "Manager")
            {
                tabControl1.TabPages.Add(tabPage3);
                tabControl1.TabPages.Add(tabPage6);
                addSaleButton.Enabled = changeSaleButton.Enabled = deleteSaleButton.Enabled = false;
            }
            else if (userName == "Employee")
            {
                tabControl1.TabPages.Add(tabPage1);
                add_product.Enabled = change_product.Enabled = delete_product.Enabled = false;
                tabControl1.TabPages.Add(tabPage3);
            }
        }

        private DataTable ExecuteQuery(string query)
        {
            database.openConnection();
            SqlDataAdapter adapter = new SqlDataAdapter(query, database.getConnection());
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            database.closeConnection();
            return dataTable;
        }

        public void Products_Load()
        {
            database.openConnection();
            string query = @"
                SELECT
                    P.ID,
                    P.NAME,
                    P.PRICE,
                    P.QUANTITY,
                    C.NAME AS CATEGORY
                FROM 
                    PRODUCTS P
                JOIN 
                    CATEGORIES C ON P.CATEGORY = C.ID";
            DataTable dataTable = ExecuteQuery(query);
            productsBindingSource.DataSource = dataTable;
            productsDataGridView.DataSource = productsBindingSource;
        }

        public void Clients_Load()
        {
            string query = @"
            SELECT
                ID,
                [NAME],
                PHONE,
                IS_REGULAR
            FROM 
                CLIENTS";
            DataTable dataTable = ExecuteQuery(query);
            clientsBindingSource.DataSource = dataTable;
            clientsDataGridView.DataSource = clientsBindingSource;
        }

        private void add_product_Click(object sender, EventArgs e)
        {

            try
            {
                using (AppProductForm addProductForm = new AppProductForm())
                {
                    if (addProductForm.ShowDialog() == DialogResult.OK)
                    {
                        AddProduct(addProductForm.NewProductName,
                            Convert.ToDouble(addProductForm.Price),
                            Convert.ToInt32(addProductForm.Quantity),
                            Convert.ToInt32(addProductForm.Category));
                        Products_Load();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка при добавлении продукта: " + ex.Message); }
        }

        private void AddProduct(string NewProductName, double Price, int Quantity, int Category)
        {
            string query1 = @"
        INSERT INTO PRODUCTS ([NAME], PRICE, QUANTITY, CATEGORY)
        VALUES (@Name, @Price, @Quantity, @Category)";
            database.openConnection();
            using (SqlCommand command = new SqlCommand(query1, database.getConnection()))
            {
                // Добавление параметров
                command.Parameters.AddWithValue("@Name", NewProductName);
                command.Parameters.AddWithValue("@Price", Price);
                command.Parameters.AddWithValue("@Quantity", Quantity);
                command.Parameters.AddWithValue("@Category", Category);

                try
                {
                    // Выполнение команды
                    command.ExecuteNonQuery();
                    MessageBox.Show("Продукт успешно добавлен!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении продукта: " + ex.Message);
                }
            }
            database.closeConnection();
        }



        private void delete_product_Click(object sender, EventArgs e)
        {
            if (productsBindingSource.Current is DataRowView currentRow)
            {
                var name = currentRow["NAME"];
                DeleteProduct(name);
                Products_Load();
            }
        }

        private void DeleteProduct(object id)
        {
            string query = "DELETE FROM PRODUCTS WHERE [NAME] = @Name";
            database.openConnection();
            using (SqlCommand command = new SqlCommand(query, database.getConnection()))
            {
                try
                {
                    command.Parameters.AddWithValue("@Name", id);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления данных: " + ex.Message); }
            }
            database.closeConnection();
        }

        private void UpdateProduct(object id, string NewProductName, double Price, int Quantity, int Category)
        {
            string query = "UPDATE PRODUCTS SET [NAME] = @Name, PRICE = @Price, QUANTITY = @Quantity, CATEGORY = @Category WHERE ID = @Id";
            database.openConnection();
            using (SqlCommand command = new SqlCommand(query, database.getConnection()))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", NewProductName);
                command.Parameters.AddWithValue("@Price", Price);
                command.Parameters.AddWithValue("@Quantity", Quantity);
                command.Parameters.AddWithValue("@Category", Category);
                try
                {
                    // Выполнение команды
                    command.ExecuteNonQuery();
                    MessageBox.Show("Данные успешно изменены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении данных: " + ex.Message);
                }
            }
            database.closeConnection();

        }

        private void change_product_Click(object sender, EventArgs e)
        {
            if (productsBindingSource.Current is DataRowView currentRow)
            {
                var id = currentRow["ID"];
                using (var updateProductForm = new UpdateProducts(id))
                {
                    if (updateProductForm.ShowDialog() == DialogResult.OK)
                    {
                        UpdateProduct(id, updateProductForm.Name, updateProductForm.Price, updateProductForm.Quantity, updateProductForm.Category);
                        Products_Load();
                    }
                }
            }
        }

        private void add_client_button_Click(object sender, EventArgs e)
        {
            try
            {
                using (AddClientForm addClientForm = new AddClientForm())
                {
                    if (addClientForm.ShowDialog() == DialogResult.OK)
                    {
                        AddClient(addClientForm.Name,
                            addClientForm.Contact,
                            addClientForm.IsRegular);
                        Clients_Load();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка при добавлении клиента: " + ex.Message); }
        }

        private void AddClient(string Name, string Contact, bool IsRegular)
        {
            string query1 = @"
        INSERT INTO CLIENTS ([NAME], PHONE, IS_REGULAR)
        VALUES (@Name, @Phone, @IsRegular)";
            database.openConnection();
            using (SqlCommand command = new SqlCommand(query1, database.getConnection()))
            {
                // Добавление параметров
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Phone", Contact);
                command.Parameters.AddWithValue("@IsRegular", IsRegular);

                try
                {
                    // Выполнение команды
                    command.ExecuteNonQuery();
                    MessageBox.Show("Клиент успешно добавлен!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении клиента: " + ex.Message);
                }
            }
            database.closeConnection();
        }

        private void delete_client_Click(object sender, EventArgs e)
        {
            if (clientsBindingSource.Current is DataRowView currentRow)
            {
                var name = currentRow["ID"];
                DeleteClient(name);
                Clients_Load();
            }
        }

        private void DeleteClient(object id)
        {
            string query = "DELETE FROM CLIENTS WHERE ID = @Id";
            database.openConnection();
            using (SqlCommand command = new SqlCommand(query, database.getConnection()))
            {
                try
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления данных: " + ex.Message); }
            }
            database.closeConnection();
        }

        private void UpdateClients(object id, string newName, string phone, bool isRegular)
        {
            string query = "UPDATE CLIENTS SET [NAME] = @Name, PHONE = @Phone, IS_REGULAR = @IsRegular WHERE ID = @Id";
            database.openConnection();
            using (SqlCommand command = new SqlCommand(query, database.getConnection()))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", newName);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@IsRegular", isRegular);
                try
                {
                    // Выполнение команды
                    command.ExecuteNonQuery();
                    MessageBox.Show("Данные клиента успешно изменены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении данных: " + ex.Message);
                }
            }
            database.closeConnection();
        }

        private void change_client_Click(object sender, EventArgs e)
        {
            if (clientsBindingSource.Current is DataRowView currentRow)
            {
                var id = currentRow["ID"];
                using (var updateClientForm = new UpdateClientsForm(id))
                {
                    if (updateClientForm.ShowDialog() == DialogResult.OK)
                    {
                        UpdateClients(id, updateClientForm.Name, updateClientForm.Phone, updateClientForm.IsRegular);
                        Clients_Load(); // Обновление списка клиентов
                    }
                }
            }
        }

        public void LoadSalesData()
        {
            string query = @"
    SELECT 
        S.ID AS SellID,
        S.SELL_DATE,
        C.NAME AS ClientName
    FROM 
        SELLS S
    JOIN 
        CLIENTS C ON S.CLIENT = C.ID
    ORDER BY 
        S.ID;";

            database.openConnection();
            SqlDataAdapter adapter = new SqlDataAdapter(query, database.getConnection());
            DataTable salesData = new DataTable();
            adapter.Fill(salesData);
            salesBindingSource.DataSource = salesData;
            sellsDataGridView.DataSource = salesBindingSource;
            sellsDataGridView.AutoGenerateColumns = true;
            sellsDataGridView.RowTemplate.Height = 30;

            database.closeConnection();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int sellId = Convert.ToInt32(sellsDataGridView.Rows[e.RowIndex].Cells["SellID"].Value);
                LoadSellDetails(sellId);
            }
        }

        private void LoadSellDetails(object sellId)
        {
            string query = @"
    SELECT 
        P.NAME AS ProductName, 
        D.QUANTITY 
    FROM 
        SELL_DETAILS D
    JOIN 
        PRODUCTS P ON D.PRODUCT = P.ID 
    WHERE 
        D.SELL = @SellId";
            SqlCommand command = new SqlCommand(query, database.getConnection());
            command.Parameters.AddWithValue("@SellId", sellId);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable detailsData = new DataTable();
            adapter.Fill(detailsData);
            detailsDataGridView.DataSource = detailsData;
        }
        public void addSaleButton_Click(object sender, EventArgs e)
        {
            using (var addSaleForm = new AddSalesForm())
            {
                if (addSaleForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSalesData();
                }
            }
        }

        private void changeSaleButton_Click(object sender, EventArgs e)
        {
            if (salesBindingSource.Current is DataRowView currentRow)
            {
                var id = currentRow["SellID"];
                using (var updateSaleForm = new UpdateSalesForm(id))
                {
                    if (updateSaleForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadSalesData();
                        LoadSellDetails(id);
                    }
                }
            }

        }

        private void DeleteSale(object sellId)
        {
            string deleteDetailsQuery = "DELETE FROM SELL_DETAILS WHERE SELL = @SellId";
            database.openConnection();
            using (SqlCommand deleteDetailsCommand = new SqlCommand(deleteDetailsQuery, database.getConnection()))
            {
                deleteDetailsCommand.Parameters.AddWithValue("@SellId", sellId);
                deleteDetailsCommand.ExecuteNonQuery();
            }

            string deleteSaleQuery = "DELETE FROM SELLS WHERE ID = @SellId";
            using (SqlCommand deleteSaleCommand = new SqlCommand(deleteSaleQuery, database.getConnection()))
            {
                deleteSaleCommand.Parameters.AddWithValue("@SellId", sellId);
                deleteSaleCommand.ExecuteNonQuery();
            }

            database.closeConnection();
            MessageBox.Show("Запись о продаже успешно удалена.");
        }

        private void deleteSaleButton_Click(object sender, EventArgs e)
        {
            if (salesBindingSource.Current is DataRowView currentRow)
            {
                var id = currentRow["SellID"];

                // Подтверждение удаления
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись о продаже?", "Подтверждение удаления", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    DeleteSale(id);
                    LoadSalesData();
                    detailsDataGridView.DataSource = null;
                    detailsDataGridView.Rows.Clear();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись о продаже для удаления.");
            }
        }

        private void Categoty_Load()
        {
            database.openConnection();
            string query = "SELECT ID, NAME FROM CATEGORIES";
            DataTable dataTable = ExecuteQuery(query);
            categoriesBindingSource.DataSource = dataTable;
            categoriesDataGridView.DataSource = categoriesBindingSource;
            database.closeConnection();
        }

        private void addCategoryButton_Click(object sender, EventArgs e)
        {
            string categoryName = categoryNameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                string insertQuery = "INSERT INTO CATEGORIES (NAME) VALUES (@Name)";
                database.openConnection();
                SqlCommand command = new SqlCommand(insertQuery, database.getConnection());
                command.Parameters.AddWithValue("@Name", categoryName);
                command.ExecuteNonQuery();
                database.closeConnection();

                Categoty_Load();
                MessageBox.Show("Категория успешно добавлена.");
            }
            else
            {
                MessageBox.Show("Введите имя категории.");
            }
        }

        private void updateCategoryButton_Click(object sender, EventArgs e)
        {
            if (categoriesBindingSource.Current is DataRowView currentRow)
            {
                int categoryId = (int)currentRow["ID"];
                string categoryName = categoryNameTextBox.Text;

                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    string updateQuery = "UPDATE CATEGORIES SET NAME = @Name WHERE ID = @Id";
                    database.openConnection();
                    SqlCommand command = new SqlCommand(updateQuery, database.getConnection());
                    command.Parameters.AddWithValue("@Name", categoryName);
                    command.Parameters.AddWithValue("@Id", categoryId);
                    command.ExecuteNonQuery();
                    database.closeConnection();

                    Categoty_Load();
                    MessageBox.Show("Категория успешно обновлена.");
                }
                else
                {
                    MessageBox.Show("Введите имя категории.");
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для обновления.");
            }
        }
        private void deleteCategoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (categoriesBindingSource.Current is DataRowView currentRow)
                {
                    int categoryId = (int)currentRow["ID"];

                    string deleteQuery = "DELETE FROM CATEGORIES WHERE ID = @Id";
                    database.openConnection();
                    using (SqlCommand command = new SqlCommand(deleteQuery, database.getConnection()))
                    {
                        command.Parameters.AddWithValue("@Id", categoryId);
                        command.ExecuteNonQuery();
                    }
                    database.closeConnection();

                    Categoty_Load();
                    MessageBox.Show("Категория успешно удалена.");
                }
                else
                {
                    MessageBox.Show("Выберите категорию для удаления.");
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Ошибка при удалении категории: {sqlEx.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (database.getConnection().State == ConnectionState.Open)
                {
                    database.closeConnection();
                }
            }
        }

        private void Roles_Load()
        {
            string query = "SELECT ID, NAME, LOGIN, PASSWORD FROM ROLES";
            DataTable rolesData = new DataTable();

            database.openConnection();
            SqlCommand command = new SqlCommand(query, database.getConnection());
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(rolesData);
            database.closeConnection();

            rolesBindingSource.DataSource = rolesData;
            rolesDataGridView.DataSource = rolesBindingSource;
        }

        private void addRoleButton_Click(object sender, EventArgs e)
        {
            using (var addRoleForm = new AddRoleForm())
            {
                if (addRoleForm.ShowDialog() == DialogResult.OK)
                {
                    Roles_Load();
                    MessageBox.Show("Новый пользователь успешно добавлен!");
                }
            }
        }

        private void updateRoleButton_Click(object sender, EventArgs e)
        {
            if (rolesBindingSource.Current is DataRowView currentRow)
            {
                int roleId = (int)currentRow["ID"];
                string roleName = currentRow["NAME"].ToString();
                string login = currentRow["LOGIN"].ToString();
                string password = currentRow["PASSWORD"].ToString();

                using (var updateRoleForm = new UpdateRoleForm(roleId, roleName, login, password))
                {
                    if (updateRoleForm.ShowDialog() == DialogResult.OK)
                    {
                        Roles_Load();
                        MessageBox.Show("Данные пользователя обновлены!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите роль для обновления.");
            }
        }

        private void deleteRoleButton_Click(object sender, EventArgs e)
        {
            if (rolesBindingSource.Current is DataRowView currentRow)
            {
                int roleId = (int)currentRow["ID"];

                string deleteQuery = "DELETE FROM ROLES WHERE ID = @Id";
                database.openConnection();
                SqlCommand command = new SqlCommand(deleteQuery, database.getConnection());
                command.Parameters.AddWithValue("@Id", roleId);
                command.ExecuteNonQuery();
                database.closeConnection();

                Roles_Load();
                MessageBox.Show("Роль успешно удалена.");
            }
            else
            {
                MessageBox.Show("Выберите роль для удаления.");
            }
        }

        private void SearchData(string searchTerm)
        {
            TabPage activeTab = tabControl1.SelectedTab;
            if (activeTab != null)
            {
                DataGridView dataGridView = activeTab.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dataGridView != null)
                {
                    BindingSource bindingSource = (BindingSource)dataGridView.DataSource;
                    if (bindingSource != null)
                    {
                        // Создаем фильтр для всех столбцов
                        List<string> filterConditions = new List<string>(); // Используем список для условий фильтрации

                        foreach (DataGridViewColumn column in dataGridView.Columns)
                        {
                            if (column.Visible)
                            {
                                string columnType = column.ValueType.Name;

                                if (columnType == "String")
                                {
                                    filterConditions.Add($"{column.Name} LIKE '%{searchTerm}%'");
                                }
                                else if (columnType == "Int32" || columnType == "Double" || columnType == "Decimal")
                                {
                                    if (decimal.TryParse(searchTerm, out decimal numericValue))
                                    {
                                        filterConditions.Add($"{column.Name} = {numericValue}");
                                    }
                                }
                                else if (columnType == "DateTime")
                                {
                                    if (DateTime.TryParse(searchTerm, out DateTime dateValue))
                                    {
                                        filterConditions.Add($"{column.Name} = #{dateValue.ToString("MM/dd/yyyy")}#");
                                    }
                                }
                            }
                        }

                        if (filterConditions.Count > 0)
                        {
                            string filter = string.Join(" OR ", filterConditions);
                            bindingSource.Filter = filter;
                        }
                        else
                        {
                            bindingSource.RemoveFilter();
                        }
                    }
                }
            }
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox searchTextBox = sender as TextBox;

            // Вызываем метод поиска для соответствующего DataGridView
            if (searchTextBox == tabPage1SearchTextBox)
            {
                SearchData(searchTextBox.Text);
            }
            else if (searchTextBox == tabPage2SearchTextBox)
            {
                SearchData(searchTextBox.Text);
            }
            else if (searchTextBox == tabPage3SearchTextBox)
            {
                SearchData(searchTextBox.Text);
            }
            else if (searchTextBox == tabPage4SearchTextBox)
            {
                SearchData(searchTextBox.Text);
            }
            else if (searchTextBox == tabPage5SearchTextBox)
            {
                SearchData(searchTextBox.Text);
            }
        }

        private void GenerateSalesReport(DateTime startDate, DateTime endDate)
        {
            database.openConnection();
            string query = @"
            SELECT 
                P.NAME AS ProductName,
                SUM(SD.QUANTITY) AS TotalQuantity,
                SUM(SD.QUANTITY * P.PRICE) AS TotalSales
            FROM 
                SELL_DETAILS SD
            JOIN 
                SELLS S ON SD.SELL = S.ID
            JOIN 
                PRODUCTS P ON SD.PRODUCT = P.ID
            WHERE 
                S.SELL_DATE BETWEEN @StartDate AND @EndDate
            GROUP BY 
                P.NAME";
            using (SqlCommand command = new SqlCommand(query, database.getConnection()))
            {
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Создаем DataTable для хранения результатов
                    DataTable salesData = new DataTable();
                    salesData.Load(reader);

                    // Отображаем данные в DataGridView
                    dataGridView1.DataSource = salesData;

                    // Подготовка данных для диаграммы
                    chart1.Series.Clear();
                    chart1.Series.Add("Sales");
                    chart1.Series["Sales"].ChartType = SeriesChartType.Pie;

                    foreach (DataRow row in salesData.Rows)
                    {
                        string productName = row["ProductName"].ToString();
                        int totalQuantity = Convert.ToInt32(row["TotalQuantity"]);
                        chart1.Series["Sales"].Points.AddXY(productName, totalQuantity);
                    }
                }
            }
            database.closeConnection();
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePickerStart.Value;
            DateTime endDate = dateTimePickerEnd.Value;
            GenerateSalesReport(startDate, endDate);
        }

        public int GetTabCount()
        {
            return tabControl1.TabPages.Count;
        }

        public TabPage GetTabPageByName(string tabPageName)
        {
            return tabControl1.TabPages.Cast<TabPage>().FirstOrDefault(tp => tp.Name == tabPageName);
        }
    }
}
