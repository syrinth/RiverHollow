using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.WorldObjects.Item;
using static RiverHollow.WorldObjects.WorldObject;

namespace RiverHollow.GUIComponents.Screens
{
    class ItemDataScreen : GUIScreen
    {
        #region XML Files
        string QUEST_XML_FILE = PATH_TO_DATA + @"\Quests.xml";
        string CHARACTER_XML_FILE = PATH_TO_DATA + @"\CharacterData.xml";
        string CLASSES_XML_FILE = PATH_TO_DATA + @"\Classes.xml";
        string WORKERS_XML_FILE = PATH_TO_DATA + @"\Workers.xml";
        string MAGIC_SHOP_XML_FILE = PATH_TO_DATA + @"\Shops\MagicShop.xml";
        string ADVENTURERS_XML_FILE = PATH_TO_DATA + @"\Shops\Adventurers.xml";
        string BUILDINGS_XML_FILE = PATH_TO_DATA + @"\Shops\Buildings.xml";
        #endregion

        #region Tags
        const string ITEM_TAGS = "ReqItems,RefinesInto,Place";
        const string QUEST_TAGS = "Item,GoalItem";
        const string CHARACTER_TAGS = "Collection";
        const string WORLD_OBJECT_TAGS = "Makes,Processes,Item";
        const string CLASSES_TAG = "DWeap,DArmor,DHead,DWrist";
        const string WORKERS_TAG = "Item, ID";
        const string SHOP_TAG = "ItemID,Requires";
        public static string MAP_ITEM_TAGS = "Item";
        public static string MAP_WORLD_OBJECTS_TAG = "Resources,ID";
        #endregion

        public static string SPECIAL = "^";
        static string PATH_TO_MAPS = string.Format(@"{0}\..\..\..\..\Content\Maps", System.Environment.CurrentDirectory);
        static string PATH_TO_DATA = string.Format(@"{0}\..\..\..\..\Content\Data", System.Environment.CurrentDirectory);

        static Rectangle RECT_IMG = new Rectangle(254, 14, 20, 20);
        static Rectangle RECT_SELECT_IMG = new Rectangle(286, 14, 20, 20);
        const int ROWS = 9;
        const int COLUMNS = 15;
        ItemBox[,] _arrItemBoxes;
        GUIWindow _gWin;
        GUIMainObject _gMainObject;
        static GUIButton _btnSave;
        static GUIButton _btnAddNew;
        static Dictionary<ItemEnum, List<ItemXMLData>> _diItems;
        static Dictionary<ObjectType, List<XMLData>> _diWorldObjectData;
        static Dictionary<string, List<XMLData>> _diBasicXML;
        Dictionary<string, TMXData> _diMapData;

        static ItemXMLData _heldData;

        public ItemDataScreen()
        {
            _diMapData = new Dictionary<string, TMXData>();
            _diBasicXML = new Dictionary<string, List<XMLData>>();
            _diWorldObjectData = new Dictionary<ObjectType, List<XMLData>>();
            _diItems = new Dictionary<ItemEnum, List<ItemXMLData>>();
            _gWin = new GUIWindow(GUIWindow.RedWin, 10, 10);
            AddControl(_gWin);
            List<GUIObject> list = new List<GUIObject>();
            _arrItemBoxes = new ItemBox[ROWS, COLUMNS];

            //Create and place the ItemBoxes
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLUMNS; j++)
                {
                    ItemBox box = new ItemBox((i * COLUMNS) + j);
                    _arrItemBoxes[i, j] = box;

                    if (i == 0 && j == 0) { _arrItemBoxes[i, j].AnchorToInnerSide(_gWin, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN); }
                    else if (j == 0) { _arrItemBoxes[i, j].AnchorAndAlignToObject(_arrItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                    else { _arrItemBoxes[i, j].AnchorAndAlignToObject(_arrItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                    _gWin.AddControl(box);
                }
            }

            foreach (string dir in Directory.GetDirectories(PATH_TO_MAPS))
            {
                foreach(string file in Directory.GetFiles(dir))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.StartsWith("map"))
                    {
                        string dirName = Path.GetDirectoryName(file);
                        string key = Path.GetFileName(dirName) + "\\" + fileName;
                        _diMapData[key] = new TMXData(file);
                    }
                }
            }

            LoadXMLDictionary(QUEST_XML_FILE, QUEST_TAGS, DataManager.DiQuestData);
            LoadXMLDictionary(CHARACTER_XML_FILE, CHARACTER_TAGS, DataManager.DiVillagerData);
            LoadXMLDictionary(CLASSES_XML_FILE, CLASSES_TAG, DataManager.DIClasses);
            LoadXMLDictionary(WORKERS_XML_FILE, WORKERS_TAG, DataManager.DIWorkers);

            LoadXMLDictionary(MAGIC_SHOP_XML_FILE, SHOP_TAG, DataManager.GetMerchandise("MagicShop"));
            LoadXMLDictionary(BUILDINGS_XML_FILE, SHOP_TAG, DataManager.GetMerchandise("Buildings"));
            LoadXMLDictionary(ADVENTURERS_XML_FILE, SHOP_TAG, DataManager.GetMerchandise("Adventurers"));

            LoadWorldObjects();
            LoadItemData();

            _gWin.Resize();
            _gWin.AnchorToScreen(SideEnum.Right);

            _btnSave = new GUIButton("Save", Save);
            _btnSave.AnchorAndAlignToObject(_gWin, SideEnum.Bottom, SideEnum.Right, GUIManager.STANDARD_MARGIN);

            _btnAddNew = new GUIButton("Add New", AddNewItem);
            _btnAddNew.AnchorAndAlignToObject(_btnSave, SideEnum.Left, SideEnum.Bottom);
            AddControl(_btnSave);
            AddControl(_btnAddNew);
        }

        private void LoadXMLDictionary(string fileName, string relevantTags, Dictionary<int, Dictionary<string, string>> dataDictionary)
        {
            List<XMLData> data = new List<XMLData>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in dataDictionary)
            {
                data.Add(new XMLData(kvp.Key, kvp.Value, relevantTags));
            }

            _diBasicXML[fileName] = data;
        }

        /// <summary>
        /// Loads the WorldObjectData
        /// </summary>
        private void LoadWorldObjects()
        {
            //Load in all the WorldObject Data
            List<XMLData> worldObjectList = new List<XMLData>();
            for (int i = 0; i < 1001; i++)
            {
                Dictionary<string, string> stringData = DataManager.GetWorldObjectData(i);
                if (stringData != null)
                {
                    worldObjectList.Add(new XMLData(i, stringData, WORLD_OBJECT_TAGS));
                }
            }

            foreach (ObjectType e in Enum.GetValues(typeof(ObjectType)))
            {
                _diWorldObjectData[e] = new List<XMLData>();
                _diWorldObjectData[e].AddRange(worldObjectList.FindAll(x => Util.ParseEnum<ObjectType>(x.GetStringValue("Type")) == e));
            }
        }

        /// <summary>
        /// Loads the ItemData and creates and places the buttons
        /// </summary>
        private void LoadItemData()
        {
            //Load in all the Item Data
            List<ItemXMLData> dataList = new List<ItemXMLData>();
            for (int i = 0; i < 1001; i++)
            {
                Dictionary<string, string> stringData = DataManager.GetItemStringData(i);
                if (stringData != null)
                {
                    dataList.Add(new ItemXMLData(i, stringData, ITEM_TAGS));
                }
            }

            FindLinkedXMLObjects(dataList);

            List<GUIObject> liButtons = new List<GUIObject>();
            foreach (ItemEnum e in Enum.GetValues(typeof(ItemEnum)))
            {
                _diItems[e] = new List<ItemXMLData>();
                _diItems[e].AddRange(dataList.FindAll(x => Util.ParseEnum<ItemEnum>(x.GetStringValue("Type")) == e));

                ItemDataButton button = new ItemDataButton(e.ToString(), LoadInfo);
                liButtons.Add(button);
                AddControl(button);
            }

            GUIObject.CreateSpacedColumn(ref liButtons, GUIButton.BTN_WIDTH / 2, 0, RiverHollow.ScreenHeight, 10);
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
                foreach (ObjectType e in Enum.GetValues(typeof(ObjectType)))
                {
                    foreach (XMLData testIt in _diWorldObjectData[e])
                    {
                        if (testIt.RefersToID(theData.ID))
                        {
                            theData.AddLinkedItem(testIt);
                        }
                        if (theData.RefersToID(testIt.ID))
                        {
                            testIt.AddLinkedItem(theData);
                        }
                    }
                }

                //Replace all item numbers on the maps
                foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                {
                    if (kvp.Value.RefersToIDWithTag(theData.ID, MAP_ITEM_TAGS))
                    {
                        theData.AddLinkedItem(kvp.Value);
                    }
                }
            }

            //Compare maps against the worldObjects
            foreach (ObjectType e in Enum.GetValues(typeof(ObjectType)))
            {
                foreach (XMLData theData in _diWorldObjectData[e])
                {
                    //Replace all worldObject numbers on the maps
                    foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
                    {
                        if (kvp.Value.RefersToIDWithTag(theData.ID, MAP_WORLD_OBJECTS_TAG))
                        {
                            theData.AddLinkedItem(kvp.Value);
                        }
                    }
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_gMainObject != null)
            {
                rv = _gMainObject.ProcessLeftButtonClick(mouse);
            }
            else
            {
                rv = base.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        /// <summary>
        /// Right-click to back out as usual.
        /// 
        /// Drop anything that might be being held
        /// </summary>
        public override bool ProcessRightButtonClick(Point mouse)
        {
            _heldData = null;
            GUIManager.SetScreen(new IntroMenuScreen());

            return true;
        }

        /// <summary>
        /// This method loads allof the items of the indicated type
        /// </summary>
        /// <param name="e">The item type to load</param>
        private void LoadInfo(ItemEnum e)
        {
            int currRow = 0;
            int currCol = 0;

            //Clear the ItemBoxes so we don't wind up melding any
            foreach (ItemBox box in _arrItemBoxes)
            {
                box.SetData(null);
            }

            //Place the XMLData entries in the ItemBoxes
            foreach (ItemXMLData data in _diItems[e])
            {
                _arrItemBoxes[currRow, currCol].SetData(data);

                currCol++;
                if (currCol == COLUMNS)
                {
                    currCol = 0;
                    currRow++;
                }
            }
        }

        /// <summary>
        /// Calls upon the SaveManager to save the XMLData to the appropriate files
        /// </summary>
        private void Save()
        {
            List<ItemXMLData> itemDataList = new List<ItemXMLData>();
            List<XMLData> worldObjectDataList = new List<XMLData>();
            ChangeIDs(ref itemDataList, ref worldObjectDataList);

            //Strip the special case character from the Item files
            foreach (ItemEnum e in Enum.GetValues(typeof(ItemEnum)))
            {
                foreach (ItemXMLData data in _diItems[e])
                {
                    if (data != null) { data.StripSpecialCharacter(); }
                }
            }

            //Strip the special case character from the WorldObject files
            foreach (ObjectType e in Enum.GetValues(typeof(ObjectType)))
            {
                foreach (XMLData data in _diWorldObjectData[e])
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
                SaveManager.SaveXMLData(_diBasicXML[s], s);
            }

            string mapPath = System.Environment.CurrentDirectory + "\\Maps";
            if (!Directory.Exists(mapPath)) { Directory.CreateDirectory(mapPath); }
            foreach (KeyValuePair<string, TMXData> kvp in _diMapData)
            {
                kvp.Value.StripSpecialCharacter();
                DirectoryInfo dirInfo = Directory.GetParent(mapPath + "\\" + kvp.Key);
                if (!Directory.Exists(dirInfo.FullName)) { Directory.CreateDirectory(dirInfo.FullName); }
                SaveManager.SaveTMXData(kvp.Value, mapPath + "\\" + kvp.Key + ".tmx");
            }

            SaveManager.SaveItemXMLData(itemDataList, PATH_TO_DATA);
            SaveManager.SaveXMLData(worldObjectDataList, PATH_TO_DATA + @"\WorldObjects.xml");
        }

        private void ChangeIDs(ref List<ItemXMLData> itemDataList, ref List<XMLData> worldObjectDataList)
        {
            //Change all IDs
            int index = 0;
            foreach (ItemEnum e in Enum.GetValues(typeof(ItemEnum)))
            {
                foreach (ItemXMLData data in _diItems[e])
                {
                    if (data != null)
                    {
                        data.ChangeID(index++);
                        itemDataList.Add(data);
                    }
                }
            }

            index = 0;
            foreach (ObjectType e in Enum.GetValues(typeof(ObjectType)))
            {
                foreach (XMLData data in _diWorldObjectData[e])
                {
                    data.ChangeID(index++);
                    worldObjectDataList.Add(data);
                }
            }
        }

        private void AddNewItem()
        {
            GUIManager.OpenMainObject(new ItemEditor());
        }

        #region Main Object
        public override void OpenMainObject(GUIMainObject o)
        {
            RemoveControl(_gMainObject);
            _gMainObject = o;
            AddControl(_gMainObject);
        }
        public override void CloseMainObject()
        {
            RemoveControl(_gMainObject);
            _gMainObject = null;
        }
        #endregion

        class ItemEditor : GUIMainObject
        {
            GUIWindow _gWin;
            GUITextInputWindow _gName;
            GUITextInputWindow _gDescription;
            GUITextInputWindow _gDetails;
            GUIButton _btnSave;

            public ItemEditor()
            {
                _gWin = SetMainWindow();

                _gName = new GUITextInputWindow("Name:", SideEnum.Left, 20);
                _gName.AnchorToInnerSide(_gWin, SideEnum.TopLeft);
                _gDescription = new GUITextInputWindow("Description:", SideEnum.Left, 60);
                _gDescription.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);
                _gDetails = new GUITextInputWindow("Details:", SideEnum.Left, 200);
                _gDetails.AnchorAndAlignToObject(_gDescription, SideEnum.Bottom, SideEnum.Left);
                _gDetails.AllowAll = true;

                _btnSave = new GUIButton("Save", BtnSave);
                _btnSave.AnchorToInnerSide(_gWin, SideEnum.BottomRight);
            }

            private void BtnSave()
            {
                string tags = _gDetails.GetText();

                Dictionary<string, string> dss = DataManager.TaggedStringToDictionary(tags);

                ItemEnum eType = Util.ParseEnum<ItemEnum>(dss["Type"]);
                int index = _diItems[eType].Count;

                ItemXMLData data = new ItemXMLData(index, dss, ITEM_TAGS);
                data.SetTextData(_gName.GetText(), _gDescription.GetText());
                _diItems[eType].Add(data);

                GUIManager.CloseMainObject();
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                rv = _btnSave.ProcessLeftButtonClick(mouse);
                if (_gName.Contains(mouse))
                {
                    SetSelection(_gName);
                }
                else if (_gDescription.Contains(mouse))
                {
                    SetSelection(_gDescription);
                }
                else if (_gDetails.Contains(mouse))
                {
                    SetSelection(_gDetails);
                }
                else
                {
                    SetSelection(null);
                }

                return rv;
            }

            private void SetSelection(GUITextInputWindow g)
            {
                if (g == _gName)
                {
                    _gName.TakeInput = true;
                    _gDescription.TakeInput = false;
                    _gDescription.HideCursor();
                    _gDetails.TakeInput = false;
                    _gDetails.HideCursor();
                }
                else if (g == _gDescription)
                {
                    _gDescription.TakeInput = true;
                    _gName.TakeInput = false;
                    _gName.HideCursor();
                    _gDetails.TakeInput = false;
                    _gDetails.HideCursor();
                }
                else if (g == _gDetails)
                {
                    _gDetails.TakeInput = true;
                    _gName.TakeInput = false;
                    _gName.HideCursor();
                    _gDescription.TakeInput = false;
                    _gDescription.HideCursor();
                }
                else
                {
                    _gDescription.TakeInput = false;
                    _gDescription.HideCursor();
                    _gDetails.TakeInput = false;
                    _gDetails.HideCursor();
                    _gDetails.TakeInput = false;
                    _gDetails.HideCursor();
                }
            }
        }

        class ItemDataButton : GUIButton
        {
            public new delegate void BtnClickDelegate(ItemEnum e);
            private new BtnClickDelegate _delAction;

            public ItemDataButton(string text, BtnClickDelegate del = null) : base(text)
            {
                _delAction = del;
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse) && Enabled && _delAction != null)
                {
                    _delAction(Util.ParseEnum<ItemEnum>(_gText.Text));
                    rv = true;
                }

                return rv;
            }
        }

        class ItemBox : GUIImage
        {
            GUIImage _gItem;
            ItemXMLData _itemData;
            public ItemXMLData ItemData => _itemData;
            int _iIndex;
            public ItemBox(int index) : base(RECT_IMG, GameManager.ScaleIt(RECT_IMG.Width), GameManager.ScaleIt(RECT_IMG.Height), @"Textures\Dialog")
            {
                _iIndex = index;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                _gItem?.Draw(spriteBatch);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    rv = true;
                    if (_heldData == null)
                    {
                        _heldData = _itemData;

                        if (_itemData != null)
                        {
                            SetData(null);
                            int index = _diItems[_heldData.ItemType].IndexOf(_heldData);
                            _diItems[_heldData.ItemType][index] = null;
                            _btnSave.Enable(false);
                        }
                    }
                    else
                    {
                        ItemXMLData temp = _itemData;
                        SetData(_heldData);

                        if(temp != null) {
                            int index = _diItems[temp.ItemType].IndexOf(temp);
                            _diItems[temp.ItemType][index] = null;
                        }

                        while (_diItems[_heldData.ItemType].Count <= _iIndex)
                        {
                            _diItems[_heldData.ItemType].Add(null);
                        }

                        //if (_iIndex > _diItems[_heldData.ItemType].Count) { _diItems[_heldData.ItemType].Add(_heldData); }
                        //else {
                            
                            if (_diItems[_heldData.ItemType][_iIndex] == null) { _diItems[_heldData.ItemType][_iIndex] = _heldData; }
                            else { _diItems[_heldData.ItemType].Insert(_iIndex, _heldData); }
                        //}

                        _heldData = temp;

                        if(_heldData== null){ _btnSave.Enable(true); }
                        
                    }
                }

                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    if (_itemData != null)
                    {
                        Vector2 mouseLoc = new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32);
                        GUITextWindow win = new GUITextWindow(mouseLoc, _itemData.Name + " - " + _itemData.Description);

                        if(mouseLoc.X + win.Width >= RiverHollow.ScreenWidth)
                        {
                            win.PositionSub(new Vector2(win.Width, 0));
                        }
                        GUIManager.OpenHoverWindow(win, this);
                    }
                    rv = true;
                }
                return rv;
            }

            public void SetData(ItemXMLData it)
            {
                _itemData = it;

                if (_itemData != null)
                {
                    _gItem = new GUIImage(_itemData.SourceRectangle, GameManager.ScaledTileSize, GameManager.ScaledTileSize, _itemData.Texture);
                    _gItem.CenterOnObject(this);
                    AddControl(_gItem);
                }
                else
                {
                    RemoveControl(_gItem);
                    _gItem = null;
                }
            }
        }
    }

    public class XMLData
    {
        protected int _iID;
        public int ID => _iID;
        protected string[] _arrRelevantTags;
        protected List<XMLData> _liLinkedItems;
        protected List<TMXData> _liLinkedMaps;
        protected Dictionary<string, string> _diTags;

        public XMLData(int id, Dictionary<string, string> stringData, string tags) {
            _liLinkedMaps = new List<TMXData>();
            _liLinkedItems = new List<XMLData>();
            _arrRelevantTags = tags.Split(',');

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
                rv += "[" + kvp.Key + ":" + kvp.Value + "]";
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
        public virtual void ChangeID(int newID)
        {
            if (_iID != newID)
            {
                int oldID = _iID;
                _iID = newID;

                foreach (XMLData d in _liLinkedItems)
                {
                    d.ReplaceLinkedIDs(oldID, _iID);
                }

                foreach(TMXData d in _liLinkedMaps)
                {
                    d.ReplaceID(ItemDataScreen.MAP_WORLD_OBJECTS_TAG, oldID, newID);
                }
            }
        }

        /// <summary>
        /// Checks the tags to see if there are any references to the given ID
        /// among the relevant tags.
        /// </summary>
        /// <param name="id">The ID to look for</param>
        /// <returns>True if there is at least one match</returns>
        public bool RefersToID(int id)
        {
            bool rv = false;

            foreach(string s in _arrRelevantTags){
                CheckTagForID(s, id, ref rv);
            }

            return rv;
        }

        /// <summary>
        /// Iterates through the relevant tags to replace any instances of the 
        /// old ID with the new ID.
        /// </summary>
        /// <param name="oldID">The old ID that has now changed</param>
        /// <param name="newID">The new ID to reference</param>
        public void ReplaceLinkedIDs(int oldID, int newID)
        {
            foreach (string s in _arrRelevantTags)
            {
                ReplaceID(s, oldID, newID);
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
        /// Call this to check the given tag for the given ID. This method is to
        /// be used for any entry that has multiples of the same type of thing in it
        /// that are delineated by a '|'. For example, a tag for the multiple things a Machine
        /// can make
        /// </summary>
        /// <param name="tag">Tag to look at</param>
        /// <param name="id">The ID to look for</param>
        /// <param name="val">Reference to the success or this and other checks</param>
        /// <returns>True if a match exists</returns>
        public void CheckTagForID(string tag, int id, ref bool val)
        {
            //If we don't have the key, don't proceed
            if (_diTags.ContainsKey(tag))
            {
                //Isolate every group of entries that aredelineated by the '|'
                string[] split = _diTags[tag].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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
        /// Call this to check the given tag for the given ID. This method is to
        /// be used for any entry that has multiples of the same type of thing in it
        /// that are delineated by a '|'.
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
                string[] split = _diTags[tag].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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
                        splitData[0] = ItemDataScreen.SPECIAL + newID.ToString() + ItemDataScreen.SPECIAL;
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
        /// Iterates through each tag and remove all instances of the special character
        /// </summary>
        public void StripSpecialCharacter()
        {
            foreach(string s in new List<string>(_diTags.Keys))
            {
                if (_diTags[s].Contains(ItemDataScreen.SPECIAL))
                {
                    string val = _diTags[s];
                    _diTags[s] = val.Replace(ItemDataScreen.SPECIAL, "");
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
        Vector2 _vSourcePos;
        ItemEnum _eType;
        public ItemEnum ItemType => _eType;

        Texture2D _texTexture;
        public Texture2D Texture => _texTexture;
        public Rectangle SourceRectangle => new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, GameManager.TileSize, GameManager.TileSize);

        public ItemXMLData(int id, Dictionary<string, string> stringData, string value) : base (id, stringData, value)
        {
            _eType = Util.ParseEnum<ItemEnum>(_diTags["Type"]);

            string[] texIndices = stringData["Image"].Split('-');
            _vSourcePos = new Vector2(int.Parse(texIndices[0]), int.Parse(texIndices[1]));

            //Determine which textrue to draw the item from based off the Item type
            switch (_eType)
            {
                case ItemEnum.Special:
                    _texTexture = DataManager.GetTexture(@"Textures\items");
                    break;
                case ItemEnum.Clothes:
                    _texTexture = DataManager.GetTexture(@"Textures\items");
                    break;
                case ItemEnum.Tool:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Tools");
                    break;
                case ItemEnum.Food:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Food");
                    break;
                case ItemEnum.Consumable:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
                    break;
                case ItemEnum.MonsterFood:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
                    break;
                case ItemEnum.StaticItem:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "StaticObjects");
                    break;
                case ItemEnum.Resource:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");
                    break;
                case ItemEnum.Equipment:
                    EquipmentEnum type = Util.ParseEnum<EquipmentEnum>(stringData["EType"]);
                    if (type.Equals(EquipmentEnum.Armor)) { _texTexture = DataManager.GetTexture(@"Textures\Items\armor"); }
                    else if (type.Equals(EquipmentEnum.Weapon)) { _texTexture = DataManager.GetTexture(@"Textures\Items\weapons"); }
                    else if (type.Equals(EquipmentEnum.Accessory)) { _texTexture = DataManager.GetTexture(@"Textures\Items\Accessories"); }
                    break;
            }

            DataManager.GetItemText(_iID, ref _sName, ref _sDescription);
        }

        public void SetTextData(string name, string desc)
        {
            _sName = name;
            _sDescription = desc;
        }

        public override void ChangeID(int newID)
        {
            if (_iID != newID)
            {
                int oldID = _iID;
                _iID = newID;

                foreach (XMLData d in _liLinkedItems)
                {
                    d.ReplaceLinkedIDs(oldID, _iID);
                }

                foreach (TMXData d in _liLinkedMaps)
                {
                    d.ReplaceID(ItemDataScreen.MAP_ITEM_TAGS, oldID, newID);
                }
            }
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
                if(s.Contains("<property name"))
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
                            string[] splitValues = propertyValue.Split('|');
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

                    //Iterate over the property parameters and collect the name of the property and its value
                    for (int j = 0; j < propertyParams.Length; j++)
                    {
                        if (propertyParams[j].Contains("="))
                        {
                            string[] splitParam = propertyParams[j].Split('=');
                            string pName = splitParam[0].Replace("\"", "");
                            string pValue = splitParam[1].Replace("\"", "").Replace("/", "").Replace(">", "");  //Entry willl ook like "\"val\"/>" so we need to strip the special characters

                            if (pName.Equals("name")) { propertyName = pValue; }
                            else if (pName.Equals("value")) {
                                valueIndex = j;             //Record the index of when we found it
                                propertyValue = pValue;
                            }
                        }
                    }

                    bool found = false;

                    //We're going to loop through every tag we've been told to search for
                    string[] tagArray = tags.Split(',');
                    foreach (string tag in tagArray)
                    {
                        if (propertyName.Equals(tag))
                        {
                            //Split the values in the property value by the '|' delimeter 
                            string[] splitValues = propertyValue.Split('|');
                            for (int j = 0; j < splitValues.Length; j++)
                            {
                                //If we found a match, set the flag to true and overwrite the value of this string
                                if (splitValues[j] == oldID.ToString())
                                {
                                    found = true;
                                    splitValues[j] = ItemDataScreen.SPECIAL + newID.ToString() + ItemDataScreen.SPECIAL;
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
                        if (j == valueIndex && found) { _liAllLines[i] += newValue + "/>";  }
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
                    else if (pName.Equals("value")) {
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
                if (s.Contains(ItemDataScreen.SPECIAL))
                {
                    string val = s;
                    _liAllLines[i] = val.Replace(ItemDataScreen.SPECIAL, "");
                }
            }
        }
    }
}