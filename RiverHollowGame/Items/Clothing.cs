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
        public string TextureKey { get; }

        public ClothingEnum ClothingType => GetEnumByIDKey<ClothingEnum>("Subtype");
        public bool GenderNeutral => GetBoolByIDKey("Neutral");

        public Clothing(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            //This is the texture to draw for the inventory representation
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Clothing");
            TextureKey = stringData["Key"];
            int row = int.Parse(stringData["Row"]);
        }

        public void SetSpritePosition(Point p)
        {
            Sprite.Position = p;
        }

        public bool SlotMatch(ClothingEnum type) { return ClothingType == type; }
    }
}
