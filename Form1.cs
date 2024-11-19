using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace NetCrawler
{
    public partial class Form1 : Form
    {
        private string subnet;

        public Form1()
        {
            InitializeComponent();
            GetIP.Click += GetIP_Click; // Attach the event handler for GetIP button click
            ScanNW.Click += ScanNW_Click; // Attach the event handler for ScanNW button click
            InitializeListView(); // Initialize ListView columns

            // Subscribe to the Form1_Load event
            this.Load += Form1_Load;
        }

        // Event handler for Form1 Load event
        private void Form1_Load(object sender, EventArgs e)
        {
            // Optional: Code to execute when the form is loaded
        }

        // Initialize ListView columns
        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.Columns.Add("IP Address", 100);
            listView1.Columns.Add("Hostname", 180);
            listView1.Columns.Add("Probable O/S", 80);
            listView1.Columns.Add("Response Time", 50);
            listView1.Columns.Add("Shared Folders", 50);
            listView1.Columns.Add("Open SMB Ports", 150);

            // Add context menu strip
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

            // New context menu item to list shared items
            ToolStripMenuItem listSharedItemsMenuItem = new ToolStripMenuItem("List Shared Items");
            listSharedItemsMenuItem.Click += ListSharedItemsItem_Click;
            contextMenu.Items.Add(listSharedItemsMenuItem);

            // Attach context menu strip to ListView
            listView1.ContextMenuStrip = contextMenu;
        }

        // Event handler for listing shared items from the selected IP
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
                }
                else
                {
                    MessageBox.Show("No shared folders found or access denied.", "Access Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Get a list of shared folders using net view command
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

        // Helper method to recursively copy directories and files
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

        // Method to get all local IP addresses and their descriptions (LAN, Wi-Fi, etc.)
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

        // New method to populate the ComboBox with local IP addresses
        private void PopulateIPComboBox()
        {
            locaddsCmb.Items.Clear();

            // Get local IP addresses
            var localIPs = GetAllLocalIPAddresses();

            foreach (var ip in localIPs)
            {
                locaddsCmb.Items.Add($"{ip.Item1} - {ip.Item2}");
            }

            // Optionally, select the first item in the ComboBox
            if (locaddsCmb.Items.Count > 0)
            {
                locaddsCmb.SelectedIndex = 0;
            }
        }

        // Event handler for GetIP button click
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

        // Event handler for ScanNW button click
        private async void ScanNW_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(subnet))
            {
                List<string[]> activeIPs = await ScanNetworkAsync(subnet);
                PopulateListView(activeIPs);
            }
            else
            {
                MessageBox.Show("Please get the local IP address first.");
            }
        }

        // Method to scan the network asynchronously
        private async Task<List<string[]>> ScanNetworkAsync(string subnet)
        {
            var tasks = new List<Task<string[]>>();
            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}.{i}";
                tasks.Add(PingAndGetHostNameAsync(ip));
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

            return activeIPs;
        }

        // Method to ping an IP address asynchronously and get its hostname and OS information
        private async Task<string[]> PingAndGetHostNameAsync(string ip)
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
                        return new string[] { ip, hostName, pingTime, osInfo };
                    }
                }
                catch (PingException)
                {
                    // Ignore PingExceptions
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error pinging IP {ip}: {ex.Message}");
                }
            }

            return null;
        }

        // Method to get the hostname of an IP address asynchronously
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

        // Method to determine the probable OS based on TTL value
        private string GetOperatingSystem(int ttl)
        {
            // Simple logic based on TTL to guess the OS
            if (ttl <= 64)
                return "Windows";
            else if (ttl <= 128)
                return "Linux";
            else
                return "Other";
        }

        // Populate the ListView with active IP addresses
        private void PopulateListView(List<string[]> activeIPs)
        {
            listView1.Items.Clear();

            foreach (var ipInfo in activeIPs)
            {
                ListViewItem item = new ListViewItem(ipInfo);
                listView1.Items.Add(item);
            }
        }

        // Event handler for Open Root Shared Folder context menu item
        private void OpenRootFolderItem_Click(object sender, EventArgs e)
        {
            // Add code to open the root shared folder of the selected IP
        }

        // Event handler for Download Shared Folders context menu item
        private void DownloadSharedFoldersItem_Click(object sender, EventArgs e)
        {
            // Add code to download shared folders of the selected IP
        }

        // Event handler for Scan Ports context menu item
        private void ScanPortsMenuItem_Click(object sender, EventArgs e)
        {
            // Add code to scan ports for the selected IP
        }
    }
}
