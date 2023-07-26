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

        public ClothingEnum ClothingType => GetEnumByIDKey<ClothingEnum>("Subtype");
        public bool GenderNeutral => GetBoolByIDKey("Neutral");

        public Clothing(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            switch (ClothingType)
            {
                case ClothingEnum.Hat:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Hats");
                    break;
                case ClothingEnum.Shirt:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Shirts");
                    break;
                case ClothingEnum.Pants:
                    _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Pants");
                    break;
            }           
        }

        public void SetSpritePosition(Point p)
        {
            Sprite.Position = p;
        }

        public bool SlotMatch(ClothingEnum type) { return ClothingType == type; }
    }
}
