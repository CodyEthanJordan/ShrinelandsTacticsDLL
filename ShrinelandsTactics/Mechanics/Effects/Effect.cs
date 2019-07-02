using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics.Effects
{
    [JsonConverter(typeof(EffectConverter))]
    public abstract class Effect
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public EffectType TypeOfEffect { get; protected set; }

        public abstract void Apply(DungeonMaster dM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures);

        public enum EffectType
        {
            Move,
            Damage,
            Null,
            ModifyCondition,
        }
    }

    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Effect).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class EffectConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Effect));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            switch (Enum.Parse(typeof(Effect.EffectType), jo["TypeOfEffect"].Value<string>()))
            {
                case Effect.EffectType.Damage:
                    return JsonConvert.DeserializeObject<DamageEffect>(jo.ToString(), SpecifiedSubclassConversion);
                case Effect.EffectType.ModifyCondition:
                    return JsonConvert.DeserializeObject<ModifyConditionEffect>(jo.ToString(), SpecifiedSubclassConversion);
                case Effect.EffectType.Null:
                    return JsonConvert.DeserializeObject<NullEffect>(jo.ToString(), SpecifiedSubclassConversion);
                case Effect.EffectType.Move:
                    return JsonConvert.DeserializeObject<MoveEffect>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new Exception("Unkown effect type to deserialize");
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }
}
