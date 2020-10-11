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
            this.cbActionType = new System.Windows.Forms.ComboBox();
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
            this.tabShops = new System.Windows.Forms.TabPage();
            this.label36 = new System.Windows.Forms.Label();
            this.tbShopID = new System.Windows.Forms.TextBox();
            this.btnShopCancel = new System.Windows.Forms.Button();
            this.dgvShopTags = new System.Windows.Forms.DataGridView();
            this.colShopTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbShopName = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.dgvShops = new System.Windows.Forms.DataGridView();
            this.colShopsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colShopsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabBuildings = new System.Windows.Forms.TabPage();
            this.btnBuildingCancel = new System.Windows.Forms.Button();
            this.dgvBuildingTags = new System.Windows.Forms.DataGridView();
            this.colBuildingTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbBuildingDescription = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.tbBuildingID = new System.Windows.Forms.TextBox();
            this.tbBuildingName = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.dgvBuildings = new System.Windows.Forms.DataGridView();
            this.colBuildingsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBuildingsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabSpirits = new System.Windows.Forms.TabPage();
            this.btnSpiritCancel = new System.Windows.Forms.Button();
            this.dgvSpiritTags = new System.Windows.Forms.DataGridView();
            this.colSpiritTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbSpiritDescription = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.tbSpiritID = new System.Windows.Forms.TextBox();
            this.tbSpiritName = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.dgvSpirits = new System.Windows.Forms.DataGridView();
            this.colSpiritsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSpiritsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabSummons = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.dgvSummonTags = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbSummonDescription = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.tbSummonID = new System.Windows.Forms.TextBox();
            this.tbSummonName = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.dgvSummons = new System.Windows.Forms.DataGridView();
            this.colSummonsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSummonsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabStatusEffects = new System.Windows.Forms.TabPage();
            this.btnStatusEffectCancel = new System.Windows.Forms.Button();
            this.dgvStatusEffectTags = new System.Windows.Forms.DataGridView();
            this.colStatusEffectsTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbStatusEffectDescription = new System.Windows.Forms.TextBox();
            this.label33 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.tbStatusEffectID = new System.Windows.Forms.TextBox();
            this.tbStatusEffectName = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.dgvStatusEffects = new System.Windows.Forms.DataGridView();
            this.colStatusEffectsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatusEffectsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCutscenesID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCutscenesName = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.tabShops.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShopTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShops)).BeginInit();
            this.tabBuildings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBuildingTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBuildings)).BeginInit();
            this.tabSpirits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpiritTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpirits)).BeginInit();
            this.tabSummons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSummonTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSummons)).BeginInit();
            this.tabStatusEffects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffectTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffects)).BeginInit();
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
            this.tabCtl.Controls.Add(this.tabShops);
            this.tabCtl.Controls.Add(this.tabBuildings);
            this.tabCtl.Controls.Add(this.tabSpirits);
            this.tabCtl.Controls.Add(this.tabSummons);
            this.tabCtl.Controls.Add(this.tabStatusEffects);
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
            this.colCutscenesID,
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
            // cbActionType
            // 
            this.cbActionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbActionType.FormattingEnabled = true;
            this.cbActionType.Location = new System.Drawing.Point(320, 110);
            this.cbActionType.Name = "cbActionType";
            this.cbActionType.Size = new System.Drawing.Size(149, 21);
            this.cbActionType.TabIndex = 44;
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
            // tabShops
            // 
            this.tabShops.Controls.Add(this.label36);
            this.tabShops.Controls.Add(this.tbShopID);
            this.tabShops.Controls.Add(this.btnShopCancel);
            this.tabShops.Controls.Add(this.dgvShopTags);
            this.tabShops.Controls.Add(this.tbShopName);
            this.tabShops.Controls.Add(this.label24);
            this.tabShops.Controls.Add(this.dgvShops);
            this.tabShops.Location = new System.Drawing.Point(4, 22);
            this.tabShops.Name = "tabShops";
            this.tabShops.Size = new System.Drawing.Size(790, 425);
            this.tabShops.TabIndex = 9;
            this.tabShops.Text = "Shops";
            this.tabShops.UseVisualStyleBackColor = true;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(714, 9);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(21, 13);
            this.label36.TabIndex = 60;
            this.label36.Text = "ID:";
            // 
            // tbShopID
            // 
            this.tbShopID.Location = new System.Drawing.Point(741, 6);
            this.tbShopID.Name = "tbShopID";
            this.tbShopID.Size = new System.Drawing.Size(43, 20);
            this.tbShopID.TabIndex = 59;
            // 
            // btnShopCancel
            // 
            this.btnShopCancel.Location = new System.Drawing.Point(709, 394);
            this.btnShopCancel.Name = "btnShopCancel";
            this.btnShopCancel.Size = new System.Drawing.Size(75, 23);
            this.btnShopCancel.TabIndex = 58;
            this.btnShopCancel.Text = "Cancel";
            this.btnShopCancel.UseVisualStyleBackColor = true;
            this.btnShopCancel.Click += new System.EventHandler(this.btnShopCancel_Click);
            // 
            // dgvShopTags
            // 
            this.dgvShopTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvShopTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colShopTags});
            this.dgvShopTags.Location = new System.Drawing.Point(320, 33);
            this.dgvShopTags.Name = "dgvShopTags";
            this.dgvShopTags.RowHeadersVisible = false;
            this.dgvShopTags.Size = new System.Drawing.Size(464, 355);
            this.dgvShopTags.TabIndex = 57;
            // 
            // colShopTags
            // 
            this.colShopTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colShopTags.HeaderText = "Tags";
            this.colShopTags.Name = "colShopTags";
            // 
            // tbShopName
            // 
            this.tbShopName.Location = new System.Drawing.Point(361, 6);
            this.tbShopName.Name = "tbShopName";
            this.tbShopName.Size = new System.Drawing.Size(108, 20);
            this.tbShopName.TabIndex = 54;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(317, 9);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(38, 13);
            this.label24.TabIndex = 53;
            this.label24.Text = "Name:";
            // 
            // dgvShops
            // 
            this.dgvShops.AllowUserToAddRows = false;
            this.dgvShops.AllowUserToDeleteRows = false;
            this.dgvShops.AllowUserToResizeColumns = false;
            this.dgvShops.AllowUserToResizeRows = false;
            this.dgvShops.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvShops.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colShopsID,
            this.colShopsName});
            this.dgvShops.Location = new System.Drawing.Point(6, 6);
            this.dgvShops.MultiSelect = false;
            this.dgvShops.Name = "dgvShops";
            this.dgvShops.ReadOnly = true;
            this.dgvShops.RowHeadersVisible = false;
            this.dgvShops.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvShops.Size = new System.Drawing.Size(308, 411);
            this.dgvShops.TabIndex = 52;
            this.dgvShops.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvShops_CellClick);
            // 
            // colShopsID
            // 
            this.colShopsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colShopsID.FillWeight = 10F;
            this.colShopsID.HeaderText = "ID";
            this.colShopsID.Name = "colShopsID";
            this.colShopsID.ReadOnly = true;
            this.colShopsID.Width = 31;
            // 
            // colShopsName
            // 
            this.colShopsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colShopsName.FillWeight = 90F;
            this.colShopsName.HeaderText = "Name";
            this.colShopsName.Name = "colShopsName";
            this.colShopsName.ReadOnly = true;
            // 
            // tabBuildings
            // 
            this.tabBuildings.Controls.Add(this.btnBuildingCancel);
            this.tabBuildings.Controls.Add(this.dgvBuildingTags);
            this.tabBuildings.Controls.Add(this.tbBuildingDescription);
            this.tabBuildings.Controls.Add(this.label23);
            this.tabBuildings.Controls.Add(this.label25);
            this.tabBuildings.Controls.Add(this.tbBuildingID);
            this.tabBuildings.Controls.Add(this.tbBuildingName);
            this.tabBuildings.Controls.Add(this.label26);
            this.tabBuildings.Controls.Add(this.dgvBuildings);
            this.tabBuildings.Location = new System.Drawing.Point(4, 22);
            this.tabBuildings.Name = "tabBuildings";
            this.tabBuildings.Size = new System.Drawing.Size(790, 425);
            this.tabBuildings.TabIndex = 10;
            this.tabBuildings.Text = "Buildings";
            this.tabBuildings.UseVisualStyleBackColor = true;
            // 
            // btnBuildingCancel
            // 
            this.btnBuildingCancel.Location = new System.Drawing.Point(709, 394);
            this.btnBuildingCancel.Name = "btnBuildingCancel";
            this.btnBuildingCancel.Size = new System.Drawing.Size(75, 23);
            this.btnBuildingCancel.TabIndex = 34;
            this.btnBuildingCancel.Text = "Cancel";
            this.btnBuildingCancel.UseVisualStyleBackColor = true;
            this.btnBuildingCancel.Click += new System.EventHandler(this.btnBuildingCancel_Click);
            // 
            // dgvBuildingTags
            // 
            this.dgvBuildingTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBuildingTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colBuildingTag});
            this.dgvBuildingTags.Location = new System.Drawing.Point(320, 110);
            this.dgvBuildingTags.Name = "dgvBuildingTags";
            this.dgvBuildingTags.RowHeadersVisible = false;
            this.dgvBuildingTags.Size = new System.Drawing.Size(464, 278);
            this.dgvBuildingTags.TabIndex = 33;
            // 
            // colBuildingTag
            // 
            this.colBuildingTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colBuildingTag.HeaderText = "Tags";
            this.colBuildingTag.Name = "colBuildingTag";
            // 
            // tbBuildingDescription
            // 
            this.tbBuildingDescription.Location = new System.Drawing.Point(320, 51);
            this.tbBuildingDescription.Multiline = true;
            this.tbBuildingDescription.Name = "tbBuildingDescription";
            this.tbBuildingDescription.Size = new System.Drawing.Size(464, 53);
            this.tbBuildingDescription.TabIndex = 32;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(317, 35);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(63, 13);
            this.label23.TabIndex = 31;
            this.label23.Text = "Description:";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(714, 9);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(21, 13);
            this.label25.TabIndex = 30;
            this.label25.Text = "ID:";
            // 
            // tbBuildingID
            // 
            this.tbBuildingID.Location = new System.Drawing.Point(741, 6);
            this.tbBuildingID.Name = "tbBuildingID";
            this.tbBuildingID.Size = new System.Drawing.Size(43, 20);
            this.tbBuildingID.TabIndex = 29;
            // 
            // tbBuildingName
            // 
            this.tbBuildingName.Location = new System.Drawing.Point(361, 6);
            this.tbBuildingName.Name = "tbBuildingName";
            this.tbBuildingName.Size = new System.Drawing.Size(108, 20);
            this.tbBuildingName.TabIndex = 28;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(317, 9);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(38, 13);
            this.label26.TabIndex = 27;
            this.label26.Text = "Name:";
            // 
            // dgvBuildings
            // 
            this.dgvBuildings.AllowUserToAddRows = false;
            this.dgvBuildings.AllowUserToDeleteRows = false;
            this.dgvBuildings.AllowUserToResizeColumns = false;
            this.dgvBuildings.AllowUserToResizeRows = false;
            this.dgvBuildings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBuildings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colBuildingsID,
            this.colBuildingsName});
            this.dgvBuildings.Location = new System.Drawing.Point(6, 6);
            this.dgvBuildings.MultiSelect = false;
            this.dgvBuildings.Name = "dgvBuildings";
            this.dgvBuildings.ReadOnly = true;
            this.dgvBuildings.RowHeadersVisible = false;
            this.dgvBuildings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBuildings.Size = new System.Drawing.Size(308, 411);
            this.dgvBuildings.TabIndex = 26;
            this.dgvBuildings.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBuildings_CellClick);
            // 
            // colBuildingsID
            // 
            this.colBuildingsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colBuildingsID.FillWeight = 10F;
            this.colBuildingsID.HeaderText = "ID";
            this.colBuildingsID.Name = "colBuildingsID";
            this.colBuildingsID.ReadOnly = true;
            this.colBuildingsID.Width = 31;
            // 
            // colBuildingsName
            // 
            this.colBuildingsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colBuildingsName.FillWeight = 90F;
            this.colBuildingsName.HeaderText = "Name";
            this.colBuildingsName.Name = "colBuildingsName";
            this.colBuildingsName.ReadOnly = true;
            // 
            // tabSpirits
            // 
            this.tabSpirits.Controls.Add(this.btnSpiritCancel);
            this.tabSpirits.Controls.Add(this.dgvSpiritTags);
            this.tabSpirits.Controls.Add(this.tbSpiritDescription);
            this.tabSpirits.Controls.Add(this.label27);
            this.tabSpirits.Controls.Add(this.label28);
            this.tabSpirits.Controls.Add(this.tbSpiritID);
            this.tabSpirits.Controls.Add(this.tbSpiritName);
            this.tabSpirits.Controls.Add(this.label29);
            this.tabSpirits.Controls.Add(this.dgvSpirits);
            this.tabSpirits.Location = new System.Drawing.Point(4, 22);
            this.tabSpirits.Name = "tabSpirits";
            this.tabSpirits.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabSpirits.Size = new System.Drawing.Size(790, 425);
            this.tabSpirits.TabIndex = 11;
            this.tabSpirits.Text = "Spirits";
            this.tabSpirits.UseVisualStyleBackColor = true;
            // 
            // btnSpiritCancel
            // 
            this.btnSpiritCancel.Location = new System.Drawing.Point(709, 394);
            this.btnSpiritCancel.Name = "btnSpiritCancel";
            this.btnSpiritCancel.Size = new System.Drawing.Size(75, 23);
            this.btnSpiritCancel.TabIndex = 43;
            this.btnSpiritCancel.Text = "Cancel";
            this.btnSpiritCancel.UseVisualStyleBackColor = true;
            this.btnSpiritCancel.Click += new System.EventHandler(this.btnSpiritCancel_Click);
            // 
            // dgvSpiritTags
            // 
            this.dgvSpiritTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSpiritTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSpiritTags});
            this.dgvSpiritTags.Location = new System.Drawing.Point(320, 110);
            this.dgvSpiritTags.Name = "dgvSpiritTags";
            this.dgvSpiritTags.RowHeadersVisible = false;
            this.dgvSpiritTags.Size = new System.Drawing.Size(464, 278);
            this.dgvSpiritTags.TabIndex = 42;
            // 
            // colSpiritTags
            // 
            this.colSpiritTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colSpiritTags.HeaderText = "Tags";
            this.colSpiritTags.Name = "colSpiritTags";
            // 
            // tbSpiritDescription
            // 
            this.tbSpiritDescription.Location = new System.Drawing.Point(320, 51);
            this.tbSpiritDescription.Multiline = true;
            this.tbSpiritDescription.Name = "tbSpiritDescription";
            this.tbSpiritDescription.Size = new System.Drawing.Size(464, 53);
            this.tbSpiritDescription.TabIndex = 41;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(317, 35);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(63, 13);
            this.label27.TabIndex = 40;
            this.label27.Text = "Description:";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(714, 9);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(21, 13);
            this.label28.TabIndex = 39;
            this.label28.Text = "ID:";
            // 
            // tbSpiritID
            // 
            this.tbSpiritID.Location = new System.Drawing.Point(741, 6);
            this.tbSpiritID.Name = "tbSpiritID";
            this.tbSpiritID.Size = new System.Drawing.Size(43, 20);
            this.tbSpiritID.TabIndex = 38;
            // 
            // tbSpiritName
            // 
            this.tbSpiritName.Location = new System.Drawing.Point(361, 6);
            this.tbSpiritName.Name = "tbSpiritName";
            this.tbSpiritName.Size = new System.Drawing.Size(108, 20);
            this.tbSpiritName.TabIndex = 37;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(317, 9);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(38, 13);
            this.label29.TabIndex = 36;
            this.label29.Text = "Name:";
            // 
            // dgvSpirits
            // 
            this.dgvSpirits.AllowUserToAddRows = false;
            this.dgvSpirits.AllowUserToDeleteRows = false;
            this.dgvSpirits.AllowUserToResizeColumns = false;
            this.dgvSpirits.AllowUserToResizeRows = false;
            this.dgvSpirits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSpirits.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSpiritsID,
            this.colSpiritsName});
            this.dgvSpirits.Location = new System.Drawing.Point(6, 6);
            this.dgvSpirits.MultiSelect = false;
            this.dgvSpirits.Name = "dgvSpirits";
            this.dgvSpirits.ReadOnly = true;
            this.dgvSpirits.RowHeadersVisible = false;
            this.dgvSpirits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSpirits.Size = new System.Drawing.Size(308, 411);
            this.dgvSpirits.TabIndex = 35;
            this.dgvSpirits.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSpirits_CellClick);
            // 
            // colSpiritsID
            // 
            this.colSpiritsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSpiritsID.FillWeight = 10F;
            this.colSpiritsID.HeaderText = "ID";
            this.colSpiritsID.Name = "colSpiritsID";
            this.colSpiritsID.ReadOnly = true;
            this.colSpiritsID.Width = 31;
            // 
            // colSpiritsName
            // 
            this.colSpiritsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colSpiritsName.FillWeight = 90F;
            this.colSpiritsName.HeaderText = "Name";
            this.colSpiritsName.Name = "colSpiritsName";
            this.colSpiritsName.ReadOnly = true;
            // 
            // tabSummons
            // 
            this.tabSummons.Controls.Add(this.button3);
            this.tabSummons.Controls.Add(this.dgvSummonTags);
            this.tabSummons.Controls.Add(this.tbSummonDescription);
            this.tabSummons.Controls.Add(this.label30);
            this.tabSummons.Controls.Add(this.label31);
            this.tabSummons.Controls.Add(this.tbSummonID);
            this.tabSummons.Controls.Add(this.tbSummonName);
            this.tabSummons.Controls.Add(this.label32);
            this.tabSummons.Controls.Add(this.dgvSummons);
            this.tabSummons.Location = new System.Drawing.Point(4, 22);
            this.tabSummons.Name = "tabSummons";
            this.tabSummons.Size = new System.Drawing.Size(790, 425);
            this.tabSummons.TabIndex = 12;
            this.tabSummons.Text = "Summons";
            this.tabSummons.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(709, 394);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 52;
            this.button3.Text = "Cancel";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnSummonCancel_Click);
            // 
            // dgvSummonTags
            // 
            this.dgvSummonTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSummonTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn4});
            this.dgvSummonTags.Location = new System.Drawing.Point(320, 110);
            this.dgvSummonTags.Name = "dgvSummonTags";
            this.dgvSummonTags.RowHeadersVisible = false;
            this.dgvSummonTags.Size = new System.Drawing.Size(464, 278);
            this.dgvSummonTags.TabIndex = 51;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.HeaderText = "Tags";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // tbSummonDescription
            // 
            this.tbSummonDescription.Location = new System.Drawing.Point(320, 51);
            this.tbSummonDescription.Multiline = true;
            this.tbSummonDescription.Name = "tbSummonDescription";
            this.tbSummonDescription.Size = new System.Drawing.Size(464, 53);
            this.tbSummonDescription.TabIndex = 50;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(317, 35);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(63, 13);
            this.label30.TabIndex = 49;
            this.label30.Text = "Description:";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(714, 9);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(21, 13);
            this.label31.TabIndex = 48;
            this.label31.Text = "ID:";
            // 
            // tbSummonID
            // 
            this.tbSummonID.Location = new System.Drawing.Point(741, 6);
            this.tbSummonID.Name = "tbSummonID";
            this.tbSummonID.Size = new System.Drawing.Size(43, 20);
            this.tbSummonID.TabIndex = 47;
            // 
            // tbSummonName
            // 
            this.tbSummonName.Location = new System.Drawing.Point(361, 6);
            this.tbSummonName.Name = "tbSummonName";
            this.tbSummonName.Size = new System.Drawing.Size(108, 20);
            this.tbSummonName.TabIndex = 46;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(317, 9);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(38, 13);
            this.label32.TabIndex = 45;
            this.label32.Text = "Name:";
            // 
            // dgvSummons
            // 
            this.dgvSummons.AllowUserToAddRows = false;
            this.dgvSummons.AllowUserToDeleteRows = false;
            this.dgvSummons.AllowUserToResizeColumns = false;
            this.dgvSummons.AllowUserToResizeRows = false;
            this.dgvSummons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSummons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSummonsID,
            this.colSummonsName});
            this.dgvSummons.Location = new System.Drawing.Point(6, 6);
            this.dgvSummons.MultiSelect = false;
            this.dgvSummons.Name = "dgvSummons";
            this.dgvSummons.ReadOnly = true;
            this.dgvSummons.RowHeadersVisible = false;
            this.dgvSummons.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSummons.Size = new System.Drawing.Size(308, 411);
            this.dgvSummons.TabIndex = 44;
            this.dgvSummons.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSummons_CellClick);
            // 
            // colSummonsID
            // 
            this.colSummonsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSummonsID.FillWeight = 10F;
            this.colSummonsID.HeaderText = "ID";
            this.colSummonsID.Name = "colSummonsID";
            this.colSummonsID.ReadOnly = true;
            this.colSummonsID.Width = 31;
            // 
            // colSummonsName
            // 
            this.colSummonsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colSummonsName.FillWeight = 90F;
            this.colSummonsName.HeaderText = "Name";
            this.colSummonsName.Name = "colSummonsName";
            this.colSummonsName.ReadOnly = true;
            // 
            // tabStatusEffects
            // 
            this.tabStatusEffects.Controls.Add(this.btnStatusEffectCancel);
            this.tabStatusEffects.Controls.Add(this.dgvStatusEffectTags);
            this.tabStatusEffects.Controls.Add(this.tbStatusEffectDescription);
            this.tabStatusEffects.Controls.Add(this.label33);
            this.tabStatusEffects.Controls.Add(this.label34);
            this.tabStatusEffects.Controls.Add(this.tbStatusEffectID);
            this.tabStatusEffects.Controls.Add(this.tbStatusEffectName);
            this.tabStatusEffects.Controls.Add(this.label35);
            this.tabStatusEffects.Controls.Add(this.dgvStatusEffects);
            this.tabStatusEffects.Location = new System.Drawing.Point(4, 22);
            this.tabStatusEffects.Name = "tabStatusEffects";
            this.tabStatusEffects.Size = new System.Drawing.Size(790, 425);
            this.tabStatusEffects.TabIndex = 13;
            this.tabStatusEffects.Text = "Status Effects";
            this.tabStatusEffects.UseVisualStyleBackColor = true;
            // 
            // btnStatusEffectCancel
            // 
            this.btnStatusEffectCancel.Location = new System.Drawing.Point(709, 394);
            this.btnStatusEffectCancel.Name = "btnStatusEffectCancel";
            this.btnStatusEffectCancel.Size = new System.Drawing.Size(75, 23);
            this.btnStatusEffectCancel.TabIndex = 61;
            this.btnStatusEffectCancel.Text = "Cancel";
            this.btnStatusEffectCancel.UseVisualStyleBackColor = true;
            this.btnStatusEffectCancel.Click += new System.EventHandler(this.btnStatusEffectCancel_Click);
            // 
            // dgvStatusEffectTags
            // 
            this.dgvStatusEffectTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStatusEffectTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStatusEffectsTag});
            this.dgvStatusEffectTags.Location = new System.Drawing.Point(320, 110);
            this.dgvStatusEffectTags.Name = "dgvStatusEffectTags";
            this.dgvStatusEffectTags.RowHeadersVisible = false;
            this.dgvStatusEffectTags.Size = new System.Drawing.Size(464, 278);
            this.dgvStatusEffectTags.TabIndex = 60;
            // 
            // colStatusEffectsTag
            // 
            this.colStatusEffectsTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusEffectsTag.HeaderText = "Tags";
            this.colStatusEffectsTag.Name = "colStatusEffectsTag";
            // 
            // tbStatusEffectDescription
            // 
            this.tbStatusEffectDescription.Location = new System.Drawing.Point(320, 51);
            this.tbStatusEffectDescription.Multiline = true;
            this.tbStatusEffectDescription.Name = "tbStatusEffectDescription";
            this.tbStatusEffectDescription.Size = new System.Drawing.Size(464, 53);
            this.tbStatusEffectDescription.TabIndex = 59;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(317, 35);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(63, 13);
            this.label33.TabIndex = 58;
            this.label33.Text = "Description:";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(714, 9);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(21, 13);
            this.label34.TabIndex = 57;
            this.label34.Text = "ID:";
            // 
            // tbStatusEffectID
            // 
            this.tbStatusEffectID.Location = new System.Drawing.Point(741, 6);
            this.tbStatusEffectID.Name = "tbStatusEffectID";
            this.tbStatusEffectID.Size = new System.Drawing.Size(43, 20);
            this.tbStatusEffectID.TabIndex = 56;
            // 
            // tbStatusEffectName
            // 
            this.tbStatusEffectName.Location = new System.Drawing.Point(361, 6);
            this.tbStatusEffectName.Name = "tbStatusEffectName";
            this.tbStatusEffectName.Size = new System.Drawing.Size(108, 20);
            this.tbStatusEffectName.TabIndex = 55;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(317, 9);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(38, 13);
            this.label35.TabIndex = 54;
            this.label35.Text = "Name:";
            // 
            // dgvStatusEffects
            // 
            this.dgvStatusEffects.AllowUserToAddRows = false;
            this.dgvStatusEffects.AllowUserToDeleteRows = false;
            this.dgvStatusEffects.AllowUserToResizeColumns = false;
            this.dgvStatusEffects.AllowUserToResizeRows = false;
            this.dgvStatusEffects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStatusEffects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStatusEffectsID,
            this.colStatusEffectsName});
            this.dgvStatusEffects.Location = new System.Drawing.Point(6, 6);
            this.dgvStatusEffects.MultiSelect = false;
            this.dgvStatusEffects.Name = "dgvStatusEffects";
            this.dgvStatusEffects.ReadOnly = true;
            this.dgvStatusEffects.RowHeadersVisible = false;
            this.dgvStatusEffects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStatusEffects.Size = new System.Drawing.Size(308, 411);
            this.dgvStatusEffects.TabIndex = 53;
            this.dgvStatusEffects.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStatusEffects_CellClick);
            // 
            // colStatusEffectsID
            // 
            this.colStatusEffectsID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStatusEffectsID.FillWeight = 10F;
            this.colStatusEffectsID.HeaderText = "ID";
            this.colStatusEffectsID.Name = "colStatusEffectsID";
            this.colStatusEffectsID.ReadOnly = true;
            this.colStatusEffectsID.Width = 31;
            // 
            // colStatusEffectsName
            // 
            this.colStatusEffectsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusEffectsName.FillWeight = 90F;
            this.colStatusEffectsName.HeaderText = "Name";
            this.colStatusEffectsName.Name = "colStatusEffectsName";
            this.colStatusEffectsName.ReadOnly = true;
            // 
            // colCutscenesID
            // 
            this.colCutscenesID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCutscenesID.FillWeight = 10F;
            this.colCutscenesID.HeaderText = "ID";
            this.colCutscenesID.Name = "colCutscenesID";
            this.colCutscenesID.ReadOnly = true;
            this.colCutscenesID.Width = 31;
            // 
            // colCutscenesName
            // 
            this.colCutscenesName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCutscenesName.FillWeight = 90F;
            this.colCutscenesName.HeaderText = "Name";
            this.colCutscenesName.Name = "colCutscenesName";
            this.colCutscenesName.ReadOnly = true;
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
            this.tabShops.ResumeLayout(false);
            this.tabShops.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShopTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShops)).EndInit();
            this.tabBuildings.ResumeLayout(false);
            this.tabBuildings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBuildingTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBuildings)).EndInit();
            this.tabSpirits.ResumeLayout(false);
            this.tabSpirits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpiritTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpirits)).EndInit();
            this.tabSummons.ResumeLayout(false);
            this.tabSummons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSummonTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSummons)).EndInit();
            this.tabStatusEffects.ResumeLayout(false);
            this.tabStatusEffects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffectTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffects)).EndInit();
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
        private System.Windows.Forms.TabPage tabShops;
        private System.Windows.Forms.DataGridView dgvShopTags;
        private System.Windows.Forms.TextBox tbShopName;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.DataGridView dgvShops;
        private System.Windows.Forms.Button btnShopCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colShopTags;
        private System.Windows.Forms.TabPage tabBuildings;
        private System.Windows.Forms.Button btnBuildingCancel;
        private System.Windows.Forms.DataGridView dgvBuildingTags;
        private System.Windows.Forms.TextBox tbBuildingDescription;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox tbBuildingID;
        private System.Windows.Forms.TextBox tbBuildingName;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.DataGridView dgvBuildings;
        private System.Windows.Forms.TabPage tabSpirits;
        private System.Windows.Forms.TabPage tabSummons;
        private System.Windows.Forms.TabPage tabStatusEffects;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBuildingTag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBuildingsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBuildingsName;
        private System.Windows.Forms.Button btnSpiritCancel;
        private System.Windows.Forms.DataGridView dgvSpiritTags;
        private System.Windows.Forms.TextBox tbSpiritDescription;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox tbSpiritID;
        private System.Windows.Forms.TextBox tbSpiritName;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.DataGridView dgvSpirits;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSpiritTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSpiritsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSpiritsName;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridView dgvSummonTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.TextBox tbSummonDescription;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox tbSummonID;
        private System.Windows.Forms.TextBox tbSummonName;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.DataGridView dgvSummons;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSummonsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSummonsName;
        private System.Windows.Forms.Button btnStatusEffectCancel;
        private System.Windows.Forms.DataGridView dgvStatusEffectTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusEffectsTag;
        private System.Windows.Forms.TextBox tbStatusEffectDescription;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox tbStatusEffectID;
        private System.Windows.Forms.TextBox tbStatusEffectName;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.DataGridView dgvStatusEffects;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusEffectsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusEffectsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colShopsID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colShopsName;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox tbShopID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCutscenesID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCutscenesName;
    }
}

