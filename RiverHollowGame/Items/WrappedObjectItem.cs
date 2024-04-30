using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class WrappedObjectItem : Item
    {
        int _iObjectID = -1;

        public override int Value => DataManager.GetIntByIDKey(_iObjectID, "Value", DataType.WorldObject);

        public WrappedObjectItem(int objectID) : base(objectID + Constants.BUILDABLE_ID_OFFSET)
        {
            _iNum = 1;
            _iObjectID = objectID;
            _eItemType = ItemTypeEnum.Buildable;

            Point imgPos = DataManager.GetPointByIDKey(_iObjectID, "Image", DataType.WorldObject);
            Point size = DataManager.GetPointByIDKey(_iObjectID, "Size", DataType.WorldObject, new Point (1,1));
            _pSourcePos = Util.MultiplyPoint(imgPos, Constants.TILE_SIZE);

            _iWidth = size.X * Constants.TILE_SIZE;
            _iHeight = size.Y * Constants.TILE_SIZE;

            _diReqToMake = DataManager.IntDictionaryFromLookup(_iObjectID, "ReqItems", DataType.WorldObject);

            BuildableEnum type = DataManager.GetEnumByIDKey<BuildableEnum>(_iObjectID, "Subtype", DataType.WorldObject);

            string texture;
            switch (type)
            {
                case BuildableEnum.Floor:
                    texture = DataManager.FILE_FLOORING;
                    break;
                case BuildableEnum.Wall:
                    texture = DataManager.FILE_WALLS;
                    break;
                default:
                    texture = DataManager.FILE_DECOR;
                    break;
            }
            _texTexture = DataManager.GetTexture(texture);
        }

        public override string Name()
        {
            return DataManager.GetTextData(_iObjectID, "Name", DataType.WorldObject);
        }
        public override string Description()
        {
            return DataManager.GetTextData(_iObjectID, "Description", DataType.WorldObject);
        }

        public override bool ItemBeingUsed()
        {
            GameManager.EnterTownModeEdit();

            if (Number > 1) { Remove(1); }
            else { InventoryManager.RemoveItemFromInventory(this); }

            GameManager.PickUpWorldObject(DataManager.CreateWorldObjectByID(_iObjectID));

            return true;
        }

        public override int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(_iObjectID, key, DataType.WorldObject, defaultValue);
        }

        public override TEnum GetEnumByIDKey<TEnum>(string key)
        {
            return DataManager.GetEnumByIDKey<TEnum>(_iObjectID, key, DataType.WorldObject);
        }
    }
}
