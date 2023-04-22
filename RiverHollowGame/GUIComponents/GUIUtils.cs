using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents
{
    internal class GUIUtils
    {
        internal static Point CenterScreen = new Point(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

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

            _liRequiredItems.ForEach(x => x.RemoveSelfFromControls());
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

        public static GUIImage GetIcon(GameIconEnum e)
        {
            GUIImage rv = null;

            switch (e)
            {
                case GameIconEnum.Book:
                    rv = new GUIImage(new Rectangle(288, 80, 14, 14), DataManager.HUD_COMPONENTS);
                    break;
                case GameIconEnum.Coin:
                    rv = new GUIImage(new Rectangle(0, 32, 16, 16), DataManager.DIALOGUE_TEXTURE);
                    break;
                case GameIconEnum.Key:
                    rv = new GUIImage(new Rectangle(16, 16, 16, 16), DataManager.DIALOGUE_TEXTURE);
                    break;
                case GameIconEnum.Traveler:
                    rv = new GUIImage(new Rectangle(336, 80, 14, 15), DataManager.HUD_COMPONENTS);
                    break;
                case GameIconEnum.Hammer:
                    rv = new GUIImage(new Rectangle(320, 80, 14, 14), DataManager.HUD_COMPONENTS);
                    break;
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

        internal static void CreateSpacedColumn(ref List<GUIObject> components, int columnLine, int start, int totalHeight, int spacing, bool alignToColumnLine = false)
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
                        components[i].AnchorAndAlignToObject(components[i - 1], SideEnum.Right, SideEnum.Top, GameManager.ScaleIt(spacing));
                    }
                    mainControl.AddControl(components[i]);
                }
            }
        }
        internal static void CreateSpacedGrid(List<GUIObject> components, Point startPoint, int start, int end, int columns, int xSpacing, int ySpacing)
        {
            for (int i = start; i < end; i++)
            {
                GUIObject obj = components[i];
                int listIndex = i - start;
                if (i == start)
                {
                    obj.Position(startPoint);
                }
                else if (i % columns == 0)
                {
                    obj.AnchorAndAlignToObject(components[listIndex - columns], SideEnum.Bottom, SideEnum.Left, GameManager.ScaleIt(ySpacing));
                }
                else
                {
                    obj.AnchorAndAlignToObject(components[listIndex - 1], SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(xSpacing));
                }
            }
        }
        internal static void CenterAndAlignToScreen(ref List<GUIObject> components)
        {
            int top = (int)components[0].Position().Y;
            int bottom = (int)components[0].Position().Y + components[0].Height;
            int left = (int)components[0].Position().X;
            int right = (int)components[0].Position().X + components[0].Width;

            foreach (GUIObject o in components)
            {
                top = (int)MathHelper.Min(top, o.Position().Y);
                bottom = (int)MathHelper.Max(bottom, o.Position().Y + o.Height);
                left = (int)MathHelper.Min(left, o.Position().X);
                right = (int)MathHelper.Max(right, o.Position().X + o.Width);
            }

            Point stackCenter = new Rectangle(left, top, right - left, bottom - top).Center;
            Point delta = CenterScreen - stackCenter;

            foreach (GUIObject o in components)
            {
                o.Position(o.Position() + delta);
            }
        }
    }
}
