using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class WrappedItem : WorldObject
    {
        readonly int _iItemID;
        public int ItemID => _iItemID;

        public WrappedItem(int itemID) : base(-1)
        {
            _iItemID = itemID;

            Item wrappedItem = DataManager.GetItem(itemID);
            Sprite = new AnimatedSprite(wrappedItem.Texture.Name);
            if (DataManager.GetItemDictionaryData(itemID).ContainsKey("WrappedImage"))
            {
                string[] split = Util.FindArguments(DataManager.GetItemDictionaryData(itemID)["WrappedImage"]);
                Sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, int.Parse(split[0]), int.Parse(split[1]), _pSize);
            }
            else { Sprite.AddAnimation(AnimationEnum.ObjectIdle, wrappedItem.SourceRectangle.Left, wrappedItem.SourceRectangle.Top, _pSize); }
        }

        public WrappedItem(int id, Dictionary<string, string> stringData) : base(id)
        {
            _iItemID = Util.AssignValue("ItemID", stringData);
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

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = new WorldObjectData
            {
                ID = ID,
                X = CollisionBox.X,
                Y = CollisionBox.Y,
                stringData = _iItemID.ToString()
            };

            return data;
        }
    }
}
