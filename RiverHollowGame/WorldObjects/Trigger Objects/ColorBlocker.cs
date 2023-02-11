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
            _eAssignedColor = Util.ParseEnum<ColorStateEnum>(data["Color"]);

            LoadSprite(data);

            if (_eAssignedColor == ColorStateEnum.Red)
            {
                Activate(false);
            }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);
            _sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _pSize);
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
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            }
            else
            {
                _bWalkable = true;
                _sprite.PlayAnimation(AnimationEnum.Action1);
            }
        }
    }
}
