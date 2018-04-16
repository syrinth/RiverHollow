using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using System;
using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.Characters.NPCs
{
    public class Spirit : WorldCharacter
    {
        private const float MIN_VISIBILITY = 0.05f;
        private float _fVisibility;
        public bool _bTriggered;

        public Spirit() : base()
        {
            _characterType = CharacterEnum.Spirit;
            _fVisibility = MIN_VISIBILITY;
            LoadContent(@"Textures\NPCs\Spirit_Forest_1");
        }

        public override void LoadContent(string textureToLoad)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.AddAnimation("Idle", TileSize, TileSize, 2, 0.6f, 0, 0);
            _sprite.SetCurrentAnimation("Idle");

            _width = _sprite.Width;
            _height = _sprite.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if (!_bTriggered)
            {
                int max = TileSize * 13;
                int dist = 0;
                if (PlayerManager.CurrentMap == CurrentMapName && PlayerManager.PlayerInRangeGetDist(_sprite.Center.ToPoint(), max, ref dist))
                {
                    float fMax = max;
                    float fDist = dist;
                    float percentage = (Math.Abs(dist - fMax)) / fMax;
                    percentage = Math.Max(percentage, MIN_VISIBILITY);
                    _fVisibility = 0.4f * percentage;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _sprite.Draw(spriteBatch, useLayerDepth, _fVisibility);
        }

        public void Talk()
        {
            _bTriggered = true;
            _fVisibility = 1.0f;
        }
    }
}
