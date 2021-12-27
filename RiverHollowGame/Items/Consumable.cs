using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Items
{
    public class Consumable : Item
    {
        public bool Recover { get; private set; }
        public int Health { get; private set; }
        public int Mana { get; private set; }
        private StatusEffect _statusEffect;
        private int _iStatusDuration;

        public bool Helpful;

        public Consumable(int id, Dictionary<string, string> stringData, int num)
        {
            ImportBasics(stringData, id, num);

            Helpful = stringData["CombatType"].Equals("Helpful");
            if (stringData.ContainsKey("Recover")) { Recover = true; }
            if (stringData.ContainsKey("Hp")) { Health = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("Mana")) { Mana = int.Parse(stringData["Mana"]); }
            if (stringData.ContainsKey("StatusEffect"))
            {
                string[] strBuffer = stringData["StatusEffect"].Split('-');
                _statusEffect = DataManager.GetStatusEffectByIndex(int.Parse(strBuffer[0]));
                _iStatusDuration = int.Parse(strBuffer[1]);
            }

            _bStacks = true;
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (Recover) { rv += "Ends Knocked Out"; }
            if (Health > 0) { rv += "Health: +" + Health + " "; }
            if (Mana > 0) { rv += "Mana: +" + Mana + " "; }
            rv = rv.Trim();

            return rv;
        }

        public override void UseItem(TextEntryVerbEnum action)
        {
            if (Helpful)
            {
                //We're passing in a verb entry of Option_#, need to isolate the int
                int partyPosition = int.Parse(Util.GetEnumString<TextEntryVerbEnum>(action).Replace("Option_", ""));
                CombatActor target = PlayerManager.GetParty()[partyPosition];

                if (Health > 0) { target.IncreaseHealth(Health); }
                if (_statusEffect != null) { target.ApplyStatusEffect(_statusEffect); }

                Remove(1);
            }
            ClearGMObjects();
        }
    }
}
