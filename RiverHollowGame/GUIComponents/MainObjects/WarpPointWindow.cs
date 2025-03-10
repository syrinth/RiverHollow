﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.MainObjects
{
    public class WarpPointWindow : GUIMainObject
    {
        public static int MAX_SHOWN_TASKS = 4;
        public static int TASK_SPACING = 5;

        WarpPoint _currWarpPoint;

        GUIList _gList;
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

            _gList = new GUIList(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING, _winMain);

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

        public void ChooseWarpPoint(WarpPoint obj)
        {
            MapManager.FadeToNewMap(obj.CurrentMap, obj.BaseRectangle.Location + new Point(0, Constants.TILE_SIZE), Enums.DirectionEnum.Down);

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

            _window = new GUIWindow(GUIUtils.WINDOW_BROWN, boxWidth, boxHeight);
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
    }
}
