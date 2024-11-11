using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

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

            // Attach context menu strip to ListView
            listView1.ContextMenuStrip = contextMenu;
        }

        // Create a ListViewItem with IP address, hostname, ping response time, and shared folders count
        private ListViewItem CreateListViewItem(string ip, string hostName, string osInfo, long pingTime, int sharedFoldersCount, int[] openPorts)
        {
            ListViewItem item = new ListViewItem(ip);
            item.Tag = ip; // Store IP address as tag for context menu
            item.SubItems.Add(hostName);
            item.SubItems.Add(osInfo);
            item.SubItems.Add($"{pingTime} ms"); // Format ping time as milliseconds
            item.SubItems.Add(sharedFoldersCount.ToString()); // Display shared folders count
            item.SubItems.Add(string.Join(", ", openPorts)); // Display open ports

            return item;
        }

        // Event handler for downloading shared folders
        private void DownloadSharedFoldersItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string ipAddress = listView1.SelectedItems[0].Text;
                DownloadSharedFolders(ipAddress);
            }
        }

        // Method to download shared folders
        private void DownloadSharedFolders(string ipAddress)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = $"Select a folder to save shared folders from {ipAddress}:";
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string localFolderPath = folderBrowserDialog.SelectedPath;

                try
                {
                    // Access shared folders using IP address
                    string sharedFolder = $@"\\{ipAddress}\";
                    DirectoryInfo sharedDirInfo = new DirectoryInfo(sharedFolder);

                    // Check if the shared directory exists and is accessible
                    if (!sharedDirInfo.Exists)
                    {
                        MessageBox.Show($"Shared folder {sharedFolder} not accessible.");
                        return;
                    }

                    // Prompt the user for a confirmation to download
                    DialogResult confirmationResult = MessageBox.Show($"Download all shared folders from {ipAddress} to {localFolderPath}?",
                                                                       "Confirmation",
                                                                       MessageBoxButtons.YesNo,
                                                                       MessageBoxIcon.Question);

                    if (confirmationResult == DialogResult.Yes)
                    {
                        // Recursively copy shared folders and files
                        CopyDirectory(sharedDirInfo, localFolderPath);

                        MessageBox.Show($"Shared folders from {ipAddress} successfully downloaded to {localFolderPath}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading shared folders: {ex.Message}");
                }
            }
        }

        private async void ScanPortsMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string ipAddress = listView1.SelectedItems[0].Text;
                await ScanAndPopulatePortsAsync(ipAddress);
            }
        }

        // Helper method to recursively copy directories and files
        private void CopyDirectory(DirectoryInfo sourceDir, string destPath)
        {
            // Create destination directory if it doesn't exist
            Directory.CreateDirectory(destPath);

            // Copy files
            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string filePath = Path.Combine(destPath, file.Name);
                file.CopyTo(filePath, true);
            }

            // Recursively copy subdirectories
            foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
            {
                string subDirPath = Path.Combine(destPath, subDir.Name);
                CopyDirectory(subDir, subDirPath);
            }
        }


        // Method to populate the ComboBox with local IP addresses
        private void PopulateIPComboBox()
        {
            locaddsCmb.Items.Clear(); // Clear any existing items
            List<Tuple<string, string>> ipAddresses = GetAllLocalIPAddresses();

            foreach (var ipAddress in ipAddresses)
            {
                locaddsCmb.Items.Add($"{ipAddress.Item1} - {ipAddress.Item2}");
            }

            if (locaddsCmb.Items.Count > 0)
            {
                locaddsCmb.SelectedIndex = 0; // Select the first item by default
            }
        }
        // Event handler for GetIP button click
        private void GetIP_Click(object sender, EventArgs e)
        {
            // Populate the ComboBox with local IP addresses when the button is clicked
            PopulateIPComboBox();

            if (locaddsCmb.SelectedItem != null)
            {
                // Get the selected IP from the ComboBox
                string selectedIP = locaddsCmb.SelectedItem.ToString().Split('-')[0].Trim();

                // Extract the subnet (first three octets) from the selected IP address
                subnet = string.Join(".", selectedIP.Split('.')[0], selectedIP.Split('.')[1], selectedIP.Split('.')[2]);

                // Optionally, show the subnet for debugging purposes
                Console.WriteLine("Subnet: " + subnet);
            }
            else
            {
                MessageBox.Show("Please select a valid IP address.");
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
                        string osInfo = GetOperatingSystem(reply.Options.Ttl); // Get OS information based on TTL
                        return new string[] { ip, hostName, pingTime, osInfo };
                    }
                }
                catch (PingException)
                {
                    // Ignore PingExceptions
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                }
            }
            return null;
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

        // Method to infer operating system based on TTL value
        private string GetOperatingSystem(int ttl)
        {
            if (ttl >= 0 && ttl <= 64)
            {
                return "Unix/Linux";
            }
            else if (ttl >= 65 && ttl <= 128)
            {
                return "Windows";
            }
            else if (ttl > 128)
            {
                return "other OS";
            }
            else
            {
                return "Unknown OS";
            }
        }

        // method to query shared folders on host
        private int GetSharedFoldersCount(string ipAddress)
        {
            int sharedFoldersCount = 0;

            try
            {
                // Form the network share path
                string sharedFolderPath = $@"\\{ipAddress}\";

                // Check if the directory is accessible
                if (Directory.Exists(sharedFolderPath))
                {
                    // Get the list of directories in the share
                    string[] directories = Directory.GetDirectories(sharedFolderPath);

                    // Count the directories (i.e., shared folders)
                    sharedFoldersCount = directories.Length;
                }
            }
            catch (Exception ex)
            {
                // Handle error (e.g., access issues)
                Console.WriteLine($"Error accessing shared folder at {ipAddress}: {ex.Message}");
            }

            return sharedFoldersCount;
        }


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

        // Method to get the hostname of an IP address asynchronously
        private async Task<string> GetHostNameAsync(string ip)
        {
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(ip);
                return hostEntry.HostName;
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        // Method to populate ListView with IP addresses, DNS information, ping response time, and OS detection
        private async Task ScanAndPopulatePortsAsync(string ipAddress)
        {
            try
            {
                // Clear existing open ports information for the selected item
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Text == ipAddress)
                    {
                        item.SubItems[5].Text = "Scanning..."; // Update "Open Ports" column status
                        break;
                    }
                }

                // Scan for open file sharing ports
                int[] openPorts = await ScanOpenFileSharingPortsAsync(ipAddress);

                // Find the item corresponding to the IP address and update open ports information
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Text == ipAddress)
                    {
                        item.SubItems[5].Text = string.Join(", ", openPorts); // Update "Open Ports" column
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning ports for {ipAddress}: {ex.Message}");
            }
        }



        // Method to populate ListView with IP addresses, DNS information, ping response time, and OS detection
        private async Task PopulateListView(List<string[]> ipInfoList)
        {
            foreach (var ipInfo in ipInfoList)
            {
                if (ipInfo != null && ipInfo.Length >= 4) // Check if IP info has sufficient elements
                {
                    string ip = ipInfo[0];
                    string hostName = ipInfo[1];
                    string pingTime = ipInfo[2];
                    string osInfo = ipInfo[3];

                    // Scan ports asynchronously
                    int[] openPorts = await ScanOpenFileSharingPortsAsync(ip);
                    int sharedFoldersCount = GetSharedFoldersCount(ip);

                    // Convert pingTime to long if possible
                    if (long.TryParse(pingTime, out long pingTimeValue))
                    {
                        // Check if an item with the same IP already exists in the ListView
                        ListViewItem existingItem = listView1.FindItemWithText(ip);
                        if (existingItem != null)
                        {
                            // Update existing item with latest information
                            existingItem.SubItems[1].Text = hostName;
                            existingItem.SubItems[2].Text = osInfo;
                            existingItem.SubItems[3].Text = $"{pingTimeValue} ms";
                            existingItem.SubItems[4].Text = sharedFoldersCount.ToString();
                            existingItem.SubItems[5].Text = string.Join(", ", openPorts);
                        }
                        else
                        {
                            // Create a new item and add it to the ListView
                            ListViewItem item = CreateListViewItem(ip, hostName, osInfo, pingTimeValue, 0, openPorts); // Pass openPorts here
                            listView1.Items.Add(item);
                        }
                    }
                    else
                    {
                        // Handle parsing error if needed
                        MessageBox.Show($"Error converting ping time '{pingTime}' to long.");
                    }
                }
            }
        }
    }
}
