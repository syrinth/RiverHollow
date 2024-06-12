using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.GUIComponents.GUIObjects.GUIWindows.GUIWindow;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents
{
    internal class GUIUtils
    {
        public enum ParentRuleEnum { Auto, ForceToObject, ForceToParent, Skip };

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

        internal static Rectangle TRAVELER_ANGRY = new Rectangle(208, 128, 16, 16);
        internal static Rectangle TRAVELER_SAD = new Rectangle(224, 128, 16, 16);
        internal static Rectangle TRAVELER_NEUTRAL = new Rectangle(240, 128, 16, 16);
        internal static Rectangle TRAVELER_HAPPY = new Rectangle(256, 128, 16, 16);

        internal static Rectangle ICON_BOOK = new Rectangle(288, 80, 14, 14);
        internal static Rectangle ICON_COIN = new Rectangle(304, 80, 16, 16);
        internal static Rectangle ICON_TINY_COIN = new Rectangle(0, 16, 6, 6);
        internal static Rectangle ICON_HAMMER = new Rectangle(320, 80, 14, 14);
        internal static Rectangle ICON_KEY = new Rectangle(320, 112, 16, 16);
        internal static Rectangle ICON_TRAVELER = new Rectangle(336, 80, 14, 15);
        internal static Rectangle ICON_BAG = new Rectangle(336, 64, 16, 16);
        internal static Rectangle ICON_CHEST = new Rectangle(304, 64, 16, 16);
        internal static Rectangle ICON_FACE = new Rectangle(288, 112, 16, 16);
        internal static Rectangle ICON_MAP_MARKER = new Rectangle(304, 112, 16, 16);
        internal static Rectangle ICON_HEART = new Rectangle(208, 72, 10, 9);
        internal static Rectangle ICON_HEART_GLOW = new Rectangle(192, 72, 12, 11);
        internal static Rectangle ICON_GARBAGE = new Rectangle(336, 112, 16, 16);
        internal static Rectangle ICON_EXCLAMATION = new Rectangle(272, 128, 4, 10);
        internal static Rectangle ICON_ERROR = new Rectangle(276, 128, 8, 8);
        internal static Rectangle ICON_BUILD = new Rectangle(336, 144, 16, 16);

        internal static Rectangle ICON_CLASS_GENERAL = new Rectangle(0, 0, 10, 10);
        internal static Rectangle ICON_CLASS_FIGHTER = new Rectangle(10, 0, 10, 10);
        internal static Rectangle ICON_CLASS_HEALER = new Rectangle(20, 0, 10, 10);
        internal static Rectangle ICON_CLASS_MAGE = new Rectangle(30, 0, 10, 10);
        internal static Rectangle ICON_CLASS_ROGUE = new Rectangle(40, 0, 10, 10);

        internal static Rectangle ICON_DESC_HEALTH = new Rectangle(0, 32, 10, 9);
        internal static Rectangle ICON_DESC_ENERGY = new Rectangle(11, 32, 10, 9);
        internal static Rectangle ICON_DESC_TIME = new Rectangle(22, 32, 10, 9);
        internal static Rectangle ICON_DESC_LEVEL = new Rectangle(33, 32, 9, 9);

        internal static Rectangle DIALOGUE_MORE = new Rectangle(160, 80, 16, 16);
        internal static Rectangle DIALOGUE_DONE = new Rectangle(176, 80, 16, 16);

        internal static Rectangle QUEST_NEW = new Rectangle(160, 96, 16, 16);
        internal static Rectangle QUEST_TURNIN = new Rectangle(176, 96, 16, 16);
        internal static Rectangle HELD_ITEM = new Rectangle(144, 128, 16, 16);

        internal static Rectangle CURSOR_POINT = new Rectangle(320, 160, 16, 16);
        internal static Rectangle CURSOR_PICKUP = new Rectangle(336, 160, 16, 16);
        internal static Rectangle CURSOR_DOOR = new Rectangle(320, 176, 16, 16);
        internal static Rectangle CURSOR_TALK = new Rectangle(336, 176, 16, 16);
        internal static Rectangle CURSOR_INTERACT = new Rectangle(304, 176, 16, 16);
        internal static Rectangle CURSOR_SHOP = new Rectangle(288, 176, 16, 16);

        internal static Rectangle BTN_MAIN = new Rectangle(176, 112, 52, 16);
        internal static Rectangle BTN_UP_SMALL = new Rectangle(295, 147, 12, 10);
        internal static Rectangle BTN_DOWN_SMALL = new Rectangle(307, 157, 12, 10);
        internal static Rectangle BTN_LEFT_SMALL = new Rectangle(297, 157, 10, 12);
        internal static Rectangle BTN_RIGHT_SMALL = new Rectangle(307, 145, 10, 12);
        internal static Rectangle BTN_SKIP = new Rectangle(304, 128, 16, 16);
        internal static Rectangle BTN_INCREASE = new Rectangle(192, 96, 7, 7);
        internal static Rectangle BTN_DECREASE = new Rectangle(201, 96, 7, 7);
        internal static Rectangle BTN_OK_TINY = new Rectangle(194, 103, 11, 9);
        internal static Rectangle BTN_BUY = new Rectangle(164, 0, 18, 19);
        internal static Rectangle BTN_GIVE = new Rectangle(164, 58, 18, 19);

        internal static Rectangle TOGGLE_MUTE = new Rectangle(320, 128, 16, 16);
        internal static Rectangle TOGGLE_UNMUTE = new Rectangle(336, 128, 16, 16);
        internal static Rectangle TOGGLE_CHECK= new Rectangle(96, 218, 10, 10);
        internal static Rectangle TOGGLE_UNCHECK = new Rectangle(96, 208, 10, 10);

        internal static Rectangle TAB_UNSELECTED = new Rectangle(23, 139, 22, 21);
        internal static Rectangle TAB_SELECTED = new Rectangle(0, 143, 22, 17);
        internal static Rectangle TAB_VILLAGER_ICON = new Rectangle(0, 160, 14, 12);
        internal static Rectangle TAB_MERCHANT_ICON = new Rectangle(14, 160, 14, 12);
        internal static Rectangle TAB_TRAVELER_ICON = new Rectangle(28, 160, 14, 12);
        internal static Rectangle TAB_MOB_ICON = new Rectangle(0, 172, 14, 12);
        internal static Rectangle TAB_ITEM_ICON = new Rectangle(14, 172, 14, 12);
        internal static Rectangle TAB_STRUCTURE_ICON = new Rectangle(28, 172, 14, 12);
        internal static Rectangle TAB_FLOOR_ICON = new Rectangle(0, 184, 14, 12);
        internal static Rectangle TAB_WALL_ICON = new Rectangle(14, 184, 14, 12);
        internal static Rectangle TAB_FURNITURE_ICON = new Rectangle(28, 184, 14, 12);
        internal static Rectangle TAB_LIGHTING_ICON = new Rectangle(0, 196, 14, 12);
        internal static Rectangle TAB_OVERVIEW_ICON = new Rectangle(14, 196, 14, 12);
        internal static Rectangle TAB_TOWN_UPGRADE_ICON = new Rectangle(28, 196, 14, 12);

        internal static Rectangle TOGGLE_ITEMS_ON = new Rectangle(48, 160, 16, 16);
        internal static Rectangle TOGGLE_ITEMS_OFF = new Rectangle(64, 160, 16, 16);
        internal static Rectangle TOGGLE_ITEMS_RESOURCES_ICON = new Rectangle(80, 160, 16, 16);
        internal static Rectangle TOGGLE_ITEMS_POTIONS_ICON = new Rectangle(48, 176, 16, 16);
        internal static Rectangle TOGGLE_ITEMS_TOOLS_ICON = new Rectangle(64, 176, 16, 16);
        internal static Rectangle TOGGLE_ITEMS_FOOD_ICON = new Rectangle(80, 176, 16, 16);
        internal static Rectangle TOGGLE_ITEMS_SPECIAL_ICON = new Rectangle(48, 192, 16, 16);

        internal static Rectangle HUD_FILL = new Rectangle(177, 161, 14, 14);
        internal static Rectangle HUD_COLOR_PICK = new Rectangle(177, 176, 7, 9);
        internal static Rectangle HUD_COLOR_TOGGLE = new Rectangle(187, 176, 3, 5);
        internal static Rectangle HUD_COLOR_PATCH = new Rectangle(178, 177, 1, 3);
        internal static Rectangle HUD_COLOR_FRAME = new Rectangle(176, 187, 32, 5);

        internal static Rectangle HUD_SCROLL_S = new Rectangle(2, 120, 100, 3);
        internal static Rectangle HUD_SCROLL_L = new Rectangle(209, 96, 142, 3);
        internal static Rectangle HUD_DIVIDER = new Rectangle(106, 122, 8, 1);

        internal static Rectangle HUD_DESC_TOP = new Rectangle(0, 352, 176, 30);
        internal static Rectangle HUD_DESC_MID = new Rectangle(0, 383, 176, 9);
        internal static Rectangle HUD_DESC_BOT = new Rectangle(0, 393, 176, 5);

        internal static Rectangle HUD_SHOP_TOP = new Rectangle(176, 288, 176, 80);
        internal static Rectangle HUD_SHOP_MID = new Rectangle(176, 368, 176, 32);
        internal static Rectangle HUD_SHOP_BOT = new Rectangle(176, 400, 176, 48);

        internal static Rectangle WIN_IMAGE_CRAFTING = new Rectangle(192, 0, 160, 50);
        internal static Rectangle LEVEL_TAB = new Rectangle(48, 144, 44, 16);

        internal static Rectangle PLAYER_INVENTORY_PANE = new Rectangle(256, 160, 28, 40);
        internal static Rectangle INVENTORY_ICON_HAT = new Rectangle(208, 144, 16, 16);
        internal static Rectangle INVENTORY_ICON_SHIRT = new Rectangle(224, 144, 16, 16);
        internal static Rectangle INVENTORY_ICON_PANTS = new Rectangle(240, 144, 16, 16);
        internal static Rectangle INVENTORY_ICON_NECK = new Rectangle(208, 160, 16, 16);
        internal static Rectangle INVENTORY_ICON_RING = new Rectangle(224, 160, 16, 16);
        internal static Rectangle INVENTORY_ICON_LANTERN = new Rectangle(256, 144, 16, 16);

        internal static WindowData WINDOW_BROWN = new WindowData(0, 240, 5, 5, 6, 6, 4);
        internal static WindowData WINDOW_DARKBLUE = new WindowData(16, 240, 5, 5, 6, 6, 4);
        internal static WindowData WINDOW_CODEX_NPC = new WindowData(32, 240, 5, 5, 6, 6, 4);
        internal static WindowData WINDOW_GREY = new WindowData(48, 240, 2, 2, 2, 2, 12);
        internal static WindowData WINDOW_DISPLAY = new WindowData(64, 240, 1, 1, 1, 1, 14);
        internal static WindowData WINDOW_WOODEN_PANEL = new WindowData(80, 240, 3, 3, 3, 3, 10);
        internal static WindowData WINDOW_WOODEN_TITLE = new WindowData(0, 256, 17, 5, 7, 7, 10);

        internal static Rectangle RECIPE_TOP = new Rectangle(0, 288, 176, 19);
        internal static Rectangle RECIPE_MIDDLE = new Rectangle(0, 307, 176, 19);
        internal static Rectangle RECIPE_BOTTOM = new Rectangle(0, 326, 176, 20);
        internal static Rectangle RECIPE_DOTS = new Rectangle(160, 272, 16, 16);

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
                
                if (!newItem.CompareNumToInventory())
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
        internal static void CreateSpacedRowAgainstObject(List<GUIObject> components, GUIObject mainControl, GUIObject objAgainst, int spacing, int yChange)
        {
            if (components.Count > 0)
            {
                int totalReqWidth = (components.Count * components[0].Width) + ((components.Count - 1) * GameManager.ScaleIt(spacing));
                int firstXPosition = (mainControl.Width / 2) - (totalReqWidth / 2);
                for (int i = 0; i < components.Count; i++)
                {
                    if (i == 0)
                    {
                        components[i].Position(new Point(mainControl.Position().X, objAgainst.Position().Y));
                        components[i].MoveBy(firstXPosition, GameManager.ScaleIt(yChange));
                    }
                    else
                    {
                        components[i].AnchorAndAlignWithSpacing(components[i - 1], SideEnum.Right, SideEnum.CenterY, spacing);
                    }
                    mainControl.AddControl(components[i]);
                }
            }
        }
        internal static void CreateSpacedGrid(List<GUIObject> components, GUIObject objSync, Point offset, int columns, int xSpacing, int ySpacing, ParentRuleEnum rule = ParentRuleEnum.Auto)
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
                    obj.AnchorAndAlignWithSpacing(components[i - columns], SideEnum.Bottom, SideEnum.Left, ySpacing, rule);
                }
                else
                {
                    obj.AnchorAndAlignWithSpacing(components[i - 1], SideEnum.Right, SideEnum.Bottom, xSpacing, rule);
                }
            }
        }

        internal static GUIImage GetClassIcon(ClassTypeEnum e)
        {
            Rectangle icon;
            switch (e)
            {
                case ClassTypeEnum.Fighter:
                    icon = ICON_CLASS_FIGHTER; break;
                case ClassTypeEnum.Healer:
                    icon = ICON_CLASS_HEALER; break;
                case ClassTypeEnum.Mage:
                    icon = ICON_CLASS_MAGE; break;
                case ClassTypeEnum.Rogue:
                    icon = ICON_CLASS_ROGUE; break;
                default:
                    icon = ICON_CLASS_GENERAL; break;
            }

            return new GUIImage(icon);
        }
    }
}
