using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUICombatTile : GUIObject
    {
        GUIImage _gTargetter;
        GUIImage _gTile;
        GUICombatActorInfo _gCombatSprite;
        public GUISprite CharacterSprite => _gCombatSprite.CharacterSprite;
        public GUISprite CharacterWeaponSprite => _gCombatSprite.CharacterWeaponSprite;
        GUIText _gEffect;
        GUISprite _gSummon;
        public GUISprite SummonSprite => _gSummon;
        GUIText _gSummonEffect;

        List<GUIStatus> _liStatus;

        LiteCombatTile _mapTile;
        public LiteCombatTile MapTile => _mapTile;

        //SpriteFont _fDmg;
        int _iDmgTimer = 40;

        public GUICombatTile(LiteCombatTile tile)
        {
            _mapTile = tile;
            _mapTile.AssignGUITile(this);
           // _fDmg = DataManager.GetFont(@"Fonts\Font");

            _gTile = new GUIImage(new Rectangle(128, 0, 32, 32), 32, 32, @"Textures\Dialog");
            _gTile.SetScale(LiteCombatManager.CombatScale);
            _gTargetter = new GUIImage(new Rectangle(256, 96, 32, 32), 32, 32, @"Textures\Dialog");

            _liStatus = new List<GUIStatus>();

            Setup();

            Width = _gTile.Width;
            Height = _gTile.Height;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (LiteCombatManager.CurrentPhase == LiteCombatManager.PhaseEnum.ChooseTarget)
            {
                _gTile.Alpha(LiteCombatManager.SelectedAction.LegalTiles.Contains(_mapTile) ? 1 : 0.5f);
            }
            else
            {
                _gTile.Alpha(1);
            }

            if (LiteCombatManager.CurrentPhase != LiteCombatManager.PhaseEnum.PerformAction && LiteCombatManager.ActiveCharacter != null && LiteCombatManager.ActiveCharacter == _mapTile.Character)
            {
                _gTile.SetColor(Color.Yellow);
            }
            else if (LiteCombatManager.SelectedAction != null)
            {
                _gTile.SetColor(LiteCombatManager.SelectedAction.GetEffectedTiles().Contains(MapTile) ? Color.Red : Color.White);
            }
            else if (LiteCombatManager.SelectedAction == null) { _gTile.SetColor(Color.White); }

            _gTile.Draw(spriteBatch);

            if (_gSummonEffect != null && _iDmgTimer < 40)
            {
                _gSummonEffect.Draw(spriteBatch);
            }
        }

        public void DrawCharacters(SpriteBatch spriteBatch)
        {
            if (Occupied())
            {
                if (_gSummon != null) { _gSummon.Draw(spriteBatch); }

                _gCombatSprite.Draw(spriteBatch);

                if (!(LiteCombatManager.CurrentPhase == LiteCombatManager.PhaseEnum.PerformAction && LiteCombatManager.ActiveCharacter == _mapTile.Character)
                    && !(_mapTile.Character.IsActorType(ActorEnum.Monster) && _mapTile.Character.IsCurrentAnimation(LiteCombatActionEnum.KO)))
                {
                    foreach (GUIStatus stat in _liStatus)
                    {
                        if (_mapTile.Character.DiConditions[stat.Status])
                        {
                            stat.Draw(spriteBatch);
                        }
                    }
                }
            }

            if (_gEffect != null && _iDmgTimer < 40)
            {
                _gEffect.Draw(spriteBatch);
            }

            if (_mapTile.Selected) { _gTargetter.Draw(spriteBatch); }
        }

        public override void Update(GameTime gameTime)
        {
            if (Occupied())
            {
                if (_gSummon != null) { _gSummon.Update(gameTime); }
                _gCombatSprite.Update(gameTime);
            }

            if (_gEffect != null)
            {
                if (_iDmgTimer < 40)
                {
                    _gEffect.MoveBy(0, -1);
                    _iDmgTimer++;

                    if (_gSummonEffect != null) { _gSummonEffect.MoveBy(0, -1); }
                }
                else if (Occupied())
                {
                    if (!String.IsNullOrEmpty(_gEffect.Text))
                    {
                        _gEffect.SetText("");
                        _gEffect.AnchorAndAlignToObject(_gCombatSprite, SideEnum.Top, SideEnum.CenterX);
                    }
                    if (_gSummonEffect != null && !String.IsNullOrEmpty(_gSummonEffect.Text))
                    {
                        _gSummonEffect.SetText("");
                        _gSummonEffect.AnchorAndAlignToObject(_gCombatSprite, SideEnum.Top, SideEnum.CenterX);
                    }
                }
                else { _gEffect = null; }
            }
        }

        private void Setup()
        {
            _gTargetter.AnchorAndAlignToObject(_gTile, SideEnum.Top, SideEnum.CenterX, 30);
            if (Occupied())
            {
                _gCombatSprite.Position(GetIdleLocation(_gCombatSprite.CharacterSprite));

                _gEffect = new GUIText();
                _gEffect.AnchorAndAlignToObject(_gCombatSprite, SideEnum.Top, SideEnum.CenterX);

                for (int i = 0; i < _liStatus.Count; i++)
                {
                    GUIStatus temp = _liStatus[i];
                    if (i == 0) { temp.AnchorAndAlignToObject(_gCombatSprite, SideEnum.Bottom, SideEnum.Left); }
                    else { temp.AnchorAndAlignToObject(_liStatus[i - 1], SideEnum.Right, SideEnum.Bottom); }
                }
            }
        }

        public void SyncGUIObjects(bool occupied)
        {
            if (occupied)
            {
                _gCombatSprite = new GUICombatActorInfo(_mapTile.Character);
                AddControl(_gCombatSprite);

                _gCombatSprite.Reset();
                _gCombatSprite.PlayAnimation(LiteCombatActionEnum.Idle);
            }
            else
            {
                _gCombatSprite = null;
            }
            Setup();
        }
        public void LinkSummon(LiteSummon s)
        {
            if (s != null)
            {
                _gSummon = new GUISprite(s.BodySprite);
                _gSummon.Position(GetIdleSummonLocation());
                _gSummonEffect = new GUIText();
                _gSummonEffect.AnchorAndAlignToObject(_gSummon, SideEnum.Top, SideEnum.CenterX);
            }
            else
            {
                _gSummon = null;
                _gSummonEffect = null;
            }
        }

        public void AssignEffect(int x, bool harms)
        {
            AssignEffect(x.ToString(), harms);
        }
        public void AssignEffect(string x, bool harms)
        {
            if (_mapTile.Character.MyGuard != null && _mapTile.Character.MyGuard.IsSummon())
            {
                AssignEffectToSummon(x);
            }
            else
            {
                _iDmgTimer = 0;
                _gEffect.SetText(x);
                _gEffect.SetColor(harms ? Color.Red : Color.LightGreen);
            }
        }

        public void AssignEffectToSummon(string x)
        {
            _iDmgTimer = 0;
            _gSummonEffect.SetText(x);
            _gSummonEffect.SetColor(Color.Red);
        }

        public void ChangeCondition(ConditionEnum c, TargetEnum target)
        {
            GUIStatus found = _liStatus.Find(test => test.Status == c);
            if (target.Equals(TargetEnum.Enemy) && found == null)       //If it targets an enemy, we add it
            {
                _liStatus.Add(new GUIStatus(c));
            }
            else if (target.Equals(TargetEnum.Ally) && found != null)    //If it targets an ally, we remove it
            {
                _liStatus.Remove(found);
            }

            _liStatus.Sort((x, y) => x.Status.CompareTo(y.Status));
            for (int i = 0; i < _liStatus.Count; i++)
            {
                if (i == 0) { _liStatus[i].AnchorAndAlignToObject(_gCombatSprite, SideEnum.Bottom, SideEnum.Left); }
                else { _liStatus[i].AnchorAndAlignToObject(_liStatus[i - 1], SideEnum.Right, SideEnum.Bottom); }
            }
        }

        public bool Occupied()
        {
            return _mapTile.Occupied();
        }

        public override bool Contains(Point mouse)
        {
            bool rv = false;

            rv = _gTile.Contains(mouse) || (Occupied() && _gCombatSprite.Contains(mouse));

            return rv;
        }

        public bool CheckForTarget(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                if (LiteCombatManager.PhaseChooseTarget())
                {
                    LiteCombatManager.TestHoverTile(_mapTile);
                    rv = true;
                }
            }

            return rv;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gTile.Position(value);
            Setup();
        }
        public Vector2 GetCharacterPosition()
        {
            Vector2 rv = Vector2.Zero;
            if (_gCombatSprite != null)
            {
                rv = _gCombatSprite.Position();
            }
            return rv;
        }

        public Vector2 GetIdleLocation(GUISprite sprite)
        {
            GUISprite temp = new GUISprite(sprite.Sprite, true);

            temp.AlignToObject(this, SideEnum.CenterX);
            temp.AlignToObject(this, SideEnum.Bottom);
            temp.MoveBy(0, -(this.Height / 3));

            return temp.Position();
        }

        public Vector2 GetIdleSummonLocation()
        {
            Vector2 rv = Vector2.Zero;
            if (_mapTile.Character.LinkedSummon != null)
            {
                GUISprite temp = new GUISprite(_mapTile.Character.LinkedSummon.BodySprite, true);

                temp.AnchorAndAlignToObject(_gCombatSprite, SideEnum.Left, SideEnum.Top);
                rv = temp.Position();
            }

            return rv;
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            _gCombatSprite.PlayAnimation(animation);
        }
    }
}
