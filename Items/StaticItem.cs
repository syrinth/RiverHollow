using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    public class StaticItem : Item
    {
        public StaticItem() { }
        public StaticItem(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);
            _texTexture = GameContentManager.GetTexture(GameContentManager.ITEM_FOLDER + "StaticObjects");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
