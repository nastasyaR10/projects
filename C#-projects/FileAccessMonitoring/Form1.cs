using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

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
        private HashSet<string> processedPrintJobs = new HashSet<string>();
        string timeStart;

        private bool flagPrint = false;

        public Form1()
        {
            InitializeComponent();
            logFile = "";
        }


        #region Обработка событий элементов управления и формы

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
            aboutForm.label1.Text = "\nКурсовая работа\nпо дисциплине: \"Защита данных\"\n\nРазработка программы протоколирования в специальном файле \nсобытий, связанных с доступом других приложений к выбираемым \nинформационным ресурсам";
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

                System.Windows.Forms.Button btnOk = new System.Windows.Forms.Button() { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(200, 220), Size = new Size(80, 30) };
                System.Windows.Forms.Button btnCancel = new System.Windows.Forms.Button() { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(290, 220), Size = new Size(80, 30) };

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
                    if (File.Exists(txtFolder2.Text))
                    {
                        File.WriteAllText(txtFolder2.Text, string.Empty);
                    }
                }
            }
        }

        private void btnBrowseRegistry_Click(object sender, EventArgs e)
        {
            using (var dialog = new RegistryBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtRegistry.Text = dialog.SelectedRegistryPath;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (IsMonitoring)
            {
                MessageBox.Show("Мониторинг уже запущен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                processedPrintJobs.Clear();

                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                checkBox6.Enabled = false;
                checkBox7.Enabled = false;
                checkBox8.Enabled = false;
                checkBox9.Enabled = false;
                checkBox10.Enabled = false;

                btnBrowse2.Enabled = false;
                btnBrowse.Enabled = false;
                btnSelectPrinter.Enabled = false;
                btnBrowseRegistry.Enabled = false;

                if (checkBox3.Checked && !string.IsNullOrEmpty(txtFolder.Text))
                {
                    txtFolder.Enabled = false;
                    StartFileMonitoring();
                }

                if (checkBox4.Checked && !string.IsNullOrEmpty(txtPrinter.Text))
                {
                    StartPrinterMonitoring();
                }

                if (checkBox5.Checked && !string.IsNullOrEmpty(txtRegistry.Text))
                {
                    txtRegistry.Enabled = false;
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

                DisableAudit();

                string stopMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг остановлен";
                AddLogMessage(stopMessage);

                lblStatus.Text = "Статус: Мониторинг остановлен";
                lblStatus.ForeColor = Color.DarkRed;
                lblStatus.Visible = true;
                txtFolder.Enabled = true;
                txtRegistry.Enabled = true;

                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                checkBox6.Enabled = true;
                checkBox7.Enabled = true;
                checkBox8.Enabled = true;
                checkBox9.Enabled = true;
                checkBox10.Enabled = true;

                btnBrowse2.Enabled = true;
                btnBrowse.Enabled = true;
                btnSelectPrinter.Enabled = true;
                btnBrowseRegistry.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsMonitoring)
            {
                StopAllMonitoring();
                DisableAudit();
            }
        }

        #endregion

        #region Проверка прав админа (необходимо для мониторинга реестра и файловых папок)

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

        #endregion

        #region Управление мониторингом (запуск/остановка)

        private void StartFileMonitoring()
        {
            try
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг папки: {txtFolder.Text}");

                psFileMonitor = new PowerShellHelper(OnPowerShellOutput);
                string script = CreateFileAuditScript(txtFolder.Text);
                psFileMonitor.StartScript(script, "FileMonitor");
            }
            catch (Exception ex)
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | Файловый мониторинг: {ex.Message}");
            }
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

        private void StartRegistryMonitoring()
        {
            try
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Настройка аудита реестра: {txtRegistry.Text}");

                psRegistryMonitor = new PowerShellHelper(OnPowerShellOutput);
                string script = CreateRegistryAuditScript(txtRegistry.Text);
                psRegistryMonitor.StartScript(script, "RegistryMonitor");

                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Мониторинг реестра запущен");
            }
            catch (Exception ex)
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | Реестр: {ex.Message}");
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

        private void DisableAudit()
        {
            try
            {
                bool needDisableFileAudit = checkBox3.Checked && !string.IsNullOrEmpty(txtFolder.Text);
                bool needDisableRegistryAudit = checkBox5.Checked && !string.IsNullOrEmpty(txtRegistry.Text);

                if (!needDisableFileAudit && !needDisableRegistryAudit)
                    return;

                using (Process psProcess = new Process())
                {
                    psProcess.StartInfo.FileName = "powershell.exe";
                    psProcess.StartInfo.UseShellExecute = false;
                    psProcess.StartInfo.CreateNoWindow = true;
                    psProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    StringBuilder commandBuilder = new StringBuilder();

                    if (needDisableFileAudit)
                    {
                        string folderPath = txtFolder.Text.Replace("'", "''");
                        commandBuilder.AppendLine($"$path = '{folderPath}'");
                        commandBuilder.AppendLine("try {");
                        commandBuilder.AppendLine("    $acl = Get-Acl -Path $path -Audit");
                        commandBuilder.AppendLine("    $auditRules = $acl.GetAuditRules($true, $true, [System.Security.Principal.SecurityIdentifier])");
                        commandBuilder.AppendLine("    if ($auditRules.Count -gt 0) {");
                        commandBuilder.AppendLine("        foreach ($rule in $auditRules) {");
                        commandBuilder.AppendLine("            $acl.RemoveAuditRule($rule)");
                        commandBuilder.AppendLine("        }");
                        commandBuilder.AppendLine("        Set-Acl -Path $path -AclObject $acl");
                        commandBuilder.AppendLine("    }");
                        commandBuilder.AppendLine("} catch { }");
                        commandBuilder.AppendLine("");
                        commandBuilder.AppendLine("auditpol /set /subcategory:\"Файловая система\" /success:disable /failure:disable");
                        commandBuilder.AppendLine("auditpol /set /subcategory:\"Общий файловый ресурс\" /success:disable /failure:disable");
                    }

                    if (needDisableRegistryAudit)
                    {
                        string regPath = txtRegistry.Text.Replace("'", "''");
                        commandBuilder.AppendLine($"$regPath = '{regPath}'");
                        commandBuilder.AppendLine("try {");
                        commandBuilder.AppendLine("    $acl = Get-Acl -Path $regPath -Audit");
                        commandBuilder.AppendLine("    $auditRules = $acl.GetAuditRules($true, $true, [System.Security.Principal.SecurityIdentifier])");
                        commandBuilder.AppendLine("    if ($auditRules.Count -gt 0) {");
                        commandBuilder.AppendLine("        foreach ($rule in $auditRules) {");
                        commandBuilder.AppendLine("            $acl.RemoveAuditRule($rule)");
                        commandBuilder.AppendLine("        }");
                        commandBuilder.AppendLine("        Set-Acl -Path $regPath -AclObject $acl");
                        commandBuilder.AppendLine("    }");
                        commandBuilder.AppendLine("} catch { }");
                        commandBuilder.AppendLine("");
                        commandBuilder.AppendLine("auditpol /set /subcategory:\"Реестр\" /success:disable /failure:disable");
                    }

                    string commands = commandBuilder.ToString();
                    byte[] commandBytes = Encoding.Unicode.GetBytes(commands);
                    string encodedCommand = Convert.ToBase64String(commandBytes);

                    psProcess.StartInfo.Arguments = $"-ExecutionPolicy Bypass -NoProfile -EncodedCommand {encodedCommand}";

                    psProcess.Start();
                    psProcess.WaitForExit(5000);
                }
            }
            catch
            {
            }
        }

        #endregion

        #region PowerShell скрипты

        private string CreateFileAuditScript(string folderPath)
        {
            return $@"
        $ErrorView = 'NormalView'
        $InformationPreference = 'Continue'
        $WarningPreference = 'Continue'

        auditpol /set /subcategory:""Файловая система"" /success:enable /failure:enable
        auditpol /set /subcategory:""Общий файловый ресурс"" /success:enable /failure:enable

        $testFolder = ""{folderPath.Replace("\"", "\\\"")}""
        $testFolder = '{folderPath.Replace("'", "''")}'

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

                            $operation = ""ЧТЕНИЕ""
                            if($event.Message -like ""*Запись данных*"" -or $event.Message -like ""*WriteData*"") {{
                                $operation = ""ЗАПИСЬ""
                            }}
                            if($event.Message -like ""*Удаление*"" -or $event.Message -like ""*Delete*"") {{
                                $operation = ""ЗАПИСЬ (УДАЛЕНИЕ)""
                            }}

                            Write-Output ""$time | $process | $operation | $fullPath""

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

        Start-FileMonitoring
        ";
        }

        private string CreateRegistryAuditScript(string registryPath)
        {
            return $@"
$ErrorView = 'NormalView'
$InformationPreference = 'Continue'
$WarningPreference = 'Continue'

auditpol /set /subcategory:""Реестр"" /success:enable /failure:enable

$registryPath = '{registryPath.Replace("'", "''")}'

$acl = Get-Acl -Path $registryPath

$readRule = New-Object System.Security.AccessControl.RegistryAuditRule(
    [System.Security.Principal.SecurityIdentifier]::new(""S-1-1-0""),
    ""QueryValues, ReadKey, EnumerateSubKeys, Notify, ReadPermissions"",
    ""ContainerInherit, ObjectInherit"",
    ""None"",
    ""Success""
)

$writeRule = New-Object System.Security.AccessControl.RegistryAuditRule(
    [System.Security.Principal.SecurityIdentifier]::new(""S-1-1-0""),
    [System.Security.AccessControl.RegistryRights]""SetValue, CreateSubKey, Delete, ChangePermissions, TakeOwnership"",
    ""ContainerInherit, ObjectInherit"",
    ""None"",
    ""Success, Failure""
)

$acl.AddAuditRule($readRule)
$acl.AddAuditRule($writeRule)

Write-Output ""РЕЕСТР_НАСТРОЕН: $registryPath""

if ($registryPath.StartsWith('HKLM')) {{
    $internalPath = '\REGISTRY\MACHINE' + $registryPath.Substring(4)
}} elseif ($registryPath.StartsWith('HKCU')) {{
    $internalPath = '\REGISTRY\USER\*' + $registryPath.Substring(4)
}} else {{
    $internalPath = $registryPath
}}

$xmlFilter = ""<QueryList><Query Id='0' Path='Security'><Select Path='Security'>(*[EventData[Data[@Name='ObjectName']='"" + $internalPath + ""']] or *[EventData[Data[@Name='ObjectName']='"" + $internalPath + ""\*']])</Select></Query></QueryList>""

$lastRecordId = 0

while ($true) {{
    try {{
        $events = Get-WinEvent -FilterXml $xmlFilter -MaxEvents 100 -ErrorAction Stop

        foreach ($event in $events) {{
            if ($event.RecordId -le $lastRecordId) {{ continue }}

            $time = $event.TimeCreated.ToString('HH:mm:ss')

            $processName = 'Unknown'
            
            if ($event.Id -eq 4657) {{
                if ($event.Properties.Count -gt 13) {{
                    $processPath = $event.Properties[13].Value
                    if ($processPath -match '([^\\\\]+\\\\.exe)$') {{
                        $processName = $matches[1]
                    }} elseif ($processPath) {{
                        $processName = $processPath
                    }}
                }}
            }}
            elseif ($event.Id -eq 4663) {{
                if ($event.Properties.Count -gt 11) {{
                    $processPath = $event.Properties[11].Value
                    if ($processPath -match '([^\\\\]+\\\\.exe)$') {{
                        $processName = $matches[1]
                    }} elseif ($processPath) {{
                        $processName = $processPath
                    }}
                }}
            }}
            else {{
                continue
            }}

            $eventRegistryPath = 'Unknown'
            if ($event.Id -eq 4657 -and $event.Properties.Count -gt 4) {{
                $eventRegistryPath = $event.Properties[4].Value
            }}
            elseif ($event.Id -eq 4663 -and $event.Properties.Count -gt 6) {{
                $eventRegistryPath = $event.Properties[6].Value
            }}

            $displayPath = $eventRegistryPath `
                -replace '^\\\\REGISTRY\\\\MACHINE\\\\', 'HKLM\\' `
                -replace '^\\\\REGISTRY\\\\USER\\\\', 'HKCU\\'

            $operation = ''
            if ($event.Id -eq 4657) {{
                $isDelete = $false
                if ($event.Properties.Count -gt 7) {{
                    $operationType = $event.Properties[7].Value
                    if ($operationType -eq '%%1906') {{
                        $isDelete = $true
                    }}
                }}

                $operation = if ($isDelete) {{ 'ЗАПИСЬ (УДАЛЕНИЕ)' }} else {{ 'ЗАПИСЬ' }}
            }}
            elseif ($event.Id -eq 4663) {{
                $operation = 'ЧТЕНИЕ'
            }}
            else {{
                continue
            }}

            Write-Output ""$time | $processName | $operation | $displayPath""

            $lastRecordId = $event.RecordId
        }}

        Start-Sleep -Milliseconds 500

    }} catch {{
        if ($_.Exception.Message -notmatch 'не найдено|No events') {{

        }}
        Start-Sleep -Seconds 1
    }}
}}
";
        }

        #endregion

        #region Функции для записи логов

        private void OnPowerShellOutput(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            Debug.WriteLine($"PowerShell output: {message}");

            if (message.Contains("АУДИТ_НАСТРОЕН:"))
            {
                timeStart = $"{DateTime.Now:HH:mm:ss}";
                return;
            }

            if (message.Contains("РЕЕСТР_НАСТРОЕН:"))
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | РЕЕСТР НАСТРОЕН");
                timeStart = $"{DateTime.Now:HH:mm:ss}";
                return;
            }

            if (message.Contains("ОШИБКА_"))
            {
                AddLogMessage($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ОШИБКА | {message}");
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

                        if (string.Compare(time, timeStart) > 0)
                        {
                            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                            string formattedMessage = $"{currentDate} {time} | {operation} | {process} | {path}";

                            if (operation.Contains("ЧТЕНИЕ") && ((checkBox1.Checked && checkBox3.Checked) || (checkBox6.Checked && checkBox5.Checked)))
                            {
                                AddLogMessage(formattedMessage);
                            }
                            if (operation.Contains("ЗАПИСЬ") && ((checkBox2.Checked && checkBox3.Checked) || (checkBox7.Checked && checkBox5.Checked)))
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

        #endregion

        #region Класс PowerShellHelper

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
                    Debug.WriteLine($"Starting PowerShell script: {name}");

                    byte[] scriptBytes = Encoding.Unicode.GetBytes(scriptContent);
                    string encodedScript = Convert.ToBase64String(scriptBytes);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -NoProfile -EncodedCommand {encodedScript}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        StandardOutputEncoding = Encoding.GetEncoding(866),
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

                    outputReaderThread = new Thread(() => ReadOutput())
                    {
                        IsBackground = true,
                        Name = $"PowerShell_{name}"
                    };
                    outputReaderThread.Start();
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
                                        if (!line.StartsWith("#<") && !line.StartsWith("<?xml") &&
                                            !line.StartsWith("<Objs") && !line.Contains("</Objs>") &&
                                            !line.Contains(" xmlns="))
                                        {
                                            Debug.WriteLine($"PowerShell stdout: {line}");
                                            onOutputReceived?.Invoke(line.Trim());
                                        }
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
                                        if (line.StartsWith("#< CLIXML") || line.StartsWith("<?xml") || line.StartsWith("<Objs"))
                                        {
                                            continue;
                                        }
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

        #endregion
    }
}