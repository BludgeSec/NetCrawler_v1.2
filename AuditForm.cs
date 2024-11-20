using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace NetCrawler
{
    public partial class AuditForm : Form
    {
        public AuditForm()
        {
            InitializeComponent();
            auditTreeView.BeforeExpand += auditTreeView_BeforeExpand; // Add the BeforeExpand event handler
        }

        // Method to load tree nodes into the TreeView
        public void LoadTreeView(List<TreeNode> allFolderNodes)
        {
            // Create a root TreeNode for each IP Address
            foreach (var folderNode in allFolderNodes)
            {
                TreeNode ipNode = new TreeNode(folderNode.Text);  // IP address node
                ipNode.Tag = folderNode.Tag;  // Tag to hold the IP address or any other info

                // Add the folder node as a child of the IP address node
                ipNode.Nodes.Add(folderNode);

                // Add the IP address node to the TreeView
                auditTreeView.Nodes.Add(ipNode);
            }

            // Optionally, you can trigger an event if needed
            auditTreeView.ExpandAll(); // Expand all nodes by default
        }


        private void auditTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // Check if the node has not been populated already
            if (e.Node.Nodes.Count == 0) // Only load subfolders if no nodes are present
            {
                string folderPath = e.Node.Tag.ToString(); // Get the full folder path from Tag
                var subItems = GetSubItems(folderPath);   // Retrieve subfolders/files for this folder path

                // Add subfolders/files as new TreeNodes under the current node
                foreach (var subItem in subItems)
                {
                    TreeNode subItemNode = new TreeNode(subItem);
                    subItemNode.Tag = subItem; // Store the full subfolder/file path
                    e.Node.Nodes.Add(subItemNode);  // Add to the current node
                }
            }
        }

        private List<string> GetSubItems(string folderPath)
        {
            List<string> subItems = new List<string>();

            try
            {
                // Get directories first
                var directories = Directory.GetDirectories(folderPath);
                foreach (var dir in directories)
                {
                    subItems.Add(dir); // Add the directory
                }

                // Then get files
                var files = Directory.GetFiles(folderPath);
                foreach (var file in files)
                {
                    subItems.Add(file); // Add the file
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access denied
                Console.WriteLine($"Access denied to {folderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sub-items for {folderPath}: {ex.Message}");
            }

            return subItems;
        }

    }

}



