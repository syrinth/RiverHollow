using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    /// <summary>
    /// Buildable represent WorldObjects that are built by the player
    /// </summary>
    public class Buildable : WorldObject
    {
        int _iValue = 0;
        public int Value => _iValue;

        protected Dictionary<int, int> _diReqToMake;
        public Dictionary<int, int> RequiredToMake => _diReqToMake;

        public bool OutsideOnly { get; protected set; } = false;
        protected bool _bSelected = false;

        public bool Unique { get; protected set; }

        protected Buildable(int id) : base(id) { }

        public Buildable(int id, Dictionary<string, string> stringData) : base(id)
        {
            _rBase.Y = _pSize.Y - BaseHeight;

            if (stringData.ContainsKey("Unique")) { Unique = true; }
            LoadDictionaryData(stringData);
        }

        protected override void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            base.LoadDictionaryData(stringData, loadSprite);

            _iValue = Util.AssignValue("Value", stringData);
            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Sprite.SetColor(_bSelected ? Color.Green : Color.White);
            base.Draw(spriteBatch);
        }

        public void SelectObject(bool val) { _bSelected = val; }

        public bool CanBuild()
        {
            if (Unique && TownManager.GetNumberTownObjects(ID) != 0)
            {
                return false;
            }

            return !OutsideOnly || (OutsideOnly && MapManager.CurrentMap.IsOutside);
        }

        public override bool ProcessLeftClick()
        {
            bool rv = false;
            if (GameManager.HeldObject == null)
            {
                rv = true;
                CurrentMap.RemoveWorldObject(this, true);
                GameManager.MovingWorldObject(this);
            }

            return rv;
        }
    }
}
