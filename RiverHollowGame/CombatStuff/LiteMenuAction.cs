using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Actors.CombatStuff
{
    public class LiteMenuAction
    {
        protected int _iId;

        protected ActionEnum _eActionType;
        protected string _sName;
        public string Name { get => _sName; }
        protected string _sDescription;
        public string Description { get => _sDescription; }

        public LiteCombatActor SkillUser;

        protected Vector2 _vIconGrid;
        public Vector2 IconGrid => _vIconGrid;

        public LiteMenuAction() { }
        public LiteMenuAction(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id);
        }

        protected void ImportBasics(Dictionary<string, string> stringData, int id)
        {
            _iId = id;
            DataManager.GetTextData("Action", _iId, ref _sName, "Name");
            DataManager.GetTextData("Action", _iId, ref _sDescription, "Description");

            _eActionType = Util.ParseEnum<ActionEnum>(stringData["Type"]);

            string[] tags = stringData["Icon"].Split('-');
            _vIconGrid = new Vector2(int.Parse(tags[0]), int.Parse(tags[1]));
        }

        public bool IsMenu() { return _eActionType == ActionEnum.MenuAction; }
        public bool IsAction() { return _eActionType == ActionEnum.Action; }
        public bool IsSpell() { return _eActionType == ActionEnum.Spell; }

        public bool IsSpecial() { return _eActionType == ActionEnum.MenuSpell; }
        public bool IsUseItem() { return _eActionType == ActionEnum.MenuItem; }
    }
}
