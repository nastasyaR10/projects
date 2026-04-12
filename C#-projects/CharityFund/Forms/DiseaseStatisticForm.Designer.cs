namespace CharityFund.Forms
{
    partial class DiseaseStatisticForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            btnLoad = new Button();
            txtMaxSum = new TextBox();
            txtMinSum = new TextBox();
            label2 = new Label();
            label1 = new Label();
            dataGridView = new DataGridView();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnLoad);
            panel1.Controls.Add(txtMaxSum);
            panel1.Controls.Add(txtMinSum);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 422);
            panel1.Name = "panel1";
            panel1.Size = new Size(868, 104);
            panel1.TabIndex = 0;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(723, 55);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(133, 38);
            btnLoad.TabIndex = 1;
            btnLoad.Text = "Применить";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // txtMaxSum
            // 
            txtMaxSum.Location = new Point(749, 22);
            txtMaxSum.Name = "txtMaxSum";
            txtMaxSum.Size = new Size(107, 27);
            txtMaxSum.TabIndex = 3;
            // 
            // txtMinSum
            // 
            txtMinSum.Location = new Point(590, 22);
            txtMinSum.Name = "txtMinSum";
            txtMinSum.Size = new Size(107, 27);
            txtMinSum.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(713, 25);
            label2.Name = "label2";
            label2.Size = new Size(26, 20);
            label2.TabIndex = 1;
            label2.Text = "до";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(403, 25);
            label1.Name = "label1";
            label1.Size = new Size(181, 20);
            label1.TabIndex = 0;
            label1.Text = "Средняя сумма сбора от";
            // 
            // dataGridView
            // 
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.Location = new Point(0, 0);
            dataGridView.Name = "dataGridView";
            dataGridView.RowHeadersWidth = 51;
            dataGridView.Size = new Size(868, 422);
            dataGridView.TabIndex = 1;
            // 
            // DiseaseStatisticForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(868, 526);
            Controls.Add(dataGridView);
            Controls.Add(panel1);
            Name = "DiseaseStatisticForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Статистика по заболеваниям";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnLoad;
        private TextBox txtMaxSum;
        private TextBox txtMinSum;
        private Label label2;
        private Label label1;
        private DataGridView dataGridView;
    }
}