using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;


namespace NetCrawler
{
    public partial class AuditForm : Form
    {
        public AuditForm()
        {
            InitializeComponent();
        }

        // Method to load tree nodes into the TreeView
        public void LoadTreeView(List<TreeNode> folderNodes)
        {
            // Clear existing nodes
            auditTreeView.Nodes.Clear();

            // Add new nodes
            foreach (var node in folderNodes)
            {
                auditTreeView.Nodes.Add(node);
            }

            // Optionally, expand all nodes
            auditTreeView.ExpandAll();
        }
    }

}



