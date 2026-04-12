namespace CharityFund
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            файлToolStripMenuItem = new ToolStripMenuItem();
            выходToolStripMenuItem = new ToolStripMenuItem();
            таблицыToolStripMenuItem = new ToolStripMenuItem();
            ChildrenToolStripMenuItem = new ToolStripMenuItem();
            DiseasesToolStripMenuItem = new ToolStripMenuItem();
            отчётыToolStripMenuItem = new ToolStripMenuItem();
            ActiveFundraisingsToolStripMenuItem = new ToolStripMenuItem();
            FinishedFundraisingsToolStripMenuItem = new ToolStripMenuItem();
            DiseaseStatisticToolStripMenuItem = new ToolStripMenuItem();
            ForeignDonationsToolStripMenuItem = new ToolStripMenuItem();
            AboutToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { файлToolStripMenuItem, таблицыToolStripMenuItem, отчётыToolStripMenuItem, AboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(782, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            файлToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { выходToolStripMenuItem });
            файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            файлToolStripMenuItem.Size = new Size(59, 24);
            файлToolStripMenuItem.Text = "Файл";
            // 
            // выходToolStripMenuItem
            // 
            выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            выходToolStripMenuItem.Size = new Size(136, 26);
            выходToolStripMenuItem.Text = "Выход";
            выходToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // таблицыToolStripMenuItem
            // 
            таблицыToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ChildrenToolStripMenuItem, DiseasesToolStripMenuItem });
            таблицыToolStripMenuItem.Name = "таблицыToolStripMenuItem";
            таблицыToolStripMenuItem.Size = new Size(85, 24);
            таблицыToolStripMenuItem.Text = "Таблицы";
            // 
            // ChildrenToolStripMenuItem
            // 
            ChildrenToolStripMenuItem.Name = "ChildrenToolStripMenuItem";
            ChildrenToolStripMenuItem.Size = new Size(224, 26);
            ChildrenToolStripMenuItem.Text = "Дети";
            ChildrenToolStripMenuItem.Click += Children_Click;
            // 
            // DiseasesToolStripMenuItem
            // 
            DiseasesToolStripMenuItem.Name = "DiseasesToolStripMenuItem";
            DiseasesToolStripMenuItem.Size = new Size(224, 26);
            DiseasesToolStripMenuItem.Text = "Болезни";
            DiseasesToolStripMenuItem.Click += Diseases_Click;
            // 
            // отчётыToolStripMenuItem
            // 
            отчётыToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ActiveFundraisingsToolStripMenuItem, FinishedFundraisingsToolStripMenuItem, DiseaseStatisticToolStripMenuItem, ForeignDonationsToolStripMenuItem });
            отчётыToolStripMenuItem.Name = "отчётыToolStripMenuItem";
            отчётыToolStripMenuItem.Size = new Size(73, 24);
            отчётыToolStripMenuItem.Text = "Отчёты";
            // 
            // ActiveFundraisingsToolStripMenuItem
            // 
            ActiveFundraisingsToolStripMenuItem.Name = "ActiveFundraisingsToolStripMenuItem";
            ActiveFundraisingsToolStripMenuItem.Size = new Size(650, 26);
            ActiveFundraisingsToolStripMenuItem.Text = "Список активных сборов";
            ActiveFundraisingsToolStripMenuItem.Click += ActiveReports_Click;
            // 
            // FinishedFundraisingsToolStripMenuItem
            // 
            FinishedFundraisingsToolStripMenuItem.Name = "FinishedFundraisingsToolStripMenuItem";
            FinishedFundraisingsToolStripMenuItem.Size = new Size(650, 26);
            FinishedFundraisingsToolStripMenuItem.Text = "Список завершённых сборов";
            FinishedFundraisingsToolStripMenuItem.Click += FinishedReports_Click;
            // 
            // DiseaseStatisticToolStripMenuItem
            // 
            DiseaseStatisticToolStripMenuItem.Name = "DiseaseStatisticToolStripMenuItem";
            DiseaseStatisticToolStripMenuItem.Size = new Size(650, 26);
            DiseaseStatisticToolStripMenuItem.Text = "Список болезней в порядке убывания частоты встречаемости ";
            DiseaseStatisticToolStripMenuItem.Click += DiseaseStatistic_Click;
            // 
            // ForeignDonationsToolStripMenuItem
            // 
            ForeignDonationsToolStripMenuItem.Name = "ForeignDonationsToolStripMenuItem";
            ForeignDonationsToolStripMenuItem.Size = new Size(650, 26);
            ForeignDonationsToolStripMenuItem.Text = "Список сборов, в которых имеются пожертвования из иностранных государств";
            ForeignDonationsToolStripMenuItem.Click += ForeignDonations_Click;
            // 
            // AboutToolStripMenuItem
            // 
            AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            AboutToolStripMenuItem.Size = new Size(118, 24);
            AboutToolStripMenuItem.Text = "О программе";
            AboutToolStripMenuItem.Click += About_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 427);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(782, 26);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(59, 20);
            statusLabel.Text = "Статус: ";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Фонд помощи детям";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem файлToolStripMenuItem;
        private ToolStripMenuItem выходToolStripMenuItem;
        private ToolStripMenuItem таблицыToolStripMenuItem;
        private ToolStripMenuItem ChildrenToolStripMenuItem;
        private ToolStripMenuItem DiseasesToolStripMenuItem;
        private ToolStripMenuItem отчётыToolStripMenuItem;
        private ToolStripMenuItem ActiveFundraisingsToolStripMenuItem;
        private ToolStripMenuItem FinishedFundraisingsToolStripMenuItem;
        private ToolStripMenuItem DiseaseStatisticToolStripMenuItem;
        private ToolStripMenuItem ForeignDonationsToolStripMenuItem;
        private ToolStripMenuItem AboutToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
    }
}
