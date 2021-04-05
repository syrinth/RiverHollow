using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens
{
    class BuildScreen : GUIScreen
    {
        const int BTN_PADDING = 10;

        //public static int BTNSIZE = ScaledTileSize;
        public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
        public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)
        List<GUIObject> _liButtons;
        GUIMainObject _gMenuObject;

        public delegate void CloseMenuDelegate();

        public BuildScreen()
        {
            _liButtons = new List<GUIObject>() {
                new GUIButton("Build", BtnBuildings),
                new GUIButton("Structures", BtnStructures),
                new GUIButton("Flooring", BtnFlooring),
                new GUIButton("Move", BtnMove),
                new GUIButton("Exit", BtnExitMenu)
            };
            AddControls(_liButtons);

            GUIObject.CreateSpacedColumn(ref _liButtons, GUIButton.BTN_WIDTH/2, 0, RiverHollow.ScreenHeight, BTN_PADDING);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = base.ProcessRightButtonClick(mouse);

            //If the right click has not been processed, we probably want to close anything that we have open.
            if (!rv)
            {
                //ToDo: Need to handle right-clicking while building which should go back to menu
                if (false)
                {
                    GUIManager.OpenMenu();
                }
                else
                {
                    if (_gMainObject == null)
                    {
                        CloseMenu();
                    }

                    GUIManager.CloseMainObject();
                    GameManager.GoToHUDScreen();
                    GUIManager.OpenMenu();
                }
            }

            return rv;
        }

        public void BtnBuildings()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, ObjectTypeEnum.Building);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnStructures()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, ObjectTypeEnum.WorldObject);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnFlooring()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, ObjectTypeEnum.Floor);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnMove()
        {
            CloseMenu();
            GameManager.EnterBuildMode();
            GameManager.ClearGMObjects();
            GameManager.MoveBuilding();
        }

        public void BtnExitMenu()
        {
            RiverHollow.ResetCamera();
            GUIManager.SetScreen(new HUDScreen());
            GameManager.Unpause();
        }

        public void BuildConstruct(BuildInfo obj)
        {
            bool create = true;
            //create = PlayerManager.Money >= _liMerchandise[_iCurrIndex].MoneyCost;
            // if (create)
            {
                foreach (KeyValuePair<int, int> kvp in obj.RequiredToMake)
                {
                    if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
                    {
                        create = false;
                    }
                }
            }

            if (create)
            {
                GameManager.EnterBuildMode();
                GameManager.PickUpWorldObject(DataManager.GetBuilding(obj.ID));
            }

            GUIManager.CloseMainObject();
        }

        public override void CloseMenu()
        {
            RemoveControls(_liButtons);
        }
    
        public override void OpenMenu()
        {
            AddControls(_liButtons);
        }

        public class HUDConstruction : GUIMainObject
        {
            public static int MAX_SHOWN_TASKS = 4;
            public static int TASK_SPACING = 20;
            public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
            public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)
            List<GUIObject> _liStructures;
            GUIList _gList;
            ObjectTypeEnum _eObjectBuildType;

            private CloseMenuDelegate _closeMenu;

            public HUDConstruction(CloseMenuDelegate closeMenu, ObjectTypeEnum objType)
            {
                _eObjectBuildType = objType;
                _closeMenu = closeMenu;
                _winMain = SetMainWindow();

                _liStructures = new List<GUIObject>();

                if (objType == ObjectTypeEnum.Building)
                {
                    foreach (BuildInfo b in GameManager.DIBuildInfo.Values)
                    {
                        if (b.Unlocked && !b.Built)
                        {
                            ConstructBox box = new ConstructBox(CONSTRUCTBOX_WIDTH, CONSTRUCTBOX_HEIGHT, objType, ConstructWorldObject);
                            box.SetConstructionInfo(b.ID);
                            _liStructures.Add(box);
                        }
                    }
                }
                else if (objType == ObjectTypeEnum.Floor)
                {
                    foreach(int i in DataManager.FloorIDs)
                    {
                        ConstructBox box = new ConstructBox(CONSTRUCTBOX_WIDTH, CONSTRUCTBOX_HEIGHT, objType, ConstructWorldObject);
                        box.SetConstructionInfo(i);
                        _liStructures.Add(box);
                    }
                }
                else if (objType == ObjectTypeEnum.WorldObject)
                {
                    foreach (int i in DataManager.StructureIDs)
                    {
                        ConstructBox box = new ConstructBox(CONSTRUCTBOX_WIDTH, CONSTRUCTBOX_HEIGHT, objType, ConstructWorldObject);
                        box.SetConstructionInfo(i);
                        _liStructures.Add(box);
                    }
                }

                _gList = new GUIList(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING/*, _gWindow.Height*/);
                _gList.CenterOnObject(_winMain);

                AddControl(_gList);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (GUIObject c in Controls)
                {
                    rv = c.ProcessLeftButtonClick(mouse);

                    if (rv) { break; }
                }

                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;
                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;
                return rv;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
            }

            public void ConstructWorldObject(int objID)
            {
                WorldObject obj;
                string name = string.Empty;
                Dictionary<int, int> requiredToMake;
                if (_eObjectBuildType == ObjectTypeEnum.Building)
                {
                    obj = DataManager.GetBuilding(objID);
                    requiredToMake = GameManager.DIBuildInfo[objID].RequiredToMake;
                    name = GameManager.DIBuildInfo[objID].Name;
                }
                else
                {
                    obj = DataManager.GetWorldObjectByID(objID);
                    requiredToMake = obj.RequiredToMake;
                    name = obj.Name;
                }

                bool create = true;
                //create = PlayerManager.Money >= _liMerchandise[_iCurrIndex].MoneyCost;
                // if (create)
                {
                    foreach (KeyValuePair<int, int> kvp in requiredToMake)
                    {
                        if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
                        {
                            create = false;
                        }
                    }
                }

                if (create)
                {
                    GameManager.EnterBuildMode();
                    GameManager.PickUpWorldObject(obj);
                }

                GUIManager.CloseMainObject();
                _closeMenu();
            }

            public class ConstructBox : GUIObject
            {
                ObjectTypeEnum _eObjectBuildType;
                GUIWindow _window;
                GUIText _gName;
                public int _iBuildID;
                public delegate void ConstructObject(int objID);
                private ConstructObject _delAction;

                public ConstructBox(int width, int height, ObjectTypeEnum type, ConstructObject del)
                {
                    _delAction = del;
                    _eObjectBuildType = type;

                    int boxHeight = height;
                    int boxWidth = width;
                    _window = new GUIWindow(GUIWindow.Window_1, boxWidth, boxHeight);
                    AddControl(_window);
                    Width = _window.Width;
                    Height = _window.Height;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (Show())
                    {
                        _window.Draw(spriteBatch);
                    }
                }
                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (Contains(mouse) && _delAction != null)
                    {
                        rv = true;
                        _delAction(_iBuildID);
                    }

                    return rv;
                }
                public override bool ProcessRightButtonClick(Point mouse)
                {
                    return base.ProcessRightButtonClick(mouse);
                }
                public override bool ProcessHover(Point mouse)
                {
                    bool rv = false;
                    return rv;
                }
                public override bool Contains(Point mouse)
                {
                    return _window.Contains(mouse);
                }

                public void SetConstructionInfo(int id)
                {
                    _iBuildID = id;

                    string name = string.Empty;
                    Dictionary<int, int> requiredToMake;
                    if (_eObjectBuildType == ObjectTypeEnum.Building) {
                        requiredToMake = GameManager.DIBuildInfo[id].RequiredToMake;
                        name = GameManager.DIBuildInfo[id].Name;
                    }
                    else {
                        WorldObject obj = DataManager.GetWorldObjectByID(id);
                        requiredToMake = obj.RequiredToMake;
                        name = obj.Name;
                    }

                    Color textColor = Color.White;
                    if (!InventoryManager.SufficientItems(requiredToMake))
                    {
                        textColor = Color.Red;
                        _delAction = null;
                    }

                    _gName = new GUIText(name);
                    _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
                    _gName.SetColor(textColor);

                    List<GUIItemBox> list = new List<GUIItemBox>();
                    foreach (KeyValuePair<int, int> kvp in requiredToMake)
                    {
                        GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));

                        if (list.Count == 0) { box.AnchorToInnerSide(_window, SideEnum.BottomRight); }
                        else { box.AnchorAndAlignToObject(list[list.Count - 1], SideEnum.Left, SideEnum.Bottom); }

                        list.Add(box);
                    }
                }
            }
        }
    }
}
