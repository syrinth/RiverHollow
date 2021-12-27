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

        public CombatActor SkillUser;

        protected Vector2 _vIconGrid;
        public Vector2 IconGrid => _vIconGrid;

        public LiteMenuAction() { }
        public LiteMenuAction(int id, ActionEnum actionType, Vector2 vGrid)
        {
            _iId = id;
            _eActionType = actionType;
            _vIconGrid = vGrid;
            DataManager.GetTextData("Action", _iId, ref _sName, "Name");
            DataManager.GetTextData("Action", _iId, ref _sDescription, "Description");
        }

        public bool Compare(ActionEnum e) { return _eActionType == e; }

        public bool IsMenu() { return Compare(ActionEnum.MenuSpell) || Compare(ActionEnum.MenuItem) || Compare(ActionEnum.MenuAction); }
    }
}
