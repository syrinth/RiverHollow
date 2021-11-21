using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class GUILiteTurnOrderDisplay : GUIObject
    {
        const int MAX_SHOWN = 10;
        int _iCurrUpdate = MAX_SHOWN;
        double _dTimer = 0;
        bool _bUpdate = false;
        bool _bTriggered = false;
        bool _bSyncing = false;

        GUIImage[] _arrBarDisplay;
        TurnDisplay[] _arrTurnDisplay;
        List<LiteCombatActor> _liNewTurnOrder;
        GUIWindow _gWindow;

        public GUILiteTurnOrderDisplay()
        {
            _arrTurnDisplay = new TurnDisplay[MAX_SHOWN];
            _arrBarDisplay = new GUIImage[MAX_SHOWN];

            _liNewTurnOrder = LiteCombatManager.CalculateTurnOrder(MAX_SHOWN);

            for (int i = 0; i < MAX_SHOWN; i++)
            {
                _arrTurnDisplay[i] = new TurnDisplay(_liNewTurnOrder[i], _arrBarDisplay);
                _arrBarDisplay[i] = new GUIImage(new Rectangle(48, 58, 10, 2), 10, 2, @"Textures\Dialog");
                _arrBarDisplay[i].SetScale(LiteCombatManager.CombatScale);
            }

            Width = MAX_SHOWN * _arrTurnDisplay[0].Width;
            Height = (2 * _arrTurnDisplay[0].Height) + _arrBarDisplay[0].Height;

            Position(Position());
        }

        public override void Update(GameTime gameTime)
        {
            if (_bUpdate)
            {
                _dTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                if (_dTimer <= 0)
                {
                    _arrTurnDisplay[_iCurrUpdate].Update(gameTime);
                    if (!_bSyncing && _iCurrUpdate == MAX_SHOWN - 1 && _arrTurnDisplay[_iCurrUpdate].Finished && _arrTurnDisplay[_iCurrUpdate].Action == TurnDisplay.ActionEnum.Insert)
                    {
                        _bUpdate = false;
                    }
                }

                if (_dTimer <= 0) { _dTimer = 0.03; }

                if (_bUpdate && (_arrTurnDisplay[_iCurrUpdate].Finished))
                {
                    _iCurrUpdate++;

                    //After incrememnting the count, which will bring us one to the left, we set
                    //The next box, ie: the box we just left, to equal the current, leftmost, box.
                    //So, 9 -> 8, 8 -> 7, 7 -> 6, 6 -> 5, 5 -> 4, 4 -> 3, 3 -> 2, 2 -> 1, 1 -> 0
                    if (!_bSyncing && _iCurrUpdate < MAX_SHOWN && _arrTurnDisplay[_iCurrUpdate - 1].Action != TurnDisplay.ActionEnum.Insert)
                    {
                        _arrTurnDisplay[_iCurrUpdate - 1] = _arrTurnDisplay[_iCurrUpdate];
                        _arrTurnDisplay[_iCurrUpdate].SetIndex(_iCurrUpdate - 1);
                    }
                    //If we're Syncing, we want to re-insert a node at the position we just popped
                    else if (_bSyncing && _arrTurnDisplay[_iCurrUpdate - 1].Action == TurnDisplay.ActionEnum.Pop)
                    {
                        _iCurrUpdate--;
                        _arrTurnDisplay[_iCurrUpdate].SetActor(_liNewTurnOrder[_iCurrUpdate]);
                        _arrTurnDisplay[_iCurrUpdate].Insert(_iCurrUpdate);
                    }
                    //When we get to the last node, we need to do some special actions
                    else if (_iCurrUpdate == MAX_SHOWN)
                    {
                        //If we're syncing, STAHP
                        if (_bSyncing)
                        {
                            _bSyncing = false;
                            _bUpdate = false;
                        }
                        else
                        {
                            int mod = --_iCurrUpdate;   //We need to act on the new, 9th box so we need to bump it back one.

                            _arrTurnDisplay[mod] = new TurnDisplay(_liNewTurnOrder[mod], _arrBarDisplay);
                            _arrTurnDisplay[mod].Insert(mod);
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (TurnDisplay displ in _arrTurnDisplay) { displ.Draw(spriteBatch); }
            foreach (GUIImage bar in _arrBarDisplay) { bar.Draw(spriteBatch); }

            if (_gWindow != null) { _gWindow.Draw(spriteBatch); }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            LiteCombatActor a = null;
            foreach (TurnDisplay t in _arrTurnDisplay)
            {
                if (t.Contains(mouse))
                {
                    rv = true;
                    a = t.Actor;
                    break;
                }
            }

            if (rv)
            {
                _gWindow = new GUIWindow(GUIWindow.Window_2, 10, 10);
                GUIText gText = new GUIText(a.Name);
                gText.AnchorToInnerSide(_gWindow, SideEnum.TopLeft);
                _gWindow.Resize();
                _gWindow.AnchorToScreen(SideEnum.BottomRight);
            }
            else
            {
                _gWindow = null;
            }

            return rv;
        }

        //Called to acquire the next turn order sequence and start off the new updates
        //Tell the currently active turn to fade out to start the updates
        public void CalculateTurnOrder()
        {
            if (_bTriggered)
            {
                List<LiteCombatActor> newList = LiteCombatManager.CalculateTurnOrder(MAX_SHOWN);

                bool change = false;
                //Assume that only one entry can be wrong for insertions
                for (int i = 0; i < MAX_SHOWN - 1; i++)
                {
                    if (_bUpdate || _liNewTurnOrder[i + 1] != newList[i])
                    {
                        change = true;
                        break;
                    }
                }

                _liNewTurnOrder = newList;


                if (change)
                {
                    for (int i = 0; i < MAX_SHOWN; i++)
                    {
                        _arrTurnDisplay[i].Pop();
                        _bSyncing = true;
                    }
                }
                else
                {
                    _arrTurnDisplay[0].Pop();
                }
            }
            else
            {
                _bTriggered = true;
            }

            _iCurrUpdate = 0;
            _bUpdate = true;
            _dTimer = 0.03;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);

            _arrTurnDisplay[MAX_SHOWN - 1].Position(value);     //Just used as a base point to start it off
            for (int i = MAX_SHOWN - 1; i >= 0; i--)
            {
                if (i == MAX_SHOWN - 1) { _arrBarDisplay[i].AnchorAndAlignToObject(_arrTurnDisplay[i], SideEnum.Bottom, SideEnum.CenterX); }
                else { _arrBarDisplay[i].AnchorAndAlignToObject(_arrBarDisplay[i + 1], SideEnum.Right, SideEnum.CenterY); }

                _arrTurnDisplay[i].Insert(i, 0.5f);
            }
        }

        private class TurnDisplay : GUIObject
        {
            public enum ActionEnum { Pop, Insert, Move };
            public ActionEnum Action;
            bool _bInParty;
            GUIText _gName;
            GUIImage _gImage;
            LiteCombatActor _actor;
            public LiteCombatActor Actor => _actor;

            bool _bFadeIn;
            public bool FadeIn => _bFadeIn;
            bool _bFadeOut;
            public bool Finished;

            float _fFadeSpeed;
            int _iIndex;
            Vector2 _vMoveTo = new Vector2(0, 0);

            GUIImage[] _arrBarDisplay;

            public TurnDisplay(LiteCombatActor actor, GUIImage[] barDisplay)
            {
                _actor = actor;
                _bInParty = !actor.IsActorType(ActorEnum.Monster);
                _gName = new GUIText(actor.Name.Substring(0, 1));
                _gImage = new GUIImage(new Rectangle(48, 48, 10, 10), 10, 10, @"Textures\Dialog");
                _gImage.SetScale(LiteCombatManager.CombatScale);

                _arrBarDisplay = barDisplay;
                Width = _gImage.Width;
                Height = _gImage.Height;
            }

            public override void Update(GameTime gameTime)
            {
                if (_bFadeOut)
                {
                    UpdateFadeOut(gameTime);
                }
                else if (_bFadeIn)
                {
                    UpdateFadeIn(gameTime);
                }
                else if (_vMoveTo != Vector2.Zero)
                {
                    UpdateMove(gameTime);
                }
            }
            private void UpdateFadeOut(GameTime gameTime)
            {
                if (Alpha() > 0)
                {
                    SetAlpha(Alpha() - _fFadeSpeed);
                }
                else
                {
                    _bFadeOut = false;
                    Finished = true;
                }
            }
            private void UpdateFadeIn(GameTime gameTime)
            {
                if (Alpha() < 1)
                {
                    SetAlpha(Alpha() + _fFadeSpeed);
                }
                else
                {
                    _bFadeIn = false;
                    Finished = true;
                }
            }
            private void UpdateMove(GameTime gameTime)
            {
                if (Position() != _vMoveTo)
                {
                    Vector2 moveDir = new Vector2(Width / 2, 0);
                    MoveBy(moveDir);
                }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    Finished = true;
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (_actor != null)
                {
                    _gImage.Draw(spriteBatch);
                    _gName.Draw(spriteBatch);
                }
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                _gImage.Position(value);
                _gName.CenterOnObject(_gImage);
            }

            public bool IsInParty()
            {
                return _bInParty;
            }

            public void SetActor(LiteCombatActor c)
            {
                _actor = c;
                _bInParty = !_actor.IsActorType(ActorEnum.Monster);
                _gName.SetText(c != null ? _actor.Name.Substring(0, 1) : string.Empty);
                _gName.CenterOnObject(_gImage);
            }
            public void SetIndex(int val)
            {
                _iIndex = val;

                _vMoveTo = GetAlignToObject(_arrBarDisplay[_iIndex], SideEnum.CenterX);

                Finished = false;
                Action = ActionEnum.Move;
            }

            public bool Occupied() { return _actor != null; }

            public void SetAlpha(float alpha)
            {
                Alpha(alpha);
                _gImage.Alpha(alpha);
                _gName.Alpha(alpha);
            }

            public string GetName()
            {
                return _actor != null ? _actor.Name : string.Empty;
            }

            public void Pop()
            {
                Finished = false;
                _bFadeOut = true;
                Action = ActionEnum.Pop;
            }
            public void Insert(int index, float speed = 0.3f)
            {
                if (_actor != null)
                {
                    Action = ActionEnum.Insert;
                    _iIndex = index;
                    _bFadeIn = true;
                    _fFadeSpeed = speed;
                    Finished = false;
                    SetAlpha(0);
                    AnchorAndAlignToObject(_arrBarDisplay[_iIndex], IsInParty() ? SideEnum.Top : SideEnum.Bottom, SideEnum.CenterX);
                }
            }
        }
    }
}
