using System;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using PanoramicData.NCalcExtensions;
using System.Reflection;
using UnhollowerRuntimeLib;
using HarmonyLib;
using System.Text.RegularExpressions;

namespace Tweakificator
{
    public class PluginComponent : MonoBehaviour
    {
        private static JsonSerializerSettings serializerSettings;

        private static JsonConverter[] jsonConverters = new JsonConverter[]
        {
            new ObjectConverter<ItemTemplate.ItemMode>("identifier", "name", "icon"),
            new ObjectConverter<CraftingRecipe.CraftingRecipeItemInput>("identifier", "amount"),
            new ObjectConverter<CraftingRecipe.CraftingRecipeElementalInput>("identifier", "amount_str"),
            new ObjectConverter<Vector3>("x", "y", "z"),
            new ObjectConverter<Vector3Int>("x", "y", "z"),
            new ObjectConverter<Color>("r", "g", "b", "a"),
            new ObjectConverter<Quaternion>("x", "y", "z", "w"),
            new ObjectConverter<BuildableObjectTemplate.DragMode>("name", "isDefault"),
            new ObjectConverter<BuildableObjectTemplate.FluidBox>("localOffset", "connectorFlags"),
            new ObjectConverter<ResearchTemplate.ResearchTemplateItemInput>("identifier", "amount"),

            new ArrayMapConverter<CraftingRecipe.CraftingRecipeItemInput>("identifier", "amount"),
            new ArrayMapConverter<CraftingRecipe.CraftingRecipeElementalInput>("identifier", "amount_str"),
            new ArrayMapConverter<ItemTemplate.ItemMode>("identifier", "name", "icon"),
            new ArrayMapConverter<ResearchTemplate.ResearchTemplateItemInput>("identifier", "amount"),

            new ListConverter<Vector3Int>(),
            new ListConverter<string>(),

            new ReferenceArrayConverter<BuildableObjectTemplate.DragMode>(),

            new EnumConverter<ItemTemplate.ItemTemplateToggleableModeTypes>(),
            new EnumConverter<BuildableObjectTemplate.BuildableObjectType>(),
            new EnumConverter<BuildableObjectTemplate.BuildableObjectPowerSubType>(),
            new EnumConverter<BuildableObjectTemplate.CustomSnapMode>(),
            new EnumConverter<BuildableObjectTemplate.DragBuildType>(),
            new EnumConverter<BuildableObjectTemplate.FoundationConnectorType>(),
            new EnumConverter<BuildableObjectTemplate.ModularBuildingType>(),
            new EnumConverter<BuildableObjectTemplate.ProducerRecipeType>(),
            new EnumConverter<BuildableObjectTemplate.ScreenPanelType>(),
            new EnumConverter<BuildableObjectTemplate.SimulationType>(),

            new EnumFlagsConverter<ItemTemplate.ItemTemplateFlags>(),
            new EnumFlagsConverter<ItemTemplate.HandheldSubType>(),
            new EnumFlagsConverter<ElementTemplate.ElementTemplateFlags>(),
            new EnumFlagsConverter<TerrainBlockType.DecorFlags>(),
            new EnumFlagsConverter<TerrainBlockType.OreSpawnFlags>(),
            new EnumFlagsConverter<TerrainBlockType.TerrainTypeFlags>(),
            new EnumFlagsConverter<BuildableObjectTemplate.PipeConnectorFlags>(),
            new EnumFlagsConverter<BuildableObjectTemplate.PoleGridTypes>(),
            new EnumFlagsConverter<ResearchTemplate.ResearchTemplateFlags>(),

            new EnumFlagsConverterByte<BuildableObjectTemplate.SimulationSleepFlags>(),

            new StringArrayConverter(),

            new Texture2DProxyConverter(),

            new SpriteConverter(),

            new StringConverter(),
            new NumericConverter()
        };

        private static System.Collections.Generic.Dictionary<int, int> iconSizes = new System.Collections.Generic.Dictionary<int, int>() {
            { 0, 1024 },
            { 512, 512 },
            { 256, 256 },
            { 128, 128 },
            { 96, 96 },
            { 64, 64 }
        };

        public static BepInEx.Logging.ManualLogSource log;

        public PluginComponent(IntPtr ptr) : base(ptr)
        {
            serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters = jsonConverters;
        }

        public static object invokeValue(Type type, JToken token)
        {
            //BepInExLoader.log.LogMessage(string.Format("{0} {1} '{2}'", type.FullName, token.GetType().FullName, token.ToString()));
            var property = token.GetType().GetProperty("Value");
            if(property != null) return property.GetGetMethod().Invoke(token, new object[] { });
            var method = token.GetType().GetMethod("Value");
            if(method != null)
            {
                if(method.IsGenericMethod) return method.MakeGenericMethod(type).Invoke(token, new object[] { });
                return method.Invoke(token, new object[] { });
            }
            throw new Exception(string.Format("Failed to get value of token '{0}'", token.ToString()));
        }

        public static object invokeValue(Type type, JObject token, string label)
        {
            var property = token.GetType().GetProperty("Value");
            if (property != null) return property.GetGetMethod().Invoke(token, new object[] { label });
            var method = token.GetType().GetMethod("Value");
            if (method != null)
            {
                if (method.IsGenericMethod) return method.MakeGenericMethod(type).Invoke(token, new object[] { label });
                return method.Invoke(token, new object[] { label });
            }
            throw new Exception(string.Format("Failed to get value of token '{0}'", token.ToString()));
        }

        public static D gatherDump<D, T>(T template) where D : new()
        {
            var dump = (System.Object)new D();
            foreach (var field in typeof(D).GetFields())
            {
                var templateProperty = template.GetType().GetProperty(field.Name);
                if (templateProperty != null)
                {
                    if (templateProperty.PropertyType == typeof(Texture2D) && field.FieldType == typeof(Texture2DProxy))
                    {
                        field.SetValue(dump, new Texture2DProxy((Texture2D)templateProperty.GetValue(template)));
                    }
                    else
                    {
                        field.SetValue(dump, templateProperty.GetValue(template));
                    }
                }
                else
                {
                    log.LogError(string.Format("Failed to dump {0}", field.Name));
                }
            }
            return (D)dump;
        }

        public static Sprite createSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        private static Texture2D duplicateTexture(Texture2D sourceTexture)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(sourceTexture, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false, true);
            readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return readableTexture;
        }

        private static Texture2D resizeTexture(Texture2D inputTexture, int width, int height)
        {
            var outputTexture = new Texture2D(width, height, inputTexture.format, false, true);
            Graphics.ConvertTexture(inputTexture, outputTexture);
            return outputTexture;
        }

        internal static Sprite getIcon(string name)
        {
            return ResourceDB.getIcon(name, 256);
        }

        public static void ResourceDBInitOnApplicationStart()
        {
            var filenames = Directory.EnumerateFiles(BepInExLoader.iconsFolder, "*.png").ToArray<string>();
            BepInExLoader.log.LogMessage(string.Format("Loading {0} custom icons.", filenames.Length));
            foreach (var filename in filenames)
            {
                var iconPath = Path.Combine(BepInExLoader.iconsFolder, filename);
                var identifier = Path.GetFileNameWithoutExtension(iconPath);
                if (!ResourceDB.dict_icons[0].ContainsKey(GameRoot.generateStringHash64(identifier)))
                {
                    var watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
                    var iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                    iconTexture.LoadImage(new Il2CppStructArray<byte>(File.ReadAllBytes(iconPath)), true);
                    int index = 0;
                    foreach (var entry in iconSizes)
                    {
                        var sizeId = entry.Key;
                        var size = entry.Value;
                        var sizeIdentifier = identifier + ((sizeId > 0) ? "_" + sizeId.ToString() : "");
                        var texture = (sizeId > 0) ? resizeTexture(iconTexture, size, size) : iconTexture;
                        texture.name = sizeIdentifier;
                        ResourceDB.dict_icons[sizeId][GameRoot.generateStringHash64(sizeIdentifier)] = createSprite(texture);

                        ++index;
                    }

                    watch.Stop();
                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Loading icon '{0}' from '{1}' took {2}ms", identifier, iconPath, watch.ElapsedMilliseconds));
                }
            }

            var listPath = Path.Combine(BepInExLoader.iconsDumpFolder, "__icons.txt");
            if (BepInExLoader.forceDump.Value || !File.Exists(listPath))
            {
                var iconNames = new System.Collections.Generic.List<string>();
                foreach (var entry in ResourceDB.dict_icons[0]) iconNames.Add(entry.Value.name);// string.Format("{0}: {1}", entry.Value.name, entry.Value.texture.format.ToString()));
                File.WriteAllText(listPath, string.Join("\r\n", iconNames));
            }

            if (BepInExLoader.dumpIcons.Value)
            {
                var cache = new System.Collections.Generic.Dictionary<string, Texture2D>();
                foreach (var entry in ResourceDB.dict_icons)
                {
                    foreach (var entry2 in entry.Value)
                    {
                        var sprite = entry2.Value;
                        var path = Path.Combine(BepInExLoader.iconsDumpFolder, sprite.name + ".png");
                        if (!File.Exists(path))
                        {
                            Texture2D texture;
                            if (!cache.TryGetValue(sprite.texture.name, out texture))
                            {
                                if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Converting icon texture '{0}'", sprite.texture.name));
                                texture = duplicateTexture(sprite.texture);
                                cache[sprite.texture.name] = texture;
                            }

                            if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Dumping icon '{0}'", sprite.name));
                            var croppedTexture = new Texture2D(Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), TextureFormat.RGBA32, false);
                            var pixels = texture.GetPixels(Mathf.FloorToInt(sprite.textureRect.x), Mathf.FloorToInt(sprite.textureRect.y), Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height));
                            croppedTexture.SetPixels(pixels);
                            croppedTexture.Apply();
                            var bytes = croppedTexture.EncodeToPNG();
                            File.WriteAllBytes(path, bytes);

                            Destroy(croppedTexture);
                            Il2CppSystem.GC.Collect();
                        }
                    }
                }
                foreach (var texture in cache.Values) Destroy(texture);
                cache.Clear();
                Il2CppSystem.GC.Collect();
            }
        }

        public static void onLoadItemTemplate(ItemTemplate __instance)
        {
            var path = Path.Combine(BepInExLoader.itemsDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<ItemDump, ItemTemplate>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataItemChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataItemChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if(__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching item {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching item {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        public static void LoadAllItemTemplatesInBuild(ref Il2CppReferenceArray<ItemTemplate> __result)
        {
            var result = new Il2CppReferenceArray<ItemTemplate>(__result.Count + BepInExLoader.patchDataItemAdditions.Count);
            for (int i = 0; i < __result.Count; ++i) result[i] = __result[i];
            int index = __result.Count;
            foreach (var entry in BepInExLoader.patchDataItemAdditions)
            {
                var source = (JObject)entry.Value;
                if (source == null) throw new Exception("Invalid item:\r\n" + entry.Value.ToString());

                ItemTemplate template = null;
                if (source.ContainsKey("__template"))
                {
                    var templateIdentifier = source.Value<string>("__template");
                    for (int i = 0; i < __result.Count; ++i)
                    {
                        if (__result[i].identifier == templateIdentifier)
                        {
                            template = __result[i];
                            break;
                        }
                    }
                    if (template == null)
                    {
                        log.LogError(string.Format("Template item {0} not found!", templateIdentifier));
                    }
                }

                if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding item {0}", entry.Key));

                ItemTemplate instance;
                if (template != null)
                {
                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
                    instance = Instantiate<ItemTemplate>(template);
                }
                else
                {
                    instance = ScriptableObject.CreateInstance<ItemTemplate>();
                    instance.railMiner_terrainTargetList_str = new Il2CppStringArray(0);
                }

                instance.identifier = entry.Key;
                JsonConvert.PopulateObject(entry.Value.ToString(), instance, serializerSettings);
                result[index++] = instance;
            }

            BepInExLoader.log.LogMessage(string.Format("Patched {0} items and added {1} items.", BepInExLoader.patchDataItemChanges != null ? BepInExLoader.patchDataItemChanges.Count : 0, BepInExLoader.patchDataItemAdditions != null ? BepInExLoader.patchDataItemAdditions.Count : 0));

            __result = result;
        }

        public static void onLoadElementTemplate(ElementTemplate __instance)
        {
            var path = Path.Combine(BepInExLoader.elementsDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<ElementDump, ElementTemplate>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataElementChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataElementChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if (__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching element {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching element {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        public static void onLoadRecipe(CraftingRecipe __instance)
        {
            var path = Path.Combine(BepInExLoader.recipesDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<RecipeDump, CraftingRecipe>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataRecipeChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataRecipeChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if (__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching recipe {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching recipe {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        public static void LoadAllCraftingRecipesInBuild(ref Il2CppReferenceArray<CraftingRecipe> __result)
        {
            var result = new Il2CppReferenceArray<CraftingRecipe>(__result.Count + BepInExLoader.patchDataRecipeAdditions.Count);
            for (int i = 0; i < __result.Count; ++i) result[i] = __result[i];
            int index = __result.Count;
            foreach (var entry in BepInExLoader.patchDataRecipeAdditions)
            {
                var source = (JObject)entry.Value;
                if (source == null) throw new Exception("Invalid recipe:\r\n" + entry.Value.ToString());

                CraftingRecipe template = null;
                if (source.ContainsKey("__template"))
                {
                    var templateIdentifier = source.Value<string>("__template");
                    for (int i = 0; i < __result.Count; ++i)
                    {
                        if (__result[i].identifier == templateIdentifier)
                        {
                            template = __result[i];
                            break;
                        }
                    }
                    if (template == null)
                    {
                        log.LogError(string.Format("Template recipe {0} not found!", templateIdentifier));
                    }
                }

                if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding recipe {0}", entry.Key));
                CraftingRecipe instance;
                if (template != null)
                {
                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
                    instance = Instantiate<CraftingRecipe>(template);
                }
                else
                {
                    instance = ScriptableObject.CreateInstance<CraftingRecipe>();
                    instance.tags = new Il2CppStringArray(0);
                    instance.input_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput>(0);
                    instance.output_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput>(0);
                    instance.inputElemental_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput>(0);
                    instance.outputElemental_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput>(0);
                    instance.tagHashes = new Il2CppStructArray<ulong>(0);
                    instance.input = new Il2CppReferenceArray<Il2CppSystem.Collections.Generic.KeyValuePair<ItemTemplate, uint>>(0);
                    instance.output = new Il2CppReferenceArray<Il2CppSystem.Collections.Generic.KeyValuePair<ItemTemplate, uint>>(0);
                    instance.input_elemental = new Il2CppReferenceArray<Il2CppSystem.Collections.Generic.KeyValuePair<ElementTemplate, long>>(0);
                    instance.output_elemental = new Il2CppReferenceArray<Il2CppSystem.Collections.Generic.KeyValuePair<ElementTemplate, long>>(0);
                }

                instance.identifier = entry.Key;
                JsonConvert.PopulateObject(entry.Value.ToString(), instance, serializerSettings);
                result[index++] = instance;
            }

            BepInExLoader.log.LogMessage(string.Format("Patched {0} recipes and added {1} recipes.", BepInExLoader.patchDataRecipeChanges != null ? BepInExLoader.patchDataRecipeChanges.Count : 0, BepInExLoader.patchDataRecipeAdditions!= null ? BepInExLoader.patchDataRecipeAdditions.Count : 0));

            __result = result;
        }

        public static void onLoadTerrainBlockType(TerrainBlockType __instance)
        {
            var path = Path.Combine(BepInExLoader.terrainBlocksDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<TerrainBlockDump, TerrainBlockType>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataTerrainChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataTerrainChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if (__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching terrain block {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching terrain block {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            changes = changes.DeepClone() as JObject;
                            if (changes.ContainsKey("texture_abledo"))
                            {
                                __instance.texture_abledo = (Texture2D)ResourceExt.FindTexture(changes["texture_abledo"].Value<string>());
                                changes.Remove("texture_abledo");
                            }
                            if (changes.ContainsKey("texture_side_abledo"))
                            {
                                __instance.texture_side_abledo = (Texture2D)ResourceExt.FindTexture(changes["texture_side_abledo"].Value<string>());
                                changes.Remove("texture_side_abledo");
                            }
                            if (changes.ContainsKey("texture_bottom_abledo"))
                            {
                                __instance.texture_bottom_abledo = (Texture2D)ResourceExt.FindTexture(changes["texture_bottom_abledo"].Value<string>());
                                changes.Remove("texture_bottom_abledo");
                            }
                            if (changes.ContainsKey("texture_height"))
                            {
                                __instance.texture_height = (Texture2D)ResourceExt.FindTexture(changes["texture_height"].Value<string>());
                                changes.Remove("texture_height");
                            }
                            if (changes.ContainsKey("texture_side_height"))
                            {
                                __instance.texture_side_height = (Texture2D)ResourceExt.FindTexture(changes["texture_side_height"].Value<string>());
                                changes.Remove("texture_side_height");
                            }
                            if (changes.ContainsKey("texture_side_mask"))
                            {
                                __instance.texture_side_mask = (Texture2D)ResourceExt.FindTexture(changes["texture_side_mask"].Value<string>());
                                changes.Remove("texture_side_mask");
                            }
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        private static bool hasRun_researchTemplates = false;
        private static bool hasRun_terrainBlockTemplates = false;
        private static bool hasRun_terrainBlockScratchGroups = false;
        private static bool hasRun_biomeTemplates = false;
        private static bool hasRun_craftingRecipes = false;
        public static void onItemTemplateManagerInitOnApplicationStart()
        {
            if (!hasRun_craftingRecipes && ItemTemplateManager.dict_craftingRecipes != null && ItemTemplateManager.dict_craftingRecipes.Count > 0)
            {
                hasRun_craftingRecipes = true;

                foreach(var entry in ItemTemplateManager.dict_craftingRecipes)
                {
                    if (entry.Value.tags.Contains("character")) checkForRecipeCycles(entry.Value);
                }
            }

            if (!hasRun_researchTemplates && ItemTemplateManager.dict_researchTemplates != null && ItemTemplateManager.dict_researchTemplates.Count > 0)
            {
                hasRun_researchTemplates = true;

                foreach (var entry in BepInExLoader.patchDataResearchAdditions)
                {
                    var source = (JObject)entry.Value;
                    if (source == null) throw new Exception("Invalid research:\r\n" + entry.Value.ToString());

                    ResearchTemplate template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source.Value<string>("__template");
                        foreach (var templateEntry in ItemTemplateManager.dict_researchTemplates.Values)
                        {
                            if (templateEntry.identifier == templateIdentifier)
                            {
                                template = templateEntry;
                                break;
                            }
                        }
                        if (template == null)
                        {
                            log.LogError(string.Format("Template research {0} not found!", templateIdentifier));
                        }
                    }

                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding research {0}", entry.Key));

                    ResearchTemplate instance;
                    if (template != null)
                    {
                        if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
                        instance = Instantiate<ResearchTemplate>(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<ResearchTemplate>();
                        instance.list_blastFurnaceModes_str = new Il2CppSystem.Collections.Generic.List<string>(0);
                        instance.list_craftingUnlocks_str = new Il2CppSystem.Collections.Generic.List<string>(0);
                        instance.list_researchDependencies_str = new Il2CppSystem.Collections.Generic.List<string>(0);
                        instance.input_data = new Il2CppReferenceArray<ResearchTemplate.ResearchTemplateItemInput>(0);
                    }

                    instance.identifier = entry.Key;
                    instance.id = GameRoot.generateStringHash64("rt_" + instance.identifier);
                    instance.id32 = GameRoot.generateStringHash32("rt_" + instance.identifier);

                    JsonConvert.PopulateObject(source.ToString(), instance, serializerSettings);

                    ItemTemplateManager.dict_researchTemplates[instance.id] = instance;

                    instance.onLoad();
                }

                BepInExLoader.log.LogMessage(string.Format("Patched {0} research and added {1} research.", BepInExLoader.patchDataResearchChanges != null ? BepInExLoader.patchDataResearchChanges.Count : 0, BepInExLoader.patchDataResearchAdditions != null ? BepInExLoader.patchDataResearchAdditions.Count : 0));
            }

            if (!hasRun_terrainBlockScratchGroups && ItemTemplateManager.dict_terrainBlockScratchGroups != null && ItemTemplateManager.dict_terrainBlockScratchGroups.Count > 0)
            {
                hasRun_terrainBlockScratchGroups = true;

                foreach (var entry in BepInExLoader.patchDataTerrainAdditions)
                {
                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Generate scratch group for {0}", entry.Key));

                    var source = (JObject)entry.Value;
                    if (source == null) throw new Exception("Invalid terrain block:\r\n" + entry.Value.ToString());

                    string terrainBlockIdentifier;
                    if (source.ContainsKey("__template"))
                    {
                        terrainBlockIdentifier = source["__template"].Value<string>();
                    }
                    else
                    {
                        terrainBlockIdentifier = "_base_concrete";
                    }

                    var template = ItemTemplateManager.getTerrainBlockScratchGroupByTerrainBlockType(terrainBlockIdentifier);

                    var instance = Instantiate<TerrainBlockScratchGroup>(template);
                    instance.terrainBlockType_identifier = entry.Key;
                    instance.id = GameRoot.generateStringHash64("tbsg_" + instance.terrainBlockType_identifier);
                    instance.id32 = GameRoot.generateStringHash32("tbsg_" + instance.terrainBlockType_identifier);

                    ItemTemplateManager.dict_terrainBlockScratchGroups[instance.id] = instance;

                    instance.onLoad();
                }

                BepInExLoader.log.LogMessage(string.Format("Added {0} terrain block scratch groups.", BepInExLoader.patchDataTerrainAdditions != null ? BepInExLoader.patchDataTerrainAdditions.Count : 0));
            }

            if (!hasRun_terrainBlockTemplates && ItemTemplateManager.dict_terrainBlockTemplates != null && ItemTemplateManager.dict_terrainBlockTemplates.Count > 0)
            {
                hasRun_terrainBlockTemplates = true;

                foreach (var entry in BepInExLoader.patchDataTerrainAdditions)
                {
                    var source = (JObject)entry.Value;
                    if (source == null) throw new Exception("Invalid terrain block:\r\n" + entry.Value.ToString());

                    TerrainBlockType template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source.Value<string>("__template");
                        foreach (var templateEntry in ItemTemplateManager.dict_terrainBlockTemplates.Values)
                        {
                            if (templateEntry.identifier == templateIdentifier)
                            {
                                template = templateEntry;
                                break;
                            }
                        }
                        if (template == null)
                        {
                            log.LogError(string.Format("Template terrain block {0} not found!", templateIdentifier));
                        }
                    }

                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding terrain block {0}", entry.Key));

                    TerrainBlockType instance;
                    if (template != null)
                    {
                        if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
                        instance = Instantiate<TerrainBlockType>(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<TerrainBlockType>();
                        instance.surfaceOre_worldDecor_identifier = new Il2CppStringArray(0);
                    }

                    instance.identifier = entry.Key;
                    instance.id = GameRoot.generateStringHash64("tbt_" + instance.identifier);
                    instance.id32 = GameRoot.generateStringHash32("tbt_" + instance.identifier);

                    if (source.ContainsKey("texture_abledo"))
                    {
                        instance.texture_abledo = (Texture2D)ResourceExt.FindTexture(source["texture_abledo"].Value<string>());
                        source.Remove("texture_abledo");
                    }
                    if (source.ContainsKey("texture_side_abledo"))
                    {
                        instance.texture_side_abledo = (Texture2D)ResourceExt.FindTexture(source["texture_side_abledo"].Value<string>());
                        source.Remove("texture_side_abledo");
                    }
                    if (source.ContainsKey("texture_bottom_abledo"))
                    {
                        instance.texture_bottom_abledo = (Texture2D)ResourceExt.FindTexture(source["texture_bottom_abledo"].Value<string>());
                        source.Remove("texture_bottom_abledo");
                    }
                    if (source.ContainsKey("texture_height"))
                    {
                        instance.texture_height = (Texture2D)ResourceExt.FindTexture(source["texture_height"].Value<string>());
                        source.Remove("texture_height");
                    }
                    if (source.ContainsKey("texture_side_height"))
                    {
                        instance.texture_side_height = (Texture2D)ResourceExt.FindTexture(source["texture_side_height"].Value<string>());
                        source.Remove("texture_side_height");
                    }
                    if (source.ContainsKey("texture_side_mask"))
                    {
                        instance.texture_side_mask = (Texture2D)ResourceExt.FindTexture(source["texture_side_mask"].Value<string>());
                        source.Remove("texture_side_mask");
                    }
                    JsonConvert.PopulateObject(source.ToString(), instance, serializerSettings);

                    ItemTemplateManager.dict_terrainBlockTemplates[instance.id] = instance;

                    instance.onLoad();
                }

                BepInExLoader.log.LogMessage(string.Format("Patched {0} terrain blocks and added {1} terrain blocks.", BepInExLoader.patchDataTerrainChanges != null ? BepInExLoader.patchDataTerrainChanges.Count : 0, BepInExLoader.patchDataTerrainAdditions != null ? BepInExLoader.patchDataTerrainAdditions.Count : 0));
            }

            if (!hasRun_biomeTemplates && ItemTemplateManager.dict_biomeTemplates != null && ItemTemplateManager.dict_biomeTemplates.Count > 0)
            {
                hasRun_biomeTemplates = true;

                foreach(var entry in ItemTemplateManager.dict_biomeTemplates)
                {
                    var biome = entry.Value;
                    var path = Path.Combine(BepInExLoader.biomeDumpFolder, biome.identifier + ".json");
                    if (BepInExLoader.forceDump.Value || !File.Exists(path))
                    {
                        File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<BiomeDump, BiomeTemplate>(biome), Formatting.Indented, serializerSettings));
                    }

                    if (BepInExLoader.patchDataBiomeChanges != null && BepInExLoader.patchDataBiomeChanges.ContainsKey(biome.identifier))
                    {
                        if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Patching biome {0}", biome.identifier));
                        var changes = BepInExLoader.patchDataResearchChanges[biome.identifier] as JObject;
                        JsonConvert.PopulateObject(changes.ToString(), biome, serializerSettings);
                    }
                }
            }
        }

        public static void onLoadBuildableObjectTemplate(BuildableObjectTemplate __instance)
        {
            var path = Path.Combine(BepInExLoader.buildingsDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<BuildableObjectDump, BuildableObjectTemplate>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataBuildingChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataBuildingChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if (__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching building {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching building {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        public static void LoadAllBuildableObjectTemplatesInBuild(ref Il2CppReferenceArray<BuildableObjectTemplate> __result)
        {
            var result = new Il2CppReferenceArray<BuildableObjectTemplate>(__result.Count + BepInExLoader.patchDataBuildingAdditions.Count);
            for (int i = 0; i < __result.Count; ++i) result[i] = __result[i];
            int index = __result.Count;
            foreach (var entry in BepInExLoader.patchDataBuildingAdditions)
            {
                var source = (JObject)entry.Value;
                if (source == null) throw new Exception("Invalid building:\r\n" + entry.Value.ToString());

                BuildableObjectTemplate template = null;
                if (source.ContainsKey("__template"))
                {
                    var templateIdentifier = source.Value<string>("__template");
                    for (int i = 0; i < __result.Count; ++i)
                    {
                        if (__result[i].identifier == templateIdentifier)
                        {
                            template = __result[i];
                            break;
                        }
                    }
                    if (template == null)
                    {
                        log.LogError(string.Format("Template building {0} not found!", templateIdentifier));
                    }
                }

                if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding building {0}", entry.Key));

                BuildableObjectTemplate instance;
                if (template != null)
                {
                    if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
                    instance = Instantiate(template);
                }
                else
                {
                    instance = ScriptableObject.CreateInstance<BuildableObjectTemplate>();
                }

                instance.identifier = entry.Key;
                JsonConvert.PopulateObject(entry.Value.ToString(), instance, serializerSettings);
                result[index++] = instance;
            }

            BepInExLoader.log.LogMessage(string.Format("Patched {0} buildings and added {1} buildings.", BepInExLoader.patchDataBuildingChanges != null ? BepInExLoader.patchDataBuildingChanges.Count : 0, BepInExLoader.patchDataBuildingAdditions != null ? BepInExLoader.patchDataBuildingAdditions.Count : 0));

            __result = result;
        }

        public static void onLoadResearchTemplate(ResearchTemplate __instance)
        {
            var path = Path.Combine(BepInExLoader.researchDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<ResearchDump, ResearchTemplate>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataResearchChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataResearchChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if (__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching research {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching research {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        public static void onLoadBiomeTemplate(BiomeTemplate __instance)
        {
            log.LogWarning("onLoadBiomeTemplate");
            var path = Path.Combine(BepInExLoader.biomeDumpFolder, __instance.identifier + ".json");
            if (BepInExLoader.forceDump.Value || !File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<BiomeDump, BiomeTemplate>(__instance), Formatting.Indented, serializerSettings));
            }

            if (BepInExLoader.patchDataBiomeChanges != null)
            {
                foreach (var entry in BepInExLoader.patchDataBiomeChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                    {
                        if (BepInExLoader.verbose.Value)
                        {
                            if (__instance.identifier != entry.Key)
                                log.LogInfo(string.Format("Patching biome {0}. Matched '{1}'", __instance.identifier, entry.Key));
                            else
                                log.LogInfo(string.Format("Patching biome {0}", __instance.identifier));
                        }
                        var changes = entry.Value as JObject;
                        if (changes != null)
                        {
                            JsonConvert.PopulateObject(changes.ToString(), __instance, serializerSettings);
                        }
                    }
                }
            }
        }

        private static System.Collections.Generic.IEnumerable<CraftingRecipe> getRecipesForItem(string identifier)
        {
            foreach(var recipe in ItemTemplateManager.dict_craftingRecipesByTag[GameRoot.generateStringHash64("character")])
            {
                bool found = false;
                foreach(var output in recipe.output_data)
                {
                    if(output.identifier == identifier)
                    {
                        found = true;
                        break;
                    }
                }
                if(found)
                {
                    yield return recipe;
                }
            }
        }

        private static System.Collections.Generic.IEnumerable<CraftingRecipe> getCharacterSubRecipes(CraftingRecipe recipe)
        {
            return recipe.input_data.SelectMany(input => getRecipesForItem(input.identifier));//.Where(subRecipe => subRecipe != null && subRecipe.tags.Contains("character"));
        }

        private class RecipeNode
        {
            public CraftingRecipe recipe;
            public RecipeNode previous;

            public RecipeNode(CraftingRecipe recipe, RecipeNode previous)
            {
                this.recipe = recipe;
                this.previous = previous;
            }
        }

        private static void checkForRecipeCycles(CraftingRecipe rootRecipe)
        {
            var recipeQueue = new System.Collections.Generic.Queue<RecipeNode>();
            recipeQueue.Enqueue(new RecipeNode(rootRecipe, null));
            while(recipeQueue.Count > 0)
            {
                var recipePath = recipeQueue.Dequeue();
                foreach (var subRecipe in getCharacterSubRecipes(recipePath.recipe))
                {
                    for (var node = recipePath; node != null; node = node.previous)
                    {
                        if(node.recipe == subRecipe)
                        {
                            var nodeNames = new System.Collections.Generic.List<string>();
                            nodeNames.Add(subRecipe.identifier);
                            for (var mnode = recipePath; mnode != node; mnode = mnode.previous) nodeNames.Add(mnode.recipe.identifier);
                            nodeNames.Add(node.recipe.identifier);
                            log.LogError("Cyclic recipe detected! This will crash!");
                            log.LogError(string.Join(" ← ", nodeNames));
                            return;
                        }
                    }
                    recipeQueue.Enqueue(new RecipeNode(subRecipe, recipePath));
                }
            }
        }

        internal static Regex buildPatternRegex(string pattern)
        {
            return new Regex("^"+Regex.Escape(pattern).Replace(@"\*", @"(?:.*?)")+"$");
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
            public ItemTemplate.ItemTemplateToggleableModeTypes toggleableModeType;
            public Il2CppReferenceArray<ItemTemplate.ItemMode> toggleableModes;
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

        private struct ElementDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public string icon_identifier;
            public ElementTemplate.ElementTemplateFlags flags;
            public Color pipeContentColor;
            public int pipeContentType;
            public string burnable_fuelValueKJPerL_str;
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
            public string _infoBox;
        }

        private struct TerrainBlockDump
        {
            public string identifier;
            public string modIdentifier;
            public string name;
            public TerrainBlockType.TerrainTypeFlags flags;
            public float movementSpeedModifier;
            public Color shatterColor;
            public Color scanColor;
            public bool destructible;
            public string yieldItemOnDig;
            public float miningTimeInSec;
            //public MovementSoundPack movementSoundPack;
            public Color mapColor;
            public TerrainBlockType.DecorFlags decorFlags;
            //public GameObject voxelMeshCorner;
            public bool hasTrim;
            public bool useTextureTiling;
            public bool useLayerNoise;
            public bool geologicalScanner_showOnScan;
            public string ore_yield_id;
            public int maxHeight;
            public int minHeight;
            public uint oreSpawn_chancePerChunk_ground;
            public uint oreSpawn_chancePerChunk_surface;
            public TerrainBlockType.OreSpawnFlags oreSpawnFlags;
            public Vector3Int minSize;
            public Vector3Int maxSize;
            public int sizeRng;
            public int averageYield;
            public string yieldVarietyPercent_str;
            public int depthIncreasePerTile;
            public string miningHardness_str;
            public Il2CppStringArray surfaceOre_worldDecor_identifier;
            public int ore_startChunkX;
            public int ore_startChunkZ;
            public int rmd_ticksPerItem;
            public string rmd_miningYield;
            public int rmd_totalYield;
            public Texture2DProxy texture_abledo;
            public float smoothness;
            public Texture2DProxy texture_height;
            public float height;
            public bool hasSideTextures;
            public Texture2DProxy texture_side_abledo;
            public Texture2DProxy texture_side_mask;
            public float smoothness_side;
            public Texture2DProxy texture_side_height;
            public float height_side;
            public bool hasBottomTextures;
            public Texture2DProxy texture_bottom_abledo;
            public float smoothness_bottom;
            public float height_bottom;
        }

        public struct BuildableObjectDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public bool includeInBuild;
            public BuildableObjectTemplate.BuildableObjectType type;
            public BuildableObjectTemplate.SimulationType simulationType;
            public BuildableObjectTemplate.SimulationSleepFlags simulationSleepFlags;
            public bool simTypeSleep_initial;
            public bool isSuperBuilding;
            public Vector3Int size;
            // public GameObject prefab;
            public bool enableBatching;
            public BuildableObjectTemplate.DragBuildType dragBuildType;
            public float dragModeOrientationSlope_planeAngle;
            public int dragModeOrientationSlope_yOrientationModifier;
            public int dragModeOrientationSlope_yOffsetPerInstance;
            public bool dragModeOrientationSlope_allowSideways;
            public Il2CppReferenceArray<BuildableObjectTemplate.DragMode> dragModes;
            public BuildableObjectTemplate.CustomSnapMode customSnapMode;
            public float demolitionTimeSec;
            public bool canBeDestroyedByDynamite;
            public string conversionGroup_str;
            public bool isVisibleOnMap;
            public byte mapColorPriority;
            public bool skipForRunningIdxGeneration;
            // public AudioClip audioClip_customBuildSound;
            // public AudioClip audioClip_customItemFinishSound;
            public int audioClipIdx_customItemFinishSound;
            public bool hasNameOverride;
            public string nameOverride;
            public BuildableObjectTemplate.ScreenPanelType screenPanelType;
            public bool hasToBeOnFoundation;
            public bool floorShouldOutlineBuilding;
            public BuildableObjectTemplate.FoundationConnectorType foundationConnection;
            public int loaderLevel;
            public bool disableLoaders;
            public bool hasPipeLoaderSupport;
            public Il2CppStructArray<v3i> blockedLoaderPositions;
            public bool rotationAllowed;
            public bool canBeRotatedAroundXAxis;
            public Il2CppStructArray<BuildableObjectTemplate.AdditionalAABB3D> additionalAABBs_input;
            public bool isModularBuilding;
            public BuildableObjectTemplate.ModularBuildingType modularBuildingType;
            public string modularBuildingModule_descriptionName;
            public uint modularBuildingModule_amountItemCost;
            public string modularBuildingModule_unlockedByResearchTemplateIdentifier;
            //public Il2CppReferenceArray<BuildableObjectTemplate.ModularBuildingConnectionNode> modularBuildingConnectionNodes;
            //public Il2CppReferenceArray<BuildableObjectTemplate.ModularBuildingModuleLimit> modularBuildingLimits;
            public Vector3Int modularBuildingLocalSearchAnchor;
            public bool modularBuildingHasConveyorConnectionManager;
            //public Il2CppStructArray<BuildableObjectTemplate.ModularBuildingConveyorConnectionData> modularBuildingConveyorConnectionData;
            public bool modularBuildingHasPipeConnectionManager;
            //public Il2CppReferenceArray<BuildableObjectTemplate.ModularBuildingPipeConnectionData> modularBuildingPipeConnectionData;
            public string modularBuildingPipeAllowedPipeMaxThroughputPerTickInLiter_str;
            public BuildableObjectTemplate.BuildableObjectPowerSubType powerSubType;
            public bool spp_showPowerButton;
            public string energyConsumptionKW_str;
            public bool hasEnergyGridConnection;
            public int powerProducer_drawPriority;
            public bool hasFuelManagerSolid;
            public bool spawnFlyingDebrisWhenExploding;
            // public GameObject debrisWithExplosionForcePrefab;
            public bool hasLightSource;
            //public Il2CppStructArray<BuildableObjectTemplate.LightEmitter> lightEmitters;
            public bool hasPoleGridConnection;
            public int poleGrid_connectionRange;
            public Vector3Int poleGrid_connectorOffset;
            public int poleGrid_maxConnections;
            public int poleGrid_reservedConnections;
            public BuildableObjectTemplate.PoleGridTypes poleGridType;
            public BuildableObjectTemplate.PoleGridTypes poleGridConnectionMatrix;
            public bool hasIntraBuildingWalkways;
            //public Il2CppStructArray<BuildableObjectTemplate.IntraBuildingWalkwayData> intraBuildingWalkwayData;
            public bool hasAdjacentWalkwayOverrides;
            //public Il2CppStructArray<BuildableObjectTemplate.IntraBuildingWalkwayData> adjacentWalkwayOverridesPos;
            public int droneMiner_oreSearchRadius;
            public int droneMiner_itemCapacityPerDrone;
            public string droneMiner_miningPower_str;
            public string droneMiner_miningSpeed_str;
            public string droneMiner_droneCharge_str;
            public int droneMiner_droneCount;
            public Vector3Int droneMiner_dockPositionInside;
            public Vector3Int droneMiner_dockPositionOutside;
            // public AudioClip droneMiner_audioClip_droneHover;
            // public AudioClip droneMiner_audioClip_droneMining;
            // public GameObject droneMiner_dronePrefab;
            public Il2CppSystem.Collections.Generic.List<Vector3Int> droneMiner_list_localBlocksAllowedToBeTraversed;
            public Vector3Int loader_localBeltOffset;
            public int loader_ticksPerAction;
            public string loader_idlePowerConsumption_kjPerS_str;
            // public Mesh loader_meshDefault;
            // public Mesh loader_meshParallel;
            // public Mesh loader_meshStraight;
            // public Material loader_material_default_impostor;
            // public Material loader_material_default_impostor_orange;
            // public GameObject loader_prefabOutputDummy;
            public bool loader_isFilter;
            public string pipeLoader_maxThroughputPerTickInLiter_str;
            public string pipeLoader_idlePowerConsumption_kjPerS_str;
            public Il2CppSystem.Collections.Generic.List<string> pipeLoader_allowedPipeGroupIdentifiers;
            public uint storage_slotSize;
            public string tank_volume_l_str;
            public BuildableObjectTemplate.ProducerRecipeType producerRecipeType;
            public string producer_recipeTimeModifier_str;
            public int producer_energyInputType;
            // public AudioClip producer_audioClip_active;
            public string producer_recipeType_fixed;
            public Il2CppStringArray producer_recipeType_tags;
            public bool conveyor_isSlope;
            public string conveyor_slopePartner_str;
            // public Material conveyor_material;
            // public Material conveyor_material_inv;
            public int conveyor_speed_slotsPerTick;
            // public Texture2D buildingPart_texture_albedo;
            public bool buildingPart_hasSideTextures;
            // public Texture2D buildingPart_texture_side_albedo;
            public bool buildingPart_hasBottomTextures;
            // public Texture2D buildingPart_texture_bottom_albedo;
            // public GameObject voxelMeshCorner;
            // public VoxelTiler voxelTiler;
            public string turbine_powerModifier_str;
            public string turbine_reactionItemIdentifier;
            public string battery_capacityKJ_str;
            public int shippingPad_inventorySlotCount;
            public string shippingPad_requiredChargeKJ_str;
            public string shippingPad_chargePerSecondKJ_str;
            public int shippingPad_timeInSpaceSec;
            public Il2CppStructArray<BuildableObjectTemplate.FluidBox> pipe_fluidBoxes;
            public string pipe_fluidBoxCapacity_l_str;
            public string pipe_groupIdentifier_str;
            public bool pipe_hasVisualUpdate;
            public uint pipe_visualUpdateFluidBoxTemplateIdx;
            public string pipe_MASS_OF_LIQUID_DEFAULT_SQUARED_FPM_str;
            public string pipe_DAMPING_FACTOR_FPM_str;
            public string pipe_FRICTION_OF_LIQUID_DEFAULT_FPM_str;
            public string transformer_transmissionRate_kjPerS_str;
            // public AudioClip transformer_audioClip_active;
            public string biomassBurner_transmissionRate_kjPerS_str;
            public string biomassBurner_source_str;
            // public AudioClip biomassBurner_audioClip_active;
            public string solarPanel_outputMax_str;
            public string solarPanel_outputMin_str;
            public bool solarPanel_rotatingPart;
            public string worldDecor_miningYield_str;
            public int worldDecor_miningYield_amount;
            public float worldDecor_miningTimeSec;
            // public GameObject worldDecor_despawnPrefab;
            // public Material worldDecor_drillMaterial;
            // public AudioClip worldDecor_audioClip_afterHarvesting;
            public bool worldDecor_isDebris;
            public bool worldDecor_useColorHue;
            // public Il2CppReferenceArray<GameObject> worldDecor_huePrefabs;
            //public Il2CppReferenceArray<BuildableObjectTemplate.WorldDecorSpecialDrop> worldDecor_specialDrops;
            public Color worldDecor_scratchColor;
            public string worldDecorGrowing_fullyGrownIdentifier_str;
            public int worldDecorGrowing_growingTimeSec;
            public Vector3 worldDecorGrowing_startingScale;
            //public Il2CppReferenceArray<BuildableObjectTemplate.SuperBuildingLevel> superBuilding_levels;
            public Il2CppStringArray superBuilding_researchUnlocks_str;
            public Il2CppStructArray<Vector3Int> superBuilding_allowedLoaderPositions;
            public int superBuilding_loaderIndicator_rotY;
            public bool superBuilding_refundOnDemolish;
            public string researchLab_sciencePack_str;
            public string pumpjack_amountPerSec_str;
            public Vector3Int pumpjack_drillOffset;
            public int pumpjack_maxDrillDepth;
            // public AudioClip pumpjack_audioClip_active;
            public string burnerGenerator_powerGenertaionRate_kjPerS_str;
            // public AudioClip burnerGenerator_audioClip_active;
            // public Mesh mesh_powerPole;
            // public Material material_powerPolePreview;
            public string terrainBlock_tbtIdentifier;
            // public Material mat_lightOn;
            // public Material mat_lightOff;
            public Vector3Int minecartDepot_connectionPoint;
            public BuildingManager.BuildOrientation minecartDepot_connectionSearchDirection;
            public uint minecartDepot_miningInventorySlots;
            public uint minecartDepot_cartInterval_sec;
            public string minecartDepot_autobuildTrackTemplate_str;
            public Il2CppStructArray<Vector3Int> minecartTracks_connectionPoints;
            public Il2CppStructArray<BuildingManager.BuildOrientation> minecartTracks_connectionSearchDirection;
            public int minecartTracks_slotLength;
            public int freightContainer_speedPerTick;
            public long freightElevator_tierID;
            public string elevatorStation_structureBOT_str;
            // public AudioClip elevatorStation_audioClip_cabinMoving;
            // public AudioClip elevatorStation_audioClip_doorsOpening;
            // public AudioClip elevatorStation_audioClip_doorsClosing;
            // public AudioClip elevatorStation_audioClip_bell;
            public float door_secondsToOpen;
            // public AudioClip door_audioClip_openClose_trigger;
            // public AudioClip door_audioClip_openClose_loop;
            // public AudioClip geologicalScanner_audioClip_active;
            public string blastFurnace_speedModifier;
            public string blastFurnace_outputMultiplier;
            public string blastFurnace_optimalRunningTemp;
            public string blastFurnace_minRunningTemp;
            public string blastFurnace_hotAirTemplateIdentifier;
            public int blastFurnace_shutdownTimer_base_sec;
            public int blastFurnace_shutdownTimer_temp_sec;
            public string blastFurnace_towerModule_capacity;
            public string blastFurnace_towerModule_speedIncrease;
            public string blastFurnace_towerModuleBotIdentifier;
            public string blastFurnace_gasExhaustDrainModuleBotIdentifier;
            public string blastFurnace_towerModule_hotAirConsumptionPercentIncrease;
            public string blastFurnace_baseHotAirConsumptionPerTick;
            public string blastFurnace_maxHeatLossPerTick;
            public string blastFurnace_heatGainPerTick;
            public string blastFurnace_speedPercentageAtMinRunningTemp;
            public byte resourceConverter_type;
            public string resourceConverter_powerConsumption_kjPerSec;
            public Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput> resourceConverter_input_elemental;
            public Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput> resourceConverter_output_elemental;
            //public Il2CppReferenceArray<BuildableObjectTemplate.ResourceConverterModuleSpeedBonus> resourceConverter_speedBonusModules;
            public bool resourceConverter_hasAdjacencyBonus;
            public string resourceConverter_powerDecreasePerAdjacentResourceConverter;
            public byte resourceConverter_adjacencyBonusAxis;
            //public AudioClip resourceConverter_audioClip_active;
        }

        public struct ResearchDump
        {
            public string identifier;
            public string modIdentifier;
            public string name;
            public string icon_identifier;
            public ResearchTemplate.ResearchTemplateFlags flags;
            public bool isInternal;
            public bool manualEndlessResearch;
            public string manualEndlessResearchTemplate_str;
            public string description;
            public Il2CppReferenceArray<ResearchTemplate.ResearchTemplateItemInput> input_data;
            public Il2CppSystem.Collections.Generic.List<string> list_researchDependencies_str;
            public Il2CppSystem.Collections.Generic.List<string> list_craftingUnlocks_str;
            public Il2CppSystem.Collections.Generic.List<string> list_blastFurnaceModes_str;
            public Il2CppSystem.Collections.Generic.List<string> list_tutorialMessageIdentifierUnlocks;
            public string mapScanner_ore_identifier;
            public string mapScanner_reservoir_identifier;
            public int inventorySize_additionalInventorySlots;
            public int endlessResearch_minLevelToDisplay;
            public string characterCraftingSpeed_additionalDecrementPercentage_str;
            public string miningDrillSpeed_miningTimeMultiplier_str;
        }

        public struct BiomeDump
        {
            public string identifier;
            public string modIdentifier;
            public string name;
            public bool isHeightBased;
            public int lowestPossibleHeight;
            public string surfaceBlock_identifier;
            public string groundBlock_identifier;
            public Il2CppStringArray decorIdentifier;
            public Il2CppStringArray vegetationIdentifier;
            public uint vegetationChancePerBlockPercent;
        }

        public struct Texture2DProxy
        {
            public string name;

            public Texture2DProxy(string name)
            {
                this.name = name;
            }

            public Texture2DProxy(Texture2D texture)
            {
                this.name = (texture is null) ? "" : texture.name;
            }
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
                    if (property.PropertyType == typeof(Sprite))
                    {
                        _memberWriters[i] = (JObject target, object value, JsonSerializer serializer) => target.Add(property.Name, JToken.FromObject(((Sprite)property.GetValue(value)).name, serializer));
                        _memberReaders[i] = (JObject source, object value, JsonSerializer serializer) => property.SetValue(value, PluginComponent.getIcon(source[property.Name].Value<string>()));
                    }
                    else
                    {
                        _memberWriters[i] = (JObject target, object value, JsonSerializer serializer) => target.Add(property.Name, JToken.FromObject(property.GetValue(value), serializer));
                        _memberReaders[i] = (JObject source, object value, JsonSerializer serializer) => property.SetValue(value, PluginComponent.invokeValue(property.PropertyType, source[property.Name]));
                    }
                }
                else
                {
                    var field = typeof(T).GetField(memberNames[i]);
                    if (field != null)
                    {
                        if (field.FieldType == typeof(Sprite))
                        {
                            _memberWriters[i] = (JObject target, object value, JsonSerializer serializer) => target.Add(field.Name, JToken.FromObject(((Sprite)field.GetValue(value)).name, serializer));
                            _memberReaders[i] = (JObject source, object value, JsonSerializer serializer) => field.SetValue(value, PluginComponent.getIcon(source[field.Name].Value<string>()));
                        }
                        else
                        {
                            _memberWriters[i] = (JObject target, object value, JsonSerializer serializer) => target.Add(field.Name, JToken.FromObject(field.GetValue(value), serializer));
                            _memberReaders[i] = (JObject source, object value, JsonSerializer serializer) => field.SetValue(value, PluginComponent.invokeValue(field.FieldType, source[field.Name]));
                        }
                    }
                    else
                    {
                        BepInExLoader.log.LogError(string.Format("Member {0} not found!", memberNames[i]));
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
                            if(BepInExLoader.verbose.Value) BepInExLoader.log.LogInfo(string.Format("Deleting {0} {1}.", typeof(E).Name, identifier));
                            targets.RemoveAt(index);
                            changed = true;
                        }
                        else
                        {
                            if (BepInExLoader.verbose.Value) BepInExLoader.log.LogInfo(string.Format("Patching {0} {1}.", typeof(E).Name, identifier));
                            targets[index] = target;
                            changed = true;
                        }
                    }
                    else
                    {
                        var target = typeof(E).GetConstructor(new[] { typeof(IntPtr) }).Invoke(new object[] { ClassInjector.DerivedConstructorPointer<E>() });
                        identifierProperty.SetValue(target, identifier);
                        serializer.Populate(new JTokenReader(source), target);
                        if (BepInExLoader.verbose.Value) BepInExLoader.log.LogInfo(string.Format("Adding {0} {1}.", typeof(E).Name, identifier));
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
                    var propertyValue = property.GetValue(source[i]);
                    if (propertyValue != null)
                    {
                        element[property.Name] = JToken.FromObject(propertyValue, serializer);
                    }
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

    class EnumFlagsConverterByte<E> : JsonConverter where E : Enum
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(E);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            byte flags = 0;
            foreach(var flagString in JToken.ReadFrom(reader).Value<string>().Split('|'))
            {
                try
                {
                    E target = (E)Enum.Parse(objectType, flagString, true);
                    flags |= (byte)(object)target;
                }
                catch { }
            }
            return (E)(object)flags;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sourceFlags = (byte)value;
            var flagsString = "";
            foreach (var flag in Enum.GetValues(typeof(E)))
            {
                if ((sourceFlags & (byte)flag) != 0)
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

    class StringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = JToken.ReadFrom(reader).ToString();
            //var source = serializer.Deserialize<string>(reader);
            if (source.StartsWith("{") && source.EndsWith("}"))
            {
                try
                {
                    var e = new NCalc.Expression(source.Substring(1, source.Length - 2));
                    e.EvaluateFunction += NCalcExtensions.Extend;
                    e.Parameters["value"] = (existingValue as string) ?? "";
                    var result = e.Evaluate() ?? "";
                    return result.ToString();
                }
                catch (Exception e)
                {
                    BepInExLoader.log.LogError(e);
                    return source;
                }
            }

            return source;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    class NumericConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            switch (Type.GetTypeCode(objectType))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = JToken.ReadFrom(reader).ToString();
            if (source.StartsWith("{") && source.EndsWith("}"))
            {
                try
                {
                    var e = new NCalc.Expression(source.Substring(1, source.Length - 2));
                    e.EvaluateFunction += NCalcExtensions.Extend;
                    e.Parameters["value"] = existingValue ?? 0;
                    var result = e.Evaluate() ?? 0;
                    return Convert.ChangeType(result, objectType);
                }
                catch (Exception e)
                {
                    BepInExLoader.log.LogError(e);
                    return source;
                }
            }

            return Convert.ChangeType(source, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }

    class ListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Il2CppSystem.Collections.Generic.List<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = (JArray)JToken.ReadFrom(reader);
            var target = new Il2CppSystem.Collections.Generic.List<T>(source.Count);
            for(int i = 0; i < source.Count; ++i) target.Add(serializer.Deserialize<T>(new JTokenReader(source[i])));
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var source = (Il2CppSystem.Collections.Generic.List<T>)value;
            writer.WriteStartArray();
            foreach(var element in source) serializer.Serialize(writer, element);
            writer.WriteEndArray();
        }
    }

    class ReferenceArrayConverter<T> : JsonConverter where T : Il2CppObjectBase
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Il2CppReferenceArray<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = (JArray)JToken.ReadFrom(reader);
            var target = new Il2CppReferenceArray<T>(source.Count);
            for(int i = 0; i < source.Count; ++i) target[i] = serializer.Deserialize<T>(new JTokenReader(source[i]));
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var source = (Il2CppReferenceArray<T>)value;
            writer.WriteStartArray();
            foreach(var element in source) serializer.Serialize(writer, element);
            writer.WriteEndArray();
        }
    }

    class Texture2DProxyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PluginComponent.Texture2DProxy);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new PluginComponent.Texture2DProxy(JToken.ReadFrom(reader).Value<string>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var texture = (PluginComponent.Texture2DProxy)value;
            writer.WriteValue(texture.name);
        }
    }

    class SpriteConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Sprite);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return PluginComponent.getIcon(JToken.ReadFrom(reader).Value<string>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sprite = (Sprite)value;
            writer.WriteValue(sprite.name);
        }
    }

    public static class ResourceExt
    {
        static System.Collections.Generic.Dictionary<string, Texture2D> loadedTextures = new System.Collections.Generic.Dictionary<string, Texture2D>();
        static Texture2D[] allTextures1 = null;
        static Texture2D[] allTextures2 = null;

        public static void RegisterTexture(string name, Texture2D texture)
        {
            loadedTextures[name] = texture;
        }

        public static Texture2D FindTexture(string name)
        {
            var tweakPath = Path.Combine(BepInExLoader.texturesFolder, name + ".png");
            Texture2D result;
            if (loadedTextures.TryGetValue(name, out result))
            {
                return result;
            }
            else if (File.Exists(tweakPath))
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                var tempTexture = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
                tempTexture.LoadImage(new Il2CppStructArray<byte>(File.ReadAllBytes(tweakPath)), false);
                var texture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGB24, true, true);
                texture.SetPixels(tempTexture.GetPixels());
                texture.Apply(true);
                texture.Compress(false);
                texture.Apply();
                watch.Stop();
                if (BepInExLoader.verbose.Value) BepInExLoader.log.LogInfo(string.Format("Loading texture '{0}' from '{1}' took {2}ms", name, tweakPath, watch.ElapsedMilliseconds));
                loadedTextures.Add(name, texture);
                GameObject.Destroy(tempTexture);
                return texture;
            }
            else
            {
                if (BepInExLoader.verbose.Value) BepInExLoader.log.LogInfo(string.Format("Searching for texture '{0}'", name));

                if(allTextures1 == null) allTextures1 = Resources.FindObjectsOfTypeAll<Texture2D>();
                foreach (Texture2D texture in allTextures1)
                {
                    if (texture.name == name)
                    {
                        loadedTextures.Add(name, texture);
                        return texture;
                    }
                }

                if (allTextures2 == null) allTextures2 = Resources.LoadAll<Texture2D>("");
                foreach (Texture2D texture in allTextures2)
                {
                    if (texture.name == name)
                    {
                        loadedTextures.Add(name, texture);
                        return texture;
                    }
                }

                loadedTextures.Add(name, null);
                BepInExLoader.log.LogError("Could not find texture: " + name);
                return null;
            }
        }

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }
    }
}