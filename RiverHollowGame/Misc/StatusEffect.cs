using System.Collections.Generic;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class StatusEffect
    {
        StatusTypeEnum _eEffectType;
        public StatusTypeEnum EffectType => _eEffectType;
        public int ID { get; } = -1;
        private readonly string _sName;
        public string Name => _sName;
        int _iPotency = -1;
        public int Potency => _iPotency;
        int _iDuration;
        public int Duration => _iDuration;

        private readonly string _sDescription;
        public string Description { get => _sDescription; }

        public StatusEffect(int id, Dictionary<string, string> data)
        {
            ID = id;
            _sName = DataManager.GetTextData(ID, "Name", DataType.StatusEffect);
            _sDescription = DataManager.GetTextData(ID, "Description", DataType.StatusEffect);

            Util.AssignValue(ref _iDuration, "Duration", data);
            Util.AssignValue(ref _iPotency, "Potency", data);
            Util.AssignValue(ref _eEffectType, "Type", data);

            if (data.ContainsKey("Modify"))
            {
                string[] splitEffects = Util.FindParams(data["Modify"]);
                foreach (string effect in splitEffects)
                {
                    string[] attributeMod = effect.Split('-');
                }
            }
        }

        public void TickDown()
        {
            _iDuration--;
        }
    }
}
