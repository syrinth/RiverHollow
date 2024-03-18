using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class ActorEmoji
    {
        public bool Finished { get; private set; }
        public Actor Villager { get; private set; }
        public ActorEmojiEnum Emoji { get; private set; }

        private float _fAlpha = 1f;
        readonly RHTimer _rhTimer;

        public ActorEmoji(ActorEmojiEnum e, Actor v, bool randomize = false)
        {
            Emoji = e;
            Villager = v;
            _rhTimer = new RHTimer(randomize ? RHRandom.Instance().Next(1, 3) : 1);
        }

        public virtual void Update(GameTime gTime)
        {
            if (_rhTimer.TickDown(gTime))
            {
                if (_fAlpha > 0f)
                {
                    _fAlpha -= Emoji == ActorEmojiEnum.Sing ? 0.1f : 0.05f;
                }
                else
                {
                    _fAlpha = 0f;
                }
            }

            if (_fAlpha == 0)
            {
                Finished = true;
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var emojiBox = Rectangle.Empty;
            switch (Emoji)
            {
                case ActorEmojiEnum.Dots:
                    emojiBox = Constants.RECTANGLE_EMOJI_DOTS;
                    break;
                case ActorEmojiEnum.Happy:
                    emojiBox = Constants.RECTANGLE_EMOJI_HAPPY;
                    break;
                case ActorEmojiEnum.Heart:
                    emojiBox = Constants.RECTANGLE_EMOJI_HEART;
                    break;
                case ActorEmojiEnum.Sing:
                    emojiBox = Constants.RECTANGLE_EMOJI_SING;
                    break;
                case ActorEmojiEnum.Sleepy:
                    emojiBox = Constants.RECTANGLE_EMOJI_SLEEPY;
                    break;
                case ActorEmojiEnum.Talk:
                    emojiBox = Constants.RECTANGLE_EMOJI_TALK;
                    break;

 
            } 

            var drawBox = new Rectangle(Villager.GetHoverPointLocation(), Constants.BasicTileSize);
            spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_MISC_SPRITES), drawBox, emojiBox, Color.White * _fAlpha, 0f, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
        }
    }
}
