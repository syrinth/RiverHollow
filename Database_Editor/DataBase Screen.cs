using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Items.Item;

namespace Database_Editor
{
    public partial class frmDBEditor : Form
    {
        private enum EditableCharacterDataEnum { Dialogue, Schedule };
        public enum XMLTypeEnum { None, Task, Character, Class, Adventurer, Building, WorldObject, Item, Monster, Action, Shop, Spirit, Summon, StatusEffect, Cutscene };
        #region XML Files
        string SHOPS_XML_FILE = PATH_TO_DATA + @"\Shops.xml";
        string ACTIONS_XML_FILE = PATH_TO_DATA + @"\CombatActions.xml";
        string CUTSCENE_XML_FILE = PATH_TO_DATA + @"\CutScenes.xml";
        string CUTSCENE_DIALOGUE_XML_FILE = PATH_TO_DIALOGUE + @"\CutsceneDialogue.xml";
        string QUEST_XML_FILE = PATH_TO_DATA + @"\Quests.xml";
        string MONSTERS_XML_FILE = PATH_TO_DATA + @"\Monsters.xml";
        string CHARACTER_XML_FILE = PATH_TO_DATA + @"\CharacterData.xml";
        string CLASSES_XML_FILE = PATH_TO_DATA + @"\Classes.xml";
        string WORKERS_XML_FILE = PATH_TO_DATA + @"\Workers.xml";
        string CONFIG_XML_FILE = PATH_TO_DATA + @"\Config.xml";
        string MAGIC_SHOP_XML_FILE = PATH_TO_DATA + @"\Shops\MagicShop.xml";
        string SPIRITS_XML_FILE = PATH_TO_DATA + @"\Spirits.xml";
        string SUMMONS_XML_FILE = PATH_TO_DATA + @"\Summons.xml";
        string STATUS_EFFECTS_XML_FILE = PATH_TO_DATA + @"\StatusEffects.xml";
        string BUILDINGS_XML_FILE = PATH_TO_DATA + @"\Buildings.xml";
        string ITEM_DATA_XML_FILE = PATH_TO_DATA + @"\ItemData.xml";
        string OBJECT_TEXT_XML_FILE = PATH_TO_TEXT_FILES + @"\Object_Text.xml";
        string WORLD_OBJECTS_DATA_XML_FILE = PATH_TO_DATA + @"\WorldObjects.xml";
        #endregion

        #region Tags
        const string TAGS_FOR_ITEMS = "ItemKeyID,ReqItems,RefinesInto,ItemID,GoalItem,ItemReward,Collection,Makes,Processes,DWeap,DArmor,DHead,DWrist";
        const string TAGS_FOR_WORLD_OBJECTS = "ObjectID,Wall,Floor,Resources,Place";
        const string TAGS_FOR_COMBAT_ACTIONS = "Ability,Spell";
        const string TAGS_FOR_ADVENTURERS = "WorkerID";
        const string TAGS_FOR_CLASSES = "Class";
        const string TAGS_FOR_SHOPDATA = "ShopData";
        const string TAGS_FOR_SPIRITS = "SpiritID";
        const string TAGS_FOR_STATUS_EFFECTS = "StatusEffectID";
        const string TAGS_FOR_SUMMONS = "SummonID";
        const string TAGS_FOR_BUILDINGS = "BuildingID,HouseID,RequiredBuildingID";

        const string ITEM_REF_TAGS = "ReqItems,RefinesInto,Place";
        const string TASK_REF_TAGS = "GoalItem,ItemReward,BuildingID,BuildingRewardID";
        const string CHARACTER_REF_TAGS = "Collection,Class,ShopData,HouseID,RequiredBuildingID";
        const string WORLD_OBJECT_REF_TAGS = "Makes,Processes,ItemID";
        const string CLASSES_REF_TAGS = "DWeap,DArmor,DHead,DWrist,Ability,Spell";
        const string SHOPDATA_REF_TAGS = "ItemID,WorkerID,BuildingID";
        const string ADVENTURERS_REF_TAG = "ItemID";
        const string SHOP_REF_TAG = "ItemID,Requires,BuildingID";
        const string CONFIG_REF_TAG = "ItemID,ObjectID";
        const string MONSTERS_REF_TAGS = "Loot,Ability,Spell";
        const string ACTIONS_REF_TAGS = "StatusEffectID,SummonID";
        const string BUILDINGS_REF_TAGS = "ReqItems";

        const string MAP_REF_TAGS = "ItemKeyID,ItemID,Resources,ObjectID,SpiritID";
        #endregion

        List<ItemXMLData> _liItemData;
        List<XMLData> _liWorldObjects;

        Dictionary<string, int> _diTabIndices;
        private int _iNextCurrID = -1;
        public static string SPECIAL_CHARACTER = "^";
        static string PATH_TO_CONTENT = string.Format(@"{0}\..\..\..\..\RiverHollow\RiverHollowGame\Content", System.Environment.CurrentDirectory);
        static string PATH_TO_MAPS = PATH_TO_CONTENT + @"\Maps";
        static string PATH_TO_DATA = PATH_TO_CONTENT + @"\Data";
        static string PATH_TO_TEXT_FILES = PATH_TO_DATA + @"\Text Files";
        static string PATH_TO_DIALOGUE = PATH_TO_TEXT_FILES + @"\Dialogue";
        static string PATH_TO_SCHEDULES = PATH_TO_DATA + @"\Schedules";

        static Dictionary<string, List<string>> _diCutsceneDialogue;
        static Dictionary<int, List<string>> _diCutscenes;
        static Dictionary<int, List<XMLData>> _diShops;
        static Dictionary<string, Dictionary<string, List<string>>> _diCharacterSchedules;
        static Dictionary<string, Dictionary<string, string>> _diCharacterDialogue;
        static Dictionary<string, Dictionary<string, string>> _diObjectText;
        static Dictionary<ItemEnum, List<ItemXMLData>> _diItems;
        static Dictionary<string, List<XMLData>> _diBasicXML;
        Dictionary<string, TMXData> _diMapData;

        delegate void VoidDelegate();
        delegate void XMLListDataDelegate(List<XMLData> Data);

        public frmDBEditor()
        {
            InitializeComponent();

            InitComboBox<ItemEnum>(cbItemType);
            InitComboBox<ObjectTypeEnum>(cbWorldObjectType);
            InitComboBox<NPCTypeEnum>(cbCharacterType);
            InitComboBox<AdventurerTypeEnum>(cbAdventurerType);
            InitComboBox<TaskTypeEnum>(cbQuestType);
            InitComboBox<EditableCharacterDataEnum>(cbEditableCharData, false);
            InitComboBox<ActionEnum>(cbActionType);

            _diTabIndices = new Dictionary<string, int>()
            {
                { "PreviousTab", 0 },
                { "Items", 0 },
                { "WorldObjects", 0 },
                { "Characters", 0 },
                { "Classes", 0 },
                { "Adventurers", 0 },
                { "Quests", 0 },
                { "Cutscenes", 0},
                { "Monsters", 0},
                { "Actions", 0 },
                { "Shops", 0 },
                { "Buildings", 0 },
                { "Spirits", 0 },
                { "Summons", 0 },
                { "StatusEffects", 0 }
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

            _diBasicXML = new Dictionary<string, List<XMLData>>();
            _diItems = new Dictionary<ItemEnum, List<ItemXMLData>>();
            _diCharacterDialogue = new Dictionary<string, Dictionary<string, string>>();
            foreach (string s in Directory.GetFiles(PATH_TO_DIALOGUE))
            {
                string fileName = string.Empty;

                if (s.Contains("NPC_")) { fileName = Path.GetFileName(s).Replace("NPC_", "").Split('.')[0]; }
                else if (s.Contains("Adventurer_")) { fileName = Path.GetFileName(s).Replace("Adventurer_", "").Split('.')[0]; }
                else { continue; }

                int charID = -1;
                if (int.TryParse(fileName, out charID))
                {
                    fileName = s;
                    Util.ParseContentFile(ref fileName);
                    _diCharacterDialogue.Add(s, ReadXMLFilToDictionary(fileName));
                }
            }

            _diCharacterSchedules = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (string s in Directory.GetFiles(PATH_TO_SCHEDULES))
            {
                string fileName = Path.GetFileName(s).Replace("NPC_", "").Split('.')[0];
                int charID = -1;
                if (int.TryParse(fileName, out charID))
                {
                    fileName = s;
                    Util.ParseContentFile(ref fileName);
                    _diCharacterSchedules.Add(s, ReadXMLFileToStringKeyDictionaryStringList(fileName));//ReadXMLFilToDictionary(fileName));
                }
            }

            _diObjectText = ReadTaggedXMLFile(OBJECT_TEXT_XML_FILE);

            LoadXMLDictionary(QUEST_XML_FILE, TASK_REF_TAGS, "");
            LoadXMLDictionary(CHARACTER_XML_FILE, CHARACTER_REF_TAGS, "");
            LoadXMLDictionary(CLASSES_XML_FILE, CLASSES_REF_TAGS, TAGS_FOR_CLASSES);
            LoadXMLDictionary(WORKERS_XML_FILE, ADVENTURERS_REF_TAG, TAGS_FOR_ADVENTURERS);
            LoadXMLDictionary(CONFIG_XML_FILE, CONFIG_REF_TAG, "");
            LoadXMLDictionary(MONSTERS_XML_FILE, MONSTERS_REF_TAGS, "");
            LoadXMLDictionary(ACTIONS_XML_FILE, ACTIONS_REF_TAGS, TAGS_FOR_COMBAT_ACTIONS);
            LoadXMLDictionary(BUILDINGS_XML_FILE, BUILDINGS_REF_TAGS, TAGS_FOR_BUILDINGS);
            LoadXMLDictionary(SPIRITS_XML_FILE, "", TAGS_FOR_SPIRITS);
            LoadXMLDictionary(SUMMONS_XML_FILE, "", TAGS_FOR_SUMMONS);
            LoadXMLDictionary(STATUS_EFFECTS_XML_FILE, "", TAGS_FOR_STATUS_EFFECTS);

            _diShops = ReadXMLFileToXMLDataListDictionary(SHOPS_XML_FILE, XMLTypeEnum.Shop, SHOPDATA_REF_TAGS, TAGS_FOR_SHOPDATA);
            _diCutscenes = ReadXMLFileToIntKeyDictionaryStringList(CUTSCENE_XML_FILE);
            _diCutsceneDialogue = ReadXMLFileToStringKeyDictionaryStringList(CUTSCENE_DIALOGUE_XML_FILE);

            LoadWorldObjects();
            LoadItemData();

            FindLinkedXMLObjects();

            LoadDataGrids();
            LoadAllInfoPanels();

        }

        private void LoadDataGrids()
        {
            LoadItemDataGrid();
            LoadWorldObjectDataGrid();
            LoadCharacterDataGrid();
            LoadClassDataGrid();
            LoadAdventurerDataGrid();
            LoadQuestDataGrid();
            LoadMonsterDataGrid();
            LoadActionDataGrid();
            LoadBuildingDataGrid();
            LoadSpiritDataGrid();
            LoadSummonDataGrid();
            LoadStatusEffectDataGrid();
            LoadCutsceneDataGrid();
            LoadShopsDataGrid();
        }

        private void LoadAllInfoPanels()
        {
            LoadItemInfo();
            LoadWorldObjectInfo();
            LoadCharacterInfo();
            LoadClassInfo();
            LoadAdventurerInfo();
            LoadQuestInfo();
            LoadCutsceneInfo();
            LoadMonsterInfo();
            LoadActionInfo();
            LoadShopInfo();
            LoadBuildingInfo();
            LoadSpiritInfo();
            LoadSummonInfo();
            LoadStatusEffectInfo();
        }

        #region Helpers
        private void InitComboBox<T>(ComboBox cb, bool type = true)
        {
            cb.Items.Clear();
            foreach (T e in Enum.GetValues(typeof(T)))
            {
                cb.Items.Add((type ? "Type:" : "") + e.ToString());
            }
            cb.SelectedIndex = 0;
        }
        private XMLTypeEnum FileNameToXMLType(string fileName)
        {
            XMLTypeEnum rv = XMLTypeEnum.None;

            if (fileName == QUEST_XML_FILE) { rv = XMLTypeEnum.Task; }
            else if (fileName == CHARACTER_XML_FILE) { rv = XMLTypeEnum.Character; }
            else if (fileName == CLASSES_XML_FILE) { rv = XMLTypeEnum.Class; }
            else if (fileName == WORKERS_XML_FILE) { rv = XMLTypeEnum.Adventurer; }
            else if (fileName == WORLD_OBJECTS_DATA_XML_FILE) { rv = XMLTypeEnum.WorldObject; }
            else if (fileName == MONSTERS_XML_FILE) { rv = XMLTypeEnum.Monster; }
            else if (fileName == ACTIONS_XML_FILE) { rv = XMLTypeEnum.Action; }
            else if (fileName == SPIRITS_XML_FILE) { rv = XMLTypeEnum.Spirit; }
            else if (fileName == BUILDINGS_XML_FILE) { rv = XMLTypeEnum.Building; }
            else if (fileName == SUMMONS_XML_FILE) { rv = XMLTypeEnum.Summon; }
            else if (fileName == STATUS_EFFECTS_XML_FILE) { rv = XMLTypeEnum.StatusEffect; }

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
        private void LoadGenericDatagrid(DataGridView dgv, List<XMLData> data, string colID, string colName, string tabIndex, int selectRow, string filter = "All")
        {
            dgv.Rows.Clear();
            int index = 0;
            for (int i = 0; i < data.Count; i++)
            {
                if (filter == "All" || data[i].GetTagValue("Type").ToString().Equals(filter))
                {
                    dgv.Rows.Add();
                    DataGridViewRow row = dgv.Rows[index++];

                    row.Cells[colID].Value = data[i].ID;
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
            for (int i = 0; i < _liItemData.Count; i++)
            {
                if (filter == "All" || _liItemData[i].ItemType.ToString().Equals(filter))
                {
                    dgvItems.Rows.Add();
                    DataGridViewRow row = dgvItems.Rows[index++];

                    row.Cells["colItemID"].Value = _liItemData[i].ID;
                    row.Cells["colItemName"].Value = _liItemData[i].Name;
                }
            }

            SelectRow(dgvItems, selectedIndex == -1 ? _diTabIndices["Items"] : selectedIndex);
            dgvItems.Focus();
        }
        private void LoadWorldObjectDataGrid(string filter = "All", int selectedIndex = -1)
        {
            LoadGenericDatagrid(dgvWorldObjects, _liWorldObjects, "colWorldObjectsID", "colWorldObjectsName", "WorldObjects", selectedIndex == -1 ? _diTabIndices["WorldObjects"] : selectedIndex, filter);
        }
        private void LoadCharacterDataGrid()
        {
            LoadGenericDatagrid(dgvCharacters, _diBasicXML[CHARACTER_XML_FILE], "colCharacterID", "colCharacterName", "Characters", _diTabIndices["Characters"]);
        }
        private void LoadClassDataGrid()
        {
            LoadGenericDatagrid(dgvClasses, _diBasicXML[CLASSES_XML_FILE], "colClassID", "colClassName", "Classes", _diTabIndices["Classes"]);
        }
        private void LoadAdventurerDataGrid()
        {
            LoadGenericDatagrid(dgvAdventurers, _diBasicXML[WORKERS_XML_FILE], "colAdventurersID", "colAdventurersName", "Adventurers", _diTabIndices["Adventurers"]);
        }
        private void LoadQuestDataGrid()
        {
            LoadGenericDatagrid(dgvQuests, _diBasicXML[QUEST_XML_FILE], "colQuestsID", "colQuestsName", "Quests", _diTabIndices["Quests"]);
        }
        private void LoadMonsterDataGrid()
        {
            LoadGenericDatagrid(dgvMonsters, _diBasicXML[MONSTERS_XML_FILE], "colMonstersID", "colMonstersName", "Monsters", _diTabIndices["Monsters"]);
        }
        private void LoadActionDataGrid()
        {
            LoadGenericDatagrid(dgvActions, _diBasicXML[ACTIONS_XML_FILE], "colActionsID", "colActionsName", "Actions", _diTabIndices["Actions"]);
        }
        private void LoadBuildingDataGrid()
        {
            LoadGenericDatagrid(dgvBuildings, _diBasicXML[BUILDINGS_XML_FILE], "colBuildingsID", "colBuildingsName", "Buildings", _diTabIndices["Buildings"]);
        }
        private void LoadSpiritDataGrid()
        {
            LoadGenericDatagrid(dgvSpirits, _diBasicXML[SPIRITS_XML_FILE], "colSpiritsID", "colSpiritsName", "Spirits", _diTabIndices["Spirits"]);
        }
        private void LoadSummonDataGrid()
        {
            LoadGenericDatagrid(dgvSummons, _diBasicXML[SUMMONS_XML_FILE], "colSummonsID", "colSummonsName", "Summons", _diTabIndices["Summons"]);
        }
        private void LoadStatusEffectDataGrid()
        {
            LoadGenericDatagrid(dgvStatusEffects, _diBasicXML[STATUS_EFFECTS_XML_FILE], "colStatusEffectsID", "colStatusEffectsName", "StatusEffects", _diTabIndices["StatusEffects"]);
        }

        private void LoadDictionaryListDatagrid(DataGridView dgv, Dictionary<int, List<XMLData>> di, string colID, string colName, string tabIndex, XMLTypeEnum xmlType)
        {
            dgv.Rows.Clear();
            foreach (KeyValuePair<int, List<XMLData>> kvp in di)
            {
                dgv.Rows.Add();

                DataGridViewRow row = dgv.Rows[kvp.Key];
                row.Cells[colID].Value = kvp.Key;
                row.Cells[colName].Value = GetTextValue(xmlType, kvp.Key, "Name");
            }

            SelectRow(dgv, _diTabIndices[tabIndex]);
            dgv.Focus();
        }
        private void LoadShopsDataGrid()
        {
            LoadDictionaryListDatagrid(dgvShops, _diShops, "colShopsID", "colShopsName", "Shops", XMLTypeEnum.Shop);
        }

        private void LoadCutsceneDataGrid()
        {
            dgvCutscenes.Rows.Clear();
            foreach (KeyValuePair<int, List<string>> kvp in _diCutscenes)
            {
                dgvCutscenes.Rows.Add();

                DataGridViewRow row = dgvCutscenes.Rows[kvp.Key];
                row.Cells["colCutscenesID"].Value = kvp.Key;
                row.Cells["colCutscenesName"].Value = GetTextValue(XMLTypeEnum.Cutscene, kvp.Key, "Name");
            }

            SelectRow(dgvCutscenes, _diTabIndices["Cutscenes"]);
            dgvCutscenes.Focus();
        }
        #endregion

        private Dictionary<string, string> ReadXMLFilToDictionary(string fileName)
        {
            string line = string.Empty;
            Dictionary<string, string> xmlDictionary = new Dictionary<string, string>();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            int i = 0;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("Item");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                xmlDictionary[xmlnode[i].ChildNodes.Item(0).InnerText] = xmlnode[i].ChildNodes.Item(1).InnerText.Trim();
            }

            return xmlDictionary;
        }
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
                                XMLData data = new XMLData(dataIndex.ToString(), n1.InnerText, refTags, tagsThatRefertoMe, identifier);
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
            string line = string.Empty;
            Dictionary<string, Dictionary<string, string>> xmlDictionary = new Dictionary<string, Dictionary<string, string>>();

            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            int i = 0;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("Item");
            for (i = 0; i <= xmlnode.Count - 1; i++)
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

        private void LoadXMLDictionary(string fileName, string tagsReferenced, string tagsThatReferenceMe)
        {
            List<XMLData> data = new List<XMLData>();
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in ReadTaggedXMLFile(fileName))
            {
                data.Add(new XMLData(kvp.Key, kvp.Value, tagsReferenced, tagsThatReferenceMe, FileNameToXMLType(fileName)));
            }

            _diBasicXML[fileName] = data;
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
                        _liWorldObjects.Add(new XMLData(strID, stringData, WORLD_OBJECT_REF_TAGS, TAGS_FOR_WORLD_OBJECTS, XMLTypeEnum.WorldObject));
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
                        _liItemData.Add(new ItemXMLData(strID, stringData, ITEM_REF_TAGS, TAGS_FOR_ITEMS));
                    }
                }
                else { break; }
            }
        }

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
                    _liItemData[j].CheckForItemLink(theData);
                }

                foreach (string s in _diBasicXML.Keys)
                {
                    foreach (XMLData testIt in _diBasicXML[s])
                    {
                        testIt.CheckForItemLink(theData);
                    }
                }

                //Compare ItemData against the WorldObjectData

                foreach (XMLData testIt in _liWorldObjects)
                {
                    //First, check to see if the object refers to the item this
                    //could be because the object makes the item for example
                    testIt.CheckForItemLink(theData);

                    //Next check to see if the item refers to the object, pass in
                    //false here to ensure that we compare only to the worldObject tags
                    //The item might place the object.
                    theData.CheckForItemLink(testIt);
                }

                //Find any maps that reference the ItemID
                foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                {
                    kvp.Value.ReferencesXMLObject(theData);
                }


                foreach (KeyValuePair<int, List<XMLData>> kvp in _diShops)
                {
                    foreach(XMLData testIt in kvp.Value)
                    {
                        testIt.CheckForItemLink(theData);
                    }
                }
            }

            //Compare maps against the worldObjects

            foreach (XMLData theData in _liWorldObjects)
            {
                //Find any maps that reference the ObjectID
                foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                {
                    kvp.Value.ReferencesXMLObject(theData);
                }

                //Find any files that reference the ObjectID
                foreach (string s in _diBasicXML.Keys)
                {
                    foreach (XMLData testIt in _diBasicXML[s])
                    {
                        testIt.CheckForItemLink(theData);
                    }
                }
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
                            foreach (XMLData testIt in _diBasicXML[comparatorFile])
                            {
                                testIt.CheckForItemLink(theData);
                            }
                        }
                    }

                    //Find any maps that reference the XMLObject
                    foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                    {
                        kvp.Value.ReferencesXMLObject(theData);
                    }

                    foreach (KeyValuePair<int, List<XMLData>> kvp in _diShops)
                    {
                        foreach (XMLData testIt in kvp.Value)
                        {
                            testIt.CheckForItemLink(theData);

                        }
                    }
                }
            }
        }

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
                case ItemEnum.Clothes:
                    cbItemSubtype.Visible = true;
                    foreach (ClothesEnum en in Enum.GetValues(typeof(ClothesEnum)))
                    {
                        cbItemSubtype.Items.Add("Subtype:" + en.ToString());
                    }
                    break;
                case ItemEnum.Equipment:
                    cbItemSubtype.Visible = true;
                    foreach (EquipmentEnum en in Enum.GetValues(typeof(EquipmentEnum)))
                    {
                        cbItemSubtype.Items.Add("Subtype:" + en.ToString());
                    }
                    break;
                case ItemEnum.Special:
                    cbItemSubtype.Visible = true;
                    foreach (SpecialItemEnum en in Enum.GetValues(typeof(SpecialItemEnum)))
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
                case ItemEnum.Clothes:
                    rv = (int)Util.ParseEnum<ClothesEnum>(value);
                    break;
                case ItemEnum.Equipment:
                    rv = (int)Util.ParseEnum<EquipmentEnum>(value);
                    break;
                case ItemEnum.Special:
                    rv = (int)Util.ParseEnum<SpecialItemEnum>(value);
                    break;
                case ItemEnum.Tool:
                    rv = (int)Util.ParseEnum<ToolEnum>(value);
                    break;
            }

            return rv;
        }

        #region Load Info Panes
        private void LoadGenericDataInfo(XMLData data, TextBox tbName, TextBox tbID, DataGridView dgvTags, TextBox tbDescription = null)
        {
            tbName.Text = data.Name;
            if (tbDescription != null)
            {
                tbDescription.Text = data.Description;
            }
            tbID.Text = data.ID.ToString();

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
            string cellValue = r.Cells["colItemID"].Value.ToString();

            ItemXMLData data = _liItemData[int.Parse(cellValue)];
            tbItemName.Text = data.Name;
            tbItemDesc.Text = data.Description;
            tbItemID.Text = data.ID.ToString();

            cbItemType.SelectedIndex = (int)data.ItemType;
            SetItemSubtype();

            dgItemTags.Rows.Clear();
            string[] tags = data.GetTagsString().Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (s.StartsWith("Subtype"))
                {
                    cbItemSubtype.SelectedIndex = GetSubtypeIndex(data.ItemType, s.Split(':')[1]);
                }
                else if (!s.StartsWith("Type"))
                {
                    dgItemTags.Rows.Add(s);
                }
            }
        }
        private void LoadWorldObjectInfo()
        {
            DataGridViewRow r = dgvWorldObjects.SelectedRows[0];
            string cellValue = r.Cells["colWorldObjectsID"].Value.ToString();
            XMLData data = _liWorldObjects[int.Parse(cellValue)];
            LoadGenericDataInfo(data, tbWorldObjectName, tbWorldObjectID, dgvWorldObjectTags);
            cbWorldObjectType.SelectedIndex = (int)Util.ParseEnum<ObjectTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadCharacterInfo()
        {
            XMLData data = _diBasicXML[CHARACTER_XML_FILE][_diTabIndices["Characters"]];
            LoadGenericDataInfo(data, tbCharacterName, tbCharacterID, dgCharacterTags);
            cbCharacterType.SelectedIndex = (int)Util.ParseEnum<NPCTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadClassInfo()
        {
            XMLData data = _diBasicXML[CLASSES_XML_FILE][_diTabIndices["Classes"]];
            LoadGenericDataInfo(data, tbClassName, tbClassID, dgClassTags, tbClassDescription);
        }
        private void LoadAdventurerInfo()
        {
            XMLData data = _diBasicXML[WORKERS_XML_FILE][_diTabIndices["Adventurers"]];
            LoadGenericDataInfo(data, tbAdventurerName, tbAdventurerID, dgvAdventurerTags);
            cbAdventurerType.SelectedIndex = (int)Util.ParseEnum<AdventurerTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadQuestInfo()
        {
            XMLData data = _diBasicXML[QUEST_XML_FILE][_diTabIndices["Quests"]];
            LoadGenericDataInfo(data, tbQuestName, tbQuestID, dgvQuestTags, tbQuestDescription);
            cbQuestType.SelectedIndex = (int)Util.ParseEnum<TaskTypeEnum>(data.GetTagValue("Type"));
        }
        private void LoadMonsterInfo()
        {
            XMLData data = _diBasicXML[MONSTERS_XML_FILE][_diTabIndices["Monsters"]];
            LoadGenericDataInfo(data, tbMonsterName, tbMonsterID, dgvMonsterTags, tbMonsterDescription);
        }
        private void LoadActionInfo()
        {
            XMLData data = _diBasicXML[ACTIONS_XML_FILE][_diTabIndices["Actions"]];
            LoadGenericDataInfo(data, tbActionName, tbActionID, dgvActionTags, tbActionDescription);
            cbActionType.SelectedIndex = (int)Util.ParseEnum<ActionEnum>(data.GetTagValue("Type"));
        }
        private void LoadBuildingInfo()
        {
            XMLData data = _diBasicXML[BUILDINGS_XML_FILE][_diTabIndices["Buildings"]];
            LoadGenericDataInfo(data, tbBuildingName, tbBuildingID, dgvBuildingTags, tbBuildingDescription);
        }
        private void LoadSpiritInfo()
        {
            XMLData data = _diBasicXML[SPIRITS_XML_FILE][_diTabIndices["Spirits"]];
            LoadGenericDataInfo(data, tbSpiritName, tbSpiritID, dgvSpiritTags, tbSpiritDescription);
        }
        private void LoadSummonInfo()
        {
            XMLData data = _diBasicXML[SUMMONS_XML_FILE][_diTabIndices["Summons"]];
            LoadGenericDataInfo(data, tbSummonName, tbSummonID, dgvSummonTags, tbSummonDescription);
        }
        private void LoadStatusEffectInfo()
        {
            XMLData data = _diBasicXML[STATUS_EFFECTS_XML_FILE][_diTabIndices["StatusEffects"]];
            LoadGenericDataInfo(data, tbStatusEffectName, tbStatusEffectID, dgvStatusEffectTags, tbStatusEffectDescription);
        }

        private void LoadDictionaryListInfo(int index, List<XMLData> dataList, TextBox tbName, DataGridView dgvTags, XMLTypeEnum xmlType, TextBox tbDescription = null)
        {
            tbName.Text = GetTextValue(xmlType, index, "Name");
            if (tbDescription != null)
            {
                tbDescription.Text = GetTextValue(xmlType, index, "Description");
            }

            dgvTags.Rows.Clear();
            foreach (XMLData d in dataList)
            {
                dgvTags.Rows.Add(d.GetTagsString());
            }
        }
        private void LoadCutsceneInfo()
        {
            List<string> listData = _diCutscenes[_diTabIndices["Cutscenes"]];

            tbCutsceneName.Text = GetTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name");
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
            tbShopName.Text = GetTextValue(XMLTypeEnum.Shop, _diTabIndices["Shops"], "Name");

            dgvShopTags.Rows.Clear();
            foreach (XMLData d in _diShops[_diTabIndices["Shops"]])
            {
                dgvShopTags.Rows.Add(d.GetTagsString());
            }
        }
        #endregion

        #region EventHandlers
        private void btnDialogue_Click(object sender, EventArgs e)
        {
            string npcKey = @"\NPC_" + _diBasicXML[CHARACTER_XML_FILE][_diTabIndices["Characters"]].ID.ToString("00") + ".xml";
            FormCharExtraData frm = null;
            if (cbEditableCharData.SelectedItem.ToString() == "Dialogue")
            {
                string key = PATH_TO_DIALOGUE + npcKey;
                if (!_diCharacterDialogue.ContainsKey(key))
                {
                    _diCharacterDialogue[key] = new Dictionary<string, string> { ["New"] = "" };
                }

                frm = new FormCharExtraData("Dialogue", _diCharacterDialogue[key]);
                frm.ShowDialog();

                _diCharacterDialogue[key] = frm.StringData;
            }
            else if (cbEditableCharData.SelectedItem.ToString() == "Schedule")
            {
                string key = PATH_TO_SCHEDULES + npcKey;
                if (!_diCharacterSchedules.ContainsKey(key))
                {
                    _diCharacterSchedules[key] = new Dictionary<string, List<string>> { ["New"] = new List<string>() };
                }

                frm = new FormCharExtraData("Schedule", _diCharacterSchedules[key]);
                frm.ShowDialog();

                _diCharacterSchedules[key] = frm.ListData;
            }
        }
        private void btnEditAdventurerDialogue_Click(object sender, EventArgs e)
        {
            string npcKey = @"\Adventurer_" + _diBasicXML[WORKERS_XML_FILE][_diTabIndices["Adventurers"]].ID.ToString("00") + ".xml";
            FormCharExtraData frm = null;
            string key = PATH_TO_DIALOGUE + npcKey;
            if (!_diCharacterDialogue.ContainsKey(key))
            {
                _diCharacterDialogue[key] = new Dictionary<string, string> { ["New"] = "" };
            }

            frm = new FormCharExtraData("Dialogue", _diCharacterDialogue[key]);
            frm.ShowDialog();

            _diCharacterDialogue[key] = frm.StringData;
        }
        private void btnEditCutsceneDialogue_Click(object sender, EventArgs e)
        {
            string keyValue = dgvCutscenes.CurrentRow.Cells["colCutscenesID"].Value.ToString();
            if (!_diCutsceneDialogue.ContainsKey(keyValue))
            {
                _diCutsceneDialogue[keyValue] = new List<string>() { "" };
            }
            Dictionary<string, string> tags = new Dictionary<string, string>();
            foreach (string s in _diCutsceneDialogue[keyValue])
            {
                string[] split = s.Split(new char[] { '[', ':', ']' }, StringSplitOptions.RemoveEmptyEntries);
                tags[split[0]] = split.Length > 1 ? split[1] : "";
            }

            FormCharExtraData frm = new FormCharExtraData("Cutscene Dialogue", tags);
            frm.ShowDialog();

            List<string> listTags = new List<string>();
            foreach (KeyValuePair<string, string> kvp in frm.StringData)
            {
                listTags.Add("[" + kvp.Key + ":" + kvp.Value + "]");
            }

            _diCutsceneDialogue[keyValue] = listTags;
        }

        #region SaveInfo
        private void SaveXMLDataInfo(List<XMLData> liData, string tabIndex, string textIDPrefix, XMLTypeEnum xmlType, TextBox tbName, TextBox tbID, ComboBox cb, DataGridView baseGridView, DataGridView dgTags, string colID, string colName, string tagsReferenced, string tagsThatReferenceMe, TextBox tbDescription = null)
        {
            XMLData data = null;
            if (liData.Count == _diTabIndices[tabIndex])
            {
                Dictionary<string, string> diText = new Dictionary<string, string>
                {
                    ["Name"] = tbName.Text,
                };
                if (tbDescription != null) { diText["Description"] = tbDescription.Text; }

                _diObjectText[textIDPrefix + "_" + tbID.Text] = diText;

                Dictionary<string, string> tags = new Dictionary<string, string>();

                if (cb != null)
                {
                    string[] typeTag = cb.SelectedItem.ToString().Split(':');
                    tags[typeTag[0]] = typeTag[1];
                }

                foreach (DataGridViewRow row in dgTags.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string key = tagInfo[0];
                        string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                        tags[key] = val;
                    }
                }

                data = new XMLData(tbID.Text, tags, tagsReferenced, tagsThatReferenceMe, xmlType);
                liData.Add(data);
            }
            else
            {
                data = liData[int.Parse(tbID.Text)];
                if (tbDescription == null) { data.SetTextData(tbName.Text); }
                else { data.SetTextData(tbName.Text, tbDescription.Text); }

                data.ClearTagInfo();
                if (cb != null)
                {
                    string[] typeTag = cb.SelectedItem.ToString().Split(':');
                    data.SetTagInfo(typeTag[0], typeTag[1]);
                }
                foreach (DataGridViewRow row in dgTags.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string key = tagInfo[0];
                        string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                        data.SetTagInfo(key, val);
                    }
                }
                data.ChangeID(int.Parse(tbID.Text), false);
            }

            DataGridViewRow updatedRow = baseGridView.Rows[_diTabIndices[tabIndex]];

            updatedRow.Cells[colID].Value = data.ID;
            updatedRow.Cells[colName].Value = data.Name;
        }
        private void SaveItemInfo()
        {
            ItemXMLData data = null;
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
                foreach (DataGridViewRow row in dgItemTags.Rows)
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

                data = new ItemXMLData(tbItemID.Text, tags, ITEM_REF_TAGS, TAGS_FOR_ITEMS);
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
                foreach (DataGridViewRow row in dgItemTags.Rows)
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

                updatedRow.Cells["colItemID"].Value = data.ID;
                updatedRow.Cells["colItemName"].Value = data.Name;
            }
        }
        private void SaveWorldObjectInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_liWorldObjects, "WorldObjects", "WorldObject", XMLTypeEnum.WorldObject, tbWorldObjectName, tbWorldObjectID, cbWorldObjectType, dgvWorldObjects, dgvWorldObjectTags, "colWorldObjectsID", "colWorldObjectsName", WORLD_OBJECT_REF_TAGS, TAGS_FOR_WORLD_OBJECTS);
        }
        private void SaveCharacterInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[CHARACTER_XML_FILE], "Characters", "Character_", XMLTypeEnum.Character, tbCharacterName, tbCharacterID, cbCharacterType, dgvCharacters, dgCharacterTags, "colCharacterID", "colCharacterName", CHARACTER_REF_TAGS, "");
        }
        private void SaveClassInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[CLASSES_XML_FILE], "Classes", "Class_", XMLTypeEnum.Class, tbClassName, tbClassID, null, dgvClasses, dgClassTags, "colClassID", "colClassName", CLASSES_REF_TAGS, "", tbClassDescription);
        }
        private void SaveAdventurerInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[WORKERS_XML_FILE], "Adventurers", "Adventurer_", XMLTypeEnum.Adventurer, tbAdventurerName, tbAdventurerID, cbAdventurerType, dgvAdventurers, dgvAdventurerTags, "colAdventurersID", "colAdventurersName", SHOPDATA_REF_TAGS, "");
        }
        private void SaveQuestInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[QUEST_XML_FILE], "Quests", "Quest_", XMLTypeEnum.Task, tbQuestName, tbQuestID, cbQuestType, dgvQuests, dgvQuestTags, "colQuestsID", "colQuestsName", TASK_REF_TAGS, "", tbQuestDescription);
        }
        private void SaveMonsterInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[MONSTERS_XML_FILE], "Monsters", "Monster_", XMLTypeEnum.Monster, tbMonsterName, tbMonsterID, null, dgvMonsters, dgvMonsterTags, "colMonstersID", "colMonstersName", MONSTERS_REF_TAGS, "", tbMonsterDescription);
        }
        private void SaveActionInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[ACTIONS_XML_FILE], "Actions", "Action_", XMLTypeEnum.Action, tbActionName, tbActionID, cbActionType, dgvActions, dgvActionTags, "colActionsID", "colActionsName", "", TAGS_FOR_COMBAT_ACTIONS, tbActionDescription);
        }
        private void SaveBuildingInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[BUILDINGS_XML_FILE], "Buildings", "Building_", XMLTypeEnum.Building, tbBuildingName, tbBuildingID, null, dgvBuildings, dgvBuildingTags, "colBuildingsID", "colBuildingsName", "", "", tbBuildingDescription);
        }
        private void SaveSpiritInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[SPIRITS_XML_FILE], "Spirits", "Spirit_", XMLTypeEnum.Spirit, tbSpiritName, tbSpiritID, null, dgvSpirits, dgvSpiritTags, "colSpiritsID", "colSpiritsName", "", "", tbSpiritDescription);
        }
        private void SaveSummonInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[SUMMONS_XML_FILE], "Summons", "Summon_", XMLTypeEnum.Summon, tbSummonName, tbSummonID, null, dgvSummons, dgvSummonTags, "colSummonsID", "colSummonsName", "", "", tbSummonDescription);
        }
        private void SaveStatusEffectInfo(List<XMLData> liData)
        {
            SaveXMLDataInfo(_diBasicXML[STATUS_EFFECTS_XML_FILE], "StatusEffects", "StatusEffect_", XMLTypeEnum.StatusEffect, tbStatusEffectName, tbStatusEffectID, null, dgvStatusEffects, dgvStatusEffectTags, "colStatusEffectsID", "colStatusEffectsName", "", "", tbStatusEffectDescription);
        }

        private void SaveCutsceneInfo()
        {
            List<string> listData = _diCutscenes[_diTabIndices["Cutscenes"]];
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

            updatedRow.Cells["colCutscenesID"].Value = _diTabIndices["Cutscenes"];
            updatedRow.Cells["colCutscenesName"].Value = GetTextValue(XMLTypeEnum.Cutscene, _diTabIndices["Cutscenes"], "Name");
        }
        private void SaveShopInfo()
        {
            List<XMLData> listData = _diShops[_diTabIndices["Shops"]];
            listData.Clear();

            string tags = string.Empty;
            foreach (DataGridViewRow r in dgvShopTags.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    listData.Add(new XMLData("-1", r.Cells[0].Value.ToString(), "", "", XMLTypeEnum.Shop));
                }
            }
            UpdateTextValue(XMLTypeEnum.Shop, _diTabIndices["Shops"], "Name", tbShopName.Text);

            DataGridViewRow updatedRow = dgvShops.Rows[_diTabIndices["Shops"]];

            updatedRow.Cells["colShopsID"].Value = _diTabIndices["Shops"];
            updatedRow.Cells["colShopsName"].Value = GetTextValue(XMLTypeEnum.Shop, _diTabIndices["Shops"], "Name");
        }
        #endregion

        private void GenericCancel(List<XMLData> liData, string tabIndex, DataGridView dgMain, VoidDelegate del)
        {
            if (liData.Count == _diTabIndices[tabIndex])
            {
                dgMain.Rows.RemoveAt(_diTabIndices[tabIndex]--);
                SelectRow(dgMain, _diTabIndices[tabIndex]);
            }
            del();
        }
        private void btnItemCancel_Click(object sender, EventArgs e)
        {
            if (_liItemData.Count == _diTabIndices["Items"])
            {
                dgvItems.Rows.RemoveAt(_diTabIndices["Items"]--);
                SelectRow(dgvItems, _diTabIndices["Items"]);
            }

            LoadItemInfo();
        }
        private void btnWorldObjectCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_liWorldObjects, "WorldObjects", dgvWorldObjects, LoadWorldObjectInfo);
        }
        private void btnCharacterCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[CHARACTER_XML_FILE], "Characters", dgvCharacters, LoadCharacterInfo);
        }
        private void btnClassCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[CLASSES_XML_FILE], "Classes", dgvClasses, LoadClassInfo);
        }
        private void btnAdventurerCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[WORKERS_XML_FILE], "Adventurers", dgvAdventurers, LoadAdventurerInfo);
        }
        private void btnQuestCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[QUEST_XML_FILE], "Quests", dgvQuests, LoadQuestInfo);
        }
        private void btnCutsceneCancel_Click(object sednder, EventArgs e)
        {
            if (_diCutscenes.Count == _diTabIndices["Cutscenes"])
            {
                dgvCutscenes.Rows.RemoveAt(_diTabIndices["Cutscenes"]--);
                SelectRow(dgvCutscenes, _diTabIndices["Cutscenes"]);
            }
            LoadCutsceneInfo();
        }
        private void btnMonsterCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[MONSTERS_XML_FILE], "Monsters", dgvMonsters, LoadMonsterInfo);
        }
        private void btnActionCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[ACTIONS_XML_FILE], "Actions", dgvActions, LoadActionInfo);
        }
        private void btnShopCancel_Click(object sender, EventArgs e)
        {
            if (_diShops.Count == _diTabIndices["Shops"])
            {
                dgvShops.Rows.RemoveAt(_diTabIndices["Shops"]--);
                SelectRow(dgvShops, _diTabIndices["Shops"]);
            }
            LoadShopInfo();
        }
        private void btnBuildingCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[BUILDINGS_XML_FILE], "Buildings", dgvBuildings, LoadBuildingInfo);
        }
        private void btnSpiritCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[SPIRITS_XML_FILE], "Spirits", dgvSpirits, LoadSpiritInfo);
        }
        private void btnSummonCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[SUMMONS_XML_FILE], "Summons", dgvSummons, LoadSummonInfo);
        }
        private void btnStatusEffectCancel_Click(object sender, EventArgs e)
        {
            GenericCancel(_diBasicXML[STATUS_EFFECTS_XML_FILE], "StatusEffects", dgvStatusEffects, LoadStatusEffectInfo);
        }

        private void GenericCellClick(DataGridViewCellEventArgs e, List<XMLData> liData, string tabIndex, DataGridView dgMain, VoidDelegate loadDel, XMLListDataDelegate saveDel)
        {
            if (e.RowIndex > -1)
            {
                saveDel(liData);
                _diTabIndices[tabIndex] = e.RowIndex;
                loadDel();
            }
        }
        private void dgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                SaveItemInfo();
                _diTabIndices["Items"] = e.RowIndex;
                LoadItemInfo();
            }
        }
        private void dgvWorldObjects_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _liWorldObjects, "WorldObjects", dgvWorldObjects, LoadWorldObjectInfo, SaveWorldObjectInfo);
        }
        private void dgvCharacters_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[CHARACTER_XML_FILE], "Characters", dgvCharacters, LoadCharacterInfo, SaveCharacterInfo);
        }
        private void dgvClasses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[CLASSES_XML_FILE], "Classes", dgvClasses, LoadClassInfo, SaveClassInfo);
        }
        private void dgvAdventurers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[WORKERS_XML_FILE], "Adventurers", dgvAdventurers, LoadAdventurerInfo, SaveAdventurerInfo);
        }
        private void dgvQuests_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[QUEST_XML_FILE], "Quests", dgvQuests, LoadQuestInfo, SaveQuestInfo);
        }
        private void dgvCutscenes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                SaveCutsceneInfo();
                _diTabIndices["Cutscenes"] = e.RowIndex;
                LoadCutsceneInfo();
            }
        }
        private void dgvMonsters_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[MONSTERS_XML_FILE], "Monsters", dgvMonsters, LoadMonsterInfo, SaveMonsterInfo);
        }
        private void dgvActions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[ACTIONS_XML_FILE], "Actions", dgvActions, LoadActionInfo, SaveActionInfo);
        }
        private void dgvShops_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                SaveShopInfo();
                _diTabIndices["Shops"] = e.RowIndex;
                LoadShopInfo();
            }
        }
        private void dgvBuildings_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[BUILDINGS_XML_FILE], "Buildings", dgvBuildings, LoadBuildingInfo, SaveBuildingInfo);
        }
        private void dgvSpirits_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[SPIRITS_XML_FILE], "Spirits", dgvSpirits, LoadSpiritInfo, SaveSpiritInfo);
        }
        private void dgvSummons_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[SUMMONS_XML_FILE], "Summons", dgvSummons, LoadSummonInfo, SaveSummonInfo);
        }
        private void dgvStatusEffects_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GenericCellClick(e, _diBasicXML[STATUS_EFFECTS_XML_FILE], "StatusEffects", dgvStatusEffects, LoadStatusEffectInfo, SaveStatusEffectInfo);
        }

        private void AddNewGenericXMLObject(TabPage page, string tabIndex, DataGridView dg, string colID, string colName, TextBox tbName, TextBox tbID, DataGridView dgTags, string tagCol, ComboBox cb = null, TextBox tbDesc = null, List<string> defaultTags = null)
        {
            tabCtl.SelectedTab = page;
            _diTabIndices[tabIndex] = dg.Rows.Count;
            dg.Rows.Add();
            SelectRow(dg, _diTabIndices[tabIndex]);

            DataGridViewRow row = dg.Rows[_diTabIndices[tabIndex]];
            row.Cells[colID].Value = _diTabIndices[tabIndex];
            row.Cells[colName].Value = "";

            tbName.Text = "";
            if (tbDesc != null) { tbDesc.Text = ""; }
            tbID.Text = _diTabIndices[tabIndex].ToString();

            if (cb != null) { cb.SelectedIndex = 0; }

            dgTags.Rows.Clear();

            if (defaultTags != null)
            {
                foreach (string s in defaultTags) { dgTags.Rows.Add(); }
                for (int i = 0; i < defaultTags.Count; i++)
                {
                    dgTags.Rows[i].Cells[tagCol].Value = defaultTags[i];
                }
            }

            tbName.Focus();
        }

        private void cbItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetItemSubtype();
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoSave();
            StreamWriter sWriter = PrepareXMLFile(OBJECT_TEXT_XML_FILE, "Dictionary[string, string]");

            _liItemData.Sort((x, y) =>
            {
                var typeComp = x.ItemType.CompareTo(y.ItemType);
                if (typeComp == 0) { return x.ID.CompareTo(y.ID); }
                else { return typeComp; }
            });
            _liWorldObjects.Sort((x, y) =>
            {
                var typeComp = x.GetTagValue("Type").CompareTo(y.GetTagValue("Type"));
                if (typeComp == 0) { return x.ID.CompareTo(y.ID); }
                else { return typeComp; }
            });
            List<ItemXMLData> itemDataList = new List<ItemXMLData>();
            List<XMLData> worldObjectDataList = new List<XMLData>();
            ChangeIDs(ref itemDataList, ref worldObjectDataList);

            //Strip the special case character from the Item files
            foreach (ItemXMLData data in _liItemData)
            {
                if (data != null) { data.StripSpecialCharacter(); }
            }

            //Strip the special case character from the WorldObject files
            foreach (XMLData data in _liWorldObjects)
            {
                data.StripSpecialCharacter();
            }

            foreach (string s in _diBasicXML.Keys)
            {
                foreach (XMLData data in _diBasicXML[s])
                {
                    data.StripSpecialCharacter();
                }
                SaveXMLData(_diBasicXML[s], s, sWriter);
            }

            foreach (KeyValuePair<int, List<XMLData>> kvp in _diShops)
            {
                foreach (XMLData data in kvp.Value)
                {
                    data.StripSpecialCharacter();
                }
            }
            SaveXMLDictionarXMLDataList(_diShops, SHOPS_XML_FILE, XMLTypeEnum.Shop, sWriter);

            foreach (string s in _diCharacterDialogue.Keys)
            {
                SaveXMLDictionary(_diCharacterDialogue[s], s, sWriter);
            }

            foreach (string s in _diCharacterSchedules.Keys)
            {
                SaveXMLDictionaryList(_diCharacterSchedules[s], s, sWriter, "string");
            }

            SaveXMLDictionaryIntKeyList(_diCutscenes, CUTSCENE_XML_FILE, XMLTypeEnum.Cutscene, sWriter);
            SaveXMLDictionaryList(_diCutsceneDialogue, CUTSCENE_DIALOGUE_XML_FILE, sWriter, "int");

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
        }
        private void tabCtl_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutoSave();

            if (tabCtl.SelectedTab == tabCtl.TabPages["tabWorldObjects"]) { dgvWorldObjects.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabItems"]) { dgvItems.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabCharacters"]) { dgvCharacters.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabClasses"]) { dgvClasses.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabAdventurers"]) { dgvAdventurers.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabQuests"]) { dgvQuests.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabCutscenes"]) { dgvCutscenes.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabMonsters"]) { dgvMonsters.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabActions"]) { dgvActions.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabShops"]) { dgvShops.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabBuildings"]) { dgvBuildings.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabSpirits"]) { dgvSpirits.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabSummons"]) { dgvSummons.Focus(); }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabStatusEffects"]) { dgvStatusEffects.Focus(); }

            _diTabIndices["PreviousTab"] = tabCtl.SelectedIndex;
        }

        private void AutoSave()
        {
            TabPage prevPage = tabCtl.TabPages[_diTabIndices["PreviousTab"]];
            if (prevPage == tabCtl.TabPages["tabWorldObjects"]) { SaveWorldObjectInfo(_liWorldObjects); }
            else if (prevPage == tabCtl.TabPages["tabItems"]) { SaveItemInfo(); }
            else if (prevPage == tabCtl.TabPages["tabCharacters"]) { SaveCharacterInfo(_diBasicXML[CHARACTER_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabClasses"]) { SaveClassInfo(_diBasicXML[CLASSES_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabAdventurers"]) { SaveAdventurerInfo(_diBasicXML[WORKERS_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabQuests"]) { SaveQuestInfo(_diBasicXML[QUEST_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabCutscenes"]) { SaveCutsceneInfo(); }
            else if (prevPage == tabCtl.TabPages["tabMonsters"]) { SaveMonsterInfo(_diBasicXML[MONSTERS_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabActions"]) { SaveActionInfo(_diBasicXML[ACTIONS_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabShops"]) { SaveShopInfo(); }
            else if (prevPage == tabCtl.TabPages["tabBuildings"]) { SaveBuildingInfo(_diBasicXML[BUILDINGS_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabSpirits"]) { SaveSpiritInfo(_diBasicXML[SPIRITS_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabSummons"]) { SaveSummonInfo(_diBasicXML[SUMMONS_XML_FILE]); }
            else if (prevPage == tabCtl.TabPages["tabStatusEffects"]) { SaveStatusEffectInfo(_diBasicXML[STATUS_EFFECTS_XML_FILE]); }
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

        public void SaveXMLDictionarXMLDataList(Dictionary<int, List<XMLData>> dataList, string fileName, XMLTypeEnum xmlType, StreamWriter sWriter)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[string, List[string]]");

            foreach (KeyValuePair<int, List<XMLData>> kvp in dataList)
            {
                string key = string.Format("      <Key>{0}</Key>", kvp.Key);
                string value = "      <Value>" + System.Environment.NewLine;
                string item = string.Empty;
                foreach (XMLData s in kvp.Value)
                {
                    value += string.Format("        <Item>{0}</Item>{1}", s.GetTagsString(), System.Environment.NewLine);
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

        public void SaveXMLDictionary(Dictionary<string, string> dataList, string fileName, StreamWriter sWriter)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[string, string]");

            foreach (KeyValuePair<string, string> kvp in dataList)
            {
                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", kvp.Key), string.Format("      <Value>{0}</Value>", kvp.Value));
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
                    id = Util.GetEnumString(type) + "_" + data.ID.ToString();
                }

                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>{0}</Value>", data.GetTagsString()));

                if (!fileName.Contains("Config"))
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

        #region Classes
        public class XMLData
        {
            protected struct LinkedItem
            {
                public TMXData MapData;
                public XMLData ItemData;
                public string LinkedTag;

                public LinkedItem(XMLData data, string tag)
                {
                    MapData = null;
                    ItemData = data;
                    LinkedTag = tag;
                }
                public LinkedItem(TMXData data, string tag)
                {
                    MapData = data;
                    ItemData = null;
                    LinkedTag = tag;
                }
            }
            protected string _sName;
            public string Name => _sName;
            protected string _sDescription;
            public string Description => _sDescription;
            protected XMLTypeEnum _eXMLType;
            protected int _iID;
            public int ID => _iID;
            protected List<string> _liTagsReferenced;
            protected List<string> _liTagsThatReferToMe;
            public List<string> TagsThatReferToMe => _liTagsThatReferToMe;
            protected List<LinkedItem> _liLinkedItems;
            protected List<LinkedItem> _liLinkedMaps;
            protected Dictionary<string, string> _diTags;

            public XMLData(string id, Dictionary<string, string> stringData, string tagsReferenced, string tagsThatReferToMe, XMLTypeEnum xmlType)
            {
                _liLinkedMaps = new List<LinkedItem>();
                _liLinkedItems = new List<LinkedItem>();
                _liTagsReferenced = new List<string>(tagsReferenced.Split(','));
                _liTagsThatReferToMe = new List<string>(tagsThatReferToMe.Split(','));

                string textID = Util.GetEnumString(xmlType) + "_" + id;
                if (xmlType != XMLTypeEnum.None)
                {
                    if (_diObjectText.ContainsKey(textID))
                    {
                        _sName = _diObjectText[textID]["Name"];

                        if (_diObjectText[textID].ContainsKey("Description"))
                        {
                            _sDescription = _diObjectText[textID]["Description"];
                        }
                    }
                }

                _iID = int.Parse(id);
                _diTags = stringData;

                _eXMLType = xmlType;
            }
            public XMLData(string id, string stringData, string tagsReferenced, string tagsThatReferToMe, XMLTypeEnum xmlType) : this(id, DataManager.TaggedStringToDictionary(stringData), tagsReferenced, tagsThatReferToMe, xmlType) { }

            public string GetStringValue(string value)
            {
                return _diTags[value];
            }

            public string GetTagsString()
            {
                string rv = string.Empty;

                foreach (KeyValuePair<string, string> kvp in _diTags)
                {
                    rv += "[" + kvp.Key + (string.IsNullOrEmpty(kvp.Value) ? "" : ":" + kvp.Value) + "]";
                }

                return rv;
            }

            public string GetTagValue(string key)
            {
                if (_diTags.ContainsKey(key)) { return _diTags[key]; }
                else { return string.Empty; }
            }
            public void SetTextData(string name)
            {
                _sName = name;
            }
            public void SetTextData(string name, string desc)
            {
                _sName = name;
                _sDescription = desc;
            }
            public void SetTagInfo(string key, string value)
            {
                _diTags[key] = value;
            }
            public void AppendToTag(string key, string value)
            {
                _diTags[key] += "|" + value;
            }
            public void ClearTagInfo()
            {
                _diTags.Clear();
            }

            /// <summary>
            /// Checks the tags to see if there are any references to the given ID
            /// among the relevant tags.
            /// </summary>
            /// <param name="id">The ID to look for</param>
            /// <returns>True if there is at least one match</returns>
            public void CheckForItemLink(XMLData testData)
            {
                foreach (string s in _liTagsReferenced)
                {
                    if (testData._liTagsThatReferToMe.Contains(s))
                    {
                        if(CheckTagForID(s, testData.ID))
                        {
                            testData.AddLinkedObject(this, s);
                        }
                    }
                }
            }

            /// <summary>
            /// Call this to check the given tag for the given ID. This method is to
            /// be used for any entry that has multiples of the same type of thing in it
            /// that are delineated by a '|'. For example, a tag for the multiple things a Machine
            /// can make
            /// </summary>
            /// <param name="tag">Tag to look at</param>
            /// <param name="id">The ID to look for</param>
            /// <param name="val">Reference to the success or this and other checks</param>
            /// <returns>True if a match exists</returns>
            private bool CheckTagForID(string tag, int id)
            {
                bool rv = false;

                //If we don't have the key, don't proceed
                if (_diTags.ContainsKey(tag))
                {
                    //Isolate every group of entries that aredelineated by the '|'
                    string[] split = Util.FindParams(_diTags[tag]);
                    foreach (string s in split)
                    {
                        //The first entry is always the item, split by the '-', find it and compare
                        string[] splitData = s.Split('-');
                        if (int.Parse(splitData[0]) == id)
                        {
                            rv = true;
                            break;
                        }
                    }
                }

                return rv;
            }

            /// <summary>
            /// Method to change the ID of the data from one value to another.
            /// 
            /// After changing, it's important to iterate over all the linked entries
            /// and tell them to replace the old ID with the new one.
            /// </summary>
            /// <param name="newID"></param>
            public virtual void ChangeID(int newID, bool item = true)
            {
                if (_iID != newID)
                {
                    int oldID = _iID;
                    _iID = newID;

                    foreach (LinkedItem d in _liLinkedItems)
                    {
                        XMLData data = d.ItemData;
                        data.ReplaceLinkedIDs(oldID, _iID, d.LinkedTag);
                    }

                    foreach (LinkedItem d in _liLinkedMaps)
                    {
                        TMXData data = d.MapData;
                        data.ReplaceID(oldID, newID, d.LinkedTag);
                    }
                }
            }

            /// <summary>
            /// Iterates through the relevant tags to replace any instances of the 
            /// old ID with the new ID.
            /// </summary>
            /// <param name="oldID">The old ID that has now changed</param>
            /// <param name="newID">The new ID to reference</param>
            /// <param name="tag">The specific tag to replace</param>
            public void ReplaceLinkedIDs(int oldID, int newID, string tag)
            {
                ReplaceID(tag, oldID, newID);
            }

            /// <summary>
            /// Call this to check the given tag for the given ID.
            /// 
            /// Replace any instances of the old ID that are found with the new ID
            /// /// </summary>
            /// <param name="tag">Tag to look at</param>
            /// <param name="oldID">The ID to look for</param>
            /// <param name="newID">The ID to replace the olf one with</param>
            public void ReplaceID(string tag, int oldID, int newID)
            {
                //If we don't have the key, don't proceed
                if (_diTags.ContainsKey(tag))
                {
                    //Isolate every group of entries that are delineated by the '|'
                    string[] split = Util.FindParams(_diTags[tag]);
                    _diTags[tag] = string.Empty;
                    for (int i = 0; i < split.Length; i++)
                    {
                        //The first entry is always the item, split by the '-', find it and compare
                        //If the value matches, replace the split string id with the newID surrounded by
                        //the special character. The special character prevents subsequent changes from
                        //overwriting this change.
                        string[] splitData = split[i].Split('-');
                        if (splitData[0] == oldID.ToString())
                        {
                            splitData[0] = SPECIAL_CHARACTER + newID.ToString() + SPECIAL_CHARACTER;
                        }

                        //Iterate over any linked values and concatenate them to re-add them to the entry
                        for (int j = 0; j < splitData.Length; j++)
                        {
                            _diTags[tag] += splitData[j];

                            //If there is a linked value, add a '-' and continue
                            if (j < splitData.Length - 1)
                            {
                                _diTags[tag] += "-";
                            }
                        }

                        //There may or may not be any additional values, if there are more coming, add the '|'
                        if (i < split.Length - 1)
                        {
                            _diTags[tag] += "|";
                        }
                    }
                }
            }

            /// <summary>
            /// Adds the given XMLData to the LinkedITems list.
            /// 
            /// Do nto do this if the list already contains it or if the
            /// XMLData is this entry.
            /// </summary>
            /// <param name="d">The linked entry to add</param>
            public void AddLinkedObject(XMLData d, string tag)
            {
                if (this != d)
                {
                    _liLinkedItems.Add(new LinkedItem(d, tag));
                }
            }
            public void AddLinkedMap(TMXData d, string tag)
            {
                _liLinkedMaps.Add(new LinkedItem(d, tag));
            }

            /// <summary>
            /// Iterates through each tag and remove all instances of the special character
            /// </summary>
            public void StripSpecialCharacter()
            {
                foreach (string s in new List<string>(_diTags.Keys))
                {
                    if (_diTags[s].Contains(SPECIAL_CHARACTER))
                    {
                        string val = _diTags[s];
                        _diTags[s] = val.Replace(SPECIAL_CHARACTER, "");
                    }
                }
            }
        }
        public class ItemXMLData : XMLData
        {
            ItemEnum _eType;
            public ItemEnum ItemType => _eType;

            public ItemXMLData(string id, Dictionary<string, string> stringData, string tagsReferenced, string tagsThatReferenceMe) : base(id, stringData, tagsReferenced, tagsThatReferenceMe, XMLTypeEnum.Item)
            {
                _eType = Util.ParseEnum<ItemEnum>(_diTags["Type"]);
                string textID = "Item_" + id;
            }

            public void SetItemType(ItemEnum e)
            {
                _eType = e;
            }
        }
        public class TMXData
        {
            List<string> _liAllLines;
            public List<string> AllLines => _liAllLines;
            public TMXData(string fileName)
            {
                _liAllLines = new List<string>();

                string line;
                string fullPathToFile = string.Format(@"{0}\..\..\..\..\Content\Maps\{1}", System.Environment.CurrentDirectory, fileName);
                System.IO.StreamReader file = new System.IO.StreamReader(fileName);
                while ((line = file.ReadLine()) != null)
                {
                    _liAllLines.Add(line);
                }

                file.Close();
            }

            /// <summary>
            /// Call to determine if the TMX file refers to the referenced id in a given tag.
            /// Needs to be careful here because maps can refer to multiple things, so the tag
            /// input is very important to be coordinated with what ovbject type is being passed in
            /// </summary>
            /// <param name="id">The ID to search for</param>
            /// <param name="tags">The value tags to search for for this object type delimited by ','</param>
            /// <returns></returns>
            public void ReferencesXMLObject(XMLData data)
            {
                //Read through each line
                for (int i = 0; i < _liAllLines.Count; i++)
                {
                    string s = _liAllLines[i];
                    if (s.Contains("<property name"))
                    {
                        int indexOfOpenBrace = s.IndexOf("<");

                        string[] propertyParams = s.Substring(indexOfOpenBrace).Split(' ');    //Find all the entries of the property tag
                        string propertyName = string.Empty;
                        string propertyValue = string.Empty;

                        //Which index is the value entry
                        GetNameAndValue(ref propertyName, ref propertyValue, propertyParams);

                        //We're going to loop through every tag we've been told to search for
                        List<string> referencedTags = new List<string>(MAP_REF_TAGS.Split(','));
                        foreach (string refTag in referencedTags)
                        {
                            if (data.TagsThatReferToMe.Contains(refTag))
                            {
                                if (propertyName.Equals(refTag))
                                {
                                    //Split the values in the property value by the '|' delimeter 
                                    string[] splitValues = Util.FindParams(propertyValue);
                                    foreach (string spVal in splitValues)
                                    {
                                        string[] splitArgs = spVal.Split('-');
                                        //Do we have a match? return true
                                        if (splitArgs[0] == data.ID.ToString())
                                        {
                                            data.AddLinkedMap(this, refTag);
                                        }
                                    }
                                }
                            }
                        }   
                    }
                }
            }

            /// <summary>
            /// Call this to check the given tag for the given ID.
            /// 
            /// Replace any instances of the old ID that are found with the new ID
            /// /// </summary>
            /// <param name="tag">Tags to look at, delmitited by ','</param>
            /// <param name="oldID">The ID to look for</param>
            /// <param name="newID">The ID to replace the olf one with</param>
            public void ReplaceID(int oldID, int newID, string tag)
            {
                //Read through every  line of the file
                for (int i = 0; i < _liAllLines.Count; i++)
                {
                    string s = _liAllLines[i];

                    //If the line is a property line, we need to read it
                    if (s.Contains("<property name"))
                    {
                        int indexOfOpenBrace = s.IndexOf("<");
                        string buffer = s.Substring(0, indexOfOpenBrace);       //Save this to preserve however many spaces are at the beginning of the line
                        string newValue = "value=\"";                           //The start of a value tag

                        string[] propertyParams = s.Substring(indexOfOpenBrace).Split(' ');    //Find all the entries of the property tag
                        string propertyName = string.Empty;
                        string propertyValue = string.Empty;

                        //Which index is the value entry
                        int valueIndex = GetNameAndValue(ref propertyName, ref propertyValue, propertyParams);

                        bool found = false;

                        //We're going to loop through every tag we've been told to search for

                        if (propertyName.Equals(tag))
                        {
                            //Split the values in the property value by the '|' delimeter 
                            string[] splitValues = Util.FindParams(propertyValue);
                            for (int j = 0; j < splitValues.Length; j++)
                            {
                                string[] splitArgs = splitValues[j].Split('-');
                                //If we found a match, set the flag to true and overwrite the value of this string
                                if (splitArgs[0] == oldID.ToString())
                                {
                                    found = true;
                                    splitArgs[0] = SPECIAL_CHARACTER + newID.ToString() + SPECIAL_CHARACTER;
                                }

                                //Concatenate it to the newValue
                                newValue += splitArgs[0];

                                //If there are more entries coming, add the '|' back
                                if (splitArgs.Length > 1)
                                {
                                    for (int k = 1; k < splitArgs.Length; k++)
                                    {
                                        newValue = newValue + "-" + splitArgs[k];
                                    }
                                }

                                //If there are more entries coming, add the '|' back
                                if (j < splitValues.Length - 1)
                                {
                                    newValue += "|";
                                }
                            }

                            //Close the quote
                            newValue += "\"";
                        }

                        //Put the buffer back at the beginning of the line
                        _liAllLines[i] = buffer;
                        for (int j = 0; j < propertyParams.Length; j++)
                        {
                            //Either write the params as we get them, or sub in the dummy value, value is always
                            //last so we needto close the tag.
                            if (j == valueIndex && found) { _liAllLines[i] += newValue + "/>"; }
                            else { _liAllLines[i] += propertyParams[j]; }

                            //If there's another entry coming, put a space there
                            if (j < propertyParams.Length - 1)
                            {
                                _liAllLines[i] += " ";
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// This method will iterate through an array of property parameters and retrieve
            /// the value of the name and the value param.
            /// </summary>
            /// <param name="propertyName">ref to the propertyName string</param>
            /// <param name="propertyValue">ref to the propertyValue string</param>
            /// <param name="propertyParams">The propery param arrays</param>
            /// <returns>The index of the value parameter</returns>
            private int GetNameAndValue(ref string propertyName, ref string propertyValue, string[] propertyParams)
            {
                int rv = -1;
                //Iterate over the property parameters and collect the name of the property and its value
                for (int j = 0; j < propertyParams.Length; j++)
                {
                    if (propertyParams[j].Contains("="))
                    {
                        string[] splitParam = propertyParams[j].Split('=');
                        string pName = splitParam[0].Replace("\"", "");
                        string pValue = splitParam[1].Replace("\"", "").Replace("/", "").Replace(">", "");

                        if (pName.Equals("name")) { propertyName = pValue; }
                        else if (pName.Equals("value"))
                        {
                            rv = j;
                            propertyValue = pValue;
                        }
                    }
                }

                return rv;
            }

            /// <summary>
            /// Iterates through each line and remove all instances of the special character
            /// </summary>
            public void StripSpecialCharacter()
            {
                for (int i = 0; i < _liAllLines.Count; i++)
                {
                    string s = _liAllLines[i];
                    if (s.Contains(SPECIAL_CHARACTER))
                    {
                        string val = s;
                        _liAllLines[i] = val.Replace(SPECIAL_CHARACTER, "");
                    }
                }
            }
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

                foreach (string s in Enum.GetNames(typeof(ItemEnum)))
                {
                    AddContextMenuItem(s, dgvItemsContextMenuClick, false);
                }
            }
            else if (dgv == dgvWorldObjects)
            {
                AddContextMenuItem("Add New", AddNewWorldObject, true);

                foreach (string s in Enum.GetNames(typeof(ObjectTypeEnum)))
                {
                    if (!s.Equals("Building") && !s.Equals("Earth"))
                    {
                        AddContextMenuItem(s, dgvWorldObjectsContextMenuClick, false);
                    }
                }
            }
            else if(dgv == dgvMonsters)
            {
                AddContextMenuItem("Add New", AddNewMonster, false);
            }
            else if (dgv == dgvQuests)
            {
                AddContextMenuItem("Add New", AddNewQuest, false);
            }
            else if (dgv == dgvActions)
            {
                AddContextMenuItem("Add New", AddNewAction, false);
            }
            else if (dgv == dgvBuildings)
            {
                AddContextMenuItem("Add New", AddNewBuilding, false);
            }
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
        #endregion
        #region Add New
        private void AddNewItem(object sender, EventArgs e)
        {
            SaveItemInfo();
            AddNewGenericXMLObject(tabCtl.TabPages["tabItems"], "Items", dgvItems, "colItemID", "colItemName", tbItemName, tbItemID, dgItemTags, "colItemTags", cbItemType, tbItemDesc, new List<string>() { "Image:0-0" });
        }
        private void AddNewWorldObject(object sender, EventArgs e)
        {
            SaveWorldObjectInfo(_liWorldObjects);
            AddNewGenericXMLObject(tabCtl.TabPages["tabWorldObjects"], "WorldObjects", dgvWorldObjects, "colWorldObjectsID", "colWorldObjectsName", tbWorldObjectName, tbWorldObjectID, dgvWorldObjectTags, "colWorldObjectTags", cbWorldObjectType, null, new List<string>() { "Image:0-0" });
        }
        private void AddNewQuest(object sender, EventArgs e)
        {
            SaveQuestInfo(_diBasicXML[QUEST_XML_FILE]);
            AddNewGenericXMLObject(tabCtl.TabPages["tabQuests"], "Quests", dgvQuests, "colQuestsID", "colQuestsName", tbQuestName, tbQuestID, dgvQuestTags, "colQuestTags", cbQuestType, tbQuestDescription);
        }
        private void AddNewAction(object sender, EventArgs e)
        {
            SaveQuestInfo(_diBasicXML[ACTIONS_XML_FILE]);
            AddNewGenericXMLObject(tabCtl.TabPages["tabActions"], "Actions", dgvActions, "colActionsID", "colActionsName", tbActionName, tbActionID, dgvActionTags, "colActionTags", cbActionType, tbActionDescription);
        }
        private void AddNewMonster(object sender, EventArgs e)
        {
            SaveMonsterInfo(_diBasicXML[MONSTERS_XML_FILE]);
            List<string> defaultTags = new List<string>() { "Texture:", "Condition:", "Lvl", "Ability:", "Loot:", "Trait:", "Walk:0-0-3-0.15-T", "Attack:0-0-3-0.15-T", "Cast:0-0-3-0.15-T", "Hurt:0-0-3-0.15-T", "Critical:0-0-3-0.15-T", "KO:0-0-3-0.15-T" };
            AddNewGenericXMLObject(tabCtl.TabPages["tabMonsters"], "Monsters", dgvMonsters, "colMonstersID", "colMonstersName", tbMonsterName, tbMonsterID, dgvMonsterTags, "colMonsterTags", null, tbMonsterDescription, defaultTags);
        }
        private void AddNewBuilding(object sender, EventArgs e)
        {
            SaveBuildingInfo(_diBasicXML[BUILDINGS_XML_FILE]);
            List<string> defaultTags = new List<string>() { "Texture:", "Dimensions:", "FirstBase:", "Width:", "Height:", "Entrance:"};
            AddNewGenericXMLObject(tabCtl.TabPages["tabBuildings"], "Buildings", dgvBuildings, "colBuildingsID", "colBuildingsName", tbBuildingName, tbBuildingID, dgvBuildingTags, "colBuildingTags", null, tbBuildingDescription, defaultTags);
        }
        #endregion

        #endregion
    }
}
