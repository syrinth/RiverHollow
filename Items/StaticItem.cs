using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class StaticItem : InventoryItem
    {
        public StaticItem(ObjectManager.ItemIDs ID, Vector2 sourcePos, Texture2D texture, string name, string description, List<KeyValuePair<ObjectManager.ItemIDs, int>> reagents) : base(ID, sourcePos, texture, name, description, 1, false, reagents)
        {

        }
    }
}
