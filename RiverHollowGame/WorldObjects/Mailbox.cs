using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Utilities;
using RiverHollow.GUIComponents;
using System;
using RiverHollow.Map_Handling;

namespace RiverHollow.WorldObjects
{
    public class Mailbox : Buildable
    {
        private AnimatedSprite _alertSprite;

        public Mailbox(int id) : base(id)
        {
            Unique = true;
            _rBase.Y = _pSize.Y - BaseHeight;

            Rectangle animation = GUIUtils.ALERT_ANIMATION;
            _alertSprite = new AnimatedSprite(DataManager.HUD_COMPONENTS);
            _alertSprite.AddAnimation(AnimationEnum.ObjectIdle, animation.X, animation.Y, animation.Width, animation.Height, 3, 0.150f, true);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (TownManager.MailboxHasMessages())
            {
                _alertSprite?.Update(gTime);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (TownManager.MailboxHasMessages())
            {
                _alertSprite?.Draw(spriteBatch, Constants.MAX_LAYER_DEPTH);
            }
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map, ignoreActors);
            _alertSprite.Position = new Point(MapPosition.X, MapPosition.Y - Constants.TILE_SIZE);
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
