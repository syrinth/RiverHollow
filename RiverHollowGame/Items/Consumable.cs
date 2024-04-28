using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Items
{
    public class Consumable : Item
    {
        public bool Helpful => GetStringByIDKey("CombatType").Equals("Helpful");
        public bool Recover => GetBoolByIDKey("Recover");
        public int Health => GetIntByIDKey("Hp");
        public int Mana => GetIntByIDKey("Mana");

        public Consumable(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
        }

        public override string GetDetails()
        {
            string rv = string.Empty;

            if (Health > 0) { rv += "Health: +" + Health + " "; }
            if (Mana > 0) { rv += "Mana: +" + Mana + " "; }
            rv = rv.Trim();

            return rv;
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            GameManager.SetSelectedItem(this);
            ConfirmItemUse(DataManager.GetGameTextEntry("Item_Confirm", Name()));

            return true;
        }

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
