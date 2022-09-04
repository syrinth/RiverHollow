using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class WrappedItem : WorldObject
    {
        readonly int _iItemID;
        public int ItemdID => _iItemID;

        public WrappedItem(int itemID) : base(-1)
        {
            _iItemID = itemID;

            Item wrappedItem = DataManager.GetItem(itemID);
            _sprite = new AnimatedSprite(wrappedItem.Texture.Name);
            if (DataManager.GetItemDictionaryData(itemID).ContainsKey("WrappedImage"))
            {
                string[] split = Util.FindArguments(DataManager.GetItemDictionaryData(itemID)["WrappedImage"]);
                _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, int.Parse(split[0]), int.Parse(split[1]), _uSize);
            }
            else { _sprite.AddAnimation(AnimationEnum.ObjectIdle, wrappedItem.SourceRectangle.Left, wrappedItem.SourceRectangle.Top, _uSize); }
        }

        public WrappedItem(int id, Dictionary<string, string> stringData) : base(id)
        {
            Util.AssignValue(ref _iItemID, "ItemID", stringData);
            LoadDictionaryData(stringData);
        }

        public override bool ProcessLeftClick() { return Gather(); }
        public override bool ProcessRightClick() { return Gather(); }

        public bool Gather()
        {
            CurrentMap.AlertSpawnPoint(this);
            InventoryManager.AddToInventory(DataManager.GetItem(_iItemID));
            MapManager.RemoveWorldObject(this);
            RemoveSelfFromTiles();

            return true;
        }

        public override bool CanPickUp()
        {
            return true;
        }
    }
}
