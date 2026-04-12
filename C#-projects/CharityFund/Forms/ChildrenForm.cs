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

using CharityFund.Forms;

namespace CharityFund.Forms
{
    public partial class ChildrenForm : Form
    {
        private readonly DatabaseService _dbService;
        public ChildrenForm(DatabaseService dbService)
        {
            _dbService = dbService;
            InitializeComponent();

            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var children = await _dbService.GetChildrenAsync();
                dataGridViewChildren.DataSource = null;
                dataGridViewChildren.DataSource = children;

                if (dataGridViewChildren.Columns["Id"] != null)
                    dataGridViewChildren.Columns["Id"].HeaderText = "ID";
                if (dataGridViewChildren.Columns["FullName"] != null)
                    dataGridViewChildren.Columns["FullName"].HeaderText = "ФИО";
                if (dataGridViewChildren.Columns["BirthDate"] != null)
                    dataGridViewChildren.Columns["BirthDate"].HeaderText = "Дата рождения";

                dataGridViewChildren.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridViewChildren.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

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
            var dialog = new AddEditChildForm();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                await _dbService.AddChildAsync(dialog.FullName, dialog.BirthDate);
                await LoadDataAsync();
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewChildren.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var child = (Child)dataGridViewChildren.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"Удалить {child.FullName}?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await _dbService.DeleteChildAsync(child.Id);
                await LoadDataAsync();
            }
        }
    }
}
