﻿using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Consumable : Item
    {
        public bool Helpful => DataManager.GetStringByIDKey(ID, "CombatType", DataType.Item).Equals("Helpful");
        public bool Recover => DataManager.GetBoolByIDKey(ID, "Recover", DataType.Item);
        public int Health => DataManager.GetIntByIDKey(ID, "Hp", DataType.Item);
        public int Mana => DataManager.GetIntByIDKey(ID, "Mana", DataType.Item);
        private StatusEffect _statusEffect;
        private int _iStatusDuration;

        public Consumable(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            if (stringData.ContainsKey("StatusEffect"))
            {
                string[] strBuffer = stringData["StatusEffect"].Split('-');
                _statusEffect = DataManager.GetStatusEffectByIndex(int.Parse(strBuffer[0]));
                _iStatusDuration = int.Parse(strBuffer[1]);
            }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
        }

        public override string Description()
        {
            string rv = base.Description();
            rv += System.Environment.NewLine;
            if (Recover) { rv += "Ends Knocked Out"; }
            if (Health > 0) { rv += "Health: +" + Health + " "; }
            if (Mana > 0) { rv += "Mana: +" + Mana + " "; }
            rv = rv.Trim();

            return rv;
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            GameManager.SetSelectedItem(this);
            TextEntry entry = DataManager.GetGameTextEntry("ItemConfirm");
            entry.FormatText(Name());
            ConfirmItemUse(entry);

            return true;
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
