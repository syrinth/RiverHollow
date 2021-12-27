using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class TurnOrder : GUIObject
    {
        enum ActorIconType { CurrentTurn, Monster, NextTurn, Party };
        List<ActorIcon> _liActorIcons;
        List<GUIImage> _liSegments;

        readonly ActorIcon _nextTurn;
        readonly ActorIcon _currentTurn;

        public TurnOrder(IList<CombatActor> TurnOrder)
        {
            _currentTurn = new ActorIcon(ActorIconType.CurrentTurn);
            AddControl(_currentTurn);

            _liSegments = new List<GUIImage>();
            _liActorIcons = new List<ActorIcon>();

            //Draws the Current Turn arrow
            GUIImage temp = new GUIImage(new Rectangle(131, 1, 5, 8), DataManager.COMBAT_TEXTURE);
            temp.AnchorAndAlignToObject(_currentTurn, SideEnum.Right, SideEnum.Top);
            temp.ScaledMoveBy(0, 8);
            AddControl(temp);

            for (int i = 0; i < TurnOrder.Count - 1; i++)
            {
                GUIImage segment = new GUIImage(new Rectangle(136, 1, 17, 8), DataManager.COMBAT_TEXTURE);
                segment.AnchorAndAlignToObject(temp, SideEnum.Right, SideEnum.Top);
                AddControl(segment);
                _liSegments.Add(segment);

                temp = segment;
                if (i < TurnOrder.Count - 2)
                {
                    GUIImage arrow = new GUIImage(new Rectangle(153, 1, 3, 8), DataManager.COMBAT_TEXTURE);
                    arrow.AnchorAndAlignToObject(temp, SideEnum.Right, SideEnum.Top);
                    AddControl(arrow);
                    temp = arrow;
                }
            }

            GUIImage endPiece = new GUIImage(new Rectangle(173, 1, 3, 8), DataManager.COMBAT_TEXTURE);
            endPiece.AnchorAndAlignToObject(temp, SideEnum.Right, SideEnum.Top);
            AddControl(endPiece);
            temp = endPiece;

            _nextTurn = new ActorIcon(ActorIconType.NextTurn);
            _nextTurn.AnchorAndAlignToObject(temp, SideEnum.Right, SideEnum.Top);
            _nextTurn.ScaledMoveBy(1, -5);
            AddControl(_nextTurn);


            Width = _nextTurn.Right - _currentTurn.Left;
            Height = _nextTurn.Bottom - _currentTurn.Top;

            AnchorToScreen(SideEnum.Top);
            ScaledMoveBy(0, 26);

            DisplayNewTurn(TurnOrder);
        }

        public void DisplayNewTurn(IList<CombatActor> actors)
        {
            _currentTurn.LinkActor(actors[0]);

            for (int i = 1; i < actors.Count; i++)
            {
                ActorIconType iconType = (actors[i].IsActorType(ActorEnum.Monster) ? ActorIconType.Monster : ActorIconType.Party);

                ActorIcon newIcon = new ActorIcon(iconType);
                newIcon.LinkActor(actors[i]);

                if(iconType == ActorIconType.Monster)
                {
                    newIcon.AnchorAndAlignToObject(_liSegments[i - 1], SideEnum.Bottom, SideEnum.Left);
                    newIcon.ScaledMoveBy(-1, -1);
                }
                else if(iconType == ActorIconType.Party)
                {
                    newIcon.AnchorAndAlignToObject(_liSegments[i - 1], SideEnum.Top, SideEnum.Left);
                    newIcon.ScaledMoveBy(-1, 1);
                }

                AddControl(newIcon);

                _liActorIcons.Add(newIcon);
            }

            _nextTurn.LinkActor(CombatManager.StartingActor);
        }

        private class ActorIcon : GUIObject
        {
            GUIImage _actorIcon;
            CombatActor _linkedActor;

            readonly ActorIconType _eIconType;
            readonly GUIImage _iconWindow;

            public ActorIcon(ActorIconType eIconType)
            {
                _eIconType = eIconType;

                Width = ScaleIt(18);
                switch (eIconType)
                {
                    case ActorIconType.CurrentTurn:
                        _iconWindow = new GUIImage(new Rectangle(112, 0, 18, 21), ScaleIt(18), ScaleIt(21), DataManager.COMBAT_TEXTURE);
                        Height = ScaleIt(21);
                        break;
                    case ActorIconType.Monster:
                        _iconWindow = new GUIImage(new Rectangle(177, 0, 18, 18), ScaleIt(18), ScaleIt(18), DataManager.COMBAT_TEXTURE);
                        Height = ScaleIt(18);
                        break;
                    case ActorIconType.NextTurn:
                        _iconWindow = new GUIImage(new Rectangle(177, 19, 18, 23), ScaleIt(18), ScaleIt(23), DataManager.COMBAT_TEXTURE);
                        Height = ScaleIt(23);
                        break;
                    case ActorIconType.Party:
                        _iconWindow = new GUIImage(new Rectangle(196, 0, 18, 18), ScaleIt(18), ScaleIt(18), DataManager.COMBAT_TEXTURE);
                        Height = ScaleIt(18);
                        break;
                }

                AddControl(_iconWindow);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override bool ProcessHover(Point mouse)
            {
                //TODO: Show icon on matching character
                return base.ProcessHover(mouse);
            }

            public void LinkActor(CombatActor actor)
            {
                _linkedActor = actor;
                _actorIcon = actor.GetIcon();

                switch (_eIconType)
                {
                    case ActorIconType.CurrentTurn:
                        _actorIcon.Position(Position());
                        _actorIcon.PositionAdd(new Vector2(0, ScaleIt(3)));
                        break;
                    default:
                        _actorIcon.Position(Position());
                        break;
                }

                AddControl(_actorIcon);
            }
        }
    }
}
