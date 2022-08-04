using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects.Trigger_Objects
{
    class ColorSwitch : Trigger
    {
        ColorStateEnum _eCurrentState;
        public ColorSwitch(int id, Dictionary<string, string> data): base(id, data)
        {
            _eCurrentState = ColorStateEnum.Blue;
        }

        public override void FireTrigger()
        {
            if (CanTrigger())
            {
                if(_eCurrentState == ColorStateEnum.Blue) { _eCurrentState = ColorStateEnum.Red; }
                else if (_eCurrentState == ColorStateEnum.Red) { _eCurrentState = ColorStateEnum.Blue; }

                if (_sprite.CurrentAnimation == AnimationEnum.Action1.ToString()) { _sprite.PlayAnimation(AnimationEnum.ObjectIdle); }
                else { _sprite.PlayAnimation(AnimationEnum.Action1); }

                if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_eCurrentState.ToString()); }
                else { GameManager.ActivateTriggers(_eCurrentState.ToString()); }
            }
        }
    }
}
