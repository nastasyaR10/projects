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
    public partial class FinishedFundraisingsForm : Form
    {
        private readonly DatabaseService _dbService;

        public FinishedFundraisingsForm(DatabaseService dbService)
        {
            _dbService = dbService;
            InitializeComponent();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var reader = await _dbService.GetFinishedFundraisingsAsync();
                var dt = new System.Data.DataTable();
                dt.Load(reader);
                dataGridView.DataSource = dt;

                //dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
