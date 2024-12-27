namespace Database_Editor
{
    partial class FrmDBEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDBEditor));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortAndSaveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mailboxMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabCtl = new System.Windows.Forms.TabControl();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.textToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(816, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToFileToolStripMenuItem
            // 
            this.saveToFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortAndSaveToolStripMenuItem1});
            this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
            this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.saveToFileToolStripMenuItem.Text = "Save To File";
            this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
            // 
            // sortAndSaveToolStripMenuItem1
            // 
            this.sortAndSaveToolStripMenuItem1.Name = "sortAndSaveToolStripMenuItem1";
            this.sortAndSaveToolStripMenuItem1.Size = new System.Drawing.Size(145, 22);
            this.sortAndSaveToolStripMenuItem1.Text = "Sort and Save";
            this.sortAndSaveToolStripMenuItem1.Click += new System.EventHandler(this.sortAndSaveToolStripMenuItem1_Click);
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameTextToolStripMenuItem,
            this.mailboxMessagesToolStripMenuItem});
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.textToolStripMenuItem.Text = "Text";
            // 
            // gameTextToolStripMenuItem
            // 
            this.gameTextToolStripMenuItem.Name = "gameTextToolStripMenuItem";
            this.gameTextToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.gameTextToolStripMenuItem.Text = "Game Text";
            this.gameTextToolStripMenuItem.Click += new System.EventHandler(this.gameTextToolStripMenuItem_Click);
            // 
            // mailboxMessagesToolStripMenuItem
            // 
            this.mailboxMessagesToolStripMenuItem.Name = "mailboxMessagesToolStripMenuItem";
            this.mailboxMessagesToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.mailboxMessagesToolStripMenuItem.Text = "Mailbox Messages";
            this.mailboxMessagesToolStripMenuItem.Click += new System.EventHandler(this.mailboxMessagesToolStripMenuItem_Click);
            // 
            // tabCtl
            // 
            this.tabCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCtl.Location = new System.Drawing.Point(12, 27);
            this.tabCtl.Name = "tabCtl";
            this.tabCtl.SelectedIndex = 0;
            this.tabCtl.Size = new System.Drawing.Size(798, 451);
            this.tabCtl.TabIndex = 12;
            this.tabCtl.SelectedIndexChanged += new System.EventHandler(this.tabCtl_SelectedIndexChanged);
            // 
            // contextMenu
            // 
            this.contextMenu.Name = "contextMenuStripItems";
            this.contextMenu.Size = new System.Drawing.Size(61, 4);
            this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // tbStatus
            // 
            this.tbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStatus.Enabled = false;
            this.tbStatus.Location = new System.Drawing.Point(12, 484);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.Size = new System.Drawing.Size(794, 20);
            this.tbStatus.TabIndex = 13;
            // 
            // FrmDBEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 511);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.tabCtl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(832, 550);
            this.Name = "FrmDBEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDBEditor_FormClosing);
            this.Load += new System.EventHandler(this.FrmDBEditor_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
        private System.Windows.Forms.TabControl tabCtl;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mailboxMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortAndSaveToolStripMenuItem1;
        private System.Windows.Forms.TextBox tbStatus;
    }
}

