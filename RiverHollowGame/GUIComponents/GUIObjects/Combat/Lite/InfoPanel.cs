using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class InfoPanel : GUIObject
    {
        int displayedHP;
        CombatActor _actor;

        GUISprite _actorSprite;
        GUIText _gName;
        GUIText _gHPValue;

        GUIImage _gHP;
        GUIImage _gPanel;
        StatusPanel _gStatusPanel;

        public InfoPanel() {
            _gPanel = new GUIImage(new Rectangle(0, 0, 112, 43), DataManager.COMBAT_TEXTURE);
            AddControl(_gPanel);

            Width = _gPanel.Width;
            Height = _gPanel.Height;
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
            RemoveControl(_gHP);
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

            _gHP = DataManager.GetIcon(GameIconEnum.MaxHealth);
            _gHP.Position(Position() + new Vector2(ScaleIt(42), ScaleIt(18)));
            AddControl(_gHP);

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
            GUIImage _gPanel;

            List<StatusIcon> _liStatusIcons;

            StatusIcon[] _arrVisibleIcons;
            public StatusPanel(List<StatusEffect> effects)
            {
                _arrVisibleIcons = new StatusIcon[3];

                _liStatusIcons = new List<StatusIcon>();
                _gPanel = new GUIImage(new Rectangle(112, 96, 66, 16), DataManager.COMBAT_TEXTURE);
                AddControl(_gPanel);

                Width = _gPanel.Width;
                Height = _gPanel.Height;

                GUIImage temp = DataManager.GetIcon(GameIconEnum.Timer);
                temp.ScaledMoveBy(4, 5);
                AddControl(temp);


                foreach(StatusEffect e in effects)
                {
                    foreach(KeyValuePair<AttributeEnum, int> kvp in e.AttributeEffects)
                    { 
                        //There exists a StatusIcon already that is 
                        if(_liStatusIcons.Find(x => x.Type == e.EffectType && x.Attribute == kvp.Key) != null)
                        {

                        }

                        StatusIcon newIcon = new StatusIcon(e.EffectType, kvp.Key, e.Duration);
                        _liStatusIcons.Add(newIcon);
                    }
                }
            }

            private class StatusIcon : GUIObject
            {
                public StatusTypeEnum Type { get; private set; }
                public AttributeEnum Attribute { get; private set; }
                public int Duration { get; private set; }
                GUIText _gDuration;
                GUIImage _gArrow;
                GUIImage _gIcon;

                public StatusIcon(StatusTypeEnum e, AttributeEnum attribute, int duration)
                {
                    Type = e;
                    Attribute = attribute;
                    Duration = duration;

                    switch (e)
                    {
                        case StatusTypeEnum.Buff:
                            _gArrow = DataManager.GetIcon(GameIconEnum.BuffArrow);
                            _gIcon = DataManager.GetIcon(GetGameIconFromAttribute(attribute));
                            _gIcon.AnchorAndAlignToObject(_gArrow, SideEnum.Bottom, SideEnum.CenterX);
                            _gIcon.MoveBy(new Vector2(0, -(_gIcon.Height/2)));

                            _gDuration = new GUIText(duration.ToString(), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
                            _gDuration.ScaledMoveBy(11, 3);

                            Height = _gIcon.Bottom - _gArrow.Top;
                            Width = _gDuration.Right - _gArrow.Left;

                            AddControl(_gArrow);
                            AddControl(_gDuration);
                            break;
                        case StatusTypeEnum.Debuff:
                            _gArrow = DataManager.GetIcon(GameIconEnum.DebuffArrow);
                            _gIcon = DataManager.GetIcon(GetGameIconFromAttribute(attribute));

                            _gArrow.AnchorAndAlignToObject(_gIcon, SideEnum.Bottom, SideEnum.CenterX);
                            _gDuration.ScaledMoveBy(11, 3);

                            Height = _gArrow.Bottom - _gIcon.Top;
                            Width = _gDuration.Right - _gArrow.Left;

                            AddControl(_gArrow);
                            AddControl(_gDuration);
                            break;
                        case StatusTypeEnum.DoT:
                            _gIcon = DataManager.GetIcon(GameIconEnum.PhysicalDamage);
                            break;
                        case StatusTypeEnum.HoT:
                            _gIcon = DataManager.GetIcon(GameIconEnum.Heal);
                            break;
                    }

                    AddControl(_gIcon);
                }
            }
        }
    }
}
