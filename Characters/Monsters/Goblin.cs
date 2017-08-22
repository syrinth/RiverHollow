using Adventure.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

using ItemIDs = Adventure.Game_Managers.ObjectManager.ItemIDs;

namespace Adventure.Characters.Monsters
{
    public class Goblin : Monster
    {
        public Goblin(Vector2 position)
        {
            _hp = 10;
            _textureName = @"Textures\Eggplant";

            LoadContent(_textureName, 32, 64, 4, 0.3f);
            Position = position;
            _dropTable = new List<KeyValuePair<ItemIDs, double>>(){
                new KeyValuePair<ItemIDs, double>(ItemIDs.ArcaneEssence, 0.5),
                new KeyValuePair<ItemIDs, double>(ItemIDs.CopperOre, 0.1)
            };
        }
    }
}
