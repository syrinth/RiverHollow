using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static RiverHollow.GUIComponents.GUIUtils;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    internal class WorldMapScreen : GUIScreen
    {
        private enum WorldMapEnum { Adventure, Travel };
        private readonly WorldMapEnum _eState;

        readonly GUIImage _gSelector;
        
        RHTimer _timerFlicker;
        List<GUIMapNode> _liNodeList;

        public WorldMapScreen()
        {
            _eState = WorldMapEnum.Adventure;
            Setup();

            foreach (var adventure in AdventureManager.GetUnlockedAdventures())
            {
                var node = new GUIAdventureNode(UpdateAdventureInfo);
                node.Position(Util.MultiplyPoint(adventure.Location, GameManager.ScaledPixel));
                AddControl(node);
                _liNodeList.Add(node);
            }
        }

        public WorldMapScreen(TravelPoint travelPoint)
        {
            _eState = WorldMapEnum.Travel;

            Setup();

            //Key is the map name, KVP key is the map its linked to to get there and the value is the time
            Dictionary<string, MapPathInfo> PathInfoDictionary = new Dictionary<string, MapPathInfo>();
            MapManager.QueryWorldMapPathing(ref PathInfoDictionary, travelPoint);

            GUITravelNode currentNode = null;
            foreach (RHMap map in MapManager.Maps.Values.Where(map => !map.WorldMapNode.Equals(default(MapNode)) && PathInfoDictionary[map.Name].Unlocked))
            {
                var node = new GUITravelNode(PathInfoDictionary[map.Name], UpdateTravelInfo);
                node.Position(Util.MultiplyPoint(map.WorldMapNode.MapPosition, GameManager.ScaledPixel));
                AddControl(node);

                if (map == MapManager.CurrentMap)
                {
                    currentNode = node;
                }

                _liNodeList.Add(node);
            }

            currentNode.SetActor(PlayerManager.PlayerActor);

            _gSelector = new GUIImage(GUIUtils.SELECT_CORNER);
            _gSelector.CenterOnObject(currentNode, ParentRuleEnum.Skip);
        }

        private void Setup()
        {
            //Stop showing the WorldMap
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _timerFlicker = new RHTimer(0.5f);

            AssignBackgroundImage(new GUIImage(GUIUtils.WORLDMAP, @"Textures\Overworld"));

            _liNodeList = new List<GUIMapNode>();
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (RHTimer.TimerCheck(_timerFlicker, gTime))
            {
                _timerFlicker.Reset();
                _liNodeList.ForEach(node => node.BlinkActor());
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            foreach(GUIMapNode node in _liNodeList)
            {
                if (node is GUITravelNode)
                {
                    node.ProcessLeftButtonClick(mouse);
                }
            }

            return true;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GameManager.CloseWorldMap(_eState == WorldMapEnum.Travel);
            return true;
        }

        private void UpdateTravelInfo(GUIMapNode obj)
        {
            if (obj is GUITravelNode travelNode)
            {
                _gSelector.CenterOnObject(obj);

                var window = new GUIWindow(GUIUtils.WINDOW_BROWN);
                GUIText text = new GUIText(travelNode.MapInfo.MapName + " - " + travelNode.MapInfo.Time + " minutes");
                text.AnchorToInnerSide(window, GUIObject.SideEnum.TopLeft);
                window.Resize();
                window.AnchorAndAlignWithSpacing(obj, GUIObject.SideEnum.Bottom, GUIObject.SideEnum.CenterX, 2);

                GUIManager.OpenHoverObject(window, obj.DrawRectangle, true);
            }
        }

        private void UpdateAdventureInfo(GUIMapNode obj)
        {
            if (obj is GUITravelNode travelNode)
            {
                _gSelector.CenterOnObject(obj);

                var window = new GUIWindow(GUIUtils.WINDOW_BROWN);
                GUIText text = new GUIText(travelNode.MapInfo.MapName + " - " + travelNode.MapInfo.Time + " minutes");
                text.AnchorToInnerSide(window, GUIObject.SideEnum.TopLeft);
                window.Resize();
                window.AnchorAndAlignWithSpacing(obj, GUIObject.SideEnum.Bottom, GUIObject.SideEnum.CenterX, 2);

                GUIManager.OpenHoverObject(window, obj.DrawRectangle, true);
            }
        }

        public class GUIMapNode : GUIImage
        {
            protected GUIImage _gActor;

            public delegate void HoverMethod(GUIMapNode obj);
            protected HoverMethod _delAction;

            public GUIMapNode(HoverMethod action) : base(GUIUtils.ICON_MAP_MARKER)
            {
                HoverControls = false;
                _delAction = action;
            }

            public void SetActor(Actor a)
            {
                if (a != null)
                {
                    if (a is PlayerCharacter)
                    {
                        _gActor = new GUIImage(GUIUtils.ICON_FACE);
                    }

                    _gActor.CenterOnObject(this, ParentRuleEnum.Skip);
                }
                else
                {
                    _gActor = null;
                }
            }

            protected override void BeginHover()
            {
                _delAction(this);
            }

            public void BlinkActor()
            {
                _gActor?.Show(!_gActor.Show());
            }
        }

        public class GUIAdventureNode : GUIMapNode
        {
            public GUIAdventureNode(HoverMethod action) : base(action)
            {

            }
        }

        public class GUITravelNode : GUIMapNode
        {
            public MapPathInfo MapInfo { get; }

            public GUITravelNode(MapPathInfo info, HoverMethod action) : base(action)
            {
                MapInfo = info;
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (Contains(mouse))
                {
                    rv = true;

                    RHMap map = MapManager.Maps[MapInfo.MapName];
                    if (map == MapManager.CurrentMap)
                    {
                        GameManager.CloseWorldMap(false);
                    }
                    else
                    {
                        TravelPoint entryPoint = map.GetTravelPoint(MapInfo.MapConnection);

                        DirectionEnum entryDir = entryPoint.EntranceDir;
                        Point newPos = entryPoint.Center;

                        switch (entryDir)
                        {
                            case DirectionEnum.Left:
                                newPos += new Point(-(PlayerManager.PlayerActor.CollisionBox.Width + (entryPoint.CollisionBox.Width) / 2), 0);
                                break;
                            case DirectionEnum.Right:
                                newPos += new Point(entryPoint.CollisionBox.Width / 2, 0);
                                break;
                            case DirectionEnum.Up:
                                newPos += new Point(-PlayerManager.PlayerActor.CollisionBox.Width / 2, -(PlayerManager.PlayerActor.CollisionBox.Height + (entryPoint.CollisionBox.Height) / 2));
                                break;
                            case DirectionEnum.Down:
                                newPos += new Point(-PlayerManager.PlayerActor.CollisionBox.Width / 2, entryPoint.CollisionBox.Height / 2);
                                break;
                        }
                        PlayerManager.PlayerActor.SetFacing(entryDir);
                        map.SpawnMapEntities();

                        PlayerManager.PlayerActor.ActivePet?.ChangeState(NPCStateEnum.Alert);
                        GameManager.GoToHUDScreen();
                        MapManager.FadeToNewMap(map, newPos, entryDir);

                        GameCalendar.AddTime(0, MapInfo.Time);

                        PlayerManager.ReleaseTile();
                    }
                }

                return rv;
            }
        }
    }
}
