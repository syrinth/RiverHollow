using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Clothing : Item
    {
        public AnimatedSprite Sprite;

        public EquipmentEnum ClothingType => GetEnumByIDKey<EquipmentEnum>("Subtype");
        public bool GenderNeutral => GetBoolByIDKey("Neutral");

        public Clothing(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            switch (ClothingType)
            {
                case EquipmentEnum.Hat:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Hats");
                    break;
                case EquipmentEnum.Shirt:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Shirts");
                    break;
                case EquipmentEnum.Pants:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Pants");
                    break;
            }           
        }

        public void SetSpritePosition(Point p)
        {
            Sprite.Position = p;
        }

        public bool SlotMatch(EquipmentEnum type) { return ClothingType == type; }
    }
}
