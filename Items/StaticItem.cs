using RiverHollow.Game_Managers;

namespace RiverHollow.WorldObjects
{
    public class StaticItem : Item
    {
        public StaticItem() { }
        public StaticItem(int id, string[] stringData)
        {
            ImportBasics(stringData, id, 1);
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }
    }
}
