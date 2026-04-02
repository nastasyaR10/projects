using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Threading;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace КР_ЗД
{
    public partial class Form1 : Form
    {
        private string logFile;
        private bool IsMonitoring = false;

        private PowerShellHelper psFileMonitor;
        private PowerShellHelper psRegistryMonitor;
        private Thread printerMonitorThread;
        private CancellationTokenSource monitoringCts;
        private ConcurrentDictionary<string, DateTime> processedEvents = new ConcurrentDictionary<string, DateTime>();
        private HashSet<string> processedPrintJobs = new HashSet<string>();

        private bool flagPrint = false;
        private Dictionary<string, string> printJobStatusHistory = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            logFile = "";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для мониторинга";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutForm = new AboutBox1();
            aboutForm.label1.Text = "Студент: Рощупкина А. Д.\nГруппа: А-18-22\n\nКурсовая работа\nпо дисциплине: \"Защита данных\"\n\nВариант 8. Разработка программы протоколирования в специальном файле \nсобытий, связанных с доступом других приложений к выбираемым \nинформационным ресурсам";
            aboutForm.ShowDialog();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabControl1.TabPages[e.Index];
            using (Brush backgroundBrush = new SolidBrush(SystemColors.Control))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                Color textColor = (e.State == DrawItemState.Selected) ? Color.Black : Color.Gray;
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(page.Text, e.Font, textBrush, e.Bounds, sf);
                }
            }
        }

        private void btnSelectPrinter_Click(object sender, EventArgs e)
        {
            using (Form printerDialog = new Form())
            {
                printerDialog.Text = "Выбор принтера";
                printerDialog.Size = new Size(400, 300);
                printerDialog.StartPosition = FormStartPosition.CenterParent;
                printerDialog.FormBorderStyle = FormBorderStyle.FixedDialog;

                ListBox listBoxPrinters = new ListBox()
                {
                    Location = new Point(10, 10),
                    Size = new Size(360, 200),
                    Font = new Font("Microsoft Sans Serif", 10)
                };

                try
                {
                    listBoxPrinters.Items.Clear();
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                    {
                        foreach (ManagementObject printer in searcher.Get())
                        {
                            string printerName = printer["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(printerName))
                            {
                                listBoxPrinters.Items.Add(printerName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки принтеров: {ex.Message}");
                }

                Button btnOk = new Button() { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(200, 220), Size = new Size(80, 30) };
                Button btnCancel = new Button() { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(290, 220), Size = new Size(80, 30) };

                printerDialog.Controls.AddRange(new Control[] { listBoxPrinters, btnOk, btnCancel });
                printerDialog.AcceptButton = btnOk;
                printerDialog.CancelButton = btnCancel;

                if (printerDialog.ShowDialog() == DialogResult.OK && listBoxPrinters.SelectedItem != null)
                {
                    txtPrinter.Text = listBoxPrinters.SelectedItem.ToString();
                }
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Лог файлы (*.log)|*.log|Текстовые файлы (*.txt)|*.txt";
                dialog.DefaultExt = "log";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    logFile = dialog.FileName;
                    txtFolder2.Text = dialog.FileName;
                }
            }
        }

        private bool CheckAdminRights()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (IsMonitoring)
            {
                MessageBox.Show("Мониторинг уже запущен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // Проверки
            if (checkBox3.Checked && string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Выберите папку для мониторинга", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (checkBox4.Checked && string.IsNullOrEmpty(txtPrinter.Text))
            {
                MessageBox.Show("Выберите принтер для мониторинга", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (checkBox5.Checked && string.IsNullOrEmpty(txtRegistry.Text))
            {
                MessageBox.Show("Выберите раздел реестра для мониторинга", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!checkBox3.Checked && !checkBox4.Checked && !checkBox5.Checked)
            {
                MessageBox.Show("Включите в мониторинг хотя бы один из объектов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtFolder2.Text))
            {
                MessageBox.Show("Выберите файл для сохранения логов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка прав администратора
            bool needAdminRights = checkBox3.Checked || checkBox5.Checked;
            if (needAdminRights && !CheckAdminRights())
            {
                var result = MessageBox.Show(
                    "Для настройки аудита требуются права администратора.\n" +
                    "Запустить программу от имени администратора?",
                    "Требуются права администратора",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        Verb = "runas",
                        UseShellExecute = true
                    };

                    try
                    {
                        Process.Start(startInfo);
                        Application.Exit();
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось запустить с правами администратора", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            try
            {
                IsMonitoring = true;
                listBox1.Items.Clear();
                logFile = txtFolder2.Text;
                monitoringCts = new CancellationTokenSource();
                processedEvents.Clear();
                processedPrintJobs.Clear();

                // Запускаем мониторинг файловой папки
                if (checkBox3.Checked && !string.IsNullOrEmpty(txtFolder.Text))
                {
                    txtFolder.Enabled = false;
                    StartFileMonitoring();
                }

                // Запускаем мониторинг принтера
                if (checkBox4.Checked && !string.IsNullOrEmpty(txtPrinter.Text))
                {
                    StartPrinterMonitoring();
                }

                // Запускаем мониторинг раздела реестра
                if (checkBox5.Checked && !string.IsNullOrEmpty(txtRegistry.Text))
                {
                    txtFolder.Enabled = false;
                    StartRegistryMonitoring();
                }

                lblStatus.Text = "Статус: Мониторинг запущен";
                lblStatus.Visible = true;
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска мониторинга: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                IsMonitoring = false;
            }
        }

        private void StartFileMonitoring()
        {
            try
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг папки: {txtFolder.Text}");

                // Создаем powershell монитор
                psFileMonitor = new PowerShellHelper(OnPowerShellOutput);
                string script = CreateFileAuditScript(txtFolder.Text);
                psFileMonitor.StartScript(script, "FileMonitor");
            }
            catch (Exception ex)
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | Файловый мониторинг: {ex.Message}");
            }
        }

        private string CreateFileAuditScript(string folderPath)
        {
            return $@"
# Настройка аудита файловой системы
auditpol /set /subcategory:""Файловая система"" /success:enable /failure:enable
auditpol /set /subcategory:""Общий файловый ресурс"" /success:enable /failure:enable

# Настройка мониторинга для папки
$testFolder = ""{folderPath.Replace("\"", "\\\"")}""

# Настраиваем аудит
$acl = Get-Acl $testFolder

$readRule = New-Object System.Security.AccessControl.FileSystemAuditRule(
    [System.Security.Principal.SecurityIdentifier]::new(""S-1-1-0""),
    ""ReadData, ReadAttributes, ReadExtendedAttributes, ReadPermissions"",
    ""ContainerInherit, ObjectInherit"",
    ""None"",
    ""Success""
)

$writeRule = New-Object System.Security.AccessControl.FileSystemAuditRule(
    [System.Security.Principal.SecurityIdentifier]::new(""S-1-1-0""),
    ""WriteData, AppendData, WriteAttributes, WriteExtendedAttributes, Delete, DeleteSubdirectoriesAndFiles"",
    ""ContainerInherit, ObjectInherit"",
    ""None"",
    ""Success, Failure""
)

$acl.AddAuditRule($readRule)
$acl.AddAuditRule($writeRule)
Set-Acl -Path $testFolder -AclObject $acl

Write-Output ""АУДИТ_НАСТРОЕН: $testFolder""

# Функция мониторинга
function Start-FileMonitoring {{
    $shownEvents = @{{}}

    Start-Sleep -Seconds 3
    
    while($true) {{
        try {{
            $events = Get-WinEvent -LogName Security -MaxEvents 30 -ErrorAction SilentlyContinue | 
                Where-Object {{ 
                    $_.Id -eq 4663 -and 
                    $_.Message -like ""*$testFolder*""
                }}
            
            foreach($event in $events) {{
                $eventKey = ""$($event.TimeCreated.ToString('HHmmss'))-$($event.Properties[6].Value)""
                
                if(-not $shownEvents.ContainsKey($eventKey)) {{
                    $time = $event.TimeCreated.ToString(""HH:mm:ss"")
                    $fullPath = $event.Properties[6].Value
                    
                    if($fullPath -notlike ""*\*.*"") {{ continue }}

                    $procMatch = [regex]::Match($event.Message, 'Имя процесса:\s*[^\n]*\\([^\n\s\.]+\.(exe|EXE))')
                    $process = if($procMatch.Success) {{ $procMatch.Groups[1].Value }} else {{ ""Система"" }}
                    
                    # Определяем операцию
                    $operation = ""ЧТЕНИЕ""
                    if($event.Message -like ""*Запись данных*"" -or $event.Message -like ""*WriteData*"") {{
                        $operation = ""ЗАПИСЬ""
                    }}
                    if($event.Message -like ""*Удаление*"" -or $event.Message -like ""*Delete*"") {{
                        $operation = ""ЗАПИСЬ (УДАЛЕНИЕ)""
                    }}
                    
                    # Выводим событие
                    Write-Output ""$time | $process | $operation | $fullPath""
                    
                    $shownEvents[$eventKey] = $true
                    
                    # Очистка старых событий
                    if($shownEvents.Count -gt 100) {{
                        $oldKeys = $shownEvents.Keys | Select-Object -First 50
                        foreach($key in $oldKeys) {{
                            $shownEvents.Remove($key)
                        }}
                    }}
                }}
            }}
            
            Start-Sleep -Milliseconds 500
        }}
        catch {{
            Start-Sleep -Seconds 1
        }}
    }}
}}

# Запуск мониторинга
Start-FileMonitoring
";
        }

        private void StartPrinterMonitoring()
        {
            try
            {
                printerMonitorThread = new Thread(() =>
                {
                    while (IsMonitoring && !monitoringCts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            CheckPrinterEvents();
                            Thread.Sleep(3000);
                        }
                        catch
                        {
                            Thread.Sleep(10000);
                        }
                    }
                })
                {
                    IsBackground = true
                };
                printerMonitorThread.Start();

                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг принтера: {txtPrinter.Text}");
            }
            catch (Exception ex)
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | Принтер: {ex.Message}");
            }
        }

        private void CheckPrinterEvents()
        {
            try
            {
                string printerName = txtPrinter.Text;

                using (var jobSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob"))
                {
                    foreach (ManagementObject printJob in jobSearcher.Get())
                    {
                        string jobPrinterName = printJob["Name"]?.ToString() ?? "";

                        if (jobPrinterName.Contains(printerName))
                        {
                            string documentName = printJob["Document"]?.ToString() ?? "Неизвестный документ";
                            string userName = printJob["Owner"]?.ToString() ?? "Неизвестный пользователь";
                            string jobId = printJob["JobId"]?.ToString() ?? "0";
                            string jobStatus = printJob["Status"]?.ToString() ?? "";

                            int totalPages = printJob["TotalPages"] != null ? Convert.ToInt32(printJob["TotalPages"]) : 0;
                            int pagesPrinted = printJob["PagesPrinted"] != null ? Convert.ToInt32(printJob["PagesPrinted"]) : 0;

                            string jobKey = $"{printerName}_{jobId}_{jobStatus}_{pagesPrinted}_{totalPages}";

                            if (!processedPrintJobs.Contains(jobKey))
                            {
                                string status = "";
                                if (jobStatus == "OK")
                                {
                                    status = "ОТПРАВКА НА ПЕЧАТЬ";
                                    flagPrint = true;
                                }
                                else
                                {
                                    if (flagPrint && pagesPrinted > 0 && pagesPrinted < totalPages)
                                    {
                                        status = "ПЕЧАТЬ";
                                    }
                                    else if (flagPrint && pagesPrinted == totalPages && totalPages > 0)
                                    {
                                        status = "ЗАВЕРШЕНИЕ ПЕЧАТИ";
                                        flagPrint = false;
                                    }
                                    else
                                    {
                                        status = "ОШИБКА ПЕЧАТИ";
                                    }
                                }
                                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {status} | {printerName} | {userName} | {documentName}";


                                if (status.Contains("ПЕЧАТЬ") && checkBox8.Checked)
                                {
                                    AddLogMessage(logMessage);
                                }
                                if (status.Contains("ЗАВЕРШЕНИЕ") && checkBox9.Checked)
                                {
                                    AddLogMessage(logMessage);
                                }
                                if (status.Contains("ОШИБКА") && checkBox9.Checked)
                                {
                                    AddLogMessage(logMessage);
                                }
                                processedPrintJobs.Add(jobKey);

                                if (processedPrintJobs.Count > 50)
                                {
                                    var oldestKeys = processedPrintJobs.Take(10).ToList();
                                    foreach (var key in oldestKeys)
                                    {
                                        processedPrintJobs.Remove(key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка мониторинга принтера: {ex.Message}");
            }
        }

        private void StartRegistryMonitoring()
        {
            //try
            //{
            //    AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | СИСТЕМА | Настройка аудита реестра: {txtRegistry.Text}");

            //    psRegistryMonitor = new PowerShellHelper(OnPowerShellOutput);
            //    string script = CreateRegistryAuditScript(txtRegistry.Text);
            //    psRegistryMonitor.StartScript(script, "RegistryMonitor");

            //    AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг реестра запущен");
            //}
            //catch (Exception ex)
            //{
            //    AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | Реестр: {ex.Message}");
            //}
        }

        private string CreateRegistryAuditScript(string registryPath)
        {
            return $@"
# Настройка аудита реестра
auditpol /set /subcategory:""Registry"" /success:enable /failure:enable

# Преобразуем путь реестра
$regPath = ""{registryPath.Replace("\"", "\\\"")}""

# Создаем ключ если его нет
try {{
    $regKey = $regPath -replace '^HKCU:', 'HKEY_CURRENT_USER'
    $regKey = $regKey -replace '^HKLM:', 'HKEY_LOCAL_MACHINE'
    
    if (-not (Test-Path ""Registry::$regPath"")) {{
        New-Item -Path ""Registry::$regPath"" -Force -ErrorAction SilentlyContinue
    }}
    
    Write-Output ""РЕЕСТР_НАСТРОЕН: $regPath""
}} catch {{
    Write-Output ""ОШИБКА_РЕЕСТРА: Не удалось создать ключ""
    exit
}}

# Функция мониторинга реестра
function Start-RegistryMonitoring {{
    $shownEvents = @{{}}
    
    while($true) {{
        try {{
            $events = Get-WinEvent -LogName Security -MaxEvents 30 -ErrorAction SilentlyContinue | 
                Where-Object {{ 
                    $_.Id -eq 4663 -and 
                    $_.Message -like ""*\\Registry*"" -and
                    $_.Message -like ""*$regPath*""
                }}
            
            foreach($event in $events) {{
                $eventKey = ""$($event.TimeCreated.ToString('HHmmss'))-$($event.Properties[6].Value)""
                
                if(-not $shownEvents.ContainsKey($eventKey)) {{
                    $time = $event.TimeCreated.ToString(""HH:mm:ss"")
                    $regPathFull = $event.Properties[6].Value
                    
                    $procMatch = [regex]::Match($event.Message, 'Имя процесса:\\s*[^\\n]*\\\\([^\\n\\s\\.]+\\.(exe|EXE))')
                    $process = if($procMatch.Success) {{ $procMatch.Groups[1].Value }} else {{ ""Система"" }}
                    
                    $operation = ""ЧТЕНИЕ""
                    if($event.Message -like ""*SetValue*"" -or $event.Message -like ""*Запись параметров*"") {{
                        $operation = ""ЗАПИСЬ""
                    }}
                    if($event.Message -like ""*Delete*"" -or $event.Message -like ""*Удаление*"") {{
                        $operation = ""ЗАПИСЬ (УДАЛЕНИЕ)""
                    }}
                    
                    $displayPath = $regPathFull -replace '\\\\REGISTRY\\\\USER\\\\[^\\\\]+', 'HKCU'
                    $displayPath = $displayPath -replace '\\\\REGISTRY\\\\MACHINE\\\\', 'HKLM\\'
                    
                    Write-Output ""$time | $process | $operation | $displayPath""
                    
                    $shownEvents[$eventKey] = $true
                    
                    if($shownEvents.Count -gt 100) {{
                        $oldKeys = $shownEvents.Keys | Select-Object -First 50
                        foreach($key in $oldKeys) {{
                            $shownEvents.Remove($key)
                        }}
                    }}
                }}
            }}
            
            Start-Sleep -Milliseconds 500
        }}
        catch {{
            Start-Sleep -Seconds 1
        }}
    }}
}}

# Запуск мониторинга
Start-RegistryMonitoring
";
        }

        string timeStart;

        private void OnPowerShellOutput(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            Debug.WriteLine($"PowerShell output: {message}");

            // Обработка системных сообщений
            if (message.Contains("АУДИТ_НАСТРОЕН:"))
            {
                timeStart = $"{DateTime.Now:HH:mm:ss}";
                return;
            }

            if (message.Contains("РЕЕСТР_НАСТРОЕН:"))
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | СИСТЕМА | {message.Replace("РЕЕСТР_НАСТРОЕН:", "Реестр настроен:")}");
                return;
            }

            if (message.Contains("ОШИБКА_"))
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | СИСТЕМА | {message}");
                return;
            }

            if (message.Contains("Команда выполнена"))
            {
                return;
            }

            if (message.Contains("|"))
            {
                try
                {
                    string[] parts = message.Split('|');
                    if (parts.Length >= 4)
                    {
                        string time = parts[0].Trim();
                        string process = parts[1].Trim();
                        string operation = parts[2].Trim();
                        string path = parts[3].Trim();

                        // Формируем полную дату-время
                        if (time != timeStart)
                        {
                            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                            string formattedMessage = $"{currentDate} {time} | {operation} | {process} | {path}";

                            if (operation.Contains("ЧТЕНИЕ") && checkBox1.Checked)
                            {
                                AddLogMessage(formattedMessage);
                            }
                            if (operation.Contains("ЗАПИСЬ") && checkBox2.Checked)
                            {
                                AddLogMessage(formattedMessage);
                            }
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка парсинга: {ex.Message}");
                }
            }

            AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!IsMonitoring)
            {
                MessageBox.Show("Мониторинг не запущен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                StopAllMonitoring();
                IsMonitoring = false;

                string stopMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг остановлен";
                AddLogMessage(stopMessage);

                lblStatus.Text = "Статус: Мониторинг остановлен";
                lblStatus.ForeColor = Color.DarkRed;
                lblStatus.Visible = true;
                txtFolder.Enabled = true;
                txtRegistry.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки: {ex.Message}");
            }
        }

        private void StopAllMonitoring()
        {
            try
            {
                monitoringCts?.Cancel();

                psFileMonitor?.Stop();
                psFileMonitor = null;

                psRegistryMonitor?.Stop();
                psRegistryMonitor = null;

                if (printerMonitorThread != null && printerMonitorThread.IsAlive)
                {
                    printerMonitorThread.Join(1000);
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | Остановка: {ex.Message}");
            }
        }

        private void SaveToLogFile(string message)
        {
            if (string.IsNullOrEmpty(logFile)) return;

            try
            {
                File.AppendAllText(logFile, message + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка записи в лог: {ex.Message}");
            }
        }

        private void AddLogMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AddLogMessage), message);
                return;
            }

            try
            {
                listBox1.Items.Add(message);

                if (listBox1.Items.Count > 500)
                {
                    while (listBox1.Items.Count > 400)
                    {
                        listBox1.Items.RemoveAt(0);
                    }
                }

                listBox1.TopIndex = listBox1.Items.Count - 1;

                SaveToLogFile(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка добавления в лог: {ex.Message}");
            }
        }

        private void AddLogMessage1(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AddLogMessage1), message);
                return;
            }

            try
            {
                listBox1.Items.Add(message);
                if (listBox1.Items.Count > 200)
                    listBox1.Items.RemoveAt(0);

                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = -1;

                SaveToLogFile(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка AddLogMessage1: {ex.Message}");
            }
        }

        private class PowerShellHelper
        {
            private Process psProcess;
            private Thread outputReaderThread;
            private readonly Action<string> onOutputReceived;
            private string monitorName;
            private bool isRunning;

            public PowerShellHelper(Action<string> outputHandler)
            {
                onOutputReceived = outputHandler;
                isRunning = false;
            }

            public void StartScript(string scriptContent, string name)
            {
                monitorName = name;
                isRunning = true;

                try
                {
                    // Создаем временный файл со скриптом
                    string scriptPath = Path.Combine(Path.GetTempPath(), $"monitor_{Guid.NewGuid()}.ps1");
                    File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);

                    Debug.WriteLine($"PowerShell script created: {scriptPath}");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{scriptPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        StandardOutputEncoding = Encoding.GetEncoding(866), // Используем кодировку консоли
                        StandardErrorEncoding = Encoding.GetEncoding(866)
                    };

                    psProcess = new Process();
                    psProcess.StartInfo = psi;
                    psProcess.EnableRaisingEvents = true;

                    psProcess.Exited += (s, e) =>
                    {
                        isRunning = false;
                        Debug.WriteLine($"PowerShell process {monitorName} exited");
                    };

                    psProcess.Start();

                    Debug.WriteLine($"PowerShell process started: {monitorName}, PID: {psProcess.Id}");

                    // Запускаем чтение вывода в отдельном потоке
                    outputReaderThread = new Thread(() => ReadOutput())
                    {
                        IsBackground = true,
                        Name = $"PowerShell_{name}"
                    };
                    outputReaderThread.Start();

                    // Удаление временного файла
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Thread.Sleep(5000);
                        try
                        {
                            if (File.Exists(scriptPath))
                                File.Delete(scriptPath);
                        }
                        catch { }
                    });
                }
                catch (Exception ex)
                {
                    isRunning = false;
                    onOutputReceived?.Invoke($"ОШИБКА_{monitorName}_СТАРТ: {ex.Message}");
                    Debug.WriteLine($"PowerShell start error: {ex.Message}");
                }
            }

            private void ReadOutput()
            {
                try
                {
                    Thread stdOutThread = new Thread(() =>
                    {
                        try
                        {
                            using (StreamReader reader = psProcess.StandardOutput)
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null && isRunning)
                                {
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        Debug.WriteLine($"PowerShell stdout: {line}");
                                        onOutputReceived?.Invoke(line.Trim());
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"PowerShell stdout error: {ex.Message}");
                        }
                    })
                    {
                        IsBackground = true
                    };
                    stdOutThread.Start();

                    Thread stdErrThread = new Thread(() =>
                    {
                        try
                        {
                            using (StreamReader reader = psProcess.StandardError)
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null && isRunning)
                                {
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        Debug.WriteLine($"PowerShell stderr: {line}");
                                        onOutputReceived?.Invoke($"ОШИБКА: {line.Trim()}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"PowerShell stderr error: {ex.Message}");
                        }
                    })
                    {
                        IsBackground = true
                    };
                    stdErrThread.Start();

                    stdOutThread.Join();
                    stdErrThread.Join();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PowerShell read output error: {ex.Message}");
                    onOutputReceived?.Invoke($"ОШИБКА_{monitorName}_ЧТЕНИЕ: {ex.Message}");
                }
            }

            public void Stop()
            {
                try
                {
                    isRunning = false;

                    if (psProcess != null && !psProcess.HasExited)
                    {
                        psProcess.Kill();
                        psProcess.WaitForExit(2000);
                        psProcess.Dispose();
                        psProcess = null;
                        Debug.WriteLine($"PowerShell process {monitorName} stopped");
                    }

                    if (outputReaderThread != null && outputReaderThread.IsAlive)
                    {
                        outputReaderThread.Join(1000);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PowerShell stop error: {ex.Message}");
                }
            }
        }
    }
}