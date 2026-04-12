using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharityFund.Forms
{
    public partial class AddEditChildForm : Form
    {
        public string FullName { get; private set; } = string.Empty;
        public DateTime BirthDate { get; private set; }

        public AddEditChildForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО");
                DialogResult = DialogResult.None;
                return;
            }

            FullName = txtFullName.Text;
            BirthDate = dtpBirthDate.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
