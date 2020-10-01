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
        #region XML Files
        string QUEST_XML_FILE = PATH_TO_DATA + @"\Quests.xml";
        string CHARACTER_XML_FILE = PATH_TO_DATA + @"\CharacterData.xml";
        string CLASSES_XML_FILE = PATH_TO_DATA + @"\Classes.xml";
        string WORKERS_XML_FILE = PATH_TO_DATA + @"\Workers.xml";
        string CONFIG_XML_FILE = PATH_TO_DATA + @"\Config.xml";
        string MAGIC_SHOP_XML_FILE = PATH_TO_DATA + @"\Shops\MagicShop.xml";
        string ADVENTURERS_XML_FILE = PATH_TO_DATA + @"\Shops\Adventurers.xml";
        string BUILDINGS_XML_FILE = PATH_TO_DATA + @"\Shops\Buildings.xml";
        string ITEM_DATA_XML_FILE = PATH_TO_DATA + @"\ItemData.xml";
        string ITEM_TEXT_XML_FILE = PATH_TO_DATA + @"\Text Files\ItemText.xml";
        string WORLD_OBJECTS_DATA_XML_FILE = PATH_TO_DATA + @"\WorldObjects.xml";
        #endregion

        #region Tags
        const string ITEM_TAGS = "ReqItems,RefinesInto";
        const string ITEM_WORLD_TAGS = "Place";
        const string QUEST_ITEM_TAGS = "Item,GoalItem";
        const string CHARACTER_ITEM_TAGS = "Collection";
        const string WORLD_OBJECT_TAGS = "Makes,Processes,Item";
        const string CLASSES_ITEM_TAG = "DWeap,DArmor,DHead,DWrist";
        const string WORKERS_ITEM_TAG = "Item, ID";
        const string SHOP_TAG = "ItemID,Requires";
        const string CONFIG_ITEM_TAG = "ItemID";
        const string CONFIG_WORLD_TAG = "ID";
        const string DEFAULT_WORLD_TAG = "";
        public static string MAP_ITEM_TAGS = "Item";
        public static string MAP_WORLD_OBJECTS_TAG = "Resources,ID";
        #endregion

        List<ItemXMLData> _liItemData;
        List<XMLData> _liWorldObjects;

        private int _iCurrItemID = 0;
        private int _iCurrWorldObjectID = 0;
        private int _iNextCurrID = -1;
        public static string SPECIAL_CHARACTER = "^";
        static string PATH_TO_MAPS = string.Format(@"{0}\..\..\..\..\Adventure\Content\Maps", System.Environment.CurrentDirectory);
        static string PATH_TO_DATA = string.Format(@"{0}\..\..\..\..\Adventure\Content\Data", System.Environment.CurrentDirectory);

        static Dictionary<int, Dictionary<string, string>> _diItemText;
        static Dictionary<ItemEnum, List<ItemXMLData>> _diItems;
        static Dictionary<ObjectTypeEnum, List<XMLData>> _diWorldObjectData;
        static Dictionary<string, List<XMLData>> _diBasicXML;
        Dictionary<string, TMXData> _diMapData;

        public frmDBEditor()
        {
            InitializeComponent();

            cbItemType.Items.Clear();
            foreach (ItemEnum e in Enum.GetValues(typeof(ItemEnum)))
            {
                cbItemType.Items.Add("Type:" + e.ToString());
            }
            cbItemType.SelectedIndex = 0;

            cbWorldObjectType.Items.Clear();
            foreach (ObjectTypeEnum e in Enum.GetValues(typeof(ObjectTypeEnum)))
            {
                cbWorldObjectType.Items.Add("Type:" + e.ToString());
            }
            cbWorldObjectType.SelectedIndex = 0;
            _diMapData = new Dictionary<string, TMXData>();
            _diBasicXML = new Dictionary<string, List<XMLData>>();
            _diWorldObjectData = new Dictionary<ObjectTypeEnum, List<XMLData>>();
            _diItems = new Dictionary<ItemEnum, List<ItemXMLData>>();

            _diItemText = ReadXMLFile(ITEM_TEXT_XML_FILE);

            LoadXMLDictionary(QUEST_XML_FILE, QUEST_ITEM_TAGS, DEFAULT_WORLD_TAG);
            LoadXMLDictionary(CHARACTER_XML_FILE, CHARACTER_ITEM_TAGS, DEFAULT_WORLD_TAG);
            LoadXMLDictionary(CLASSES_XML_FILE, CLASSES_ITEM_TAG, DEFAULT_WORLD_TAG);
            LoadXMLDictionary(WORKERS_XML_FILE, WORKERS_ITEM_TAG, DEFAULT_WORLD_TAG);
            LoadXMLDictionary(CONFIG_XML_FILE, CONFIG_ITEM_TAG, CONFIG_WORLD_TAG);

            LoadXMLDictionary(MAGIC_SHOP_XML_FILE, SHOP_TAG, DEFAULT_WORLD_TAG);
            LoadXMLDictionary(BUILDINGS_XML_FILE, SHOP_TAG, DEFAULT_WORLD_TAG);
            LoadXMLDictionary(ADVENTURERS_XML_FILE, SHOP_TAG, DEFAULT_WORLD_TAG);

            LoadWorldObjects();
            LoadItemData();

            LoadItemDatagrid();
            LoadWorldObjectDataGrid();

            LoadItemInfo();
            LoadWorldObjectInfo();
        }

        #region DataGridView Loading
        private void LoadItemDatagrid()
        {
            dgItems.Rows.Clear();
            List<string> names = new List<string>();
            for (int i = 0; i < _liItemData.Count; i++)
            {
                dgItems.Rows.Add();
                DataGridViewRow row = dgItems.Rows[i];

                row.Cells["colItemID"].Value = _liItemData[i].ID;
                row.Cells["colItemName"].Value = _liItemData[i].Name;
            }

            SelectRow(dgItems, _iCurrItemID);
            dgItems.Focus();
        }

        private void LoadWorldObjectDataGrid()
        {
            dgWorldObjects.Rows.Clear();
            List<string> names = new List<string>();
            for (int i = 0; i < _liWorldObjects.Count; i++)
            {
                dgWorldObjects.Rows.Add();
                DataGridViewRow row = dgWorldObjects.Rows[i];

                row.Cells["colWorldObjectsID"].Value = _liWorldObjects[i].ID;
                row.Cells["colWorldObjectsName"].Value = _liWorldObjects[i].GetTagInfo("Name");
            }

            SelectRow(dgWorldObjects, _iCurrWorldObjectID);
        }
        #endregion

        #region Load Info Panes
        private void LoadItemInfo()
        {
            ItemXMLData data = _liItemData[_iCurrItemID];
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
            XMLData data = _liWorldObjects[_iCurrWorldObjectID];
            tbWorldObjectName.Text = data.GetTagInfo("Name");
            tbWorldObjectID.Text = data.ID.ToString();

            cbWorldObjectType.SelectedIndex = (int)Util.ParseEnum<ObjectTypeEnum>(data.GetTagInfo("Type"));

            dgWorldObjectTags.Rows.Clear();
            string[] tags = data.GetTagsString().Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (!s.StartsWith("Type") && !s.StartsWith("Name"))
                {
                    dgWorldObjectTags.Rows.Add(s);
                }
            }
        }
        #endregion


        private Dictionary<int, Dictionary<string, string>> ReadXMLFile(string fileName)
        {
            string line = string.Empty;
            Dictionary<int, Dictionary<string, string>> xmlDictionary = new Dictionary<int, Dictionary<string, string>>();

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

                xmlDictionary[int.Parse(xmlnode[i].ChildNodes.Item(0).InnerText)] = tagDictionary;
            }

            return xmlDictionary;
        }

        private void LoadXMLDictionary(string fileName, string itemTags, string objectTags)
        {
            List<XMLData> data = new List<XMLData>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in ReadXMLFile(fileName))
            {
                data.Add(new XMLData(kvp.Key, kvp.Value, itemTags, objectTags));
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
            Dictionary<int, Dictionary<string, string>> worldObjectDictionary = ReadXMLFile(WORLD_OBJECTS_DATA_XML_FILE);
            for (int i = 0; i < 1001; i++)
            {
                if (worldObjectDictionary.ContainsKey(i))
                {
                    Dictionary<string, string> stringData = worldObjectDictionary[i];
                    if (stringData != null)
                    {
                        _liWorldObjects.Add(new XMLData(i, stringData, WORLD_OBJECT_TAGS, DEFAULT_WORLD_TAG));
                    }
                }
                else { break; }
            }

            foreach (ObjectTypeEnum e in Enum.GetValues(typeof(ObjectTypeEnum)))
            {
                _diWorldObjectData[e] = new List<XMLData>();
                _diWorldObjectData[e].AddRange(_liWorldObjects.FindAll(x => Util.ParseEnum<ObjectTypeEnum>(x.GetStringValue("Type")) == e));
            }
        }

        private void LoadItemData()
        {
            //Load in all the Item Data
            _liItemData = new List<ItemXMLData>();
            Dictionary<int, Dictionary<string, string>> itemDictionary = ReadXMLFile(ITEM_DATA_XML_FILE);
            for (int i = 0; i < 1001; i++)
            {
                if (itemDictionary.ContainsKey(i))
                {
                    Dictionary<string, string> stringData = itemDictionary[i];
                    if (stringData != null)
                    {
                        _liItemData.Add(new ItemXMLData(i, stringData, ITEM_TAGS, ITEM_WORLD_TAGS));
                    }
                }
                else { break; }
            }

            FindLinkedXMLObjects(_liItemData);
        }

        /// <summary>
        /// This iterates through all the ItemData entries and compares them against
        /// the other objects, ensuring that eachobject knows about any object that
        /// rfeferences it.
        /// </summary>
        /// <param name="dataList"></param>
        private void FindLinkedXMLObjects(List<ItemXMLData> dataList)
        {
            //Compare item Data against the other Items
            for (int i = 0; i < dataList.Count; i++)
            {
                ItemXMLData theData = dataList[i];
                for (int j = 0; j < dataList.Count; j++)
                {
                    XMLData testIt = dataList[j];
                    if (testIt.RefersToID(theData.ID))
                    {
                        theData.AddLinkedItem(testIt);
                    }
                }

                foreach (string s in _diBasicXML.Keys)
                {
                    foreach (XMLData testIt in _diBasicXML[s])
                    {
                        if (testIt.RefersToID(theData.ID))
                        {
                            theData.AddLinkedItem(testIt);
                        }
                    }
                }

                //Compare ItemData against the WorldObjectData
                foreach (ObjectTypeEnum e in Enum.GetValues(typeof(ObjectTypeEnum)))
                {
                    foreach (XMLData testIt in _diWorldObjectData[e])
                    {
                        //First, check to see if the object refers to the item this
                        //could be because the object makes the item for example
                        if (testIt.RefersToID(theData.ID))
                        {
                            theData.AddLinkedItem(testIt);
                        }

                        //Next check to see if the item refers to the object, pass in
                        //false here to ensure that we compare only to the worldObject tags
                        //The item might place the object.
                        if (theData.RefersToID(testIt.ID, false))
                        {
                            testIt.AddLinkedItem(theData);
                        }
                    }
                }

                //Find any maps that reference the ItemID
                foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                {
                    if (kvp.Value.RefersToIDWithTag(theData.ID, MAP_ITEM_TAGS))
                    {
                        theData.AddLinkedItem(kvp.Value);
                    }
                }
            }

            //Compare maps against the worldObjects
            foreach (ObjectTypeEnum e in Enum.GetValues(typeof(ObjectTypeEnum)))
            {
                foreach (XMLData theData in _diWorldObjectData[e])
                {
                    //Find any maps that reference the ObjectID
                    foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                    {
                        if (kvp.Value.RefersToIDWithTag(theData.ID, MAP_WORLD_OBJECTS_TAG))
                        {
                            theData.AddLinkedItem(kvp.Value);
                        }
                    }

                    //Find any files that reference the ObjectID
                    foreach (string s in _diBasicXML.Keys)
                    {
                        foreach (XMLData testIt in _diBasicXML[s])
                        {
                            if (testIt.RefersToID(theData.ID, false))
                            {
                                theData.AddLinkedItem(testIt);
                            }
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
                    if(data.ID == _iCurrItemID) { _iNextCurrID = index; }
                    data.ChangeID(index++);
                    itemDataList.Add(data);
                }
            }

            index = 0;
            foreach (ObjectTypeEnum e in Enum.GetValues(typeof(ObjectTypeEnum)))
            {
                foreach (XMLData data in _diWorldObjectData[e])
                {
                    data.ChangeID(index++, false);
                    worldObjectDataList.Add(data);
                }
            }
        }

        private void SelectRow(DataGridView dg, int id)
        {
            dg.Rows[id].Selected = true;
            dg.CurrentCell = dg.Rows[id].Cells[0];
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

        #region EventHandlers
        private void btnItemSave_Click(object sender, EventArgs e)
        {
            if (_liItemData.Count == _iCurrItemID)
            {
                Dictionary<string, string> diText = new Dictionary<string, string>
                {
                    ["Name"] = tbItemName.Text,
                    ["Description"] = tbItemDesc.Text
                };

                _diItemText[int.Parse(tbItemID.Text)] = diText;

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

                _liItemData.Add(new ItemXMLData(_iCurrItemID, tags, ITEM_TAGS, ITEM_WORLD_TAGS));
            }
            else
            {
                SaveItemInfo(_liItemData[_iCurrItemID]);
            }

            LoadItemDatagrid();
        }

        private void btnWorldObjectSave_Click(object sender, EventArgs e)
        {
            if (_liWorldObjects.Count == _iCurrWorldObjectID)
            {
                Dictionary<string, string> tags = new Dictionary<string, string>();
                tags["Name"] = tbWorldObjectName.Text;

                string[] typeTag = cbWorldObjectType.SelectedItem.ToString().Split(':');
                tags[typeTag[0]] = typeTag[1];

                foreach (DataGridViewRow row in dgWorldObjectTags.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string key = tagInfo[0];
                        string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                        if (key != "Type")
                        {
                            tags[key] = val;
                        }
                    }
                }

                _liWorldObjects.Add(new XMLData(_iCurrItemID, tags, WORLD_OBJECT_TAGS, DEFAULT_WORLD_TAG));
            }
            else
            {
                SaveWorldObjectInfo(_liWorldObjects[_iCurrWorldObjectID]);
            }

            LoadWorldObjectDataGrid();
        }

        private void SaveItemInfo(ItemXMLData data)
        {
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

        private void SaveWorldObjectInfo(XMLData data)
        {
            data.ClearTagInfo();
            data.SetTagInfo("Name", tbWorldObjectName.Text);
            string[] typeTag = cbWorldObjectType.SelectedItem.ToString().Split(':');
            data.SetTagInfo(typeTag[0], typeTag[1]);
            foreach (DataGridViewRow row in dgWorldObjectTags.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    string[] tagInfo = row.Cells[0].Value.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    string key = tagInfo[0];
                    string val = (tagInfo.Length == 2 ? tagInfo[1] : string.Empty);
                    if (key != "Type")
                    {
                        data.SetTagInfo(key, val);
                    }
                }
            }
        }

        private void btnItemCancel_Click(object sender, EventArgs e)
        {
            if (_liItemData.Count == _iCurrItemID)
            {
                dgItems.Rows.RemoveAt(_iCurrItemID--);
                SelectRow(dgItems, _iCurrItemID);
            }

            LoadItemInfo();
        }

        private void dgItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                if (_liItemData.Count == _iCurrItemID)
                {
                    dgItems.Rows.RemoveAt(_iCurrItemID--);
                }

                _iCurrItemID = int.Parse(dgItems.Rows[e.RowIndex].Cells[0].Value.ToString());
                LoadItemInfo();
            }
        }

        private void dgWorldObjects_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                if (_liWorldObjects.Count == _iCurrWorldObjectID)
                {
                    dgWorldObjects.Rows.RemoveAt(_iCurrWorldObjectID--);
                }

                _iCurrWorldObjectID = int.Parse(dgWorldObjects.Rows[e.RowIndex].Cells[0].Value.ToString());
                LoadWorldObjectInfo();
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_liItemData.Count == _iCurrItemID) { btnItemSave_Click(sender, e); }

            _liItemData.Sort((x, y) =>
            {
                var typeComp = x.ItemType.CompareTo(y.ItemType);
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
            foreach (ObjectTypeEnum it in Enum.GetValues(typeof(ObjectTypeEnum)))
            {
                foreach (XMLData data in _diWorldObjectData[it])
                {
                    data.StripSpecialCharacter();
                }
            }

            foreach (string s in _diBasicXML.Keys)
            {
                foreach (XMLData data in _diBasicXML[s])
                {
                    data.StripSpecialCharacter();
                }
                SaveXMLData(_diBasicXML[s], s);
            }

            string mapPath = PATH_TO_MAPS;
            if (!Directory.Exists(mapPath)) { Directory.CreateDirectory(mapPath); }
            foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
            {
                kvp.Value.StripSpecialCharacter();
                DirectoryInfo dirInfo = Directory.GetParent(mapPath + "\\" + kvp.Key);
                if (!Directory.Exists(dirInfo.FullName)) { Directory.CreateDirectory(dirInfo.FullName); }
                SaveTMXData(kvp.Value, dirInfo.FullName + "\\" + Path.GetFileName(kvp.Key) + ".tmx");
            }

            SaveItemXMLData(itemDataList, PATH_TO_DATA);
            SaveXMLData(worldObjectDataList, WORLD_OBJECTS_DATA_XML_FILE);

            if (_iNextCurrID != -1)
            {
                _iCurrItemID = _iNextCurrID;
                _iNextCurrID = -1;
            }

            LoadItemDatagrid();
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _iCurrItemID = dgItems.Rows.Count;
            dgItems.Rows.Add();
            SelectRow(dgItems, _iCurrItemID);

            DataGridViewRow row = dgItems.Rows[_iCurrItemID];
            row.Cells["colItemID"].Value = _iCurrItemID;
            row.Cells["colItemName"].Value = "New";

            tbItemName.Text = "";
            tbItemDesc.Text = "";
            tbItemID.Text = _iCurrItemID.ToString();

            cbItemType.SelectedIndex = 0;

            dgItemTags.Rows.Clear();
            dgItemTags.Rows.Add();
            row = dgItemTags.Rows[0];
            row.Cells["colItemTags"].Value = "Image:0-0";

            tbItemName.Focus();
        }

        private void cbItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetItemSubtype();
        }
        #endregion

        #region Save Methods
        private static StreamWriter PrepareXMLFile(string fileName, string assetType)
        {
            StreamWriter dataFile = new StreamWriter(fileName);
            dataFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            dataFile.WriteLine("<XnaContent xmlns:Generic=\"System.Collections.Generic\">");
            dataFile.WriteLine("  <Asset Type=\"" + assetType + "\">"); //Dictionary[int, string]
            return dataFile;
        }

        private static void CloseStreamWriter(ref StreamWriter dataFile)
        {
            dataFile.WriteLine("  </Asset>");
            dataFile.WriteLine("</XnaContent>");
            dataFile.Close();
        }

        public static void SaveXMLData(List<XMLData> dataList, string fileName)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[int, string]");

            foreach (XMLData data in dataList)
            {
                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>{0}</Value>", data.GetTagsString()));
            }

            CloseStreamWriter(ref dataFile);
        }

        private static void WriteXMLEntry(StreamWriter dataFile, string key, string value)
        {
            dataFile.WriteLine("    <Item>");
            dataFile.WriteLine(key);
            dataFile.WriteLine(value);
            dataFile.WriteLine("    </Item>");
        }

        public static void SaveItemXMLData(List<ItemXMLData> dataList, string pathToDir)
        {
            StreamWriter dataFile = PrepareXMLFile(pathToDir + @"\ItemData.xml", "Dictionary[int, string]");
            StreamWriter textFile = PrepareXMLFile(pathToDir + @"\Text Files\ItemText.xml", "Dictionary[int, string]");

            foreach (ItemXMLData data in dataList)
            {
                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>{0}</Value>", data.GetTagsString()));
                WriteXMLEntry(textFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>[Name:{0}][Description:{1}]</Value>", data.Name, data.Description));
            }

            CloseStreamWriter(ref dataFile);
            CloseStreamWriter(ref textFile);
        }       

        public static void SaveTMXData(TMXData data, string fileName)
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
            protected int _iID;
            public int ID => _iID;
            protected string[] _arrItemTags;
            protected string[] _arrWorldObjectTags;
            protected List<XMLData> _liLinkedItems;
            protected List<TMXData> _liLinkedMaps;
            protected Dictionary<string, string> _diTags;

            public XMLData(int id, Dictionary<string, string> stringData, string itemTags, string objectTags)
            {
                _liLinkedMaps = new List<TMXData>();
                _liLinkedItems = new List<XMLData>();
                _arrItemTags = itemTags.Split(',');
                _arrWorldObjectTags = objectTags.Split(',');

                _iID = id;
                _diTags = stringData;
            }

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

            public string GetTagInfo(string key)
            {
                if (_diTags.ContainsKey(key)) { return _diTags[key]; }
                else { return string.Empty; }
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
            public bool RefersToID(int id, bool item = true)
            {
                bool rv = false;

                foreach (string s in (item ? _arrItemTags : _arrWorldObjectTags))
                {
                    CheckTagForID(s, id, ref rv);
                }

                return rv;
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
            private void CheckTagForID(string tag, int id, ref bool val)
            {
                //If we don't have the key, don't proceed
                if (_diTags.ContainsKey(tag))
                {
                    //Isolate every group of entries that aredelineated by the '|'
                    string[] split = Util.GetEntries(_diTags[tag]);
                    foreach (string s in split)
                    {
                        //The first entry is always the item, split by the '-', find it and compare
                        string[] splitData = s.Split('-');
                        if (int.Parse(splitData[0]) == id)
                        {
                            val = true;
                            return;
                        }
                    }
                }
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

                    foreach (XMLData d in _liLinkedItems)
                    {
                        d.ReplaceLinkedIDs(oldID, _iID, item);
                    }

                    foreach (TMXData d in _liLinkedMaps)
                    {
                        d.ReplaceID(MAP_WORLD_OBJECTS_TAG, oldID, newID);
                    }
                }
            }

            /// <summary>
            /// Iterates through the relevant tags to replace any instances of the 
            /// old ID with the new ID.
            /// </summary>
            /// <param name="oldID">The old ID that has now changed</param>
            /// <param name="newID">The new ID to reference</param>
            public void ReplaceLinkedIDs(int oldID, int newID, bool item = true)
            {
                foreach (string s in (item ? _arrItemTags : _arrWorldObjectTags))
                {
                    ReplaceID(s, oldID, newID);
                }
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
                    string[] split = Util.GetEntries(_diTags[tag]);
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
            public void AddLinkedItem(XMLData d)
            {
                if (!_liLinkedItems.Contains(d) && this != d)
                {
                    _liLinkedItems.Add(d);
                }
            }
            public void AddLinkedItem(TMXData d)
            {
                if (!_liLinkedMaps.Contains(d))
                {
                    _liLinkedMaps.Add(d);
                }
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
            string _sName;
            public string Name => _sName;
            string _sDescription;
            public string Description => _sDescription;
            ItemEnum _eType;
            public ItemEnum ItemType => _eType;

            public ItemXMLData(int id, Dictionary<string, string> stringData, string itemTags, string worldTags) : base(id, stringData, itemTags, worldTags)
            {
                _eType = Util.ParseEnum<ItemEnum>(_diTags["Type"]);
                _sName = _diItemText[id]["Name"];
                _sDescription = _diItemText[id]["Description"];
            }

            public void SetTextData(string name, string desc)
            {
                _sName = name;
                _sDescription = desc;
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
            public bool RefersToIDWithTag(int id, string tags)
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
                        string[] tagArray = tags.Split(',');
                        foreach (string tag in tagArray)
                        {
                            if (propertyName.Equals(tag))
                            {
                                //Split the values in the property value by the '|' delimeter 
                                string[] splitValues = Util.GetEntries(propertyValue);
                                foreach (string spVal in splitValues)
                                {
                                    //Do we have a match? return true
                                    if (spVal == id.ToString())
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Call this to check the given tag for the given ID.
            /// 
            /// Replace any instances of the old ID that are found with the new ID
            /// /// </summary>
            /// <param name="tag">Tags to look at, delmitited by ','</param>
            /// <param name="oldID">The ID to look for</param>
            /// <param name="newID">The ID to replace the olf one with</param>
            public void ReplaceID(string tags, int oldID, int newID)
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
                        string[] tagArray = tags.Split(',');
                        foreach (string tag in tagArray)
                        {
                            if (propertyName.Equals(tag))
                            {
                                //Split the values in the property value by the '|' delimeter 
                                string[] splitValues = Util.GetEntries(propertyValue);
                                for (int j = 0; j < splitValues.Length; j++)
                                {
                                    //If we found a match, set the flag to true and overwrite the value of this string
                                    if (splitValues[j] == oldID.ToString())
                                    {
                                        found = true;
                                        splitValues[j] = SPECIAL_CHARACTER + newID.ToString() + SPECIAL_CHARACTER;
                                    }

                                    //Concatenate it to the newValue
                                    newValue += splitValues[j];

                                    //If there are more entries coming, add the '|' back
                                    if (j < splitValues.Length - 1)
                                    {
                                        newValue += "|";
                                    }
                                }

                                //Close the quote
                                newValue += "\"";
                            }
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

        private void tabCtl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabCtl.SelectedTab == tabCtl.TabPages["tabWorldObjects"])
            {
                dgWorldObjects.Focus();
            }
            else if (tabCtl.SelectedTab == tabCtl.TabPages["tabItems"])
            {
                dgItems.Focus();
            }
        }
    }
}
