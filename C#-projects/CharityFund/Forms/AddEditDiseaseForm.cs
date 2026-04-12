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
    public partial class AddEditDiseaseForm : Form
    {
        public string DiseaseName { get; private set; } = string.Empty;
        public string? DiseaseDescription { get; private set; }

        public AddEditDiseaseForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название заболевания", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            DiseaseName = txtName.Text.Trim();
            DiseaseDescription = string.IsNullOrWhiteSpace(txtDescription.Text)
                ? null
                : txtDescription.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
