using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Misc;
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

        public PlayerDisplayBox(Rectangle pane, Point moveBy) : base (pane)
        {
            _pMoveBy = moveBy;

            SyncSprites();
        }

        public void SyncSprites()
        {
            CleanControls();

            var actor = PlayerManager.PlayerActor;
            _liSprites = new List<GUISprite>();
            foreach (var c in actor.GetCosmetics())
            {
                AddCosmetic(c);
            }

            PlayAnimation(_eLastVerb, _eLastDir);
        }

        private void AddCosmetic(Cosmetic c)
        {
            if (c != null)
            {
                GUISprite spr = new GUISprite(c.GetSprite(), true);
                spr.PositionAndMove(this, _pMoveBy);

                int mod = 0;
                switch (c.CosmeticSlot)
                {
                    case CosmeticSlotEnum.Head:
                        mod = Constants.PLAYER_HAT_OFFSET;
                        break;
                    case CosmeticSlotEnum.Body:
                        mod = Constants.PLAYER_SHIRT_OFFSET;
                        break;
                    case CosmeticSlotEnum.Legs:
                        mod = Constants.PLAYER_PANTS_OFFSET;
                        break;
                }

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
