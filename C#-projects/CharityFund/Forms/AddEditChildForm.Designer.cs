namespace CharityFund.Forms
{
    partial class AddEditChildForm
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
            label1 = new Label();
            label2 = new Label();
            txtFullName = new TextBox();
            dtpBirthDate = new DateTimePicker();
            btnOk = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 18);
            label1.Name = "label1";
            label1.Size = new Size(49, 20);
            label1.TabIndex = 0;
            label1.Text = "ФИО: ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 59);
            label2.Name = "label2";
            label2.Size = new Size(119, 20);
            label2.TabIndex = 1;
            label2.Text = "Дата рождения:";
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(171, 15);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(181, 27);
            txtFullName.TabIndex = 2;
            // 
            // dtpBirthDate
            // 
            dtpBirthDate.Format = DateTimePickerFormat.Short;
            dtpBirthDate.Location = new Point(171, 52);
            dtpBirthDate.MinDate = new DateTime(2008, 1, 1, 0, 0, 0, 0);
            dtpBirthDate.Name = "dtpBirthDate";
            dtpBirthDate.Size = new Size(181, 27);
            dtpBirthDate.TabIndex = 3;
            dtpBirthDate.Value = new DateTime(2026, 2, 28, 0, 0, 0, 0);
            // 
            // btnOk
            // 
            btnOk.Location = new Point(249, 95);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(103, 33);
            btnOk.TabIndex = 4;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // AddEditChildForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(364, 140);
            Controls.Add(btnOk);
            Controls.Add(dtpBirthDate);
            Controls.Add(txtFullName);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "AddEditChildForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Добавление ребёнка";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox txtFullName;
        private DateTimePicker dtpBirthDate;
        private Button btnOk;
    }
}