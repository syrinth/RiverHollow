using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class LiteCombatTile
    {
        TargetEnum _tileType;
        public TargetEnum TargetType => _tileType;

        int _iRow;
        public int Row => _iRow;
        int _iCol;
        public int Col => _iCol;

        bool _bSelected;
        public bool Selected => _bSelected;

        LiteCombatActor _character;
        public LiteCombatActor Character => _character;
        GUICombatTile _gTile;
        public GUICombatTile GUITile => _gTile;

        public LiteCombatTile(int row, int col, TargetEnum tileType)
        {
            _iRow = row;
            _iCol = col;
            _tileType = tileType;
        }

        public void SetCombatant(LiteCombatActor c, bool moveCharNow = true)
        {
            _character = c;
            if (c != null)
            {
                if (_character.Tile != null)
                {
                    _character.Tile.SetCombatant(null);
                }
                if (_character.Tile != null)
                {
                    foreach (LiteCombatTile tile in LiteCombatManager.GetAdjacent(_character.Tile))
                    {
                        CheckForProtected(tile);
                    }
                }
                _character.Tile = this;
                CheckForProtected(this);
            }

            _gTile.SyncGUIObjects(_character != null);
            if (_character != null)
            {
                foreach (KeyValuePair<ConditionEnum, bool> kvp in _character.DiConditions)
                {
                    if (kvp.Value)
                    {
                        GUITile.ChangeCondition(kvp.Key, TargetEnum.Enemy);
                    }
                }
            }
        }

        private void CheckForProtected(LiteCombatTile t)
        {
            bool found = false;
            List<LiteCombatTile> adjacent = LiteCombatManager.GetAdjacent(t);
            foreach (LiteCombatTile tile in adjacent)
            {
                if (tile.Occupied() && this.TargetType == tile.TargetType)
                {
                    if (tile.Character != this.Character && tile.Character.IsActorType(ActorEnum.PartyMember) && this.Character.IsActorType(ActorEnum.PartyMember))
                    {
                        found = true;
                        LitePartyMember adv = (LitePartyMember)tile.Character;
                        adv.Protected = true;
                        adv = (LitePartyMember)this.Character;
                        adv.Protected = true;
                    }
                }
            }

            if (!found && this.Character.IsActorType(ActorEnum.PartyMember))
            {
                LitePartyMember adv = (LitePartyMember)this.Character;
                adv.Protected = false;
            }
        }

        public void AssignGUITile(GUICombatTile c)
        {
            _gTile = c;
        }

        public bool Occupied()
        {
            return _character != null;
        }

        public void Select(bool val)
        {
            _bSelected = val;

            if (_bSelected && LiteCombatManager.SelectedTile != this)
            {
                if (LiteCombatManager.SelectedTile != null) { LiteCombatManager.SelectedTile.Select(false); }
                LiteCombatManager.SelectedTile = this;
            }
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            _gTile.PlayAnimation(animation);
        }
    }
}
