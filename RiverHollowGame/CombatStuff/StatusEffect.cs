using System.Collections.Generic;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

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
        private readonly string _sName;
        public string Name => _sName;
        int _iPotency = -1;
        public int Potency => _iPotency;
        int _iDuration;
        public int Duration => _iDuration;
        public List<KeyValuePair<AttributeEnum, string>> AffectedAttributes { get; }

        private readonly string _sDescription;
        public string Description { get => _sDescription; }

        public StatusEffect(int id, Dictionary<string, string> data)
        {
            ID = id;
            _sName = DataManager.GetTextData(ID, "Name", DataType.StatusEffect);
            _sDescription = DataManager.GetTextData(ID, "Description", DataType.StatusEffect);

            AffectedAttributes = new List<KeyValuePair<AttributeEnum, string>>();
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
                    AffectedAttributes.Add(new KeyValuePair<AttributeEnum, string>(Util.ParseEnum<AttributeEnum>(attributeMod[0]), attributeMod[1]));
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
