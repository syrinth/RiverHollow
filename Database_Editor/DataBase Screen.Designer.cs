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
            this.tabAction = new System.Windows.Forms.TabPage();
            this.cbActionType = new System.Windows.Forms.ComboBox();
            this.btnActionCancel = new System.Windows.Forms.Button();
            this.dgvActionTags = new System.Windows.Forms.DataGridView();
            this.colActionTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbActionDescription = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.tbActionID = new System.Windows.Forms.TextBox();
            this.tbActionName = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.dgvActions = new System.Windows.Forms.DataGridView();
            this.colActionsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tabNPC = new System.Windows.Forms.TabPage();
            this.cbEditableCharData = new System.Windows.Forms.ComboBox();
            this.btnEdit = new System.Windows.Forms.Button();
            this.cbNPCType = new System.Windows.Forms.ComboBox();
            this.btnCancelNPC = new System.Windows.Forms.Button();
            this.dgvNPCTags = new System.Windows.Forms.DataGridView();
            this.colNPCTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNPCID = new System.Windows.Forms.TextBox();
            this.tbNPCName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dgvNPCs = new System.Windows.Forms.DataGridView();
            this.colNPCsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabClass = new System.Windows.Forms.TabPage();
            this.tbClassDescription = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.btnClassCancel = new System.Windows.Forms.Button();
            this.dgvClassTags = new System.Windows.Forms.DataGridView();
            this.colClassTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label7 = new System.Windows.Forms.Label();
            this.tbClassID = new System.Windows.Forms.TextBox();
            this.tbClassName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvClasses = new System.Windows.Forms.DataGridView();
            this.colClassName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCutscene = new System.Windows.Forms.TabPage();
            this.label26 = new System.Windows.Forms.Label();
            this.tbCutsceneID = new System.Windows.Forms.TextBox();
            this.btnEditCutsceneDialogue = new System.Windows.Forms.Button();
            this.tbCutsceneDetails = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.tbCutsceneTriggers = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.dgvCutsceneTags = new System.Windows.Forms.DataGridView();
            this.colCutsceneTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbCutsceneName = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.dgvCutscenes = new System.Windows.Forms.DataGridView();
            this.colCutscenesName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabDungeon = new System.Windows.Forms.TabPage();
            this.tbDungeonDescription = new System.Windows.Forms.TextBox();
            this.label40 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.tbDungeonID = new System.Windows.Forms.TextBox();
            this.tbDungeonName = new System.Windows.Forms.TextBox();
            this.label39 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.dgvDungeonTags = new System.Windows.Forms.DataGridView();
            this.colDungeonsTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDungeons = new System.Windows.Forms.DataGridView();
            this.colDungeonsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.colItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabLight = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.tbLightID = new System.Windows.Forms.TextBox();
            this.tbLightName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.dgvLightTags = new System.Windows.Forms.DataGridView();
            this.colLightTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvLights = new System.Windows.Forms.DataGridView();
            this.colLightsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabMonster = new System.Windows.Forms.TabPage();
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
            this.colMonstersName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabShop = new System.Windows.Forms.TabPage();
            this.label36 = new System.Windows.Forms.Label();
            this.tbShopID = new System.Windows.Forms.TextBox();
            this.btnShopCancel = new System.Windows.Forms.Button();
            this.dgvShopTags = new System.Windows.Forms.DataGridView();
            this.colShopTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbShopName = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.dgvShops = new System.Windows.Forms.DataGridView();
            this.colShopsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabStatusEffect = new System.Windows.Forms.TabPage();
            this.cbStatusEffect = new System.Windows.Forms.ComboBox();
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
            this.colStatusEffectsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabTask = new System.Windows.Forms.TabPage();
            this.cbTaskType = new System.Windows.Forms.ComboBox();
            this.btnTaskCancel = new System.Windows.Forms.Button();
            this.dgvTaskTags = new System.Windows.Forms.DataGridView();
            this.colTaskTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbTaskDescription = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tbTaskID = new System.Windows.Forms.TextBox();
            this.tbTaskName = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.dgvTasks = new System.Windows.Forms.DataGridView();
            this.colTasksName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabWorldObject = new System.Windows.Forms.TabPage();
            this.cbWorldObjectType = new System.Windows.Forms.ComboBox();
            this.btnWorldObjectCancel = new System.Windows.Forms.Button();
            this.dgvWorldObjectTags = new System.Windows.Forms.DataGridView();
            this.colWorldObjectTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.tbWorldObjectID = new System.Windows.Forms.TextBox();
            this.tbWorldObjectName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvWorldObjects = new System.Windows.Forms.DataGridView();
            this.colWorldObjectsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabUpgrade = new System.Windows.Forms.TabPage();
            this.btnUpgradeCancel = new System.Windows.Forms.Button();
            this.dgvUpgradeTags = new System.Windows.Forms.DataGridView();
            this.colUpgradeTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label23 = new System.Windows.Forms.Label();
            this.tbUpgradeID = new System.Windows.Forms.TextBox();
            this.tbUpgradeName = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.dgvUpgrades = new System.Windows.Forms.DataGridView();
            this.colUpgradesName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.tabCtl.SuspendLayout();
            this.tabAction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActionTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).BeginInit();
            this.tabNPC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNPCTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNPCs)).BeginInit();
            this.tabClass.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClassTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClasses)).BeginInit();
            this.tabCutscene.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutsceneTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutscenes)).BeginInit();
            this.tabDungeon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDungeonTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDungeons)).BeginInit();
            this.tabItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgItemTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.tabLight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLightTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLights)).BeginInit();
            this.tabMonster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsterTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsters)).BeginInit();
            this.tabShop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShopTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShops)).BeginInit();
            this.tabStatusEffect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffectTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffects)).BeginInit();
            this.tabTask.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTaskTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).BeginInit();
            this.tabWorldObject.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjectTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjects)).BeginInit();
            this.tabUpgrade.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUpgradeTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUpgrades)).BeginInit();
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
            this.tabCtl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCtl.Controls.Add(this.tabAction);
            this.tabCtl.Controls.Add(this.tabNPC);
            this.tabCtl.Controls.Add(this.tabClass);
            this.tabCtl.Controls.Add(this.tabCutscene);
            this.tabCtl.Controls.Add(this.tabDungeon);
            this.tabCtl.Controls.Add(this.tabItems);
            this.tabCtl.Controls.Add(this.tabLight);
            this.tabCtl.Controls.Add(this.tabMonster);
            this.tabCtl.Controls.Add(this.tabShop);
            this.tabCtl.Controls.Add(this.tabStatusEffect);
            this.tabCtl.Controls.Add(this.tabTask);
            this.tabCtl.Controls.Add(this.tabWorldObject);
            this.tabCtl.Controls.Add(this.tabUpgrade);
            this.tabCtl.Location = new System.Drawing.Point(12, 27);
            this.tabCtl.Name = "tabCtl";
            this.tabCtl.SelectedIndex = 0;
            this.tabCtl.Size = new System.Drawing.Size(798, 451);
            this.tabCtl.TabIndex = 12;
            this.tabCtl.SelectedIndexChanged += new System.EventHandler(this.tabCtl_SelectedIndexChanged);
            // 
            // tabAction
            // 
            this.tabAction.Controls.Add(this.cbActionType);
            this.tabAction.Controls.Add(this.btnActionCancel);
            this.tabAction.Controls.Add(this.dgvActionTags);
            this.tabAction.Controls.Add(this.tbActionDescription);
            this.tabAction.Controls.Add(this.label20);
            this.tabAction.Controls.Add(this.label21);
            this.tabAction.Controls.Add(this.tbActionID);
            this.tabAction.Controls.Add(this.tbActionName);
            this.tabAction.Controls.Add(this.label22);
            this.tabAction.Controls.Add(this.dgvActions);
            this.tabAction.Location = new System.Drawing.Point(4, 22);
            this.tabAction.Name = "tabAction";
            this.tabAction.Size = new System.Drawing.Size(790, 425);
            this.tabAction.TabIndex = 8;
            this.tabAction.Text = "Actions";
            this.tabAction.UseVisualStyleBackColor = true;
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
            this.dgvActionTags.AllowUserToResizeColumns = false;
            this.dgvActionTags.AllowUserToResizeRows = false;
            this.dgvActionTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvActionTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvActionTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colActionTags});
            this.dgvActionTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvActionTags.Location = new System.Drawing.Point(320, 137);
            this.dgvActionTags.Name = "dgvActionTags";
            this.dgvActionTags.RowHeadersVisible = false;
            this.dgvActionTags.Size = new System.Drawing.Size(464, 251);
            this.dgvActionTags.TabIndex = 42;
            // 
            // colActionTags
            // 
            this.colActionTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colActionTags.HeaderText = "Tags";
            this.colActionTags.Name = "colActionTags";
            this.colActionTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.colActionsName});
            this.dgvActions.ContextMenuStrip = this.contextMenu;
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
            // colActionsName
            // 
            this.colActionsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colActionsName.FillWeight = 90F;
            this.colActionsName.HeaderText = "Name";
            this.colActionsName.Name = "colActionsName";
            this.colActionsName.ReadOnly = true;
            this.colActionsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // contextMenu
            // 
            this.contextMenu.Name = "contextMenuStripItems";
            this.contextMenu.Size = new System.Drawing.Size(61, 4);
            this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // tabNPC
            // 
            this.tabNPC.Controls.Add(this.cbEditableCharData);
            this.tabNPC.Controls.Add(this.btnEdit);
            this.tabNPC.Controls.Add(this.cbNPCType);
            this.tabNPC.Controls.Add(this.btnCancelNPC);
            this.tabNPC.Controls.Add(this.dgvNPCTags);
            this.tabNPC.Controls.Add(this.label3);
            this.tabNPC.Controls.Add(this.tbNPCID);
            this.tabNPC.Controls.Add(this.tbNPCName);
            this.tabNPC.Controls.Add(this.label6);
            this.tabNPC.Controls.Add(this.dgvNPCs);
            this.tabNPC.Location = new System.Drawing.Point(4, 22);
            this.tabNPC.Name = "tabNPC";
            this.tabNPC.Size = new System.Drawing.Size(790, 425);
            this.tabNPC.TabIndex = 2;
            this.tabNPC.Text = "NPCs";
            this.tabNPC.UseVisualStyleBackColor = true;
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
            // cbNPCType
            // 
            this.cbNPCType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbNPCType.FormattingEnabled = true;
            this.cbNPCType.Location = new System.Drawing.Point(320, 33);
            this.cbNPCType.Name = "cbNPCType";
            this.cbNPCType.Size = new System.Drawing.Size(149, 21);
            this.cbNPCType.TabIndex = 45;
            // 
            // btnCancelNPC
            // 
            this.btnCancelNPC.Location = new System.Drawing.Point(709, 394);
            this.btnCancelNPC.Name = "btnCancelNPC";
            this.btnCancelNPC.Size = new System.Drawing.Size(75, 23);
            this.btnCancelNPC.TabIndex = 44;
            this.btnCancelNPC.Text = "Cancel";
            this.btnCancelNPC.UseVisualStyleBackColor = true;
            this.btnCancelNPC.Click += new System.EventHandler(this.btnNPCCancel_Click);
            // 
            // dgvNPCTags
            // 
            this.dgvNPCTags.AllowUserToResizeColumns = false;
            this.dgvNPCTags.AllowUserToResizeRows = false;
            this.dgvNPCTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNPCTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNPCTags});
            this.dgvNPCTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvNPCTags.Location = new System.Drawing.Point(320, 60);
            this.dgvNPCTags.Name = "dgvNPCTags";
            this.dgvNPCTags.RowHeadersVisible = false;
            this.dgvNPCTags.Size = new System.Drawing.Size(464, 328);
            this.dgvNPCTags.TabIndex = 42;
            // 
            // colNPCTags
            // 
            this.colNPCTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colNPCTags.HeaderText = "Tags";
            this.colNPCTags.Name = "colNPCTags";
            this.colNPCTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            // tbNPCID
            // 
            this.tbNPCID.Location = new System.Drawing.Point(741, 6);
            this.tbNPCID.Name = "tbNPCID";
            this.tbNPCID.Size = new System.Drawing.Size(43, 20);
            this.tbNPCID.TabIndex = 40;
            // 
            // tbNPCName
            // 
            this.tbNPCName.Location = new System.Drawing.Point(361, 6);
            this.tbNPCName.Name = "tbNPCName";
            this.tbNPCName.Size = new System.Drawing.Size(108, 20);
            this.tbNPCName.TabIndex = 39;
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
            // dgvNPCs
            // 
            this.dgvNPCs.AllowUserToAddRows = false;
            this.dgvNPCs.AllowUserToDeleteRows = false;
            this.dgvNPCs.AllowUserToResizeColumns = false;
            this.dgvNPCs.AllowUserToResizeRows = false;
            this.dgvNPCs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNPCs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNPCsName});
            this.dgvNPCs.ContextMenuStrip = this.contextMenu;
            this.dgvNPCs.Location = new System.Drawing.Point(6, 6);
            this.dgvNPCs.MultiSelect = false;
            this.dgvNPCs.Name = "dgvNPCs";
            this.dgvNPCs.ReadOnly = true;
            this.dgvNPCs.RowHeadersVisible = false;
            this.dgvNPCs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNPCs.Size = new System.Drawing.Size(308, 411);
            this.dgvNPCs.TabIndex = 37;
            this.dgvNPCs.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvNPCs_CellClick);
            // 
            // colNPCsName
            // 
            this.colNPCsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colNPCsName.FillWeight = 90F;
            this.colNPCsName.HeaderText = "Name";
            this.colNPCsName.Name = "colNPCsName";
            this.colNPCsName.ReadOnly = true;
            this.colNPCsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabClass
            // 
            this.tabClass.Controls.Add(this.tbClassDescription);
            this.tabClass.Controls.Add(this.label37);
            this.tabClass.Controls.Add(this.btnClassCancel);
            this.tabClass.Controls.Add(this.dgvClassTags);
            this.tabClass.Controls.Add(this.label7);
            this.tabClass.Controls.Add(this.tbClassID);
            this.tabClass.Controls.Add(this.tbClassName);
            this.tabClass.Controls.Add(this.label8);
            this.tabClass.Controls.Add(this.dgvClasses);
            this.tabClass.Location = new System.Drawing.Point(4, 22);
            this.tabClass.Name = "tabClass";
            this.tabClass.Size = new System.Drawing.Size(790, 425);
            this.tabClass.TabIndex = 3;
            this.tabClass.Text = "Classes";
            this.tabClass.UseVisualStyleBackColor = true;
            // 
            // tbClassDescription
            // 
            this.tbClassDescription.Location = new System.Drawing.Point(320, 51);
            this.tbClassDescription.Multiline = true;
            this.tbClassDescription.Name = "tbClassDescription";
            this.tbClassDescription.Size = new System.Drawing.Size(464, 53);
            this.tbClassDescription.TabIndex = 55;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(317, 35);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(63, 13);
            this.label37.TabIndex = 54;
            this.label37.Text = "Description:";
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
            // dgvClassTags
            // 
            this.dgvClassTags.AllowUserToResizeColumns = false;
            this.dgvClassTags.AllowUserToResizeRows = false;
            this.dgvClassTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvClassTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colClassTags});
            this.dgvClassTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvClassTags.Location = new System.Drawing.Point(320, 110);
            this.dgvClassTags.Name = "dgvClassTags";
            this.dgvClassTags.RowHeadersVisible = false;
            this.dgvClassTags.Size = new System.Drawing.Size(464, 278);
            this.dgvClassTags.TabIndex = 51;
            // 
            // colClassTags
            // 
            this.colClassTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colClassTags.HeaderText = "Tags";
            this.colClassTags.Name = "colClassTags";
            this.colClassTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.colClassName});
            this.dgvClasses.ContextMenuStrip = this.contextMenu;
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
            // colClassName
            // 
            this.colClassName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colClassName.FillWeight = 90F;
            this.colClassName.HeaderText = "Name";
            this.colClassName.Name = "colClassName";
            this.colClassName.ReadOnly = true;
            this.colClassName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabCutscene
            // 
            this.tabCutscene.Controls.Add(this.label26);
            this.tabCutscene.Controls.Add(this.tbCutsceneID);
            this.tabCutscene.Controls.Add(this.btnEditCutsceneDialogue);
            this.tabCutscene.Controls.Add(this.tbCutsceneDetails);
            this.tabCutscene.Controls.Add(this.label17);
            this.tabCutscene.Controls.Add(this.tbCutsceneTriggers);
            this.tabCutscene.Controls.Add(this.label14);
            this.tabCutscene.Controls.Add(this.button2);
            this.tabCutscene.Controls.Add(this.dgvCutsceneTags);
            this.tabCutscene.Controls.Add(this.tbCutsceneName);
            this.tabCutscene.Controls.Add(this.label16);
            this.tabCutscene.Controls.Add(this.dgvCutscenes);
            this.tabCutscene.Location = new System.Drawing.Point(4, 22);
            this.tabCutscene.Name = "tabCutscene";
            this.tabCutscene.Size = new System.Drawing.Size(790, 425);
            this.tabCutscene.TabIndex = 6;
            this.tabCutscene.Text = "Cutscenes";
            this.tabCutscene.UseVisualStyleBackColor = true;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(714, 9);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(21, 13);
            this.label26.TabIndex = 58;
            this.label26.Text = "ID:";
            // 
            // tbCutsceneID
            // 
            this.tbCutsceneID.Location = new System.Drawing.Point(741, 6);
            this.tbCutsceneID.Name = "tbCutsceneID";
            this.tbCutsceneID.Size = new System.Drawing.Size(43, 20);
            this.tbCutsceneID.TabIndex = 57;
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
            this.dgvCutsceneTags.AllowUserToResizeColumns = false;
            this.dgvCutsceneTags.AllowUserToResizeRows = false;
            this.dgvCutsceneTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCutsceneTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCutsceneTags});
            this.dgvCutsceneTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvCutsceneTags.Location = new System.Drawing.Point(320, 110);
            this.dgvCutsceneTags.Name = "dgvCutsceneTags";
            this.dgvCutsceneTags.RowHeadersVisible = false;
            this.dgvCutsceneTags.Size = new System.Drawing.Size(464, 278);
            this.dgvCutsceneTags.TabIndex = 33;
            // 
            // colCutsceneTags
            // 
            this.colCutsceneTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCutsceneTags.HeaderText = "Tags";
            this.colCutsceneTags.Name = "colCutsceneTags";
            this.colCutsceneTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.dgvCutscenes.ContextMenuStrip = this.contextMenu;
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
            this.colCutscenesName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabDungeon
            // 
            this.tabDungeon.Controls.Add(this.tbDungeonDescription);
            this.tabDungeon.Controls.Add(this.label40);
            this.tabDungeon.Controls.Add(this.label38);
            this.tabDungeon.Controls.Add(this.tbDungeonID);
            this.tabDungeon.Controls.Add(this.tbDungeonName);
            this.tabDungeon.Controls.Add(this.label39);
            this.tabDungeon.Controls.Add(this.button4);
            this.tabDungeon.Controls.Add(this.dgvDungeonTags);
            this.tabDungeon.Controls.Add(this.dgvDungeons);
            this.tabDungeon.Location = new System.Drawing.Point(4, 22);
            this.tabDungeon.Name = "tabDungeon";
            this.tabDungeon.Size = new System.Drawing.Size(790, 425);
            this.tabDungeon.TabIndex = 15;
            this.tabDungeon.Text = "Dungeon";
            this.tabDungeon.UseVisualStyleBackColor = true;
            // 
            // tbDungeonDescription
            // 
            this.tbDungeonDescription.Location = new System.Drawing.Point(320, 51);
            this.tbDungeonDescription.Multiline = true;
            this.tbDungeonDescription.Name = "tbDungeonDescription";
            this.tbDungeonDescription.Size = new System.Drawing.Size(464, 53);
            this.tbDungeonDescription.TabIndex = 70;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(317, 35);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(63, 13);
            this.label40.TabIndex = 69;
            this.label40.Text = "Description:";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(714, 9);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(21, 13);
            this.label38.TabIndex = 68;
            this.label38.Text = "ID:";
            // 
            // tbDungeonID
            // 
            this.tbDungeonID.Location = new System.Drawing.Point(741, 6);
            this.tbDungeonID.Name = "tbDungeonID";
            this.tbDungeonID.Size = new System.Drawing.Size(43, 20);
            this.tbDungeonID.TabIndex = 67;
            // 
            // tbDungeonName
            // 
            this.tbDungeonName.Location = new System.Drawing.Point(361, 6);
            this.tbDungeonName.Name = "tbDungeonName";
            this.tbDungeonName.Size = new System.Drawing.Size(108, 20);
            this.tbDungeonName.TabIndex = 66;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(317, 9);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(38, 13);
            this.label39.TabIndex = 65;
            this.label39.Text = "Name:";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(709, 394);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 64;
            this.button4.Text = "Cancel";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.btnDungeonCancel_Click);
            // 
            // dgvDungeonTags
            // 
            this.dgvDungeonTags.AllowUserToResizeColumns = false;
            this.dgvDungeonTags.AllowUserToResizeRows = false;
            this.dgvDungeonTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDungeonTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDungeonsTag});
            this.dgvDungeonTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvDungeonTags.Location = new System.Drawing.Point(320, 110);
            this.dgvDungeonTags.Name = "dgvDungeonTags";
            this.dgvDungeonTags.RowHeadersVisible = false;
            this.dgvDungeonTags.Size = new System.Drawing.Size(464, 278);
            this.dgvDungeonTags.TabIndex = 63;
            // 
            // colDungeonsTag
            // 
            this.colDungeonsTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDungeonsTag.HeaderText = "Tags";
            this.colDungeonsTag.Name = "colDungeonsTag";
            this.colDungeonsTag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvDungeons
            // 
            this.dgvDungeons.AllowUserToAddRows = false;
            this.dgvDungeons.AllowUserToDeleteRows = false;
            this.dgvDungeons.AllowUserToResizeColumns = false;
            this.dgvDungeons.AllowUserToResizeRows = false;
            this.dgvDungeons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDungeons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDungeonsName});
            this.dgvDungeons.ContextMenuStrip = this.contextMenu;
            this.dgvDungeons.Location = new System.Drawing.Point(6, 6);
            this.dgvDungeons.MultiSelect = false;
            this.dgvDungeons.Name = "dgvDungeons";
            this.dgvDungeons.ReadOnly = true;
            this.dgvDungeons.RowHeadersVisible = false;
            this.dgvDungeons.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDungeons.Size = new System.Drawing.Size(308, 411);
            this.dgvDungeons.TabIndex = 62;
            this.dgvDungeons.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDungeons_CellClick);
            // 
            // colDungeonsName
            // 
            this.colDungeonsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDungeonsName.FillWeight = 90F;
            this.colDungeonsName.HeaderText = "Name";
            this.colDungeonsName.Name = "colDungeonsName";
            this.colDungeonsName.ReadOnly = true;
            this.colDungeonsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.tabItems.Text = "Item";
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
            this.dgItemTags.AllowUserToResizeColumns = false;
            this.dgItemTags.AllowUserToResizeRows = false;
            this.dgItemTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgItemTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colItemTags});
            this.dgItemTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
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
            this.colItemTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.colItemName});
            this.dgvItems.ContextMenuStrip = this.contextMenu;
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
            // colItemName
            // 
            this.colItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colItemName.FillWeight = 90F;
            this.colItemName.HeaderText = "Name";
            this.colItemName.Name = "colItemName";
            this.colItemName.ReadOnly = true;
            this.colItemName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabLight
            // 
            this.tabLight.Controls.Add(this.label9);
            this.tabLight.Controls.Add(this.tbLightID);
            this.tabLight.Controls.Add(this.tbLightName);
            this.tabLight.Controls.Add(this.label10);
            this.tabLight.Controls.Add(this.button1);
            this.tabLight.Controls.Add(this.dgvLightTags);
            this.tabLight.Controls.Add(this.dgvLights);
            this.tabLight.Location = new System.Drawing.Point(4, 22);
            this.tabLight.Name = "tabLight";
            this.tabLight.Size = new System.Drawing.Size(790, 425);
            this.tabLight.TabIndex = 14;
            this.tabLight.Text = "Light";
            this.tabLight.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(714, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 13);
            this.label9.TabIndex = 61;
            this.label9.Text = "ID:";
            // 
            // tbLightID
            // 
            this.tbLightID.Location = new System.Drawing.Point(741, 6);
            this.tbLightID.Name = "tbLightID";
            this.tbLightID.Size = new System.Drawing.Size(43, 20);
            this.tbLightID.TabIndex = 60;
            // 
            // tbLightName
            // 
            this.tbLightName.Location = new System.Drawing.Point(361, 6);
            this.tbLightName.Name = "tbLightName";
            this.tbLightName.Size = new System.Drawing.Size(108, 20);
            this.tbLightName.TabIndex = 59;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(317, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(38, 13);
            this.label10.TabIndex = 58;
            this.label10.Text = "Name:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(709, 394);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 28;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnLightCancel_Click);
            // 
            // dgvLightTags
            // 
            this.dgvLightTags.AllowUserToResizeColumns = false;
            this.dgvLightTags.AllowUserToResizeRows = false;
            this.dgvLightTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLightTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLightTags});
            this.dgvLightTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvLightTags.Location = new System.Drawing.Point(320, 33);
            this.dgvLightTags.Name = "dgvLightTags";
            this.dgvLightTags.RowHeadersVisible = false;
            this.dgvLightTags.Size = new System.Drawing.Size(464, 355);
            this.dgvLightTags.TabIndex = 27;
            // 
            // colLightTags
            // 
            this.colLightTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLightTags.HeaderText = "Tags";
            this.colLightTags.Name = "colLightTags";
            this.colLightTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvLights
            // 
            this.dgvLights.AllowUserToAddRows = false;
            this.dgvLights.AllowUserToDeleteRows = false;
            this.dgvLights.AllowUserToResizeColumns = false;
            this.dgvLights.AllowUserToResizeRows = false;
            this.dgvLights.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLights.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLightsName});
            this.dgvLights.ContextMenuStrip = this.contextMenu;
            this.dgvLights.Location = new System.Drawing.Point(6, 6);
            this.dgvLights.MultiSelect = false;
            this.dgvLights.Name = "dgvLights";
            this.dgvLights.ReadOnly = true;
            this.dgvLights.RowHeadersVisible = false;
            this.dgvLights.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLights.Size = new System.Drawing.Size(308, 411);
            this.dgvLights.TabIndex = 26;
            this.dgvLights.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLights_CellClick);
            // 
            // colLightsName
            // 
            this.colLightsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLightsName.FillWeight = 90F;
            this.colLightsName.HeaderText = "Name";
            this.colLightsName.Name = "colLightsName";
            this.colLightsName.ReadOnly = true;
            this.colLightsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabMonster
            // 
            this.tabMonster.Controls.Add(this.btnMonsterCancel);
            this.tabMonster.Controls.Add(this.dgvMonsterTags);
            this.tabMonster.Controls.Add(this.tbMonsterDescription);
            this.tabMonster.Controls.Add(this.label15);
            this.tabMonster.Controls.Add(this.label18);
            this.tabMonster.Controls.Add(this.tbMonsterID);
            this.tabMonster.Controls.Add(this.tbMonsterName);
            this.tabMonster.Controls.Add(this.label19);
            this.tabMonster.Controls.Add(this.dgvMonsters);
            this.tabMonster.Location = new System.Drawing.Point(4, 22);
            this.tabMonster.Name = "tabMonster";
            this.tabMonster.Size = new System.Drawing.Size(790, 425);
            this.tabMonster.TabIndex = 16;
            this.tabMonster.Text = "Monster";
            this.tabMonster.UseVisualStyleBackColor = true;
            // 
            // btnMonsterCancel
            // 
            this.btnMonsterCancel.Location = new System.Drawing.Point(709, 394);
            this.btnMonsterCancel.Name = "btnMonsterCancel";
            this.btnMonsterCancel.Size = new System.Drawing.Size(75, 23);
            this.btnMonsterCancel.TabIndex = 52;
            this.btnMonsterCancel.Text = "Cancel";
            this.btnMonsterCancel.UseVisualStyleBackColor = true;
            // 
            // dgvMonsterTags
            // 
            this.dgvMonsterTags.AllowUserToResizeColumns = false;
            this.dgvMonsterTags.AllowUserToResizeRows = false;
            this.dgvMonsterTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonsterTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMonsterTags});
            this.dgvMonsterTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvMonsterTags.Location = new System.Drawing.Point(320, 110);
            this.dgvMonsterTags.Name = "dgvMonsterTags";
            this.dgvMonsterTags.RowHeadersVisible = false;
            this.dgvMonsterTags.Size = new System.Drawing.Size(464, 278);
            this.dgvMonsterTags.TabIndex = 51;
            // 
            // colMonsterTags
            // 
            this.colMonsterTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMonsterTags.HeaderText = "Tags";
            this.colMonsterTags.Name = "colMonsterTags";
            this.colMonsterTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tbMonsterDescription
            // 
            this.tbMonsterDescription.Location = new System.Drawing.Point(320, 51);
            this.tbMonsterDescription.Multiline = true;
            this.tbMonsterDescription.Name = "tbMonsterDescription";
            this.tbMonsterDescription.Size = new System.Drawing.Size(464, 53);
            this.tbMonsterDescription.TabIndex = 50;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(317, 35);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(63, 13);
            this.label15.TabIndex = 49;
            this.label15.Text = "Description:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(714, 9);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(21, 13);
            this.label18.TabIndex = 48;
            this.label18.Text = "ID:";
            // 
            // tbMonsterID
            // 
            this.tbMonsterID.Location = new System.Drawing.Point(741, 6);
            this.tbMonsterID.Name = "tbMonsterID";
            this.tbMonsterID.Size = new System.Drawing.Size(43, 20);
            this.tbMonsterID.TabIndex = 47;
            // 
            // tbMonsterName
            // 
            this.tbMonsterName.Location = new System.Drawing.Point(361, 6);
            this.tbMonsterName.Name = "tbMonsterName";
            this.tbMonsterName.Size = new System.Drawing.Size(108, 20);
            this.tbMonsterName.TabIndex = 46;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(317, 9);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(38, 13);
            this.label19.TabIndex = 45;
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
            this.colMonstersName});
            this.dgvMonsters.ContextMenuStrip = this.contextMenu;
            this.dgvMonsters.Location = new System.Drawing.Point(6, 6);
            this.dgvMonsters.MultiSelect = false;
            this.dgvMonsters.Name = "dgvMonsters";
            this.dgvMonsters.ReadOnly = true;
            this.dgvMonsters.RowHeadersVisible = false;
            this.dgvMonsters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMonsters.Size = new System.Drawing.Size(308, 411);
            this.dgvMonsters.TabIndex = 44;
            this.dgvMonsters.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMonsters_CellClick);
            // 
            // colMonstersName
            // 
            this.colMonstersName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMonstersName.FillWeight = 90F;
            this.colMonstersName.HeaderText = "Name";
            this.colMonstersName.Name = "colMonstersName";
            this.colMonstersName.ReadOnly = true;
            this.colMonstersName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabShop
            // 
            this.tabShop.Controls.Add(this.label36);
            this.tabShop.Controls.Add(this.tbShopID);
            this.tabShop.Controls.Add(this.btnShopCancel);
            this.tabShop.Controls.Add(this.dgvShopTags);
            this.tabShop.Controls.Add(this.tbShopName);
            this.tabShop.Controls.Add(this.label24);
            this.tabShop.Controls.Add(this.dgvShops);
            this.tabShop.Location = new System.Drawing.Point(4, 22);
            this.tabShop.Name = "tabShop";
            this.tabShop.Size = new System.Drawing.Size(790, 425);
            this.tabShop.TabIndex = 9;
            this.tabShop.Text = "Shop";
            this.tabShop.UseVisualStyleBackColor = true;
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
            this.dgvShopTags.AllowUserToResizeColumns = false;
            this.dgvShopTags.AllowUserToResizeRows = false;
            this.dgvShopTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvShopTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colShopTags});
            this.dgvShopTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
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
            this.colShopTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.colShopsName});
            this.dgvShops.ContextMenuStrip = this.contextMenu;
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
            // colShopsName
            // 
            this.colShopsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colShopsName.FillWeight = 90F;
            this.colShopsName.HeaderText = "Name";
            this.colShopsName.Name = "colShopsName";
            this.colShopsName.ReadOnly = true;
            this.colShopsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabStatusEffect
            // 
            this.tabStatusEffect.Controls.Add(this.cbStatusEffect);
            this.tabStatusEffect.Controls.Add(this.btnStatusEffectCancel);
            this.tabStatusEffect.Controls.Add(this.dgvStatusEffectTags);
            this.tabStatusEffect.Controls.Add(this.tbStatusEffectDescription);
            this.tabStatusEffect.Controls.Add(this.label33);
            this.tabStatusEffect.Controls.Add(this.label34);
            this.tabStatusEffect.Controls.Add(this.tbStatusEffectID);
            this.tabStatusEffect.Controls.Add(this.tbStatusEffectName);
            this.tabStatusEffect.Controls.Add(this.label35);
            this.tabStatusEffect.Controls.Add(this.dgvStatusEffects);
            this.tabStatusEffect.Location = new System.Drawing.Point(4, 22);
            this.tabStatusEffect.Name = "tabStatusEffect";
            this.tabStatusEffect.Size = new System.Drawing.Size(790, 425);
            this.tabStatusEffect.TabIndex = 13;
            this.tabStatusEffect.Text = "Status Effect";
            this.tabStatusEffect.UseVisualStyleBackColor = true;
            // 
            // cbStatusEffect
            // 
            this.cbStatusEffect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbStatusEffect.FormattingEnabled = true;
            this.cbStatusEffect.Location = new System.Drawing.Point(320, 110);
            this.cbStatusEffect.Name = "cbStatusEffect";
            this.cbStatusEffect.Size = new System.Drawing.Size(149, 21);
            this.cbStatusEffect.TabIndex = 62;
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
            this.dgvStatusEffectTags.AllowUserToResizeColumns = false;
            this.dgvStatusEffectTags.AllowUserToResizeRows = false;
            this.dgvStatusEffectTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStatusEffectTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStatusEffectsTag});
            this.dgvStatusEffectTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvStatusEffectTags.Location = new System.Drawing.Point(320, 137);
            this.dgvStatusEffectTags.Name = "dgvStatusEffectTags";
            this.dgvStatusEffectTags.RowHeadersVisible = false;
            this.dgvStatusEffectTags.Size = new System.Drawing.Size(464, 251);
            this.dgvStatusEffectTags.TabIndex = 60;
            // 
            // colStatusEffectsTag
            // 
            this.colStatusEffectsTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusEffectsTag.HeaderText = "Tags";
            this.colStatusEffectsTag.Name = "colStatusEffectsTag";
            this.colStatusEffectsTag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            // colStatusEffectsName
            // 
            this.colStatusEffectsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusEffectsName.FillWeight = 90F;
            this.colStatusEffectsName.HeaderText = "Name";
            this.colStatusEffectsName.Name = "colStatusEffectsName";
            this.colStatusEffectsName.ReadOnly = true;
            this.colStatusEffectsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabTask
            // 
            this.tabTask.Controls.Add(this.cbTaskType);
            this.tabTask.Controls.Add(this.btnTaskCancel);
            this.tabTask.Controls.Add(this.dgvTaskTags);
            this.tabTask.Controls.Add(this.tbTaskDescription);
            this.tabTask.Controls.Add(this.label11);
            this.tabTask.Controls.Add(this.label12);
            this.tabTask.Controls.Add(this.tbTaskID);
            this.tabTask.Controls.Add(this.tbTaskName);
            this.tabTask.Controls.Add(this.label13);
            this.tabTask.Controls.Add(this.dgvTasks);
            this.tabTask.Location = new System.Drawing.Point(4, 22);
            this.tabTask.Name = "tabTask";
            this.tabTask.Size = new System.Drawing.Size(790, 425);
            this.tabTask.TabIndex = 5;
            this.tabTask.Text = "Tasks";
            this.tabTask.UseVisualStyleBackColor = true;
            // 
            // cbTaskType
            // 
            this.cbTaskType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTaskType.FormattingEnabled = true;
            this.cbTaskType.Location = new System.Drawing.Point(320, 110);
            this.cbTaskType.Name = "cbTaskType";
            this.cbTaskType.Size = new System.Drawing.Size(149, 21);
            this.cbTaskType.TabIndex = 37;
            // 
            // btnTaskCancel
            // 
            this.btnTaskCancel.Location = new System.Drawing.Point(709, 394);
            this.btnTaskCancel.Name = "btnTaskCancel";
            this.btnTaskCancel.Size = new System.Drawing.Size(75, 23);
            this.btnTaskCancel.TabIndex = 34;
            this.btnTaskCancel.Text = "Cancel";
            this.btnTaskCancel.UseVisualStyleBackColor = true;
            // 
            // dgvTaskTags
            // 
            this.dgvTaskTags.AllowUserToResizeColumns = false;
            this.dgvTaskTags.AllowUserToResizeRows = false;
            this.dgvTaskTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTaskTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTaskTags});
            this.dgvTaskTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvTaskTags.Location = new System.Drawing.Point(320, 137);
            this.dgvTaskTags.Name = "dgvTaskTags";
            this.dgvTaskTags.RowHeadersVisible = false;
            this.dgvTaskTags.Size = new System.Drawing.Size(464, 251);
            this.dgvTaskTags.TabIndex = 33;
            // 
            // colTaskTags
            // 
            this.colTaskTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTaskTags.HeaderText = "Tags";
            this.colTaskTags.Name = "colTaskTags";
            this.colTaskTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tbTaskDescription
            // 
            this.tbTaskDescription.Location = new System.Drawing.Point(320, 51);
            this.tbTaskDescription.Multiline = true;
            this.tbTaskDescription.Name = "tbTaskDescription";
            this.tbTaskDescription.Size = new System.Drawing.Size(464, 53);
            this.tbTaskDescription.TabIndex = 32;
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
            // tbTaskID
            // 
            this.tbTaskID.Location = new System.Drawing.Point(741, 6);
            this.tbTaskID.Name = "tbTaskID";
            this.tbTaskID.Size = new System.Drawing.Size(43, 20);
            this.tbTaskID.TabIndex = 29;
            // 
            // tbTaskName
            // 
            this.tbTaskName.Location = new System.Drawing.Point(361, 6);
            this.tbTaskName.Name = "tbTaskName";
            this.tbTaskName.Size = new System.Drawing.Size(108, 20);
            this.tbTaskName.TabIndex = 28;
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
            // dgvTasks
            // 
            this.dgvTasks.AllowUserToAddRows = false;
            this.dgvTasks.AllowUserToDeleteRows = false;
            this.dgvTasks.AllowUserToResizeColumns = false;
            this.dgvTasks.AllowUserToResizeRows = false;
            this.dgvTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTasks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTasksName});
            this.dgvTasks.ContextMenuStrip = this.contextMenu;
            this.dgvTasks.Location = new System.Drawing.Point(6, 6);
            this.dgvTasks.MultiSelect = false;
            this.dgvTasks.Name = "dgvTasks";
            this.dgvTasks.ReadOnly = true;
            this.dgvTasks.RowHeadersVisible = false;
            this.dgvTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTasks.Size = new System.Drawing.Size(308, 411);
            this.dgvTasks.TabIndex = 26;
            this.dgvTasks.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTasks_CellClick);
            // 
            // colTasksName
            // 
            this.colTasksName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTasksName.FillWeight = 90F;
            this.colTasksName.HeaderText = "Name";
            this.colTasksName.Name = "colTasksName";
            this.colTasksName.ReadOnly = true;
            this.colTasksName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabWorldObject
            // 
            this.tabWorldObject.Controls.Add(this.cbWorldObjectType);
            this.tabWorldObject.Controls.Add(this.btnWorldObjectCancel);
            this.tabWorldObject.Controls.Add(this.dgvWorldObjectTags);
            this.tabWorldObject.Controls.Add(this.label4);
            this.tabWorldObject.Controls.Add(this.tbWorldObjectID);
            this.tabWorldObject.Controls.Add(this.tbWorldObjectName);
            this.tabWorldObject.Controls.Add(this.label5);
            this.tabWorldObject.Controls.Add(this.dgvWorldObjects);
            this.tabWorldObject.Location = new System.Drawing.Point(4, 22);
            this.tabWorldObject.Name = "tabWorldObject";
            this.tabWorldObject.Padding = new System.Windows.Forms.Padding(3);
            this.tabWorldObject.Size = new System.Drawing.Size(790, 425);
            this.tabWorldObject.TabIndex = 1;
            this.tabWorldObject.Text = "WorldObj";
            this.tabWorldObject.UseVisualStyleBackColor = true;
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
            this.dgvWorldObjectTags.AllowUserToResizeColumns = false;
            this.dgvWorldObjectTags.AllowUserToResizeRows = false;
            this.dgvWorldObjectTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorldObjectTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colWorldObjectTags});
            this.dgvWorldObjectTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
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
            this.colWorldObjectTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.colWorldObjectsName});
            this.dgvWorldObjects.ContextMenuStrip = this.contextMenu;
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
            // colWorldObjectsName
            // 
            this.colWorldObjectsName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colWorldObjectsName.FillWeight = 90F;
            this.colWorldObjectsName.HeaderText = "Name";
            this.colWorldObjectsName.Name = "colWorldObjectsName";
            this.colWorldObjectsName.ReadOnly = true;
            this.colWorldObjectsName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabUpgrade
            // 
            this.tabUpgrade.Controls.Add(this.btnUpgradeCancel);
            this.tabUpgrade.Controls.Add(this.dgvUpgradeTags);
            this.tabUpgrade.Controls.Add(this.label23);
            this.tabUpgrade.Controls.Add(this.tbUpgradeID);
            this.tabUpgrade.Controls.Add(this.tbUpgradeName);
            this.tabUpgrade.Controls.Add(this.label25);
            this.tabUpgrade.Controls.Add(this.dgvUpgrades);
            this.tabUpgrade.Location = new System.Drawing.Point(4, 22);
            this.tabUpgrade.Name = "tabUpgrade";
            this.tabUpgrade.Size = new System.Drawing.Size(790, 425);
            this.tabUpgrade.TabIndex = 17;
            this.tabUpgrade.Text = "Upgrades";
            this.tabUpgrade.UseVisualStyleBackColor = true;
            // 
            // btnUpgradeCancel
            // 
            this.btnUpgradeCancel.Location = new System.Drawing.Point(709, 394);
            this.btnUpgradeCancel.Name = "btnUpgradeCancel";
            this.btnUpgradeCancel.Size = new System.Drawing.Size(75, 23);
            this.btnUpgradeCancel.TabIndex = 42;
            this.btnUpgradeCancel.Text = "Cancel";
            this.btnUpgradeCancel.UseVisualStyleBackColor = true;
            // 
            // dgvUpgradeTags
            // 
            this.dgvUpgradeTags.AllowUserToResizeColumns = false;
            this.dgvUpgradeTags.AllowUserToResizeRows = false;
            this.dgvUpgradeTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUpgradeTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUpgradeTags});
            this.dgvUpgradeTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvUpgradeTags.Location = new System.Drawing.Point(320, 33);
            this.dgvUpgradeTags.Name = "dgvUpgradeTags";
            this.dgvUpgradeTags.RowHeadersVisible = false;
            this.dgvUpgradeTags.Size = new System.Drawing.Size(464, 356);
            this.dgvUpgradeTags.TabIndex = 41;
            // 
            // colUpgradeTags
            // 
            this.colUpgradeTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colUpgradeTags.HeaderText = "Tags";
            this.colUpgradeTags.Name = "colUpgradeTags";
            this.colUpgradeTags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(714, 9);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(21, 13);
            this.label23.TabIndex = 40;
            this.label23.Text = "ID:";
            // 
            // tbUpgradeID
            // 
            this.tbUpgradeID.Location = new System.Drawing.Point(741, 6);
            this.tbUpgradeID.Name = "tbUpgradeID";
            this.tbUpgradeID.Size = new System.Drawing.Size(43, 20);
            this.tbUpgradeID.TabIndex = 39;
            // 
            // tbUpgradeName
            // 
            this.tbUpgradeName.Location = new System.Drawing.Point(361, 6);
            this.tbUpgradeName.Name = "tbUpgradeName";
            this.tbUpgradeName.Size = new System.Drawing.Size(108, 20);
            this.tbUpgradeName.TabIndex = 38;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(317, 9);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(38, 13);
            this.label25.TabIndex = 37;
            this.label25.Text = "Name:";
            // 
            // dgvUpgrades
            // 
            this.dgvUpgrades.AllowUserToAddRows = false;
            this.dgvUpgrades.AllowUserToDeleteRows = false;
            this.dgvUpgrades.AllowUserToResizeColumns = false;
            this.dgvUpgrades.AllowUserToResizeRows = false;
            this.dgvUpgrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUpgrades.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUpgradesName});
            this.dgvUpgrades.ContextMenuStrip = this.contextMenu;
            this.dgvUpgrades.Location = new System.Drawing.Point(6, 6);
            this.dgvUpgrades.MultiSelect = false;
            this.dgvUpgrades.Name = "dgvUpgrades";
            this.dgvUpgrades.ReadOnly = true;
            this.dgvUpgrades.RowHeadersVisible = false;
            this.dgvUpgrades.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUpgrades.Size = new System.Drawing.Size(308, 411);
            this.dgvUpgrades.TabIndex = 27;
            this.dgvUpgrades.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUpgrades_CellClick);
            // 
            // colUpgradesName
            // 
            this.colUpgradesName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colUpgradesName.FillWeight = 90F;
            this.colUpgradesName.HeaderText = "Name";
            this.colUpgradesName.Name = "colUpgradesName";
            this.colUpgradesName.ReadOnly = true;
            this.colUpgradesName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tbStatus
            // 
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
            this.Name = "FrmDBEditor";
            this.Text = "Database Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDBEditor_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabCtl.ResumeLayout(false);
            this.tabAction.ResumeLayout(false);
            this.tabAction.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActionTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).EndInit();
            this.tabNPC.ResumeLayout(false);
            this.tabNPC.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNPCTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNPCs)).EndInit();
            this.tabClass.ResumeLayout(false);
            this.tabClass.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClassTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClasses)).EndInit();
            this.tabCutscene.ResumeLayout(false);
            this.tabCutscene.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutsceneTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCutscenes)).EndInit();
            this.tabDungeon.ResumeLayout(false);
            this.tabDungeon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDungeonTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDungeons)).EndInit();
            this.tabItems.ResumeLayout(false);
            this.tabItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgItemTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.tabLight.ResumeLayout(false);
            this.tabLight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLightTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLights)).EndInit();
            this.tabMonster.ResumeLayout(false);
            this.tabMonster.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsterTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonsters)).EndInit();
            this.tabShop.ResumeLayout(false);
            this.tabShop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShopTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvShops)).EndInit();
            this.tabStatusEffect.ResumeLayout(false);
            this.tabStatusEffect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffectTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStatusEffects)).EndInit();
            this.tabTask.ResumeLayout(false);
            this.tabTask.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTaskTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).EndInit();
            this.tabWorldObject.ResumeLayout(false);
            this.tabWorldObject.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjectTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorldObjects)).EndInit();
            this.tabUpgrade.ResumeLayout(false);
            this.tabUpgrade.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUpgradeTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUpgrades)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
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
        private System.Windows.Forms.TabPage tabWorldObject;
        private System.Windows.Forms.ComboBox cbWorldObjectType;
        private System.Windows.Forms.Button btnWorldObjectCancel;
        private System.Windows.Forms.DataGridView dgvWorldObjectTags;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbWorldObjectID;
        private System.Windows.Forms.TextBox tbWorldObjectName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgvWorldObjects;
        private System.Windows.Forms.TabPage tabNPC;
        private System.Windows.Forms.Button btnCancelNPC;
        private System.Windows.Forms.DataGridView dgvNPCTags;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbNPCID;
        private System.Windows.Forms.TextBox tbNPCName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridView dgvNPCs;
        private System.Windows.Forms.ComboBox cbNPCType;
        private System.Windows.Forms.TabPage tabClass;
        private System.Windows.Forms.Button btnClassCancel;
        private System.Windows.Forms.DataGridView dgvClassTags;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbClassID;
        private System.Windows.Forms.TextBox tbClassName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgvClasses;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.TabPage tabTask;
        private System.Windows.Forms.Button btnTaskCancel;
        private System.Windows.Forms.DataGridView dgvTaskTags;
        private System.Windows.Forms.TextBox tbTaskDescription;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbTaskID;
        private System.Windows.Forms.TextBox tbTaskName;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.DataGridView dgvTasks;
        private System.Windows.Forms.ComboBox cbTaskType;
        private System.Windows.Forms.ComboBox cbEditableCharData;
        private System.Windows.Forms.TabPage tabCutscene;
        private System.Windows.Forms.TextBox tbCutsceneDetails;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbCutsceneTriggers;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView dgvCutsceneTags;
        private System.Windows.Forms.TextBox tbCutsceneName;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.DataGridView dgvCutscenes;
        private System.Windows.Forms.Button btnEditCutsceneDialogue;
        private System.Windows.Forms.TabPage tabAction;
        private System.Windows.Forms.Button btnActionCancel;
        private System.Windows.Forms.DataGridView dgvActionTags;
        private System.Windows.Forms.TextBox tbActionDescription;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tbActionID;
        private System.Windows.Forms.TextBox tbActionName;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.DataGridView dgvActions;
        private System.Windows.Forms.ComboBox cbActionType;
        private System.Windows.Forms.TabPage tabShop;
        private System.Windows.Forms.DataGridView dgvShopTags;
        private System.Windows.Forms.TextBox tbShopName;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.DataGridView dgvShops;
        private System.Windows.Forms.Button btnShopCancel;
        private System.Windows.Forms.TabPage tabStatusEffect;
        private System.Windows.Forms.Button btnStatusEffectCancel;
        private System.Windows.Forms.DataGridView dgvStatusEffectTags;
        private System.Windows.Forms.TextBox tbStatusEffectDescription;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox tbStatusEffectID;
        private System.Windows.Forms.TextBox tbStatusEffectName;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.DataGridView dgvStatusEffects;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox tbShopID;
        private System.Windows.Forms.TextBox tbClassDescription;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mailboxMessagesToolStripMenuItem;
        private System.Windows.Forms.TabPage tabLight;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbLightID;
        private System.Windows.Forms.TextBox tbLightName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dgvLightTags;
        private System.Windows.Forms.DataGridView dgvLights;
        private System.Windows.Forms.TabPage tabDungeon;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TextBox tbDungeonID;
        private System.Windows.Forms.TextBox tbDungeonName;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DataGridView dgvDungeonTags;
        private System.Windows.Forms.DataGridView dgvDungeons;
        private System.Windows.Forms.TextBox tbDungeonDescription;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNPCTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTaskTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCutsceneTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colShopTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusEffectsTag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLightTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDungeonsTag;
        private System.Windows.Forms.ComboBox cbStatusEffect;
        private System.Windows.Forms.TabPage tabMonster;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn colActionTags;
        private System.Windows.Forms.ToolStripMenuItem sortAndSaveToolStripMenuItem1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colActionsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNPCsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCutscenesName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDungeonsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLightsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMonstersName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colShopsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusEffectsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTasksName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorldObjectsName;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.TabPage tabUpgrade;
        private System.Windows.Forms.DataGridView dgvUpgrades;
        private System.Windows.Forms.Button btnUpgradeCancel;
        private System.Windows.Forms.DataGridView dgvUpgradeTags;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox tbUpgradeID;
        private System.Windows.Forms.TextBox tbUpgradeName;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpgradesName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpgradeTags;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox tbCutsceneID;
    }
}

