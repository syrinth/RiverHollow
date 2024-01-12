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
        Dictionary<string, int> _diTabIndices;

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

            cbActorType.Items.Clear();
            InitComboBox<ActorTypeEnum>(cbActorType, true);

            _diTabIndices = new Dictionary<string, int>()
            {
                { "PreviousTab", 0 },
                { "Items", 0 },
                { "WorldObjects", 0 },
                { "Actors", 0 },
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
            LoadXMLDictionary(TASK_XML_FILE, TASK_REF_TAGS, "", ref _diBasicXML);
            LoadXMLDictionary(ACTOR_XML_FILE, ACTOR_REF_TAGS, TAGS_FOR_ACTORS, ref _diBasicXML);
            LoadXMLDictionary(CONFIG_XML_FILE, CONFIG_REF_TAG, "", ref _diBasicXML);
            LoadXMLDictionary(STATUS_EFFECTS_XML_FILE, "", TAGS_FOR_STATUS_EFFECTS, ref _diBasicXML);
            LoadXMLDictionary(LIGHTS_XML_FILE, "", TAGS_FOR_LIGHTS, ref _diBasicXML);
            LoadXMLDictionary(UPGRADES_XML_FILE, "", TAGS_FOR_UPGRADES, ref _diBasicXML);
            LoadXMLDictionary(DUNGEON_XML_FILE, DUNGEON_REF_TAGS, TAGS_FOR_DUNGEONS, ref _diBasicXML);
            LoadXMLDictionary(SHOPS_XML_FILE, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA, ref _diBasicXML);
            LoadXMLDictionary(ITEM_DATA_XML_FILE, ITEM_REF_TAGS, TAGS_FOR_ITEMS, ref _diBasicXML);
            LoadXMLDictionary(WORLD_OBJECTS_DATA_XML_FILE, WORLD_OBJECT_REF_TAGS, TAGS_FOR_WORLD_OBJECTS, ref _diBasicXML);

            //_diShops = ReadXMLFileToXMLDataListDictionary(SHOPS_XML_FILE, XMLTypeEnum.Shop, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA);
            _diCutscenes = ReadXMLFileToIntKeyDictionaryStringList(CUTSCENE_XML_FILE);

            FindLinkedXMLObjects();

            LoadDataGrids();
            LoadAllInfoPanels();
        }


        private void SetupTabCollections()
        {
            _diTabCollections = new Dictionary<XMLTypeEnum, XMLCollection>
            {
                [XMLTypeEnum.WorldObject] = new XMLCollection(XMLTypeEnum.WorldObject, WORLD_OBJECT_REF_TAGS, TAGS_FOR_WORLD_OBJECTS),
                [XMLTypeEnum.Task] = new XMLCollection(XMLTypeEnum.Task, TASK_REF_TAGS, TAGS_FOR_TASKS),
                [XMLTypeEnum.Cutscene] = new XMLCollection(XMLTypeEnum.Cutscene, CUTSCENE_REF_TAGS, ""),
                [XMLTypeEnum.StatusEffect] = new XMLCollection(XMLTypeEnum.StatusEffect, "", TAGS_FOR_STATUS_EFFECTS),
                [XMLTypeEnum.Dungeon] = new XMLCollection(XMLTypeEnum.Dungeon, DUNGEON_REF_TAGS, TAGS_FOR_DUNGEONS),
                [XMLTypeEnum.Item] = new XMLCollection(XMLTypeEnum.Item, ITEM_REF_TAGS, TAGS_FOR_ITEMS),
                [XMLTypeEnum.Actor] = new XMLCollection(XMLTypeEnum.Actor, ACTOR_REF_TAGS, ""),
                [XMLTypeEnum.Shop] = new XMLCollection(XMLTypeEnum.Shop, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA),
                [XMLTypeEnum.Light] = new XMLCollection(XMLTypeEnum.Light, "", TAGS_FOR_LIGHTS),
                [XMLTypeEnum.Upgrade] = new XMLCollection(XMLTypeEnum.Upgrade, "", TAGS_FOR_UPGRADES),
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

        #region Find Linked Objects
        /// <summary>
        /// This iterates through all the ItemData entries and compares them against
        /// the other objects, ensuring that eachobject knows about any object that
        /// rfeferences it.
        /// </summary>
        /// <param name="dataList"></param>
        private void FindLinkedXMLObjects()
        {
            var items = _diBasicXML[ITEM_DATA_XML_FILE];
            var worldObjects = _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE];
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

        private void ChangeIDs(ref List<XMLData> itemDataList, ref List<XMLData> worldObjectDataList)
        {
            //Change all IDs
            int index = 0;

            foreach (XMLData data in _diBasicXML[ITEM_DATA_XML_FILE])
            {
                if (data != null)
                {
                    data.ChangeID(index++);
                    itemDataList.Add(data);
                }
            }

            index = 0;
            foreach (XMLData data in _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE])
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
                    SubtypeHelper<EquipmentEnum>();
                    break;
                case ItemEnum.Consumable:
                    cbItemSubtype.Visible = true;
                    cbItemSubtype.Items.Add("Subtype:" + ItemGroupEnum.None.ToString());
                    cbItemSubtype.Items.Add("Subtype:" + ItemGroupEnum.Potion.ToString());
                    break;
                case ItemEnum.Food:
                    cbItemSubtype.Items.Add("Subtype:" + ItemGroupEnum.Food.ToString());
                    cbItemSubtype.Items.Add("Subtype:" + ItemGroupEnum.Meal.ToString());
                    break;
                case ItemEnum.NPCToken:
                    SubtypeHelper<NPCTokenTypeEnum>();
                    break;
                case ItemEnum.Resource:
                    SubtypeHelper<ItemGroupEnum>(new List<ItemGroupEnum>() { ItemGroupEnum.Food, ItemGroupEnum.Meal });
                    break;
                case ItemEnum.Tool:
                    SubtypeHelper<ToolEnum>();
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

        private void SubtypeHelper<T>(List<T> skip = null)
        {
            cbItemSubtype.Visible = true;
            foreach (T en in Enum.GetValues(typeof(T)))
            {
                if (skip != null && skip.Contains(en))
                {
                    continue;
                }
                else
                {
                    cbItemSubtype.Items.Add("Subtype:" + en.ToString());
                }
            }
        }

        private void SetActorSubtype()
        {
            cbActorSubtype.Items.Clear();
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
                    break;
                default:
                    cbActorSubtype.Visible = false;
                    break;
            }

            if (cbActorSubtype.Visible)
            {
                cbActorSubtype.SelectedIndex = 0;
            }
        }

        #region Helpers
        private int GetTotalEntries()
        {
            int rv = 0;

            if (tabCtl.SelectedTab == tabCtl.TabPages["tabActor"]) { rv = _diBasicXML[ACTOR_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabCutscene"]) { rv = _diBasicXML[CUTSCENE_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabDungeon"]) { rv = _diBasicXML[DUNGEON_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabItem"]) { rv = _diBasicXML[ITEM_DATA_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabLight"]) { rv = _diBasicXML[LIGHTS_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabShop"]) { rv = _diBasicXML[SHOPS_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabStatusEffect"]) { rv = _diBasicXML[STATUS_EFFECTS_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabTask"]) { rv = _diBasicXML[TASK_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabUpgrade"]) { rv = _diBasicXML[UPGRADES_XML_FILE].Count; }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabWorldObject"]) { rv = _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE].Count; }

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
            else if (fileName == ACTOR_XML_FILE) { rv = XMLTypeEnum.Actor; }
            else if (fileName == WORLD_OBJECTS_DATA_XML_FILE) { rv = XMLTypeEnum.WorldObject; }
            else if (fileName == STATUS_EFFECTS_XML_FILE) { rv = XMLTypeEnum.StatusEffect; }
            else if (fileName == LIGHTS_XML_FILE) { rv = XMLTypeEnum.Light; }
            else if (fileName == UPGRADES_XML_FILE) { rv = XMLTypeEnum.Upgrade; }
            else if (fileName == DUNGEON_XML_FILE) { rv = XMLTypeEnum.Dungeon; }
            else if (fileName == SHOPS_XML_FILE) { rv = XMLTypeEnum.Shop; }
            else if (fileName == ITEM_DATA_XML_FILE) { rv = XMLTypeEnum.Item; }
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
                case ComponentTypeEnum.ComboBoxSubtype:
                    return "cb" + strType + "Subtype";
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
            LoadItemDataGrid();
            LoadWorldObjectDataGrid();
            LoadActorDataGrid();
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
        private void LoadItemDataGrid(string filter = "All", int selectedIndex = -1)
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Item], _diBasicXML[ITEM_DATA_XML_FILE], selectedIndex == -1 ? _diTabIndices["Items"] : selectedIndex, filter);
        }
        private void LoadWorldObjectDataGrid(string filter = "All", int selectedIndex = -1)
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.WorldObject], _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE], selectedIndex == -1 ? _diTabIndices["WorldObjects"] : selectedIndex, filter);
        }
        private void LoadActorDataGrid(string filter = "All", int selectedIndex = -1)
        {
            LoadGenericDatagrid(_diTabCollections[XMLTypeEnum.Actor], _diBasicXML[ACTOR_XML_FILE], selectedIndex == -1 ? _diTabIndices["Actors"] : selectedIndex, filter);
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

        private void cbActorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetActorSubtype();
        }

        private void cbWorldObjectType_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            if (tabCtl.SelectedTab == tabCtl.TabPages["tabActor"])
            {
                LoadActorDataGrid();
                dgvActors.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabCutscene"])
            {
                LoadCutsceneDataGrid();
                dgvCutscenes.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabDungeon"])
            {
                LoadDungeonDataGrid();
                dgvDungeons.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabItem"])
            {
                LoadItemDataGrid();
                dgvItems.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabLight"])
            {
                LoadLightDataGrid();
                dgvLights.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabShop"])
            {
                LoadShopsDataGrid();
                dgvShops.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabStatusEffect"])
            {
                LoadStatusEffectDataGrid();
                dgvStatusEffects.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabTask"])
            {
                LoadTaskDataGrid();
                dgvTasks.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabWorldObject"])
            {
                LoadWorldObjectDataGrid();
                dgvWorldObjects.Focus();
            }

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

            if (prevPage == tabCtl.TabPages["tabNPCs"]) { SaveActorInfo(); }      
            else if (prevPage == tabCtl.TabPages["tabCutscenes"]) { SaveCutsceneInfo(); }
            else if (prevPage == tabCtl.TabPages["tabDungeons"]) { SaveDungeonInfo(); }            
            else if (prevPage == tabCtl.TabPages["tabItems"]) { SaveItemInfo(); }
            else if (prevPage == tabCtl.TabPages["tabLights"]) { SaveLightInfo(); }
            else if (prevPage == tabCtl.TabPages["tabUpgrade"]) { SaveUpgradeInfo(); }
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
                var items = Enum.GetNames(typeof(ItemEnum)).ToList();
                items.Remove(Util.GetEnumString(ItemEnum.Buildable));
                AddContextMenuItem("Add New", AddNewItem, true, items.ToArray());
                AddContextMenuItem("All", dgvItemsContextMenuClick, false);

                foreach (ItemEnum en in Enum.GetValues(typeof(ItemEnum)))
                {
                    var s = Util.GetEnumString(en);
                    switch (en)
                    {
                        case ItemEnum.Buildable:
                            continue;
                        case ItemEnum.Resource:
                            var names = Enum.GetNames(typeof(ItemGroupEnum)).ToList();
                            names.Insert(0, "All");
                            names.Remove(Util.GetEnumString(ItemGroupEnum.Food));
                            names.Remove(Util.GetEnumString(ItemGroupEnum.Meal));
                            AddContextMenuItem(s, dgvItemsContextMenuClick, false, names.ToArray());
                            break;
                        case ItemEnum.Food:
                            AddContextMenuItem(s, dgvItemsContextMenuClick, false, new string[] { "All", Util.GetEnumString(ItemGroupEnum.Food), Util.GetEnumString(ItemGroupEnum.Meal) });
                            break;
                        case ItemEnum.Consumable:
                            AddContextMenuItem(s, dgvItemsContextMenuClick, false, new string[] { "All", Util.GetEnumString(ItemGroupEnum.None), Util.GetEnumString(ItemGroupEnum.Potion) });
                            break;
                        default:
                            AddContextMenuItem(s, dgvItemsContextMenuClick, false);
                            break;

                    }
                }
            }
            else if (dgv == dgvWorldObjects)
            {
                AddContextMenuItem("Add New", AddNewWorldObject, true, Enum.GetNames(typeof(ObjectTypeEnum)));
                AddContextMenuItem("All", dgvWorldObjectsContextMenuClick, false);

                foreach (ObjectTypeEnum en in Enum.GetValues(typeof(ObjectTypeEnum)))
                {
                    var s = Util.GetEnumString(en);
                    switch (en)
                    {
                        case ObjectTypeEnum.Buildable:
                            var names = Enum.GetNames(typeof(BuildableEnum)).ToList();
                            names.Insert(0, "All");
                            AddContextMenuItem(s, dgvWorldObjectsContextMenuClick, false, names.ToArray());
                            break;
                        default:
                            AddContextMenuItem(s, dgvWorldObjectsContextMenuClick, false);
                            break;
                    }
                }
            }
            else if (dgv == dgvTasks) { AddContextMenuItem("Add New", AddNewTask, false, Enum.GetNames(typeof(TaskTypeEnum))); }
            else if (dgv == dgvCutscenes) { AddContextMenuItem("Add New", AddNewCutscene, false); }
            else if (dgv == dgvActors) {
                AddContextMenuItem("Add New", AddNewActor, true, Enum.GetNames(typeof(ActorTypeEnum)));
                AddContextMenuItem("All", dgActorsContextMenuClick, false);
                foreach (string s in Enum.GetNames(typeof(ActorTypeEnum)))
                {
                    if (!s.Equals("Actor"))
                    {
                        AddContextMenuItem(s, dgActorsContextMenuClick, false);
                    }
                }
            }
            else if (dgv == dgvLights) { AddContextMenuItem("Add New", AddNewLight, false); }
            else if (dgv == dgvUpgrades) { AddContextMenuItem("Add New", AddNewUpgrade, false); }
            else if (dgv == dgvDungeons) { AddContextMenuItem("Add New", AddNewDungeon, false); }
            else if (dgv == dgvShops) { AddContextMenuItem("Add New", AddNewShop, false); }
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
        private void dgvItemsContextMenuClick(object sender, EventArgs e)
        {
            _diTabIndices["Items"] = 0;
            var selection = ((ToolStripMenuItem)sender);
            if (selection.OwnerItem == null)
            {
                _subtypeFilter = "All";
                LoadItemDataGrid(selection.Text, 0);
            }
            else
            {
                _subtypeFilter = selection.Text;
                LoadItemDataGrid(selection.OwnerItem.Text, 0);
            }
            LoadItemInfo();
        }

        private void dgvWorldObjectsContextMenuClick(object sender, EventArgs e)
        {
            _diTabIndices["WorldObjects"] = 0;
            var selection = ((ToolStripMenuItem)sender);
            if (selection.OwnerItem == null)
            {
                _subtypeFilter = "All";
                LoadWorldObjectDataGrid(selection.Text, 0);
            }
            else
            {
                _subtypeFilter = selection.Text;
                LoadWorldObjectDataGrid(selection.OwnerItem.Text, 0);
            }
            LoadWorldObjectInfo();
        }

        private void dgActorsContextMenuClick(object sender, EventArgs e)
        {
            _diTabIndices["Actors"] = 0;
            LoadActorDataGrid(((ToolStripMenuItem)sender).Text, 0);
            LoadActorInfo();
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
        private void AddNewGenericXMLObject(XMLCollection collection, string chosenType)
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
            ItemEnum e = Util.ParseEnum<ItemEnum>(chosenType);
            switch (e)
            {
                case ItemEnum.Blueprint:
                    defaultTags = DEFAULT_ITEM_TAGS + ",ObjectID:";
                    break;
                case ItemEnum.Food:
                    defaultTags = DEFAULT_ITEM_TAGS + ",FoodType:,FoodValue:,Stam:";
                    break;
                case ItemEnum.NPCToken:
                    defaultTags = DEFAULT_ITEM_TAGS + ",NPC_ID:";
                    break;
                case ItemEnum.Resource:
                    defaultTags = DEFAULT_ITEM_TAGS;
                    break;
                case ItemEnum.Seed:
                    defaultTags = DEFAULT_ITEM_TAGS + ",ObjectID:,Season:";
                    break;
                case ItemEnum.Tool:
                    defaultTags = DEFAULT_ITEM_TAGS + ",Level:,Stam:";
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
                case ObjectTypeEnum.Machine:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",Daily,Makes:";
                    break;
                case ObjectTypeEnum.Plant:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS + ",TrNum:,TrTime:,Season:,SeedID:,ItemID:";
                    break;
                default:
                    defaultTags = DEFAULT_WORLD_OBJECT_TAGS;
                    break;
            }
        }

        private void AddNewItem(object sender, EventArgs e)
        {
            SaveItemInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Item], sender.ToString());
        }
        private void AddNewWorldObject(object sender, EventArgs e)
        {
            SaveWorldObjectInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.WorldObject], sender.ToString());
        }
        private void AddNewTask(object sender, EventArgs e)
        {
            SaveTaskInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Task], sender.ToString());
        }
        private void AddNewCutscene(object sender, EventArgs e)
        {
            SaveCutsceneInfo();
            tbCutsceneTriggers.Clear();
            tbCutsceneDetails.Clear();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Cutscene], sender.ToString());
        }
        private void AddNewActor(object sender, EventArgs e)
        {
            SaveActorInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Actor], sender.ToString());
        }
        private void AddNewLight(object sender, EventArgs e)
        {
            SaveLightInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Light], sender.ToString());
        }
        private void AddNewUpgrade(object sender, EventArgs e)
        {
            SaveUpgradeInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Upgrade], sender.ToString());
        }
        private void AddNewDungeon(object sender, EventArgs e)
        {
            SaveDungeonInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Dungeon], "");
        }
        private void AddNewShop(object sender, EventArgs e)
        {
            SaveShopInfo();
            AddNewGenericXMLObject(_diTabCollections[XMLTypeEnum.Shop], "");
        }
        #endregion

        #region Load Info Panes
        private void LoadAllInfoPanels()
        {
            LoadItemInfo();
            LoadWorldObjectInfo();
            LoadActorInfo();
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
            ComboBox cb = FindComboBoxByName(collection.XMLType, ComponentTypeEnum.ComboBoxType);
            ComboBox cbSubtype = FindComboBoxByName(collection.XMLType, ComponentTypeEnum.ComboBoxSubtype);

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
                if (!s.StartsWith("Type") && !s.StartsWith("Subtype"))
                {
                    dgvTags.Rows.Add(s);
                }
            }
            if (cb != null && data.HasTag("Type"))
            {
                cb.SelectedItem = "Type:" + data.GetTagValue("Type");
            }
            if (cbSubtype != null && data.HasTag("Subtype"))
            {
                cbSubtype.SelectedItem = "Subtype:" + data.GetTagValue("Subtype");
            }
        }
        private void LoadItemInfo()
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvItems.SelectedRows[0];
                XMLData data = null;
                if (_typeFilter == "All") { data = _diBasicXML[ITEM_DATA_XML_FILE][r.Index]; }
                else if (_subtypeFilter != "All")
                {
                    data = _diBasicXML[ITEM_DATA_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter) && x.GetTagValue("Subtype").Equals(_subtypeFilter))[r.Index];
                }
                else { data = _diBasicXML[ITEM_DATA_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter))[r.Index]; }

                LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Item]);
            }
        }
        private void LoadWorldObjectInfo()
        {
            if (dgvWorldObjects.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvWorldObjects.SelectedRows[0];
                XMLData data = null;
                if (_typeFilter == "All") { data = _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE][r.Index]; }
                else if (_subtypeFilter != "All")
                {
                    data = _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter) && x.GetTagValue("Subtype").Equals(_subtypeFilter))[r.Index];
                }
                else { data = _diBasicXML[WORLD_OBJECTS_DATA_XML_FILE].FindAll(x => x.GetTagValue("Type").Equals(_typeFilter))[r.Index]; }

                LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.WorldObject]);
            }
        }
        private void LoadActorInfo()
        {
            if (dgvActors.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvActors.SelectedRows[0];
                XMLData data = null;
                if (_typeFilter == "All") { data = _diBasicXML[ACTOR_XML_FILE][_diTabIndices["Actors"]]; }
                else { data = _diBasicXML[ACTOR_XML_FILE].FindAll(x => x.GetTagValue("Type").ToString().Equals(_typeFilter))[r.Index]; }
                LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Actor]);
            }
        }
        private void LoadTaskInfo()
        {
            XMLData data = _diBasicXML[TASK_XML_FILE][_diTabIndices["Tasks"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Task]);
        }
        private void LoadStatusEffectInfo()
        {
            XMLData data = _diBasicXML[STATUS_EFFECTS_XML_FILE][_diTabIndices["StatusEffects"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.StatusEffect]);
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
        private void LoadShopInfo()
        {
            XMLData data = _diBasicXML[SHOPS_XML_FILE][_diTabIndices["Shops"]];
            LoadGenericDataInfo(data, _diTabCollections[XMLTypeEnum.Shop]);
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
            ComboBox cbSubtype = FindComboBoxByName(collection.XMLType, ComponentTypeEnum.ComboBoxSubtype);

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
            SaveXMLDataInfo(_diBasicXML[ITEM_DATA_XML_FILE], _diTabCollections[XMLTypeEnum.Item]);
        }
        private void SaveWorldObjectInfo()
        {
            SaveXMLDataInfo(_diBasicXML[WORLD_OBJECTS_DATA_XML_FILE], _diTabCollections[XMLTypeEnum.WorldObject]);
        }
        private void SaveActorInfo()
        {
            SaveXMLDataInfo(_diBasicXML[ACTOR_XML_FILE], _diTabCollections[XMLTypeEnum.Actor]);
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
                else if (sender == dgvActors) { GenericCellClick(e, "Actors", LoadActorInfo, SaveActorInfo); }
                else if (sender == dgvShops) { GenericCellClick(e, "Shops", LoadShopInfo, SaveShopInfo); }
                else if (sender == dgvStatusEffects) { GenericCellClick(e, "StatusEffects", LoadStatusEffectInfo, SaveStatusEffectInfo); }
                else if (sender == dgvTasks) { GenericCellClick(e, "Tasks", LoadTaskInfo, SaveTaskInfo); }
                else if (sender == dgvUpgrades) { GenericCellClick(e, "Upgrades", LoadUpgradeInfo, SaveUpgradeInfo); }
                else if (sender == dgvWorldObjects) { GenericCellClick(e, "WorldObjects", LoadWorldObjectInfo, SaveWorldObjectInfo); }
                else if (sender == dgvItems) { GenericCellClick(e, "Items", LoadItemInfo, SaveItemInfo); }
                else if (sender == dgvCutscenes)
                {
                    SaveCutsceneInfo();
                    _diTabIndices["Cutscenes"] = e.RowIndex;
                    LoadCutsceneInfo();
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
            else if (sender == btnActorCancel) { GenericCancel(_diBasicXML[ACTOR_XML_FILE], "Actors", dgvActors, LoadActorInfo); }
            else if (sender == btnShopCancel) { GenericCancel(_diBasicXML[SHOPS_XML_FILE], "Shops", dgvShops, LoadShopInfo); }
            else if (sender == btnStatusEffectCancel) { GenericCancel(_diBasicXML[STATUS_EFFECTS_XML_FILE], "StatusEffects", dgvStatusEffects, LoadStatusEffectInfo); }
            else if (sender == btnTaskCancel) { GenericCancel(_diBasicXML[TASK_XML_FILE], "Tasks", dgvTasks, LoadTaskInfo); }
            else if (sender == btnUpgradeCancel) { GenericCancel(_diBasicXML[UPGRADES_XML_FILE], "Upgrades", dgvUpgrades, LoadUpgradeInfo); }
            else if (sender == btnWorldObjectCancel) { GenericCancel(_diBasicXML[WORLD_OBJECTS_DATA_XML_FILE], "WorldObjects", dgvWorldObjects, LoadWorldObjectInfo); }
            else if (sender == btnItemCancel) { GenericCancel(_diBasicXML[ITEM_DATA_XML_FILE], "Items", dgvItems, LoadItemInfo); }
            else if (sender == btnCutsceneCancel)
            {
                if (_diCutscenes.Count == _diTabIndices["Cutscenes"])
                {
                    dgvCutscenes.Rows.RemoveAt(_diTabIndices["Cutscenes"]--);
                    SelectRow(dgvCutscenes, _diTabIndices["Cutscenes"]);
                }
                LoadCutsceneInfo();
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

        private void FrmDBEditor_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(832, 0);
            this.MaximumSize = new Size(832, Screen.PrimaryScreen.Bounds.Height);

        }
    }
}
