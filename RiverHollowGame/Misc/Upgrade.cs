using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Upgrade
    {
        public int ID { get; private set; }
        public int Cost => int.Parse(DataManager.GetDataValueByIDKey(ID, "Cost", DataType.Upgrade));
        public Point Icon => Util.ParsePoint(DataManager.GetDataValueByIDKey(ID, "Icon", DataType.Upgrade));


        public Upgrade(int id)
        {
            ID = id;
        }
    }
}
