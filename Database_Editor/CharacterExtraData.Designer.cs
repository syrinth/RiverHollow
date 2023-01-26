namespace Database_Editor
{
    partial class FormCharExtraData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCharExtraData));
            this.dgvCharExtraData = new System.Windows.Forms.DataGridView();
            this.colCharExtraID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCharExtraName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tbCharExtraDataInfo = new System.Windows.Forms.TextBox();
            this.tbCharExtraDataName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvExtraTags = new System.Windows.Forms.DataGridView();
            this.colTaskTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCharExtraData)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExtraTags)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCharExtraData
            // 
            this.dgvCharExtraData.AllowUserToAddRows = false;
            this.dgvCharExtraData.AllowUserToDeleteRows = false;
            this.dgvCharExtraData.AllowUserToResizeColumns = false;
            this.dgvCharExtraData.AllowUserToResizeRows = false;
            this.dgvCharExtraData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCharExtraData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCharExtraID,
            this.colCharExtraName});
            this.dgvCharExtraData.ContextMenuStrip = this.contextMenuStrip;
            this.dgvCharExtraData.Location = new System.Drawing.Point(12, 12);
            this.dgvCharExtraData.MultiSelect = false;
            this.dgvCharExtraData.Name = "dgvCharExtraData";
            this.dgvCharExtraData.ReadOnly = true;
            this.dgvCharExtraData.RowHeadersVisible = false;
            this.dgvCharExtraData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCharExtraData.Size = new System.Drawing.Size(308, 426);
            this.dgvCharExtraData.TabIndex = 15;
            this.dgvCharExtraData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCharExtraData_CellClick);
            // 
            // colCharExtraID
            // 
            this.colCharExtraID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCharExtraID.FillWeight = 10F;
            this.colCharExtraID.HeaderText = "ID";
            this.colCharExtraID.Name = "colCharExtraID";
            this.colCharExtraID.ReadOnly = true;
            this.colCharExtraID.Width = 31;
            // 
            // colCharExtraName
            // 
            this.colCharExtraName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCharExtraName.FillWeight = 90F;
            this.colCharExtraName.HeaderText = "Name";
            this.colCharExtraName.Name = "colCharExtraName";
            this.colCharExtraName.ReadOnly = true;
            this.colCharExtraName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(124, 26);
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.addNewToolStripMenuItem.Text = "Add New";
            this.addNewToolStripMenuItem.Click += new System.EventHandler(this.addNewToolStripMenuItem_Click);
            // 
            // tbCharExtraDataInfo
            // 
            this.tbCharExtraDataInfo.Location = new System.Drawing.Point(329, 51);
            this.tbCharExtraDataInfo.Multiline = true;
            this.tbCharExtraDataInfo.Name = "tbCharExtraDataInfo";
            this.tbCharExtraDataInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbCharExtraDataInfo.Size = new System.Drawing.Size(459, 117);
            this.tbCharExtraDataInfo.TabIndex = 21;
            // 
            // tbCharExtraDataName
            // 
            this.tbCharExtraDataName.Location = new System.Drawing.Point(370, 12);
            this.tbCharExtraDataName.Name = "tbCharExtraDataName";
            this.tbCharExtraDataName.Size = new System.Drawing.Size(108, 20);
            this.tbCharExtraDataName.TabIndex = 23;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(326, 15);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 22;
            this.lblName.Text = "Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(326, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Info:";
            // 
            // dgvExtraTags
            // 
            this.dgvExtraTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvExtraTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTaskTags});
            this.dgvExtraTags.Location = new System.Drawing.Point(329, 174);
            this.dgvExtraTags.Name = "dgvExtraTags";
            this.dgvExtraTags.RowHeadersVisible = false;
            this.dgvExtraTags.Size = new System.Drawing.Size(459, 206);
            this.dgvExtraTags.TabIndex = 46;
            // 
            // colTaskTags
            // 
            this.colTaskTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTaskTags.HeaderText = "Tags";
            this.colTaskTags.Name = "colTaskTags";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(713, 386);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 47;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // FormCharExtraData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.dgvExtraTags);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbCharExtraDataName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.tbCharExtraDataInfo);
            this.Controls.Add(this.dgvCharExtraData);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCharExtraData";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCharExtraData_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCharExtraData)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvExtraTags)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvCharExtraData;
        private System.Windows.Forms.TextBox tbCharExtraDataInfo;
        private System.Windows.Forms.TextBox tbCharExtraDataName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvExtraTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTaskTags;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharExtraID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharExtraName;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
    }
}