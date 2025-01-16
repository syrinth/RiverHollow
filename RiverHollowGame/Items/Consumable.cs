using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Items
{
    public class Consumable : Item
    {
        public bool Helpful => GetStringByIDKey("CombatType").Equals("Helpful");
        public bool Recover => GetBoolByIDKey("Recover");
        public int Health => GetIntByIDKey("Hp");
        public int Mana => GetIntByIDKey("Mana");

        public override bool Usable => true;

        public Consumable(int id, int num) : base(id, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
        }

        public override bool HasUse() { return true; }

        public override void UseItem()
        {
            if (Health > 0) { PlayerManager.PlayerActor.IncreaseHealth(Health); }
            if (GetBoolByIDKey("StatusEffect"))
            {
                string[] strBuffer = Util.FindArguments(GetStringByIDKey("StatusEffect"));
                if (int.TryParse(strBuffer[0], out int id) && int.TryParse(strBuffer[0], out int duration))
                {
                    PlayerManager.PlayerActor.AssignStatusEffect(DataManager.GetStatusEffectByIndex(id), duration);
                }
            }

            Remove(1);
            ClearGMObjects();
        }
    }
}
