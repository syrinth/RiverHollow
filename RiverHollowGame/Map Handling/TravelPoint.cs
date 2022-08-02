using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Map_Handling
{
    public class TravelPoint
    {
        public Building TargetBuilding { get; private set; }
        public Rectangle CollisionBox { get; private set; }
        public Point Location => CollisionBox.Location;
        string _sMapName;

        private string _sLinkedMapName = string.Empty;
        public int LinkedBuildingID => TargetBuilding != null ? TargetBuilding.ID : -1;
        public string LinkedMap => (TargetBuilding != null ? TargetBuilding.MapName : _sLinkedMapName);
        public Vector2 Center => CollisionBox.Center.ToVector2();
        public bool IsDoor { get; private set; }
        public bool IsActive { get; private set; } = false;
        private bool _bNoMove;
        public bool NoMove => _bNoMove;

        private bool _bModular = false;
        public bool Modular => _bModular;

        private int _iDungeonInfoID = -1;
        public int DungeonInfoID => _iDungeonInfoID;

        private string _sGoToMap;
        public string GoToMap => _sGoToMap;

        DirectionEnum _eEntranceDir;
        public DirectionEnum Dir => _eEntranceDir;

        public TravelPoint(TiledMapObject obj, string mapName)
        {
            _sMapName = mapName;
            CollisionBox = Util.FloatRectangle(obj.Position, obj.Size.Width, obj.Size.Height);
            if (obj.Properties.ContainsKey("Map"))
            {
                _sLinkedMapName = obj.Properties["Map"] == "Home" ? Constants.TOWN_MAP_NAME : obj.Properties["Map"];
                IsActive = true;
            }

            Util.AssignValue(ref _eEntranceDir, "EntranceDir", obj.Properties);
            Util.AssignValue(ref _iDungeonInfoID, "DungeonID", obj.Properties);
            Util.AssignValue(ref _sGoToMap, "GoTo", obj.Properties);
            Util.AssignValue(ref _bModular, "Modular", obj.Properties);
            Util.AssignValue(ref _bNoMove, "NoMove", obj.Properties);

            if (_iDungeonInfoID > -1) { IsActive = true; }

        }
        public TravelPoint(Building b, string mapName, int buildingID)
        {
            TargetBuilding = b;
            _sMapName = mapName;
            CollisionBox = b.TravelBox;
            _eEntranceDir = DirectionEnum.Down;
            IsDoor = true;
            IsActive = true;
        }

        public bool Intersects(Rectangle value)
        {
            return CollisionBox.Intersects(value);
        }

        /// <summary>
        /// USe to determine the exit point of the TravelObject based on the distance of the Actor to the
        /// linked TravelObject they interacted with
        /// </summary>
        /// <param name="oldPointCenter">The center of the previous TravelPoint</param>
        /// <param name="c">The moving Actor</param>
        /// <returns></returns>
        public Vector2 FindLinkedPointPosition(Vector2 oldPointCenter, WorldActor c)
        {
            //Find the difference between the position of the center of the actor's collisionBox
            //and the TravelPoint that the actor interacted with.
            Point actorCollisionCenter = c.CollisionBox.Center;
            Vector2 vDiff = actorCollisionCenter.ToVector2() - oldPointCenter;

            //If we move Left/Right, ignore the X axis, Up/Down, ignore the Y axis then just set
            //the difference in the relevant axis to the difference between the centers of those two boxes
            switch (_eEntranceDir)
            {
                case DirectionEnum.Left:
                    vDiff.X = -1 * (CollisionBox.Width / 2 + c.CollisionBox.Width / 2);
                    break;
                case DirectionEnum.Right:
                    vDiff.X = (CollisionBox.Width / 2 + c.CollisionBox.Width / 2);
                    break;
                case DirectionEnum.Up:
                    vDiff.Y = -1 * (CollisionBox.Height / 2 + c.CollisionBox.Height / 2);
                    break;
                case DirectionEnum.Down:
                    vDiff.Y = (CollisionBox.Height / 2 + c.CollisionBox.Height / 2);
                    break;
            }

            //Add the diff to the center of the current TravelPoint
            Vector2 rv = new Vector2(Center.X + vDiff.X, Center.Y + vDiff.Y);

            //Get the difference between the Position of the character and the center of their collision box
            rv += c.Position - actorCollisionCenter.ToVector2();

            return rv;
        }

        public void SetDoor()
        {
            IsDoor = true;
        }

        /// <summary>
        /// Finds the center point ofthe TravelPoint and returns the RHTile the center point
        /// resides on.
        /// 
        /// This method is primarily/mostly used for NPC pathfinding to TravelPoints
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenterTilePosition()
        {
            return CollisionBox.Center.ToVector2();
        }

        public Vector2 GetMovedCenter()
        {
            RHTile rv = MapManager.Maps[_sMapName].GetTileByPixelPosition(GetCenterTilePosition());
            return rv.GetTileByDirection(_eEntranceDir).Position;
        }

        public void AssignLinkedMap(string mapName)
        {
            if (_bModular)
            {
                _sLinkedMapName = mapName;
                IsActive = true;
            }
        }

        public void Reset()
        {
            if (_bModular)
            {
                _sLinkedMapName = string.Empty;
                if (MapManager.Maps[_sMapName].Modular)
                {
                    IsActive = false;
                }
            }
        }
    }
}
