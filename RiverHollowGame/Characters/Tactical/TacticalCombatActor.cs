using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.TravelManager;

namespace RiverHollow.Characters
{
    public abstract class TacticalCombatActor : TalkingActor
    {
        #region Properties
        protected const int MAX_STAT = 99;
        protected string _sUnique;

        protected bool _bPause;
        public bool Paused => _bPause;

        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        protected int _iCurrentHP;
        public int CurrentHP
        {
            get { return _iCurrentHP; }
            set { _iCurrentHP = value; }
        }
        public virtual int MaxHP => 20 + (int)Math.Pow(((double)StatVit / 3), 1.98);

        protected int _iCurrentMP;
        public int CurrentMP
        {
            get { return _iCurrentMP; }
            set { _iCurrentMP = value; }
        }
        public virtual int MaxMP => StatMag * 3;

        public int CurrentCharge;
        public int DummyCharge;
        public RHTile BaseTile => _arrTiles[0, 0];
        protected RHTile[,] _arrTiles;
        public PriorityQueue<RHTile> legalTiles;

        #region Stats
        protected int _iMoveSpeed = 5;
        public int MovementSpeed => _iMoveSpeed;

        public virtual int Attack => 9;

        protected int _iStrength;
        public virtual int StatStr => _iStrength + _iBuffStr;
        protected int _iDefense;
        public virtual int StatDef => _iDefense + _iBuffDef;
        protected int _iVitality;
        public virtual int StatVit => _iVitality + _iBuffVit;
        protected int _iMagic;
        public virtual int StatMag => _iMagic + _iBuffMag;
        protected int _iResistance;
        public virtual int StatRes => _iResistance + _iBuffRes;
        protected int _iSpeed;
        public virtual int StatSpd => _iSpeed + _iBuffSpd;

        protected int _iBuffStr;
        protected int _iBuffDef;
        protected int _iBuffVit;
        protected int _iBuffMag;
        protected int _iBuffRes;
        protected int _iBuffSpd;
        protected int _iBuffCrit;
        protected int _iBuffEvade;
        #endregion

        protected int _iCrit = 10;
        public int CritRating => _iCrit + _iBuffCrit;

        public int Evasion => (int)(40 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatSpd))))) + _iBuffEvade;
        public int ResistStatus => (int)(50 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatRes)))));

        protected List<TacticalMenuAction> _liActions;
        public virtual List<TacticalMenuAction> TacticalAbilityList => _liActions;

        protected List<StatusEffect> _liStatusEffects;
        public List<StatusEffect> LiBuffs { get => _liStatusEffects; }

        protected Dictionary<ConditionEnum, bool> _diConditions;
        public Dictionary<ConditionEnum, bool> DiConditions => _diConditions;

        private ElementEnum _elementAttackEnum;
        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;

        public TacticalSummon LinkedSummon { get; private set; }

        public bool Counter;
        public bool GoToCounter;

        public TacticalCombatActor MyGuard;
        public TacticalCombatActor GuardTarget;
        protected bool _bGuard;
        public bool Guard => _bGuard;

        public bool Swapped;
        #endregion

        public TacticalCombatActor() : base()
        {
            legalTiles = new PriorityQueue<RHTile>();
            _arrTiles = new RHTile[_iSize, _iSize];
            _liActions = new List<TacticalMenuAction>();
            _liStatusEffects = new List<StatusEffect>();
            _diConditions = new Dictionary<ConditionEnum, bool>
            {
                [ConditionEnum.KO] = false,
                [ConditionEnum.Poisoned] = false,
                [ConditionEnum.Silenced] = false
            };

            _diElementalAlignment = new Dictionary<ElementEnum, ElementAlignment>
            {
                [ElementEnum.Fire] = ElementAlignment.Neutral,
                [ElementEnum.Ice] = ElementAlignment.Neutral,
                [ElementEnum.Lightning] = ElementAlignment.Neutral
            };

        }
        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);

            if (TacticalCombatManager.InCombat && _iCurrentHP > 0)
            {
                Texture2D texture = DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE);
                Vector2 pos = Position;
                pos.Y += (TILE_SIZE * _iSize);

                //Do not allow the bar to have less than 2 pixels, one for the border and one to display.
                double totalWidth = TILE_SIZE * Size;
                double percent = (double)CurrentHP / (double)MaxHP;
                int drawWidth = Math.Max((int)(totalWidth * percent), 2);

                DrawDisplayBar(spriteBatch, pos, texture, drawWidth, (int)totalWidth, 16, 0);

                if (MaxMP > 0)
                {
                    totalWidth = TILE_SIZE * Size;
                    percent = (double)CurrentMP / (double)MaxMP;
                    drawWidth = Math.Max((int)(totalWidth * percent), 2);

                    pos.Y += 4;
                    DrawDisplayBar(spriteBatch, pos, texture, drawWidth, (int)totalWidth, 22, 0);
                }
            }
        }

        private void DrawDisplayBar(SpriteBatch spriteBatch, Vector2 pos, Texture2D texture, int drawWidth, int totalWidth, int startX, int startY)
        {
            spriteBatch.Draw(texture, new Rectangle((int)pos.X, (int)pos.Y, drawWidth, 4), new Rectangle(startX + 4, startY, 1, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y);

            //Draw End, then middle, then other end
            spriteBatch.Draw(texture, new Rectangle((int)pos.X, (int)pos.Y, 1, 4), new Rectangle(startX, startY, 1, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
            spriteBatch.Draw(texture, new Rectangle((int)pos.X + 1, (int)pos.Y, (int)totalWidth - 2, 4), new Rectangle(startX + 1, startY, 2, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
            spriteBatch.Draw(texture, new Rectangle((int)pos.X + (int)totalWidth - 1, (int)pos.Y, 1, 4), new Rectangle(startX + 3, startY, 1, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //Finished being hit, determine action
            if (IsCurrentAnimationVerb(VerbEnum.Hurt) && BodySprite.GetPlayCount() >= 1)
            {
                if (_bPause) { _bPause = false; }

                if (CurrentHP == 0) { KO(); }
                else { GoToIdle(); }
            }

            ///Stand back up after the KO status has been removed
            if (!_diConditions[ConditionEnum.KO] && IsCurrentAnimation(AnimationEnum.KO))
            {
                GoToIdle();
            }

            if (IsCurrentAnimationVerb(VerbEnum.Critical) && !IsCritical())
            {
                PlayAnimationVerb(VerbEnum.Walk);
            }

            if (!_bPause && (MapManager.Maps.ContainsKey(CurrentMapName) && MapManager.Maps[CurrentMapName].ContainsActor(this) || this == PlayerManager.World))
            {
                if (_vMoveTo != Vector2.Zero)
                {
                    HandleMove(_vMoveTo);
                }
                else if (_liTilePath.Count > 0)
                {
                    if (!DoorCheck())
                    {
                        Vector2 targetPos = _liTilePath[0].Position;
                        if (Position == targetPos)
                        {
                            RHTile newTile = _liTilePath[0];
                            _liTilePath.Remove(newTile);

                            if (_liTilePath.Count == 0)
                            {
                                if (PlayerManager.ReadyToSleep)
                                {
                                    if (_dCooldown == 0)
                                    {
                                        Facing = DirectionEnum.Left;
                                        PlayAnimation(VerbEnum.Idle, DirectionEnum.Left);
                                        _dCooldown = 3;
                                        PlayerManager.AllowMovement = true;
                                    }
                                }
                                else
                                {
                                    DetermineFacing(Vector2.Zero);
                                    GoToIdle();
                                }
                            }
                            else if (TacticalCombatManager.InCombat)
                            {
                                TacticalCombatManager.CheckTileForActiveHazard(this, newTile);
                            }
                        }
                        else
                        {
                            HandleMove(targetPos);
                        }
                    }
                }
            }
        }

        public virtual void GoToIdle()
        {
            if (IsCritical()) { PlayAnimationVerb(VerbEnum.Critical); }
            else { PlayAnimationVerb(VerbEnum.Walk); }
        }

        public virtual void KO()
        {
            TacticalCombatManager.RemoveKnockedOutCharacter(this);
            PlayAnimation(AnimationEnum.KO);
        }

        /// <summary>
        /// Calculates the damage to be dealt against the actor.
        /// 
        /// Run the damage equation against the defender, then apply any 
        /// relevant elemental resistances.
        /// 
        /// Finally, roll against the crit rating. Rolling higher than the 
        /// rating on a percentile roll means no crit. Crit Rating 10 means
        /// roll 10 or less
        /// </summary>
        /// <param name="attacker">Who is attacking</param>
        /// <param name="potency">The potency of the attack</param>
        /// <param name="element">any associated element</param>
        /// <returns></returns>
        public void ProcessAttack(TacticalCombatActor attacker, int potency, int critRating, ElementEnum element = ElementEnum.None)
        {
            double compression = 0.8;
            double potencyMod = potency / 100.0;   //100 potency is considered an average attack
            double base_attack = attacker.Attack;  //Attack stat is either weapon damage or mod on monster str
            double StrMult = Math.Round(1 + ((double)attacker.StatStr / 4 * attacker.StatStr / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_attack - StatDef) * compression * StrMult * potencyMod);
            dmg += ApplyResistances(dmg, element);

            if (RHRandom.Instance().Next(1, 100) <= (attacker.CritRating + critRating)) { dmg *= 2; }

            ModifyHealth(dmg, true);
        }
        public void ProcessSpell(TacticalCombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

            double damage = Math.Round(maxDmg / divisor);
            damage += ApplyResistances(damage, element);

            ModifyHealth(damage, true);
        }
        public double ApplyResistances(double dmg, ElementEnum element = ElementEnum.None)
        {
            double modifiedDmg = 0;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }

                //Should only apply for Summoners
                if (LinkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (LinkedSummon.Element.Equals(element))
                    {
                        modifiedDmg += (dmg * 0.8) - dmg;
                    }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    modifiedDmg += (dmg * 0.8) - dmg;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    modifiedDmg += (dmg * 1.2) - dmg;
                }
            }

            return modifiedDmg;
        }

        public void ProcessHealingSpell(TacticalCombatActor attacker, int potency)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

            int damage = (int)Math.Round(maxDmg / divisor);

            ModifyHealth(damage, false);
        }

        /// <summary>
        /// Handler for modifying the health of a CombatActor. Ensures
        /// </summary>
        /// <param name="value">The amount to modify HP by</param>
        /// <param name="bHarmful">Whether the modification is harmful or helpful</param>
        public virtual void ModifyHealth(double value, bool bHarmful)
        {
            //Round the value down in case it's not an int due to resistances
            int iValue = (int)Math.Round(value);

            //Handler for when the modification is harmful.
            if (bHarmful)
            {
                //Checks that the current HP is greater than the amount of damage dealt
                //If not, just remove the current HP so that we don't go negative.
                _iCurrentHP -= (_iCurrentHP - iValue >= 0) ? iValue : _iCurrentHP;
                PlayAnimationVerb(VerbEnum.Hurt);

                if (this == TacticalCombatManager.ActiveCharacter) { _bPause = true; }

                //If the character goes to 0 hp, give them the KO status and unlink any summons
                if (_iCurrentHP == 0)
                {
                    _diConditions[ConditionEnum.KO] = true;
                    UnlinkSummon();
                }
            }
            else
            {
                //Can't restore HP when the KO condition is present.
                if (!KnockedOut())
                {
                    //Adds only enough life to get to the max. No Overhealing
                    if (_iCurrentHP + iValue <= MaxHP)
                    {
                        _iCurrentHP += iValue;
                    }
                    else
                    {
                        iValue = MaxHP - _iCurrentHP;
                        _iCurrentHP = MaxHP;
                    }
                }
            }

            TacticalCombatManager.AddFloatingText(new FloatingText(this.Position, this.Width, iValue.ToString(), bHarmful ? Color.Red : Color.Green));
        }

        public bool IsCritical()
        {
            return (float)CurrentHP / (float)MaxHP <= 0.25;
        }

        public void IncreaseMana(int x)
        {
            if (_iCurrentMP + x <= MaxMP)
            {
                _iCurrentMP += x;
            }
            else
            {
                _iCurrentMP = MaxMP;
            }
        }

        /// <summary>
        /// Reduce the duration of each status effect on the Actor by one
        /// If the effect's duration reaches 0, remove it, otherwise have it run
        /// any upkeep effects it may need to do.
        /// </summary>
        public void TickStatusEffects()
        {
            List<StatusEffect> toRemove = new List<StatusEffect>();
            foreach (StatusEffect b in _liStatusEffects)
            {
                if (--b.Duration == 0)
                {
                    toRemove.Add(b);
                    RemoveStatusEffect(b);
                }
                else
                {
                    if (b.DoT)
                    {
                        ProcessSpell(b.TacticalCaster, b.Potency);
                    }
                    if (b.HoT)
                    {
                        ProcessHealingSpell(b.TacticalCaster, b.Potency);
                    }
                }
            }

            foreach (StatusEffect b in toRemove)
            {
                _liStatusEffects.Remove(b);
            }
            toRemove.Clear();
        }

        /// <summary>
        /// Adds the StatusEffect objectto the character's list of status effects.
        /// </summary>
        /// <param name="b">Effect toadd</param>
        public void AddStatusEffect(StatusEffect b)
        {
            //Only one song allowed at a time so see if there is another
            //songand,if so, remove it.
            if (b.Song)
            {
                StatusEffect song = _liStatusEffects.Find(status => status.Song);
                if (song != null)
                {
                    RemoveStatusEffect(song);
                    _liStatusEffects.Remove(song);
                }
            }

            //Look to see if the status effect already exists, if so, just
            //set the duration to be the new duration. No stacking.
            StatusEffect find = _liStatusEffects.Find(status => status.Name == b.Name);
            if (find == null) { _liStatusEffects.Add(b); }
            else { find.Duration = b.Duration; }

            foreach (KeyValuePair<StatEnum, int> kvp in b.StatMods)
            {
                HandleStatBuffs(kvp);
            }

            //If the status effect provides counter, turn counter on.
            if (b.Counter) { Counter = true; }

            if (b.Guard) { _bGuard = true; }
        }

        /// <summary>
        /// Removes the status effect from the Actor
        /// </summary>
        /// <param name="b"></param>
        public void RemoveStatusEffect(StatusEffect b)
        {
            foreach (KeyValuePair<StatEnum, int> kvp in b.StatMods)
            {
                HandleStatBuffs(kvp, true);
            }

            if (b.Counter) { Counter = false; }
            if (b.Guard) { _bGuard = false; }
        }

        /// <summary>
        /// Helper to not repeat code for the Stat buffing/debuffing
        /// 
        /// Pass in the statmod kvp and an integer representing positive or negative
        /// and multiply the mod by it. If we are adding, it will remain unchanged, 
        /// if we are subtracting, the positive value will go negative.
        /// </summary>
        /// <param name="kvp">The stat to modifiy and how much</param>
        /// <param name="negative">Whether or not we need to add or remove the value</param>
        private void HandleStatBuffs(KeyValuePair<StatEnum, int> kvp, bool negative = false)
        {
            int modifier = negative ? -1 : 1;
            switch (kvp.Key)
            {
                case StatEnum.Str:
                    _iBuffStr += kvp.Value * modifier;
                    break;
                case StatEnum.Def:
                    _iBuffDef += kvp.Value * modifier;
                    break;
                case StatEnum.Vit:
                    _iBuffVit += kvp.Value * modifier;
                    break;
                case StatEnum.Mag:
                    _iBuffMag += kvp.Value * modifier;
                    break;
                case StatEnum.Res:
                    _iBuffRes += kvp.Value * modifier;
                    break;
                case StatEnum.Spd:
                    _iBuffSpd += kvp.Value * modifier;
                    break;
                case StatEnum.Crit:
                    _iBuffCrit += kvp.Value * modifier;
                    break;
                case StatEnum.Evade:
                    _iBuffEvade += kvp.Value * modifier;
                    break;
            }
        }

        #region Tile Handling
        /// <summary>
        /// Sets the base tile, which will always be the upper-left most tile
        /// to the given tile, then assign the character to the appropiate tiles around it.
        /// </summary>
        /// <param name="newTile">The tile to be the new base tile</param>
        public void SetBaseTile(RHTile newTile, bool setPosition)
        {
            ClearTiles();

            RHTile lastTile = newTile;
            for (int i = 0; i < _iSize; i++)
            {
                for (int j = 0; j < _iSize; j++)
                {
                    _arrTiles[i, j] = lastTile;
                    _arrTiles[i, j].SetCombatant(this);
                    lastTile = lastTile.GetTileByDirection(DirectionEnum.Right);
                }

                //Reset to the first Tile in the current row and go down one
                lastTile = _arrTiles[i, 0].GetTileByDirection(DirectionEnum.Down);
            }

            TacticalCombatManager.CheckTileForActiveHazard(this);
            if (setPosition) { Position = BaseTile.Position; }
        }

        /// <summary>
        /// Returns a List of all RHTiles adjacent to the CombatActor. This method
        /// works in tandem with the Actors size value to return the proper RHTiles
        /// </summary>
        /// <returns></returns>
        public List<RHTile> GetAdjacentTiles()
        {
            List<RHTile> rvList = new List<RHTile>();

            foreach (RHTile t in _arrTiles)
            {
                foreach (RHTile adjTile in t.GetAdjacentTiles())
                {
                    //Do not add the same RHTile twice, nor add a RHTile containing ourself.
                    if (!rvList.Contains(adjTile) && adjTile.Character != this)
                    {
                        rvList.Add(adjTile);
                    }
                }
            }

            return rvList;
        }

        public List<RHTile> GetTileList()
        {
            List<RHTile> rvList = new List<RHTile>();

            for (int y = 0; y < _iSize; y++)
            {
                for (int x = 0; x < _iSize; x++)
                {
                    rvList.Add(_arrTiles[x, y]);
                }
            }

            return rvList;
        }

        public RHTile GetTileAt(int x, int y)
        {
            return _arrTiles[x, y];
        }

        public void ClearTiles()
        {
            //Remove self from each tile that they are registered to.
            foreach (RHTile t in _arrTiles)
            {
                t?.SetCombatant(null);
            }
        }
        #endregion

        public void LinkSummon(TacticalSummon s)
        {
            LinkedSummon = s;
            s.linkedChar = this;
        }

        public void UnlinkSummon()
        {
            LinkedSummon?.KO();
            LinkedSummon = null;
        }

        /// <summary>
        /// Returns the Elemental type of the attack. In the event that there is
        /// a LinkedSummon, which should only be the case for Summoners, use the Summons
        /// elemental attack instead if none exists.
        /// </summary>
        /// <returns></returns>
        public virtual ElementEnum GetAttackElement()
        {
            ElementEnum e = _elementAttackEnum;

            if (LinkedSummon != null && e.Equals(ElementEnum.None))
            {
                e = LinkedSummon.Element;
            }

            return e;
        }

        public bool CanCast(int x)
        {
            return x <= CurrentMP;
        }

        public void SetUnique(string u)
        {
            _sUnique = u;
        }

        public bool KnockedOut()
        {
            return _diConditions[ConditionEnum.KO];
        }

        public bool Poisoned()
        {
            return _diConditions[ConditionEnum.Poisoned];
        }

        public bool Silenced()
        {
            return _diConditions[ConditionEnum.Silenced];
        }

        public void ChangeConditionStatus(ConditionEnum c, bool setTo)
        {
            _diConditions[c] = setTo;
        }

        public void ClearConditions()
        {
            foreach (ConditionEnum condition in Enum.GetValues(typeof(ConditionEnum)))
            {
                ChangeConditionStatus(condition, false);
            }
        }

        public virtual void EndTurn() { }

        public void GetHP(ref double curr, ref double max)
        {
            curr = _iCurrentHP;
            max = MaxHP;
        }

        public void GetMP(ref double curr, ref double max)
        {
            curr = _iCurrentMP;
            max = MaxMP;
        }

        public virtual List<TacticalCombatAction> GetCurrentSpecials()
        {
            return null;
        }

        public virtual bool IsSummon() { return false; }

    }
}
