using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RiverHollow.Characters;
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
        public double Duration { get; private set; }

        private readonly string _sDescription;
        public string Description { get => _sDescription; }

        public StatusEffect(int id)
        {
            ID = id;
            _sName = DataManager.GetTextData(ID, "Name", DataType.StatusEffect);
            _sDescription = DataManager.GetTextData(ID, "Description", DataType.StatusEffect);

            Duration = GetFloatByIDKey("Duration");

            _iPotency = GetIntByIDKey("Potency");
            _eEffectType = GetEnumByIDKey<StatusTypeEnum>("Type");

            if (GetBoolByIDKey("Modify"))
            {
                string[] splitEffects = Util.FindParams(GetStringByIDKey("Modify"));
                foreach (string effect in splitEffects)
                {
                    string[] attributeMod = Util.FindArguments(effect);
                }
            }
        }

        public void SetDuration(double time)
        {
            if (time > 0)
            {
                Duration = time;
            }
        }

        public void Update(GameTime gTime)
        {
            Duration -= gTime.ElapsedGameTime.TotalSeconds;
        }
        public void AssignEffects(CombatActor c)
        {
            if (GetBoolByIDKey("Light"))
            {
                PlayerManager.PlayerActor.SetLightSource(GetIntByIDKey("Light"));
            }
        }

        public void RemoveEffects(CombatActor c)
        {
            if (GetBoolByIDKey("Light"))
            {
                PlayerManager.PlayerActor.SetLightSource();
            }
        }

        #region Lookup Handlers
        public bool GetBoolByIDKey(string key)
        {
            return DataManager.GetBoolByIDKey(ID, key, DataType.StatusEffect);
        }
        public virtual int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(ID, key, DataType.StatusEffect, defaultValue);
        }
        public virtual float GetFloatByIDKey(string key, float defaultValue = -1)
        {
            return DataManager.GetFloatByIDKey(ID, key, DataType.StatusEffect, defaultValue);
        }
        public string GetStringByIDKey(string key)
        {
            return DataManager.GetStringByIDKey(ID, key, DataType.StatusEffect);
        }
        public virtual TEnum GetEnumByIDKey<TEnum>(string key) where TEnum : struct
        {
            return DataManager.GetEnumByIDKey<TEnum>(ID, key, DataType.StatusEffect);
        }
        #endregion
    }
}
