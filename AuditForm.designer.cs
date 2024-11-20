namespace NetCrawler
{
    partial class AuditForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.auditTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(13, 13);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(259, 236);
            this.treeView1.TabIndex = 0;
            // 
            // auditTreeView
            // 
            this.auditTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.auditTreeView.Location = new System.Drawing.Point(-2, 0);
            this.auditTreeView.Name = "auditTreeView";
            this.auditTreeView.Size = new System.Drawing.Size(1306, 894);
            this.auditTreeView.TabIndex = 1;
            // 
            // AuditForm
            // 
            this.ClientSize = new System.Drawing.Size(1306, 896);
            this.Controls.Add(this.auditTreeView);
            this.Controls.Add(this.treeView1);
            this.Name = "AuditForm";
            this.Text = "Audit Form";
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TreeView auditTreeView;
    }
}
