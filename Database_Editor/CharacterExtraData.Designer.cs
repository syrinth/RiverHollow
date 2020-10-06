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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCharExtraData));
            this.dgvCharExtraData = new System.Windows.Forms.DataGridView();
            this.colCharExtraID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbCharExtraDataInfo = new System.Windows.Forms.TextBox();
            this.tbCharExtraDataName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddNew = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCharExtraData)).BeginInit();
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
            this.colCharExtraID});
            this.dgvCharExtraData.Location = new System.Drawing.Point(12, 12);
            this.dgvCharExtraData.MultiSelect = false;
            this.dgvCharExtraData.Name = "dgvCharExtraData";
            this.dgvCharExtraData.ReadOnly = true;
            this.dgvCharExtraData.RowHeadersVisible = false;
            this.dgvCharExtraData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCharExtraData.Size = new System.Drawing.Size(308, 411);
            this.dgvCharExtraData.TabIndex = 15;
            this.dgvCharExtraData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCharExtraData_CellClick);
            // 
            // colCharExtraID
            // 
            this.colCharExtraID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCharExtraID.FillWeight = 90F;
            this.colCharExtraID.HeaderText = "Name";
            this.colCharExtraID.Name = "colCharExtraID";
            this.colCharExtraID.ReadOnly = true;
            // 
            // tbCharExtraDataInfo
            // 
            this.tbCharExtraDataInfo.Location = new System.Drawing.Point(329, 51);
            this.tbCharExtraDataInfo.Multiline = true;
            this.tbCharExtraDataInfo.Name = "tbCharExtraDataInfo";
            this.tbCharExtraDataInfo.Size = new System.Drawing.Size(464, 189);
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
            // btnAddNew
            // 
            this.btnAddNew.Location = new System.Drawing.Point(326, 400);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(75, 23);
            this.btnAddNew.TabIndex = 45;
            this.btnAddNew.Text = "Add New";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btnAddNew_Click);
            // 
            // FormCharExtraData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnAddNew);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbCharExtraDataName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.tbCharExtraDataInfo);
            this.Controls.Add(this.dgvCharExtraData);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCharExtraData";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCharExtraData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvCharExtraData;
        private System.Windows.Forms.TextBox tbCharExtraDataInfo;
        private System.Windows.Forms.TextBox tbCharExtraDataName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharExtraID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddNew;
    }
}