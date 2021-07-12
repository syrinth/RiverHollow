﻿using Microsoft.Xna.Framework;
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
        public enum MenuEnum { BuildMenu, EditMenu };
        public enum BuildTypeEnum { Building, Floor, WorldObject, Storage };
        const int BTN_PADDING = 10;

        private MenuEnum _eCurrentMenu;

        //public static int BTNSIZE = ScaledTileSize;
        public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
        public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)

        GUIButton storageButton;
        List<GUIObject> _liBuildMenuObjects;
        List<GUIObject> _liEditMenuObjects;
        GUIMainObject _gMenuObject;

        public delegate void CloseMenuDelegate();

        public BuildScreen()
        {
            _eCurrentMenu = MenuEnum.BuildMenu;
            Scry(true);
            GUIButton buildingsButton = new GUIButton("Buildings", BtnBuildings);
            storageButton = new GUIButton("Storage", BtnStorage);
            storageButton.Enable(PlayerManager.GetStorageItems().Count > 0);

            _liBuildMenuObjects = new List<GUIObject>();
            if (MapManager.CurrentMap.IsOutside && MapManager.CurrentMap.IsTown) {
                _liBuildMenuObjects.Add(buildingsButton);
            }

            _liBuildMenuObjects.Add(new GUIButton("Structures", BtnStructures));
            _liBuildMenuObjects.Add(new GUIButton("Flooring", BtnFlooring));
            _liBuildMenuObjects.Add(storageButton);
            _liBuildMenuObjects.Add(new GUIButton("Edit Town", BtnEditTown));
            _liBuildMenuObjects.Add(new GUIButton("Exit", BtnExitBuildMenu));

            AddControls(_liBuildMenuObjects);
            GUIObject.CreateSpacedColumn(ref _liBuildMenuObjects, GUIButton.BTN_WIDTH/2, 0, RiverHollow.ScreenHeight, BTN_PADDING);

            _liEditMenuObjects = new List<GUIObject>
            {
                new GUIButton("Move", BtnMove),
                new GUIButton("Rotate", BtnRotate),
                new GUIButton("Upgrade", BtnUpgrade),
                new GUIButton("Remove", BtnRemove),
                new GUIButton("Storage", BtnStorageMode),
                new GUIButton("Exit", BtnLeaveEditMode)
            };
            GUIObject.CreateSpacedColumn(ref _liEditMenuObjects, GUIButton.BTN_WIDTH / 2, 0, RiverHollow.ScreenHeight, BTN_PADDING);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = base.ProcessRightButtonClick(mouse);

            //If we're holding something, and we're moving an object. Do not close anything
            if (GameManager.HeldObject != null && GameManager.TownModeMoving()) { rv = true; }

            //If the right click has not been processed, we probably want to close anything that we have open.
            if (!rv)
            {
                if (TownModeUpgrade()) {
                    if (_gMainObject != null) { GUIManager.CloseMainObject(); }
                    else {
                        LeaveTownMode();
                        GUIManager.OpenMenu();
                        GameManager.DropWorldObject();
                    }
                }
                else if (InTownMode())
                {
                    LeaveTownMode();
                    GUIManager.OpenMenu();
                    GameManager.DropWorldObject();
                }
                else
                {
                    switch (_eCurrentMenu)
                    {
                        case MenuEnum.BuildMenu:
                            if (_gMainObject != null) { GUIManager.CloseMainObject(); }
                            else
                            {
                                CloseMenu();

                                Scry(false);

                                GUIManager.CloseMainObject();
                                GameManager.GoToHUDScreen();
                                GUIManager.OpenMenu();
                            }
                            break;
                        case MenuEnum.EditMenu:
                            if (_gMainObject != null) { GUIManager.CloseMainObject(); }
                            else { BtnLeaveEditMode(); }
                            break;
                    }
                }
            }

            return rv;
        }

        public void BtnBuildings()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, BuildTypeEnum.Building);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnStructures()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, BuildTypeEnum.WorldObject);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnFlooring()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, BuildTypeEnum.Floor);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnStorage()
        {
            _gMenuObject = new HUDConstruction(CloseMenu, BuildTypeEnum.Storage);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnEditTown()
        {
            SwitchMenus(MenuEnum.EditMenu);
        }

        public void BtnExitBuildMenu()
        {
            if (_gMainObject == null)
            {
                CloseMenu();
            }

            Scry(false);

            GUIManager.CloseMainObject();
            GameManager.GoToHUDScreen();
            GUIManager.OpenMenu();
        }

        public void BtnMove()
        {
            CloseMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeMoving();
        }

        public void BtnRotate()
        {
            CloseMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeRotate();
        }

        public void BtnUpgrade()
        {
            CloseMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeUpgrade();
        }

        public void BtnRemove()
        {
            CloseMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeDestroy();
        }

        public void BtnStorageMode()
        {
            CloseMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeStorage();
        }

        public void BtnLeaveEditMode()
        {
            SwitchMenus(MenuEnum.BuildMenu);
        }

        public override void CloseMenu()
        {
            switch (_eCurrentMenu)
            {
                case MenuEnum.BuildMenu:
                    RemoveControls(_liBuildMenuObjects);
                    break;
                case MenuEnum.EditMenu:
                    RemoveControls(_liEditMenuObjects);
                    break;
            }            
        }
    
        public override void OpenMenu()
        {
            switch (_eCurrentMenu)
            {
                case MenuEnum.BuildMenu:
                    AddControls(_liBuildMenuObjects);
                    storageButton.Enable(PlayerManager.GetStorageItems().Count > 0);
                    break;
                case MenuEnum.EditMenu:
                    AddControls(_liEditMenuObjects);
                    break;
            }
            
        }

        private void SwitchMenus(MenuEnum newMenu)
        {
            CloseMenu();

            _eCurrentMenu = newMenu;
            OpenMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
        }

        public class HUDConstruction : GUIMainObject
        {
            public static int MAX_SHOWN_TASKS = 4;
            public static int TASK_SPACING = 20;
            List<GUIObject> _liStructures;
            GUIList _gList;
            BuildTypeEnum _eObjectBuildType;

            private CloseMenuDelegate _closeMenu;

            public HUDConstruction(CloseMenuDelegate closeMenu, BuildTypeEnum objType)
            {
                _eObjectBuildType = objType;
                _closeMenu = closeMenu;
                _winMain = SetMainWindow();

                _liStructures = new List<GUIObject>();

                switch (objType)
                {
                    case BuildTypeEnum.Floor:
                        GenerateConstructBoxes(DataManager.FloorIDs);
                        break;
                    case BuildTypeEnum.WorldObject:
                        GenerateConstructBoxes(DataManager.StructureIDs);
                        break;
                    case BuildTypeEnum.Storage:
                        GenerateConstructBoxes(PlayerManager.GetStorageItems());
                        break;
                    case BuildTypeEnum.Building:
                        foreach (BuildInfo b in PlayerManager.DIBuildInfo.Values)
                        {
                            if (b.Unlocked && !b.Built)
                            {
                                ConstructBox box = new ConstructBox(ConstructWorldObject);
                                box.SetConstructionInfo(b.ID, b.Name, b.RequiredToMake);
                                _liStructures.Add(box);
                            }
                        }
                        break;
                }

                _gList = new GUIList(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING/*, _gWindow.Height*/);
                _gList.CenterOnObject(_winMain);

                AddControl(_gList);
            }

            /// <summary>
            /// Given a list of WorldObject ids, generate a ConstructBox
            /// </summary>
            /// <param name="idList">The list of item IDs to create a box for</param>
            public void GenerateConstructBoxes(List<int> idList)
            {
                foreach (int i in idList)
                {
                    Buildable obj = (Buildable)DataManager.GetWorldObjectByID(i);
                    if (obj.RequiredToMake.Count > 0 && obj.CanBuild())
                    {
                        ConstructBox box = new ConstructBox(ConstructWorldObject);
                        box.SetConstructionInfo(i, obj.Name, obj.RequiredToMake);
                        _liStructures.Add(box);
                    }
                }
            }

            /// <summary>
            /// Given a list of WorldObject ids, generate a ConstructBox
            /// </summary>
            /// <param name="idList">The list of item IDs to create a box for</param>
            public void GenerateConstructBoxes(Dictionary<int, int> dictionary)
            {
                foreach (KeyValuePair<int, int> kvp in dictionary)
                {
                    ConstructBox box = new ConstructBox(ConstructStorageObject);
                    Buildable obj = (Buildable)DataManager.GetWorldObjectByID(kvp.Key);
                    box.SetConstructionInfo(kvp.Key, obj.Name, kvp.Value);
                    _liStructures.Add(box);
                }
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

            public override bool ProcessRightButtonClick(Point mouse) { return false; }

            public override bool ProcessHover(Point mouse) { return false; }

            public void ConstructWorldObject(int objID)
            {
                Buildable obj;

                Dictionary<int, int> requiredToMake;
                if (_eObjectBuildType == BuildTypeEnum.Building)
                {
                    obj = DataManager.GetBuilding(objID);
                    requiredToMake = PlayerManager.DIBuildInfo[objID].RequiredToMake;
                }
                else
                {
                    obj = (Buildable)DataManager.GetWorldObjectByID(objID);
                    requiredToMake = obj.RequiredToMake;
                }

                if (InventoryManager.HasSufficientItems(requiredToMake))
                {
                    GameManager.EnterTownModeBuild();
                    GameManager.PickUpWorldObject(obj);
                    MapManager.CurrentMap.AddHeldLights(obj.GetLights());
                    obj.SetPickupOffset();
                }

                GUIManager.CloseMainObject();
                _closeMenu();
            }

            public void ConstructStorageObject(int objID)
            {
                Buildable obj = (Buildable)DataManager.GetWorldObjectByID(objID);

                GameManager.EnterTownModeBuild(true);
                GameManager.PickUpWorldObject(obj);
                MapManager.CurrentMap.AddHeldLights(obj.GetLights());
                obj.SetPickupOffset();

                GUIManager.CloseMainObject();
                _closeMenu();
            }
        }
    }
}
