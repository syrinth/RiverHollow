using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using RiverHollow.Items;
using System;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    internal class ItemDisplayWindow : GUIImage
    {
        public int ID { get; }
        public bool Found { get; private set; }
        const float FADE = 0.8f;

        public ItemDisplayWindow(int ItemID, ValueTuple<bool, bool> state) : base((state.Item1 && state.Item2) ? GUIUtils.CODEX_ITEM_ARCHIVED : GUIUtils.CODEX_ITEM)
        {
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
    }
}
