using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class CustomTreeNode : TreeNode
        {
            public ProgressBar ProgressBar { get; set; }

            public CustomTreeNode(string text)
            {
                Text = text;
                ProgressBar = new ProgressBar();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    TreeNode driveNode = treeView1.Nodes.Add(drive.Name);
                    driveNode.Tag = drive.RootDirectory.FullName;
                    driveNode.Nodes.Add("");
                }
            }

            treeView1.BeforeExpand += TreeView1_BeforeExpand;
        }


        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            string directoryPath = selectedNode.Tag.ToString();

            selectedNode.Nodes.Clear();

            try
            {
                string[] directories = Directory.GetDirectories(directoryPath);

                foreach (string dir in directories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    TreeNode dirNode = selectedNode.Nodes.Add(dirInfo.Name);
                    dirNode.Tag = dirInfo.FullName;
                    dirNode.Nodes.Add("");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void FillListBoxWithFiles(string selectedPath)
        {
            listBox1.Items.Clear();

            try
            {
                string[] files = Directory.GetFiles(selectedPath);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    long fileSize = new FileInfo(file).Length;

                    string fileInfo = $"{fileName,-50} {FormatSize(fileSize),15}";

                    listBox1.Items.Add(fileInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F2} {suffixes[suffixIndex]}";
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (Directory.Exists(e.Node.Tag.ToString()))
            {
                string selectedPath = e.Node.Tag.ToString();

                FillListBoxWithFiles(selectedPath);
                if (chart1.Series.IndexOf("FileExtensions") == -1)
                {
                    chart1.Series.Add("FileExtensions");
                    chart1.Series["FileExtensions"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                }
                chart1.Series["FileExtensions"].Points.Clear();

                try
                {
                    string[] files = Directory.GetFiles(selectedPath);

                    Dictionary<string, int> fileExtensionsCount = new Dictionary<string, int>();

                    List<System.Drawing.Color> colors = new List<System.Drawing.Color>
            {
                System.Drawing.Color.Blue, System.Drawing.Color.Red, System.Drawing.Color.Green,
                System.Drawing.Color.Orange, System.Drawing.Color.Purple, System.Drawing.Color.Brown
            };

                    int colorIndex = 0; 

                    foreach (string file in files)
                    {
                        string extension = Path.GetExtension(file);

                        if (!string.IsNullOrEmpty(extension))
                        {
                            extension = extension.TrimStart('.');

                            if (fileExtensionsCount.ContainsKey(extension))
                            {
                                fileExtensionsCount[extension]++;
                            }
                            else
                            {
                                fileExtensionsCount[extension] = 1;
                            }
                        }
                    }


                    foreach (var pair in fileExtensionsCount)
                    {
                        chart1.Series["FileExtensions"].Points.AddXY(pair.Key, pair.Value);
                        chart1.Series["FileExtensions"].Points.Last().Color = colors[colorIndex];

                        double percentFilled = (pair.Value * 100.0) / files.Length;
                        chart1.Series["FileExtensions"].Points.Last().Label = $"{percentFilled:F2}%"; 

                        colorIndex = (colorIndex + 1) % colors.Count;

                    }

                    DriveInfo driveInfo = new DriveInfo(selectedPath);

                    long freeSpace = driveInfo.TotalFreeSpace;
                    long totalSpace = driveInfo.TotalSize;
                    int percentFree = (int)((freeSpace * 100.0) / totalSpace);


                    progressBar1.Value = percentFree;
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void fgjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string selectedDrive = treeView1.SelectedNode.Tag.ToString();

                try
                {
                    DriveInfo driveInfo = new DriveInfo(selectedDrive);

                    if (driveInfo.IsReady)
                    {
                        string driveInfoText = $"Информация о диске {driveInfo.Name}:\n\n";
                        driveInfoText += $"Метка диска: {driveInfo.VolumeLabel}\n";
                        driveInfoText += $"Тип диска: {driveInfo.DriveType}\n";
                        driveInfoText += $"Общий размер диска: {driveInfo.TotalSize} байт\n";
                        driveInfoText += $"Свободное место на диске: {driveInfo.TotalFreeSpace} байт\n";
                        driveInfoText += $"Файловая система: {driveInfo.DriveFormat}\n";
                        driveInfoText += $"Статус диска: {driveInfo.IsReady}\n\n";

                        MessageBox.Show(driveInfoText, "Информация о диске");
                    }
                    else
                    {
                        MessageBox.Show($"Диск {selectedDrive} не доступен или не готов к использованию.", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите диск из TreeView для отображения информации.", "Предупреждение");
            }
        }
    }
}
