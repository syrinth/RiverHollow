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
        private StatusEffect _statusEffect;
        private int _iStatusDuration;

        public Consumable(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            if (stringData.ContainsKey("StatusEffect"))
            {
                string[] strBuffer = Util.FindArguments(stringData["StatusEffect"]);
                _statusEffect = DataManager.GetStatusEffectByIndex(int.Parse(strBuffer[0]));
                _iStatusDuration = int.Parse(strBuffer[1]);
            }

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
            ConfirmItemUse(DataManager.GetGameTextEntry("ItemConfirm", Name()));

            return true;
        }

        public override void UseItem()
        {
            if (Helpful)
            {
                if (Health > 0) { PlayerManager.PlayerActor.IncreaseHealth(Health); }
                if (_statusEffect != null) { PlayerManager.PlayerActor.ApplyStatusEffect(_statusEffect); }

                Remove(1);
            }
            ClearGMObjects();
        }
    }
}
