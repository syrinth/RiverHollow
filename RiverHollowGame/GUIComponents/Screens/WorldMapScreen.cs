using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        readonly GUIImage _gSelector;
        readonly GUIImage _gPlayer;
        
        readonly RHTimer _timerFlicker;
        readonly List<GUIMapNode> nodeList;

        public WorldMapScreen(TravelPoint travelPoint)
        {
            //Stop showing the WorldMap
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _timerFlicker = new RHTimer(0.5f);

            AssignBackgroundImage(new GUIImage(GUIUtils.WORLDMAP, @"Textures\Overworld"));

            //Key is the map name, KVP key is the map its linked to to get there and the value is the time
            Dictionary<string, MapPathInfo> PathInfoDictionary = new Dictionary<string, MapPathInfo>();
            MapManager.QueryWorldMapPathing(ref PathInfoDictionary, travelPoint);

            GUIMapNode currentNode = null;

            nodeList = new List<GUIMapNode>();
            foreach (RHMap map in MapManager.Maps.Values.Where(map => !map.WorldMapNode.Equals(default(MapNode)) && PathInfoDictionary[map.Name].Unlocked))
            {
                var node = new GUIMapNode(PathInfoDictionary[map.Name], UpdateInfo);
                node.Position(Util.MultiplyPoint(map.WorldMapNode.MapPosition, GameManager.ScaledPixel));
                AddControl(node);

                if (map == MapManager.CurrentMap)
                {
                    currentNode = node;
                }
            }

            _gPlayer = new GUIImage(GUIUtils.ICON_FACE);
            _gSelector = new GUIImage(GUIUtils.SELECT_CORNER);

            _gPlayer.CenterOnObject(currentNode, ParentRuleEnum.Skip);
            _gSelector.CenterOnObject(currentNode, ParentRuleEnum.Skip);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (RHTimer.TimerCheck(_timerFlicker, gTime))
            {
                _timerFlicker.Reset();
                _gPlayer.Show(!_gPlayer.Show());
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            foreach(GUIMapNode node in nodeList)
            {
                if (node.Contains(mouse))
                {
                    RHMap map = MapManager.Maps[node.MapInfo.MapName];
                    if (map == MapManager.CurrentMap)
                    {
                        ReturnToMap();
                    }
                    else
                    {
                        TravelPoint entryPoint = map.DictionaryTravelPoints[node.MapInfo.MapConnection];

                        DirectionEnum entryDir = entryPoint.Dir;
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
                        PlayerManager.PlayerActor.Facing = entryDir;
                        map.SpawnMapEntities();

                        PlayerManager.PlayerActor.ActivePet?.ChangeState(NPCStateEnum.Alert);
                        GameManager.GoToHUDScreen();
                        MapManager.FadeToNewMap(map, newPos);

                        GameCalendar.AddTime(0, node.MapInfo.Time);

                        PlayerManager.ReleaseTile();
                    }
                }
            }

            return true;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            ReturnToMap();
            return true;
        }

        private void UpdateInfo(GUIMapNode obj)
        {
            _gSelector.CenterOnObject(obj);

            var window = new GUIWindow(GUIUtils.Brown_Window);
            GUIText text = new GUIText(obj.MapInfo.MapName + " - " + obj.MapInfo.Time + " minutes");
            text.AnchorToInnerSide(window, GUIObject.SideEnum.TopLeft);
            window.Resize();
            window.AnchorAndAlignWithSpacing(obj, GUIObject.SideEnum.Bottom, GUIObject.SideEnum.CenterX, 2);

            GUIManager.OpenHoverObject(window, obj.DrawRectangle, true);
        }

        private void ReturnToMap()
        {
            GameManager.GoToHUDScreen();
            PlayerManager.PlayerActor.Facing = Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing);
        }

        public class GUIMapNode : GUIImage
        {
            public MapPathInfo MapInfo { get; }

            public delegate void HoverMethod(GUIMapNode obj);
            private HoverMethod _delAction;

            public GUIMapNode(MapPathInfo info, HoverMethod action) : base(GUIUtils.ICON_MAP_MARKER)
            {
                HoverControls = false;
                MapInfo = info;
                _delAction = action;
            }

            protected override void BeginHover()
            {
                _delAction(this);
            }
        }
    }
}
