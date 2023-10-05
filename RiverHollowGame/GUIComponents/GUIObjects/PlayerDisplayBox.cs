using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;

using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class PlayerDisplayBox : GUIImage
    {
        VerbEnum _eLastVerb = VerbEnum.Idle;
        DirectionEnum _eLastDir = DirectionEnum.Down;

        Point _pMoveBy;

        List<GUISprite> _liSprites;
        Item[] _arrClothing;

        public PlayerDisplayBox(Rectangle pane, Point moveBy) : base (pane)
        {
            _pMoveBy = moveBy;

            _arrClothing = new Item[Constants.PLAYER_GEAR_ROWS];
            _arrClothing[0] = PlayerManager.PlayerActor.Hat;
            _arrClothing[1] = PlayerManager.PlayerActor.Shirt;
            _arrClothing[2] = PlayerManager.PlayerActor.Pants;

            SyncSprites();
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_arrClothing[0] != PlayerManager.PlayerActor.Hat ||
            _arrClothing[1] != PlayerManager.PlayerActor.Shirt ||
            _arrClothing[2] != PlayerManager.PlayerActor.Pants)
            {
                SyncSprites();

                _arrClothing[0] = PlayerManager.PlayerActor.Hat;
                _arrClothing[1] = PlayerManager.PlayerActor.Shirt;
                _arrClothing[2] = PlayerManager.PlayerActor.Pants;
            }
        }

        public void SyncSprites()
        {
            CleanControls();

            var actor = PlayerManager.PlayerActor;
            _liSprites = new List<GUISprite>();
            for (int i = 0; i < actor.GetSprites().Count; i++)
            {
                AddSprite(actor.GetSprites()[i]);
            }

            PlayAnimation(_eLastVerb, _eLastDir);
        }

        private void AddSprite(AnimatedSprite sprite)
        {
            if (sprite != null)
            {
                GUISprite spr = new GUISprite(sprite, true);
                spr.PositionAndMove(this, _pMoveBy);

                int mod = 0;
                if (sprite == PlayerManager.PlayerActor.HatSprite) { mod = Constants.PLAYER_HAT_OFFSET; }
                else if (sprite == PlayerManager.PlayerActor.ShirtSprite) { mod = Constants.PLAYER_SHIRT_OFFSET; }
                else if (sprite == PlayerManager.PlayerActor.PantsSprite) { mod = Constants.PLAYER_PANTS_OFFSET; }

                spr.ScaledMoveBy(0, mod);
                _liSprites.Add(spr);
            }
        }

        public void PlayAnimation(VerbEnum verb, DirectionEnum dir) {
            _eLastVerb = verb;
            _eLastDir = dir;

            for (int i = 0; i < _liSprites.Count; i++)
            {
                _liSprites[i].PlayAnimation(_eLastVerb, _eLastDir);
                _liSprites[i].PlayAnimation(dir);
            }
        }
    }
}
