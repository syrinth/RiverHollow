using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Gatherable : WorldObject
    {
        readonly int _iItemID;

        public Gatherable(int itemID) : base(-1)
        {
            _iItemID = itemID;

            Item wrappedItem = DataManager.GetItem(itemID);
            _sprite = new AnimatedSprite(wrappedItem.Texture.Name);
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, wrappedItem.SourceRectangle.Left, wrappedItem.SourceRectangle.Top, _uSize);
        }

        public Gatherable(int id, Dictionary<string, string> stringData) : base(id)
        {
            Util.AssignValue(ref _iItemID, "ItemID", stringData);
            LoadDictionaryData(stringData);
        }

        public override void ProcessLeftClick() { Gather(); }
        public override void ProcessRightClick() { Gather(); }

        public void Gather()
        {
            InventoryManager.AddToInventory(DataManager.GetItem(_iItemID));
            MapManager.RemoveWorldObject(this);
            RemoveSelfFromTiles();
        }

        public override bool CanPickUp()
        {
            return true;
        }
    }
}
