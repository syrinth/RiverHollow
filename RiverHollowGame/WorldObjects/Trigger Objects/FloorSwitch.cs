using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects.Trigger_Objects
{
    class FloorSwitch : Trigger
    {
        private bool _bOnlyOnce = false;
        private bool _bHoldDown = false;
        private WorldObject _trackedObject;

        public FloorSwitch(int id, Dictionary<string, string> dataString) : base(id, dataString) {
            _bDrawUnder = true;
            _bWalkable = true;

            _ePlacement = ObjectPlacementEnum.Floor;

            Util.AssignValue(ref _bHoldDown, "HoldDown", dataString);
            Util.AssignValue(ref _bOnlyOnce, "OnlyOnce", dataString);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if(_bHasBeenTriggered && _bOnlyOnce) { return; }

            if (_bHoldDown)
            {
                if (HeldDown())
                {
                    if (!_bHasBeenTriggered) { FireTrigger(); }
                }
                else
                {
                    if (_bHasBeenTriggered) { FireTrigger(); }
                }
            }
            else
            {
                if (HeldDown())
                {
                    if (!_bHasBeenTriggered) { FireTrigger(); }
                }
            }
        }

        public override void FireTrigger()
        {
            SoundManager.PlayEffect(SoundEffectEnum.GrindStone);
            base.FireTrigger();
        }

        public override void ResetTrigger()
        {
            SoundManager.PlayEffect(SoundEffectEnum.Switch);
            base.ResetTrigger();
        }

        private bool HeldDown()
        {
            //We normally will be pushing objects onto switches, but we might create an object directly on top of one, which needs to be tracked as well.
            if (_trackedObject == null)
            {
                if (PlayerManager.GrabbedObject != null)
                {
                    _trackedObject = PlayerManager.GrabbedObject;
                }
                else
                {
                    _trackedObject = FirstTile().WorldObject;
                }
            }
            else if (!CollisionBox.Contains(_trackedObject.CollisionCenter)) { _trackedObject = null; }

            return CollisionBox.Contains(PlayerManager.PlayerActor.CollisionCenter) || (_trackedObject != null && CollisionBox.Contains(_trackedObject.CollisionCenter));
        }
    }
}
