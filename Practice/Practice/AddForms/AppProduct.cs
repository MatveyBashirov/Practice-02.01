using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Practice.AddForms
{
    public partial class AppProductForm : Form
    {
        public string NewProductName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public string Category { get; private set; }

        public AppProductForm()
        {
            InitializeComponent();
        }

        private void add_button_Click(object sender, EventArgs e)
        {
            NewProductName = productNameTextBox.Text;
            Price = decimal.Parse(priceTextBox.Text);
            Quantity = int.Parse(quantityTextBox.Text);
            Category = categoryTextBox.Text;

            this.DialogResult = DialogResult.OK; // Устанавливаем результат диалога
            this.Close(); // Закрываем форму
        }
    }
}
