using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework;
using RiverHollow.Items;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    internal class ItemDisplayWindow : GUIImage
    {
        public int ID { get; }
        public bool Found { get; private set; }
        const float FADE = 0.8f;

        public ItemDisplayWindow(int ItemID, bool found) : base(new Rectangle(96, 128, 20, 20), DataManager.HUD_COMPONENTS)
        {
            ID = ItemID;
            Found = found;

            Item item = DataManager.GetItem(ItemID);
            GUIImage spr = new GUIImage(item.SourceRectangle, item.Texture);
            spr.CenterOnObject(this);
            AddControl(spr);

            if (!found)
            {
                spr.SetColor(Color.Black * FADE);
            }
        }
    }
}
