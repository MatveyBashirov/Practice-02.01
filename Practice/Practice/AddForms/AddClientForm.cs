using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Practice.AddForms
{
    public partial class AddClientForm : Form
    {
        public string Name { get; private set; }
        public string Contact { get; private set; }
        public bool IsRegular { get; private set; }

        public AddClientForm()
        {
            InitializeComponent();
        }

        private void add_button_Click(object sender, EventArgs e)
        {
            Name = nameTextBox.Text;
            Contact = contactTextBox.Text;
            IsRegular = checkBox1.Checked;
            this.DialogResult = DialogResult.OK; // Устанавливаем результат диалога
            this.Close(); // Закрываем форму
        }
    }
}
