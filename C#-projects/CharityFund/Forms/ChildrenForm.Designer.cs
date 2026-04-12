namespace CharityFund.Forms
{
    partial class ChildrenForm
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
            dataGridViewChildren = new DataGridView();
            panel1 = new Panel();
            btnDelete = new Button();
            btnAdd = new Button();
            btnRefresh = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewChildren).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridViewChildren
            // 
            dataGridViewChildren.AllowUserToAddRows = false;
            dataGridViewChildren.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewChildren.Dock = DockStyle.Fill;
            dataGridViewChildren.Location = new Point(0, 0);
            dataGridViewChildren.Name = "dataGridViewChildren";
            dataGridViewChildren.ReadOnly = true;
            dataGridViewChildren.RowHeadersWidth = 51;
            dataGridViewChildren.Size = new Size(782, 453);
            dataGridViewChildren.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnDelete);
            panel1.Controls.Add(btnAdd);
            panel1.Controls.Add(btnRefresh);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 382);
            panel1.Name = "panel1";
            panel1.Size = new Size(782, 71);
            panel1.TabIndex = 1;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(582, 14);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(146, 45);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Удалить";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(314, 14);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(146, 45);
            btnAdd.TabIndex = 1;
            btnAdd.Text = "Добавить";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(50, 14);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(146, 45);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "Обновить";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // ChildrenForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(panel1);
            Controls.Add(dataGridViewChildren);
            Name = "ChildrenForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Таблица \"Дети\"";
            ((System.ComponentModel.ISupportInitialize)dataGridViewChildren).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridViewChildren;
        private Panel panel1;
        private Button btnDelete;
        private Button btnAdd;
        private Button btnRefresh;
    }
}