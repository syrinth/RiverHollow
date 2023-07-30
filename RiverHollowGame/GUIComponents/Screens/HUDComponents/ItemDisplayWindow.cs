using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using RiverHollow.Items;
using System;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    internal class ItemDisplayWindow : GUIImage
    {
        public int ID { get; }
        public bool Found { get; private set; }
        const float FADE = 0.8f;

        public ItemDisplayWindow(int ItemID, ValueTuple<bool, bool> state) : base((state.Item1 && state.Item2) ? GUIUtils.CODEX_ITEM_ARCHIVED : GUIUtils.CODEX_ITEM)
        {
            HoverControls = false;

            ID = ItemID;
            Found = state.Item1;

            Item item = DataManager.GetItem(ItemID);
            GUIImage spr = new GUIImage(item.SourceRectangle, item.Texture);
            spr.CenterOnObject(this);
            AddControl(spr);

            if (!state.Item1)
            {
                spr.SetColor(Color.Black * FADE);
            }
        }

        protected override void BeginHover()
        {
            var infoWindow = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL);

            string strText = Found ? DataManager.GetTextData(ID, "Name", DataType.Item) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(infoWindow, SideEnum.TopLeft);

            if (Found)
            {
                string strDescText = DataManager.GetTextData(ID, "Description", DataType.Item);
                if (!string.IsNullOrEmpty(strDescText))
                {
                    GUIText descText = new GUIText(strDescText);
                    descText.AnchorAndAlignWithSpacing(text, SideEnum.Bottom, SideEnum.Left, 2);
                }
            }

            infoWindow.Resize(false);
            infoWindow.AnchorAndAlignWithSpacing(this, SideEnum.Bottom, SideEnum.CenterX, -1, GUIUtils.ParentRuleEnum.Skip);
            text.AlignToObject(infoWindow, SideEnum.CenterX);

            GUIManager.OpenHoverObject(infoWindow, DrawRectangle, true);
        }
    }
}
