using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
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
            _liSprites = new List<GUISprite>
            {
                new GUISprite(actor.BodySprite),
                new GUISprite(actor.ArmSprite)
            };
            _liSprites.ForEach(x => x.PositionAndMove(this, _pMoveBy));

            var feetCosmetic = actor.GetAppliedCosmetic(CosmeticSlotEnum.Feet);
            var legCosmetic = actor.GetAppliedCosmetic(CosmeticSlotEnum.Legs);
            var bodyCosmetic = actor.GetAppliedCosmetic(CosmeticSlotEnum.Body);
            var cosmetics = new List<AppliedCosmetic>
            {
                actor.GetAppliedCosmetic(CosmeticSlotEnum.Eyes),
                actor.GetAppliedCosmetic(CosmeticSlotEnum.Hair),
                actor.GetAppliedCosmetic(CosmeticSlotEnum.Head),
                feetCosmetic,
                legCosmetic,
                bodyCosmetic
            };

            CheckLayering(ref cosmetics, feetCosmetic, legCosmetic);
            CheckLayering(ref cosmetics, legCosmetic, bodyCosmetic);

            foreach (var c in cosmetics)
            {
                AddCosmetic(c);
            }
            
            _liSprites.ForEach(x => x.SetScale(GameManager.CurrentScale * 2));

            PlayAnimation(_eLastVerb, _eLastDir);
        }

        private void CheckLayering(ref List<AppliedCosmetic> cosmetics, AppliedCosmetic lower, AppliedCosmetic top)
        {
            if (lower.MyCosmetic == null || top.MyCosmetic == null)
            {
                return;
            }

            if (lower.MyCosmetic.DrawAbove && !top.MyCosmetic.DrawAbove)
            {
                var lowerIndex = cosmetics.FindIndex(x => x == lower);
                var upperIndex = cosmetics.FindIndex(x => x == top);

                var temp = cosmetics[upperIndex];
                cosmetics[upperIndex] = lower;
                cosmetics[lowerIndex] = temp;
            }
        }

        private void AddCosmetic(AppliedCosmetic c)
        {
            if (c.MyCosmetic != null)
            {
                GUISprite spr = new GUISprite(c.MySprite, true);
                spr.PositionAndMove(this, _pMoveBy);

                int mod = 0;
                switch (c.MyCosmetic.CosmeticSlot)
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
                    case CosmeticSlotEnum.Feet:
                        mod = Constants.PLAYER_FEET_OFFSET;
                        break;
                }

                spr.ScaledMoveBy(0, mod * 2);
                _liSprites.Add(spr);
            }
        }

        public void PlayAnimation(VerbEnum verb, DirectionEnum dir) {
            _eLastVerb = verb;
            _eLastDir = dir;

            _liSprites.ForEach(x =>
            {
                x.PlayAnimation(_eLastVerb, _eLastDir);
                x.PlayAnimation(dir);
            });
        }
    }
}
