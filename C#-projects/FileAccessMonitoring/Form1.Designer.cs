namespace КР_ЗД
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            StopAllMonitoring();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageFolders = new System.Windows.Forms.TabPage();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPagePrinters = new System.Windows.Forms.TabPage();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnSelectPrinter = new System.Windows.Forms.Button();
            this.txtPrinter = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.tabPageRegistry = new System.Windows.Forms.TabPage();
            this.btnBrowseRegistry = new System.Windows.Forms.Button();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.txtRegistry = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.справкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtFolder2 = new System.Windows.Forms.TextBox();
            this.btnBrowse2 = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPageFolders.SuspendLayout();
            this.tabPagePrinters.SuspendLayout();
            this.tabPageRegistry.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageFolders);
            this.tabControl1.Controls.Add(this.tabPagePrinters);
            this.tabControl1.Controls.Add(this.tabPageRegistry);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.tabControl1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tabControl1.Location = new System.Drawing.Point(12, 43);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(925, 216);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);
            // 
            // tabPageFolders
            // 
            this.tabPageFolders.Controls.Add(this.checkBox3);
            this.tabPageFolders.Controls.Add(this.checkBox2);
            this.tabPageFolders.Controls.Add(this.checkBox1);
            this.tabPageFolders.Controls.Add(this.label3);
            this.tabPageFolders.Controls.Add(this.btnBrowse);
            this.tabPageFolders.Controls.Add(this.txtFolder);
            this.tabPageFolders.Controls.Add(this.label1);
            this.tabPageFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPageFolders.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.tabPageFolders.Location = new System.Drawing.Point(4, 26);
            this.tabPageFolders.Name = "tabPageFolders";
            this.tabPageFolders.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFolders.Size = new System.Drawing.Size(917, 186);
            this.tabPageFolders.TabIndex = 0;
            this.tabPageFolders.Text = "Папки";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(9, 158);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(221, 22);
            this.checkBox3.TabIndex = 8;
            this.checkBox3.Text = "Включить мониториг папки";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(213, 113);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(139, 22);
            this.checkBox2.TabIndex = 7;
            this.checkBox2.Text = "Запись файлов";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(213, 69);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(138, 22);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Чтение файлов";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(99, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 18);
            this.label3.TabIndex = 5;
            this.label3.Text = "Тип доступа:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.btnBrowse.Location = new System.Drawing.Point(732, 14);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(179, 37);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "Выбрать папку";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.txtFolder.Location = new System.Drawing.Point(213, 21);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(493, 24);
            this.txtFolder.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Папка для мониторинга:";
            // 
            // tabPagePrinters
            // 
            this.tabPagePrinters.Controls.Add(this.checkBox10);
            this.tabPagePrinters.Controls.Add(this.checkBox9);
            this.tabPagePrinters.Controls.Add(this.checkBox8);
            this.tabPagePrinters.Controls.Add(this.label9);
            this.tabPagePrinters.Controls.Add(this.btnSelectPrinter);
            this.tabPagePrinters.Controls.Add(this.txtPrinter);
            this.tabPagePrinters.Controls.Add(this.label4);
            this.tabPagePrinters.Controls.Add(this.checkBox4);
            this.tabPagePrinters.Location = new System.Drawing.Point(4, 26);
            this.tabPagePrinters.Name = "tabPagePrinters";
            this.tabPagePrinters.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePrinters.Size = new System.Drawing.Size(917, 186);
            this.tabPagePrinters.TabIndex = 1;
            this.tabPagePrinters.Text = "Принтеры";
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.Location = new System.Drawing.Point(240, 123);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(137, 22);
            this.checkBox10.TabIndex = 16;
            this.checkBox10.Text = "Ошибка печати";
            this.checkBox10.UseVisualStyleBackColor = true;
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.Location = new System.Drawing.Point(240, 95);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(166, 22);
            this.checkBox9.TabIndex = 15;
            this.checkBox9.Text = "Завершение печати";
            this.checkBox9.UseVisualStyleBackColor = true;
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Location = new System.Drawing.Point(240, 67);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(168, 22);
            this.checkBox8.TabIndex = 14;
            this.checkBox8.Text = "Отправка на печать";
            this.checkBox8.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(103, 67);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(105, 18);
            this.label9.TabIndex = 13;
            this.label9.Text = "Тип действия:";
            // 
            // btnSelectPrinter
            // 
            this.btnSelectPrinter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.btnSelectPrinter.Location = new System.Drawing.Point(719, 15);
            this.btnSelectPrinter.Name = "btnSelectPrinter";
            this.btnSelectPrinter.Size = new System.Drawing.Size(179, 37);
            this.btnSelectPrinter.TabIndex = 12;
            this.btnSelectPrinter.Text = "Выбрать принтер";
            this.btnSelectPrinter.UseVisualStyleBackColor = true;
            this.btnSelectPrinter.Click += new System.EventHandler(this.btnSelectPrinter_Click);
            // 
            // txtPrinter
            // 
            this.txtPrinter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.txtPrinter.Location = new System.Drawing.Point(240, 22);
            this.txtPrinter.Name = "txtPrinter";
            this.txtPrinter.ReadOnly = true;
            this.txtPrinter.Size = new System.Drawing.Size(459, 24);
            this.txtPrinter.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.label4.Location = new System.Drawing.Point(6, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(194, 18);
            this.label4.TabIndex = 10;
            this.label4.Text = "Принтер для мониторинга:";
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(6, 158);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(244, 22);
            this.checkBox4.TabIndex = 9;
            this.checkBox4.Text = "Включить мониториг принтера";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // tabPageRegistry
            // 
            this.tabPageRegistry.Controls.Add(this.btnBrowseRegistry);
            this.tabPageRegistry.Controls.Add(this.checkBox7);
            this.tabPageRegistry.Controls.Add(this.checkBox6);
            this.tabPageRegistry.Controls.Add(this.label7);
            this.tabPageRegistry.Controls.Add(this.label6);
            this.tabPageRegistry.Controls.Add(this.checkBox5);
            this.tabPageRegistry.Controls.Add(this.txtRegistry);
            this.tabPageRegistry.Controls.Add(this.label5);
            this.tabPageRegistry.Location = new System.Drawing.Point(4, 26);
            this.tabPageRegistry.Name = "tabPageRegistry";
            this.tabPageRegistry.Size = new System.Drawing.Size(917, 186);
            this.tabPageRegistry.TabIndex = 2;
            this.tabPageRegistry.Text = "Реестр";
            // 
            // btnBrowseRegistry
            // 
            this.btnBrowseRegistry.Location = new System.Drawing.Point(732, 8);
            this.btnBrowseRegistry.Name = "btnBrowseRegistry";
            this.btnBrowseRegistry.Size = new System.Drawing.Size(179, 37);
            this.btnBrowseRegistry.TabIndex = 18;
            this.btnBrowseRegistry.Text = "Выбрать раздел";
            this.btnBrowseRegistry.UseVisualStyleBackColor = true;
            this.btnBrowseRegistry.Click += new System.EventHandler(this.btnBrowseRegistry_Click);
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Location = new System.Drawing.Point(301, 133);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(143, 22);
            this.checkBox7.TabIndex = 17;
            this.checkBox7.Text = "Запись в реестр";
            this.checkBox7.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(301, 99);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(138, 22);
            this.checkBox6.TabIndex = 16;
            this.checkBox6.Text = "Чтение реестра";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(174, 99);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 18);
            this.label7.TabIndex = 15;
            this.label7.Text = "Тип доступа:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Gray;
            this.label6.Location = new System.Drawing.Point(6, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(291, 18);
            this.label6.TabIndex = 14;
            this.label6.Text = "Примеры: HKCU\\Software, HKLM\\System\r\n";
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(9, 161);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(298, 22);
            this.checkBox5.TabIndex = 13;
            this.checkBox5.Text = "Включить мониториг раздела реестра";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // txtRegistry
            // 
            this.txtRegistry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.txtRegistry.Location = new System.Drawing.Point(301, 15);
            this.txtRegistry.Name = "txtRegistry";
            this.txtRegistry.Size = new System.Drawing.Size(405, 24);
            this.txtRegistry.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.label5.Location = new System.Drawing.Point(6, 18);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(247, 18);
            this.label5.TabIndex = 11;
            this.label5.Text = "Раздел реестра для мониторинга:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.справкаToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(963, 28);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(59, 24);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(136, 26);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // справкаToolStripMenuItem
            // 
            this.справкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.оПрограммеToolStripMenuItem});
            this.справкаToolStripMenuItem.Name = "справкаToolStripMenuItem";
            this.справкаToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
            this.справкаToolStripMenuItem.Text = "Справка";
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(21, 333);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(103, 47);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Старт";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(143, 333);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(103, 47);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "Стоп";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Location = new System.Drawing.Point(12, 399);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(935, 246);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Отслеживаемые ресурсы";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(9, 27);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(920, 212);
            this.listBox1.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.label8.Location = new System.Drawing.Point(22, 281);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(197, 16);
            this.label8.TabIndex = 8;
            this.label8.Text = "Расположение файла логов: ";
            // 
            // txtFolder2
            // 
            this.txtFolder2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.txtFolder2.Location = new System.Drawing.Point(229, 278);
            this.txtFolder2.Name = "txtFolder2";
            this.txtFolder2.ReadOnly = true;
            this.txtFolder2.Size = new System.Drawing.Size(493, 22);
            this.txtFolder2.TabIndex = 9;
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.btnBrowse2.Location = new System.Drawing.Point(748, 270);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(179, 37);
            this.btnBrowse2.TabIndex = 9;
            this.btnBrowse2.Text = "Выбрать";
            this.btnBrowse2.UseVisualStyleBackColor = true;
            this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(270, 348);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(53, 16);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Статус";
            this.lblStatus.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 657);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnBrowse2);
            this.Controls.Add(this.txtFolder2);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Мониторинг доступа к ресурсам";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPageFolders.ResumeLayout(false);
            this.tabPageFolders.PerformLayout();
            this.tabPagePrinters.ResumeLayout(false);
            this.tabPagePrinters.PerformLayout();
            this.tabPageRegistry.ResumeLayout(false);
            this.tabPageRegistry.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageFolders;
        private System.Windows.Forms.TabPage tabPagePrinters;
        private System.Windows.Forms.TabPage tabPageRegistry;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem справкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.Button btnSelectPrinter;
        private System.Windows.Forms.TextBox txtPrinter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.TextBox txtRegistry;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtFolder2;
        private System.Windows.Forms.Button btnBrowse2;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkBox10;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnBrowseRegistry;
    }
}

