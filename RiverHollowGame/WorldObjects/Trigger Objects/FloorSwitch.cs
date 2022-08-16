using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects.Trigger_Objects
{
    class FloorSwitch : Trigger
    {
        private bool _bHoldDown = false;
        private WorldObject _objOnMe;

        public FloorSwitch(int id, Dictionary<string, string> dataString) : base(id, dataString) {
            _bDrawUnder = true;
            _bWalkable = true;

            _ePlacement = ObjectPlacementEnum.Floor;

            Util.AssignValue(ref _bHoldDown, "HoldDown", dataString);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

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
                else { Reset(); }
            }
        }

        private bool HeldDown()
        {
            if (_objOnMe == null) { _objOnMe = Tiles[0].WorldObject; }
            return CollisionBox.Contains(PlayerManager.PlayerActor.CollisionCenter) || (_objOnMe != null && CollisionBox.Contains(_objOnMe.CollisionCenter));
        }

        public override void ProcessLeftClick() { }
        public override void ProcessRightClick() { }
    }
}
