namespace CharityFund.Forms
{
    partial class AddEditDiseaseForm
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
            btnOk = new Button();
            txtName = new TextBox();
            label2 = new Label();
            label1 = new Label();
            txtDescription = new RichTextBox();
            SuspendLayout();
            // 
            // btnOk
            // 
            btnOk.Location = new Point(275, 204);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(103, 33);
            btnOk.TabIndex = 9;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // txtName
            // 
            txtName.Location = new Point(169, 17);
            txtName.Name = "txtName";
            txtName.Size = new Size(209, 27);
            txtName.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 61);
            label2.Name = "label2";
            label2.Size = new Size(82, 20);
            label2.TabIndex = 6;
            label2.Text = "Описание:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 20);
            label1.Name = "label1";
            label1.Size = new Size(84, 20);
            label1.TabIndex = 5;
            label1.Text = "Название: ";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(135, 58);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(243, 140);
            txtDescription.TabIndex = 10;
            txtDescription.Text = "";
            // 
            // AddEditDiseaseForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 249);
            Controls.Add(txtDescription);
            Controls.Add(btnOk);
            Controls.Add(txtName);
            Controls.Add(label2);
            Controls.Add(label1);
            MaximizeBox = false;
            Name = "AddEditDiseaseForm";
            Text = "Добавление заболевания";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnOk;
        private TextBox txtName;
        private Label label2;
        private Label label1;
        private RichTextBox txtDescription;
    }
}