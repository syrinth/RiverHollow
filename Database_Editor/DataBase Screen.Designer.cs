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
            this.questToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabCtl = new System.Windows.Forms.TabControl();
            this.tabItems = new System.Windows.Forms.TabPage();
            this.cbItemSubtype = new System.Windows.Forms.ComboBox();
            this.cbItemType = new System.Windows.Forms.ComboBox();
            this.btnItemCancel = new System.Windows.Forms.Button();
            this.dgItemTags = new System.Windows.Forms.DataGridView();
            this.colItemTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbItemDesc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbItemID = new System.Windows.Forms.TextBox();
            this.tbItemName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.dgvItems = new System.Windows.Forms.DataGridView();
            this.colItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabWorldObjects = new System.Windows.Forms.TabPage();
            this.cbWorldObjectType = new System.Windows.Forms.ComboBox();
            this.btnWorldObjectCancel = new System.Windows.Forms.Button();
            this.dgvWorldObjectTags = new System.Windows.Forms.DataGridView();
            this.colWorldObjectTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.tbWorldObjectID = new System.Windows.Forms.TextBox();
            this.tbWorldObjectName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvWorldObjects = new System.Windows.Forms.DataGridView();
            this.colWorldObjectsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorldObjectsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCharacters = new System.Windows.Forms.TabPage();
            this.cbEditableCharData = new System.Windows.Forms.ComboBox();
            this.btnEdit = new System.Windows.Forms.Button();
            this.cbCharacterType = new System.Windows.Forms.ComboBox();
            this.btnCancelCharacter = new System.Windows.Forms.Button();
            this.dgCharacterTags = new System.Windows.Forms.DataGridView();
            this.colCharacterTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.tbCharacterID = new System.Windows.Forms.TextBox();
            this.tbCharacterName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dgvCharacters = new System.Windows.Forms.DataGridView();
            this.colCharacterID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCharacterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabClasses = new System.Windows.Forms.TabPage();
            this.btnClassCancel = new System.Windows.Forms.Button();
            this.dgClassTags = new System.Windows.Forms.DataGridView();
            this.colClassTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label7 = new System.Windows.Forms.Label();
            this.tbClassID = new System.Windows.Forms.TextBox();
            this.tbClassName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvClasses = new System.Windows.Forms.DataGridView();
            this.colClassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colClassName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabAdventurers = new System.Windows.Forms.TabPage();
            this.cbAdventurerType = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.dgvAdventurerTags = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label9 = new System.Windows.Forms.Label();
            this.tbAdventurerID = new System.Windows.Forms.TextBox();
            this.tbAdventurerName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.dgvAdventurers = new System.Windows.Forms.DataGridView();
            this.colAdventurersID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAdventurersName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabQuests = new System.Windows.Forms.TabPage();
            this.cbQuestType = new System.Windows.Forms.ComboBox();
            this.btnQuestCancel = new System.Windows.Forms.Button();
            this.dgvQuestTags = new System.Windows.Forms.DataGridView();
            this.colQuestTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbQuestDescription = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tbQuestID = new System.Windows.Forms.TextBox();
            this.tbQuestName = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.dgvQuests = new System.Windows.Forms.DataGridView();
            this.colQuestsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQuestsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCutscenes = new System.Windows.Forms.TabPage();
            this.btnEditCutsceneDialogue = new System.Windows.Forms.Button();
            this.tbCutsceneDetails = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.tbCutsceneTriggers = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.dgvCutsceneTags = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbCutsceneName = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.dgvCutscenes = new System.Windows.Forms.DataGridView();
            this.colCutscenesName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabMonsters = new System.Windows.Forms.TabPage();
            this.btnMonsterCancel = new System.Windows.Forms.Button();
            this.dgvMonsterTags = new System.Windows.Forms.DataGridView();
            this.colMonsterTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbMonsterDescription = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.tbMonsterID = new System.Windows.Forms.TextBox();
            this.tbMonsterName = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.dgvMonsters = new System.Windows.Forms.DataGridView();
            this.colMonstersID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMonstersName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabActions = new System.Windows.Forms.TabPage();
            this.btnActionCancel = new System.Windows.Forms.Button();
            this.dgvActionTags = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbActionDescription = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.tbActionID = new System.Windows.Forms.TextBox();
            this.tbActionName = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.dgvActions = new System.Windows.Forms.DataGridView();
            this.colActionsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colActionsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cbActionType = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.tabCtl.SuspendLayout();
            this.tabItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgItemTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.tabWorldObjects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjectTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjects)).BeginInit();
            this.tabCharacters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCharacterTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCharacters)).BeginInit();
            this.tabClasses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgClassTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClasses)).BeginInit();
            this.tabAdventurers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdventurerTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdventurers)).BeginInit();
            this.tabQuests.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuestTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuests)).BeginInit();
            this.tabCutscenes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutsceneTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutscenes)).BeginInit();
            this.tabMonsters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsterTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsters)).BeginInit();
            this.tabActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActionTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).BeginInit();
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
            this.questToolStripMenuItem,
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
            // questToolStripMenuItem
            // 
            this.questToolStripMenuItem.Name = "questToolStripMenuItem";
            this.questToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.questToolStripMenuItem.Text = "Quest";
            this.questToolStripMenuItem.Click += new System.EventHandler(this.questToolStripMenuItem_Click);
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
            this.tabCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCtl.Controls.Add(this.tabItems);
            this.tabCtl.Controls.Add(this.tabWorldObjects);
            this.tabCtl.Controls.Add(this.tabCharacters);
            this.tabCtl.Controls.Add(this.tabClasses);
            this.tabCtl.Controls.Add(this.tabAdventurers);
            this.tabCtl.Controls.Add(this.tabQuests);
            this.tabCtl.Controls.Add(this.tabCutscenes);
            this.tabCtl.Controls.Add(this.tabMonsters);
            this.tabCtl.Controls.Add(this.tabActions);
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
            this.tabItems.Controls.Add(this.dgItemTags);
            this.tabItems.Controls.Add(this.tbItemDesc);
            this.tabItems.Controls.Add(this.label1);
            this.tabItems.Controls.Add(this.label2);
            this.tabItems.Controls.Add(this.tbItemID);
            this.tabItems.Controls.Add(this.tbItemName);
            this.tabItems.Controls.Add(this.lblName);
            this.tabItems.Controls.Add(this.dgvItems);
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
            this.cbItemSubtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbItemSubtype.FormattingEnabled = true;
            this.cbItemSubtype.Location = new System.Drawing.Point(475, 110);
            this.cbItemSubtype.Name = "cbItemSubtype";
            this.cbItemSubtype.Size = new System.Drawing.Size(149, 21);
            this.cbItemSubtype.TabIndex = 25;
            this.cbItemSubtype.Visible = false;
            // 
            // cbItemType
            // 
            this.cbItemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            // dgvItems
            // 
            this.dgvItems.AllowUserToAddRows = false;
            this.dgvItems.AllowUserToDeleteRows = false;
            this.dgvItems.AllowUserToResizeColumns = false;
            this.dgvItems.AllowUserToResizeRows = false;
            this.dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colItemID,
            this.colItemName});
            this.dgvItems.Location = new System.Drawing.Point(6, 6);
            this.dgvItems.MultiSelect = false;
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.ReadOnly = true;
            this.dgvItems.RowHeadersVisible = false;
            this.dgvItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvItems.Size = new System.Drawing.Size(308, 411);
            this.dgvItems.TabIndex = 14;
            this.dgvItems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItems_CellClick);
            // 
            // colItemID
            // 
            this.colItemID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colItemID.FillWeight = 10F;
            this.colItemID.HeaderText = "ID";
            this.colItemID.Name = "colItemID";
            this.colItemID.ReadOnly = true;
            this.colItemID.Width = 31;
            // 
            // colItemName
            // 
            this.colItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colItemName.FillWeight = 90F;
            this.colItemName.HeaderText = "Name";
            this.colItemName.Name = "colItemName";
            this.colItemName.ReadOnly = true;
            // 
            // tabWorldObjects
            // 
            this.tabWorldObjects.Controls.Add(this.cbWorldObjectType);
            this.tabWorldObjects.Controls.Add(this.btnWorldObjectCancel);
            this.tabWorldObjects.Controls.Add(this.dgvWorldObjectTags);
            this.tabWorldObjects.Controls.Add(this.label4);
            this.tabWorldObjects.Controls.Add(this.tbWorldObjectID);
            this.tabWorldObjects.Controls.Add(this.tbWorldObjectName);
            this.tabWorldObjects.Controls.Add(this.label5);
            this.tabWorldObjects.Controls.Add(this.dgvWorldObjects);
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
            this.cbWorldObjectType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            // dgvWorldObjectTags
            // 
            this.dgvWorldObjectTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorldObjectTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorldObjectTags});
            this.dgvWorldObjectTags.Location = new System.Drawing.Point(320, 60);
            this.dgvWorldObjectTags.Name = "dgvWorldObjectTags";
            this.dgvWorldObjectTags.RowHeadersVisible = false;
            this.dgvWorldObjectTags.Size = new System.Drawing.Size(464, 328);
            this.dgvWorldObjectTags.TabIndex = 33;
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
            // dgvWorldObjects
            // 
            this.dgvWorldObjects.AllowUserToAddRows = false;
            this.dgvWorldObjects.AllowUserToDeleteRows = false;
            this.dgvWorldObjects.AllowUserToResizeColumns = false;
            this.dgvWorldObjects.AllowUserToResizeRows = false;
            this.dgvWorldObjects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorldObjects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorldObjectsID,
            this.colWorldObjectsName});
            this.dgvWorldObjects.Location = new System.Drawing.Point(6, 6);
            this.dgvWorldObjects.MultiSelect = false;
            this.dgvWorldObjects.Name = "dgvWorldObjects";
            this.dgvWorldObjects.ReadOnly = true;
            this.dgvWorldObjects.RowHeadersVisible = false;
            this.dgvWorldObjects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWorldObjects.Size = new System.Drawing.Size(308, 411);
            this.dgvWorldObjects.TabIndex = 26;
            this.dgvWorldObjects.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvWorldObjects_CellClick);
            // 
            // colWorldObjectsID
            // 
            this.colWorldObjectsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colWorldObjectsID.FillWeight = 10F;
            this.colWorldObjectsID.HeaderText = "ID";
            this.colWorldObjectsID.Name = "colWorldObjectsID";
            this.colWorldObjectsID.ReadOnly = true;
            this.colWorldObjectsID.Width = 31;
            // 
            // colWorldObjectsName
            // 
            this.colWorldObjectsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colWorldObjectsName.FillWeight = 90F;
            this.colWorldObjectsName.HeaderText = "Name";
            this.colWorldObjectsName.Name = "colWorldObjectsName";
            this.colWorldObjectsName.ReadOnly = true;
            // 
            // tabCharacters
            // 
            this.tabCharacters.Controls.Add(this.cbEditableCharData);
            this.tabCharacters.Controls.Add(this.btnEdit);
            this.tabCharacters.Controls.Add(this.cbCharacterType);
            this.tabCharacters.Controls.Add(this.btnCancelCharacter);
            this.tabCharacters.Controls.Add(this.dgCharacterTags);
            this.tabCharacters.Controls.Add(this.label3);
            this.tabCharacters.Controls.Add(this.tbCharacterID);
            this.tabCharacters.Controls.Add(this.tbCharacterName);
            this.tabCharacters.Controls.Add(this.label6);
            this.tabCharacters.Controls.Add(this.dgvCharacters);
            this.tabCharacters.Location = new System.Drawing.Point(4, 22);
            this.tabCharacters.Name = "tabCharacters";
            this.tabCharacters.Size = new System.Drawing.Size(790, 425);
            this.tabCharacters.TabIndex = 2;
            this.tabCharacters.Text = "Characters";
            this.tabCharacters.UseVisualStyleBackColor = true;
            // 
            // cbEditableCharData
            // 
            this.cbEditableCharData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEditableCharData.FormattingEnabled = true;
            this.cbEditableCharData.Location = new System.Drawing.Point(320, 396);
            this.cbEditableCharData.Name = "cbEditableCharData";
            this.cbEditableCharData.Size = new System.Drawing.Size(149, 21);
            this.cbEditableCharData.TabIndex = 56;
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(475, 394);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 55;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnDialogue_Click);
            // 
            // cbCharacterType
            // 
            this.cbCharacterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            // dgvCharacters
            // 
            this.dgvCharacters.AllowUserToAddRows = false;
            this.dgvCharacters.AllowUserToDeleteRows = false;
            this.dgvCharacters.AllowUserToResizeColumns = false;
            this.dgvCharacters.AllowUserToResizeRows = false;
            this.dgvCharacters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCharacters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCharacterID,
            this.colCharacterName});
            this.dgvCharacters.Location = new System.Drawing.Point(6, 6);
            this.dgvCharacters.MultiSelect = false;
            this.dgvCharacters.Name = "dgvCharacters";
            this.dgvCharacters.ReadOnly = true;
            this.dgvCharacters.RowHeadersVisible = false;
            this.dgvCharacters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCharacters.Size = new System.Drawing.Size(308, 411);
            this.dgvCharacters.TabIndex = 37;
            this.dgvCharacters.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCharacters_CellClick);
            // 
            // colCharacterID
            // 
            this.colCharacterID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCharacterID.FillWeight = 10F;
            this.colCharacterID.HeaderText = "ID";
            this.colCharacterID.Name = "colCharacterID";
            this.colCharacterID.ReadOnly = true;
            this.colCharacterID.Width = 31;
            // 
            // colCharacterName
            // 
            this.colCharacterName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCharacterName.FillWeight = 90F;
            this.colCharacterName.HeaderText = "Name";
            this.colCharacterName.Name = "colCharacterName";
            this.colCharacterName.ReadOnly = true;
            // 
            // tabClasses
            // 
            this.tabClasses.Controls.Add(this.btnClassCancel);
            this.tabClasses.Controls.Add(this.dgClassTags);
            this.tabClasses.Controls.Add(this.label7);
            this.tabClasses.Controls.Add(this.tbClassID);
            this.tabClasses.Controls.Add(this.tbClassName);
            this.tabClasses.Controls.Add(this.label8);
            this.tabClasses.Controls.Add(this.dgvClasses);
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
            // dgvClasses
            // 
            this.dgvClasses.AllowUserToAddRows = false;
            this.dgvClasses.AllowUserToDeleteRows = false;
            this.dgvClasses.AllowUserToResizeColumns = false;
            this.dgvClasses.AllowUserToResizeRows = false;
            this.dgvClasses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvClasses.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colClassID,
            this.colClassName});
            this.dgvClasses.Location = new System.Drawing.Point(6, 6);
            this.dgvClasses.MultiSelect = false;
            this.dgvClasses.Name = "dgvClasses";
            this.dgvClasses.ReadOnly = true;
            this.dgvClasses.RowHeadersVisible = false;
            this.dgvClasses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvClasses.Size = new System.Drawing.Size(308, 411);
            this.dgvClasses.TabIndex = 46;
            this.dgvClasses.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvClasses_CellClick);
            // 
            // colClassID
            // 
            this.colClassID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colClassID.FillWeight = 10F;
            this.colClassID.HeaderText = "ID";
            this.colClassID.Name = "colClassID";
            this.colClassID.ReadOnly = true;
            this.colClassID.Width = 31;
            // 
            // colClassName
            // 
            this.colClassName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colClassName.FillWeight = 90F;
            this.colClassName.HeaderText = "Name";
            this.colClassName.Name = "colClassName";
            this.colClassName.ReadOnly = true;
            // 
            // tabAdventurers
            // 
            this.tabAdventurers.Controls.Add(this.cbAdventurerType);
            this.tabAdventurers.Controls.Add(this.button1);
            this.tabAdventurers.Controls.Add(this.dgvAdventurerTags);
            this.tabAdventurers.Controls.Add(this.label9);
            this.tabAdventurers.Controls.Add(this.tbAdventurerID);
            this.tabAdventurers.Controls.Add(this.tbAdventurerName);
            this.tabAdventurers.Controls.Add(this.label10);
            this.tabAdventurers.Controls.Add(this.dgvAdventurers);
            this.tabAdventurers.Location = new System.Drawing.Point(4, 22);
            this.tabAdventurers.Name = "tabAdventurers";
            this.tabAdventurers.Size = new System.Drawing.Size(790, 425);
            this.tabAdventurers.TabIndex = 4;
            this.tabAdventurers.Text = "Adventurers";
            this.tabAdventurers.UseVisualStyleBackColor = true;
            // 
            // cbAdventurerType
            // 
            this.cbAdventurerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAdventurerType.FormattingEnabled = true;
            this.cbAdventurerType.Location = new System.Drawing.Point(320, 33);
            this.cbAdventurerType.Name = "cbAdventurerType";
            this.cbAdventurerType.Size = new System.Drawing.Size(149, 21);
            this.cbAdventurerType.TabIndex = 62;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(709, 394);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 61;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnAdventurerCancel_Click);
            // 
            // dgvAdventurerTags
            // 
            this.dgvAdventurerTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAdventurerTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1});
            this.dgvAdventurerTags.Location = new System.Drawing.Point(320, 60);
            this.dgvAdventurerTags.Name = "dgvAdventurerTags";
            this.dgvAdventurerTags.RowHeadersVisible = false;
            this.dgvAdventurerTags.Size = new System.Drawing.Size(464, 328);
            this.dgvAdventurerTags.TabIndex = 59;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.HeaderText = "Tags";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(714, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 13);
            this.label9.TabIndex = 58;
            this.label9.Text = "ID:";
            // 
            // tbAdventurerID
            // 
            this.tbAdventurerID.Location = new System.Drawing.Point(741, 6);
            this.tbAdventurerID.Name = "tbAdventurerID";
            this.tbAdventurerID.Size = new System.Drawing.Size(43, 20);
            this.tbAdventurerID.TabIndex = 57;
            // 
            // tbAdventurerName
            // 
            this.tbAdventurerName.Location = new System.Drawing.Point(361, 6);
            this.tbAdventurerName.Name = "tbAdventurerName";
            this.tbAdventurerName.Size = new System.Drawing.Size(108, 20);
            this.tbAdventurerName.TabIndex = 56;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(317, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(38, 13);
            this.label10.TabIndex = 55;
            this.label10.Text = "Name:";
            // 
            // dgvAdventurers
            // 
            this.dgvAdventurers.AllowUserToAddRows = false;
            this.dgvAdventurers.AllowUserToDeleteRows = false;
            this.dgvAdventurers.AllowUserToResizeColumns = false;
            this.dgvAdventurers.AllowUserToResizeRows = false;
            this.dgvAdventurers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAdventurers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAdventurersID,
            this.colAdventurersName});
            this.dgvAdventurers.Location = new System.Drawing.Point(6, 6);
            this.dgvAdventurers.MultiSelect = false;
            this.dgvAdventurers.Name = "dgvAdventurers";
            this.dgvAdventurers.ReadOnly = true;
            this.dgvAdventurers.RowHeadersVisible = false;
            this.dgvAdventurers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAdventurers.Size = new System.Drawing.Size(308, 411);
            this.dgvAdventurers.TabIndex = 54;
            this.dgvAdventurers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAdventurers_CellClick);
            // 
            // colAdventurersID
            // 
            this.colAdventurersID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colAdventurersID.FillWeight = 10F;
            this.colAdventurersID.HeaderText = "ID";
            this.colAdventurersID.Name = "colAdventurersID";
            this.colAdventurersID.ReadOnly = true;
            this.colAdventurersID.Width = 31;
            // 
            // colAdventurersName
            // 
            this.colAdventurersName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colAdventurersName.FillWeight = 90F;
            this.colAdventurersName.HeaderText = "Name";
            this.colAdventurersName.Name = "colAdventurersName";
            this.colAdventurersName.ReadOnly = true;
            // 
            // tabQuests
            // 
            this.tabQuests.Controls.Add(this.cbQuestType);
            this.tabQuests.Controls.Add(this.btnQuestCancel);
            this.tabQuests.Controls.Add(this.dgvQuestTags);
            this.tabQuests.Controls.Add(this.tbQuestDescription);
            this.tabQuests.Controls.Add(this.label11);
            this.tabQuests.Controls.Add(this.label12);
            this.tabQuests.Controls.Add(this.tbQuestID);
            this.tabQuests.Controls.Add(this.tbQuestName);
            this.tabQuests.Controls.Add(this.label13);
            this.tabQuests.Controls.Add(this.dgvQuests);
            this.tabQuests.Location = new System.Drawing.Point(4, 22);
            this.tabQuests.Name = "tabQuests";
            this.tabQuests.Size = new System.Drawing.Size(790, 425);
            this.tabQuests.TabIndex = 5;
            this.tabQuests.Text = "Quests";
            this.tabQuests.UseVisualStyleBackColor = true;
            // 
            // cbQuestType
            // 
            this.cbQuestType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbQuestType.FormattingEnabled = true;
            this.cbQuestType.Location = new System.Drawing.Point(320, 110);
            this.cbQuestType.Name = "cbQuestType";
            this.cbQuestType.Size = new System.Drawing.Size(149, 21);
            this.cbQuestType.TabIndex = 37;
            // 
            // btnQuestCancel
            // 
            this.btnQuestCancel.Location = new System.Drawing.Point(709, 394);
            this.btnQuestCancel.Name = "btnQuestCancel";
            this.btnQuestCancel.Size = new System.Drawing.Size(75, 23);
            this.btnQuestCancel.TabIndex = 34;
            this.btnQuestCancel.Text = "Cancel";
            this.btnQuestCancel.UseVisualStyleBackColor = true;
            // 
            // dgvQuestTags
            // 
            this.dgvQuestTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvQuestTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colQuestTags});
            this.dgvQuestTags.Location = new System.Drawing.Point(320, 137);
            this.dgvQuestTags.Name = "dgvQuestTags";
            this.dgvQuestTags.RowHeadersVisible = false;
            this.dgvQuestTags.Size = new System.Drawing.Size(464, 251);
            this.dgvQuestTags.TabIndex = 33;
            // 
            // colQuestTags
            // 
            this.colQuestTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colQuestTags.HeaderText = "Tags";
            this.colQuestTags.Name = "colQuestTags";
            // 
            // tbQuestDescription
            // 
            this.tbQuestDescription.Location = new System.Drawing.Point(320, 51);
            this.tbQuestDescription.Multiline = true;
            this.tbQuestDescription.Name = "tbQuestDescription";
            this.tbQuestDescription.Size = new System.Drawing.Size(464, 53);
            this.tbQuestDescription.TabIndex = 32;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(317, 35);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 31;
            this.label11.Text = "Description:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(714, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 13);
            this.label12.TabIndex = 30;
            this.label12.Text = "ID:";
            // 
            // tbQuestID
            // 
            this.tbQuestID.Location = new System.Drawing.Point(741, 6);
            this.tbQuestID.Name = "tbQuestID";
            this.tbQuestID.Size = new System.Drawing.Size(43, 20);
            this.tbQuestID.TabIndex = 29;
            // 
            // tbQuestName
            // 
            this.tbQuestName.Location = new System.Drawing.Point(361, 6);
            this.tbQuestName.Name = "tbQuestName";
            this.tbQuestName.Size = new System.Drawing.Size(108, 20);
            this.tbQuestName.TabIndex = 28;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(317, 9);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(38, 13);
            this.label13.TabIndex = 27;
            this.label13.Text = "Name:";
            // 
            // dgvQuests
            // 
            this.dgvQuests.AllowUserToAddRows = false;
            this.dgvQuests.AllowUserToDeleteRows = false;
            this.dgvQuests.AllowUserToResizeColumns = false;
            this.dgvQuests.AllowUserToResizeRows = false;
            this.dgvQuests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvQuests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colQuestsID,
            this.colQuestsName});
            this.dgvQuests.Location = new System.Drawing.Point(6, 6);
            this.dgvQuests.MultiSelect = false;
            this.dgvQuests.Name = "dgvQuests";
            this.dgvQuests.ReadOnly = true;
            this.dgvQuests.RowHeadersVisible = false;
            this.dgvQuests.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvQuests.Size = new System.Drawing.Size(308, 411);
            this.dgvQuests.TabIndex = 26;
            this.dgvQuests.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvQuests_CellClick);
            // 
            // colQuestsID
            // 
            this.colQuestsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colQuestsID.FillWeight = 10F;
            this.colQuestsID.HeaderText = "ID";
            this.colQuestsID.Name = "colQuestsID";
            this.colQuestsID.ReadOnly = true;
            this.colQuestsID.Width = 31;
            // 
            // colQuestsName
            // 
            this.colQuestsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colQuestsName.FillWeight = 90F;
            this.colQuestsName.HeaderText = "Name";
            this.colQuestsName.Name = "colQuestsName";
            this.colQuestsName.ReadOnly = true;
            // 
            // tabCutscenes
            // 
            this.tabCutscenes.Controls.Add(this.btnEditCutsceneDialogue);
            this.tabCutscenes.Controls.Add(this.tbCutsceneDetails);
            this.tabCutscenes.Controls.Add(this.label17);
            this.tabCutscenes.Controls.Add(this.tbCutsceneTriggers);
            this.tabCutscenes.Controls.Add(this.label14);
            this.tabCutscenes.Controls.Add(this.button2);
            this.tabCutscenes.Controls.Add(this.dgvCutsceneTags);
            this.tabCutscenes.Controls.Add(this.tbCutsceneName);
            this.tabCutscenes.Controls.Add(this.label16);
            this.tabCutscenes.Controls.Add(this.dgvCutscenes);
            this.tabCutscenes.Location = new System.Drawing.Point(4, 22);
            this.tabCutscenes.Name = "tabCutscenes";
            this.tabCutscenes.Size = new System.Drawing.Size(790, 425);
            this.tabCutscenes.TabIndex = 6;
            this.tabCutscenes.Text = "Cutscenes";
            this.tabCutscenes.UseVisualStyleBackColor = true;
            // 
            // btnEditCutsceneDialogue
            // 
            this.btnEditCutsceneDialogue.Location = new System.Drawing.Point(320, 394);
            this.btnEditCutsceneDialogue.Name = "btnEditCutsceneDialogue";
            this.btnEditCutsceneDialogue.Size = new System.Drawing.Size(96, 23);
            this.btnEditCutsceneDialogue.TabIndex = 56;
            this.btnEditCutsceneDialogue.Text = "Edit Dialogue";
            this.btnEditCutsceneDialogue.UseVisualStyleBackColor = true;
            this.btnEditCutsceneDialogue.Click += new System.EventHandler(this.btnEditCutsceneDialogue_Click);
            // 
            // tbCutsceneDetails
            // 
            this.tbCutsceneDetails.Location = new System.Drawing.Point(320, 84);
            this.tbCutsceneDetails.Name = "tbCutsceneDetails";
            this.tbCutsceneDetails.Size = new System.Drawing.Size(464, 20);
            this.tbCutsceneDetails.TabIndex = 38;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(320, 68);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(42, 13);
            this.label17.TabIndex = 37;
            this.label17.Text = "Details:";
            // 
            // tbCutsceneTriggers
            // 
            this.tbCutsceneTriggers.Location = new System.Drawing.Point(320, 45);
            this.tbCutsceneTriggers.Name = "tbCutsceneTriggers";
            this.tbCutsceneTriggers.Size = new System.Drawing.Size(464, 20);
            this.tbCutsceneTriggers.TabIndex = 36;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(317, 29);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(48, 13);
            this.label14.TabIndex = 35;
            this.label14.Text = "Triggers:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(709, 394);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 34;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnCutsceneCancel_Click);
            // 
            // dgvCutsceneTags
            // 
            this.dgvCutsceneTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCutsceneTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2});
            this.dgvCutsceneTags.Location = new System.Drawing.Point(320, 110);
            this.dgvCutsceneTags.Name = "dgvCutsceneTags";
            this.dgvCutsceneTags.RowHeadersVisible = false;
            this.dgvCutsceneTags.Size = new System.Drawing.Size(464, 278);
            this.dgvCutsceneTags.TabIndex = 33;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "Tags";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // tbCutsceneName
            // 
            this.tbCutsceneName.Location = new System.Drawing.Point(361, 6);
            this.tbCutsceneName.Name = "tbCutsceneName";
            this.tbCutsceneName.Size = new System.Drawing.Size(108, 20);
            this.tbCutsceneName.TabIndex = 28;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(317, 9);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(38, 13);
            this.label16.TabIndex = 27;
            this.label16.Text = "Name:";
            // 
            // dgvCutscenes
            // 
            this.dgvCutscenes.AllowUserToAddRows = false;
            this.dgvCutscenes.AllowUserToDeleteRows = false;
            this.dgvCutscenes.AllowUserToResizeColumns = false;
            this.dgvCutscenes.AllowUserToResizeRows = false;
            this.dgvCutscenes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCutscenes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCutscenesName});
            this.dgvCutscenes.Location = new System.Drawing.Point(6, 6);
            this.dgvCutscenes.MultiSelect = false;
            this.dgvCutscenes.Name = "dgvCutscenes";
            this.dgvCutscenes.ReadOnly = true;
            this.dgvCutscenes.RowHeadersVisible = false;
            this.dgvCutscenes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCutscenes.Size = new System.Drawing.Size(308, 411);
            this.dgvCutscenes.TabIndex = 26;
            this.dgvCutscenes.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCutscenes_CellClick);
            // 
            // colCutscenesName
            // 
            this.colCutscenesName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCutscenesName.FillWeight = 90F;
            this.colCutscenesName.HeaderText = "Name";
            this.colCutscenesName.Name = "colCutscenesName";
            this.colCutscenesName.ReadOnly = true;
            // 
            // tabMonsters
            // 
            this.tabMonsters.Controls.Add(this.btnMonsterCancel);
            this.tabMonsters.Controls.Add(this.dgvMonsterTags);
            this.tabMonsters.Controls.Add(this.tbMonsterDescription);
            this.tabMonsters.Controls.Add(this.label15);
            this.tabMonsters.Controls.Add(this.label18);
            this.tabMonsters.Controls.Add(this.tbMonsterID);
            this.tabMonsters.Controls.Add(this.tbMonsterName);
            this.tabMonsters.Controls.Add(this.label19);
            this.tabMonsters.Controls.Add(this.dgvMonsters);
            this.tabMonsters.Location = new System.Drawing.Point(4, 22);
            this.tabMonsters.Name = "tabMonsters";
            this.tabMonsters.Size = new System.Drawing.Size(790, 425);
            this.tabMonsters.TabIndex = 7;
            this.tabMonsters.Text = "Monsters";
            this.tabMonsters.UseVisualStyleBackColor = true;
            // 
            // btnMonsterCancel
            // 
            this.btnMonsterCancel.Location = new System.Drawing.Point(709, 394);
            this.btnMonsterCancel.Name = "btnMonsterCancel";
            this.btnMonsterCancel.Size = new System.Drawing.Size(75, 23);
            this.btnMonsterCancel.TabIndex = 34;
            this.btnMonsterCancel.Text = "Cancel";
            this.btnMonsterCancel.UseVisualStyleBackColor = true;
            // 
            // dgvMonsterTags
            // 
            this.dgvMonsterTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonsterTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMonsterTags});
            this.dgvMonsterTags.Location = new System.Drawing.Point(320, 110);
            this.dgvMonsterTags.Name = "dgvMonsterTags";
            this.dgvMonsterTags.RowHeadersVisible = false;
            this.dgvMonsterTags.Size = new System.Drawing.Size(464, 278);
            this.dgvMonsterTags.TabIndex = 33;
            // 
            // colMonsterTags
            // 
            this.colMonsterTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMonsterTags.HeaderText = "Tags";
            this.colMonsterTags.Name = "colMonsterTags";
            // 
            // tbMonsterDescription
            // 
            this.tbMonsterDescription.Location = new System.Drawing.Point(320, 51);
            this.tbMonsterDescription.Multiline = true;
            this.tbMonsterDescription.Name = "tbMonsterDescription";
            this.tbMonsterDescription.Size = new System.Drawing.Size(464, 53);
            this.tbMonsterDescription.TabIndex = 32;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(317, 35);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(63, 13);
            this.label15.TabIndex = 31;
            this.label15.Text = "Description:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(714, 9);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(21, 13);
            this.label18.TabIndex = 30;
            this.label18.Text = "ID:";
            // 
            // tbMonsterID
            // 
            this.tbMonsterID.Location = new System.Drawing.Point(741, 6);
            this.tbMonsterID.Name = "tbMonsterID";
            this.tbMonsterID.Size = new System.Drawing.Size(43, 20);
            this.tbMonsterID.TabIndex = 29;
            // 
            // tbMonsterName
            // 
            this.tbMonsterName.Location = new System.Drawing.Point(361, 6);
            this.tbMonsterName.Name = "tbMonsterName";
            this.tbMonsterName.Size = new System.Drawing.Size(108, 20);
            this.tbMonsterName.TabIndex = 28;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(317, 9);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(38, 13);
            this.label19.TabIndex = 27;
            this.label19.Text = "Name:";
            // 
            // dgvMonsters
            // 
            this.dgvMonsters.AllowUserToAddRows = false;
            this.dgvMonsters.AllowUserToDeleteRows = false;
            this.dgvMonsters.AllowUserToResizeColumns = false;
            this.dgvMonsters.AllowUserToResizeRows = false;
            this.dgvMonsters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonsters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMonstersID,
            this.colMonstersName});
            this.dgvMonsters.Location = new System.Drawing.Point(6, 6);
            this.dgvMonsters.MultiSelect = false;
            this.dgvMonsters.Name = "dgvMonsters";
            this.dgvMonsters.ReadOnly = true;
            this.dgvMonsters.RowHeadersVisible = false;
            this.dgvMonsters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMonsters.Size = new System.Drawing.Size(308, 411);
            this.dgvMonsters.TabIndex = 26;
            this.dgvMonsters.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMonsters_CellClick);
            // 
            // colMonstersID
            // 
            this.colMonstersID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMonstersID.FillWeight = 10F;
            this.colMonstersID.HeaderText = "ID";
            this.colMonstersID.Name = "colMonstersID";
            this.colMonstersID.ReadOnly = true;
            this.colMonstersID.Width = 31;
            // 
            // colMonstersName
            // 
            this.colMonstersName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMonstersName.FillWeight = 90F;
            this.colMonstersName.HeaderText = "Name";
            this.colMonstersName.Name = "colMonstersName";
            this.colMonstersName.ReadOnly = true;
            // 
            // tabActions
            // 
            this.tabActions.Controls.Add(this.cbActionType);
            this.tabActions.Controls.Add(this.btnActionCancel);
            this.tabActions.Controls.Add(this.dgvActionTags);
            this.tabActions.Controls.Add(this.tbActionDescription);
            this.tabActions.Controls.Add(this.label20);
            this.tabActions.Controls.Add(this.label21);
            this.tabActions.Controls.Add(this.tbActionID);
            this.tabActions.Controls.Add(this.tbActionName);
            this.tabActions.Controls.Add(this.label22);
            this.tabActions.Controls.Add(this.dgvActions);
            this.tabActions.Location = new System.Drawing.Point(4, 22);
            this.tabActions.Name = "tabActions";
            this.tabActions.Size = new System.Drawing.Size(790, 425);
            this.tabActions.TabIndex = 8;
            this.tabActions.Text = "Actions";
            this.tabActions.UseVisualStyleBackColor = true;
            // 
            // btnActionCancel
            // 
            this.btnActionCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnActionCancel.Location = new System.Drawing.Point(709, 394);
            this.btnActionCancel.Name = "btnActionCancel";
            this.btnActionCancel.Size = new System.Drawing.Size(75, 23);
            this.btnActionCancel.TabIndex = 43;
            this.btnActionCancel.Text = "Cancel";
            this.btnActionCancel.UseVisualStyleBackColor = true;
            this.btnActionCancel.Click += new System.EventHandler(this.btnActionCancel_Click);
            // 
            // dgvActionTags
            // 
            this.dgvActionTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvActionTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvActionTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3});
            this.dgvActionTags.Location = new System.Drawing.Point(320, 137);
            this.dgvActionTags.Name = "dgvActionTags";
            this.dgvActionTags.RowHeadersVisible = false;
            this.dgvActionTags.Size = new System.Drawing.Size(464, 251);
            this.dgvActionTags.TabIndex = 42;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.HeaderText = "Tags";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // tbActionDescription
            // 
            this.tbActionDescription.Location = new System.Drawing.Point(320, 51);
            this.tbActionDescription.Multiline = true;
            this.tbActionDescription.Name = "tbActionDescription";
            this.tbActionDescription.Size = new System.Drawing.Size(464, 53);
            this.tbActionDescription.TabIndex = 41;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(317, 35);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(63, 13);
            this.label20.TabIndex = 40;
            this.label20.Text = "Description:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(714, 9);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(21, 13);
            this.label21.TabIndex = 39;
            this.label21.Text = "ID:";
            // 
            // tbActionID
            // 
            this.tbActionID.Location = new System.Drawing.Point(741, 6);
            this.tbActionID.Name = "tbActionID";
            this.tbActionID.Size = new System.Drawing.Size(43, 20);
            this.tbActionID.TabIndex = 38;
            // 
            // tbActionName
            // 
            this.tbActionName.Location = new System.Drawing.Point(361, 6);
            this.tbActionName.Name = "tbActionName";
            this.tbActionName.Size = new System.Drawing.Size(108, 20);
            this.tbActionName.TabIndex = 37;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(317, 9);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(38, 13);
            this.label22.TabIndex = 36;
            this.label22.Text = "Name:";
            // 
            // dgvActions
            // 
            this.dgvActions.AllowUserToAddRows = false;
            this.dgvActions.AllowUserToDeleteRows = false;
            this.dgvActions.AllowUserToResizeColumns = false;
            this.dgvActions.AllowUserToResizeRows = false;
            this.dgvActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvActions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvActions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colActionsID,
            this.colActionsName});
            this.dgvActions.Location = new System.Drawing.Point(6, 6);
            this.dgvActions.MultiSelect = false;
            this.dgvActions.Name = "dgvActions";
            this.dgvActions.ReadOnly = true;
            this.dgvActions.RowHeadersVisible = false;
            this.dgvActions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvActions.Size = new System.Drawing.Size(308, 411);
            this.dgvActions.TabIndex = 35;
            this.dgvActions.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvActions_CellClick);
            // 
            // colActionsID
            // 
            this.colActionsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colActionsID.FillWeight = 10F;
            this.colActionsID.HeaderText = "ID";
            this.colActionsID.Name = "colActionsID";
            this.colActionsID.ReadOnly = true;
            this.colActionsID.Width = 31;
            // 
            // colActionsName
            // 
            this.colActionsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colActionsName.FillWeight = 90F;
            this.colActionsName.HeaderText = "Name";
            this.colActionsName.Name = "colActionsName";
            this.colActionsName.ReadOnly = true;
            // 
            // cbActionType
            // 
            this.cbActionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbActionType.FormattingEnabled = true;
            this.cbActionType.Location = new System.Drawing.Point(320, 110);
            this.cbActionType.Name = "cbActionType";
            this.cbActionType.Size = new System.Drawing.Size(149, 21);
            this.cbActionType.TabIndex = 44;
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
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.tabWorldObjects.ResumeLayout(false);
            this.tabWorldObjects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjectTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjects)).EndInit();
            this.tabCharacters.ResumeLayout(false);
            this.tabCharacters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCharacterTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCharacters)).EndInit();
            this.tabClasses.ResumeLayout(false);
            this.tabClasses.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgClassTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClasses)).EndInit();
            this.tabAdventurers.ResumeLayout(false);
            this.tabAdventurers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdventurerTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdventurers)).EndInit();
            this.tabQuests.ResumeLayout(false);
            this.tabQuests.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuestTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuests)).EndInit();
            this.tabCutscenes.ResumeLayout(false);
            this.tabCutscenes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutsceneTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutscenes)).EndInit();
            this.tabMonsters.ResumeLayout(false);
            this.tabMonsters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsterTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsters)).EndInit();
            this.tabActions.ResumeLayout(false);
            this.tabActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActionTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).EndInit();
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
        private System.Windows.Forms.DataGridView dgItemTags;
        private System.Windows.Forms.TextBox tbItemDesc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbItemID;
        private System.Windows.Forms.TextBox tbItemName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.DataGridView dgvItems;
        private System.Windows.Forms.TabPage tabWorldObjects;
        private System.Windows.Forms.ComboBox cbWorldObjectType;
        private System.Windows.Forms.Button btnWorldObjectCancel;
        private System.Windows.Forms.DataGridView dgvWorldObjectTags;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbWorldObjectID;
        private System.Windows.Forms.TextBox tbWorldObjectName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgvWorldObjects;
        private System.Windows.Forms.ToolStripMenuItem itemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem worldObjectToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemTags;
        private System.Windows.Forms.TabPage tabCharacters;
        private System.Windows.Forms.Button btnCancelCharacter;
        private System.Windows.Forms.DataGridView dgCharacterTags;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbCharacterID;
        private System.Windows.Forms.TextBox tbCharacterName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridView dgvCharacters;
        private System.Windows.Forms.ComboBox cbCharacterType;
        private System.Windows.Forms.TabPage tabClasses;
        private System.Windows.Forms.Button btnClassCancel;
        private System.Windows.Forms.DataGridView dgClassTags;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbClassID;
        private System.Windows.Forms.TextBox tbClassName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgvClasses;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharacterTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassTags;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.TabPage tabAdventurers;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dgvAdventurerTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbAdventurerID;
        private System.Windows.Forms.TextBox tbAdventurerName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DataGridView dgvAdventurers;
        private System.Windows.Forms.ComboBox cbAdventurerType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharacterID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCharacterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAdventurersID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAdventurersName;
        private System.Windows.Forms.TabPage tabQuests;
        private System.Windows.Forms.Button btnQuestCancel;
        private System.Windows.Forms.DataGridView dgvQuestTags;
        private System.Windows.Forms.TextBox tbQuestDescription;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbQuestID;
        private System.Windows.Forms.TextBox tbQuestName;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.DataGridView dgvQuests;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQuestsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQuestsName;
        private System.Windows.Forms.ComboBox cbQuestType;
        private System.Windows.Forms.ComboBox cbEditableCharData;
        private System.Windows.Forms.ToolStripMenuItem questToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQuestTags;
        private System.Windows.Forms.TabPage tabCutscenes;
        private System.Windows.Forms.TextBox tbCutsceneDetails;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbCutsceneTriggers;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView dgvCutsceneTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.TextBox tbCutsceneName;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.DataGridView dgvCutscenes;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCutscenesName;
        private System.Windows.Forms.Button btnEditCutsceneDialogue;
        private System.Windows.Forms.TabPage tabMonsters;
        private System.Windows.Forms.Button btnMonsterCancel;
        private System.Windows.Forms.DataGridView dgvMonsterTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMonsterTags;
        private System.Windows.Forms.TextBox tbMonsterDescription;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox tbMonsterID;
        private System.Windows.Forms.TextBox tbMonsterName;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.DataGridView dgvMonsters;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMonstersID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMonstersName;
        private System.Windows.Forms.TabPage tabActions;
        private System.Windows.Forms.Button btnActionCancel;
        private System.Windows.Forms.DataGridView dgvActionTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.TextBox tbActionDescription;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tbActionID;
        private System.Windows.Forms.TextBox tbActionName;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.DataGridView dgvActions;
        private System.Windows.Forms.DataGridViewTextBoxColumn colActionsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colActionsName;
        private System.Windows.Forms.ComboBox cbActionType;
    }
}

