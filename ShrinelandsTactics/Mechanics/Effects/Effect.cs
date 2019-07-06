using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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
        [JsonProperty]
        public bool AffectCaster = false;

        public abstract Outcome Apply(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures=null);

        public enum EffectType
        {
            Move,
            Damage,
            Null,
            ModifyCondition,
            RegainStat,
            Redraw,
            ResolveByTarget,
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
            //TODO: reflect on thy sins
            JObject jo = JObject.Load(reader);

            MethodInfo method =  typeof(JsonConvert).GetMethods()
                .Where(m => m.IsGenericMethod && m.Name == "DeserializeObject" &&
                m.GetParameters().Length == 2 && 
                m.GetParameters().Any(p => p.ParameterType == typeof(JsonSerializerSettings))).First();
            Type effectType = Type.GetType(
                "ShrinelandsTactics.Mechanics.Effects." + jo["TypeOfEffect"].Value<string>() + "Effect");
            MethodInfo generic = method.MakeGenericMethod(effectType);
            object[] parameters = { jo.ToString(), SpecifiedSubclassConversion };
            return generic.Invoke(null, parameters);

            //switch (Enum.Parse(typeof(Effect.EffectType), jo["TypeOfEffect"].Value<string>()))
            //{
            //    case Effect.EffectType.Damage:
            //        return JsonConvert.DeserializeObject<DamageEffect>(jo.ToString(), SpecifiedSubclassConversion);
            //    case Effect.EffectType.ModifyCondition:
            //        return JsonConvert.DeserializeObject<ModifyConditionEffect>(jo.ToString(), SpecifiedSubclassConversion);
            //    case Effect.EffectType.Null:
            //        return JsonConvert.DeserializeObject<NullEffect>(jo.ToString(), SpecifiedSubclassConversion);
            //    case Effect.EffectType.Move:
            //        return JsonConvert.DeserializeObject<MoveEffect>(jo.ToString(), SpecifiedSubclassConversion);
            //    case Effect.EffectType.RegainStat:
            //        return JsonConvert.DeserializeObject<RegainStatEffect>(jo.ToString(), SpecifiedSubclassConversion);
            //    case Effect.EffectType.Redraw:
            //        return JsonConvert.DeserializeObject<RedrawEffect>(jo.ToString(), SpecifiedSubclassConversion);
            //    default:
            //        throw new Exception("Unkown effect type to deserialize");
            //}
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
