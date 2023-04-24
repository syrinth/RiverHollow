using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    internal class WorldMapScreen : GUIScreen
    {
        readonly GUIImage _gSelector;
        readonly GUIImage _gPlayer;
        
        readonly RHTimer _timerFlicker;
        readonly Dictionary<GUIImage, MapPathInfo> mapDictionary;

        GUIWindow _gWindow;
        string _sSelectedMap;

        public WorldMapScreen(TravelPoint travelPoint)
        {
            //Stop showing the WorldMap
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _timerFlicker = new RHTimer(0.5f);

            AssignBackgroundImage(new GUIImage(new Rectangle(0, 0, 480, 270), @"Textures\Overworld"));

            _gPlayer = new GUIImage(new Rectangle(144, 0, 16, 16), DataManager.DIALOGUE_TEXTURE);
            _gSelector = new GUIImage(new Rectangle(260, 0, 20, 20), DataManager.DIALOGUE_TEXTURE);

            //Key is the map name, KVP key is the map its linked to to get there and the value is the time
            Dictionary<string, MapPathInfo> PathInfoDictionary = new Dictionary<string, MapPathInfo>();
            MapManager.QueryWorldMapPathing(ref PathInfoDictionary, travelPoint);

            mapDictionary = new Dictionary<GUIImage, MapPathInfo>();
            foreach (RHMap map in MapManager.Maps.Values.Where(map => !map.WorldMapNode.Equals(default(MapNode)) && PathInfoDictionary[map.Name].Unlocked))
            {
                GUIImage img = new GUIImage(new Rectangle(160, 0, 16, 16), DataManager.DIALOGUE_TEXTURE);
                img.Position(Util.MultiplyPoint(map.WorldMapNode.MapPosition, GameManager.ScaledPixel));
                mapDictionary[img] = PathInfoDictionary[map.Name];
                AddControl(img);

                if (map == MapManager.CurrentMap)
                {
                    _gPlayer.CenterOnObject(img);
                    _gSelector.CenterOnObject(img);
                    _sSelectedMap = string.Empty;
                }
            }

            AddControl(_gPlayer);
            AddControl(_gSelector);
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
            foreach(KeyValuePair<GUIImage, MapPathInfo> kvp in mapDictionary)
            {
                if (kvp.Key.Contains(mouse))
                {
                    RHMap map = MapManager.Maps[kvp.Value.MapName];
                    if (map == MapManager.CurrentMap)
                    {
                        ReturnToMap();
                    }
                    else
                    {
                        TravelPoint entryPoint = map.DictionaryTravelPoints[kvp.Value.MapConnection];

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

                        GameCalendar.AddTime(0, kvp.Value.Time);

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

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (KeyValuePair<GUIImage, MapPathInfo> kvp in mapDictionary)
            {
                if (kvp.Key.Contains(mouse) && !_sSelectedMap.Equals(kvp.Value.MapName))
                {
                    rv = true;

                    _sSelectedMap = kvp.Value.MapName;

                    RemoveControl(_gWindow);
                    _gSelector.CenterOnObject(kvp.Key);
                    _gWindow = new GUIWindow(GUIWindow.Brown_Window);
                    GUIText text = new GUIText(kvp.Value.MapName + " - " + kvp.Value.Time + " minutes");
                    text.AnchorToInnerSide(_gWindow, GUIObject.SideEnum.TopLeft);
                    _gWindow.Resize();
                    _gWindow.AnchorAndAlignWithSpacing(kvp.Key, GUIObject.SideEnum.Bottom, GUIObject.SideEnum.CenterX, 2);
                    AddControl(_gWindow);
                }
                else if (!kvp.Key.Contains(mouse) && _sSelectedMap.Equals(kvp.Value.MapName))
                {
                    _sSelectedMap = string.Empty;
                    RemoveControl(_gWindow);
                }
            }

            return rv;
        }
        private void ReturnToMap()
        {
            GameManager.GoToHUDScreen();
            PlayerManager.PlayerActor.Facing = Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing);
        }
    }
}
