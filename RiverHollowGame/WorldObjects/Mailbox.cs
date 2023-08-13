using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.GUIComponents;
using RiverHollow.Map_Handling;

using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;

namespace RiverHollow.WorldObjects
{
    public class Mailbox : Buildable
    {
        private AnimatedSprite _alertSprite;
        private RHTimer _timer;
        private bool _bBounce;

        public Mailbox(int id) : base(id)
        {
            Unique = true;
            _rBase.Y = _pSize.Y - BaseHeight;

            Rectangle animation = GUIUtils.ICON_EXCLAMATION;
            _alertSprite = new AnimatedSprite(DataManager.HUD_COMPONENTS);
            _alertSprite.AddAnimation(AnimationEnum.ObjectIdle, animation.X, animation.Y, animation.Width, animation.Height, 1, 0.3f, true);
            _alertSprite.SetLinkedSprite(Sprite, false);

            _timer = new RHTimer(0.5);
            _bBounce = false;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (AlertVisible() && _timer.TickDown(gTime, true))
            {
                _alertSprite.Position += new Point(0, _bBounce ? 1 : -1);
                _bBounce = !_bBounce;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (AlertVisible())
            {
                _alertSprite?.Draw(spriteBatch);
            }
        }

        private bool AlertVisible()
        {
            return TownManager.MailboxHasMessages() && GameManager.HeldObject != this;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map, ignoreActors);
            _alertSprite.Position = new Point(MapPosition.X + 6, MapPosition.Y + 1);
            return rv;
        }

        public override bool ProcessRightClick()
        {
            TextEntry entry = TownManager.MailboxTakeMessage();
            if (entry != null)
            {
                GUIManager.OpenTextWindow(entry);
            }

            return true;
        }
    }
}
