using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    class ColorBlocker : TriggerObject
    {
        private bool _bActive = false;
        ColorStateEnum _eAssignedColor;
        public ColorBlocker(int id, Dictionary<string, string> data) : base(id, data)
        {
            _eAssignedColor = GetEnumByIDKey<ColorStateEnum>("Color");

            Activate(_eAssignedColor != ColorStateEnum.Red);
        }

        protected override void LoadSprite()
        {
            base.LoadSprite();
            Sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X, _pImagePos.Y + Height, _pSize);
        }

        public override void AttemptToTrigger(string name)
        {
            ColorStateEnum trigger = Util.ParseEnum<ColorStateEnum>(name);
            if(trigger != ColorStateEnum.None)
            {
                Activate(trigger == _eAssignedColor);
            }
        }

        private void Activate(bool value)
        {
            _bActive = value;
            if (_bActive)
            {
                _bWalkable = false;
                _bDrawUnder = false;
                Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            }
            else
            {
                _bWalkable = true;
                _bDrawUnder = GetBoolByIDKey("DrawUnder");
                Sprite.PlayAnimation(AnimationEnum.Action1);
            }
        }
    }
}
