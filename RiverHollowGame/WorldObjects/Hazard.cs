using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using Microsoft.Xna.Framework;

namespace RiverHollow.WorldObjects
{

    public class Hazard : WorldObject
    {
        readonly HazardTypeEnum _eHazardType;

        bool _bDrawOver;
        public int Damage { get; }
        public bool Active { get; private set; }
        RHTimer _timer;

        public Hazard(int id, Dictionary<string, string> stringData) : base(id)
        {
            _eObjectType = ObjectTypeEnum.Hazard;
            _eHazardType = Util.ParseEnum<HazardTypeEnum>(stringData["Subtype"]);

            LoadDictionaryData(stringData);
            Damage = int.Parse(stringData["Damage"]);
            Util.AssignValue(ref _bDrawOver, "DrawOver", stringData);
            _bWalkable = true;
            _sprite.SetLayerDepthMod(_bDrawOver ? 1 : -999);

            Activate(_eHazardType == HazardTypeEnum.Passive);

            if(_eHazardType == HazardTypeEnum.Timed)
            {
                _timer = new RHTimer(int.Parse(stringData["Timer"]));
            }
        }

        public override void Update(GameTime gTime)
        {
            if (_eHazardType == HazardTypeEnum.Timed)
            {
                _timer.TickDown(gTime);
                if (_timer.Finished())
                {
                    _timer.Reset();
                    Activate(!Active);
                }
            }

            if (Active && Tiles.Find(x => x.Contains(PlayerManager.PlayerActor)) != null)
            {
                PlayerManager.HazardHarmParty(Damage);
            }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);

            if (_eHazardType != HazardTypeEnum.Passive)
            {
                _sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + TILE_SIZE, _pImagePos.Y, _uSize);
            }
        }

        private void Activate(bool value)
        {
            Active = value;
            switch (_eHazardType)
            {
                case HazardTypeEnum.Passive:
                    _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                    break;
                case HazardTypeEnum.Timed:
                case HazardTypeEnum.Triggered:
                    _sprite.PlayAnimation(value ? AnimationEnum.Action1 : AnimationEnum.ObjectIdle);
                    break;
            }
        }
    }
}
