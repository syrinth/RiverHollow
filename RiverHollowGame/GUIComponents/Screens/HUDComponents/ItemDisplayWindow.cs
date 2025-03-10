﻿using RiverHollow.Game_Managers;
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

        public ItemDisplayWindow(int ItemID, ItemDataState state) : base((state.Codexed && state.Archived) ? GUIUtils.CODEX_ITEM_ARCHIVED : GUIUtils.CODEX_ITEM)
        {
            HoverControls = false;

            ID = ItemID;
            Found = state.Codexed;

            GUIItem spr = new GUIItem(DataManager.GetItem(ItemID), ItemBoxDraw.Never);
            spr.CenterOnObject(this);
            AddControl(spr);

            if (!state.Codexed)
            {
                spr.DrawShadow(false);
                spr.SetColor(Color.Black * FADE);
                spr.SetImageColor(Color.Black * FADE);
            }
        }

        protected override void BeginHover()
        {
            var infoWindow = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL);

            string strText = Found ? DataManager.GetTextData(ID, "Name", DataType.Item) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(infoWindow, SideEnum.TopLeft);

            infoWindow.Resize(false);
            infoWindow.AnchorAndAlignWithSpacing(this, SideEnum.Bottom, SideEnum.CenterX, -1, GUIUtils.ParentRuleEnum.Skip);
            text.AlignToObject(infoWindow, SideEnum.CenterX);

            GUIManager.OpenHoverObject(infoWindow, DrawRectangle, true);
        }
    }
}
