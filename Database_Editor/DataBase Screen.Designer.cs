namespace Database_Editor
{
    partial class frmDBEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDBEditor));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabCtl = new System.Windows.Forms.TabControl();
            this.tabItems = new System.Windows.Forms.TabPage();
            this.cbItemSubtype = new System.Windows.Forms.ComboBox();
            this.cbItemType = new System.Windows.Forms.ComboBox();
            this.btnItemCancel = new System.Windows.Forms.Button();
            this.btnItemSave = new System.Windows.Forms.Button();
            this.dgItemTags = new System.Windows.Forms.DataGridView();
            this.colItemTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbItemDesc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbItemID = new System.Windows.Forms.TextBox();
            this.tbItemName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.dgItems = new System.Windows.Forms.DataGridView();
            this.tabWorldObjects = new System.Windows.Forms.TabPage();
            this.cbWorldObjectType = new System.Windows.Forms.ComboBox();
            this.btnWorldObjectCancel = new System.Windows.Forms.Button();
            this.btnWorldObjectSave = new System.Windows.Forms.Button();
            this.dgWorldObjectTags = new System.Windows.Forms.DataGridView();
            this.colWorldObjectTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.tbWorldObjectID = new System.Windows.Forms.TextBox();
            this.tbWorldObjectName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dgWorldObjects = new System.Windows.Forms.DataGridView();
            this.tabCharacters = new System.Windows.Forms.TabPage();
            this.cbCharacterType = new System.Windows.Forms.ComboBox();
            this.btnCancelCharacter = new System.Windows.Forms.Button();
            this.btnSaveCharacter = new System.Windows.Forms.Button();
            this.dgCharacterTags = new System.Windows.Forms.DataGridView();
            this.colCharacterTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.tbCharacterID = new System.Windows.Forms.TextBox();
            this.tbCharacterName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dgCharacters = new System.Windows.Forms.DataGridView();
            this.tabClasses = new System.Windows.Forms.TabPage();
            this.btnClassCancel = new System.Windows.Forms.Button();
            this.btnClassSave = new System.Windows.Forms.Button();
            this.dgClassTags = new System.Windows.Forms.DataGridView();
            this.colClassTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label7 = new System.Windows.Forms.Label();
            this.tbClassID = new System.Windows.Forms.TextBox();
            this.tbClassName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.dgClasses = new System.Windows.Forms.DataGridView();
            this.colClassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colClassName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCharacterID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCharacterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorldObjectsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorldObjectsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1.SuspendLayout();
            this.tabCtl.SuspendLayout();
            this.tabItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgItemTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgItems)).BeginInit();
            this.tabWorldObjects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgWorldObjectTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgWorldObjects)).BeginInit();
            this.tabCharacters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCharacterTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgCharacters)).BeginInit();
            this.tabClasses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgClassTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgClasses)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(816, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveToFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemToolStripMenuItem,
            this.worldObjectToolStripMenuItem});
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.addNewToolStripMenuItem.Text = "Add New";
            // 
            // itemToolStripMenuItem
            // 
            this.itemToolStripMenuItem.Name = "itemToolStripMenuItem";
            this.itemToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.itemToolStripMenuItem.Text = "Item";
            this.itemToolStripMenuItem.Click += new System.EventHandler(this.addNewToolStripMenuItem_Click);
            // 
            // worldObjectToolStripMenuItem
            // 
            this.worldObjectToolStripMenuItem.Name = "worldObjectToolStripMenuItem";
            this.worldObjectToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.worldObjectToolStripMenuItem.Text = "World Object";
            this.worldObjectToolStripMenuItem.Click += new System.EventHandler(this.addNewToolStripMenuWorldObject_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(133, 6);
            // 
            // saveToFileToolStripMenuItem
            // 
            this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
            this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.saveToFileToolStripMenuItem.Text = "Save To File";
            this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
            // 
            // tabCtl
            // 
            this.tabCtl.Controls.Add(this.tabItems);
            this.tabCtl.Controls.Add(this.tabWorldObjects);
            this.tabCtl.Controls.Add(this.tabCharacters);
            this.tabCtl.Controls.Add(this.tabClasses);
            this.tabCtl.Location = new System.Drawing.Point(12, 27);
            this.tabCtl.Name = "tabCtl";
            this.tabCtl.SelectedIndex = 0;
            this.tabCtl.Size = new System.Drawing.Size(798, 451);
            this.tabCtl.TabIndex = 12;
            this.tabCtl.SelectedIndexChanged += new System.EventHandler(this.tabCtl_SelectedIndexChanged);
            // 
            // tabItems
            // 
            this.tabItems.Controls.Add(this.cbItemSubtype);
            this.tabItems.Controls.Add(this.cbItemType);
            this.tabItems.Controls.Add(this.btnItemCancel);
            this.tabItems.Controls.Add(this.btnItemSave);
            this.tabItems.Controls.Add(this.dgItemTags);
            this.tabItems.Controls.Add(this.tbItemDesc);
            this.tabItems.Controls.Add(this.label1);
            this.tabItems.Controls.Add(this.label2);
            this.tabItems.Controls.Add(this.tbItemID);
            this.tabItems.Controls.Add(this.tbItemName);
            this.tabItems.Controls.Add(this.lblName);
            this.tabItems.Controls.Add(this.dgItems);
            this.tabItems.Location = new System.Drawing.Point(4, 22);
            this.tabItems.Name = "tabItems";
            this.tabItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabItems.Size = new System.Drawing.Size(790, 425);
            this.tabItems.TabIndex = 0;
            this.tabItems.Text = "Items";
            this.tabItems.UseVisualStyleBackColor = true;
            // 
            // cbItemSubtype
            // 
            this.cbItemSubtype.FormattingEnabled = true;
            this.cbItemSubtype.Location = new System.Drawing.Point(475, 110);
            this.cbItemSubtype.Name = "cbItemSubtype";
            this.cbItemSubtype.Size = new System.Drawing.Size(149, 21);
            this.cbItemSubtype.TabIndex = 25;
            this.cbItemSubtype.Visible = false;
            // 
            // cbItemType
            // 
            this.cbItemType.FormattingEnabled = true;
            this.cbItemType.Location = new System.Drawing.Point(320, 110);
            this.cbItemType.Name = "cbItemType";
            this.cbItemType.Size = new System.Drawing.Size(149, 21);
            this.cbItemType.TabIndex = 24;
            this.cbItemType.SelectedIndexChanged += new System.EventHandler(this.cbItemType_SelectedIndexChanged);
            // 
            // btnItemCancel
            // 
            this.btnItemCancel.Location = new System.Drawing.Point(709, 394);
            this.btnItemCancel.Name = "btnItemCancel";
            this.btnItemCancel.Size = new System.Drawing.Size(75, 23);
            this.btnItemCancel.TabIndex = 23;
            this.btnItemCancel.Text = "Cancel";
            this.btnItemCancel.UseVisualStyleBackColor = true;
            this.btnItemCancel.Click += new System.EventHandler(this.btnItemCancel_Click);
            // 
            // btnItemSave
            // 
            this.btnItemSave.Location = new System.Drawing.Point(628, 394);
            this.btnItemSave.Name = "btnItemSave";
            this.btnItemSave.Size = new System.Drawing.Size(75, 23);
            this.btnItemSave.TabIndex = 22;
            this.btnItemSave.Text = "Save";
            this.btnItemSave.UseVisualStyleBackColor = true;
            this.btnItemSave.Click += new System.EventHandler(this.btnItemSave_Click);
            // 
            // dgItemTags
            // 
            this.dgItemTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgItemTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colItemTags});
            this.dgItemTags.Location = new System.Drawing.Point(320, 137);
            this.dgItemTags.Name = "dgItemTags";
            this.dgItemTags.RowHeadersVisible = false;
            this.dgItemTags.Size = new System.Drawing.Size(464, 251);
            this.dgItemTags.TabIndex = 21;
            // 
            // colItemTags
            // 
            this.colItemTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colItemTags.HeaderText = "Tags";
            this.colItemTags.Name = "colItemTags";
            // 
            // tbItemDesc
            // 
            this.tbItemDesc.Location = new System.Drawing.Point(320, 51);
            this.tbItemDesc.Multiline = true;
            this.tbItemDesc.Name = "tbItemDesc";
            this.tbItemDesc.Size = new System.Drawing.Size(464, 53);
            this.tbItemDesc.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(317, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Description:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(714, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "ID:";
            // 
            // tbItemID
            // 
            this.tbItemID.Location = new System.Drawing.Point(741, 6);
            this.tbItemID.Name = "tbItemID";
            this.tbItemID.Size = new System.Drawing.Size(43, 20);
            this.tbItemID.TabIndex = 17;
            // 
            // tbItemName
            // 
            this.tbItemName.Location = new System.Drawing.Point(361, 6);
            this.tbItemName.Name = "tbItemName";
            this.tbItemName.Size = new System.Drawing.Size(108, 20);
            this.tbItemName.TabIndex = 16;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(317, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 15;
            this.lblName.Text = "Name:";
            // 
            // dgItems
            // 
            this.dgItems.AllowUserToAddRows = false;
            this.dgItems.AllowUserToDeleteRows = false;
            this.dgItems.AllowUserToResizeColumns = false;
            this.dgItems.AllowUserToResizeRows = false;
            this.dgItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colItemID,
            this.colItemName});
            this.dgItems.Location = new System.Drawing.Point(6, 6);
            this.dgItems.MultiSelect = false;
            this.dgItems.Name = "dgItems";
            this.dgItems.ReadOnly = true;
            this.dgItems.RowHeadersVisible = false;
            this.dgItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgItems.Size = new System.Drawing.Size(308, 411);
            this.dgItems.TabIndex = 14;
            this.dgItems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgItems_CellClick);
            // 
            // tabWorldObjects
            // 
            this.tabWorldObjects.Controls.Add(this.cbWorldObjectType);
            this.tabWorldObjects.Controls.Add(this.btnWorldObjectCancel);
            this.tabWorldObjects.Controls.Add(this.btnWorldObjectSave);
            this.tabWorldObjects.Controls.Add(this.dgWorldObjectTags);
            this.tabWorldObjects.Controls.Add(this.label4);
            this.tabWorldObjects.Controls.Add(this.tbWorldObjectID);
            this.tabWorldObjects.Controls.Add(this.tbWorldObjectName);
            this.tabWorldObjects.Controls.Add(this.label5);
            this.tabWorldObjects.Controls.Add(this.dgWorldObjects);
            this.tabWorldObjects.Location = new System.Drawing.Point(4, 22);
            this.tabWorldObjects.Name = "tabWorldObjects";
            this.tabWorldObjects.Padding = new System.Windows.Forms.Padding(3);
            this.tabWorldObjects.Size = new System.Drawing.Size(790, 425);
            this.tabWorldObjects.TabIndex = 1;
            this.tabWorldObjects.Text = "WorldObj";
            this.tabWorldObjects.UseVisualStyleBackColor = true;
            // 
            // cbWorldObjectType
            // 
            this.cbWorldObjectType.FormattingEnabled = true;
            this.cbWorldObjectType.Location = new System.Drawing.Point(320, 33);
            this.cbWorldObjectType.Name = "cbWorldObjectType";
            this.cbWorldObjectType.Size = new System.Drawing.Size(149, 21);
            this.cbWorldObjectType.TabIndex = 36;
            // 
            // btnWorldObjectCancel
            // 
            this.btnWorldObjectCancel.Location = new System.Drawing.Point(709, 394);
            this.btnWorldObjectCancel.Name = "btnWorldObjectCancel";
            this.btnWorldObjectCancel.Size = new System.Drawing.Size(75, 23);
            this.btnWorldObjectCancel.TabIndex = 35;
            this.btnWorldObjectCancel.Text = "Cancel";
            this.btnWorldObjectCancel.UseVisualStyleBackColor = true;
            this.btnWorldObjectCancel.Click += new System.EventHandler(this.btnWorldObjectCancel_Click);
            // 
            // btnWorldObjectSave
            // 
            this.btnWorldObjectSave.Location = new System.Drawing.Point(628, 394);
            this.btnWorldObjectSave.Name = "btnWorldObjectSave";
            this.btnWorldObjectSave.Size = new System.Drawing.Size(75, 23);
            this.btnWorldObjectSave.TabIndex = 34;
            this.btnWorldObjectSave.Text = "Save";
            this.btnWorldObjectSave.UseVisualStyleBackColor = true;
            this.btnWorldObjectSave.Click += new System.EventHandler(this.btnWorldObjectSave_Click);
            // 
            // dgWorldObjectTags
            // 
            this.dgWorldObjectTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgWorldObjectTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorldObjectTags});
            this.dgWorldObjectTags.Location = new System.Drawing.Point(320, 60);
            this.dgWorldObjectTags.Name = "dgWorldObjectTags";
            this.dgWorldObjectTags.RowHeadersVisible = false;
            this.dgWorldObjectTags.Size = new System.Drawing.Size(464, 328);
            this.dgWorldObjectTags.TabIndex = 33;
            // 
            // colWorldObjectTags
            // 
            this.colWorldObjectTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colWorldObjectTags.HeaderText = "Tags";
            this.colWorldObjectTags.Name = "colWorldObjectTags";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(714, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "ID:";
            // 
            // tbWorldObjectID
            // 
            this.tbWorldObjectID.Location = new System.Drawing.Point(741, 6);
            this.tbWorldObjectID.Name = "tbWorldObjectID";
            this.tbWorldObjectID.Size = new System.Drawing.Size(43, 20);
            this.tbWorldObjectID.TabIndex = 29;
            // 
            // tbWorldObjectName
            // 
            this.tbWorldObjectName.Location = new System.Drawing.Point(361, 6);
            this.tbWorldObjectName.Name = "tbWorldObjectName";
            this.tbWorldObjectName.Size = new System.Drawing.Size(108, 20);
            this.tbWorldObjectName.TabIndex = 28;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(317, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Name:";
            // 
            // dgWorldObjects
            // 
            this.dgWorldObjects.AllowUserToAddRows = false;
            this.dgWorldObjects.AllowUserToDeleteRows = false;
            this.dgWorldObjects.AllowUserToResizeColumns = false;
            this.dgWorldObjects.AllowUserToResizeRows = false;
            this.dgWorldObjects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgWorldObjects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorldObjectsID,
            this.colWorldObjectsName});
            this.dgWorldObjects.Location = new System.Drawing.Point(6, 6);
            this.dgWorldObjects.MultiSelect = false;
            this.dgWorldObjects.Name = "dgWorldObjects";
            this.dgWorldObjects.ReadOnly = true;
            this.dgWorldObjects.RowHeadersVisible = false;
            this.dgWorldObjects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgWorldObjects.Size = new System.Drawing.Size(308, 411);
            this.dgWorldObjects.TabIndex = 26;
            this.dgWorldObjects.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgWorldObjects_CellClick);
            // 
            // tabCharacters
            // 
            this.tabCharacters.Controls.Add(this.cbCharacterType);
            this.tabCharacters.Controls.Add(this.btnCancelCharacter);
            this.tabCharacters.Controls.Add(this.btnSaveCharacter);
            this.tabCharacters.Controls.Add(this.dgCharacterTags);
            this.tabCharacters.Controls.Add(this.label3);
            this.tabCharacters.Controls.Add(this.tbCharacterID);
            this.tabCharacters.Controls.Add(this.tbCharacterName);
            this.tabCharacters.Controls.Add(this.label6);
            this.tabCharacters.Controls.Add(this.dgCharacters);
            this.tabCharacters.Location = new System.Drawing.Point(4, 22);
            this.tabCharacters.Name = "tabCharacters";
            this.tabCharacters.Size = new System.Drawing.Size(790, 425);
            this.tabCharacters.TabIndex = 2;
            this.tabCharacters.Text = "Characters";
            this.tabCharacters.UseVisualStyleBackColor = true;
            // 
            // cbCharacterType
            // 
            this.cbCharacterType.FormattingEnabled = true;
            this.cbCharacterType.Location = new System.Drawing.Point(320, 33);
            this.cbCharacterType.Name = "cbCharacterType";
            this.cbCharacterType.Size = new System.Drawing.Size(149, 21);
            this.cbCharacterType.TabIndex = 45;
            // 
            // btnCancelCharacter
            // 
            this.btnCancelCharacter.Location = new System.Drawing.Point(709, 394);
            this.btnCancelCharacter.Name = "btnCancelCharacter";
            this.btnCancelCharacter.Size = new System.Drawing.Size(75, 23);
            this.btnCancelCharacter.TabIndex = 44;
            this.btnCancelCharacter.Text = "Cancel";
            this.btnCancelCharacter.UseVisualStyleBackColor = true;
            this.btnCancelCharacter.Click += new System.EventHandler(this.btnCharacterCancel_Click);
            // 
            // btnSaveCharacter
            // 
            this.btnSaveCharacter.Location = new System.Drawing.Point(628, 394);
            this.btnSaveCharacter.Name = "btnSaveCharacter";
            this.btnSaveCharacter.Size = new System.Drawing.Size(75, 23);
            this.btnSaveCharacter.TabIndex = 43;
            this.btnSaveCharacter.Text = "Save";
            this.btnSaveCharacter.UseVisualStyleBackColor = true;
            this.btnSaveCharacter.Click += new System.EventHandler(this.btnCharacterSave_Click);
            // 
            // dgCharacterTags
            // 
            this.dgCharacterTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgCharacterTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCharacterTags});
            this.dgCharacterTags.Location = new System.Drawing.Point(320, 60);
            this.dgCharacterTags.Name = "dgCharacterTags";
            this.dgCharacterTags.RowHeadersVisible = false;
            this.dgCharacterTags.Size = new System.Drawing.Size(464, 328);
            this.dgCharacterTags.TabIndex = 42;
            // 
            // colCharacterTags
            // 
            this.colCharacterTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCharacterTags.HeaderText = "Tags";
            this.colCharacterTags.Name = "colCharacterTags";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(714, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 13);
            this.label3.TabIndex = 41;
            this.label3.Text = "ID:";
            // 
            // tbCharacterID
            // 
            this.tbCharacterID.Location = new System.Drawing.Point(741, 6);
            this.tbCharacterID.Name = "tbCharacterID";
            this.tbCharacterID.Size = new System.Drawing.Size(43, 20);
            this.tbCharacterID.TabIndex = 40;
            // 
            // tbCharacterName
            // 
            this.tbCharacterName.Location = new System.Drawing.Point(361, 6);
            this.tbCharacterName.Name = "tbCharacterName";
            this.tbCharacterName.Size = new System.Drawing.Size(108, 20);
            this.tbCharacterName.TabIndex = 39;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(317, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 38;
            this.label6.Text = "Name:";
            // 
            // dgCharacters
            // 
            this.dgCharacters.AllowUserToAddRows = false;
            this.dgCharacters.AllowUserToDeleteRows = false;
            this.dgCharacters.AllowUserToResizeColumns = false;
            this.dgCharacters.AllowUserToResizeRows = false;
            this.dgCharacters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgCharacters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCharacterID,
            this.colCharacterName});
            this.dgCharacters.Location = new System.Drawing.Point(6, 6);
            this.dgCharacters.MultiSelect = false;
            this.dgCharacters.Name = "dgCharacters";
            this.dgCharacters.ReadOnly = true;
            this.dgCharacters.RowHeadersVisible = false;
            this.dgCharacters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgCharacters.Size = new System.Drawing.Size(308, 411);
            this.dgCharacters.TabIndex = 37;
            this.dgCharacters.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgCharacters_CellClick);
            // 
            // tabClasses
            // 
            this.tabClasses.Controls.Add(this.btnClassCancel);
            this.tabClasses.Controls.Add(this.btnClassSave);
            this.tabClasses.Controls.Add(this.dgClassTags);
            this.tabClasses.Controls.Add(this.label7);
            this.tabClasses.Controls.Add(this.tbClassID);
            this.tabClasses.Controls.Add(this.tbClassName);
            this.tabClasses.Controls.Add(this.label8);
            this.tabClasses.Controls.Add(this.dgClasses);
            this.tabClasses.Location = new System.Drawing.Point(4, 22);
            this.tabClasses.Name = "tabClasses";
            this.tabClasses.Size = new System.Drawing.Size(790, 425);
            this.tabClasses.TabIndex = 3;
            this.tabClasses.Text = "Classes";
            this.tabClasses.UseVisualStyleBackColor = true;
            // 
            // btnClassCancel
            // 
            this.btnClassCancel.Location = new System.Drawing.Point(709, 394);
            this.btnClassCancel.Name = "btnClassCancel";
            this.btnClassCancel.Size = new System.Drawing.Size(75, 23);
            this.btnClassCancel.TabIndex = 53;
            this.btnClassCancel.Text = "Cancel";
            this.btnClassCancel.UseVisualStyleBackColor = true;
            this.btnClassCancel.Click += new System.EventHandler(this.btnClassCancel_Click);
            // 
            // btnClassSave
            // 
            this.btnClassSave.Location = new System.Drawing.Point(628, 394);
            this.btnClassSave.Name = "btnClassSave";
            this.btnClassSave.Size = new System.Drawing.Size(75, 23);
            this.btnClassSave.TabIndex = 52;
            this.btnClassSave.Text = "Save";
            this.btnClassSave.UseVisualStyleBackColor = true;
            this.btnClassSave.Click += new System.EventHandler(this.btnClassSave_Click);
            // 
            // dgClassTags
            // 
            this.dgClassTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgClassTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colClassTags});
            this.dgClassTags.Location = new System.Drawing.Point(320, 32);
            this.dgClassTags.Name = "dgClassTags";
            this.dgClassTags.RowHeadersVisible = false;
            this.dgClassTags.Size = new System.Drawing.Size(464, 356);
            this.dgClassTags.TabIndex = 51;
            // 
            // colClassTags
            // 
            this.colClassTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colClassTags.HeaderText = "Tags";
            this.colClassTags.Name = "colClassTags";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(714, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 13);
            this.label7.TabIndex = 50;
            this.label7.Text = "ID:";
            // 
            // tbClassID
            // 
            this.tbClassID.Location = new System.Drawing.Point(741, 6);
            this.tbClassID.Name = "tbClassID";
            this.tbClassID.Size = new System.Drawing.Size(43, 20);
            this.tbClassID.TabIndex = 49;
            // 
            // tbClassName
            // 
            this.tbClassName.Location = new System.Drawing.Point(361, 6);
            this.tbClassName.Name = "tbClassName";
            this.tbClassName.Size = new System.Drawing.Size(108, 20);
            this.tbClassName.TabIndex = 48;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(317, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 47;
            this.label8.Text = "Name:";
            // 
            // dgClasses
            // 
            this.dgClasses.AllowUserToAddRows = false;
            this.dgClasses.AllowUserToDeleteRows = false;
            this.dgClasses.AllowUserToResizeColumns = false;
            this.dgClasses.AllowUserToResizeRows = false;
            this.dgClasses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgClasses.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colClassID,
            this.colClassName});
            this.dgClasses.Location = new System.Drawing.Point(6, 6);
            this.dgClasses.MultiSelect = false;
            this.dgClasses.Name = "dgClasses";
            this.dgClasses.ReadOnly = true;
            this.dgClasses.RowHeadersVisible = false;
            this.dgClasses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgClasses.Size = new System.Drawing.Size(308, 411);
            this.dgClasses.TabIndex = 46;
            this.dgClasses.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgClasses_CellClick);
            // 
            // colClassID
            // 
            this.colClassID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colClassID.FillWeight = 10F;
            this.colClassID.HeaderText = "ID";
            this.colClassID.Name = "colClassID";
            this.colClassID.ReadOnly = true;
            // 
            // colClassName
            // 
            this.colClassName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colClassName.FillWeight = 90F;
            this.colClassName.HeaderText = "Name";
            this.colClassName.Name = "colClassName";
            this.colClassName.ReadOnly = true;
            // 
            // colCharacterID
            // 
            this.colCharacterID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCharacterID.FillWeight = 10F;
            this.colCharacterID.HeaderText = "ID";
            this.colCharacterID.Name = "colCharacterID";
            this.colCharacterID.ReadOnly = true;
            // 
            // colCharacterName
            // 
            this.colCharacterName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCharacterName.FillWeight = 90F;
            this.colCharacterName.HeaderText = "Name";
            this.colCharacterName.Name = "colCharacterName";
            this.colCharacterName.ReadOnly = true;
            // 
            // colWorldObjectsID
            // 
            this.colWorldObjectsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colWorldObjectsID.FillWeight = 10F;
            this.colWorldObjectsID.HeaderText = "ID";
            this.colWorldObjectsID.Name = "colWorldObjectsID";
            this.colWorldObjectsID.ReadOnly = true;
            // 
            // colWorldObjectsName
            // 
            this.colWorldObjectsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colWorldObjectsName.FillWeight = 90F;
            this.colWorldObjectsName.HeaderText = "Name";
            this.colWorldObjectsName.Name = "colWorldObjectsName";
            this.colWorldObjectsName.ReadOnly = true;
            // 
            // colItemID
            // 
            this.colItemID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colItemID.FillWeight = 10F;
            this.colItemID.HeaderText = "ID";
            this.colItemID.Name = "colItemID";
            this.colItemID.ReadOnly = true;
            // 
            // colItemName
            // 
            this.colItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colItemName.FillWeight = 90F;
            this.colItemName.HeaderText = "Name";
            this.colItemName.Name = "colItemName";
            this.colItemName.ReadOnly = true;
            // 
            // frmDBEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 489);
            this.Controls.Add(this.tabCtl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmDBEditor";
            this.Text = "Database Editor";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabCtl.ResumeLayout(false);
            this.tabItems.ResumeLayout(false);
            this.tabItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgItemTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgItems)).EndInit();
            this.tabWorldObjects.ResumeLayout(false);
            this.tabWorldObjects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgWorldObjectTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgWorldObjects)).EndInit();
            this.tabCharacters.ResumeLayout(false);
            this.tabCharacters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCharacterTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgCharacters)).EndInit();
            this.tabClasses.ResumeLayout(false);
            this.tabClasses.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgClassTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgClasses)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TabControl tabCtl;
        private System.Windows.Forms.TabPage tabItems;
        private System.Windows.Forms.ComboBox cbItemSubtype;
        private System.Windows.Forms.ComboBox cbItemType;
        private System.Windows.Forms.Button btnItemCancel;
        private System.Windows.Forms.Button btnItemSave;
        private System.Windows.Forms.DataGridView dgItemTags;
        private System.Windows.Forms.TextBox tbItemDesc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbItemID;
        private System.Windows.Forms.TextBox tbItemName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.DataGridView dgItems;
        private System.Windows.Forms.TabPage tabWorldObjects;
        private System.Windows.Forms.ComboBox cbWorldObjectType;
        private System.Windows.Forms.Button btnWorldObjectCancel;
        private System.Windows.Forms.Button btnWorldObjectSave;
        private System.Windows.Forms.DataGridView dgWorldObjectTags;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbWorldObjectID;
        private System.Windows.Forms.TextBox tbWorldObjectName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgWorldObjects;
        private System.Windows.Forms.ToolStripMenuItem itemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem worldObjectToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemTags;
        private System.Windows.Forms.TabPage tabCharacters;
        private System.Windows.Forms.Button btnCancelCharacter;
        private System.Windows.Forms.Button btnSaveCharacter;
        private System.Windows.Forms.DataGridView dgCharacterTags;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbCharacterID;
        private System.Windows.Forms.TextBox tbCharacterName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridView dgCharacters;
        private System.Windows.Forms.ComboBox cbCharacterType;
        private System.Windows.Forms.TabPage tabClasses;
        private System.Windows.Forms.Button btnClassCancel;
        private System.Windows.Forms.Button btnClassSave;
        private System.Windows.Forms.DataGridView dgClassTags;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbClassID;
        private System.Windows.Forms.TextBox tbClassName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgClasses;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharacterTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharacterID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharacterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassName;
    }
}

