using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.Map_Handling
{
    public class ShopLocation
    {
        string _sMap;
        int _iShopID;
        Rectangle _rCLick;
        int _iShopX;
        int _iShopY;
        public ShopLocation(string map, TiledMapObject shopObj)
        {
            _sMap = map;
            _rCLick = Util.FloatRectangle(shopObj.Position, shopObj.Size.Width, shopObj.Size.Height);
            _iShopID = int.Parse(shopObj.Properties["NPC_ID"]);
            _iShopX = int.Parse(shopObj.Properties["ShopKeepX"]);
            _iShopY = int.Parse(shopObj.Properties["ShopKeepY"]);
        }

        internal bool Contains(Point mouseLocation)
        {
            return _rCLick.Contains(mouseLocation);
        }

        internal bool IsOpen()
        {
            bool rv = false;

            if (DataManager.DIVillagers[_iShopID].CurrentMapName == _sMap)
            {
                if (MapManager.RetrieveTile(_iShopX, _iShopY).Contains(DataManager.DIVillagers[_iShopID]))
                {
                    rv = true;
                }
            }

            return rv;
        }

        internal void Talk()
        {
            (DataManager.DIVillagers[_iShopID]).SetShopOpenStatus(true);
            (DataManager.DIVillagers[_iShopID]).StartConversation();
            (DataManager.DIVillagers[_iShopID]).SetShopOpenStatus(false);
        }
    }
}
