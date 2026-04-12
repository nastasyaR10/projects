using CharityFund.Services;
using CharityFund.Forms;

namespace CharityFund
{
    public partial class MainForm : Form
    {
        private readonly DatabaseService _dbService;
        public MainForm(DatabaseService dbService)
        {
            _dbService = dbService;
            InitializeComponent();
        }

        #region --кнопки меню---
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private async void Children_Click(object sender, EventArgs e)
        {
            var form = new ChildrenForm(_dbService);
            form.ShowDialog();
        }

        private async void Diseases_Click(object sender, EventArgs e)
        {
            var form = new DiseasesForm(_dbService);
            form.ShowDialog();
        }

        private async void ActiveReports_Click(object sender, EventArgs e)
        {
            var form = new ActiveFundraisingsForm(_dbService);
            form.ShowDialog();
        }

        private async void FinishedReports_Click(object sender, EventArgs e)
        {
            var form = new FinishedFundraisingsForm(_dbService);
            form.ShowDialog();
        }

        private async void DiseaseStatistic_Click(object sender, EventArgs e)
        {
            var form = new DiseaseStatisticForm(_dbService);
            form.ShowDialog();
        }

        private async void ForeignDonations_Click(object sender, EventArgs e)
        {
            var form = new ForeignDonationsForm(_dbService);
            form.ShowDialog();
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Название: Фонд сбора средств для помощи детям\n" +
                "с тяжелыми заболеваниями\n\n" +
                "Технологии:\n" +
                "- .NET 8\n" +
                "- Windows Forms\n" +
                "- PostgreSQL\n" +
                "- Async/Await\n\n" +
                "© 2026",
                "О программе"
            );
        }
        #endregion

        public void SetDatabaseConnectionStatus(bool isConnected, string errorMessage = "")
        {
            if (isConnected)
            {
                statusLabel.Text = "Подключение к БД установлено";
                statusLabel.ForeColor = Color.Green;
            }
            else
            {
                statusLabel.Text = "Не удалось подключиться к БД";
                statusLabel.ForeColor = Color.Red;

                MessageBox.Show($"Не удалось подключиться к базе данных:\n{errorMessage}",
                    "Ошибка подключения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                ChildrenToolStripMenuItem.Enabled = false;
                DiseasesToolStripMenuItem.Enabled = false;
                ActiveFundraisingsToolStripMenuItem.Enabled = false;
                FinishedFundraisingsToolStripMenuItem.Enabled = false;
                DiseaseStatisticToolStripMenuItem.Enabled = false;
                ForeignDonationsToolStripMenuItem.Enabled = false;
            }
        }
    }
}
