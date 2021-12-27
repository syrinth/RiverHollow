using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.Combat.Lite;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUICombatTile : GUIObject
    {
        //GUIImage _gTargetter;
        GUIImage _gTile;
        GUILiteCombatActorInfo _gCombatSprite;
        public GUISprite CharacterSprite => _gCombatSprite.CharacterSprite;
        public GUISprite CharacterWeaponSprite => _gCombatSprite.CharacterWeaponSprite;
        GUIText _gEffect;
        GUISprite _gSummon;
        public GUISprite SummonSprite => _gSummon;
        GUIText _gSummonEffect;

        LiteCombatTile _mapTile;
        public LiteCombatTile MapTile => _mapTile;

        //SpriteFont _fDmg;
        int _iDmgTimer = 40;

        public GUICombatTile(LiteCombatTile tile)
        {
            _mapTile = tile;
            _mapTile.AssignGUITile(this);
           // _fDmg = DataManager.GetFont(@"Fonts\Font");

            _gTile = new GUIImage(new Rectangle(145, 33, 30, 31), 30, 31, DataManager.COMBAT_TEXTURE);
            _gTile.SetScale(GameManager.CurrentScale);
            //_gTargetter = new GUIImage(new Rectangle(256, 96, 32, 32), 32, 32, @"Textures\Dialog");

            Setup();

            Width = _gTile.Width;
            Height = _gTile.Height;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget)
            {
                _gTile.Alpha(CombatManager.SelectedAction.LegalTiles.Contains(_mapTile) ? 1 : 0.5f);
            }
            else
            {
                _gTile.Alpha(1);
            }

            if (CombatManager.SelectedAction != null)
            {
                _gTile.SetColor(CombatManager.SelectedAction.GetEffectedTiles().Contains(MapTile) ? Color.Red : Color.White);
            }
            else if (CombatManager.SelectedAction == null) { _gTile.SetColor(Color.White); }

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
            }

            if (_gEffect != null && _iDmgTimer < 40)
            {
                _gEffect.Draw(spriteBatch);
            }

            //if (_mapTile.Selected) { _gTargetter.Draw(spriteBatch); }
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
            //_gTargetter.AnchorAndAlignToObject(_gTile, SideEnum.Top, SideEnum.CenterX, 30);
            if (Occupied())
            {
                _gCombatSprite.Position(GetIdleLocation(_gCombatSprite.CharacterSprite));

                _gEffect = new GUIText();
                _gEffect.AnchorAndAlignToObject(_gCombatSprite, SideEnum.Top, SideEnum.CenterX);
            }
        }

        public void SyncGUIObjects(bool occupied)
        {
            if (occupied)
            {
                _gCombatSprite = new GUILiteCombatActorInfo(_mapTile.Character);
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
                if (CombatManager.PhaseChooseTarget())
                {
                    CombatManager.TestHoverTile(_mapTile);
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
            temp.ScaledMoveBy(0, -12);

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
