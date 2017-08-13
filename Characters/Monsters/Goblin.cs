using Adventure.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

using ItemIDs = Adventure.Items.ItemList.ItemIDs;

namespace Adventure.Characters.Monsters
{
    public class Goblin : Monster
    {
        public Goblin(ContentManager theContentManager, Vector2 position)
        {
            _textureName = @"Textures\Eggplant";

            LoadContent(theContentManager, 32, 64, 4, 0.3f);
            Position = position;
            _dropTable = new List<KeyValuePair<ItemIDs, double>>(){
                new KeyValuePair<ItemIDs, double>(ItemIDs.ARCANE_ESSENCE, 0.5),
                new KeyValuePair<ItemIDs, double>(ItemIDs.COPPER_ORE, 0.1)
            };
        }
    }
}
