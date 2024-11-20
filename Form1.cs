using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;

namespace NetCrawler
{
    public partial class Form1 : Form
    {
        private string subnet;
        public Form1()
        {
            InitializeComponent();
            GetIP.Click += GetIP_Click;
            ScanNW.Click += ScanNW_Click;
            auditBtn.Click += auditBtn_Click;
            InitializeListView();
            this.Load += Form1_Load;
        }
        private void Form1_Load(object sender, EventArgs e) { }

        //list view, shows columns in main program window for ip address information
        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.Columns.Add("IP Address", 100);
            listView1.Columns.Add("Hostname", 180);
            listView1.Columns.Add("Response Time", 110);
            listView1.Columns.Add("Probable O/S", 80);
            listView1.Columns.Add("Root Shares", 80);
            listView1.Columns.Add("Open SMB Ports", 150);

        //Context menu options for right clicking on an ip address

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem openRootFolderItem = new ToolStripMenuItem("Open Root Shared Folder");
            openRootFolderItem.Click += OpenRootFolderItem_Click;
            contextMenu.Items.Add(openRootFolderItem);

            ToolStripMenuItem downloadSharedFoldersItem = new ToolStripMenuItem("Download Shared Folders");
            downloadSharedFoldersItem.Click += DownloadSharedFoldersItem_Click;
            contextMenu.Items.Add(downloadSharedFoldersItem);

            ToolStripMenuItem scanPortsMenuItem = new ToolStripMenuItem("Scan Ports");
            scanPortsMenuItem.Click += ScanPortsMenuItem_Click;
            contextMenu.Items.Add(scanPortsMenuItem);

            ToolStripMenuItem listSharedItemsMenuItem = new ToolStripMenuItem("List Shared Items");
            listSharedItemsMenuItem.Click += ListSharedItemsItem_Click;
            contextMenu.Items.Add(listSharedItemsMenuItem);
            listView1.ContextMenuStrip = contextMenu;
        }

        //Method for listing number and names of shared items in a message box popup
        //To be replaced with a tree view window

        private void ListSharedItemsItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string ipAddress = listView1.SelectedItems[0].Text;
                var sharedFolders = GetSharedFoldersUsingNetView(ipAddress);
                if (sharedFolders.Count > 0)
                {
                    string message = $"Shared Folders ({sharedFolders.Count}):\n" + string.Join("\n", sharedFolders);
                    MessageBox.Show(message, "Shared Items List");

                    // Update the "Shared Folders" column in the ListView
                    listView1.SelectedItems[0].SubItems[4].Text = sharedFolders.Count.ToString();
                }
                else
                {
                    MessageBox.Show("No shared folders found or access denied.", "Access Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Update the "Shared Folders" column to indicate no shares
                    listView1.SelectedItems[0].SubItems[4].Text = "0";
                }
            }

        }

        //Method to scan for shared folders
        private List<string> GetSharedFoldersUsingNetView(string ipAddress)
        {
            var sharedFolders = new List<string>();
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C net view \\\\{ipAddress}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(processStartInfo))
                using (var reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    bool foundHeader = false;
                    foreach (string line in lines)
                    {
                        if (line.Contains("Share name"))
                        {
                            foundHeader = true;
                            continue;
                        }
                        if (foundHeader && line.Contains("Disk"))
                        {
                            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0) sharedFolders.Add(parts[0]);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please check your credentials.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting shared folders: {ex.Message}");
            }
            return sharedFolders;
        }

        //Method to open root shared folder

        // Event handler for opening root shared folder
        private void OpenRootFolderItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string ipAddress = listView1.SelectedItems[0].Text;
                OpenRootSharedFolder(ipAddress);
            }
        }

        // Method to open root shared folder
        private void OpenRootSharedFolder(string ipAddress)
        {
            string sharedFolder = $@"\\{ipAddress}\";
            try
            {
                System.Diagnostics.Process.Start(sharedFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening shared folder: {ex.Message}");
            }
        }


        //Method for copying root directory of ip address
        ////NOT WORKING YET////
        private void CopyDirectory(DirectoryInfo sourceDir, string destPath)
        {
            Directory.CreateDirectory(destPath);
            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string filePath = Path.Combine(destPath, file.Name);
                file.CopyTo(filePath, true);
            }
            foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
            {
                string subDirPath = Path.Combine(destPath, subDir.Name);
                CopyDirectory(subDir, subDirPath);
            }
        }

        //Method for retrieving all local ip addresses for the combo box
        private List<Tuple<string, string>> GetAllLocalIPAddresses()
        {
            List<Tuple<string, string>> ipAddresses = new List<Tuple<string, string>>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ipInfo in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            string description = $"{networkInterface.Description} ({networkInterface.NetworkInterfaceType})";
                            ipAddresses.Add(new Tuple<string, string>(ipInfo.Address.ToString(), description));
                        }
                    }
                }
            }
            return ipAddresses;
        }
        private void PopulateIPComboBox()
        {
            locaddsCmb.Items.Clear();
            var localIPs = GetAllLocalIPAddresses();
            foreach (var ip in localIPs)
            {
                locaddsCmb.Items.Add($"{ip.Item1} - {ip.Item2}");
            }
            if (locaddsCmb.Items.Count > 0)
            {
                locaddsCmb.SelectedIndex = 0;
            }
        }
        private void GetIP_Click(object sender, EventArgs e)
        {
            PopulateIPComboBox();
            if (locaddsCmb.SelectedItem != null)
            {
                string selectedIP = locaddsCmb.SelectedItem.ToString().Split('-')[0].Trim();
                subnet = string.Join(".", selectedIP.Split('.')[0], selectedIP.Split('.')[1], selectedIP.Split('.')[2]);
                Console.WriteLine("Subnet: " + subnet);
            }
            else
            {
                MessageBox.Show("Please select a valid IP address.");
            }
        }

        //Method for scanning network, retrieving active IP addresses and other information
        private void ScanNW_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(subnet))
            {
                // Show the progress bar
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Visible = true;

                // Disable other UI elements while scan is running
                ScanNW.Enabled = false;
                GetIP.Enabled = false;

                // Start the scan asynchronously
                Task.Run(async () =>
                {
                    List<string[]> activeIPs = await ScanNetworkAsync(subnet);

                    // Update ListView after scan completes
                    this.Invoke((Action)(() => PopulateListView(activeIPs)));

                    // Hide the progress bar and re-enable UI elements
                    this.Invoke((Action)(() =>
                    {
                        progressBar1.Visible = false;
                        ScanNW.Enabled = true;
                        GetIP.Enabled = true;
                    }));
                });
            }
            else
            {
                MessageBox.Show("Please get the local IP address first.");
            }
        }

        private async void auditBtn_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                // Show progress bar while rescanning
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Visible = true;

                // Disable other UI elements while scan is running
                ScanNW.Enabled = false;
                auditBtn.Enabled = false;

                // Start rescanning asynchronously
                List<TreeNode> allFolderNodes = new List<TreeNode>();
                foreach (ListViewItem item in listView1.Items)
                {
                    string ipAddress = item.Text; // Get IP address from ListView

                    // Get the shared folders and sub-items for this IP
                    var folderNodes = await GetSharedFoldersAndSubItemsAsync(ipAddress);
                    allFolderNodes.AddRange(folderNodes);
                }

                // Now open a new form and show the tree view
                AuditForm auditForm = new AuditForm();
                auditForm.LoadTreeView(allFolderNodes);
                auditForm.Show();

                // Accumulate the data for all nodes in a string builder
                StringBuilder reportContent = new StringBuilder();

                // Loop through all the nodes and add their text to the report
                foreach (TreeNode node in allFolderNodes)
                {
                    reportContent.AppendLine($"IP Address: {node.Text}");
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        reportContent.AppendLine($"  - {childNode.Text}");
                    }
                }

                // Show SaveFileDialog to prompt the user to choose a location to save the file
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
                    saveFileDialog.DefaultExt = "txt";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.Title = "Save Shared Items Report";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Get the selected file path from the dialog
                        string filePath = saveFileDialog.FileName;

                        // Write the accumulated report content to the chosen file
                        try
                        {
                            File.WriteAllText(filePath, reportContent.ToString());
                            MessageBox.Show($"The shared items have been written to: {filePath}", "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error writing to file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                // Hide the progress bar and re-enable UI elements
                progressBar1.Visible = false;
                ScanNW.Enabled = true;
                auditBtn.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please scan the network first.");
            }
        }



        private async Task<List<TreeNode>> GetSharedFoldersAndSubItemsAsync(string ipAddress)
        {
            var sharedFolders = GetSharedFoldersUsingNetView(ipAddress); // Existing method to get shared folders
            var folderNodes = new List<TreeNode>();

            foreach (var folder in sharedFolders)
            {
                string folderPath = $@"\\{ipAddress}\{folder}";

                // Create a TreeNode for the shared folder
                var rootNode = new TreeNode(folder)
                {
                    Tag = folderPath  // Store the full path in the Tag
                };

                // Fetch sub-items (files and directories) asynchronously
                var subItems = await GetSubItemsAsync(folderPath);
                foreach (var subItem in subItems)
                {
                    var subItemNode = new TreeNode(subItem)
                    {
                        Tag = subItem // Store the full sub-item path
                    };
                    rootNode.Nodes.Add(subItemNode);
                }

                folderNodes.Add(rootNode);
            }

            return folderNodes;
        }

        private async Task WriteSharedFoldersToFile(string ipAddress, string outputFilePath)
        {
            // Get the shared folders for the IP address
            var sharedFolders = await GetSharedFoldersAndSubItemsAsync(ipAddress);

            try
            {
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    // Write a header
                    writer.WriteLine($"Shared Items for IP Address: {ipAddress}");
                    writer.WriteLine($"Generated on: {DateTime.Now}");
                    writer.WriteLine(new string('-', 50));

                    // Loop through each shared folder
                    foreach (var folderNode in sharedFolders)
                    {
                        // Write the root folder (shared folder)
                        await WriteFolderToFile(folderNode, writer, 0);
                    }

                    writer.WriteLine(new string('-', 50));
                    writer.WriteLine("End of report");
                }

                MessageBox.Show($"The shared items have been written to: {outputFilePath}", "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task WriteFolderToFile(TreeNode folderNode, StreamWriter writer, int indentLevel)
        {
            // Indent the folder based on the indentLevel
            string indent = new string(' ', indentLevel * 2);
            writer.WriteLine($"{indent}Folder: {folderNode.Text}");

            string folderPath = folderNode.Tag.ToString(); // Full folder path

            // Get the sub-items (files and subdirectories) for the folder
            var subItems = await GetSubItemsAsync(folderPath);

            foreach (var subItem in subItems)
            {
                if (Directory.Exists(subItem))
                {
                    // If it's a directory, call recursively to list subfolders
                    var subFolderNode = new TreeNode(subItem)
                    {
                        Tag = subItem
                    };

                    await WriteFolderToFile(subFolderNode, writer, indentLevel + 1);
                }
                else
                {
                    // If it's a file, simply write it
                    writer.WriteLine($"{indent}  File: {subItem}");
                }
            }
        }



        private async Task<List<string>> GetSubItemsAsync(string folderPath)
        {
            var subItems = new List<string>();
            try
            {
                // Add directories and their files recursively
                await Task.Run(() => TraverseDirectories(folderPath, subItems));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sub-items for {folderPath}: {ex.Message}");
            }

            return subItems;
        }

        // Recursive method to get subfolders and files
        private void TraverseDirectories(string currentDirectory, List<string> subItems)
        {
            try
            {
                // Add files in the current directory
                var files = Directory.GetFiles(currentDirectory);
                subItems.AddRange(files);

                // Recurse into subdirectories
                var directories = Directory.GetDirectories(currentDirectory);
                foreach (var dir in directories)
                {
                    subItems.Add(dir); // Add the subdirectory to the list

                    // Recursively traverse this directory
                    TraverseDirectories(dir, subItems);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle cases where access is denied to a folder
                Console.WriteLine($"Access denied: {currentDirectory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error traversing directory {currentDirectory}: {ex.Message}");
            }
        }



        private async Task<List<string[]>> ScanNetworkAsync(string subnet)
        {
            var tasks = new List<Task<string[]>>();

            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}.{i}";
                tasks.Add(PingAndGetHostNameAndPortsAsync(ip)); // Keep the existing ping and info fetch
            }

            var results = await Task.WhenAll(tasks);
            var activeIPs = new List<string[]>();

            foreach (var result in results)
            {
                if (result != null)
                {
                    activeIPs.Add(result);
                }
            }

            // Now, after scanning the network, get the shared folders for each active IP asynchronously
            var folderTasks = new List<Task>();

            foreach (var ipInfo in activeIPs)
            {
                string ipAddress = ipInfo[0]; // Get the IP Address
                var task = Task.Run(() =>
                {
                    var sharedFolders = GetSharedFoldersUsingNetView(ipAddress);
                    int sharedFolderCount = sharedFolders.Count;  // Count the number of shared folders

                    // Update shared folder count in the ListView in a thread-safe manner
                    this.Invoke((Action)(() =>
                    {
                        ipInfo[4] = sharedFolderCount.ToString();  // Update column 4 with the count
                    }));
                });
                folderTasks.Add(task);
            }

            await Task.WhenAll(folderTasks); // Wait for all shared folder tasks to complete
            return activeIPs;
        }


        private async Task<string[]> PingAndGetHostNameAndPortsAsync(string ip)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var reply = await ping.SendPingAsync(ip, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        string hostName = await GetHostNameAsync(ip);
                        string pingTime = reply.RoundtripTime.ToString();
                        string osInfo = GetOperatingSystem(reply.Options.Ttl);

                        // Scan for open SMB ports
                        int[] openPorts = await ScanOpenFileSharingPortsAsync(ip);
                        string ports = openPorts.Length > 0 ? string.Join(", ", openPorts) : "None";

                        return new string[] { ip, hostName, pingTime, osInfo, "", ports };
                    }
                }
                catch (PingException) { }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error pinging IP {ip}: {ex.Message}");
                }
            }
            return null;
        }

        private async Task<string> GetHostNameAsync(string ip)
        {
            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(ip);
                return hostEntry.HostName;
            }
            catch (Exception)
            {
                return "Unknown Host";
            }
        }
        private string GetOperatingSystem(int ttl)
        {
            if (ttl <= 128) return "Windows";
            else if (ttl <= 64) return "Linux";
            else return "Other";
        }
        private void PopulateListView(List<string[]> activeIPs)
        {
            listView1.Items.Clear();
            foreach (var ipInfo in activeIPs)
            {
                ListViewItem item = new ListViewItem(ipInfo);
                listView1.Items.Add(item);
            }
        }

        // Method to asynchronously scan for file sharing ports on a given IP address
        private async Task<int[]> ScanOpenFileSharingPortsAsync(string ipAddress)
        {
            List<int> openPorts = new List<int>();
            int[] fileSharingPorts = { 445, 139 }; // SMB ports

            try
            {
                foreach (int port in fileSharingPorts)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        tcpClient.ReceiveTimeout = 100; // Adjust timeout as needed
                        await tcpClient.ConnectAsync(ipAddress, port);

                        // If connection succeeds, the port is open
                        openPorts.Add(port);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle connection errors or timeouts
                Console.WriteLine($"Error scanning port: {ex.Message}");
            }

            return openPorts.ToArray(); // Convert List<int> to int[]
        }
        private void DownloadSharedFoldersItem_Click(object sender, EventArgs e) { }
        private void ScanPortsMenuItem_Click(object sender, EventArgs e) { }


    }
}
