using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.Screens
{
    class BuildScreen : GUIScreen
    {
        const int BTN_PADDING = 10;
        GUIButton _btnBuild;
        GUIButton _btnFlooring;
        GUIButton _btnMove;
        GUIButton _btnExitBuild;

        //public static int BTNSIZE = ScaledTileSize;
        public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
        public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)
        List<GUIObject> _liButtons;
        GUIMainObject _gMenuObject;

        public delegate void CloseMenuDelegate();

        public BuildScreen()
        {
            _liButtons = new List<GUIObject>();

            _btnBuild = new GUIButton("Build", BtnBuild);
            AddControl(_btnBuild);

            _btnFlooring = new GUIButton("Flooring", BtnFlooring);
            AddControl(_btnFlooring);

            _btnMove = new GUIButton("Move", BtnMove);
            AddControl(_btnMove);

            _btnExitBuild = new GUIButton("Exit", BtnExitMenu);
            AddControl(_btnExitBuild);

            _liButtons = new List<GUIObject>() { _btnBuild, _btnFlooring, _btnMove, _btnExitBuild };
            GUIObject.CreateSpacedColumn(ref _liButtons, GUIButton.BTN_WIDTH/2, 0, RiverHollow.ScreenHeight, BTN_PADDING);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            //Returns false here because we don't handle it
            //By returning false, we will start closing options
            return false;
        }

        public void BtnBuild()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, HUDConstruction.ConstructionEnum.Building);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnFlooring()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, HUDConstruction.ConstructionEnum.Flooring);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnMove()
        {
            CloseMenu();
            RiverHollow.EnterBuildMode();
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
                RiverHollow.EnterBuildMode();
                GameManager.PickUpBuilding(DataManager.GetBuilding(obj.ID));
            }

            GUIManager.CloseMainObject();
        }

        public override void CloseMenu()
        {
            foreach (GUIObject obj in _liButtons)
            {
                RemoveControl(obj);
            }
        }

        public override void OpenMenu()
        {
            foreach (GUIObject obj in _liButtons)
            {
                AddControl(obj);
            }
        }

        public class HUDConstruction : GUIMainObject
        {
            public enum ConstructionEnum { Building, Flooring}
            private ConstructionEnum _eConstructionType;

            public static int MAX_SHOWN_TASKS = 4;
            public static int TASK_SPACING = 20;
            public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
            public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)
            List<GUIObject> _liStructures;
            GUIList _gList;

            private CloseMenuDelegate _closeMenu;

            public HUDConstruction(CloseMenuDelegate closeMenu, ConstructionEnum type)
            {
                _eConstructionType = type;
                _closeMenu = closeMenu;
                _winMain = SetMainWindow();

                _liStructures = new List<GUIObject>();

                if (_eConstructionType == ConstructionEnum.Building)
                {
                    foreach (BuildInfo b in GameManager.DIBuildInfo.Values)
                    {
                        if (b.Unlocked && !b.Built)
                        {
                            ConstructBox box = new ConstructBox(CONSTRUCTBOX_WIDTH, CONSTRUCTBOX_HEIGHT, _eConstructionType, ConstructBuilding);
                            box.SetConstructionInfo(b);
                            _liStructures.Add(box);
                        }
                    }
                }
                else if (_eConstructionType == ConstructionEnum.Flooring)
                {
                    foreach(int i in DataManager.FloorIDs)
                    {
                        ConstructBox box = new ConstructBox(CONSTRUCTBOX_WIDTH, CONSTRUCTBOX_HEIGHT, _eConstructionType, ConstructBuilding);
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

            public void ConstructBuilding(BuildInfo obj)
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
                    RiverHollow.EnterBuildMode();
                    GameManager.PickUpBuilding(DataManager.GetBuilding(obj.ID));
                }

                GUIManager.CloseMainObject();
                _closeMenu();
            }

            public void ConstructWorldObject(WorldObject obj)
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
                    RiverHollow.EnterBuildMode();
                    GameManager.PickUpBuilding(DataManager.GetBuilding(obj.ID));
                }

                GUIManager.CloseMainObject();
                _closeMenu();
            }

            public class ConstructBox : GUIObject
            {
                GUIWindow _window;
                GUIText _gName;
                public BuildInfo BuildingInfo { get; private set; }
                public WorldObject BuildObj;
                public delegate void ConstructBuilding(BuildInfo obj);
                public delegate void ConstructObject(WorldObject obj);
                private ConstructBuilding _delAction;
                private ConstructObject _delActionObject;

                public ConstructBox(int width, int height, HUDConstruction.ConstructionEnum type, ConstructBuilding del)
                {
                    _delAction = del;

                    int boxHeight = height;
                    int boxWidth = width;
                    _window = new GUIWindow(GUIWindow.Window_1, boxWidth, boxHeight);
                    AddControl(_window);
                    Width = _window.Width;
                    Height = _window.Height;
                    BuildingInfo = null;
                }

                public ConstructBox(int width, int height, HUDConstruction.ConstructionEnum type, ConstructObject del)
                {
                    _delActionObject = del;

                    int boxHeight = height;
                    int boxWidth = width;
                    _window = new GUIWindow(GUIWindow.Window_1, boxWidth, boxHeight);
                    AddControl(_window);
                    Width = _window.Width;
                    Height = _window.Height;
                    BuildingInfo = null;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if ((BuildingInfo != null  || BuildObj  != null) && Show())
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
                        _delAction(BuildingInfo);
                    }
                    else if (Contains(mouse) && _delActionObject != null)
                    {
                        rv = true;
                        _delActionObject(BuildObj);
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

                public void SetConstructionInfo(BuildInfo b)
                {
                    BuildingInfo = b;

                    Color textColor = Color.White;
                    if (!InventoryManager.SufficientItems(BuildingInfo.RequiredToMake))
                    {
                        textColor = Color.Red;
                        _delAction = null;
                    }

                    _gName = new GUIText(BuildingInfo.Name);
                    _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
                    _gName.SetColor(textColor);

                    List<GUIItemBox> list = new List<GUIItemBox>();
                    foreach (KeyValuePair<int, int> kvp in BuildingInfo.RequiredToMake)
                    {
                        GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));

                        if (list.Count == 0) { box.AnchorToInnerSide(_window, SideEnum.BottomRight); }
                        else { box.AnchorAndAlignToObject(list[list.Count - 1], SideEnum.Left, SideEnum.Bottom); }

                        list.Add(box);
                    }
                }

                public void SetConstructionInfo(int id)
                {
                    BuildObj = DataManager.GetWorldObject(id);

                    Color textColor = Color.White;
                    if (!InventoryManager.SufficientItems(BuildObj.RequiredToMake))
                    {
                        textColor = Color.Red;
                        _delAction = null;
                    }

                    _gName = new GUIText(BuildObj.Name);
                    _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
                    _gName.SetColor(textColor);

                    List<GUIItemBox> list = new List<GUIItemBox>();
                    foreach (KeyValuePair<int, int> kvp in BuildObj.RequiredToMake)
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
