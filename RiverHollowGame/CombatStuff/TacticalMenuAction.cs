//using Microsoft.Xna.Framework;
//using RiverHollow.Game_Managers;

//using static RiverHollow.Game_Managers.GameManager;

//namespace RiverHollow.CombatStuff
//{
//    public class TacticalMenuAction
//    {
//        protected int _iId;

//        protected ActionEnum _eActionType;
//        protected string _sName;
//        public string Name => _sName;
//        protected string _sDescription;
//        public string Description => _sDescription;

//        protected Vector2 _vIconGrid;
//        public Vector2 IconGrid => _vIconGrid;

//        public TacticalMenuAction() { }
//        public TacticalMenuAction(int id, ActionEnum actionType, Vector2 vGrid)
//        {
//            _iId = id;
//            _eActionType = actionType;
//            _vIconGrid = vGrid;
//            DataManager.GetTextData("Action", _iId, ref _sName, "Name");
//            DataManager.GetTextData("Action", _iId, ref _sDescription, "Description");
//        }

//        public bool IsActionMenu() { return _eActionType == ActionEnum.MenuAction; }
//        public bool IsSpellMenu() { return _eActionType == ActionEnum.MenuSpell; }
//        public bool IsUseItem() { return _eActionType == ActionEnum.MenuItem; }
//        public bool IsEndTurn() { return _eActionType == ActionEnum.EndTurn; }
//        public bool IsMove() { return _eActionType == ActionEnum.Move; }

//        public bool IsAction() { return _eActionType == ActionEnum.Action; }
//        public bool IsSpell() { return _eActionType == ActionEnum.Spell; }
//    }
//}
