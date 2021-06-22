using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.Screens
{
    class DataScreen : GUIScreen
    {
        GUIObject _gObject;
        public DataScreen()
        {
            _gObject = new Menu(RemoveControl, AddControl);
            AddControl(_gObject);
        }
    }

    class TransitionalObject : GUIObject
    {
        public delegate void ActionDelegate(GUIObject o);
        protected ActionDelegate _delRemove;
        protected ActionDelegate _delAdd;

        public TransitionalObject(ActionDelegate remove, ActionDelegate add)
        {
            _delRemove = remove;
            _delAdd = add;
        }
    }

    class Menu : TransitionalObject
    {
        const int BTN_PADDING = 50;

        GUIButton _btnSpawn;
        GUIButton _btnExit;

        public Menu(ActionDelegate remove, ActionDelegate add) : base(remove, add)
        {
            _btnSpawn = new GUIButton("Spawn Point", ChangeSpawn);
            _btnExit = new GUIButton("Main Menu", Exit);

            List<GUIObject> listButtons = new List<GUIObject>() { _btnSpawn, _btnExit };
            GUIObject.CreateSpacedColumn(ref listButtons, RiverHollow.ScreenWidth / 2, 0, RiverHollow.ScreenHeight, BTN_PADDING);

            AddControls(listButtons);
        }

        public void ChangeSpawn()
        {
            _delRemove(this);
            _delAdd(new SpawnControls(_delRemove, _delAdd));
        }

        public void Exit()
        {
            GUIManager.SetScreen(new IntroMenuScreen());
        }
    }

    class SpawnControls : TransitionalObject
    {
        struct SpawnData
        {
            public string SpawnMap;
            public int X;
            public int Y;
            public SpawnData(string map, int x, int y)
            {
                SpawnMap = map;
                X = x;
                Y = y;
            }
        }
        static string PATH_TO_DATA = string.Format(@"{0}\..\..\..\..\Content\Data", System.Environment.CurrentDirectory);

        int _iSpawnMapIndex;
        List<SpawnXMLData> _liConfigData;
        List<GUIObject> _liButtons;
        SpawnData targetSpawnData;
        GUIButton _btnAddNew;

        public SpawnControls(ActionDelegate remove, ActionDelegate add) : base(remove, add)
        {
            bool found = false;
            _iSpawnMapIndex = 0;
            _liConfigData = new List<SpawnXMLData>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.Config)
            {
                _liConfigData.Add(new SpawnXMLData(kvp.Key, kvp.Value));

                if (!found && !kvp.Value.ContainsKey("SpawnMap")) { _iSpawnMapIndex++; }
                else { found = true; }
            }

            string[] maps = Util.FindParams(DataManager.Config[7]["Maps"]);

            _liButtons = new List<GUIObject>();

            foreach (string m in maps)
            {
                AddButton(m);
            }

            EnableButtons();

            _btnAddNew = new GUIButton("New Point", OpenSpawnEditor);
            _btnAddNew.AnchorToScreen(SideEnum.BottomRight);
            AddControl(_btnAddNew);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            _delRemove(this);
            _delAdd(new Menu(_delRemove, _delAdd));

            _liConfigData[_iSpawnMapIndex].SetTagInfo("SpawnMap", targetSpawnData.SpawnMap+ "-" + targetSpawnData.X + "-" + targetSpawnData.Y);

            SaveManager.SaveXMLData(_liConfigData, PATH_TO_DATA + @"\Config.xml");

            GUIManager.CloseMainObject();

            return true;
        }

        /// <summary>
        /// Loops through the buttons to disable the button that matches with the current spawn map
        /// </summary>
        public void EnableButtons()
        {
            foreach(GUISpawnButton b in _liButtons)
            {
                bool match = MapManager.SpawnMap == b.MapName;
                b.Enable(!match);

                if (match)
                {
                    targetSpawnData = new SpawnData(b.MapName, b.TileX, b.TileY);
                }
            }
        }

        public void OpenSpawnEditor()
        {
            GUIManager.OpenMainObject(new SpawnEditor(AddNew));
        }

        public void AddNew(string value)
        {
            AddButton(value);
            _liConfigData[7].AppendToTag("Maps", value);
        }

        /// <summary>
        /// Splits the given string into subcomponents to determine what information to
        /// give to the new SpawnButton.
        /// 
        /// Then adds the button to the proper location on the screen
        /// </summary>
        /// <param name="m">Spawn Information</param>
        private void AddButton(string m)
        {
            string[] splitData = m.Split('-');
            GUISpawnButton b = new GUISpawnButton(splitData[0], splitData[1], splitData[2], splitData[3], EnableButtons);
            _liButtons.Add(b);

            int index = _liButtons.Count - 1;
            if (_liButtons.Count == 1) { b.AnchorToScreen(SideEnum.TopLeft, 10); }
            else if ((float)_liButtons.Count % 6f == 0) { b.AnchorAndAlignToObject(_liButtons[index - 5], SideEnum.Right, SideEnum.Bottom, 10); }
            else { b.AnchorAndAlignToObject(_liButtons[index - 1], SideEnum.Bottom, SideEnum.Left, 10); }

            AddControlDelayed(b);
        }

        class GUISpawnButton : GUIButton
        {
            string _sMapName;
            public string MapName => _sMapName;

            int _iX;
            public int TileX => _iX;
            int _iY;
            public int TileY => _iY;

            public GUISpawnButton(string name, string map, string x, string y, BtnClickDelegate del) : base (name, del)
            {
                _sMapName = map;
                _iX = int.Parse(x);
                _iY = int.Parse(y);
            }

            /// <summary>
            /// When pressed, set the game spawn information
            /// </summary>
            /// <param name="mouse"></param>
            /// <returns></returns>
            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    MapManager.SetSpawnMap(_sMapName, _iX, _iY);
                    _delAction();
                }

                return rv;
            }
        }

        class SpawnEditor : GUIMainObject
        {
            GUITextInputWindow _gName;
            GUITextInputWindow _gMap;
            GUITextInputWindow _gX;
            GUITextInputWindow _gY;
            GUIButton _btnSave;

            public delegate void SpawnSaveDelegate(string value);
            protected SpawnSaveDelegate _delAction;

            public SpawnEditor(SpawnSaveDelegate action)
            {
                _winMain = SetMainWindow();

                _delAction = action;

                _gName = new GUITextInputWindow("Name:", SideEnum.Left, 10);
                _gName.AnchorToInnerSide(_winMain, SideEnum.TopLeft);
                _gMap = new GUITextInputWindow("MapName:", SideEnum.Left, 60);
                _gMap.AllowAll = true;
                _gMap.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);
                _gX = new GUITextInputWindow("X:", SideEnum.Left, 3);
                _gX.AllowAll = true;
                _gX.AnchorAndAlignToObject(_gMap, SideEnum.Bottom, SideEnum.Left);
                _gY = new GUITextInputWindow("Y:", SideEnum.Left, 3);
                _gY.AllowAll = true;
                _gY.AnchorAndAlignToObject(_gX, SideEnum.Bottom, SideEnum.Left);
                _gName.AllowAll = true;

                _btnSave = new GUIButton("Save", BtnSave);
                _btnSave.AnchorToInnerSide(_winMain, SideEnum.BottomRight);
            }

            private void BtnSave()
            {
                string value = _gName.GetText() + "-" + _gMap.GetText() + "-" + _gX.GetText() + "-" + _gY.GetText();

                _delAction(value);

                GUIManager.CloseMainObject();
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                rv = _btnSave.ProcessLeftButtonClick(mouse);
                if (_gName.Contains(mouse)) { SetSelection(_gName); }
                else if (_gMap.Contains(mouse)) { SetSelection(_gMap); }
                else if (_gX.Contains(mouse)) { SetSelection(_gX); }
                else if (_gY.Contains(mouse)) { SetSelection(_gY); }
                else { SetSelection(null); }

                return rv;
            }

            private void SetSelection(GUITextInputWindow g)
            {
                _gName.Activate(g == _gName);
                _gMap.Activate(g == _gMap);
                _gX.Activate(g == _gX);
                _gY.Activate(g == _gY);
            }
        }
    }

    public class SpawnXMLData
    {
        protected int _iID;
        public int ID => _iID;
        protected Dictionary<string, string> _diTags;

        public SpawnXMLData(int id, Dictionary<string, string> stringData)
        {
            _iID = id;
            _diTags = stringData;
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

        public void SetTagInfo(string key, string value)
        {
            _diTags[key] = value;
        }
        public void AppendToTag(string key, string value)
        {
            _diTags[key] += "|" + value;
        }
    }
}