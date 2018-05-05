using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GUIObjects.ManagementScreen.MgmtWindow;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class ManagementScreen : GUIScreen
    {
        public enum ActionTypeEnum { View, Sell, Buy, Upgrade };
        private ActionTypeEnum _eAction;
        public ActionTypeEnum Action => _eAction;
        public static int BTN_PADDING = 20;
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;

        MgmtWindow _mgmtWindow;

        List<GUIObject> _liWorkers;

        WorldAdventurer _worker;
        int _iCost;

        public ManagementScreen(ActionTypeEnum action = ActionTypeEnum.View)
        {
            _eAction = action;
            _liWorkers = new List<GUIObject>();

            _mgmtWindow = new MainBuildingsWin(this);
            Controls.Add(_mgmtWindow);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach(GUIObject g in Controls)
            {
                rv = g.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            rv = _mgmtWindow.ProcessRightButtonClick(mouse);

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;

            rv = _mgmtWindow.ProcessHover(mouse);

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void HandleBuildingSelection(WorkerBuilding selectedBuilding)
        {
            if (_worker == null)
            {
                if (selectedBuilding != null)
                {
                    Controls.Remove(_mgmtWindow);
                    _mgmtWindow = new BuildingDetailsWin(this, selectedBuilding);
                    Controls.Add(_mgmtWindow);
                }
            }
            else
            {
                RHRandom r = new RHRandom();
                if (_worker.Building != null) {
                    _worker.Building.RemoveWorker(_worker);
                }

                selectedBuilding.AddWorker(_worker, r);
                
                if (_eAction == ActionTypeEnum.Buy)
                {
                    PlayerManager.TakeMoney(_iCost);
                    GameManager.BackToMain();
                    GUIManager.SetScreen(new TextInputScreen(_worker));
                }
                
                _worker = null;
            }
        }

        public void HandleMoveWorker(WorldAdventurer worldAdventurer)
        {
            if (worldAdventurer != null)
            {
                Controls.Remove(_mgmtWindow);
                _mgmtWindow = new MainBuildingsWin(this);
                _worker = worldAdventurer;
                Controls.Add(_mgmtWindow);
            }
        }

        public void SetMgmtWindow(MgmtWindow newWin)
        {
            Controls.Remove(_mgmtWindow);
            _mgmtWindow = newWin;
            Controls.Add(_mgmtWindow);
        }

        public void Sell()
        {
            _eAction = ActionTypeEnum.Sell;
        }

        public bool Selling()
        {
            return _eAction == ActionTypeEnum.Sell;
        }

        public void PurchaseWorker(WorldAdventurer w, int cost)
        {
            if (w != null)
            {
                _iCost = cost;
                _worker = w;
                _eAction = ActionTypeEnum.Buy;
                SetMgmtWindow(new MainBuildingsWin(this, w));
            }
        }

        public override bool Contains(Point mouse)
        {
            bool rv = false;
            foreach(GUIObject g in Controls)
            {
                if (g.Contains(mouse))
                {
                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public class MgmtWindow : GUIObject
        {
            protected GUIWindow _window;
            protected ManagementScreen _parent;
            protected List<GUIObject> _liButtons;
            protected List<GUIObject> Controls;

            private MgmtWindow(ManagementScreen s)
            {
                _liButtons = new List<GUIObject>();
                Controls = new List<GUIObject>();
                _parent = s;

                _window = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedWin, WIDTH, HEIGHT);
                Controls.Add(_window);
                Width = _window.Width;
                Height = _window.Height;
                Position(_window.Position());
            }

            public override void Update(GameTime gameTime)
            {
                foreach (GUIObject g in Controls)
                {
                    g.Update(gameTime);
                }
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                foreach (GUIObject g in Controls)
                {
                    g.Draw(spriteBatch);
                }
            }

            public virtual bool ProcessHover(Point mouse)
            {
                return false;
            }

            public class MainBuildingsWin : MgmtWindow
            {
                public MainBuildingsWin(ManagementScreen s, WorldAdventurer w = null) : base(s)
                {
                    foreach (WorkerBuilding b in PlayerManager.Buildings)
                    {
                        bool good = false;

                        if(_parent.Action == ActionTypeEnum.Upgrade) { good = b.Level < GameManager.MaxBldgLevel; }
                        else if (w == null || b.CanHold(w)) { good = true; }

                        if (good)
                        {
                            BuildingBox box = new BuildingBox(b);
                            _liButtons.Add(box);
                            Controls.Add(box);
                        }
                    }

                    CreateSpacedGrid(ref _liButtons, _window.InnerTopLeft(), _window.MidWidth(), 3);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    foreach (BuildingBox b in _liButtons)
                    {
                        if (b.Contains(mouse))
                        {
                            if (_parent._eAction == ActionTypeEnum.Upgrade) {
                                b.Building.Upgrade();
                                GameManager.BackToMain();
                            }
                            else{ _parent.HandleBuildingSelection(b.Building); }
                            rv = true;
                            break;
                        }
                    }

                    return rv;
                }

                public override bool ProcessRightButtonClick(Point mouse)
                {
                    GameManager.BackToMain();
                    return false;
                }

                private class BuildingBox : GUIObject
                {
                    GUIButton _btn;
                    WorkerBuilding _b;
                    public WorkerBuilding Building => _b;
                    public BuildingBox(WorkerBuilding b)
                    {
                        _b = b;
                        _btn = new GUIButton(b.GivenName);
                        Width = _btn.Width;
                        Height = _btn.Height;
                    }

                    public override void Update(GameTime gameTime)
                    {
                        _btn.Update(gameTime);
                    }

                    public override void Draw(SpriteBatch spriteBatch)
                    {
                        _btn.Draw(spriteBatch);
                    }

                    public override bool Contains(Point mouse)
                    {
                        return _btn.Contains(mouse);
                    }

                    public override void Position(Vector2 value)
                    {
                        base.Position(value);
                        _btn.Position(value);
                    }
                }
            }
            public class BuildingDetailsWin : MgmtWindow
            {
                public BuildingDetailsWin(ManagementScreen s, WorkerBuilding selectedBuilding) : base(s)
                {
                    foreach (WorldAdventurer w in selectedBuilding.Workers)
                    {
                        WorkerBox btn = new WorkerBox(w);
                        _liButtons.Add(btn);
                        Controls.Add(btn);
                    }
                    CreateSpacedGrid(ref _liButtons, _window.InnerTopLeft(), _window.MidWidth(), 3);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    foreach (WorkerBox w in _liButtons)
                    {
                        if (w.Contains(mouse))
                        {
                            if (_parent.Selling())
                            {
                                _parent.AddTextSelection(w.Worker, "Really sell contract? [Yes:SellContract|No:DoNothing]");
                            }
                            else
                            {
                                _parent.SetMgmtWindow(new WorkerDetailsWin(_parent, w.Worker));
                            }
                            rv = true;
                            break;
                        }
                    }
                    return rv;
                }
                public override bool ProcessRightButtonClick(Point mouse)
                {
                    _parent.SetMgmtWindow(new MainBuildingsWin(_parent));
                    return true;
                }

                private class WorkerBox : GUIObject
                {
                    GUIButton _btn;
                    WorldAdventurer _w;
                    public WorldAdventurer Worker => _w;
                    public WorkerBox(WorldAdventurer w)
                    {
                        _w = w;
                        _btn = new GUIButton(w.Name);
                        Width = _btn.Width;
                        Height = _btn.Height;
                    }

                    public override void Update(GameTime gameTime)
                    {
                        _btn.Update(gameTime);
                    }

                    public override void Draw(SpriteBatch spriteBatch)
                    {
                        _btn.Draw(spriteBatch);
                    }

                    public override bool Contains(Point mouse)
                    {
                        return _btn.Contains(mouse);
                    }

                    public override void Position(Vector2 value)
                    {
                        base.Position(value);
                        _btn.Position(value);
                    }
                }
            }
            public class WorkerDetailsWin : MgmtWindow
            {
                GUIButton _btnMove;
                WorldAdventurer _w;
                GUIText _name, _actionText, _classInfo, _xp, _dmg, _def, _hp, _mag, _spd;
                GUIItemBox _weapon, _armor;
                public WorkerDetailsWin(ManagementScreen s, WorldAdventurer selectedAdventurer) : base(s)
                {
                    _w = selectedAdventurer;
                    _btnMove = new GUIButton("Move");
                    Controls.Add(_btnMove);
                    _btnMove.AnchorToInnerSide(_window, SideEnum.BottomRight);

                    //Data
                    _name = new GUIText(_w.Name);
                    _name.AnchorToInnerSide(_window, SideEnum.TopLeft);

                    _classInfo = new GUIText(_w.Combat.CharacterClass.Name + "    " + _w.Combat.ClassLevel);
                    _classInfo.AnchorAndAlignToObject(_name, SideEnum.Bottom, SideEnum.Left);
                    Controls.Add(_classInfo);

                    _xp = new GUIText("Exp:" + _w.Combat.XP);
                    _xp.AnchorAndAlignToObject(_classInfo, SideEnum.Right, SideEnum.Bottom, 10);
                    Controls.Add(_xp);

                    _weapon = new GUIItemBox(Vector2.Zero, new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _w.Combat.Weapon);
                    _weapon.AnchorToInnerSide(_window, SideEnum.TopRight);

                    _armor = new GUIItemBox(Vector2.Zero + new Vector2(400, 0), new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _w.Combat.Armor);
                    _armor.AnchorAndAlignToObject(_weapon, SideEnum.Left, SideEnum.Bottom);
                    Controls.Add(_armor);

                    AddStat(ref _dmg, null, "Dmg", _w.Combat.StatDmg.ToString());
                    AddStat(ref _def, _dmg, "Def", _w.Combat.StatDmg.ToString());
                    AddStat(ref _hp, _def, "HP", _w.Combat.StatHP.ToString());
                    AddStat(ref _mag, _hp, "Mag", _w.Combat.StatMagic.ToString());
                    AddStat(ref _spd, _mag, "Spd", _w.Combat.StatSpd.ToString());

                    string strAction = "Idle";
                    if (_w.CurrentlyMaking != null)
                    {
                        strAction = "Making " + _w.CurrentlyMaking.ToString() + ", done in " + (int)(_w.CurrentlyMaking.ProcessingTime-_w.ProcessedTime) + " minutes";
                    }
                    else if (_w.Adventuring)
                    {
                        strAction = "Adventuring";
                    }

                    _actionText = new GUIText(strAction);
                    _actionText.AnchorToInnerSide(_window, SideEnum.BottomLeft);
                }

                private void AddStat(ref GUIText curr, GUIText prev, string statName, string stat)
                {
                    string statTemplate = "XXX:XXX";
                    curr = new GUIText(statTemplate);

                    if (prev == null) { curr.AnchorAndAlignToObject(_classInfo, SideEnum.Bottom, SideEnum.Left, 5); }
                    else { curr.AnchorAndAlignToObject(prev, SideEnum.Right, SideEnum.Bottom, 5); }

                    curr.SetText(statName + ":" +stat);
                    Controls.Add(curr);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (_btnMove.Contains(mouse))
                    {
                        _parent.HandleMoveWorker(_w);
                        rv = true;
                    }
                   
                    return rv;
                }
                public override bool ProcessRightButtonClick(Point mouse)
                {
                    _parent.SetMgmtWindow(new BuildingDetailsWin(_parent, _w.Building));
                    return true;
                }

                public override bool ProcessHover(Point mouse)
                {
                    bool rv = false;
                    if (_weapon.ProcessHover(mouse))
                    {
                        rv = true;
                    }
                    if (_armor.ProcessHover(mouse))
                    {
                        rv = true;
                    }
                    return rv;
                }
            }

        }
    }
}