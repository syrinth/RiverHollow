using Adventure.Game_Managers;
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
        public StaticItem(ObjectManager.ItemIDs ID, Texture2D texture, string name, string description) : base(ID, texture, name, description, 1, false)
        {

        }
    }
}
