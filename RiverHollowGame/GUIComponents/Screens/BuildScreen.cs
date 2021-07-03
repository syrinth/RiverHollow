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
            Scry(true);

            _liButtons = new List<GUIObject>() {
                new GUIButton("Build", BtnBuildings),
                new GUIButton("Structures", BtnStructures),
                new GUIButton("Flooring", BtnFlooring),
                new GUIButton("Move", BtnMove),
                new GUIButton("Remove", BtnRemove),
                new GUIButton("Exit", BtnExitMenu)
            };
            AddControls(_liButtons);

            GUIObject.CreateSpacedColumn(ref _liButtons, GUIButton.BTN_WIDTH/2, 0, RiverHollow.ScreenHeight, BTN_PADDING);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = base.ProcessRightButtonClick(mouse);

            //If we're holding something, and we're moving an object. Do not close anything
            if (GameManager.HeldObject != null && GameManager.TownModeMoving()) { rv = true; }

            //If the right click has not been processed, we probably want to close anything that we have open.
            if (!rv)
            {
                if (InTownMode()) {
                    LeaveTownMode();
                    GUIManager.OpenMenu();
                    GameManager.DropWorldObject();
                }
                else
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
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeMoving();
        }

        public void BtnRemove()
        {
            CloseMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeDestroy();
        }

        public void BtnExitMenu()
        {
            RiverHollow.ResetCamera();
            GUIManager.SetScreen(new HUDScreen());
            GameManager.Unpause();
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

                switch (objType)
                {
                    case ObjectTypeEnum.Floor:
                        GenerateConstructBoxes(DataManager.FloorIDs);
                        break;
                    case ObjectTypeEnum.Plant:
                        GenerateConstructBoxes(DataManager.PlantIDs);
                        break;
                    case ObjectTypeEnum.WorldObject:
                        GenerateConstructBoxes(DataManager.StructureIDs);
                        break;
                    case ObjectTypeEnum.Building:
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
                    ConstructBox box = new ConstructBox(ConstructWorldObject);
                    Buildable obj = (Buildable)DataManager.GetWorldObjectByID(i);
                    box.SetConstructionInfo(i, obj.Name, obj.RequiredToMake);
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
                string name = string.Empty;
                Dictionary<int, int> requiredToMake;
                if (_eObjectBuildType == ObjectTypeEnum.Building)
                {
                    obj = DataManager.GetBuilding(objID);
                    requiredToMake = PlayerManager.DIBuildInfo[objID].RequiredToMake;
                    name = PlayerManager.DIBuildInfo[objID].Name;
                }
                else
                {
                    obj = (Buildable)DataManager.GetWorldObjectByID(objID);
                    requiredToMake = obj.RequiredToMake;
                    name = obj.Name;
                }

                if (InventoryManager.HasSufficientItems(requiredToMake))
                {
                    GameManager.EnterTownModeBuild();
                    GameManager.PickUpWorldObject(obj);
                    obj.SetPickupOffset();
                }

                GUIManager.CloseMainObject();
                _closeMenu();
            }
        }
    }
}
