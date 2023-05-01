using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class WrappedObjectItem : Item
    {
        int _iObjectID = -1;

        public override int Value => DataManager.GetIntByIDKey(_iObjectID, "Value", DataType.WorldObject);
        public override int BuyPrice => Value;

        public WrappedObjectItem(int objectID) : base(objectID + Constants.BUILDABLE_ID_OFFSET)
        {
            _iNum = 1;
            _iObjectID = objectID;
            _eItemType = ItemEnum.Buildable;

            Point imgPos = DataManager.GetPointByIDKey(_iObjectID, "Image", DataType.WorldObject);
            Point size = DataManager.GetPointByIDKey(_iObjectID, "Size", DataType.WorldObject, new Point (1,1));
            _pSourcePos = imgPos;

            _iWidth = size.X * Constants.TILE_SIZE;
            _iHeight = size.Y * Constants.TILE_SIZE;

            _diReqToMake = DataManager.IntDictionaryFromLookup(_iObjectID, "ReqItems", DataType.WorldObject);

            ObjectTypeEnum type = DataManager.GetEnumByIDKey<ObjectTypeEnum>(_iObjectID, "Type", DataType.WorldObject);

            string texture;
            switch (type)
            {
                case ObjectTypeEnum.Floor:
                    texture = DataManager.FILE_FLOORING;
                    break;
                default:
                    texture = DataManager.FILE_WORLDOBJECTS;
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
            return Name() + System.Environment.NewLine + DataManager.GetTextData(_iObjectID, "Description", DataType.WorldObject);
        }

        public override bool ItemBeingUsed()
        {
            if (GameManager.TownModeEdit())
            {
                if (Number > 1) { Remove(1); }
                else { InventoryManager.RemoveItemFromInventory(this); }

                GameManager.PickUpWorldObject(DataManager.CreateWorldObjectByID(_iObjectID));

                return true;
            }

            return false;
        }

        public override int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(_iObjectID, key, DataType.WorldObject, defaultValue);
        }
    }
}
