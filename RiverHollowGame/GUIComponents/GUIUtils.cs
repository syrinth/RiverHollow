﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.GUIComponents.GUIObjects.GUIWindows.GUIWindow;

namespace RiverHollow.GUIComponents
{
    internal class GUIUtils
    {
        internal static Point SCREEN_CENTER = new Point(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

        internal static Rectangle SELECT_CORNER = new Rectangle(160, 128, 20, 20);
        internal static Rectangle SELECT_HIGHLIGHT = new Rectangle(163, 20, 20, 20);
        internal static Rectangle POINTER = new Rectangle(192, 87, 8, 9);
        internal static Rectangle WORLDMAP = new Rectangle(0, 0, 480, 270);
        internal static Rectangle NEW_DISPLAY = new Rectangle(112, 208, 50, 49);
        internal static Rectangle ITEM_BOX = new Rectangle(182, 130, 20, 20);
        internal static Rectangle STRUCTURE_BOX = new Rectangle(284, 204, 68, 68);
        internal static Rectangle PLACEMENT_BOX = new Rectangle(272, 112, 16, 16);
        internal static Rectangle CODEX_ITEM = new Rectangle(96, 128, 20, 20);
        internal static Rectangle CODEX_ITEM_ARCHIVED = new Rectangle(116, 128, 20, 20);

        internal static Rectangle BLACK_BOX = new Rectangle(240, 112, 16, 16);
        internal static Rectangle INVISIBLE = new Rectangle(164, 21, 16, 16);
        internal static Rectangle ALERT_ANIMATION = new Rectangle(0, 256, 16, 16);

        internal static Rectangle ICON_BOOK = new Rectangle(288, 80, 14, 14);
        internal static Rectangle ICON_COIN = new Rectangle(304, 80, 16, 16);
        internal static Rectangle ICON_HAMMER = new Rectangle(320, 80, 14, 14);
        internal static Rectangle ICON_KEY = new Rectangle(320, 112, 16, 16);
        internal static Rectangle ICON_TRAVELER = new Rectangle(336, 80, 14, 15);
        internal static Rectangle ICON_FACE = new Rectangle(288, 112, 16, 16);
        internal static Rectangle ICON_MAP_MARKER = new Rectangle(304, 112, 16, 16);
        internal static Rectangle ICON_HEART = new Rectangle(208, 72, 10, 9);
        internal static Rectangle ICON_HEART_GLOW = new Rectangle(192, 72, 12, 11);
        internal static Rectangle ICON_GARBAGE = new Rectangle(336, 112, 16, 16);
        internal static Rectangle ICON_EXCLAMATION = new Rectangle(278, 131, 4, 10);

        internal static Rectangle DIALOGUE_MORE = new Rectangle(160, 80, 16, 16);
        internal static Rectangle DIALOGUE_DONE = new Rectangle(176, 80, 16, 16);

        internal static Rectangle QUEST_NEW = new Rectangle(160, 96, 16, 16);
        internal static Rectangle QUEST_TURNIN = new Rectangle(176, 96, 16, 16);

        internal static Rectangle CURSOR_POINT = new Rectangle(320, 160, 16, 16);
        internal static Rectangle CURSOR_PICKUP = new Rectangle(336, 160, 16, 16);
        internal static Rectangle CURSOR_DOOR = new Rectangle(320, 176, 16, 16);
        internal static Rectangle CURSOR_TALK = new Rectangle(336, 176, 16, 16);

        internal static Rectangle BTN_MAIN = new Rectangle(176, 112, 52, 16);
        internal static Rectangle BTN_LEFT_SMALL = new Rectangle(163, 43, 10, 13);
        internal static Rectangle BTN_RIGHT_SMALL = new Rectangle(173, 43, 10, 13);
        internal static Rectangle BTN_SKIP = new Rectangle(304, 128, 16, 16);
        internal static Rectangle BTN_DOWN = new Rectangle(288, 160, 16, 16);
        internal static Rectangle BTN_UP = new Rectangle(304, 160, 16, 16);
        internal static Rectangle BTN_RIGHT = new Rectangle(288, 176, 16, 16);
        internal static Rectangle BTN_LEFT = new Rectangle(304, 160, 16, 16);
        internal static Rectangle BTN_INCREASE = new Rectangle(192, 96, 7, 7);
        internal static Rectangle BTN_DECREASE = new Rectangle(201, 96, 7, 7);
        internal static Rectangle BTN_OK_TINY = new Rectangle(194, 103, 11, 9);
        internal static Rectangle BTN_BUY = new Rectangle(164, 0, 18, 19);
        internal static Rectangle BTN_GIVE = new Rectangle(164, 58, 18, 19);

        internal static Rectangle TOGGLE_MUTE = new Rectangle(320, 128, 16, 16);
        internal static Rectangle TOGGLE_UNMUTE = new Rectangle(336, 128, 16, 16);
        internal static Rectangle TOGGLE_CHECK= new Rectangle(160, 170, 10, 10);
        internal static Rectangle TOGGLE_UNCHECK = new Rectangle(160, 160, 10, 10);
        internal static Rectangle TOGGLE_VILLAGERS_ON = new Rectangle(23, 139, 22, 21);
        internal static Rectangle TOGGLE_VILLAGERS_OFF = new Rectangle(0, 143, 22, 17);
        internal static Rectangle TOGGLE_MERCHANTS_ON = new Rectangle(71, 139, 22, 21);
        internal static Rectangle TOGGLE_MERCHANTS_OFF = new Rectangle(48, 143, 22, 17);
        internal static Rectangle TOGGLE_TRAVELERS_ON = new Rectangle(23, 171, 22, 21);
        internal static Rectangle TOGGLE_TRAVELERS_OFF = new Rectangle(0, 175, 22, 17);
        internal static Rectangle TOGGLE_MOBS_ON = new Rectangle(71, 171, 22, 21);
        internal static Rectangle TOGGLE_MOBS_OFF = new Rectangle(48, 175, 22, 17);
        internal static Rectangle TOGGLE_ITEMS_ON = new Rectangle(23, 203, 22, 21);
        internal static Rectangle TOGGLE_ITEMS_OFF = new Rectangle(0, 207, 22, 17);

        internal static Rectangle TOGGLE_RESOURCE_ON = new Rectangle(96, 160, 16, 16);
        internal static Rectangle TOGGLE_RESOURCE_OFF = new Rectangle(96, 176, 16, 16);
        internal static Rectangle TOGGLE_POTIONS_ON = new Rectangle(112, 160, 16, 16);
        internal static Rectangle TOGGLE_POTIONS_OFF = new Rectangle(112, 176, 16, 16);
        internal static Rectangle TOGGLE_TOOLS_ON = new Rectangle(128, 160, 16, 16);
        internal static Rectangle TOGGLE_TOOLS_OFF = new Rectangle(128, 176, 16, 16);
        internal static Rectangle TOGGLE_SPECIAL_ON = new Rectangle(144, 160, 16, 16);
        internal static Rectangle TOGGLE_SPECIAL_OFF = new Rectangle(144, 176, 16, 16);

        internal static Rectangle HUD_FILL = new Rectangle(177, 161, 14, 14);
        internal static Rectangle HUD_COLOR_PICK = new Rectangle(177, 176, 7, 9);
        internal static Rectangle HUD_COLOR_TOGGLE = new Rectangle(187, 176, 3, 5);
        internal static Rectangle HUD_COLOR_PATCH = new Rectangle(178, 177, 1, 3);
        internal static Rectangle HUD_COLOR_FRAME = new Rectangle(176, 187, 32, 5);

        internal static Rectangle HUD_SCROLL_S = new Rectangle(2, 120, 100, 3);
        internal static Rectangle HUD_SCROLL_L = new Rectangle(209, 96, 142, 3);
        internal static Rectangle HUD_DIVIDER = new Rectangle(106, 122, 8, 1);

        internal static Rectangle WIN_IMAGE_CRAFTING = new Rectangle(192, 0, 160, 71);
        internal static Rectangle WIN_UPGRADE = new Rectangle(0, 0, 162, 119);

        internal static WindowData Brown_Window = new WindowData(0, 240, 5, 5, 6, 6, 4);
        internal static WindowData DarkBlue_Window = new WindowData(16, 240, 5, 5, 6, 6, 4);
        internal static WindowData Codex_NPC_Window = new WindowData(32, 240, 5, 5, 6, 6, 4);
        internal static WindowData GreyWin = new WindowData(48, 240, 2, 2, 2, 2, 12);
        internal static WindowData DisplayWin = new WindowData(64, 240, 1, 1, 1, 1, 14);
        internal static WindowData WoodenPanel = new WindowData(80, 240, 3, 3, 3, 3, 10);

        public static bool ProcessLeftMouseButton(Point mouse, params GUIObject[] list)
        {
            bool rv = false;
            foreach (var g in list)
            {
                rv = g.ProcessLeftButtonClick(mouse);
                if (rv)
                {
                    break;
                }
            }

            return rv;
        }
        public static bool CreateRequiredItemsList(ref List<GUIItemBox> _liRequiredItems, Dictionary<int, int> requiredItemList)
        {
            bool rv = true;

            _liRequiredItems.ForEach(x => x.RemoveSelfFromControl());
            _liRequiredItems.Clear();

            foreach (KeyValuePair<int, int> kvp in requiredItemList)
            {
                GUIItemBox newItem = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));
                if (!newItem.CompareNumToPlayer())
                {
                    rv = false;
                }
                _liRequiredItems.Add(newItem);
            }

            return rv;
        }
        public static void SetObjectScale(GUIObject guiObject, int width, int height, int simulatedBoxes)
        {
            int chosenScale = GameManager.CurrentScale;
            int biggestValue = Math.Max(width, height);

            for (int i = 1; i <= GameManager.CurrentScale; i++)
            {
                int comparator = (Constants.TILE_SIZE * simulatedBoxes * GameManager.CurrentScale);
                if (biggestValue * i <= comparator)
                {
                    chosenScale = i;
                }
            }

            guiObject.SetScale(chosenScale);
        }

        internal static void CreateSpacedColumn(List<GUIObject> components, int columnLine, int start, int totalHeight, int spacing, bool alignToColumnLine = false)
        {
            int startY = start + ((totalHeight - (components.Count * components[0].Height) - (spacing * components.Count - 1)) / 2) + components[0].Height / 2;
            Point position = new Point(alignToColumnLine ? columnLine : columnLine - components[0].Width / 2, startY);

            foreach (GUIObject o in components)
            {
                o.Position(position);
                position.Y += o.Height + spacing;
            }
        }
        internal static void CreateSpacedRowAgainstObject(List<GUIObject> components, GUIObject mainControl, GUIObject objAbove, int spacing, int yDrop)
        {
            if (components.Count > 0)
            {
                int totalReqWidth = (components.Count * components[0].Width) + ((components.Count - 1) * GameManager.ScaleIt(spacing));
                int firstXPosition = (mainControl.Width / 2) - (totalReqWidth / 2);
                for (int i = 0; i < components.Count; i++)
                {
                    if (i == 0)
                    {
                        components[i].Position(new Point(mainControl.Position().X, objAbove.Position().Y));
                        components[i].MoveBy(firstXPosition, GameManager.ScaleIt(yDrop));
                    }
                    else
                    {
                        components[i].AnchorAndAlignWithSpacing(components[i - 1], SideEnum.Right, SideEnum.Top, spacing);
                    }
                    mainControl.AddControl(components[i]);
                }
            }
        }
        internal static void CreateSpacedGrid(List<GUIObject> components, GUIObject objSync, Point offset, int columns, int xSpacing, int ySpacing)
        {
            for (int i = 0; i < components.Count; i++)
            {
                GUIObject obj = components[i];
                if (i == 0)
                {
                    obj.PositionAndMove(objSync, offset);
                }
                else if (i % columns == 0)
                {
                    obj.AnchorAndAlignWithSpacing(components[i - columns], SideEnum.Bottom, SideEnum.Left, ySpacing);
                }
                else
                {
                    obj.AnchorAndAlignWithSpacing(components[i - 1], SideEnum.Right, SideEnum.Bottom, xSpacing);
                }
            }
        }
    }
}
