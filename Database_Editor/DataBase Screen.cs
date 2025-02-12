using Database_Editor.Classes;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using static Database_Editor.Classes.Constants;
using static RiverHollow.Utilities.Enums;

namespace Database_Editor
{
    public partial class FrmDBEditor : Form
    {
        const int ComboBoxWidth = 111;
        readonly Dictionary<string, int> _diTabIndices;

        static Dictionary<int, List<string>> _diCutscenes;
        static Dictionary<string, Dictionary<string, List<string>>> _diNPCSchedules;
        static Dictionary<string, List<XMLData>> _diNPCDialogue;
        static Dictionary<string, List<XMLData>> _diCutsceneDialogue;
        static List<XMLData> _liMailbox;
        static List<XMLData> _liGameText;
        static Dictionary<string, Dictionary<string, string>> _diObjectText;
        static Dictionary<string, List<XMLData>> _diBasicXML;
        //Dictionary<string, TMXData> _diMapData;

        Dictionary<XMLTypeEnum, XMLCollection> _diTabCollections;

        string _typeFilter = "All";
        string _subtypeFilter = "All";

        delegate void VoidDelegate();

        public FrmDBEditor()
        {
            InitializeComponent();
            CreateNewTabs();
            SetupTabCollections();

            InitComboBox<ItemTypeEnum>(FindComboBox(XMLTypeEnum.Item, ComponentTypeEnum.ComboBoxType));
            InitComboBox<ObjectTypeEnum>(FindComboBox(XMLTypeEnum.WorldObject, ComponentTypeEnum.ComboBoxType));
            InitComboBox<TaskTypeEnum>(FindComboBox(XMLTypeEnum.Task, ComponentTypeEnum.ComboBoxType));
            InitComboBox<EditableNPCDataEnum>(FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxEditable), false);
            InitComboBox<StatusTypeEnum>(FindComboBox(XMLTypeEnum.StatusEffect, ComponentTypeEnum.ComboBoxType));
            InitComboBox<UpgradeTypeEnum>(FindComboBox(XMLTypeEnum.Upgrade, ComponentTypeEnum.ComboBoxType));
            InitComboBox<CosmeticSlotEnum>(FindComboBox(XMLTypeEnum.Cosmetic, ComponentTypeEnum.ComboBoxType));

            var cbActor = FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxType);
            cbActor.Items.Clear();
            InitComboBox<ActorTypeEnum>(cbActor, true);

            _diTabIndices = new Dictionary<string, int>() { { "PreviousTab", 0 } };

            foreach(XMLTypeEnum e in Enum.GetValues(typeof(XMLTypeEnum)))
            {
                if(IsDataFile(e))
                {
                    _diTabIndices[GetTabIndexName(e)] = 0;
                }
            }

            //_diMapData = new Dictionary<string, TMXData>();
            //foreach (string s in Directory.GetDirectories(PATH_TO_MAPS))
            //{
            //    foreach (string mapName in Directory.GetFiles(s))
            //    {
            //        if (mapName.EndsWith(".tmx"))
            //        {
            //            _diMapData[mapName] = new TMXData(mapName);
            //        }
            //    }
            //}

            _diNPCDialogue = new Dictionary<string, List<XMLData>>();
            string[] dialogueFiles = Directory.GetFiles(PATH_TO_VILLAGER_DIALOGUE);
            for (int i = 0; i < dialogueFiles.Length; i++)
            {
                LoadXMLDictionary(dialogueFiles[i], TEXTFILE_REF_TAGS, "", ref _diNPCDialogue);
            }

            _diCutsceneDialogue = new Dictionary<string, List<XMLData>>();
            dialogueFiles = Directory.GetFiles(PATH_TO_CUTSCENE_DIALOGUE);
            for (int i = 0; i < dialogueFiles.Length; i++)
            {
                LoadXMLDictionary(dialogueFiles[i], CUTSCENE_REF_TAGS, "", ref _diCutsceneDialogue);
            }

            _liGameText = LoadXMLList(PATH_TO_TEXT_FILES + @"\GameText.xml", TEXTFILE_REF_TAGS, "");
            _liMailbox = LoadXMLList(PATH_TO_TEXT_FILES + @"\Mailbox_Text.xml", TEXTFILE_REF_TAGS, "");

            _diNPCSchedules = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (string s in Directory.GetFiles(PATH_TO_SCHEDULES))
            {
                string fileName = Path.GetFileName(s).Replace("NPC_", "").Split('.')[0];
                int charID = -1;
                if (int.TryParse(fileName, out charID))
                {
                    fileName = s;
                    Util.ParseContentFile(ref fileName);
                    _diNPCSchedules.Add(s, ReadXMLFileToStringKeyDictionaryStringList(fileName));//ReadXMLFilToDictionary(fileName));
                }
            }

            _diObjectText = ReadTaggedXMLFile(OBJECT_TEXT_XML_FILE);

            _diBasicXML = new Dictionary<string, List<XMLData>>();
            LoadXMLDictionary(CONFIG_XML_FILE, CONFIG_REF_TAG, "", ref _diBasicXML);
            foreach (XMLTypeEnum en in Enum.GetValues(typeof(XMLTypeEnum)))
            {
                if (IsDataFile(en) && en != XMLTypeEnum.Cutscene)
                {
                    LoadXMLDictionary(GetXMLDataFileName(en), GetRefTags(en), GetTagsForRef(en), ref _diBasicXML);
                }
            }

            _diCutscenes = ReadXMLFileToIntKeyDictionaryStringList(CUTSCENE_XML_FILE);

            FindLinkedXMLObjects();

            LoadDataGrids();
            LoadAllInfoPanels();
        }

        #region Dynamic Forms
        private void CreateNewTabs()
        {
            foreach (XMLTypeEnum en in Enum.GetValues(typeof(XMLTypeEnum)))
            {
                if (!IsDataFile(en))
                {
                    continue;
                }

                int counter = 0;
                bool hasDescription = HasDescription(en);
                string enumStr = en.ToString();

                var tabPage = new TabPage();
                this.tabCtl.Controls.Add(tabPage);

                //tabPage Setup
                tabPage.Location = new System.Drawing.Point(4, 22);
                tabPage.Name = "tab" + enumStr;
                tabPage.Size = new System.Drawing.Size(790, 425);
                tabPage.TabIndex = 2;
                tabPage.Text = en.ToString() + "s";
                tabPage.UseVisualStyleBackColor = true;

                var dgv = CreateDataGridView(GetName(en, ComponentTypeEnum.DataGrid), "Name", GetName(en, ComponentTypeEnum.ColumnName), new Point(6, 6), new Size(308, 411), true);
                dgv.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(ProcessCellClick);
                dgv.ContextMenuStrip = contextMenu;
                dgv.MultiSelect = false;
                dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                tabPage.Controls.Add(dgv);

                var lblName = CreateLabel("label" + counter++, "Name:", new Point(317, 9), new Size(38, 13), true);
                var tbName = CreateTextBox(GetName(en, ComponentTypeEnum.TextBoxName), new Point(361, 6), new Size(108, 20));
                tabPage.Controls.Add(lblName);
                tabPage.Controls.Add(tbName);

                var lblID = CreateLabel("label" + counter++, "ID:", new Point(714, 9), new Size(21, 13), true);
                var tbID = CreateTextBox(GetName(en, ComponentTypeEnum.TextBoxID), new Point(741, 6), new Size(43, 20));
                tabPage.Controls.Add(lblID);
                tabPage.Controls.Add(tbID);

                if (hasDescription)
                {
                    var lblDesc = CreateLabel("label" + counter++, "Description:", new Point(317, 35), new Size(63, 13), true);
                    var tbDesc = CreateTextBox(GetName(en, ComponentTypeEnum.TextBoxDescription), new Point(320, 51), new Size(464, 53), true);
                    tabPage.Controls.Add(lblDesc);
                    tabPage.Controls.Add(tbDesc);
                }

                Point dgvTagLocation;
                Size dgvTagSize;
                if (en == XMLTypeEnum.Cutscene)
                {
                    dgvTagLocation = new Point(320, 110);
                    dgvTagSize = new Size(464, 278);

                    var lblTrigger = CreateLabel("label" + counter++, "Triggers:", new Point(317, 29), new Size(48, 13), true);
                    var tbTrigger = CreateTextBox(GetName(en, ComponentTypeEnum.TextBoxTriggers), new Point(320, 45), new Size(464, 20));
                    tabPage.Controls.Add(lblTrigger);
                    tabPage.Controls.Add(tbTrigger);

                    var lblDetail = CreateLabel("label" + counter++, "Details:", new Point(320, 68), new Size(42, 13), true);
                    var tbDetail = CreateTextBox(GetName(en, ComponentTypeEnum.TextBoxDetails), new Point(320, 84), new Size(464, 20));
                    tabPage.Controls.Add(lblDetail);
                    tabPage.Controls.Add(tbDetail);
                }
                else
                {
                    int numComboBox = GetNumberComboBoxes(en);

                    for (int i = 0; i < numComboBox; i++)
                    {
                        int cbHeight = hasDescription ? 110 : 33;

                        ComponentTypeEnum type = ComponentTypeEnum.ComboBoxType;

                        if (i == 1) { type = ComponentTypeEnum.ComboBoxSubtype; }
                        else if (i == 2) { type = ComponentTypeEnum.ComboBoxGroup; }
                        else if (i == 3) { type = ComponentTypeEnum.ComboBoxSubGroup; }

                        var cbType = CreateComboBox(GetName(en, type), new Point(320 + (i * (ComboBoxWidth + 6)), cbHeight), type == ComponentTypeEnum.ComboBoxType);

                        if (i == 0)
                        {
                            switch (en)
                            {
                                case XMLTypeEnum.Actor:
                                    cbType.SelectedIndexChanged += new System.EventHandler(cbActorType_SelectedIndexChanged);
                                    break;
                                case XMLTypeEnum.Item:
                                    cbType.SelectedIndexChanged += new System.EventHandler(cbItemType_SelectedIndexChanged);
                                    break;
                                case XMLTypeEnum.WorldObject:
                                    cbType.SelectedIndexChanged += new System.EventHandler(cbWorldObjectType_SelectedIndexChanged);
                                    break;
                            }
                        }
                        tabPage.Controls.Add(cbType);
                    }

                    if (hasDescription)
                    {
                        if (numComboBox == 0)
                        {
                            dgvTagLocation = new Point(320, 110);
                            dgvTagSize = new Size(464, 278);
                        }
                        else
                        {
                            dgvTagLocation = new Point(320, 137);
                            dgvTagSize = new Size(464, 251);
                        }
                    }
                    else
                    {
                        if (numComboBox == 0)
                        {
                            dgvTagLocation = new Point(320, 33);
                            dgvTagSize = new Size(464, 355);
                        }
                        else
                        {
                            dgvTagLocation = new Point(320, 60);
                            dgvTagSize = new Size(464, 328);
                        }
                    }
                }

                var dgvTags = CreateDataGridView(GetName(en, ComponentTypeEnum.DataGridTags), "Tags", GetName(en, ComponentTypeEnum.ColumnTags), dgvTagLocation, dgvTagSize, false);
                dgvTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
                tabPage.Controls.Add(dgvTags);

                var cancelBtn = CreateButton(GetName(en, ComponentTypeEnum.ButtonCancel), "Cancel", new Point(709, 394), new Size(75, 23));
                cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                cancelBtn.Click += new System.EventHandler(ProcessCancel_Click);
                tabPage.Controls.Add(cancelBtn);

                if (en == XMLTypeEnum.Actor)
                {
                    var cbType = CreateComboBox(GetName(en, ComponentTypeEnum.ComboBoxEditable), new Point(320, 396), true);
                    cbType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                    tabPage.Controls.Add(cbType);

                    var editBtn = CreateButton(GetName(en, ComponentTypeEnum.ButtonEdit), "Edit", new Point(434, 395), new Size(75, 23));
                    editBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                    editBtn.Click += new System.EventHandler(btnDialogue_Click);
                    tabPage.Controls.Add(editBtn);
                }
            }
        }
        public bool HasDescription(XMLTypeEnum en)
        {
            switch(en)
            {
                case XMLTypeEnum.Cutscene:
                case XMLTypeEnum.Light:
                case XMLTypeEnum.Shop:
                case XMLTypeEnum.WorldObject:
                    return false;
                default:
                    return true;
            }
        }

        private int GetNumberComboBoxes(XMLTypeEnum en)
        {
            switch (en)
            {
                case XMLTypeEnum.Item:
                    return 3;
                case XMLTypeEnum.Actor:
                    return 4;
                case XMLTypeEnum.WorldObject:
                    return 2;
                case XMLTypeEnum.Cosmetic:
                case XMLTypeEnum.StatusEffect:
                case XMLTypeEnum.Task:
                case XMLTypeEnum.Upgrade:
                    return 1;
                default:
                    return 0;
            }
        }

        private Label CreateLabel(string name, string text, Point location, Size size, bool autosize)
        {
            var obj = new Label
            {
                AutoSize = autosize,
                Location = location,
                Name = name,
                Size = size,
                TabIndex = 38,
                Text = text
            };

            return obj;
        }

        private TextBox CreateTextBox(string name, Point location, Size size, bool multiline = false)
        {
            var obj = new TextBox
            {
                Location = location,
                Multiline = multiline,
                Name = name,
                Size = size,
                TabIndex = 38,
            };

            return obj;
        }

        private ComboBox CreateComboBox(string name, Point location, bool visible)
        {
            var obj = new ComboBox
            {
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Location = location,
                Name = name,
                Size = new Size(ComboBoxWidth, 21),
                TabIndex = 60,
                Visible = visible
            };

            return obj;
        }

        private DataGridView CreateDataGridView(string name, string header, string columnName, Point location, Size size, bool readOnly)
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = !readOnly,
                AllowUserToDeleteRows = !readOnly,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left))),
                ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = location,
                MultiSelect = false,
                Name = name,
                ReadOnly = readOnly,
                RowHeadersVisible = false,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                Size = size,
            };

            var column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = header,
                Name = columnName,
                ReadOnly = readOnly,
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
            };

            if (readOnly)
            {
                column.FillWeight = 90F;
            }

            dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { column });

            return dgv;
        }

        private Button CreateButton(string name, string text, Point location, Size size)
        {
            var obj = new Button
            {
                Location = location,
                Name = name,
                Size = size,
                TabIndex = 44,
                Text = text,
                UseVisualStyleBackColor = true
            };

            return obj;
        }
        #endregion

        private void SetupTabCollections()
        {
            _diTabCollections = new Dictionary<XMLTypeEnum, XMLCollection>();

            foreach (XMLTypeEnum en in Enum.GetValues(typeof(XMLTypeEnum)))
            {
                if (IsDataFile(en))
                {
                    _diTabCollections[en] = new XMLCollection(en, GetRefTags(en), GetTagsForRef(en));
                }
            }
        }

        //private Dictionary<string, string> ReadXMLFileToDictionary(string fileName)
        //{
        //    string line = string.Empty;
        //    Dictionary<string, string> xmlDictionary = new Dictionary<string, string>();

        //    XmlDocument xmldoc = new XmlDocument();
        //    XmlNodeList xmlnode;
        //    int i = 0;

        //    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        //    xmldoc.Load(fs);
        //    xmlnode = xmldoc.GetElementsByTagName("Item");
        //    for (i = 0; i <= xmlnode.Count - 1; i++)
        //    {
        //        //MAR
        //        //Dictionary<string, string> diTags = new Dictionary<string, string>();
        //        //Util.DictionaryFromTaggedString(ref diTags, xmlnode[i].ChildNodes.Item(1).InnerText.Trim());
        //        //xmlDictionary[xmlnode[i].ChildNodes.Item(0).InnerText] = ;
        //    }

        //    return xmlDictionary;
        //}
        private Dictionary<int, List<XMLData>> ReadXMLFileToXMLDataListDictionary(string fileName, XMLTypeEnum typeEnum, string refTags, string tagsThatRefertoMe)
        {
            string line = string.Empty;
            Dictionary<int, List<XMLData>> xmlDictionary = new Dictionary<int, List<XMLData>>();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlNodeList;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlNodeList = xmldoc.ChildNodes;
            XmlNode node = xmlNodeList[1].ChildNodes[0];

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                int dataIndex = 0;
                int key = -1;
                List<XMLData> tagList = new List<XMLData>();
                XmlNode n = node.ChildNodes[i];
                if (n.Name == "Item")
                {
                    XmlNode keyNode = n.ChildNodes[0];
                    if (keyNode.Name == "Key")
                    {
                        key = int.Parse(keyNode.InnerText);

                        XmlNode tagNode = n.ChildNodes[1];
                        foreach (XmlNode n1 in tagNode.ChildNodes)
                        {
                            if (n1.Name == "Item" && !string.IsNullOrEmpty(n1.InnerText))
                            {
                                dataIndex++;
                                XMLTypeEnum identifier = Util.ParseEnum<XMLTypeEnum>(DataManager.TaggedStringToDictionary(n1.InnerText)["Type"]);
                                XMLData data = new XMLData(dataIndex.ToString(), n1.InnerText, refTags, tagsThatRefertoMe, identifier, ref _diObjectText);
                                tagList.Add(data);
                            }
                        }
                    }

                    xmlDictionary[key] = tagList;
                }
            }

            return xmlDictionary;
        }
        private Dictionary<int, List<string>> ReadXMLFileToIntKeyDictionaryStringList(string fileName)
        {
            string line = string.Empty;
            Dictionary<int, List<string>> xmlDictionary = new Dictionary<int, List<string>>();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlNodeList;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlNodeList = xmldoc.ChildNodes;
            XmlNode node = xmlNodeList[1].ChildNodes[0];

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                int key = -1;
                List<string> tagList = new List<string>();
                XmlNode n = node.ChildNodes[i];
                if (n.Name == "Item")
                {
                    XmlNode keyNode = n.ChildNodes[0];
                    if (keyNode.Name == "Key")
                    {
                        key = int.Parse(keyNode.InnerText);

                        XmlNode tagNode = n.ChildNodes[1];
                        foreach (XmlNode n1 in tagNode.ChildNodes)
                        {
                            if (n1.Name == "Item")
                            {
                                tagList.Add(n1.InnerText);
                            }
                        }
                    }
                    xmlDictionary[key] = tagList;
                }
            }

            return xmlDictionary;
        }
        private Dictionary<string, List<string>> ReadXMLFileToStringKeyDictionaryStringList(string fileName)
        {
            string line = string.Empty;
            Dictionary<string, List<string>> xmlDictionary = new Dictionary<string, List<string>>();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlNodeList;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlNodeList = xmldoc.ChildNodes;
            XmlNode node = xmlNodeList[1].ChildNodes[0];

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                string key = string.Empty;
                List<string> tagList = new List<string>();
                XmlNode n = node.ChildNodes[i];
                if (n.Name == "Item")
                {
                    XmlNode keyNode = n.ChildNodes[0];
                    if (keyNode.Name == "Key")
                    {
                        key = keyNode.InnerText;

                        XmlNode tagNode = n.ChildNodes[1];
                        foreach (XmlNode n1 in tagNode.ChildNodes)
                        {
                            if (n1.Name == "Item")
                            {
                                tagList.Add(n1.InnerText);
                            }
                        }
                    }
                    xmlDictionary[key] = tagList;
                }
            }

            return xmlDictionary;
        }
        private Dictionary<string, Dictionary<string, string>> ReadTaggedXMLFile(string fileName)
        {
            Dictionary<string, Dictionary<string, string>> xmlDictionary = new Dictionary<string, Dictionary<string, string>>();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("Item");
            for (int i = 0; i <= xmlnode.Count - 1; i++)
            {
                string tags = xmlnode[i].ChildNodes.Item(1).InnerText.Trim();
                string[] split = tags.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, string> tagDictionary = new Dictionary<string, string>();
                foreach (string s in split)
                {
                    string[] kvp = s.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    tagDictionary[kvp[0]] = kvp.Length > 1 ? kvp[1] : string.Empty;
                }

                xmlDictionary[xmlnode[i].ChildNodes.Item(0).InnerText] = tagDictionary;
            }

            return xmlDictionary;
        }

        private void LoadDialogueDictionary(string path, string fileMatch, ref Dictionary<string, Dictionary<string, Dictionary<string, string>>> dialogueDictionary)
        {
            dialogueDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            foreach (string s in Directory.GetFiles(path))
            {
                string fileNameID = Path.GetFileName(s).Replace(fileMatch, "").Split('.')[0];

                int charID = -1;
                if (int.TryParse(fileNameID, out charID))
                {
                    fileNameID = s;
                    Util.ParseContentFile(ref fileNameID);
                    dialogueDictionary.Add(s, ReadTaggedXMLFile(fileNameID));
                }
            }
        }

        private List<XMLData> LoadXMLList(string fileName, string tagsReferenced, string tagsThatReferenceMe)
        {
            List<XMLData> data = new List<XMLData>();
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in ReadTaggedXMLFile(fileName))
            {
                data.Add(new XMLData(kvp.Key, kvp.Value, tagsReferenced, tagsThatReferenceMe, FileNameToXMLType(fileName), ref _diObjectText));
            }
            return data;
        }
        private void LoadXMLDictionary(string fileName, string tagsReferenced, string tagsThatReferenceMe, ref Dictionary<string, List<XMLData>> dictionary)
        {
            dictionary[fileName] = LoadXMLList(fileName, tagsReferenced, tagsThatReferenceMe);
        }

        #region Find Linked Objects
        /// <summary>
        /// This iterates through all the ItemData entries and compares them against
        /// the other objects, ensuring that eachobject knows about any object that
        /// rfeferences it.
        /// </summary>
        /// <param name="dataList"></param>
        private void FindLinkedXMLObjects()
        {
            var items = _diBasicXML[ITEM_XML_FILE];
            var worldObjects = _diBasicXML[WORLD_OBJECTS_XML_FILE];
            //Compare item Data against the other Items
            for (int i = 0; i < items.Count; i++)
            {
                XMLData theData = items[i];
                for (int j = 0; j < items.Count; j++)
                {
                    items[j].CheckForObjectLink(theData);
                }
                FindLinkedXMLObjects(theData, _liMailbox);
                FindLinkedXMLObjectsInDictionary(theData, _diBasicXML);
                FindLinkedXMLObjectsInDictionary(theData, _diNPCDialogue);

                //Compare ItemData against the WorldObjectData

                foreach (XMLData testIt in worldObjects)
                {
                    //First, check to see if the object refers to the item this
                    //could be because the object makes the item for example
                    testIt.CheckForObjectLink(theData);

                    //Next check to see if the item refers to the object, pass in
                    //false here to ensure that we compare only to the worldObject tags
                    //The item might place the object.
                    theData.CheckForObjectLink(testIt);
                }

                FindLinkedTMXObjects(theData);
            }

            foreach (XMLData theData in worldObjects)
            {
                FindLinkedXMLObjects(theData, _liMailbox);
                FindLinkedXMLObjects(theData, worldObjects);

                FindLinkedXMLObjectsInDictionary(theData, _diBasicXML);
                FindLinkedXMLObjectsInDictionary(theData, _diNPCDialogue);

                FindLinkedTMXObjects(theData);
            }

            foreach (string baseFile in _diBasicXML.Keys)
            {
                foreach (XMLData theData in _diBasicXML[baseFile])
                {
                    //Find any files that reference the ObjectID
                    foreach (string comparatorFile in _diBasicXML.Keys)
                    {
                        if (!baseFile.Equals(comparatorFile))
                        {
                            FindLinkedXMLObjects(theData, _diBasicXML[comparatorFile]);
                        }
                    }

                    FindLinkedXMLObjects(theData, _liMailbox);
                    FindLinkedXMLObjectsInDictionary(theData, _diNPCDialogue);

                    FindLinkedTMXObjects(theData);

                    FindLinkedCutscenes(theData, ref _diCutscenes);
                }
            }
        }
        private void FindLinkedXMLObjectsInDictionary(XMLData theData, Dictionary<string, List<XMLData>> dictionaryData)
        {
            foreach (string s in dictionaryData.Keys)
            {
                FindLinkedXMLObjects(theData, dictionaryData[s]);
            }
        }
        private void FindLinkedXMLObjects(XMLData theData, List<XMLData> dataList)
        {
            foreach (XMLData testIt in dataList)
            {
                if (testIt != theData)
                {
                    testIt.CheckForObjectLink(theData);
                }
            }
        }
        private void FindLinkedTMXObjects(XMLData data)
        {
            //Find any maps that reference the ItemID
            //foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
            //{
            //    kvp.Value.ReferencesXMLObject(data);
            //}
        }
        private void FindLinkedCutscenes(XMLData theData, ref Dictionary<int, List<string>> dictionaryData)
        {
            foreach (KeyValuePair<int, List<string>> kvp in dictionaryData)
            {
                bool weDone = false;
                foreach (string s in kvp.Value)
                {
                    foreach (string tag in theData.TagsThatReferToMe)
                    {
                        if (s.Contains(tag + ":" + theData.ID + "-") || s.Contains(tag + ":" + theData.ID + "]"))
                        {
                            theData.AddLinkedCutscene(dictionaryData[kvp.Key]);
                            weDone = true;
                            break;
                        }
                    }
                    if (weDone) { break; }
                }
            }
        }
        #endregion

        private void ChangeIDs(ref List<XMLData> itemDataList, ref List<XMLData> worldObjectDataList)
        {
            //Change all IDs
            int index = 0;

            foreach (XMLData data in _diBasicXML[ITEM_XML_FILE])
            {
                if (data != null)
                {
                    data.ChangeID(index++);
                    itemDataList.Add(data);
                }
            }

            index = 0;
            foreach (XMLData data in _diBasicXML[WORLD_OBJECTS_XML_FILE])
            {
                data.ChangeID(index++, false);
                worldObjectDataList.Add(data);
            }
        }

        private void SortDictionaryByType(ref List<XMLData> xmlDataDictionary)
        {
            xmlDataDictionary.Sort((x, y) =>
            {
                var typeComp = y.GetTagValue("Type").CompareTo(x.GetTagValue("Type"));
                if (typeComp == 0) { return x.Name.CompareTo(y.Name); }
                else { return typeComp; }
            });

            int index = 0;
            foreach (XMLData data in xmlDataDictionary)
            {
                data.ChangeID(index++, false);
            }
        }

        private void SortDictionaryByName(ref List<XMLData> xmlDataDictionary)
        {
            xmlDataDictionary.Sort((x, y) =>
            {
                var typeComp = x.Name.CompareTo(y.Name);
                if (typeComp == 0) { return x.ID.CompareTo(y.ID); }
                else { return typeComp; }
            });

            int index = 0;
            foreach (XMLData data in xmlDataDictionary)
            {
                data.ChangeID(index++, false);
            }
        }

        private void SelectRow(DataGridView dg, int id)
        {
            if (dg.Rows.Count > id)
            {
                dg.Rows[id].Selected = true;
                dg.CurrentCell = dg.Rows[id].Cells[0];
            }
        }

        private void SetItemSubtype()
        {
            var cbItemType = FindComboBox(XMLTypeEnum.Item, ComponentTypeEnum.ComboBoxType);
            var cbItemSubtype = FindComboBox(XMLTypeEnum.Item, ComponentTypeEnum.ComboBoxSubtype);
            var cbItemGroup = FindComboBox(XMLTypeEnum.Item, ComponentTypeEnum.ComboBoxGroup);

            cbItemSubtype.Items.Clear();
            cbItemGroup.Items.Clear();

            cbItemSubtype.Visible = false;
            cbItemGroup.Visible = false;

            ItemTypeEnum itemType = Util.ParseEnum<ItemTypeEnum>(cbItemType.SelectedItem.ToString().Split(':')[1]);
            switch (itemType)
            {
                case ItemTypeEnum.Merchandise:
                    ItemComboBoxHelper<MerchandiseTypeEnum>(ref cbItemSubtype);
                    ItemComboBoxHelper<ClassTypeEnum>(ref cbItemGroup);
                    break;
                case ItemTypeEnum.Food:
                    cbItemSubtype.Items.Add("Subtype:" + ResourceTypeEnum.Food.ToString());
                    cbItemSubtype.Items.Add("Subtype:" + ResourceTypeEnum.Meal.ToString());
                    break;
                case ItemTypeEnum.NPCToken:
                    ItemComboBoxHelper<NPCTokenTypeEnum>(ref cbItemSubtype);
                    break;
                case ItemTypeEnum.Resource:
                    ItemComboBoxHelper(ref cbItemSubtype, new List<ResourceTypeEnum>() { ResourceTypeEnum.Food, ResourceTypeEnum.Meal });
                    break;
                case ItemTypeEnum.Tool:
                    ItemComboBoxHelper<ToolEnum>(ref cbItemSubtype);
                    break;
            }

            if (cbItemSubtype.Visible)
            {
                cbItemSubtype.SelectedIndex = 0;
            }

            if (cbItemGroup.Visible)
            {
                cbItemGroup.SelectedIndex = 0;
            }
        }

        private void ItemComboBoxHelper<T>(ref ComboBox cb, List<T> skip = null)
        {
            var cbItemSubtype = FindComboBox(XMLTypeEnum.Item, ComponentTypeEnum.ComboBoxSubtype);
            cb.Visible = true;
            foreach (T en in Enum.GetValues(typeof(T)))
            {
                if (skip != null && skip.Contains(en))
                {
                    continue;
                }
                else
                {
                    string prefix = (cb == cbItemSubtype) ? "Subtype" : "Group";
                    cb.Items.Add(prefix + ":" + en.ToString());
                }
            }
        }

        private void SetActorSubtype()
        {
            var cbActorType = FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxType);
            var cbActorSubtype = FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxSubtype);
            var cbActorGroup = FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxGroup);
            var cbActorSubGroup = FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxSubGroup);

            cbActorSubtype.Items.Clear();
            cbActorGroup.Items.Clear();
            cbActorSubGroup.Items.Clear();

            cbActorSubtype.Visible = false;
            cbActorGroup.Visible = false;
            cbActorSubGroup.Visible = false;

            ActorTypeEnum itemType = Util.ParseEnum<ActorTypeEnum>(cbActorType.SelectedItem.ToString().Split(':')[1]);
            switch (itemType)
            {
                case ActorTypeEnum.Mob:
                    cbActorSubtype.Visible = true;
                    foreach (MobTypeEnum e in Enum.GetValues(typeof(MobTypeEnum)))
                    {
                        cbActorSubtype.Items.Add("Subtype:" + e.ToString());
                    }
                    break;
                case ActorTypeEnum.Traveler:
                    cbActorSubtype.Visible = true;
                    foreach (TravelerGroupEnum e in Enum.GetValues(typeof(TravelerGroupEnum)))
                    {
                        cbActorSubtype.Items.Add("Subtype:" + e.ToString());
                    }
                    cbActorGroup.Visible = true;
                    foreach (ClassTypeEnum e in Enum.GetValues(typeof(ClassTypeEnum)))
                    {
                        cbActorGroup.Items.Add("Group:" + e.ToString());
                    }
                    cbActorSubGroup.Visible = true;
                    foreach (AffinityEnum e in Enum.GetValues(typeof(AffinityEnum)))
                    {
                        cbActorSubGroup.Items.Add("SubGroup:" + e.ToString());
                    }
                    break;
            }

            //if (cbActorSubtype.Visible) { cbActorSubtype.SelectedIndex = 0; }
           // if (cbActorGroup.Visible) { cbActorGroup.SelectedIndex = 0; }
            //if (cbActorSubGroup.Visible) { cbActorSubGroup.SelectedIndex = 0; }

        }

        #region Helpers
        private bool IsDataFile(XMLTypeEnum en)
        {
            return en != XMLTypeEnum.None && en != XMLTypeEnum.TextFile;
        }
        private void InitComboBox<T>(ComboBox cb, bool type = true, List<T> skip = null)
        {
            cb.Items.Clear();
            foreach (T e in Enum.GetValues(typeof(T)))
            {
                if (skip == null || (skip != null && !skip.Contains(e)))
                {
                    cb.Items.Add((type ? "Type:" : "") + e.ToString());
                }
            }
            cb.SelectedIndex = 0;
        }

        private XMLTypeEnum FileNameToXMLType(string fileName)
        {
            XMLTypeEnum rv;

            if (fileName.Contains("Text Files"))
            {
                rv = XMLTypeEnum.TextFile;
            }
            else
            {
                var strippedFile = Path.GetFileNameWithoutExtension(fileName);
                strippedFile = strippedFile.Replace("Data", "");
                rv = Util.ParseEnum<XMLTypeEnum>(strippedFile);
            }

            return rv;
        }

        public string GetTextValue(XMLTypeEnum xmlType, int id, string key)
        {
            string rv = string.Empty;
            string textID = Util.GetEnumString(xmlType) + "_" + id;
            if (xmlType != XMLTypeEnum.None)
            {
                if (_diObjectText.ContainsKey(textID) && _diObjectText[textID].ContainsKey(key))
                {
                    rv = _diObjectText[textID][key];
                }
            }

            return rv;
        }

        public void UpdateTextValue(XMLTypeEnum xmlType, int id, string key, string newValue)
        {
            string textID = Util.GetEnumString(xmlType) + "_" + id;
            if (!_diObjectText.ContainsKey(textID)) { _diObjectText[textID] = new Dictionary<string, string>(); }
            _diObjectText[textID][key] = newValue;
        }
        #endregion

        #region DataGridView Loading
        private bool TypeFilterCheck(XMLData data)
        {
            return _typeFilter == "All" || data.GetTagValue("Type").ToString().Equals(_typeFilter);
        }
        private bool SubTypeFilterCheck(XMLData data)
        {
            return _subtypeFilter == "All" || data.GetTagValue("Subtype").ToString().Equals(_subtypeFilter);
        }
        private void LoadDataGrids()
        {
            foreach (XMLTypeEnum en in Enum.GetValues(typeof(XMLTypeEnum)))
            {
                if (IsDataFile(en))
                {
                    LoadDataGrid(en, GetTabIndex(en));
                }
            }
        }

        private void LoadDataGrid(XMLTypeEnum en, int selectRow = -1, string filter = "All")
        {
            switch (en)
            {
                case XMLTypeEnum.TextFile:
                    return;
                case XMLTypeEnum.Cutscene:
                    LoadCutsceneDataGrid();
                    break;
                default:
                    LoadGenericDatagrid(_diTabCollections[en], GetDataList(en), selectRow == -1 ? GetTabIndex(en) : selectRow, filter);
                    break;
            }
        }

        private void LoadGenericDatagrid(XMLCollection collection, List<XMLData> data, int selectRow, string filter = "All")
        {
            string colName = GetName(collection.XMLType, ComponentTypeEnum.ColumnName);
            DataGridView dgv = FindDataGridView(collection.XMLType, ComponentTypeEnum.DataGrid);

            dgv.Rows.Clear();
            int index = 0;
            _typeFilter = filter;
            for (int i = 0; i < data.Count; i++)
            {
                if (TypeFilterCheck(data[i]) && SubTypeFilterCheck(data[i]))
                {
                    dgv.Rows.Add();
                    DataGridViewRow row = dgv.Rows[index++];

                    //row.Cells[colID].Value = data[i].ID;
                    row.Cells[colName].Value = data[i].Name;
                }
            }

            SelectRow(dgv, selectRow);
            dgv.Focus();
        }

        private void LoadCutsceneDataGrid()
        {
            var dgv = FindDataGridView(XMLTypeEnum.Cutscene, ComponentTypeEnum.DataGrid);
            dgv.Rows.Clear();
            foreach (KeyValuePair<int, List<string>> kvp in _diCutscenes)
            {
                dgv.Rows.Add();

                DataGridViewRow row = dgv.Rows[kvp.Key];
                //row.Cells["colCutscenesID"].Value = kvp.Key;
                row.Cells["colCutscenesName"].Value = GetTextValue(XMLTypeEnum.Cutscene, kvp.Key, "Name");
            }

            SelectRow(dgv, _diTabIndices["Cutscenes"]);
            dgv.Focus();
        }

        private void LoadDictionaryListDatagrid(DataGridView dgv, Dictionary<int, List<XMLData>> di, string colID, string colName, string tabIndex, XMLTypeEnum xmlType)
        {
            dgv.Rows.Clear();
            foreach (KeyValuePair<int, List<XMLData>> kvp in di)
            {
                dgv.Rows.Add();

                DataGridViewRow row = dgv.Rows[kvp.Key];
                //row.Cells[colID].Value = kvp.Key;
                row.Cells[colName].Value = GetTextValue(xmlType, kvp.Key, "Name");
            }

            SelectRow(dgv, _diTabIndices[tabIndex]);
            dgv.Focus();
        }
        #endregion

        #region EventHandlers
        private void FrmDBEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult res = MessageBox.Show("Save first?", "Closing", MessageBoxButtons.YesNoCancel);
            switch (res)
            {
                case DialogResult.Yes:
                    SaveAll(false);
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
        private void btnDialogue_Click(object sender, EventArgs e)
        {
            var tbActorID = FindTextBox(XMLTypeEnum.Actor, ComponentTypeEnum.TextBoxID);
            var cbEditableCharData = FindComboBox(XMLTypeEnum.Actor, ComponentTypeEnum.ComboBoxEditable);

            XMLData data = _diBasicXML[ACTOR_XML_FILE][int.Parse(tbActorID.Text)];
            string npcKey = @"\NPC_" + data.GetTagValue("Key") + ".xml";
            FormCharExtraData frm;
            if (cbEditableCharData.SelectedItem.ToString() == "Dialogue")
            {
                string key = string.Empty;
                switch (Util.ParseEnum<ActorTypeEnum>(data.GetTagValue("Type")))
                {
                    case ActorTypeEnum.Villager:
                        key = PATH_TO_VILLAGER_DIALOGUE + npcKey;
                        break;
                    case ActorTypeEnum.Traveler:
                        key = PATH_TO_TRAVELER_DIALOGUE + npcKey;
                        break;
                }
                if (!string.IsNullOrEmpty(key))
                {
                    if (!_diNPCDialogue.ContainsKey(key))
                    {
                        _diNPCDialogue[key] = new List<XMLData>();
                    }

                    frm = new FormCharExtraData("Dialogue", _diNPCDialogue[key], ref _diObjectText);
                    frm.ShowDialog();

                    _diNPCDialogue[key] = frm.StringData;
                }
            }
            else if (cbEditableCharData.SelectedItem.ToString() == "Schedule")
            {
                string key = PATH_TO_SCHEDULES + npcKey;
                if (!_diNPCSchedules.ContainsKey(key))
                {
                    _diNPCSchedules[key] = new Dictionary<string, List<string>> { ["New"] = new List<string>() };
                }

                frm = new FormCharExtraData("Schedule", _diNPCSchedules[key]);
                frm.ShowDialog();

                _diNPCSchedules[key] = frm.ListData;
            }
        }

        private void btnEditCutsceneDialogue_Click(object sender, EventArgs e)
        {
            var dgv = FindDataGridView(XMLTypeEnum.Cutscene, ComponentTypeEnum.DataGrid);
            string cutSceneFileName = String.Format(@"{0}\Cutscene_{1}.xml", PATH_TO_CUTSCENE_DIALOGUE, dgv.CurrentRow.Cells["colCutscenesID"].Value.ToString());

            if (!_diCutsceneDialogue.ContainsKey(cutSceneFileName))
            {
                _diCutsceneDialogue[cutSceneFileName] = new List<XMLData>();
            }

            FormCharExtraData frm = new FormCharExtraData("Cutscene Dialogue", _diCutsceneDialogue[cutSceneFileName], ref _diObjectText);
            frm.ShowDialog();

            _diCutsceneDialogue[cutSceneFileName] = frm.StringData;
        }

        private void cbItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetItemSubtype();
        }

        private void cbActorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetActorSubtype();
        }

        private void cbWorldObjectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cbWorldObjectType = FindComboBox(XMLTypeEnum.WorldObject, ComponentTypeEnum.ComboBoxType);
            var cbWorldObjectSubtype = FindComboBox(XMLTypeEnum.WorldObject, ComponentTypeEnum.ComboBoxSubtype);

            cbWorldObjectSubtype.Items.Clear();
            ObjectTypeEnum itemType = Util.ParseEnum<ObjectTypeEnum>(cbWorldObjectType.SelectedItem.ToString().Split(':')[1]);
            switch (itemType)
            {
                case ObjectTypeEnum.Buildable:
                    cbWorldObjectSubtype.Visible = true;
                    foreach (BuildableEnum t in Enum.GetValues(typeof(BuildableEnum)))
                    {
                        cbWorldObjectSubtype.Items.Add("Subtype:" + t.ToString());
                    }
                    break;
                case ObjectTypeEnum.DungeonObject:
                    cbWorldObjectSubtype.Visible = true;
                    foreach (TriggerObjectEnum t in Enum.GetValues(typeof(TriggerObjectEnum)))
                    {
                        cbWorldObjectSubtype.Items.Add("Subtype:" + t.ToString());
                    }
                    break;
                default:
                    cbWorldObjectSubtype.Visible = false;
                    break;
            }

            if (cbWorldObjectSubtype.Visible)
            {
                cbWorldObjectSubtype.SelectedIndex = 0;
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.DropDown.Close();
            SaveAll(false);
        }

        private void sortAndSaveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.DropDown.Close();
            SaveAll(true);
        }

        private void SaveXMLDataDictionary(Dictionary<string, List<XMLData>> dictionaryData, StreamWriter sWriter)
        {
            foreach (string s in dictionaryData.Keys)
            {
                foreach (XMLData data in dictionaryData[s])
                {
                    data.StripSpecialCharacter();
                }
                SaveXMLData(dictionaryData[s], s, sWriter);
            }
        }

        private void tabCtl_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutoSave();

            _typeFilter = "All";
            _subtypeFilter = "All";
            XMLTypeEnum en = CurrentTabXML();
            LoadDataGrid(en);
            FindDataGridView(en, ComponentTypeEnum.DataGrid).Focus();

            _diTabIndices["PreviousTab"] = tabCtl.SelectedIndex;
        }

        private void Backup()
        {
            Backup(PATH_TO_DATA, PATH_TO_BACKUP);
        }
        private static void Backup(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                if (directory != PATH_TO_BACKUP)
                {
                    string dirName = Path.GetFileName(directory);
                    if (!Directory.Exists(Path.Combine(dest, dirName)))
                    {
                        Directory.CreateDirectory(Path.Combine(dest, dirName));
                    }
                    Backup(directory, Path.Combine(dest, dirName));
                }
            }

            foreach (var file in Directory.GetFiles(root))
            {
                string destFile = Path.Combine(dest, Path.GetFileName(file));
                if (File.Exists(destFile)) { File.Delete(destFile); }
                File.Copy(file, destFile);
            }
        }

        private void SaveAll(bool SortIDs)
        {
            Backup();
            AutoSave();
            StreamWriter sWriter = PrepareXMLFile(OBJECT_TEXT_XML_FILE, "Dictionary[string, string]");

            //if (SortIDs)
            //{
            //    UpdateStatus("Sorting Started...");
            //    _liItems.Sort((x, y) =>
            //    {
            //        var typeComp = x.GetTagValue("Type").CompareTo(y.GetTagValue("Type"));
            //        if (typeComp == 0) { return x.ID.CompareTo(y.ID); }
            //        else { return typeComp; }
            //    });
            //    _liWorldObjects.Sort((x, y) =>
            //    {
            //        var typeComp = x.GetTagValue("Type").CompareTo(y.GetTagValue("Type"));
            //        if (typeComp == 0) { return x.Name.CompareTo(y.Name); }
            //        else { return typeComp; }
            //    });

            //    ChangeIDs(ref itemDataList, ref worldObjectDataList);

            //    //Strip the special case character from the Item files
            //    foreach (XMLData data in _liItems)
            //    {
            //        if (data != null) { data.StripSpecialCharacter(); }
            //    }

            //    //Strip the special case character from the WorldObject files
            //    foreach (XMLData data in _liWorldObjects)
            //    {
            //        data.StripSpecialCharacter();
            //    }

            //    //Sort the following Dictionaries by name
            //    List<XMLData> listToSort = _diBasicXML[ACTOR_XML_FILE];
            //    SortDictionaryByType(ref listToSort);
            //}
            //else
            //{
            //    itemDataList = _liItems;
            //    worldObjectDataList = _liWorldObjects;
            //}

            UpdateStatus("Saving...");
            SaveXMLDataDictionary(_diBasicXML, sWriter);
            SaveXMLDataDictionary(_diNPCDialogue, sWriter);
            SaveXMLDataDictionary(_diCutsceneDialogue, sWriter);

            SaveXMLData(_liMailbox, PATH_TO_TEXT_FILES + @"\Mailbox_Text.xml", sWriter);
            //SaveXMLDataDictionary(_diGameText, sWriter);
            //SaveXMLData(_diGameText, PATH_TO_TEXT_FILES + @"\GameText.xml", sWriter);
            //SaveXMLData(_diMailbox, PATH_TO_TEXT_FILES + @"\Mailbox_Text.xml", sWriter);

            foreach (string s in _diNPCSchedules.Keys)
            {
                SaveXMLDictionaryList(_diNPCSchedules[s], s, sWriter, "string");
            }

            for (int i = 0; i < _diCutscenes.Count; i++)
            {
                for (int j = 0; j < _diCutscenes[i].Count; j++)
                {
                    _diCutscenes[i][j] = _diCutscenes[i][j].Replace(SPECIAL_CHARACTER, "");
                }
            }

            SaveXMLDictionaryIntKeyList(_diCutscenes, CUTSCENE_XML_FILE, XMLTypeEnum.Cutscene, sWriter);
            //SaveXMLDictionaryList(_diCutsceneDialogue, PATH_TO_CUTSCENE_DIALOGUE, sWriter, "int");

            //string mapPath = PATH_TO_MAPS;
            //if (!Directory.Exists(mapPath)) { Directory.CreateDirectory(mapPath); }
            //foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
            //{
            //    kvp.Value.StripSpecialCharacter();
            //    DirectoryInfo dirInfo = Directory.GetParent(kvp.Key);
            //    if (!Directory.Exists(dirInfo.FullName)) { Directory.CreateDirectory(dirInfo.FullName); }
            //    SaveTMXData(kvp.Value, kvp.Key);
            //}

            CloseStreamWriter(ref sWriter);

            List<string> keys = new List<string>(_diTabIndices.Keys);
            foreach (string key in keys)
            {
                _diTabIndices[key] = 0;
            }
            //if (_iNextCurrID != -1)
            //{
            //    _diTabIndices["Items"] = _iNextCurrID;
            //    _iNextCurrID = -1;
            //}

            //LoadDataGrids();
            //LoadAllInfoPanels();

            UpdateStatus("Save Complete.");
        }

        private void UpdateStatus(string status)
        {
            tbStatus.Text = status;
        }

        private void AutoSave()
        {
            TabPage prevPage = tabCtl.TabPages[_diTabIndices["PreviousTab"]];

            SaveInfo(TabToEnum(prevPage));
        }
        #endregion

        #region Save Methods
        private StreamWriter PrepareXMLFile(string fileName, string assetType)
        {
            StreamWriter dataFile = new StreamWriter(fileName);
            dataFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            dataFile.WriteLine("<XnaContent xmlns:Generic=\"System.Collections.Generic\">");
            dataFile.WriteLine("  <Asset Type=\"" + assetType + "\">"); //Dictionary[int, string]
            return dataFile;
        }

        private void CloseStreamWriter(ref StreamWriter dataFile)
        {
            dataFile.WriteLine("  </Asset>");
            dataFile.WriteLine("</XnaContent>");
            dataFile.Close();
        }

        public void SaveXMLDictionaryIntKeyList(Dictionary<int, List<string>> dataList, string fileName, XMLTypeEnum xmlType, StreamWriter sWriter)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[int, List[string]]");

            foreach (KeyValuePair<int, List<string>> kvp in dataList)
            {
                string key = string.Format("      <Key>{0}</Key>", kvp.Key);
                string value = "      <Value>" + System.Environment.NewLine;
                string item = string.Empty;
                foreach (string s in kvp.Value)
                {
                    value += string.Format("        <Item>{0}</Item>{1}", s, System.Environment.NewLine);
                }
                value += "      </Value>";
                WriteXMLEntry(dataFile, key, value);

                string name = GetTextValue(xmlType, kvp.Key, "Name");
                string description = GetTextValue(xmlType, kvp.Key, "Description");

                string textValue = string.Format("[Name:{0}]", name);
                if (!string.IsNullOrEmpty(description)) { value += string.Format("[Description:{0}]", description); }
                WriteXMLEntry(sWriter, string.Format("      <Key>{0}</Key>", Util.GetEnumString(xmlType) + "_" + kvp.Key), string.Format("      <Value>{0}</Value>", textValue));
            }

            CloseStreamWriter(ref dataFile);
        }
        public void SaveXMLDictionaryList(Dictionary<string, List<string>> dataList, string fileName, StreamWriter sWriter, string keyType)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[" + keyType + ", List[string]]");

            foreach (KeyValuePair<string, List<string>> kvp in dataList)
            {
                string key = string.Format("      <Key>{0}</Key>", kvp.Key);
                string value = "      <Value>" + System.Environment.NewLine;
                string item = string.Empty;
                foreach (string s in kvp.Value)
                {
                    value += string.Format("        <Item>{0}</Item>{1}", s, System.Environment.NewLine);
                }
                value += "      </Value>";
                WriteXMLEntry(dataFile, key, value);
            }

            CloseStreamWriter(ref dataFile);
        }

        public void SaveXMLDictionary(Dictionary<string, Dictionary<string, string>> dataList, string fileName, StreamWriter sWriter)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[string, string]");

            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in dataList)
            {
                string tagString = string.Empty;

                foreach (KeyValuePair<string, string> tag in kvp.Value)
                {
                    tagString += "[" + tag.Key + (string.IsNullOrEmpty(tag.Value) ? "" : ":" + tag.Value) + "]";
                }

                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", kvp.Key), string.Format("      <Value>{0}</Value>", tagString));
            }

            CloseStreamWriter(ref dataFile);
        }

        public void SaveXMLData(List<XMLData> dataList, string fileName, StreamWriter sWriter)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[int, string]");

            foreach (XMLData data in dataList)
            {
                string id = data.ID.ToString();
                XMLTypeEnum type = FileNameToXMLType(fileName);
                if (type != XMLTypeEnum.None)
                {
                    id = data.GetObjectTextID();
                }

                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>{0}</Value>", data.GetTagsString()));

                //This is where we save Name/Desc to the Object_Text file
                if (!fileName.Contains("Config") && type != XMLTypeEnum.TextFile)
                {
                    string value = string.Format("[Name:{0}]", data.Name);
                    if (!string.IsNullOrEmpty(data.Description)) { value += string.Format("[Description:{0}]", data.Description); }
                    WriteXMLEntry(sWriter, string.Format("      <Key>{0}</Key>", id), string.Format("      <Value>{0}</Value>", value));
                }
            }

            CloseStreamWriter(ref dataFile);
        }

        private void WriteXMLEntry(StreamWriter dataFile, string key, string value)
        {
            dataFile.WriteLine("    <Item>");
            dataFile.WriteLine(key);
            dataFile.WriteLine(value);
            dataFile.WriteLine("    </Item>");
        }

        public void SaveTMXData(TMXData data, string fileName)
        {
            StreamWriter dataFile = new StreamWriter(fileName);

            foreach (string s in data.AllLines)
            {
                dataFile.WriteLine(s);
            }

            dataFile.Close();
        }
        #endregion

        #region Context Menu Methods
        private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            DataGridView dgv = contextMenu.SourceControl as DataGridView;

            contextMenu.Items.Clear();

            var currentTabXML = CurrentTabXML();
            switch (currentTabXML)
            {
                case XMLTypeEnum.Item:
                    var items = Enum.GetNames(typeof(ItemTypeEnum)).ToList();
                    items.Remove(Util.GetEnumString(ItemTypeEnum.Buildable));
                    AddContextMenuItem("Add New", AddNewEntry, true, items.ToArray());
                    AddContextMenuItem("All", dgvContextMenuClick, false);

                    foreach (ItemTypeEnum en in Enum.GetValues(typeof(ItemTypeEnum)))
                    {
                        var s = Util.GetEnumString(en);
                        switch (en)
                        {
                            case ItemTypeEnum.Buildable:
                                continue;
                            case ItemTypeEnum.Resource:
                                var resourceNames = Enum.GetNames(typeof(ResourceTypeEnum)).ToList();
                                resourceNames.Insert(0, "All");
                                resourceNames.Remove(Util.GetEnumString(ResourceTypeEnum.Food));
                                resourceNames.Remove(Util.GetEnumString(ResourceTypeEnum.Meal));
                                AddContextMenuItem(s, dgvContextMenuClick, false, resourceNames.ToArray());
                                break;
                            case ItemTypeEnum.Food:
                                AddContextMenuItem(s, dgvContextMenuClick, false, new string[] { "All", Util.GetEnumString(ResourceTypeEnum.Food), Util.GetEnumString(ResourceTypeEnum.Meal) });
                                break;
                            case ItemTypeEnum.Merchandise:
                                var merchNames = Enum.GetNames(typeof(MerchandiseTypeEnum)).ToList();
                                merchNames.Insert(0, "All");
                                AddContextMenuItem(s, dgvContextMenuClick, false, merchNames.ToArray());
                                break;
                            default:
                                AddContextMenuItem(s, dgvContextMenuClick, false);
                                break;

                        }
                    }
                    break;
                case XMLTypeEnum.WorldObject:
                    AddContextMenuItem("Add New", AddNewEntry, true, Enum.GetNames(typeof(ObjectTypeEnum)));
                    AddContextMenuItem("All", dgvContextMenuClick, false);

                    foreach (ObjectTypeEnum en in Enum.GetValues(typeof(ObjectTypeEnum)))
                    {
                        var s = Util.GetEnumString(en);
                        switch (en)
                        {
                            case ObjectTypeEnum.Buildable:
                                var names = Enum.GetNames(typeof(BuildableEnum)).ToList();
                                names.Insert(0, "All");
                                AddContextMenuItem(s, dgvContextMenuClick, false, names.ToArray());
                                break;
                            default:
                                AddContextMenuItem(s, dgvContextMenuClick, false);
                                break;
                        }
                    }
                    break;
                case XMLTypeEnum.Task:
                    AddContextMenuItem("Add New", AddNewEntry, false, Enum.GetNames(typeof(TaskTypeEnum)));
                    break;
                case XMLTypeEnum.Actor:
                    AddContextMenuItem("Add New", AddNewEntry, true, Enum.GetNames(typeof(ActorTypeEnum)));
                    AddContextMenuItem("All", dgvContextMenuClick, false);
                    foreach (string s in Enum.GetNames(typeof(ActorTypeEnum)))
                    {
                        if (!s.Equals("Actor"))
                        {
                            AddContextMenuItem(s, dgvContextMenuClick, false);
                        }
                    }
                    break;
                default:
                    AddContextMenuItem("Add New", AddNewEntry, false);
                    break;
            }
        }

        private void AddContextMenuItem(string text, EventHandler triggeredEvent, bool separator, params string[] subEntries)
        {
            ToolStripMenuItem tsmi = new ToolStripMenuItem(text);
            if (subEntries.Length > 0)
            {
                foreach (var entry in subEntries)
                {
                    ToolStripMenuItem e = new ToolStripMenuItem(entry);
                    e.Click += new EventHandler(triggeredEvent);
                    tsmi.DropDownItems.Add(e);
                }
            }
            else
            {
                tsmi.Click += new EventHandler(triggeredEvent);
            }
            contextMenu.Items.Add(tsmi);

            if (separator)
            {
                contextMenu.Items.Add(new ToolStripSeparator());
            }
        }

        #region Filters
        private void dgvContextMenuClick(object sender, EventArgs e)
        {
            XMLTypeEnum en = CurrentTabXML();
            _diTabIndices[GetTabIndexName(en)] = 0;

            var selection = ((ToolStripMenuItem)sender);
            if (selection.OwnerItem == null)
            {
                _subtypeFilter = "All";
                LoadDataGrid(en, 0, selection.Text);
            }
            else
            {
                _subtypeFilter = selection.Text;
                LoadDataGrid(en, 0, selection.OwnerItem.Text);
            }
            LoadInfo(en);
        }

        //private void dgvItemsContextMenuClick(object sender, EventArgs e)
        //{
        //    _diTabIndices["Items"] = 0;
        //    var selection = ((ToolStripMenuItem)sender);
        //    if (selection.OwnerItem == null)
        //    {
        //        _subtypeFilter = "All";
        //        LoadInfo(selection.Text)
        //        //LoadItemDataGrid(selection.Text, 0);
        //    }
        //    else
        //    {
        //        _subtypeFilter = selection.Text;
        //        //LoadItemDataGrid(selection.OwnerItem.Text, 0);
        //    }
        //    LoadItemInfo();
        //}

        //private void dgvWorldObjectsContextMenuClick(object sender, EventArgs e)
        //{
        //    _diTabIndices["WorldObjects"] = 0;
        //    var selection = ((ToolStripMenuItem)sender);
        //    if (selection.OwnerItem == null)
        //    {
        //        _subtypeFilter = "All";
        //        //LoadWorldObjectDataGrid(selection.Text, 0);
        //    }
        //    else
        //    {
        //        _subtypeFilter = selection.Text;
        //        //LoadWorldObjectDataGrid(selection.OwnerItem.Text, 0);
        //    }
        //    LoadWorldObjectInfo();
        //}

        //private void dgActorsContextMenuClick(object sender, EventArgs e)
        //{
        //    _diTabIndices["Actors"] = 0;
        //    //LoadActorDataGrid(((ToolStripMenuItem)sender).Text, 0);
        //    //LoadActorInfo();
        //}
        #endregion

        private void gameTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCharExtraData frm = null;

            frm = new FormCharExtraData("Dialogue", _liGameText, ref _diObjectText);
            frm.ShowDialog();

            _liGameText = frm.StringData;
        }

        private void mailboxMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCharExtraData frm = null;

            frm = new FormCharExtraData("Dialogue", _liMailbox, ref _diObjectText);
            frm.ShowDialog();

            _liMailbox = frm.StringData;
        }
        #endregion

        #region Add New
        private void AddNewEntry(object sender, EventArgs e)
        {
            AddNewGenericXMLObject(_diTabCollections[CurrentTabXML()], sender.ToString());
        }
        private void AddNewGenericXMLObject(XMLCollection collection, string chosenType)
        {
            string tabIndex = GetName(collection.XMLType, ComponentTypeEnum.TabIndex);
            string columnName = GetName(collection.XMLType, ComponentTypeEnum.ColumnName);
            string columnTags = GetName(collection.XMLType, ComponentTypeEnum.ColumnTags);
            TextBox tbName = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxName);
            TextBox tbID = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxID);
            TextBox tbDescription = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxDescription);
            DataGridView dgv = FindDataGridView(collection.XMLType, ComponentTypeEnum.DataGrid);
            DataGridView dgvTags = FindDataGridView(collection.XMLType, ComponentTypeEnum.DataGridTags);
            ComboBox cb = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxType);

            _diTabIndices[tabIndex] = dgv.Rows.Count;
            dgv.Rows.Add();
            SelectRow(dgv, _diTabIndices[tabIndex]);

            DataGridViewRow row = dgv.Rows[_diTabIndices[tabIndex]];
            //row.Cells[collection.ColumnID].Value = _diTabIndices[tabIndex];
            row.Cells[columnName].Value = "";

            tbName.Text = "";
            tbID.Text = GetDataList(CurrentTabXML()).Count.ToString();
            if (tbDescription != null)
            {
                tbDescription.Text = "";
            }

            if (cb != null)
            {
                if (string.IsNullOrEmpty(chosenType)) { cb.SelectedIndex = 0; }
                else { cb.SelectedItem = "Type:" + chosenType; }
            }

            dgvTags.Rows.Clear();

            string defaultTags = string.Empty;
            switch (collection.XMLType)
            {
                case XMLTypeEnum.Actor:
                    GetActorDefault(chosenType, ref defaultTags);
                    break;
                case XMLTypeEnum.Item:
                    GetItemDefault(chosenType, ref defaultTags);
                    break;
                case XMLTypeEnum.Light:
                    defaultTags = DEFAULT_LIGHT_TAGS;
                    break;
                case XMLTypeEnum.Shop:
                    defaultTags = DEFAULT_SHOP_TAGS;
                    break;
                case XMLTypeEnum.Task:
                    GetTaskDefault(chosenType, ref defaultTags);
                    break;
                case XMLTypeEnum.Upgrade:
                    defaultTags = DEFAULT_UPGRADE_TAGS;
                    break;
                case XMLTypeEnum.WorldObject:
                    GetWorldObjectDefault(chosenType, ref defaultTags);
                    break;
            }

            if (defaultTags != null)
            {
                string[] split = defaultTags.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    dgvTags.Rows.Add();
                    dgvTags.Rows[i].Cells[columnTags].Value = split[i];
                }
            }

            tbName.Focus();
        }
        private void GetActorDefault(string chosenType, ref string defaultTags)
        {
            ActorTypeEnum e = Util.ParseEnum<ActorTypeEnum>(chosenType);
            switch (e)
            {
                case ActorTypeEnum.Animal:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",ObjectID:,ItemID:";
                    break;
                case ActorTypeEnum.Critter:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",Action1:0-0-3-0.15-T,Action2:64-0-2-0.15-T";
                    break;
                case ActorTypeEnum.Merchant:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",ShopData:,Day:Monday";
                    break;
                case ActorTypeEnum.Mob:
                    defaultTags = "Key:,Size:16-32,Idle:3-.15-T,Walk:3-.15-T,KO:384-0-6-0.15-F,HP:,Damage:,Weight:,Speed:,ItemID:";
                    break;
                case ActorTypeEnum.Mount:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",ObjectID:";
                    break;
                case ActorTypeEnum.Pet:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",Alert:0-64-3-0.5-T,ObjectID:";
                    break;
                case ActorTypeEnum.Projectile:
                    defaultTags = "Speed:3,Damage:1,Size:16-16,CollisionOffset:,CollisionSize:,Idle:0-0-1-1-T,KO:16-0-1-0.1-F";
                    break;
                case ActorTypeEnum.Traveler:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",FavFood:,Disliked:,Building:";
                    break;
                case ActorTypeEnum.Villager:
                    defaultTags = DEFAULT_ACTOR_TAGS + ",HouseID:,Collection:";
                    break;
                default:
                    defaultTags = DEFAULT_ACTOR_TAGS;
                    break;
            }
        }
        private void GetItemDefault(string chosenType, ref string defaultTags)
        {
            ItemTypeEnum e = Util.ParseEnum<ItemTypeEnum>(chosenType);
            switch (e)
            {
                case ItemTypeEnum.Blueprint:
                    defaultTags = DEFAULT_ITEM_TAGS + ",ObjectID:";
                    break;
                case ItemTypeEnum.Food:
                    defaultTags = DEFAULT_ITEM_TAGS + ",FoodType:,FoodValue:,EnergyRecovery:";
                    break;
                case ItemTypeEnum.NPCToken:
                    defaultTags = DEFAULT_ITEM_TAGS + ",NPC_ID:";
                    break;
                case ItemTypeEnum.Resource:
                    defaultTags = DEFAULT_ITEM_TAGS;
                    break;
                case ItemTypeEnum.Seed:
                    defaultTags = DEFAULT_ITEM_TAGS + ",ObjectID:,Season:";
                    break;
                case ItemTypeEnum.Tool:
                    defaultTags = DEFAULT_ITEM_TAGS + ",Level:,EnergyCost:";
                    break;
                default:
                    defaultTags = DEFAULT_ITEM_TAGS + ",Count:";
                    break;
            }
        }
        private void GetTaskDefault(string chosenType, ref string defaultTags)
        {
            TaskTypeEnum e = Util.ParseEnum<TaskTypeEnum>(chosenType);
            switch (e)
            {
                case TaskTypeEnum.Build:
                    defaultTags = DEFAULT_TASK_TAGS + ",TargetObjectID:,EndBuildingID:,CutsceneID:";
                    break;
                case TaskTypeEnum.Fetch:
                    defaultTags = DEFAULT_TASK_TAGS + ",GoalItem:,Count:,GoalNPC:";
                    break;
                case TaskTypeEnum.Talk:
                    defaultTags = DEFAULT_TASK_TAGS + ",GoalNPC:";
                    break;
                default:
                    defaultTags = DEFAULT_TASK_TAGS + ",Count:,GoalNPC";
                    break;
            }
        }
        private void GetWorldObjectDefault(string chosenType, ref string defaultTags)
        {
            ObjectTypeEnum e = Util.ParseEnum<ObjectTypeEnum>(chosenType);
            switch (e)
            {
                case ObjectTypeEnum.Buildable:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",ReqItems:";
                    break;
                //case ObjectTypeEnum.Building:
                //    defaultTags = "Texture:,Size:7-8,Base:0-4-7-4,Entrance:4-6-1-2,ReqItems:55-5|60-5";
                //    break;
                //case ObjectTypeEnum.Container:
                //    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",Rows:,Cols:,Opens";
                //    break;
                //case ObjectTypeEnum.Decor:
                //    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",Rotation:FourWay,RotationBaseOffset:0-1,RotationSize:1-3";
                //    break;
                case ObjectTypeEnum.Destructible:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",HP:,Tool:,ReqLvl:1,ItemID:,DestructionAnim:208-0-3-0.1";
                    break;
                case ObjectTypeEnum.Hazard:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",Damage:";
                    break;
                case ObjectTypeEnum.Plant:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",TrNum:,TrTime:,Season:,SeedID:,ItemID:";
                    break;
                default:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS;
                    break;
            }
        }
        #endregion

        #region Load Info Panes
        private void LoadAllInfoPanels()
        {
            foreach (XMLTypeEnum en in Enum.GetValues(typeof(XMLTypeEnum)))
            {
                if (IsDataFile(en))
                {
                    LoadInfo(en);
                }
            }
        }

        private void LoadInfo(XMLTypeEnum en)
        {
            var dgv = FindDataGridView(en, ComponentTypeEnum.DataGrid);
            if (dgv.SelectedRows.Count > 0)
            {
                XMLData data = null;

                string strEnum = en.ToString();
                string fileName = GetXMLDataFileName(en);

                switch (en)
                {
                    case XMLTypeEnum.Actor:
                        LoadActorInfo();
                        break;
                    case XMLTypeEnum.Item:
                        LoadItemInfo();
                        break;
                    case XMLTypeEnum.WorldObject:
                        LoadWorldObjectInfo();
                        break;
                    case XMLTypeEnum.Cutscene:
                        LoadCutsceneInfo();
                        break;
                    default:
                        data = GetDataList(en)[GetTabIndex(en)];
                        LoadGenericDataInfo(data, _diTabCollections[en]);
                        break;
                }
            }
        }

        private void LoadGenericDataInfo(XMLData data, XMLCollection collection)
        {
            TextBox tbName = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxName);
            TextBox tbID = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxID);
            TextBox tbDescription = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxDescription);
            DataGridView dgvTags = FindDataGridView(collection.XMLType, ComponentTypeEnum.DataGridTags);
            ComboBox cbType = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxType);
            ComboBox cbSubtype = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxSubtype);
            ComboBox cbGrouptype = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxGroup);
            ComboBox cbSubGrouptype = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxSubGroup);

            tbName.Text = data.Name;
            tbID.Text = data.ID.ToString();
            if (tbDescription != null)
            {
                tbDescription.Text = data.Description;
            }

            dgvTags.Rows.Clear();
            string[] tags = data.GetTagsString().Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (!s.StartsWith("Type") && !s.StartsWith("Subtype") && !s.StartsWith("Group") && !s.StartsWith("SubGroup"))
                {
                    dgvTags.Rows.Add(s);
                }
            }

            LoadSelected(cbType, data, "Type");
            LoadSelected(cbSubtype, data, "Subtype");
            LoadSelected(cbGrouptype, data, "Group");
            LoadSelected(cbSubGrouptype, data, "SubGroup");
        }

        private void LoadSelected(ComboBox cb, XMLData data, string tag)
        {
            if (cb != null && data.HasTag(tag))
            {
                cb.SelectedItem = tag + ":" + data.GetTagValue(tag);
            }
        }

        private void LoadActorInfo()
        {
            var dgv = FindDataGridView(XMLTypeEnum.Actor, ComponentTypeEnum.DataGrid);
            if (dgv.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgv.SelectedRows[0];
                XMLData data = null;
                if (_typeFilter == "All") { data = _diBasicXML[ACTOR_XML_FILE][_diTabIndices["Actors"]]; }
                else { data = _diBasicXML[ACTOR_XML_FILE].FindAll(x => x.GetTagValue("Type").ToString().Equals(_typeFilter))[r.Index]; }
                LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Actor]);
            }
        }

        private void LoadItemInfo()
        {
            var dgv = FindDataGridView(XMLTypeEnum.Item, ComponentTypeEnum.DataGrid);
            if (dgv.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgv.SelectedRows[0];
                XMLData data = null;
                if (_typeFilter == "All") { data = _diBasicXML[ITEM_XML_FILE][r.Index]; }
                else if (_subtypeFilter != "All")
                {
                    data = _diBasicXML[ITEM_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter) && x.GetTagValue("Subtype").Equals(_subtypeFilter))[r.Index];
                }
                else { data = _diBasicXML[ITEM_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter))[r.Index]; }

                LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Item]);
            }
        }
        private void LoadWorldObjectInfo()
        {
            var dgv = FindDataGridView(XMLTypeEnum.WorldObject, ComponentTypeEnum.DataGrid);
            if (dgv.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgv.SelectedRows[0];
                XMLData data = null;
                if (_typeFilter == "All") { data = _diBasicXML[WORLD_OBJECTS_XML_FILE][r.Index]; }
                else if (_subtypeFilter != "All")
                {
                    data = _diBasicXML[WORLD_OBJECTS_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter) && x.GetTagValue("Subtype").Equals(_subtypeFilter))[r.Index];
                }
                else { data = _diBasicXML[WORLD_OBJECTS_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter))[r.Index]; }

                LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.WorldObject]);
            }
        }

        private void LoadCutsceneInfo()
        {
            var dgvTags = FindDataGridView(XMLTypeEnum.Cutscene, ComponentTypeEnum.DataGridTags);
            var tbName = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxName);
            var tbID = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxID);
            var tbCutsceneTriggers = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxTriggers);
            var tbCutsceneDetails = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxDetails);

            List<string> listData = _diCutscenes[_diTabIndices["Cutscenes"]];

            tbName.Text = GetTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name");
            tbID.Text = _diTabIndices["Cutscenes"].ToString();
            tbCutsceneTriggers.Text = listData[0];
            tbCutsceneDetails.Text = listData[1];

            dgvTags.Rows.Clear();
            string[] tags = listData[2].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (!s.StartsWith("Type"))
                {
                    dgvTags.Rows.Add(s);
                }
            }
        }
        #endregion

        #region SaveInfo
        private void SaveInfo(XMLTypeEnum en)
        {
            switch (en)
            {
                case XMLTypeEnum.TextFile:
                    return;
                case XMLTypeEnum.Cutscene:
                    SaveCutsceneInfo();
                    break;
                default:
                    SaveXMLDataInfo(GetDataList(en), _diTabCollections[en]);
                    break;
            }
        }

        private void SaveCutsceneInfo()
        {
            var dgvTags = FindDataGridView(XMLTypeEnum.Cutscene, ComponentTypeEnum.DataGridTags);
            var tbName = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxName);
            var tbCutsceneTriggers = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxTriggers);
            var tbCutsceneDetails = FindTextBox(XMLTypeEnum.Cutscene, ComponentTypeEnum.TextBoxDetails);

            UpdateStatus("Saving Cutscenes");
            List<string> listData;
            if (!_diCutscenes.ContainsKey(_diTabIndices["Cutscenes"]))
            {
                _diCutscenes[_diTabIndices["Cutscenes"]] = new List<string>();
                listData = _diCutscenes[_diTabIndices["Cutscenes"]];
            }
            else { listData = _diCutscenes[_diTabIndices["Cutscenes"]]; }

            listData.Clear();
            listData.Add(tbCutsceneTriggers.Text);
            listData.Add(tbCutsceneDetails.Text);

            string tags = string.Empty;
            foreach (DataGridViewRow r in dgvTags.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    tags += "[" + r.Cells[0].Value + "]";
                }
            }
            listData.Add(tags);
            UpdateTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name", tbName.Text);

            DataGridViewRow updatedRow = FindDataGridView(XMLTypeEnum.Cutscene, ComponentTypeEnum.DataGrid).Rows[_diTabIndices["Cutscenes"]];

            //updatedRow.Cells["colCutscenesID"].Value = _diTabIndices["Cutscenes"];
            updatedRow.Cells["colCutscenesName"].Value = GetTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name");
        }
        private void SaveXMLDataInfo(List<XMLData> liData, XMLCollection collection)
        {
            string tabIndex = GetName(collection.XMLType, ComponentTypeEnum.TabIndex);
            string columnName = GetName(collection.XMLType, ComponentTypeEnum.ColumnName);
            string columnTags = GetName(collection.XMLType, ComponentTypeEnum.ColumnTags);
            TextBox tbName = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxName);
            TextBox tbID = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxID);
            TextBox tbDescription = FindTextBox(collection.XMLType, ComponentTypeEnum.TextBoxDescription);
            DataGridView dgv = FindDataGridView(collection.XMLType, ComponentTypeEnum.DataGrid);
            DataGridView dgvTags = FindDataGridView(collection.XMLType, ComponentTypeEnum.DataGridTags);
            ComboBox cb = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxType);
            ComboBox cbSubtype = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxSubtype);
            ComboBox cbGrouptype = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxGroup);
            ComboBox cbSubGrouptype = FindComboBox(collection.XMLType, ComponentTypeEnum.ComboBoxSubGroup);

            UpdateStatus("Saving " + tabIndex);

            XMLData data;
            if (liData.Count == int.Parse(tbID.Text))
            {
                data = new XMLData(tbID.Text, new Dictionary<string, string>(), collection.TagsReferenced, collection.TagsThatReferToMe, collection.XMLType, ref _diObjectText);
                liData.Add(data);
            }
            else { data = liData[int.Parse(tbID.Text)]; }


            if (tbDescription == null) { data.SetTextData(tbName.Text); }
            else { data.SetTextData(tbName.Text, tbDescription.Text); }

            data.ClearTagInfo();
            if (cb != null)
            {
                string[] typeTag = cb.SelectedItem.ToString().Split(':');
                data.SetTagInfo(typeTag[0], typeTag[1]);
            }

            if (cbSubtype != null && cbSubtype.SelectedItem != null)
            {
                string[] typeTag = cbSubtype.SelectedItem.ToString().Split(':');
                data.SetTagInfo(typeTag[0], typeTag[1]);
            }

            if (cbGrouptype != null && cbGrouptype.SelectedItem != null)
            {
                string[] typeTag = cbGrouptype.SelectedItem.ToString().Split(':');
                data.SetTagInfo(typeTag[0], typeTag[1]);
            }

            if (cbSubGrouptype != null && cbSubGrouptype.SelectedItem != null)
            {
                string[] typeTag = cbSubGrouptype.SelectedItem.ToString().Split(':');
                data.SetTagInfo(typeTag[0], typeTag[1]);
            }

            foreach (DataGridViewRow row in dgvTags.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tagInfo.Length > 0)
                    {
                        string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                        data.SetTagInfo(tagInfo[0], val);
                    }
                }
            }
            data.ChangeID(int.Parse(tbID.Text), false);

            DataGridViewRow updatedRow = dgv.Rows[_diTabIndices[tabIndex]];

            //updatedRow.Cells[colID].Value = data.ID;
            updatedRow.Cells[columnName].Value = data.Name;
        }
        #endregion

        #region DataGridViewCell_Click
        private void ProcessCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                XMLTypeEnum en = CurrentTabXML();
                SaveInfo(en);
                _diTabIndices[GetTabIndexName(en)] = e.RowIndex;
                LoadInfo(en);
            }
        }
        #endregion

        #region Cancel Button
        private void ProcessCancel_Click(object sender, EventArgs e)
        {
            XMLTypeEnum en = CurrentTabXML();
            var tabIndex = GetTabIndex(en);

            if (GetDataList(en).Count == tabIndex)
            {
                var dgv = FindDataGridView(en, ComponentTypeEnum.DataGrid);
                dgv.Rows.RemoveAt(tabIndex--);
                SelectRow(dgv, tabIndex);
            }

            LoadInfo(en);
        }
        #endregion

        private void FrmDBEditor_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(832, 0);
            this.MaximumSize = new Size(832, Screen.PrimaryScreen.Bounds.Height);

        }

        #region Dynamic Code
        public string GetName(XMLTypeEnum strType, ComponentTypeEnum e)
        {
            return GetName(Util.GetEnumString(strType), e);
        }
        public string GetName(string strType, ComponentTypeEnum e)
        {
            switch (e)
            {
                case ComponentTypeEnum.ButtonCancel:
                    return "btn" + strType + "Cancel";
                case ComponentTypeEnum.TextBoxName:
                    return "tb" + strType + "Name";
                case ComponentTypeEnum.TextBoxID:
                    return "tb" + strType + "ID";
                case ComponentTypeEnum.TextBoxDescription:
                    return "tb" + strType + "Description";
                case ComponentTypeEnum.DataGrid:
                    return "dgv" + strType + "s";
                case ComponentTypeEnum.DataGridTags:
                    return "dgv" + strType + "Tags";
                case ComponentTypeEnum.TabIndex:
                    return strType + "s";
                case ComponentTypeEnum.ColumnName:
                    return "col" + strType + "sName";
                case ComponentTypeEnum.ColumnId:
                    return "col" + strType + "sID";
                case ComponentTypeEnum.ColumnTags:
                    return "col" + strType + "Tags";
                case ComponentTypeEnum.ComboBoxType:
                    return "cb" + strType + "Type";
                case ComponentTypeEnum.ComboBoxSubtype:
                    return "cb" + strType + "Subtype";
                case ComponentTypeEnum.ComboBoxGroup:
                    return "cb" + strType + "Group";
                case ComponentTypeEnum.ComboBoxSubGroup:
                    return "cb" + strType + "SubGroup";
            }

            return string.Empty;
        }

        private TabPage GetPage(XMLTypeEnum en)
        {
            return tabCtl.TabPages["tab" + en.ToString()];
        }

        private TextBox FindTextBox(XMLTypeEnum xmlType, ComponentTypeEnum type)
        {
            string strXMLType = Util.GetEnumString(xmlType);
            string name = GetName(strXMLType, type);

            return GetPage(xmlType).Controls.OfType<TextBox>().FirstOrDefault(val => val.Name == name);
        }

        private DataGridView FindDataGridView(XMLTypeEnum xmlType, ComponentTypeEnum type)
        {
            string strXMLType = Util.GetEnumString(xmlType);
            string name = GetName(strXMLType, type);

            return GetPage(xmlType).Controls.OfType<DataGridView>().FirstOrDefault(val => val.Name == name);
        }

        private ComboBox FindComboBox(XMLTypeEnum xmlType, ComponentTypeEnum type)
        {
            string strXMLType = Util.GetEnumString(xmlType);
            string name = GetName(strXMLType, type);

            var page = GetPage(xmlType);
            return page.Controls.OfType<ComboBox>().FirstOrDefault(val => val.Name == name);
        }

        private XMLTypeEnum CurrentTabXML()
        {
            return TabToEnum(tabCtl.SelectedTab);
        }
        private int GetTabIndex(XMLTypeEnum en)
        {
            return _diTabIndices[GetTabIndexName(en)];
        }
        private string GetTabIndexName(XMLTypeEnum en)
        {
            string strEnum = en.ToString();
            return strEnum + "s";
        }
        private List<XMLData> GetDataList(XMLTypeEnum en)
        {
            return _diBasicXML[GetXMLDataFileName(en)];
        }
        private string GetXMLDataFileName(XMLTypeEnum en)
        {
            switch (en)
            {
                case XMLTypeEnum.Actor:
                    return ACTOR_XML_FILE;
                case XMLTypeEnum.Adventure:
                    return ADVENTURE_XML_FILE;
                case XMLTypeEnum.Cosmetic:
                    return COSMETIC_XML_FILE;
                case XMLTypeEnum.Cutscene:
                    return CUTSCENE_XML_FILE;
                case XMLTypeEnum.Item:
                    return ITEM_XML_FILE;
                case XMLTypeEnum.Light:
                    return LIGHT_XML_FILE;
                case XMLTypeEnum.Shop:
                    return SHOP_XML_FILE;
                case XMLTypeEnum.StatusEffect:
                    return STATUS_EFFECTS_XML_FILE;
                case XMLTypeEnum.Task:
                    return TASK_XML_FILE;
                case XMLTypeEnum.Upgrade:
                    return UPGRADES_XML_FILE;
                case XMLTypeEnum.WorldObject:
                    return WORLD_OBJECTS_XML_FILE;
            }

            return string.Empty;
        }

        private string GetRefTags(XMLTypeEnum en)
        {
            switch (en)
            {
                case XMLTypeEnum.Actor:
                    return ACTOR_REF_TAGS;
                case XMLTypeEnum.Adventure:
                    return ADVENTURE_REF_TAGS;
                case XMLTypeEnum.Cosmetic:
                    return COSMETIC_REF_TAGS;
                case XMLTypeEnum.Cutscene:
                    return CUTSCENE_REF_TAGS;
                case XMLTypeEnum.Item:
                    return ITEM_REF_TAGS;
                case XMLTypeEnum.Light:
                    return LIGHT_REF_TAGS;
                case XMLTypeEnum.Shop:
                    return SHOPDATA_REF_TAGS;
                case XMLTypeEnum.StatusEffect:
                    return STATUS_EFFECTS_REF_TAGS;
                case XMLTypeEnum.Task:
                    return TASK_REF_TAGS;
                case XMLTypeEnum.Upgrade:
                    return UPGRADES_REF_TAGS;
                case XMLTypeEnum.WorldObject:
                    return WORLD_OBJECT_REF_TAGS;
            }

            return string.Empty;
        }

        private string GetTagsForRef(XMLTypeEnum en)
        {
            switch (en)
            {
                case XMLTypeEnum.Actor:
                    return TAGS_FOR_ACTORS;
                case XMLTypeEnum.Adventure:
                    return TAGS_FOR_ADVENTURES;
                case XMLTypeEnum.Cosmetic:
                    return TAGS_FOR_COSMETICS;
                case XMLTypeEnum.Cutscene:
                    return CUTSCENE_REF_TAGS;
                case XMLTypeEnum.Item:
                    return TAGS_FOR_ITEMS;
                case XMLTypeEnum.Light:
                    return TAGS_FOR_LIGHTS;
                case XMLTypeEnum.Shop:
                    return TAGS_FOR_SHOPDATA;
                case XMLTypeEnum.StatusEffect:
                    return TAGS_FOR_STATUS_EFFECTS;
                case XMLTypeEnum.Task:
                    return TAGS_FOR_TASKS;
                case XMLTypeEnum.Upgrade:
                    return TAGS_FOR_UPGRADES;
                case XMLTypeEnum.WorldObject:
                    return TAGS_FOR_WORLD_OBJECTS;
            }

            return string.Empty;
        }

        private XMLTypeEnum TabToEnum(TabPage tab)
        {
            switch (tab.Name)
            {
                case "tabActor":
                    return XMLTypeEnum.Actor;
                case "tabAdventure":
                    return XMLTypeEnum.Adventure;
                case "tabCosmetic":
                    return XMLTypeEnum.Cosmetic;
                case "tabCutscene":
                    return XMLTypeEnum.Cutscene;
                case "tabItem":
                    return XMLTypeEnum.Item;
                case "tabLight":
                    return XMLTypeEnum.Light;
                case "tabShop":
                    return XMLTypeEnum.Shop;
                case "tabStatusEffect":
                    return XMLTypeEnum.StatusEffect;
                case "tabTask":
                    return XMLTypeEnum.Task;
                case "tabUpgrade":
                    return XMLTypeEnum.Upgrade;
                case "tabWorldObject":
                    return XMLTypeEnum.WorldObject;
            }

            return XMLTypeEnum.None;
        }
        #endregion
    }
}