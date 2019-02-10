using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Buildings;
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

        public void HandleBuildingSelection(Building selectedBuilding)
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
                    GUIManager.SetScreen(new NamingScreen(_worker));
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

                _window = new GUIWindow(GUIWindow.RedWin, WIDTH, HEIGHT);
                _window.CenterOnScreen();

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
                    foreach (Building b in PlayerManager.Buildings)
                    {
                        bool good = false;

                        if(_parent.Action == ActionTypeEnum.Upgrade) { good = b.Level < GameManager.MaxBldgLevel; }
                        else if (w == null || b.CanHold(w)) { good = true; }

                        if (good)
                        {
                            BuildingBox box = new BuildingBox(b, w != null);
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
                    bool _bShowWorkers;
                    GUIButton _btn;
                    GUIText _gText;
                    Building _b;
                    public Building Building => _b;

                    public BuildingBox(Building b, bool showWorkerNum)
                    {
                        _b = b;
                        _btn = new GUIButton(b.GivenName);
                        _bShowWorkers = showWorkerNum;

                        _gText = new GUIText(b.Workers.Count + @"/" + b.MaxWorkers);
                        _gText.AnchorAndAlignToObject(_btn, SideEnum.Bottom, SideEnum.Left);
                        Width = _btn.Width > _gText.Width ? _btn.Width : _gText.Width;
                        Height = _btn.Height + _gText.Height;
                    }

                    public override void Update(GameTime gameTime)
                    {
                        _btn.Update(gameTime);
                    }

                    public override void Draw(SpriteBatch spriteBatch)
                    {
                        _btn.Draw(spriteBatch);
                        if (_bShowWorkers)
                        {
                            _gText.Draw(spriteBatch);
                        }
                    }

                    public override bool Contains(Point mouse)
                    {
                        return _btn.Contains(mouse);
                    }

                    public override void Position(Vector2 value)
                    {
                        base.Position(value);
                        _btn.Position(value);
                        _gText.AnchorAndAlignToObject(_btn, SideEnum.Bottom, SideEnum.CenterX);
                    }
                }
            }
            public class BuildingDetailsWin : MgmtWindow
            {
                public BuildingDetailsWin(ManagementScreen s, Building selectedBuilding) : base(s)
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
                                GameManager.gmNPC = w.Worker;
                                _parent.AddTextSelection("Really sell contract? [Yes:SellContract|No:DoNothing]");
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
                WorldAdventurer _character;
                GUIText _gName, _actionText, _gClass, _gXP, _gStr, _gDef, _gVit, _gMagic, _gRes, _gSpd;
                GUIItemBox _weapon, _armor;
                public WorkerDetailsWin(ManagementScreen s, WorldAdventurer selectedAdventurer) : base(s)
                {
                    int statSpacing = 10;
                    _character = selectedAdventurer;
                    _btnMove = new GUIButton("Move", 128, 32);
                    _btnMove.AnchorToInnerSide(_window, SideEnum.BottomRight);

                    string nameLen = "";
                    for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

                    _gName = new GUIText(nameLen);
                    _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
                    _gClass = new GUIText("XXXXXXXX 99");
                    _gClass.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);

                    _gXP = new GUIText(@"9999/9999");//new GUIText(_character.XP + @"/" + CombatAdventurer.LevelRange[_character.ClassLevel]);
                    _gXP.AnchorAndAlignToObject(_gClass, SideEnum.Right, SideEnum.Top, 10);

                    _gName.SetText(_character.Name);
                    _gClass.SetText(_character.Combat.CharacterClass.Name + " " + _character.Combat.ClassLevel);
                    _gXP.SetText("Exp:" + _character.Combat.XP);

                    _weapon = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Combat.Weapon);
                    _weapon.AnchorToInnerSide(_window, SideEnum.TopRight);

                    _armor = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Combat.Armor);
                    _armor.AnchorAndAlignToObject(_weapon, SideEnum.Left, SideEnum.Bottom);

                    _gStr = new GUIText("Dmg: 999");
                    _gDef = new GUIText("Def: 999");
                    _gVit = new GUIText("HP: 999");
                    _gMagic = new GUIText("Mag: 999");
                    _gRes = new GUIText("Res: 999");
                    _gSpd = new GUIText("Spd: 999");
                    _gMagic.AnchorAndAlignToObject(_gClass, SideEnum.Bottom, SideEnum.Left);
                    _gDef.AnchorAndAlignToObject(_gMagic, SideEnum.Right, SideEnum.Bottom, statSpacing);
                    _gStr.AnchorAndAlignToObject(_gDef, SideEnum.Right, SideEnum.Bottom, statSpacing);
                    _gVit.AnchorAndAlignToObject(_gStr, SideEnum.Right, SideEnum.Bottom, statSpacing);
                    _gSpd.AnchorAndAlignToObject(_gVit, SideEnum.Right, SideEnum.Bottom, statSpacing);
                    _gRes.AnchorAndAlignToObject(_gSpd, SideEnum.Right, SideEnum.Bottom, statSpacing);

                    _gStr.SetText("Str: " + _character.Combat.StatStr);
                    _gDef.SetText("Def: " + _character.Combat.StatDef);
                    _gVit.SetText("Vit: " + _character.Combat.StatVit);
                    _gMagic.SetText("Mag: " + _character.Combat.StatMag);
                    _gRes.SetText("Res: " + _character.Combat.StatRes);
                    _gSpd.SetText("Spd: " + _character.Combat.StatSpd);

                    string strAction = "Idle";
                    if (_character.CurrentlyMaking != null)
                    {
                        strAction = "Making " + _character.CurrentlyMaking.ToString() + ", done in " + (int)(_character.CurrentlyMaking.ProcessingTime-_character.ProcessedTime) + " minutes";
                    }
                    else if (_character.Adventuring)
                    {
                        strAction = "Adventuring";
                    }

                    _actionText = new GUIText(strAction);
                    _actionText.AnchorToInnerSide(_window, SideEnum.BottomLeft);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (_btnMove.Contains(mouse))
                    {
                        _parent.HandleMoveWorker(_character);
                        rv = true;
                    }
                   
                    return rv;
                }
                public override bool ProcessRightButtonClick(Point mouse)
                {
                    _parent.SetMgmtWindow(new BuildingDetailsWin(_parent, _character.Building));
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