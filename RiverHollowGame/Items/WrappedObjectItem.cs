using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Security.Cryptography;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class WrappedObjectItem : Item
    {
        int _iObjectID = -1;

        public override int Value => DataManager.GetIntByIDKey(_iObjectID, "Value", DataType.WorldObject);
        public override int BuyPrice => Value;

        public WrappedObjectItem(int objectID) : base(objectID + Constants.FURNITURE_ID_OFFSET)
        {
            _iNum = 1;
            _iObjectID = objectID;
            _eItemType = ItemEnum.Furniture;

            Point imgPos = DataManager.GetPointByIDKey(_iObjectID, "Image", DataType.WorldObject);
            Point size = DataManager.GetPointByIDKey(_iObjectID, "Size", DataType.WorldObject);
            _pSourcePos = imgPos;

            _iWidth = size.X * Constants.TILE_SIZE;
            _iHeight = size.Y * Constants.TILE_SIZE;

            _texTexture = DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS);
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
            InventoryManager.RemoveItemFromInventory(this);
            GameManager.MovingWorldObject(DataManager.CreateWorldObjectByID(_iObjectID));

            return true;
        }
    }
}
