using System;
using UnhollowerBaseLib;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;

namespace Tweakificator
{
    public class PluginComponent : MonoBehaviour
    {
        private static JsonConverter[] jsonConverters = new JsonConverter[]
        {
            new ObjectConverter<ItemTemplate.ItemMode>("identifier", "name"),
            new ObjectConverter<CraftingRecipe.CraftingRecipeItemInput>("identifier", "amount"),
            new ArrayMapConverter<CraftingRecipe.CraftingRecipeItemInput>("identifier", "amount"),
            new ObjectConverter<CraftingRecipe.CraftingRecipeElementalInput>("identifier", "amount_str"),
            new ArrayMapConverter<CraftingRecipe.CraftingRecipeElementalInput>("identifier", "amount_str"),
            new EnumConverter<ItemTemplate.ItemTemplateToggleableModeTypes>(),
            new EnumFlagsConverter<ItemTemplate.ItemTemplateFlags>(),
            new EnumFlagsConverter<ItemTemplate.HandheldSubType>(),
            new StringArrayConverter()
        };

        public PluginComponent(IntPtr ptr) : base(ptr)
        {
        }

        public static object invokeValue(Type type, JToken token)
        {
            return token.GetType().GetMethod("Value").MakeGenericMethod(type).Invoke(token, new object[] { });
        }

        public static object invokeValue(Type type, JObject token, string label)
        {
            return token.GetType().GetMethod("Value").MakeGenericMethod(type).Invoke(token, new object[] { label });
        }

        public static D gatherDump<D, T>(T template) where D : new()
        {
            var dump = (System.Object)new D();
            foreach (var field in typeof(D).GetFields())
            {
                var templateProperty = template.GetType().GetProperty(field.Name);
                if (templateProperty != null)
                {
                    field.SetValue(dump, templateProperty.GetValue(template));
                }
                else
                {
                    var templateField = template.GetType().GetField(field.Name);
                    if (templateField != null)
                    {
                        field.SetValue(dump, Convert.ChangeType(templateField.GetValue(template), field.FieldType));
                    }
                    else
                    {
                        BepInExLoader.log.LogMessage(string.Format("Failed to dump {0}", field.Name));
                    }
                }
            }
            return (D)dump;
        }

        public static void onLoadItemTemplate(ItemTemplate __instance)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters = jsonConverters;

            var path = Path.Combine(BepInExLoader.itemsFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<ItemDump, ItemTemplate>(__instance), Formatting.Indented, settings));
            }

            if (BepInExLoader.patchDataItemChanges != null && BepInExLoader.patchDataItemChanges.ContainsKey(__instance.identifier))
            {
                BepInExLoader.log.LogMessage(string.Format("Patching item {0}", __instance.identifier));
                var changes = BepInExLoader.patchDataItemChanges[__instance.identifier] as JObject;
                if (changes != null)
                {
                    JsonConvert.PopulateObject(changes.ToString(), __instance, settings);
                }
            }
        }

        public static void LoadAllItemTemplatesInBuild(ref Il2CppReferenceArray<ItemTemplate> __result)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters = jsonConverters;

            var result = new Il2CppReferenceArray<ItemTemplate>(__result.Count + BepInExLoader.patchDataItemAdditions.Count);
            for (int i = 0; i < __result.Count; ++i) result[i] = __result[i];
            int index = __result.Count;
            foreach(var entry in BepInExLoader.patchDataItemAdditions)
            {
                var source = (JObject)entry.Value;
                if (source == null) throw new Exception("Invalid item:\r\n" + entry.Value.ToString());

                ItemTemplate template = null;
                if (source.ContainsKey("__template"))
                {
                    var templateIdentifier = source.Value<string>("__template");
                    for(int i = 0; i < __result.Count; ++i)
                    {
                        if(__result[i].identifier == templateIdentifier)
                        {
                            template = __result[i];
                            break;
                        }
                    }
                    if (template == null)
                    {
                        BepInExLoader.log.LogError(string.Format("Template item {0} not found!", templateIdentifier));
                    }
                }

                BepInExLoader.log.LogMessage(string.Format("Adding item {0}", entry.Key));

                var instance = ScriptableObject.CreateInstance<ItemTemplate>();
                instance.railMiner_terrainTargetList_str = new Il2CppStringArray(0);
                if (template != null)
                {
                    BepInExLoader.log.LogMessage(string.Format("Using template {0}", template.identifier));
                    foreach(var field in typeof(ItemDump).GetFields())
                    {
                        if(field.Name != "identifier")
                        {
                            var property = typeof(ItemTemplate).GetProperty(field.Name);
                            property.SetValue(instance, property.GetValue(template));
                        }
                    }
                    foreach (var fieldName in new[] { "toggleableModeType", "toggleableModes" })
                    {
                        var property = typeof(ItemTemplate).GetProperty(fieldName);
                        property.SetValue(instance, property.GetValue(template));
                    }
                }

                instance.identifier = entry.Key;
                JsonConvert.PopulateObject(entry.Value.ToString(), instance, settings);
                result[index++] = instance;
            }

            __result = result;
        }

        public static void onLoadRecipe(CraftingRecipe __instance)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters = jsonConverters;

            var path = Path.Combine(BepInExLoader.recipesFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<RecipeDump, CraftingRecipe>(__instance), Formatting.Indented, settings));
            }

            if (BepInExLoader.patchDataRecipeChanges != null && BepInExLoader.patchDataRecipeChanges.ContainsKey(__instance.identifier))
            {
                BepInExLoader.log.LogMessage(string.Format("Patching recipe {0}", __instance.identifier));
                var changes = BepInExLoader.patchDataRecipeChanges[__instance.identifier] as JObject;

                JsonConvert.PopulateObject(changes.ToString(), __instance, settings);
            }
        }

        public static void LoadAllCraftingRecipesInBuild(ref Il2CppReferenceArray<CraftingRecipe> __result)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters = jsonConverters;

            var result = new Il2CppReferenceArray<CraftingRecipe>(__result.Count + BepInExLoader.patchDataRecipeAdditions.Count);
            for (int i = 0; i < __result.Count; ++i) result[i] = __result[i];
            int index = __result.Count;
            foreach(var entry in BepInExLoader.patchDataRecipeAdditions)
            {
                var source = (JObject)entry.Value;
                if (source == null) throw new Exception("Invalid recipe:\r\n" + entry.Value.ToString());

                CraftingRecipe template = null;
                if (source.ContainsKey("__template"))
                {
                    var templateIdentifier = source.Value<string>("__template");
                    for(int i = 0; i < __result.Count; ++i)
                    {
                        if(__result[i].identifier == templateIdentifier)
                        {
                            template = __result[i];
                            break;
                        }
                    }
                    if(template == null)
                    {
                        BepInExLoader.log.LogError(string.Format("Template recipe {0} not found!", templateIdentifier));
                    }
                }

                BepInExLoader.log.LogMessage(string.Format("Adding recipe {0}", entry.Key));

                var instance = ScriptableObject.CreateInstance<CraftingRecipe>();
                instance.tagHashes = new Il2CppStructArray<ulong>(0);
                instance.input_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput>(0);
                instance.output_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput>(0);
                instance.inputElemental_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput>(0);
                instance.outputElemental_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput>(0);

                if (template != null)
                {
                    BepInExLoader.log.LogMessage(string.Format("Using template {0}", template.identifier));
                    foreach(var field in typeof(RecipeDump).GetFields())
                    {
                        if(field.Name != "identifier")
                        {
                            var property = typeof(CraftingRecipe).GetProperty(field.Name);
                            property.SetValue(instance, property.GetValue(template));
                        }
                    }
                }

                instance.identifier = entry.Key;
                JsonConvert.PopulateObject(entry.Value.ToString(), instance, settings);
                result[index++] = instance;
            }

            __result = result;
        }

#pragma warning disable CS0649
        private struct ItemDump
        {
            public string identifier;
            public string modIdentifier;
            public string name;
            public bool includeInBuild;
            public string icon_identifier;
            public uint stackSize;
            public bool isHiddenItem;
            public ItemTemplate.ItemTemplateFlags flags;
            //public ItemTemplate.ItemTemplateToggleableModeTypes toggleableModeType;
            //public Il2CppReferenceArray<ItemTemplate.ItemMode> toggleableModes;
            public bool skipForRunningIdxGeneration;
            public string buildableObjectIdentifer;
            public ItemTemplate.HandheldSubType handheldSubType;
            public bool handheld_miner_shakeRightArmOnUse;
            public string handheld_defaultPowerPoleItemTemplate_str;
            public float miningTimeReductionInSec;
            public float miningRange;
            public int explosionRadius;
            public string burnable_fuelValueKJ_str;
            public string burnable_residualItemTemplate_str;
            public uint burnable_residualItemTemplate_count;
            public string sciencePack_processingBaseTimeMs_str;
            public string sciencePack_labIdentifier;
            public int sciencePack_researchFrameSortingOrder;
            public int railminer_slotLength;
            public int railminer_speedInSlotsPerTick;
            public int railminer_minecartSpeedInSlotsPerTick;
            public Il2CppStringArray railMiner_terrainTargetList_str;
            public bool isMinecartItem;
            public string trainVehicle_templateIdentifier;
        }

        private struct RecipeDump
        {
            public string identifier;
            public string modIdentifier;
            public string name;
            public bool includeInBuild;
            public string icon_identifier;
            public string category_identifier;
            public string rowGroup_identifier;
            public bool hideInCraftingFrame;
            public Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput> input_data;
            public Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput> output_data;
            public Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput> inputElemental_data;
            public Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput> outputElemental_data;
            public string relatedItemTemplateIdentifier;
            public int sortingOrderWithinRowGroup;
            public int timeMs;
            public string burnableEnergyConsumptionKW_str;
            public bool isInternal;
            public Il2CppStringArray tags;
            public bool forceShowOutputsAtTooltips;
            public bool showBurnabelFuelCostInTooltip;
            public string recipe_efficiency_str;
            public int recipePriority;
        }
#pragma warning restore CS0649
    }

    class ObjectConverter<T> : JsonConverter where T : new()
    {
        private delegate void MemberWriter(JObject target, object value, JsonSerializer serializer);
        private delegate void MemberReader(JObject source, object value, JsonSerializer serializer);

        private MemberWriter[] _memberWriters;
        private MemberReader[] _memberReaders;

        public ObjectConverter(params string[] memberNames)
        {
            _memberWriters = new MemberWriter[memberNames.Length];
            _memberReaders = new MemberReader[memberNames.Length];
            for (int i = 0; i < memberNames.Length; ++i)
            {
                var property = typeof(T).GetProperty(memberNames[i]);
                if (property != null)
                {
                    _memberWriters[i] = (JObject target, object value, JsonSerializer serializer) => target.Add(property.Name, JToken.FromObject(property.GetValue(value), serializer));
                    _memberReaders[i] = (JObject source, object value, JsonSerializer serializer) => property.SetValue(value, PluginComponent.invokeValue(property.PropertyType, source[property.Name]));
                }
                else
                {
                    var field = typeof(T).GetField(memberNames[i]);
                    if (field != null)
                    {
                        _memberWriters[i] = (JObject target, object value, JsonSerializer serializer) => target.Add(field.Name, JToken.FromObject(field.GetValue(value), serializer));
                        _memberReaders[i] = (JObject source, object value, JsonSerializer serializer) => field.SetValue(value, PluginComponent.invokeValue(field.FieldType, source[field.Name]));
                    }
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = (JObject)JToken.ReadFrom(reader);
            if (existingValue == null) existingValue = new T();
            foreach (var memberReader in _memberReaders) memberReader(source, existingValue, serializer);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var target = new JObject();
            foreach (var memberWriter in _memberWriters) memberWriter(target, value, serializer);
            target.WriteTo(writer);
        }
    }

    class ArrayMapConverter<E> : JsonConverter where E : Il2CppObjectBase
    {
        private PropertyInfo identifierProperty;
        private PropertyInfo[] propertys;

        public ArrayMapConverter(string identifierLabel, params string[] propertyLabels)
        {
            identifierProperty = typeof(E).GetProperty(identifierLabel);
            propertys = new PropertyInfo[propertyLabels.Length];
            for (int i = 0; i < propertyLabels.Length; ++i) propertys[i] = typeof(E).GetProperty(propertyLabels[i]);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Il2CppReferenceArray<E>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var targetValue = (Il2CppReferenceArray<E>)existingValue;
            if (targetValue == null) targetValue = new Il2CppReferenceArray<E>(0);
            var sources = (JObject)JObject.ReadFrom(reader);
            var targets = new System.Collections.Generic.List<E>(targetValue.Count);
            bool changed = false;
            for (int i = 0; i < targetValue.Count; ++i) targets.Add(targetValue[i]);

            int findTarget(string identifier)
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    if ((string)identifierProperty.GetValue(targets[i]) == identifier) return i;
                }
                return -1;
            }

            foreach (var entry in sources)
            {
                var source = (JObject)entry.Value;
                if (source != null)
                {
                    var identifier = entry.Key;
                    var index = findTarget(identifier);
                    if(index >= 0)
                    {
                        var target = targets[index];
                        bool isEmpty = true;
                        foreach (var property in propertys)
                        {
                            if (source.ContainsKey(property.Name))
                            {
                                property.SetValue(target, serializer.Deserialize(new JTokenReader(source[property.Name]), property.PropertyType));
                                isEmpty = false;
                            }
                        }

                        if(isEmpty)
                        {
                            BepInExLoader.log.LogMessage(string.Format("Deleting {0} {1}.", typeof(E).Name, identifier));
                            targets.RemoveAt(index);
                            changed = true;
                        }
                        else
                        {
                            BepInExLoader.log.LogMessage(string.Format("Patching {0} {1}.", typeof(E).Name, identifier));
                            targets[index] = target;
                            changed = true;
                        }
                    }
                    else
                    {
                        var target = typeof(E).GetConstructor(new[] { typeof(IntPtr) }).Invoke(new object[] { ClassInjector.DerivedConstructorPointer<E>() });
                        identifierProperty.SetValue(target, identifier);
                        serializer.Populate(new JTokenReader(source), target);
                        BepInExLoader.log.LogMessage(string.Format("Adding {0} {1}.", typeof(E).Name, identifier));
                        targets.Add((E)target);
                        changed = true;
                    }
                }
            }

            if(!changed) return existingValue;

            return new Il2CppReferenceArray<E>(targets.ToArray());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var source = (Il2CppReferenceArray<E>)value;
            var target = new JObject();
            for(int i = 0; i < source.Count; ++i)
            {
                var identifier = identifierProperty.GetValue(source[i]);
                var element = new JObject();
                foreach (var property in propertys)
                {
                    element[property.Name] = JToken.FromObject(property.GetValue(source[i]));
                }
                target[identifier] = element;
            }
            target.WriteTo(writer);
        }
    }

    class EnumConverter<E> : JsonConverter where E : Enum
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(E);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                E target = (E)Enum.Parse(objectType, JToken.ReadFrom(reader).Value<string>(), true);
                return target;
            }
            catch
            {
                return default(E);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((E)value).ToString());
        }
    }

    class EnumFlagsConverter<E> : JsonConverter where E : Enum
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(E);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int flags = 0;
            foreach(var flagString in JToken.ReadFrom(reader).Value<string>().Split('|'))
            {
                try
                {
                    E target = (E)Enum.Parse(objectType, flagString, true);
                    flags |= (int)(object)target;
                }
                catch { }
            }
            return (E)(object)flags;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sourceFlags = (int)value;
            var flagsString = "";
            foreach (var flag in Enum.GetValues(typeof(E)))
            {
                if ((sourceFlags & (int)flag) != 0)
                {
                    flagsString = ((flagsString.Length > 0) ? flagsString + "|" : "") + Enum.GetName(typeof(E), flag);
                }
            }
            writer.WriteValue(flagsString);
        }
    }

    class StringArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Il2CppStringArray);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = (JArray)JToken.ReadFrom(reader);
            var target = new Il2CppStringArray(source.Count);
            for(int i = 0; i < source.Count; ++i) target[i] = source[i].Value<string>();
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var source = (Il2CppStringArray)value;
            writer.WriteStartArray();
            foreach(var element in source) writer.WriteValue(element);
            writer.WriteEndArray();
        }
    }
}