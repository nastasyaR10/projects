using CharityFund.Entities;
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
    public partial class DiseasesForm : Form
    {
        private readonly DatabaseService _dbService;
        public DiseasesForm(DatabaseService dbService)
        {
            _dbService = dbService;
            InitializeComponent();

            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var children = await _dbService.GetDiseasesAsync();
                dataGridView.DataSource = null;
                dataGridView.DataSource = children;

                if (dataGridView.Columns["Id"] != null)
                    dataGridView.Columns["Id"].HeaderText = "ID";
                if (dataGridView.Columns["Name"] != null)
                    dataGridView.Columns["Name"].HeaderText = "Название";
                if (dataGridView.Columns["Description"] != null)
                    dataGridView.Columns["Description"].HeaderText = "Описание";

                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var dialog = new AddEditDiseaseForm();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _dbService.AddDiseaseAsync(dialog.DiseaseName, dialog.DiseaseDescription);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении заболевания:\n{ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var disease = (Disease)dataGridView.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"Удалить {disease.Name}?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await _dbService.DeleteDiseaseAsync(disease.Id);
                await LoadDataAsync();
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
