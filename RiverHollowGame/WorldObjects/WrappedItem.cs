using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;

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
            LoadSprite();
        }

        protected override void LoadSprite()
        {
            Item wrappedItem = DataManager.GetItem(_iItemID);
            Sprite = new AnimatedSprite(wrappedItem.Texture.Name);
            if (DataManager.GetBoolByIDKey(_iItemID, "WrappedImage", DataType.Item))
            {
                string[] split = DataManager.GetStringParamsByIDKey(_iItemID, "WrappedImage", DataType.Item);
                Sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, int.Parse(split[0]), int.Parse(split[1]), _pSize);
            }
            else { Sprite.AddAnimation(AnimationEnum.ObjectIdle, wrappedItem.SourceRectangle.Left, wrappedItem.SourceRectangle.Top, _pSize); }
        }

        public override bool ProcessLeftClick() { return Gather(); }
        public override bool ProcessRightClick() { return Gather(); }

        public bool Gather()
        {
            if (InventoryManager.HasSpaceInInventory(_iItemID, 1))
            {
                CurrentMap.AlertSpawnPoint(this);
                MapManager.RemoveWorldObject(this, true);
                RemoveSelfFromTiles();
                InventoryManager.AddToInventory(DataManager.GetItem(_iItemID));
            }

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
