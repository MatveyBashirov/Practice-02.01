using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Practice;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;
using Moq;
using Practice.AddForms;
using Practice.UpdateForms;
using System.Data.Common;

namespace UnitTestProject1
{
    [TestClass]
    public class SQlConnectionTest
    {
        [TestMethod]
        public void GetConnection_ReturnsSqlConnection()
        {
            var database = new Database();
            SqlConnection _connection = database.getConnection();
            Assert.IsNotNull(_connection, "Соединение не должно иметь пустого значения");
            Assert.IsInstanceOfType(_connection, typeof(SqlConnection), "Соедиенение должно иметь тип SqlConnection");
        }
    }

    [TestClass]
    public class FormTests
    {
        private ShopBase _form;
        [TestInitialize]
        public void Form_Init()
        {
            _form = new ShopBase("Admin");
        }

        [TestMethod]
        public void SetTabPageAccess_Admin_AddsAllTabs()
        {
            _form.SetTabPageAccess("Admin");
            Assert.AreEqual(6, _form.GetTabCount(), "Администратор должен иметь доступ ко всем вкладкам");
        }

        [TestMethod]
        public void SetTabPageAccess_Manager_AddsManagerTabs()
        {
            _form.SetTabPageAccess("Manager");
            Assert.IsNotNull(_form.GetTabPageByName("tabPage3"), "tabPage3 должна отобразиться для Менеджера");
            Assert.IsNotNull(_form.GetTabPageByName("tabPage6"), "tabPage6 должна отобразиться для Менеджера");
        }

        [TestMethod]
        public void SetTabPageAccess_Employee_AddsEmployeeTabs()
        {
            _form.SetTabPageAccess("Employee");
            Assert.IsNotNull(_form.GetTabPageByName("tabPage1"), "tabPage1 должна отобразиться для Продавца");
            Assert.IsNotNull(_form.GetTabPageByName("tabPage3"), "tabPage3 должна отобразиться для Продавца");
        }
    }

    [TestClass]
    public class TablesLoadingTests
    {
        private ShopBase database;

        [TestInitialize]
        public void Setup()
        {
            database = new ShopBase("Admin");
        }

        [TestMethod]
        public void Products_Loading() {
            database.Products_Load();
            var bindingSourceData = (DataTable)database.productsBindingSource.DataSource;
            Assert.AreEqual(7, bindingSourceData.Rows.Count, "Количество записей не соответствует количеству в исходной таблице БД");
            Assert.AreEqual("Гитара «Highway»", bindingSourceData.Rows[0]["NAME"], "Несоответствие названия первого продукта");
            Assert.AreEqual("Скрипка «Lonely»", bindingSourceData.Rows[1]["NAME"], "Несоответствие названия второго продукта");
        }

        [TestMethod]
        public void Clients_Loading()
        {
            database.Clients_Load();
            var bindingSourceData = (DataTable)database.clientsBindingSource.DataSource;
            Assert.AreEqual(5, bindingSourceData.Rows.Count, "Количество записей не соответствует количеству в исходной таблице БД");
            Assert.AreEqual("Иванов Иван Иванович", bindingSourceData.Rows[0]["NAME"], "Несоответствие названия первого продукта");
            Assert.AreEqual("Петров Петр Петрович", bindingSourceData.Rows[1]["NAME"], "Несоответствие названия второго продукта");
        }

        [TestMethod]

        public void SalesData_Loading()
        {
            database.LoadSalesData();
            var bindingSourceData = (DataTable)database.salesBindingSource.DataSource;
            Assert.AreEqual(7, bindingSourceData.Rows.Count, "Количество записей не соответствует количеству в исходной таблице БД");
            Assert.AreEqual("Иванов Иван Иванович", bindingSourceData.Rows[0]["ClientName"], "Несоответствие данных первого клиента");
            Assert.AreEqual("Петров Петр Петрович", bindingSourceData.Rows[1]["ClientName"], "Несоответствие данных идентификатора второго клиента");
        }
    }

    [TestClass]
    public class AuthorizationTests
    {
        Database database = new Database();

        [TestInitialize]
        public void Setup() {
            database.openConnection();
            var command = new SqlCommand("INSERT INTO ROLES ([NAME],LOGIN, PASSWORD) VALUES ('TestUser', 'testUser', 'testPassword')", database.getConnection());
            command.ExecuteNonQuery();
        }

        [TestCleanup]
        public void Cleanup()
        {
            var command = new SqlCommand("DELETE FROM ROLES WHERE LOGIN = 'testUser'", database.getConnection());
            command.ExecuteNonQuery();
            database.closeConnection();
        }

        [TestMethod]
        public void IsUser_ActualUser_Test()
        {
            bool result = Login.IsUser("testUser", "testPassword", database.getConnection());
            Assert.IsTrue(result, "Пользователь не принадлежит базе данных");
        }

        [TestMethod]
        public void IsUser_WrongPass_Test()
        {
            bool result = Login.IsUser("testUser", "wrongPassword", database.getConnection());
            Assert.IsFalse(result, "Пароль для пользователя указан верно");
        }

        [TestMethod]
        public void IsUser_NoUser_Test()
        {
            bool result = Login.IsUser("noUser", "noPassword", database.getConnection());
            Assert.IsFalse(result, "Пользователь принадлежит базе данных");
        }
    }
}
