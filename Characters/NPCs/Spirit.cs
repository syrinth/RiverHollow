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
        const float MIN_VISIBILITY = 0.05f;
        float _fVisibility;
        string _sType;
        string _sCondition;

        bool _bActive;
        public bool Active => _bActive;
        public bool Triggered;

        public Spirit(string name, string type, string condition) : base()
        {
            _characterType = CharacterEnum.Spirit;
            _fVisibility = MIN_VISIBILITY;

            _sName = name;
            _sType = type;
            _sCondition = condition;
            _bActive = false;

            LoadContent(@"Textures\NPCs\Spirit_" + _sType);
        }

        public override void LoadContent(string textureToLoad)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.AddAnimation("Idle", 16, 18, 2, 0.6f, 0, 0);
            _sprite.SetCurrentAnimation("Idle");

            _width = _sprite.Width;
            _height = _sprite.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            if (_bActive)
            {
                base.Update(theGameTime);
                if (!Triggered)
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
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bActive)
            {
                _sprite.Draw(spriteBatch, useLayerDepth, _fVisibility);
            }
        }

        public void CheckCondition()
        {
            bool active = false;
            string[] splitCondition = _sCondition.Split('/');
            foreach (string s in splitCondition)
            {
                if (s.Equals("Raining"))
                {
                    active = GameCalendar.IsRaining();
                }
                else if (s.Contains("day"))
                {
                    active = s.Equals(GameCalendar.GetDayOfWeek());
                }
                else if (s.Contains("Season"))
                {
                    string[] seasonsplit = s.Split(':');
                    active = seasonsplit[1].Equals(GameCalendar.GetSeason());
                }
                else if (s.Equals("Night"))
                {
                    active = GameCalendar.IsNight();
                }

                if (!active) { break; }
            }

            _bActive = active;
            Triggered = false;
        }
        public void Talk()
        {
            if (_bActive)
            {
                Triggered = true;
                _fVisibility = 1.0f;
            }
        }
    }
}
