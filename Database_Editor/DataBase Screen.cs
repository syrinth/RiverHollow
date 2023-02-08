using Database_Editor.Classes;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
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
        List<ItemXMLData> _liItemData;
        List<XMLData> _liWorldObjects;

        Dictionary<string, int> _diTabIndices;
        private int _iNextCurrID = -1;

        static Dictionary<int, List<string>> _diCutscenes;
        static Dictionary<string, Dictionary<string, List<string>>> _diNPCSchedules;
        static Dictionary<string, List<XMLData>> _diNPCDialogue;
        static Dictionary<string, List<XMLData>> _diCutsceneDialogue;
        static List<XMLData> _liMailbox;
        static List<XMLData> _liGameText; 
        static Dictionary<string, Dictionary<string, string>> _diObjectText;
        static Dictionary<ItemEnum, List<ItemXMLData>> _diItems;
        static Dictionary<string, List<XMLData>> _diBasicXML;
        Dictionary<string, TMXData> _diMapData;

        Dictionary<XMLTypeEnum, XMLCollection> _diTabCollections;

        string _itemFilter = "All";
        string _worldObjFilter = "All";

        delegate void VoidDelegate();
        delegate void XMLListDataDelegate();

        public FrmDBEditor()
        {
            InitializeComponent();
            SetupTabCollections();

            InitComboBox<ItemEnum>(cbItemType);
            InitComboBox<ObjectTypeEnum>(cbWorldObjectType);
            InitComboBox<TaskTypeEnum>(cbTaskType);
            InitComboBox<EditableNPCDataEnum>(cbEditableCharData, false);
            InitComboBox<StatusTypeEnum>(cbStatusEffect);

            cbNPCType.Items.Clear();
            InitComboBox(cbNPCType, true, new List<WorldActorTypeEnum>() { WorldActorTypeEnum.Actor});

            _diTabIndices = new Dictionary<string, int>()
            {
                { "PreviousTab", 0 },
                { "Items", 0 },
                { "WorldObjects", 0 },
                { "NPCs", 0 },
                { "Tasks", 0 },
                { "Cutscenes", 0},
                { "Mobs", 0},
                { "Shops", 0 },
                { "Buildings", 0 },
                { "StatusEffects", 0 },
                { "Lights", 0 },
                { "Dungeons", 0 },
                { "Upgrades", 0 }
            };

            _diMapData = new Dictionary<string, TMXData>();
            foreach (string s in Directory.GetDirectories(PATH_TO_MAPS))
            {
                foreach (string mapName in Directory.GetFiles(s))
                {
                    if (mapName.EndsWith(".tmx"))
                    {
                        _diMapData[mapName] = new TMXData(mapName);
                    }
                }
            }
            
            _diItems = new Dictionary<ItemEnum, List<ItemXMLData>>();
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
            LoadXMLDictionary(TASK_XML_FILE, TASK_REF_TAGS, "", ref _diBasicXML);
            LoadXMLDictionary(NPC_XML_FILE, NPC_REF_TAGS, TAGS_FOR_NPCS, ref _diBasicXML);
            LoadXMLDictionary(CONFIG_XML_FILE, CONFIG_REF_TAG, "", ref _diBasicXML);
            LoadXMLDictionary(STATUS_EFFECTS_XML_FILE, "", TAGS_FOR_STATUS_EFFECTS, ref _diBasicXML);
            LoadXMLDictionary(LIGHTS_XML_FILE, "", TAGS_FOR_LIGHTS, ref _diBasicXML);
            LoadXMLDictionary(UPGRADES_XML_FILE, "", TAGS_FOR_UPGRADES, ref _diBasicXML);
            LoadXMLDictionary(DUNGEON_XML_FILE, DUNGEON_REF_TAGS, TAGS_FOR_DUNGEONS, ref _diBasicXML);
            LoadXMLDictionary(SHOPS_XML_FILE, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA, ref _diBasicXML);

            //_diShops = ReadXMLFileToXMLDataListDictionary(SHOPS_XML_FILE, XMLTypeEnum.Shop, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA);
            _diCutscenes = ReadXMLFileToIntKeyDictionaryStringList(CUTSCENE_XML_FILE);

            LoadWorldObjects();
            LoadItemData();

            FindLinkedXMLObjects();

            LoadDataGrids();
            LoadAllInfoPanels();
        }

        private void SetupTabCollections()
        {
            _diTabCollections = new Dictionary<XMLTypeEnum, XMLCollection>
            {
                [XMLTypeEnum.WorldObject] = new XMLCollection(XMLTypeEnum.WorldObject, WORLD_OBJECT_REF_TAGS, TAGS_FOR_WORLD_OBJECTS, DEFAULT_WORLD_OBJECT_TAGS),
                [XMLTypeEnum.Task] = new XMLCollection(XMLTypeEnum.Task, TASK_REF_TAGS, TAGS_FOR_TASKS, ""),
                [XMLTypeEnum.Cutscene] = new XMLCollection(XMLTypeEnum.Cutscene, CUTSCENE_REF_TAGS, "", ""),
                [XMLTypeEnum.StatusEffect] = new XMLCollection(XMLTypeEnum.StatusEffect, "", TAGS_FOR_STATUS_EFFECTS, ""),
                [XMLTypeEnum.Dungeon] = new XMLCollection(XMLTypeEnum.Dungeon, DUNGEON_REF_TAGS, TAGS_FOR_DUNGEONS, ""),
                [XMLTypeEnum.Item] = new XMLCollection(XMLTypeEnum.Item, ITEM_REF_TAGS, TAGS_FOR_ITEMS, DEFAULT_ITEM_TAGS),
                [XMLTypeEnum.NPC] = new XMLCollection(XMLTypeEnum.NPC, NPC_REF_TAGS, "", DEFAULT_NPC_TAGS),
                [XMLTypeEnum.Shop] = new XMLCollection(XMLTypeEnum.Shop, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA, DEFAULT_SHOP_TAGS),
                [XMLTypeEnum.Light] = new XMLCollection(XMLTypeEnum.Light, "", TAGS_FOR_LIGHTS, DEFAULT_LIGHT_TAGS),
                [XMLTypeEnum.Upgrade] = new XMLCollection(XMLTypeEnum.Upgrade, "", TAGS_FOR_UPGRADES, DEFAULT_UPGRADE_TAGS),
            };
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

        /// <summary>
        /// Loads the WorldObjectData
        /// </summary>
        private void LoadWorldObjects()
        {
            //Load in all the WorldObject Data
            _liWorldObjects = new List<XMLData>();
            Dictionary<string, Dictionary<string, string>> worldObjectDictionary = ReadTaggedXMLFile(WORLD_OBJECTS_DATA_XML_FILE);
            for (int i = 0; i < 1001; i++)
            {
                string strID = i.ToString();
                if (worldObjectDictionary.ContainsKey(strID))
                {
                    Dictionary<string, string> stringData = worldObjectDictionary[strID];
                    if (stringData != null)
                    {
                        _liWorldObjects.Add(new XMLData(strID, stringData, WORLD_OBJECT_REF_TAGS, TAGS_FOR_WORLD_OBJECTS, XMLTypeEnum.WorldObject, ref _diObjectText));
                    }
                }
                else { break; }
            }
        }

        private void LoadItemData()
        {
            //Load in all the Item Data
            _liItemData = new List<ItemXMLData>();
            Dictionary<string, Dictionary<string, string>> itemDictionary = ReadTaggedXMLFile(ITEM_DATA_XML_FILE);
            for (int i = 0; i < 1001; i++)
            {
                string strID = i.ToString();
                if (itemDictionary.ContainsKey(strID))
                {
                    Dictionary<string, string> stringData = itemDictionary[strID];
                    if (stringData != null)
                    {
                        _liItemData.Add(new ItemXMLData(strID, stringData, ITEM_REF_TAGS, TAGS_FOR_ITEMS, ref _diObjectText));
                    }
                }
                else { break; }
            }
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
            //Compare item Data against the other Items
            for (int i = 0; i < _liItemData.Count; i++)
            {
                ItemXMLData theData = _liItemData[i];
                for (int j = 0; j < _liItemData.Count; j++)
                {
                    _liItemData[j].CheckForObjectLink(theData);
                }
                FindLinkedXMLObjects(theData, _liMailbox);
                FindLinkedXMLObjectsInDictionary(theData, _diBasicXML);
                FindLinkedXMLObjectsInDictionary(theData, _diNPCDialogue);

                //Compare ItemData against the WorldObjectData

                foreach (XMLData testIt in _liWorldObjects)
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

            foreach (XMLData theData in _liWorldObjects)
            {
                FindLinkedXMLObjects(theData, _liMailbox);
                FindLinkedXMLObjects(theData, _liWorldObjects);

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
            foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
            {
                kvp.Value.ReferencesXMLObject(data);
            }
        }
        private void FindLinkedCutscenes(XMLData theData, ref Dictionary<int, List<string>> dictionaryData)
        {
            foreach (KeyValuePair<int, List<string>> kvp in dictionaryData)
            {
                bool weDone = false;
                foreach(string s in kvp.Value)
                {
                    foreach (string tag in theData.TagsThatReferToMe)
                    {
                        if(s.Contains(tag + ":"+ theData.ID + "-") || s.Contains(tag + ":" + theData.ID + "]"))
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

        private void ChangeIDs(ref List<ItemXMLData> itemDataList, ref List<XMLData> worldObjectDataList)
        {
            //Change all IDs
            int index = 0;

            foreach (ItemXMLData data in _liItemData)
            {
                if (data != null)
                {
                    if (data.ID == _diTabIndices["Items"]) { _iNextCurrID = index; }
                    data.ChangeID(index++);
                    itemDataList.Add(data);
                }
            }

            index = 0;
            foreach (XMLData data in _liWorldObjects)
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
            cbItemSubtype.Items.Clear();
            ItemEnum itemType = Util.ParseEnum<ItemEnum>(cbItemType.SelectedItem.ToString().Split(':')[1]);
            switch (itemType)
            {
                case ItemEnum.Clothing:
                    cbItemSubtype.Visible = true;
                    foreach (ClothingEnum en in Enum.GetValues(typeof(ClothingEnum)))
                    {
                        cbItemSubtype.Items.Add("Subtype:" + en.ToString());
                    }
                    break;
                case ItemEnum.NPCToken:
                    cbItemSubtype.Visible = true;
                    foreach (NPCTokenTypeEnum en in Enum.GetValues(typeof(NPCTokenTypeEnum)))
                    {
                        cbItemSubtype.Items.Add("Subtype:" + en.ToString());
                    }
                    break;
                case ItemEnum.Tool:
                    cbItemSubtype.Visible = true;
                    foreach (ToolEnum en in Enum.GetValues(typeof(ToolEnum)))
                    {
                        cbItemSubtype.Items.Add("Subtype:" + en.ToString());
                    }
                    break;
                default:
                    cbItemSubtype.Visible = false;
                    break;
            }

            if (cbItemSubtype.Visible)
            {
                cbItemSubtype.SelectedIndex = 0;
            }
        }

        private int GetSubtypeIndex(ItemEnum e, string value)
        {
            int rv = 0;
            switch (e)
            {
                case ItemEnum.Clothing:
                    rv = (int)Util.ParseEnum<ClothingEnum>(value);
                    break;
                case ItemEnum.NPCToken:
                    rv = (int)Util.ParseEnum<NPCTokenTypeEnum>(value);
                    break;
                case ItemEnum.Tool:
                    rv = (int)Util.ParseEnum<ToolEnum>(value);
                    break;
            }

            return rv;
        }

        #region Helpers
        private int GetTotalEntries()
        {
            int rv = 0;

            if (tabCtl.SelectedTab == tabCtl.TabPages["tabNPC"]) { rv = _diBasicXML[NPC_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabCutscene"]) { rv = _diBasicXML[CUTSCENE_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabDungeon"]) { rv = _diBasicXML[DUNGEON_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabItem"]) { rv = _liItemData.Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabLight"]) { rv = _diBasicXML[LIGHTS_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabShop"]) { rv = _diBasicXML[SHOPS_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabStatusEffect"]) { rv = _diBasicXML[STATUS_EFFECTS_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabTask"]) { rv = _diBasicXML[TASK_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabWorldObject"]) { rv = _liWorldObjects.Count; }

            return rv;
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
            XMLTypeEnum rv = XMLTypeEnum.None;

            if (fileName == TASK_XML_FILE) { rv = XMLTypeEnum.Task; }
            else if (fileName == NPC_XML_FILE) { rv = XMLTypeEnum.NPC; }
            else if (fileName == WORLD_OBJECTS_DATA_XML_FILE) { rv = XMLTypeEnum.WorldObject; }
            else if (fileName == STATUS_EFFECTS_XML_FILE) { rv = XMLTypeEnum.StatusEffect; }
            else if (fileName == LIGHTS_XML_FILE) { rv = XMLTypeEnum.Light; }
            else if (fileName == UPGRADES_XML_FILE) { rv = XMLTypeEnum.Upgrade; }
            else if (fileName == DUNGEON_XML_FILE) { rv = XMLTypeEnum.Dungeon; }
            else if (fileName == SHOPS_XML_FILE) { rv = XMLTypeEnum.Shop; }
            else if (fileName.Contains("Text Files")) { rv = XMLTypeEnum.TextFile; }

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

        public string GetName(XMLTypeEnum strType, ComponentTypeEnum e)
        {
            return GetName(Util.GetEnumString(strType), e);
        }
        public string GetName(string strType, ComponentTypeEnum e)
        {
            switch (e)
            {
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
            }

            return string.Empty;
        }

        private TabPage GetPage(string xmlType)
        {
            return tabCtl.TabPages["tab" + xmlType];
        }

        private TextBox FindTextBoxByName(XMLTypeEnum xmlType, ComponentTypeEnum type)
        {
            string strXMLType = Util.GetEnumString(xmlType);
            string name = GetName(strXMLType, type);

            return GetPage(strXMLType).Controls.OfType<TextBox>().FirstOrDefault(val => val.Name == name);
        }

        private DataGridView FindDGVByName(XMLTypeEnum xmlType, ComponentTypeEnum type)
        {
            string strXMLType = Util.GetEnumString(xmlType);
            string name = GetName(strXMLType, type);

            return GetPage(strXMLType).Controls.OfType<DataGridView>().FirstOrDefault(val => val.Name == name);
        }

        private ComboBox FindComboBoxByName(XMLTypeEnum xmlType, ComponentTypeEnum type)
        {
            string strXMLType = Util.GetEnumString(xmlType);
            string name = GetName(strXMLType, type);

            return GetPage(strXMLType).Controls.OfType<ComboBox>().FirstOrDefault(val => val.Name == name);
        }
        #endregion

        #region DataGridView Loading
        private void LoadDataGrids()
        {
            LoadItemDataGrid();
            LoadWorldObjectDataGrid();
            LoadNPCDataGrid();
            LoadTaskDataGrid();
            LoadStatusEffectDataGrid();
            LoadLightDataGrid();
            LoadCutsceneDataGrid();
            LoadShopsDataGrid();
            LoadDungeonDataGrid();
            LoadUpgradeDataGrid();
        }
        private void LoadGenericDatagrid(XMLCollection collection, List<XMLData> data, int selectRow, string filter = "All")
        {
            string colName = GetName(collection.XMLType, ComponentTypeEnum.ColumnName);
            DataGridView dgv = FindDGVByName(collection.XMLType, ComponentTypeEnum.DataGrid);

            dgv.Rows.Clear();
            int index = 0;
            _worldObjFilter = filter;
            for (int i = 0; i < data.Count; i++)
            {
                if (_worldObjFilter == "All" || data[i].GetTagValue("Type").ToString().Equals(_worldObjFilter))
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
        private void LoadItemDataGrid(string filter = "All", int selectedIndex = -1)
        {
            dgvItems.Rows.Clear();
            int index = 0;
            _itemFilter = filter;
            for (int i = 0; i < _liItemData.Count; i++)
            {
                if (filter == "All" || _liItemData[i].ItemType.ToString().Equals(_itemFilter))
                {
                    dgvItems.Rows.Add();
                    DataGridViewRow row = dgvItems.Rows[index++];

                    //row.Cells["colItemID"].Value = _liItemData[i].ID;
                    row.Cells["colItemsName"].Value = _liItemData[i].Name;
                }
            }

            SelectRow(dgvItems, selectedIndex == -1 ? _diTabIndices["Items"] : selectedIndex);
            dgvItems.Focus();
        }
        private void LoadWorldObjectDataGrid(string filter = "All", int selectedIndex = -1)
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.WorldObject], _liWorldObjects, selectedIndex == -1 ? _diTabIndices["WorldObjects"] : selectedIndex, filter);
        }
        private void LoadNPCDataGrid(string filter = "All", int selectedIndex = -1)
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.NPC], _diBasicXML[NPC_XML_FILE], selectedIndex == -1 ? _diTabIndices["NPCs"] : selectedIndex, filter);
        }
        private void LoadTaskDataGrid()
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Task], _diBasicXML[TASK_XML_FILE], _diTabIndices["Tasks"]);
        }
        private void LoadStatusEffectDataGrid()
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.StatusEffect], _diBasicXML[STATUS_EFFECTS_XML_FILE], _diTabIndices["StatusEffects"]);
        }
        private void LoadLightDataGrid()
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Light], _diBasicXML[LIGHTS_XML_FILE], _diTabIndices["Lights"]);
        }
        private void LoadUpgradeDataGrid()
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Upgrade], _diBasicXML[UPGRADES_XML_FILE], _diTabIndices["Upgrades"]);
        }
        private void LoadDungeonDataGrid()
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Dungeon], _diBasicXML[DUNGEON_XML_FILE], _diTabIndices["Dungeons"]);
        }
        private void LoadShopsDataGrid()
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Shop], _diBasicXML[SHOPS_XML_FILE], _diTabIndices["Shops"]);
        }
        private void LoadCutsceneDataGrid()
        {
            dgvCutscenes.Rows.Clear();
            foreach (KeyValuePair<int, List<string>> kvp in _diCutscenes)
            {
                dgvCutscenes.Rows.Add();

                DataGridViewRow row = dgvCutscenes.Rows[kvp.Key];
                //row.Cells["colCutscenesID"].Value = kvp.Key;
                row.Cells["colCutscenesName"].Value = GetTextValue(XMLTypeEnum.Cutscene, kvp.Key, "Name");
            }

            SelectRow(dgvCutscenes, _diTabIndices["Cutscenes"]);
            dgvCutscenes.Focus();
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
            string npcKey = @"\NPC_" + _diBasicXML[NPC_XML_FILE][_diTabIndices["NPCs"]].GetTagValue("Key") + ".xml";
            FormCharExtraData frm;
            if (cbEditableCharData.SelectedItem.ToString() == "Dialogue")
            {
                string key = PATH_TO_VILLAGER_DIALOGUE + npcKey;
                if (!_diNPCDialogue.ContainsKey(key))
                {
                    _diNPCDialogue[key] = new List<XMLData>();
                }

                frm = new FormCharExtraData("Dialogue", _diNPCDialogue[key], ref _diObjectText);
                frm.ShowDialog();

                _diNPCDialogue[key] = frm.StringData;
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
            string cutSceneFileName = String.Format(@"{0}\Cutscene_{1}.xml", PATH_TO_CUTSCENE_DIALOGUE, dgvCutscenes.CurrentRow.Cells["colCutscenesID"].Value.ToString());

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

            if (tabCtl.SelectedTab == tabCtl.TabPages["tabNPCs"]) { dgvNPCs.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabCutscenes"]) { dgvCutscenes.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabDungeons"]) { dgvDungeons.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabItems"]) { dgvItems.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabLights"]) { dgvLights.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabShops"]) { dgvShops.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabStatusEffects"]) { dgvStatusEffects.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabTasks"]) { dgvTasks.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabWorldObjects"]) { dgvWorldObjects.Focus(); }

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

            List<ItemXMLData> itemDataList = new List<ItemXMLData>();
            List<XMLData> worldObjectDataList = new List<XMLData>();

            if (SortIDs)
            {
                UpdateStatus("Sorting Started...");
                _liItemData.Sort((x, y) =>
                {
                    var typeComp = x.ItemType.CompareTo(y.ItemType);
                    if (typeComp == 0) { return x.ID.CompareTo(y.ID); }
                    else { return typeComp; }
                });
                _liWorldObjects.Sort((x, y) =>
                {
                    var typeComp = x.GetTagValue("Type").CompareTo(y.GetTagValue("Type"));
                    if (typeComp == 0) { return x.Name.CompareTo(y.Name); }
                    else { return typeComp; }
                });

                ChangeIDs(ref itemDataList, ref worldObjectDataList);

                //Strip the special case NPC from the Item files
                foreach (ItemXMLData data in _liItemData)
                {
                    if (data != null) { data.StripSpecialCharacter(); }
                }

                //Strip the special case NPC from the WorldObject files
                foreach (XMLData data in _liWorldObjects)
                {
                    data.StripSpecialCharacter();
                }

                //Sort the following Dictionaries by name
                List<XMLData> listToSort = _diBasicXML[NPC_XML_FILE];
                SortDictionaryByType(ref listToSort);
            }
            else
            {
                itemDataList = _liItemData;
                worldObjectDataList = _liWorldObjects;
            }

            UpdateStatus("Saving...");
            SaveXMLDataDictionary(_diBasicXML, sWriter);
            SaveXMLDataDictionary(_diNPCDialogue, sWriter);
            SaveXMLDataDictionary(_diCutsceneDialogue, sWriter);

            //SaveXMLDataDictionary(_diGameText, sWriter);
            //SaveXMLDataDictionary(_diMailbox, sWriter);
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

            string mapPath = PATH_TO_MAPS;
            if (!Directory.Exists(mapPath)) { Directory.CreateDirectory(mapPath); }
            foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
            {
                kvp.Value.StripSpecialCharacter();
                DirectoryInfo dirInfo = Directory.GetParent(kvp.Key);
                if (!Directory.Exists(dirInfo.FullName)) { Directory.CreateDirectory(dirInfo.FullName); }
                SaveTMXData(kvp.Value, kvp.Key);
            }

            SaveItemXMLData(itemDataList, PATH_TO_DATA, sWriter);
            SaveXMLData(worldObjectDataList, WORLD_OBJECTS_DATA_XML_FILE, sWriter);
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

            LoadDataGrids();
            LoadAllInfoPanels();

            UpdateStatus("Save Complete.");
        }

        private void UpdateStatus(string status)
        {
            tbStatus.Text = status;
        }

        private void AutoSave()
        {
            TabPage prevPage = tabCtl.TabPages[_diTabIndices["PreviousTab"]];

            if (prevPage == tabCtl.TabPages["tabNPCs"]) { SaveNPCInfo(); }      
            else if (prevPage == tabCtl.TabPages["tabCutscenes"]) { SaveCutsceneInfo(); }
            else if (prevPage == tabCtl.TabPages["tabDungeons"]) { SaveDungeonInfo(); }            
            else if (prevPage == tabCtl.TabPages["tabItems"]) { SaveItemInfo(); }
            else if (prevPage == tabCtl.TabPages["tabLights"]) { SaveLightInfo(); }
            else if (prevPage == tabCtl.TabPages["tabUpgrades"]) { SaveUpgradeInfo(); }
            else if (prevPage == tabCtl.TabPages["tabShops"]) { SaveShopInfo(); }
            else if (prevPage == tabCtl.TabPages["tabStatusEffects"]) { SaveStatusEffectInfo(); }
            else if (prevPage == tabCtl.TabPages["tabTasks"]) { SaveTaskInfo(); }
            else if (prevPage == tabCtl.TabPages["tabWorldObjects"]) { SaveWorldObjectInfo(); }
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

        public void SaveItemXMLData(List<ItemXMLData> dataList, string pathToDir, StreamWriter sWriter)
        {
            StreamWriter dataFile = PrepareXMLFile(pathToDir + @"\ItemData.xml", "Dictionary[int, string]");

            foreach (ItemXMLData data in dataList)
            {
                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>{0}</Value>", data.GetTagsString()));
                WriteXMLEntry(sWriter, string.Format("      <Key>Item_{0}</Key>", data.ID), string.Format("      <Value>[Name:{0}][Description:{1}]</Value>", data.Name, data.Description));
            }

            CloseStreamWriter(ref dataFile);
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
            if (dgv == dgvItems)
            {
                AddContextMenuItem("Add New", AddNewItem, true);
                AddContextMenuItem("All", dgvItemsContextMenuClick, false);

                foreach (string s in Enum.GetNames(typeof(ItemEnum)))
                {
                    AddContextMenuItem(s, dgvItemsContextMenuClick, false);
                }
            }
            else if (dgv == dgvWorldObjects)
            {
                AddContextMenuItem("Add New", AddNewWorldObject, true);
                AddContextMenuItem("All", dgvWorldObjectsContextMenuClick, false);

                foreach (string s in Enum.GetNames(typeof(ObjectTypeEnum)))
                {
                    if (!s.Equals("Earth"))
                    {
                        AddContextMenuItem(s, dgvWorldObjectsContextMenuClick, false);
                    }
                }
            }
            else if (dgv == dgvTasks) { AddContextMenuItem("Add New", AddNewTask, false); }
            else if (dgv == dgvCutscenes) { AddContextMenuItem("Add New", AddNewCutscene, false); }
            else if (dgv == dgvNPCs) {
                AddContextMenuItem("Add New", AddNewNPC, true);
                AddContextMenuItem("All", dgvNPCsContextMenuClick, false);
                foreach (string s in Enum.GetNames(typeof(WorldActorTypeEnum)))
                {
                    if (!s.Equals("Actor"))
                    {
                        AddContextMenuItem(s, dgvNPCsContextMenuClick, false);
                    }
                }
            }
            else if (dgv == dgvLights) { AddContextMenuItem("Add New", AddNewLight, false); }
            else if (dgv == dgvUpgrades) { AddContextMenuItem("Add New", AddNewUpgrade, false); }
            else if (dgv == dgvDungeons) { AddContextMenuItem("Add New", AddNewDungeon, false); }
            else if (dgv == dgvShops) { AddContextMenuItem("Add New", AddNewShop, false); }
        }

        private void AddContextMenuItem(string text, EventHandler triggeredEvent, bool separator)
        {
            ToolStripMenuItem tsmi = new ToolStripMenuItem(text);
            tsmi.Click += new EventHandler(triggeredEvent);
            contextMenu.Items.Add(tsmi);

            if (separator)
            {
                contextMenu.Items.Add(new ToolStripSeparator());
            }
        }

        #region Filters
        private void dgvItemsContextMenuClick(object sender, EventArgs e)
        {
            _diTabIndices["Items"] = 0;
            LoadItemDataGrid(((ToolStripMenuItem)sender).Text, 0);
            LoadItemInfo();
        }

        private void dgvWorldObjectsContextMenuClick(object sender, EventArgs e)
        {
            _diTabIndices["WorldObjects"] = 0;
            LoadWorldObjectDataGrid(((ToolStripMenuItem)sender).Text, 0);
            LoadWorldObjectInfo();
        }

        private void dgvNPCsContextMenuClick(object sender, EventArgs e)
        {
            _diTabIndices["NPCs"] = 0;
            LoadNPCDataGrid(((ToolStripMenuItem)sender).Text, 0);
            LoadNPCInfo();
        }
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
        private void AddNewGenericXMLObject(XMLCollection collection)
        {
            string tabIndex = GetName(collection.XMLType, ComponentTypeEnum.TabIndex);
            string columnName = GetName(collection.XMLType, ComponentTypeEnum.ColumnName);
            string columnTags = GetName(collection.XMLType, ComponentTypeEnum.ColumnTags);
            TextBox tbName = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxName);
            TextBox tbID = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxID);
            TextBox tbDescription = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxDescription);
            DataGridView dgv = FindDGVByName(collection.XMLType, ComponentTypeEnum.DataGrid);
            DataGridView dgvTags = FindDGVByName(collection.XMLType, ComponentTypeEnum.DataGridTags);
            ComboBox cb = FindComboBoxByName(collection.XMLType, ComponentTypeEnum.ComboBoxType);

            _diTabIndices[tabIndex] = dgv.Rows.Count;
            dgv.Rows.Add();
            SelectRow(dgv, _diTabIndices[tabIndex]);

            DataGridViewRow row = dgv.Rows[_diTabIndices[tabIndex]];
            //row.Cells[collection.ColumnID].Value = _diTabIndices[tabIndex];
            row.Cells[columnName].Value = "";

            tbName.Text = "";
            tbID.Text = GetTotalEntries().ToString();
            if (tbDescription != null)
            {
                tbDescription.Text = "";
            }

            if (cb != null)
            {
                cb.SelectedIndex = 0;
            }

            dgvTags.Rows.Clear();

            if (collection.DefaultTags != null)
            {
                foreach (string s in collection.DefaultTags) { dgvTags.Rows.Add(); }
                for (int i = 0; i < collection.DefaultTags.Count; i++)
                {
                    dgvTags.Rows[i].Cells[columnTags].Value = collection.DefaultTags[i];
                }
            }

            tbName.Focus();
        }
        private void AddNewItem(object sender, EventArgs e)
        {
            SaveItemInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Item]);
        }
        private void AddNewWorldObject(object sender, EventArgs e)
        {
            SaveWorldObjectInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.WorldObject]);
        }
        private void AddNewTask(object sender, EventArgs e)
        {
            SaveTaskInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Task]);
        }
        private void AddNewCutscene(object sender, EventArgs e)
        {
            SaveCutsceneInfo();
            tbCutsceneTriggers.Clear();
            tbCutsceneDetails.Clear();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Cutscene]);
        }
        private void AddNewNPC(object sender, EventArgs e)
        {
            SaveNPCInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.NPC]);
        }
        private void AddNewLight(object sender, EventArgs e)
        {
            SaveLightInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Light]);
        }
        private void AddNewUpgrade(object sender, EventArgs e)
        {
            SaveUpgradeInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Upgrade]);
        }
        private void AddNewDungeon(object sender, EventArgs e)
        {
            SaveDungeonInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Dungeon]);
        }
        private void AddNewShop(object sender, EventArgs e)
        {
            SaveShopInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Shop]);
        }
        #endregion

        #region Load Info Panes
        private void LoadAllInfoPanels()
        {
            LoadItemInfo();
            LoadWorldObjectInfo();
            LoadNPCInfo();
            LoadTaskInfo();
            LoadCutsceneInfo();
            LoadShopInfo();
            LoadStatusEffectInfo();
            LoadLightInfo();
            LoadUpgradeInfo();
            LoadDungeonInfo();
        }

        private void LoadGenericDataInfo(XMLData data, XMLCollection collection)
        {
            TextBox tbName = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxName);
            TextBox tbID = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxID);
            TextBox tbDescription = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxDescription);
            DataGridView dgvTags = FindDGVByName(collection.XMLType, ComponentTypeEnum.DataGridTags);

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
                if (!s.StartsWith("Type"))
                {
                    dgvTags.Rows.Add(s);
                }
            }
        }
        private void LoadItemInfo()
        {
            DataGridViewRow r = dgvItems.SelectedRows[0];

            ItemXMLData data = null;
            if (_itemFilter == "All") { data = _liItemData[r.Index]; }
            else { data = _liItemData.FindAll(x => x.ItemType.ToString().Equals(_itemFilter))[r.Index]; }

            tbItemName.Text = data.Name;
            tbItemDesc.Text = data.Description;
            tbItemID.Text = data.ID.ToString();

            cbItemType.SelectedIndex = (int)data.ItemType;
            SetItemSubtype();

            dgvItemTags.Rows.Clear();
            string[] tags = data.GetTagsString().Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (s.StartsWith("Subtype"))
                {
                    cbItemSubtype.SelectedIndex = GetSubtypeIndex(data.ItemType, s.Split(':')[1]);
                }
                else if (!s.StartsWith("Type"))
                {
                    dgvItemTags.Rows.Add(s);
                }
            }
        }
        private void LoadWorldObjectInfo()
        {
            DataGridViewRow r = dgvWorldObjects.SelectedRows[0];
            XMLData data = null;
            if (_worldObjFilter == "All") { data = _liWorldObjects[r.Index]; }
            else { data = _liWorldObjects.FindAll(x => x.GetTagValue("Type").ToString().Equals(_worldObjFilter))[r.Index]; }

            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.WorldObject]);
            cbWorldObjectType.SelectedIndex = (int)Util.ParseEnum<ObjectTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadNPCInfo()
        {
            DataGridViewRow r = dgvNPCs.SelectedRows[0];
            XMLData data = null;
            if (_worldObjFilter == "All") { data = _diBasicXML[NPC_XML_FILE][_diTabIndices["NPCs"]]; }
            else { data = _diBasicXML[NPC_XML_FILE].FindAll(x => x.GetTagValue("Type").ToString().Equals(_worldObjFilter))[r.Index]; }
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.NPC]);

            for (int i = 0; i < cbNPCType.Items.Count; i++)
            {
                if (cbNPCType.Items[i].ToString().Split(':')[1] == data.GetTagValue("Type"))
                {
                    cbNPCType.SelectedIndex = i;
                    break;
                }
            }
        }
        private void LoadTaskInfo()
        {
            XMLData data = _diBasicXML[TASK_XML_FILE][_diTabIndices["Tasks"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Task]);
            cbTaskType.SelectedIndex = (int)Util.ParseEnum<TaskTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadStatusEffectInfo()
        {
            XMLData data = _diBasicXML[STATUS_EFFECTS_XML_FILE][_diTabIndices["StatusEffects"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.StatusEffect]);
            cbStatusEffect.SelectedIndex = (int)Util.ParseEnum<StatusTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadLightInfo()
        {
            XMLData data = _diBasicXML[LIGHTS_XML_FILE][_diTabIndices["Lights"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Light]);
        }
        private void LoadUpgradeInfo()
        {
            XMLData data = _diBasicXML[UPGRADES_XML_FILE][_diTabIndices["Upgrades"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Upgrade]);
        }
        private void LoadDungeonInfo()
        {
            XMLData data = _diBasicXML[DUNGEON_XML_FILE][_diTabIndices["Dungeons"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Dungeon]);
        }
        private void LoadCutsceneInfo()
        {
            List<string> listData = _diCutscenes[_diTabIndices["Cutscenes"]];

            tbCutsceneName.Text = GetTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name");
            tbCutsceneID.Text = _diTabIndices["Cutscenes"].ToString();
            tbCutsceneTriggers.Text = listData[0];
            tbCutsceneDetails.Text = listData[1];

            dgvCutsceneTags.Rows.Clear();
            string[] tags = listData[2].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (!s.StartsWith("Type"))
                {
                    dgvCutsceneTags.Rows.Add(s);
                }
            }
        }
        private void LoadShopInfo()
        {
            XMLData data = _diBasicXML[SHOPS_XML_FILE][_diTabIndices["Shops"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Shop]);

            //tbShopName.Text = GetTextValue(XMLTypeEnum.Shop, _diTabIndices["Shops"], "Name");

            //dgvShopTags.Rows.Clear();
            //foreach (XMLData d in _diShops[_diTabIndices["Shops"]])
            //{
            //    dgvShopTags.Rows.Add(d.GetTagsString());
            //}
        }
        #endregion

        #region SaveInfo
        private void SaveXMLDataInfo(List<XMLData> liData, XMLCollection collection)
        {
            string tabIndex = GetName(collection.XMLType, ComponentTypeEnum.TabIndex);
            string columnName = GetName(collection.XMLType, ComponentTypeEnum.ColumnName);
            string columnTags = GetName(collection.XMLType, ComponentTypeEnum.ColumnTags);
            TextBox tbName = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxName);
            TextBox tbID = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxID);
            TextBox tbDescription = FindTextBoxByName(collection.XMLType, ComponentTypeEnum.TextBoxDescription);
            DataGridView dgv = FindDGVByName(collection.XMLType, ComponentTypeEnum.DataGrid);
            DataGridView dgvTags = FindDGVByName(collection.XMLType, ComponentTypeEnum.DataGridTags);
            ComboBox cb = FindComboBoxByName(collection.XMLType, ComponentTypeEnum.ComboBoxType);

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
        private void SaveItemInfo()
        {
            UpdateStatus("Saving Items");
            ItemXMLData data;
            if (_liItemData.Count == _diTabIndices["Items"])
            {
                Dictionary<string, string> diText = new Dictionary<string, string>
                {
                    ["Name"] = tbItemName.Text,
                    ["Description"] = tbItemDesc.Text
                };

                _diObjectText["Item_" + tbItemID.Text] = diText;

                Dictionary<string, string> tags = new Dictionary<string, string>();

                string[] typeTag = cbItemType.SelectedItem.ToString().Split(':');
                tags[typeTag[0]] = typeTag[1];
                if (cbItemSubtype.Visible)
                {
                    string[] subTypeTag = cbItemSubtype.SelectedItem.ToString().Split(':');
                    tags[subTypeTag[0]] = subTypeTag[1];
                }
                foreach (DataGridViewRow row in dgvItemTags.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string key = tagInfo[0];
                        string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                        if (key != "Type" && key != "Subtype")
                        {
                            tags[key] = val;
                        }
                    }
                }

                data = new ItemXMLData(tbItemID.Text, tags, ITEM_REF_TAGS, TAGS_FOR_ITEMS, ref _diObjectText);
                _liItemData.Add(data);
            }
            else
            {
                data = _liItemData[int.Parse(tbItemID.Text)];
                data.SetTextData(tbItemName.Text, tbItemDesc.Text);

                data.ClearTagInfo();
                string[] typeTag = cbItemType.SelectedItem.ToString().Split(':');
                data.SetTagInfo(typeTag[0], typeTag[1]);
                data.SetItemType(Util.ParseEnum<ItemEnum>(typeTag[1]));
                if (cbItemSubtype.Visible)
                {
                    string[] subTypeTag = cbItemSubtype.SelectedItem.ToString().Split(':');
                    data.SetTagInfo(subTypeTag[0], subTypeTag[1]);
                }
                foreach (DataGridViewRow row in dgvItemTags.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string key = tagInfo[0];
                        string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                        if (key != "Type" && key != "Subtype")
                        {
                            data.SetTagInfo(key, val);
                        }
                    }
                }
            }

            int ID = int.Parse(tbItemID.Text);
            if (ID != data.ID)
            {
                int oldID = data.ID;
                _liItemData.Remove(data);
                _liItemData.Insert(ID, data);
                data.ChangeID(ID);
                foreach (ItemXMLData d in _liItemData)
                {
                    if (d != data)
                    {
                        if (d.ID >= ID && d.ID < oldID)
                        {
                            d.ChangeID(d.ID + 1);
                        }
                        else if (d.ID >= oldID)
                        {
                            //    d.ChangeID(d.ID - 1);
                        }
                    }
                }

                LoadItemDataGrid();
                SelectRow(dgvItems, ID);
            }
            else
            {
                DataGridViewRow updatedRow = dgvItems.Rows[_diTabIndices["Items"]];

                //updatedRow.Cells["colItemID"].Value = data.ID;
                updatedRow.Cells["colItemsName"].Value = data.Name;
            }
        }
        private void SaveWorldObjectInfo()
        {
            SaveXMLDataInfo(_liWorldObjects, _diTabCollections[XMLTypeEnum.WorldObject]);
        }
        private void SaveNPCInfo()
        {
            SaveXMLDataInfo(_diBasicXML[NPC_XML_FILE], _diTabCollections[XMLTypeEnum.NPC]);
        }
        private void SaveTaskInfo()
        {
            SaveXMLDataInfo(_diBasicXML[TASK_XML_FILE], _diTabCollections[XMLTypeEnum.Task]);
        }
        private void SaveStatusEffectInfo()
        {
            SaveXMLDataInfo(_diBasicXML[STATUS_EFFECTS_XML_FILE], _diTabCollections[XMLTypeEnum.StatusEffect]);
        }
        private void SaveLightInfo()
        {
            SaveXMLDataInfo(_diBasicXML[LIGHTS_XML_FILE], _diTabCollections[XMLTypeEnum.Light]);
        }
        private void SaveUpgradeInfo()
        {
            SaveXMLDataInfo(_diBasicXML[UPGRADES_XML_FILE], _diTabCollections[XMLTypeEnum.Upgrade]);
        }
        private void SaveDungeonInfo()
        {
            SaveXMLDataInfo(_diBasicXML[DUNGEON_XML_FILE], _diTabCollections[XMLTypeEnum.Dungeon]);
        }
        private void SaveShopInfo()
        {
            SaveXMLDataInfo(_diBasicXML[SHOPS_XML_FILE], _diTabCollections[XMLTypeEnum.Shop]);
        }

        private void SaveCutsceneInfo()
        {
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
            foreach (DataGridViewRow r in dgvCutsceneTags.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    tags += "[" + r.Cells[0].Value + "]";
                }
            }
            listData.Add(tags);
            UpdateTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name", tbCutsceneName.Text);

            DataGridViewRow updatedRow = dgvCutscenes.Rows[_diTabIndices["Cutscenes"]];

            //updatedRow.Cells["colCutscenesID"].Value = _diTabIndices["Cutscenes"];
            updatedRow.Cells["colCutscenesName"].Value = GetTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name");
        }

        #endregion

        #region DataGridViewCell_Click
        private void ProcessCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                if (sender == dgvDungeons) { GenericCellClick(e, "Dungeons", LoadDungeonInfo, SaveDungeonInfo); }
                else if (sender == dgvLights) { GenericCellClick(e, "Lights", LoadLightInfo, SaveLightInfo); }
                else if (sender == dgvNPCs) { GenericCellClick(e, "NPCs", LoadNPCInfo, SaveNPCInfo); }
                else if (sender == dgvShops) { GenericCellClick(e, "Shops", LoadShopInfo, SaveShopInfo); }
                else if (sender == dgvStatusEffects) { GenericCellClick(e, "StatusEffects", LoadStatusEffectInfo, SaveStatusEffectInfo); }
                else if (sender == dgvTasks) { GenericCellClick(e, "Tasks", LoadTaskInfo, SaveTaskInfo); }
                else if (sender == dgvUpgrades) { GenericCellClick(e, "Upgrades", LoadUpgradeInfo, SaveUpgradeInfo); }
                else if (sender == dgvWorldObjects) { GenericCellClick(e, "WorldObjects", LoadWorldObjectInfo, SaveWorldObjectInfo); }
                else if (sender == dgvCutscenes)
                {
                    SaveCutsceneInfo();
                    _diTabIndices["Cutscenes"] = e.RowIndex;
                    LoadCutsceneInfo();
                }
                else if (sender == dgvItems)
                {
                    SaveItemInfo();
                    _diTabIndices["Items"] = e.RowIndex;
                    LoadItemInfo();
                }
            }
        }
        private void GenericCellClick(DataGridViewCellEventArgs e, string tabIndex, VoidDelegate loadDel, XMLListDataDelegate saveDel)
        {
            if (e.RowIndex > -1)
            {
                saveDel();
                _diTabIndices[tabIndex] = e.RowIndex;
                loadDel();
            }
        }
        #endregion

        #region Cancel Button
        private void ProcessCancel_Click(object sender, EventArgs e)
        {
            if (sender == btnDungeonCancel) { GenericCancel(_diBasicXML[DUNGEON_XML_FILE], "Dungeon", dgvDungeons, LoadDungeonInfo); }
            else if (sender == btnLightCancel) { GenericCancel(_diBasicXML[LIGHTS_XML_FILE], "Light", dgvLights, LoadLightInfo); }
            else if (sender == btnNPCCancel) { GenericCancel(_diBasicXML[NPC_XML_FILE], "NPCs", dgvNPCs, LoadNPCInfo); }
            else if (sender == btnShopCancel) { GenericCancel(_diBasicXML[SHOPS_XML_FILE], "Shops", dgvShops, LoadShopInfo); }
            else if (sender == btnStatusEffectCancel) { GenericCancel(_diBasicXML[STATUS_EFFECTS_XML_FILE], "StatusEffects", dgvStatusEffects, LoadStatusEffectInfo); }
            else if (sender == btnTaskCancel) { GenericCancel(_diBasicXML[TASK_XML_FILE], "Tasks", dgvTasks, LoadTaskInfo); }
            else if (sender == btnUpgradeCancel) { GenericCancel(_diBasicXML[UPGRADES_XML_FILE], "Upgrades", dgvUpgrades, LoadUpgradeInfo); }
            else if (sender == btnWorldObjectCancel) { GenericCancel(_liWorldObjects, "WorldObjects", dgvWorldObjects, LoadWorldObjectInfo); }
            else if (sender == btnCutsceneCancel)
            {
                if (_diCutscenes.Count == _diTabIndices["Cutscenes"])
                {
                    dgvCutscenes.Rows.RemoveAt(_diTabIndices["Cutscenes"]--);
                    SelectRow(dgvCutscenes, _diTabIndices["Cutscenes"]);
                }
                LoadCutsceneInfo();
            }
            else if (sender == btnItemCancel)
            {
                if (_liItemData.Count == _diTabIndices["Items"])
                {
                    dgvItems.Rows.RemoveAt(_diTabIndices["Items"]--);
                    SelectRow(dgvItems, _diTabIndices["Items"]);
                }

                LoadItemInfo();
            }
        }
        private void GenericCancel(List<XMLData> liData, string tabIndex, DataGridView dgMain, VoidDelegate del)
        {
            if (liData.Count == _diTabIndices[tabIndex])
            {
                dgMain.Rows.RemoveAt(_diTabIndices[tabIndex]--);
                SelectRow(dgMain, _diTabIndices[tabIndex]);
            }
            del();
        }
        #endregion
    }
}
