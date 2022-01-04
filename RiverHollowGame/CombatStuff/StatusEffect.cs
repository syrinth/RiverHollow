using System.Collections.Generic;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class StatusEffect
    {
        public CombatActor SkillUser;
        AttributeEnum _eDamageAttribute = AttributeEnum.Magic;
        public AttributeEnum PowerAttribute => _eDamageAttribute;

        StatusTypeEnum _eEffectType;
        public StatusTypeEnum EffectType => _eEffectType;
        public int ID { get; } = -1;
        private string _sName;
        public string Name => _sName;
        int _iPotency;
        public int Potency => _iPotency;
        int _iDuration;
        public int Duration => _iDuration;
        public List<KeyValuePair<AttributeEnum, int>> AttributeEffects { get; }

        private string _sDescription;
        public string Description { get => _sDescription; }

        public StatusEffect(int id, Dictionary<string, string> data)
        {
            ID = id;
            DataManager.GetTextData("StatusEffect", ID, ref _sName, "Name");
            DataManager.GetTextData("StatusEffect", ID, ref _sDescription, "Description");

            AttributeEffects = new List<KeyValuePair<AttributeEnum, int>>();
            ImportBasics(id, data);
        }

        protected void ImportBasics(int id, Dictionary<string, string> data)
        {
            Util.AssignValue(ref _iDuration, "Duration", data);
            Util.AssignValue(ref _iPotency, "Potency", data);
            Util.AssignValue(ref _eEffectType, "Type", data);
            Util.AssignValue(ref _eDamageAttribute, "Attribute", data);

            if (data.ContainsKey("Modify"))
            {
                string[] splitEffects = Util.FindParams(data["Modify"]);
                foreach (string effect in splitEffects)
                {
                    string[] attributeMod = effect.Split('-');
                    int value = attributeMod[1] == "Minor" ? 10 : 0;
                    AttributeEffects.Add(new KeyValuePair<AttributeEnum, int>(Util.ParseEnum<AttributeEnum>(attributeMod[0]), value));
                }
            }
        }

        public void TickDown()
        {
            _iDuration--;
        }

        public void AssignCaster(CombatActor act) { SkillUser = act; }
    }
}
