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
        string _sTextureAnimationName;
        public string TextureAnimationName => _sTextureAnimationName;
        ClothingEnum _eSlot;
        public ClothingEnum ClothesType => _eSlot;

        public AnimatedSprite Sprite;

        bool _bGenderNeutral = false;
        public bool GenderNeutral => _bGenderNeutral;

        public Clothing(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            //This is the texture to draw for the inventory representation
            _texTexture = DataManager.GetTexture(@"Textures\items");
            _sTextureAnimationName = stringData["TexName"];

            _bStacks = false;
            int row = 0;

            _eSlot = Util.ParseEnum<ClothingEnum>(stringData["Subtype"]);
            _bGenderNeutral = stringData.ContainsKey("Neutral");
            row = int.Parse(stringData["Row"]);
        }

        public void SetSpritePosition(Vector2 Position)
        {
            Sprite.Position = Position;
        }

        public bool SlotMatch(ClothingEnum type) { return _eSlot == type; }
    }
}
