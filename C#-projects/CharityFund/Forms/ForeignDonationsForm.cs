using CharityFund.Services;
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
    public partial class ForeignDonationsForm : Form
    {
        private readonly DatabaseService _dbService;
        public ForeignDonationsForm(DatabaseService dbService)
        {
            _dbService = dbService;
            InitializeComponent();
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                decimal min = decimal.Parse(txtMinSum.Text);
                decimal max = decimal.Parse(txtMaxSum.Text);

                using var reader = await _dbService.GetForeignDonationsStatisticAsync(min, max);
                var dt = new System.Data.DataTable();
                dt.Load(reader);
                dataGridView.DataSource = dt;
                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
