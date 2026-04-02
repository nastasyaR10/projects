using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace КР_ЗД
{
    public partial class RegistryBrowserDialog : Form
    {
        public string SelectedRegistryPath { get; private set; }

        public RegistryBrowserDialog()
        {
            InitializeComponent();
            InitializeComponent1();
            InitializeForm();
            LoadRegistryTree();
        }

        private void InitializeForm()
        {
            this.Text = "Выбор раздела реестра";
            this.Size = new System.Drawing.Size(500, 400);

            if (treeViewRegistry == null)
            {
                treeViewRegistry = new System.Windows.Forms.TreeView();
                treeViewRegistry.Dock = DockStyle.Fill;
                treeViewRegistry.BeforeExpand += treeViewRegistry_BeforeExpand;
                treeViewRegistry.AfterSelect += treeViewRegistry_AfterSelect;
                this.Controls.Add(treeViewRegistry);
            }
        }

        private void LoadRegistryTree()
        {
            treeViewRegistry.Nodes.Clear();

            var hives = new Dictionary<string, RegistryKey>
            {
                { "HKEY_CLASSES_ROOT", Registry.ClassesRoot },
                { "HKEY_CURRENT_USER", Registry.CurrentUser },
                { "HKEY_LOCAL_MACHINE", Registry.LocalMachine },
                { "HKEY_USERS", Registry.Users },
                { "HKEY_CURRENT_CONFIG", Registry.CurrentConfig }
            };

            foreach (var hive in hives)
            {
                var hiveNode = new TreeNode(hive.Key);
                hiveNode.Tag = hive.Key;
                hiveNode.ImageIndex = 0;
                hiveNode.SelectedImageIndex = 0;

                var dummyNode = new TreeNode("Загрузка...");
                hiveNode.Nodes.Add(dummyNode);

                treeViewRegistry.Nodes.Add(hiveNode);
            }
        }

        private void treeViewRegistry_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (node.Nodes.Count == 1 && node.Nodes[0].Text == "Загрузка...")
            {
                node.Nodes.Clear();
                LoadSubKeys(node);
            }
        }

        private void LoadSubKeys(TreeNode parentNode)
        {
            try
            {
                string registryPath = GetFullPath(parentNode);
                RegistryKey rootKey = GetRegistryKeyFromPath(registryPath);

                if (rootKey != null)
                {
                    foreach (string subKeyName in rootKey.GetSubKeyNames())
                    {
                        var node = new TreeNode(subKeyName);
                        node.Tag = subKeyName;
                        node.ImageIndex = 1;
                        node.SelectedImageIndex = 1;

                        try
                        {
                            using (var subKey = rootKey.OpenSubKey(subKeyName))
                            {
                                if (subKey != null && subKey.SubKeyCount > 0)
                                {
                                    node.Nodes.Add(new TreeNode("Загрузка..."));
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }

                        parentNode.Nodes.Add(node);
                    }
                    rootKey.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private RegistryKey GetRegistryKeyFromPath(string path)
        {
            try
            {
                if (path.StartsWith("HKEY_CLASSES_ROOT\\"))
                    return Registry.ClassesRoot.OpenSubKey(path.Substring(18));
                else if (path.StartsWith("HKEY_CURRENT_USER\\"))
                    return Registry.CurrentUser.OpenSubKey(path.Substring(18));
                else if (path.StartsWith("HKEY_LOCAL_MACHINE\\"))
                    return Registry.LocalMachine.OpenSubKey(path.Substring(19));
                else if (path.StartsWith("HKEY_USERS\\"))
                    return Registry.Users.OpenSubKey(path.Substring(11));
                else if (path.StartsWith("HKEY_CURRENT_CONFIG\\"))
                    return Registry.CurrentConfig.OpenSubKey(path.Substring(20));
                else if (path == "HKEY_CLASSES_ROOT")
                    return Registry.ClassesRoot;
                else if (path == "HKEY_CURRENT_USER")
                    return Registry.CurrentUser;
                else if (path == "HKEY_LOCAL_MACHINE")
                    return Registry.LocalMachine;
                else if (path == "HKEY_USERS")
                    return Registry.Users;
                else if (path == "HKEY_CURRENT_CONFIG")
                    return Registry.CurrentConfig;
            }
            catch
            {
                return null;
            }

            return null;
        }


        private string GetFullPath(TreeNode node)
        {
            var pathParts = new List<string>();
            TreeNode currentNode = node;

            while (currentNode != null)
            {
                if (currentNode.Tag != null)
                    pathParts.Insert(0, currentNode.Tag.ToString());
                currentNode = currentNode.Parent;
            }

            if (pathParts.Count == 1 && pathParts[0].StartsWith("HKEY_"))
            {
                return pathParts[0];
            }

            return string.Join("\\", pathParts);
        }

        private void treeViewRegistry_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedRegistryPath = GetFullPath(e.Node);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (treeViewRegistry.SelectedNode != null)
            {
                SelectedRegistryPath = GetFullPath(treeViewRegistry.SelectedNode);

                SelectedRegistryPath = ConvertRegistryPathFormat(SelectedRegistryPath);

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Выберите раздел реестра", "Предупреждение",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Метод преобразования формата пути
        private string ConvertRegistryPathFormat(string registryPath)
        {
            if (string.IsNullOrEmpty(registryPath))
                return registryPath;

            if (registryPath.StartsWith("HKEY_CLASSES_ROOT\\"))
            {
                return "HKCR\\" + registryPath.Substring(18);
            }
            else if (registryPath.StartsWith("HKEY_CURRENT_USER\\"))
            {
                return "HKCU\\" + registryPath.Substring(18);
            }
            else if (registryPath.StartsWith("HKEY_LOCAL_MACHINE\\"))
            {
                return "HKLM\\" + registryPath.Substring(19);
            }
            else if (registryPath.StartsWith("HKEY_USERS\\"))
            {
                return "HKU\\" + registryPath.Substring(11);
            }
            else if (registryPath.StartsWith("HKEY_CURRENT_CONFIG\\"))
            {
                return "HKCC\\" + registryPath.Substring(20);
            }
            else if (registryPath == "HKEY_CLASSES_ROOT")
            {
                return "HKCR\\";
            }
            else if (registryPath == "HKEY_CURRENT_USER")
            {
                return "HKCU\\";
            }
            else if (registryPath == "HKEY_LOCAL_MACHINE")
            {
                return "HKLM\\";
            }
            else if (registryPath == "HKEY_USERS")
            {
                return "HKU\\";
            }
            else if (registryPath == "HKEY_CURRENT_CONFIG")
            {
                return "HKCC\\";
            }

            return registryPath;
        }

        private System.Windows.Forms.TreeView treeViewRegistry;
        private Panel panelButtons;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private Label labelInfo;

        private void InitializeComponent1()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegistryBrowserDialog));

            this.treeViewRegistry = new System.Windows.Forms.TreeView();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // treeViewRegistry
            this.treeViewRegistry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRegistry.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.treeViewRegistry.Location = new System.Drawing.Point(0, 30);
            this.treeViewRegistry.Name = "treeViewRegistry";
            this.treeViewRegistry.Size = new System.Drawing.Size(484, 331);
            this.treeViewRegistry.TabIndex = 0;
            this.treeViewRegistry.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewRegistry_BeforeExpand);
            this.treeViewRegistry.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewRegistry_AfterSelect);

            // panelButtons
            this.panelButtons.Controls.Add(this.btnOK);
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 361);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(484, 50);
            this.panelButtons.TabIndex = 1;

            // btnOK
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOK.Location = new System.Drawing.Point(296, 15);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(85, 30);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCancel.Location = new System.Drawing.Point(387, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;

            // labelInfo
            this.labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelInfo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelInfo.Location = new System.Drawing.Point(0, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Padding = new System.Windows.Forms.Padding(5);
            this.labelInfo.Size = new System.Drawing.Size(484, 30);
            this.labelInfo.TabIndex = 2;
            this.labelInfo.Text = "Выберите раздел реестра:";
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // RegistryBrowserDialog
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 411);
            this.Controls.Add(this.treeViewRegistry);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.panelButtons);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 450);
            this.Name = "RegistryBrowserDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Обзор реестра";
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
