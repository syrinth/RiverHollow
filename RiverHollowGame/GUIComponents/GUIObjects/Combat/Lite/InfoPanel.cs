using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class InfoPanel : GUIObject
    {
        int displayedHP;
        CombatActor _actor;

        GUISprite _actorSprite;
        GUIText _gName;
        GUIText _gHPValue;

        GUIImage _gHPIcon;
        StatusPanel _gStatusPanel;

        public InfoPanel() {
            GUIImage panel = new GUIImage(new Rectangle(0, 0, 112, 43), DataManager.COMBAT_TEXTURE);
            AddControl(panel);

            Width = panel.Width;
            Height = panel.Height;
        }

        public InfoPanel(CombatActor act) : this()
        {
            SetActor(act);
        }

        public bool ShouldRefresh(CombatActor act)
        {
            if (act == _actor)
            {
                if (displayedHP == act.CurrentHP) { return false; }
            }

            return true;
        }

        public void SetActor(CombatActor actor)
        {
            RemoveControl(_actorSprite);
            RemoveControl(_gName);
            RemoveControl(_gHPIcon);
            RemoveControl(_gHPValue);
            RemoveControl(_gStatusPanel);

            _actor = actor;

            _actorSprite = new GUISprite(_actor.BodySprite, true);
            _actorSprite.PlayAnimation(actor.IsCritical() ? AnimationEnum.Critical : AnimationEnum.Idle);
            _actorSprite.Position(Position() + new Vector2(ScaleIt(4), ScaleIt(4)));
            AddControl(_actorSprite);

            _gName = new GUIText(_actor.Name);
            _gName.Position(Position() + new Vector2(ScaleIt(41), ScaleIt(2)));
            AddControl(_gName);

            _gHPIcon = DataManager.GetIcon(GameIconEnum.MaxHealth);
            _gHPIcon.Position(Position() + new Vector2(ScaleIt(42), ScaleIt(18)));
            AddControl(_gHPIcon);

            displayedHP = _actor.CurrentHP;
            _gHPValue = new GUIText(_actor.CurrentHP + @"/" + _actor.MaxHP, DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
            _gHPValue.Position(Position() + new Vector2(ScaleIt(53), ScaleIt(19)));
            AddControl(_gHPValue);

            if(actor.StatusEffects.Count > 0)
            {
                //TODO:add StatusEffectPanel here
                _gStatusPanel = new StatusPanel(actor.StatusEffects);
                _gStatusPanel.Position(Position() + new Vector2(ScaleIt(40), ScaleIt(27)));
                AddControl(_gStatusPanel);
            }
        }

        private class StatusPanel : GUIObject
        {
            const int REFRESH_RATE = 3;
            GUIImage _gTimer;
            GUIImage _gPanel;
            List<StatusPanelEffectIcon> _liStatusIcons;
            StatusPanelEffectIcon[] _arrVisibleIcons;

            int _iStatusIndex = 0;
            double _dTimer = 0;
            public StatusPanel(List<StatusEffect> effects)
            {
                _arrVisibleIcons = new StatusPanelEffectIcon[3];

                _liStatusIcons = new List<StatusPanelEffectIcon>();
                _gPanel = new GUIImage(new Rectangle(112, 96, 66, 16), DataManager.COMBAT_TEXTURE);
                AddControl(_gPanel);

                Width = _gPanel.Width;
                Height = _gPanel.Height;

                _gTimer = DataManager.GetIcon(GameIconEnum.Timer);
                _gTimer.ScaledMoveBy(4, 5);
                AddControl(_gTimer);

                foreach(StatusEffect e in effects)
                {
                    foreach(KeyValuePair<AttributeEnum, string> kvp in e.AffectedAttributes)
                    {
                        //There exists a StatusIcon already that is 
                        //if(_liStatusIcons.Find(x => x.Type == e.EffectType && x.Attribute == kvp.Key) != null)
                        //{

                        //}

                        _liStatusIcons.Add(new StatusPanelEffectIcon(new GUIEffectDetailIcon(kvp.Key, e.EffectType), e.Duration));
                    }

                    if(e.Potency != -1)
                    {
                        _liStatusIcons.Add(new StatusPanelEffectIcon(DataManager.GetIcon(GameIconEnum.MagicDamage), e.Duration));
                    }
                }

                PlaceIcons();
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);

                if(_liStatusIcons.Count > 3)
                {
                    _dTimer += gTime.ElapsedGameTime.TotalSeconds;
                    if(_dTimer >= REFRESH_RATE)
                    {
                        _dTimer = 0;

                        if (_liStatusIcons.Count >= _iStatusIndex + 3) { _iStatusIndex += 3; }
                        else { _iStatusIndex = 0; }
                        
                        PlaceIcons();
                    }
                }
            }

            private void PlaceIcons()
            {
                foreach(GUIObject obj in _arrVisibleIcons) { RemoveControl(obj); }

                int visibleIndex = 0;
                for (int i = _iStatusIndex; i < _iStatusIndex + 3 && i < _liStatusIcons.Count; i++)
                {
                    _arrVisibleIcons[visibleIndex] = _liStatusIcons[i];
                    PositionIcon(_liStatusIcons[i], visibleIndex++);
                    AddControl(_liStatusIcons[i]);

                }
            }

            private void PositionIcon(GUIObject newIcon, int index)
            {
                if (index == 0)
                {
                    newIcon.AnchorAndAlignToObject(_gTimer, SideEnum.Right, SideEnum.CenterY);
                    newIcon.ScaledMoveBy(2, 0);
                }
                else
                {
                    newIcon.AnchorAndAlignToObject(_arrVisibleIcons[index-1], SideEnum.Right, SideEnum.CenterY);
                    newIcon.ScaledMoveBy(1, 0);
                }
            }
        }

        private class StatusPanelEffectIcon : GUIObject
        {
            public StatusPanelEffectIcon(GUIObject obj, int Duration)
            {
                AddControl(obj);

                GUIText duration = new GUIText(Duration.ToString(), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
                duration.AnchorAndAlignToObject(obj, SideEnum.Right, SideEnum.CenterY);
                duration.ScaledMoveBy(1, 0);
                AddControl(duration);

                Width = duration.Right - obj.Left;
                Height = obj.Height;
            }
        }
    }
}
