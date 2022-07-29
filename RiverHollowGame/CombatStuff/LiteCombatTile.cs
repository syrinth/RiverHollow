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
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.CombatStuff
{
    public class CombatTile
    {
        public TargetEnum TargetType { get; }
        public int Row { get; }
        public int Column { get; }
        public bool Selected { get; private set; }
        public CombatActor Character { get; private set; }
        public GUICombatTile GUITile { get; private set; }

        public CombatTile(int row, int col, TargetEnum tileType)
        {
            Row = row;
            Column = col;
            TargetType = tileType;
        }

        public void SetCombatant(CombatActor c, bool moveCharNow = true)
        {
            Character = c;
            if (c != null)
            {
                if (Character.Tile != null && Character.Tile.Character == c)
                {
                    Character.Tile.SetCombatant(null);
                }
                Character.Tile = this;
            }

            GUITile.SyncGUIObjects(Character != null);
        }

        public void AssignGUITile(GUICombatTile c)
        {
            GUITile = c;
        }

        public bool Occupied()
        {
            return Character != null;
        }

        public void Select(bool val)
        {
            Selected = val;

            if (Selected && CombatManager.SelectedTile != this)
            {
                if (CombatManager.SelectedTile != null) { CombatManager.SelectedTile.Select(false); }
                CombatManager.SelectedTile = this;
            }
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            GUITile.PlayAnimation(animation);
        }

        public void LevelUp()
        {
            GUITile.LevelledUp();
        }
    }
}
