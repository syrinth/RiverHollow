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
        public string TextureKey { get; }
        public ClothingEnum ClothesType { get; }

        public AnimatedSprite Sprite;
        public bool GenderNeutral { get; } = false;

        public Clothing(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            //This is the texture to draw for the inventory representation
            _texTexture = DataManager.GetTexture(@"Textures\Clothing");
            TextureKey = stringData["Key"];

            _bStacks = false;
            int row = 0;

            ClothesType = Util.ParseEnum<ClothingEnum>(stringData["Subtype"]);
            GenderNeutral = stringData.ContainsKey("Neutral");
            row = int.Parse(stringData["Row"]);
        }

        public void SetSpritePosition(Vector2 Position)
        {
            Sprite.Position = Position;
        }

        public bool SlotMatch(ClothingEnum type) { return ClothesType == type; }
    }
}
