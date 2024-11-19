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
            GetIP.Click += GetIP_Click;
            ScanNW.Click += ScanNW_Click;
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
            listView1.Columns.Add("Response Time", 50);
            listView1.Columns.Add("Probable O/S", 80);
            listView1.Columns.Add("Shared Folders", 50);
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
        private async Task<List<string[]>> ScanNetworkAsync(string subnet)
        {
            var tasks = new List<Task<string[]>>();

            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}.{i}";
                tasks.Add(PingAndGetHostNameAndPortsAsync(ip)); // Updated to include port scanning
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
            if (ttl <= 64) return "Windows";
            else if (ttl <= 128) return "Linux";
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
