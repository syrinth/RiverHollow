using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.MainObjects
{
    public class WarpPointWindow : GUIMainObject
    {
        public static int MAX_SHOWN_TASKS = 4;
        public static int TASK_SPACING = 20;

        WarpPoint _currWarpPoint;

        List _gList;
        List<GUIObject> _liStructures;        

        public WarpPointWindow(WarpPoint obj)
        {
            _winMain = SetMainWindow();
            _currWarpPoint = obj;

            _liStructures = new List<GUIObject>();
            foreach (WarpPoint w in DungeonManager.CurrentDungeon.WarpPoints)
            {
                if (w.Active && w != _currWarpPoint)
                {
                    WarpPointBox box = new WarpPointBox(w, ChooseWarpPoint);
                    _liStructures.Add(box);
                }
            }

            _gList = new List(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING);
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

        public override bool ProcessRightButtonClick(Point mouse) { return false; }

        public override bool ProcessHover(Point mouse) { return false; }

        public void ChooseWarpPoint(WarpPoint obj)
        {
            MapManager.FadeToNewMap(obj.CurrentMap, obj.CollisionBox.Location.ToVector2() + new Vector2(0, GameManager.TILE_SIZE));

            GUIManager.CloseMainObject();
        }
    }

    public class WarpPointBox : GUIObject
    {
        public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
        public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)

        GUIWindow _window;
        GUIText _gName;
        WarpPoint _objWarpPoint;
        public delegate void SelectWarpPoint(WarpPoint selectedPoint);
        private SelectWarpPoint _delAction;

        public WarpPointBox(WarpPoint obj, SelectWarpPoint del)
        {
            _delAction = del;
            _objWarpPoint = obj;

            int boxWidth = CONSTRUCTBOX_WIDTH;
            int boxHeight = CONSTRUCTBOX_HEIGHT;

            _window = new GUIWindow(GUIWindow.Window_1, boxWidth, boxHeight);
            AddControl(_window);

            _gName = new GUIText(_objWarpPoint.CurrentMap.Name);
            _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);

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
                _delAction(_objWarpPoint);
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse) { return false; }
    }
}

//namespace RiverHollow.GUIComponents.MainObjects
//{
//    public class GardenWindow : GUIMainObject
//    {
//        public static int MAX_SHOWN_TASKS = 4;
//        public static int TASK_SPACING = 20;

//        List<GUIObject> _liStructures;
//        GUIList _gList;
//        Garden _objGarden;

//        public GardenWindow(Garden targetGarden)
//        {
//            _winMain = SetMainWindow();
//            _objGarden = targetGarden;

//            _liStructures = new List<GUIObject>();

//            foreach (int i in DataManager.PlantIDs)
//            {
//                ConstructBox box = new ConstructBox(ChoosePlant);
//                Plant obj = (Plant)DataManager.GetWorldObjectByID(i);

//                Dictionary<int, int> toMake = new Dictionary<int, int> { [obj.SeedID] = 1 };
//                box.SetConstructionInfo(i, obj.Name, toMake);
//                _liStructures.Add(box);
//            }

//            _gList = new GUIList(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING);
//            _gList.CenterOnObject(_winMain);

//            AddControl(_gList);
//        }

//        public override bool ProcessLeftButtonClick(Point mouse)
//        {
//            bool rv = false;

//            foreach (GUIObject c in Controls)
//            {
//                rv = c.ProcessLeftButtonClick(mouse);

//                if (rv) { break; }
//            }

//            return rv;
//        }

//        public override bool ProcessRightButtonClick(Point mouse) { return false; }

//        public override bool ProcessHover(Point mouse) { return false; }

//        public void ChoosePlant(int objID)
//        {
//            Plant obj = (Plant)DataManager.GetWorldObjectByID(objID);

//            if (InventoryManager.HasItemInPlayerInventory(obj.SeedID, 1))
//            {
//                _objGarden.SetPlant(obj);
//            }

//            GUIManager.CloseMainObject();
//        }
//    }
//}
