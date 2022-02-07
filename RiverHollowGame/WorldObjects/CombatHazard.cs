using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.WorldObjects
{

    public class CombatHazard : WorldObject
    {
        public enum HazardTypeEnum { Passive, Timed, Triggered };
        readonly HazardTypeEnum _eHazardType;

        int _iInit;
        bool _bDrawOver;
        public int Damage { get; }
        public bool Active { get; private set; }

        public CombatHazard(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData);
            _eHazardType = Util.ParseEnum<HazardTypeEnum>(stringData["Subtype"]);
            Damage = int.Parse(stringData["Damage"]);
            Util.AssignValue(ref _bDrawOver, "DrawOver", stringData);
            _sprite.SetLayerDepthMod(_bDrawOver ? 1 : -999);

            _iInit = 0;
            if (_eHazardType == HazardTypeEnum.Passive) { Active = true; }
            else { Activate(false); }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);
            _sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + TILE_SIZE, _pImagePos.Y, _uSize);
        }

        public bool Charge()
        {
            bool rv = false;

            _iInit += 3;

            if (_iInit >= 100)
            {
                _iInit = 0;
                Activate(!Active);
            }

            return rv;
        }

        private void Activate(bool value)
        {
            Active = value;
            _sprite.PlayAnimation(value ? AnimationEnum.Action1 : AnimationEnum.ObjectIdle);
        }

        public bool SubtypeMatch(HazardTypeEnum cmp)
        {
            return _eHazardType == cmp;
        }
    }
}
