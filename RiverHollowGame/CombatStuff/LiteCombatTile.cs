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

        CombatActor _character;
        public CombatActor Character => _character;
        GUICombatTile _gTile;
        public GUICombatTile GUITile => _gTile;

        public LiteCombatTile(int row, int col, TargetEnum tileType)
        {
            _iRow = row;
            _iCol = col;
            _tileType = tileType;
        }

        public void SetCombatant(CombatActor c, bool moveCharNow = true)
        {
            _character = c;
            if (c != null)
            {
                if (_character.Tile != null)
                {
                    _character.Tile.SetCombatant(null);
                }
                _character.Tile = this;
            }

            _gTile.SyncGUIObjects(_character != null);
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

            if (_bSelected && CombatManager.SelectedTile != this)
            {
                if (CombatManager.SelectedTile != null) { CombatManager.SelectedTile.Select(false); }
                CombatManager.SelectedTile = this;
            }
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            _gTile.PlayAnimation(animation);
        }
    }
}
