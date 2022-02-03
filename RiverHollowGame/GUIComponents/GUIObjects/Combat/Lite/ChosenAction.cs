using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.CombatManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class ChosenAction
    {
        private Consumable _chosenItem;
        private CombatAction _chosenAction;
        public List<CombatTile> LegalTiles { get; }
        public CombatActor User;
        public string Name { get; }

        bool _bDrawItem;

        public ChosenAction(Consumable it)
        {
            User = ActiveCharacter;
            _chosenItem = it;
            Name = _chosenItem.Name;
            LegalTiles = new List<CombatTile>();

            //Only the adjacent tiles are legal
            if (TargetsAlly())
            {
                LegalTiles.Add(User.Tile);

                List<CombatTile> adj = GetAdjacent(User.Tile);
                foreach (CombatTile t in adj)
                {
                    if (t.TargetType == TargetEnum.Ally) { LegalTiles.Add(t); }
                }
            }
            else if (TargetsEnemy())
            {
                EnemyFrontLineLegal();
            }
        }

        public ChosenAction(CombatAction obj)
        {
            User = ActiveCharacter;
            _chosenAction = obj;
            Name = _chosenAction.Name;

            _chosenAction.SkillUser = ActiveCharacter;

            LegalTiles = new List<CombatTile>();
            if (IsMelee())
            {
                EnemyFrontLineLegal();
            }
            else if (IsRanged())
            {
                int col = -1;
                int maxCol = MAX_COLUMN;
                if (TargetsEnemy()) { col = ENEMY_FRONT; }
                else
                {
                    col = 0;
                    maxCol = ENEMY_FRONT;
                }

                for (; col < maxCol; col++)
                {
                    for (int row = 0; row < MAX_ROW; row++)
                    {
                        LegalTiles.Add(GetTileFromMap(row, col));
                    }
                }
            }
            else if (SelfOnly())
            {
                LegalTiles.Add(User.Tile);
            }
            else if (Columns())
            {
                int startCol = ActiveCharacter.Tile.Column;
                int endCol = ActiveCharacter.Tile.Column;

                if (ActiveCharacter.Tile.Column > 0) { startCol = ActiveCharacter.Tile.Column - 1; }
                if (ActiveCharacter.Tile.Column < ALLY_FRONT) { endCol = ActiveCharacter.Tile.Column + 1; }

                for (int row = 0; row < MAX_ROW; row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        if (!GetTileFromMap(row, col).Occupied())
                        {
                            LegalTiles.Add(GetTileFromMap(row, col));
                        }
                    }
                }
            }
            else if (Adjacent())
            {
                int col = ActiveCharacter.Tile.Column;
                int row = ActiveCharacter.Tile.Row;

                if (row - 1 >= 0) { LegalTiles.Add(GetTileFromMap(row - 1, col)); }
                if (row + 1 < MAX_ROW) { LegalTiles.Add(GetTileFromMap(row + 1, col)); }

                if (col - 1 >= 0) { LegalTiles.Add(GetTileFromMap(row, col - 1)); }
                if (col + 1 < ALLY_FRONT) { LegalTiles.Add(GetTileFromMap(row, col + 1)); }
            }

            if(obj.Compare(ActionEnum.Move))
            {
                LegalTiles.RemoveAll(x => x.Occupied());
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (_bDrawItem && _chosenItem != null)     //We want to draw the item above the character's head
            {
                int size = GameManager.TILE_SIZE * GameManager.CurrentScale;
                GUIImage gItem = new GUIImage(_chosenItem.SourceRectangle, size, size, _chosenItem.Texture);
                CombatActor c = ActiveCharacter;

                gItem.AnchorAndAlignToObject(c.GetSprite(), SideEnum.Top, SideEnum.CenterX);
                gItem.Draw(spritebatch);
            }
            if (_chosenAction != null && _chosenAction.Sprite != null)
            {
                _chosenAction.Sprite.Draw(spritebatch);
            }
        }

        public void PerformAction(GameTime gameTime)
        {
            if (_chosenAction != null) { _chosenAction.HandlePhase(gameTime); }
            else if (_chosenItem != null)
            {
                bool finished = false;
                CombatActor c = ActiveCharacter;
                if (!c.IsCurrentAnimation(AnimationEnum.Action1))
                {
                    c.Tile.PlayAnimation(AnimationEnum.Action1);
                    _bDrawItem = true;
                }
                else if (c.AnimationPlayedXTimes(3))
                {
                    c.Tile.PlayAnimation(AnimationEnum.Idle);
                    _bDrawItem = false;
                    finished = true;
                }

                if (finished) { UseItem(); }
            }
        }

        public void UseItem()
        {
            if (_chosenItem != null)
            {
                if (_chosenItem.Recover)
                {
                    TargetTile.Character.Recover();
                }

                int val = TargetTile.Character.IncreaseHealth(_chosenItem.Health);
                if (val > 0)
                {
                    TargetTile.GUITile.AssignEffect(val, false);
                }
                _chosenItem.Remove(1);
            }

            EndTurn();
        }

        public void SetSkillTarget()
        {
            SetTargetTile(SelectedTile);
            if (_chosenAction != null)
            {
                _chosenAction.AnimationSetup();
                Text = SelectedAction.Name;
            }
            else if (_chosenItem != null)
            {
                Text = SelectedAction.Name;
            }
            CurrentPhase = PhaseEnum.DisplayAttack;
            ClearSelectedTile();
        }

        private void EnemyFrontLineLegal()
        {
            int col = FindEnemyFrontLine();
            for (int row = 0; row < MAX_ROW; row++)
            {
                LegalTiles.Add(GetTileFromMap(row, col));
            }
        }

        /// <summary>
        /// Retrieves the tiles that will be effected by this skill based off the area type
        /// </summary>
        /// <returns>A complete list of tiles that will be hit</returns>
        public List<CombatTile> GetAffectedTiles()
        {
            List<CombatTile> listTiles = new List<CombatTile>();
            if (_chosenItem != null)
            {
                listTiles.Add(SelectedTile);
            }
            else
            {
                if (SelectedTile != null)
                {
                    //Describes which side of the Battlefield we are targetting
                    bool targetMonsterGrid = (PlayerTurn && TargetsEnemy()) || (!PlayerTurn && TargetsAlly());
                    bool targetPlayerGrid = (!PlayerTurn && TargetsEnemy()) || (PlayerTurn && TargetsAlly());

                    //All we need to do here is select all of the tiles containing the appropriate characters
                    if (_chosenAction.AreaType == AreaTypeEnum.All)
                    {
                        if (targetMonsterGrid)
                        {
                            foreach (Monster m in Monsters)
                            {
                                if (!listTiles.Contains(m.Tile)) { listTiles.Add(m.Tile); }
                            }
                        }
                        else
                        {
                            foreach (CombatActor adv in Party)
                            {
                                if (!listTiles.Contains(adv.Tile)) { listTiles.Add(adv.Tile); }
                            }
                        }
                    }
                    else
                    {
                        //The coordinates of the selected tile
                        int targetRow = SelectedTile.Row;
                        int targetCol = SelectedTile.Column;

                        //Determines how far to the side the skill can go, based on whether it grows left or right
                        int minCol = targetMonsterGrid ? ENEMY_FRONT : 0;
                        int maxCol = targetMonsterGrid ? MAX_COLUMN : ENEMY_FRONT;
                        if (_chosenAction.AreaType == AreaTypeEnum.Column)
                        {
                            for (int i = 0; i < MAX_ROW; i++)
                            {
                                listTiles.Add(GetTileFromMap(i, targetCol));
                            }
                        }
                        else if (_chosenAction.AreaType == AreaTypeEnum.Row)
                        {
                            for (int i = minCol; i < maxCol; i++)
                            {
                                listTiles.Add(GetTileFromMap(targetRow, i));
                            }
                        }
                        else if (_chosenAction.AreaType == AreaTypeEnum.Square)
                        {
                            if(targetRow == MAX_ROW - 1) { targetRow--; }
                            if(targetCol == maxCol - 1) { targetCol--; }

                            listTiles.Add(GetTileFromMap(targetRow, targetCol));
                            listTiles.Add(GetTileFromMap(targetRow, targetCol + 1));
                            listTiles.Add(GetTileFromMap(targetRow + 1, targetCol));
                            listTiles.Add(GetTileFromMap(targetRow + 1, targetCol + 1));
                        }
                        else if (_chosenAction.AreaType == AreaTypeEnum.Single)
                        {
                            listTiles.Add(SelectedTile);
                        }
                        else if (_chosenAction.AreaType == AreaTypeEnum.Self)
                        {
                            listTiles.Add(User.Tile);
                        }
                    }
                }
            }

            return listTiles;
        }

        public void SetUser(CombatActor c)
        {
            if (_chosenAction != null)
            {
                _chosenAction.SkillUser = c;
            }
        }

        public bool CompareTargetType(TargetEnum t) { return t == _chosenAction.Target; }
        public bool TargetsAlly()
        {
            bool rv = false;

            if (_chosenAction != null) { rv = _chosenAction.IsHelpful(); }
            else if (_chosenItem != null) { rv = _chosenItem.Helpful; }

            return rv;
        }
        public bool TargetsEnemy()
        {
            bool rv = false;

            if (_chosenAction != null) { rv = !_chosenAction.IsHelpful(); }
            else if (_chosenItem != null) { rv = !_chosenItem.Helpful; }

            return rv;
        }
        public bool Compare(ActionEnum e) { return _chosenAction != null && _chosenAction.Compare(e); }
        public bool IsSummonSpell() { return _chosenAction != null && _chosenAction.IsSummonSpell(); }
        public bool SelfOnly() { return _chosenAction.Target == TargetEnum.Self; }
        public bool IsMelee() { return _chosenAction.Range == RangeEnum.Melee; }
        public bool IsRanged() { return _chosenAction.Range == RangeEnum.Ranged; }
        public bool SingleTarget()
        {
            bool rv = false;
            if (_chosenAction != null) { rv = _chosenAction.AreaType == AreaTypeEnum.Single; }
            else if (_chosenItem != null) { rv = true; }

            return rv;
        }
        public bool CanTwinCast()
        {
            bool rv = false;
            if (_chosenAction != null)
            {
                rv = _chosenAction.Potency > 0;
            }
            return rv;
        }

        /// <summary>
        /// Used to determine whether or not the skill is used over an
        /// area or needs to have a specifiedsingle target.
        /// </summary>
        /// <returns>True if can be spread over an area</returns>
        public bool TargetsEach()
        {
            bool rv = false;

            if (_chosenAction != null) { rv = _chosenAction.TargetsEach(); }

            return rv;
        }
        public bool AreaOfEffect()
        {
            bool rv = false;

            if (_chosenAction != null) { rv = _chosenAction.AreaType != AreaTypeEnum.Single; }
            else if (_chosenItem != null) { rv = false; }

            return rv;
        }
        public bool Columns()
        {
            bool rv = false;
            if (_chosenAction != null) { rv = _chosenAction.Range == RangeEnum.Column; }
            else if (_chosenItem != null) { rv = false; }

            return rv;
        }
        public bool Adjacent()
        {
            bool rv = false;
            if (_chosenAction != null) { rv = _chosenAction.Range == RangeEnum.Adjacent; }
            else if (_chosenItem != null) { rv = false; }

            return rv;
        }

        public void Clear()
        {
            if (_chosenAction != null) { _chosenAction.TileTargetList.Clear(); }
            else if (_chosenItem != null) { SetTargetTile(null); }
        }
        public List<CombatTile> GetTargetTiles()
        {
            if (_chosenAction != null)
            {
                return _chosenAction.TileTargetList;
            }
            else
            {
                return null;
            }
        }
        public void SetTargetTiles(List<CombatTile> li)
        {
            if (_chosenAction != null)
            {
                _chosenAction.AnimationSetup();
                _chosenAction.TileTargetList = li;
            }
        }
    }
}
