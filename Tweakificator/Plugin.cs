using C3;
using C3.ModKit;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unfoundry;
using UnityEngine;
using TinyJSON;

namespace Tweakificator
{
    [UnfoundryMod(GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "Tweakificator",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "2.0.0";

        public static LogSource log;

        public static TypedConfigEntry<bool> forceDump;
        public static TypedConfigEntry<bool> dumpIcons;
        public static TypedConfigEntry<bool> verbose;

        public static string assemblyFolder;
        public static string dumpFolder;
        public static string itemsDumpFolder;
        public static string elementsDumpFolder;
        public static string recipesDumpFolder;
        public static string terrainBlocksDumpFolder;
        public static string buildingsDumpFolder;
        public static string researchDumpFolder;
        public static string biomeDumpFolder;
        public static string iconsDumpFolder;
        public static string tweaksFolder;
        public static string iconsFolder;
        public static string texturesFolder;

        public static bool firstRun = false;

        public static Variant patchData = null;
        public static Variant patchDataItemChanges = null;
        public static Variant patchDataElementChanges = null;
        public static Variant patchDataRecipeChanges = null;
        public static Variant patchDataTerrainChanges = null;
        public static Variant patchDataBuildingChanges = null;
        public static Variant patchDataResearchChanges = null;
        public static Variant patchDataBiomeChanges = null;
        public static Variant patchDataItemAdditions = null;
        public static Variant patchDataElementAdditions = null;
        public static Variant patchDataRecipeAdditions = null;
        public static Variant patchDataTerrainAdditions = null;
        public static Variant patchDataBuildingAdditions = null;
        public static Variant patchDataResearchAdditions = null;
        public static Variant patchDataBiomeAdditions = null;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            assemblyFolder = Path.Combine(Path.GetFullPath("."), MODNAME.ToLower());
            dumpFolder = Path.Combine(assemblyFolder, "Dumps");
            itemsDumpFolder = Path.Combine(dumpFolder, "Items");
            elementsDumpFolder = Path.Combine(dumpFolder, "Elements");
            recipesDumpFolder = Path.Combine(dumpFolder, "Recipes");
            terrainBlocksDumpFolder = Path.Combine(dumpFolder, "TerrainBlocks");
            buildingsDumpFolder = Path.Combine(dumpFolder, "Buildings");
            researchDumpFolder = Path.Combine(dumpFolder, "Research");
            biomeDumpFolder = Path.Combine(dumpFolder, "Biomes");
            iconsDumpFolder = Path.Combine(dumpFolder, "Icons");
            tweaksFolder = Path.Combine(assemblyFolder, "..\\tweaks");
            iconsFolder = Path.Combine(tweaksFolder, "icons");
            texturesFolder = Path.Combine(tweaksFolder, "textures");

            new Config(GUID)
                .Group("Dump")
                    .Entry(out forceDump, "forceDump", false, true, "Overwrite existing dump files.")
                    .Entry(out dumpIcons, "dumpIcons", false, true, "Dump icon files. (very slow)")
                .EndGroup()
                .Group("Log")
                    .Entry(out verbose, "verbose", false, true, "Log extra information.")
                .EndGroup()
                .Load()
                .Save();

            if (!Directory.Exists(dumpFolder))
            {
                Directory.CreateDirectory(dumpFolder);
                firstRun = true;
            }

            if (!Directory.Exists(itemsDumpFolder)) Directory.CreateDirectory(itemsDumpFolder);
            if (!Directory.Exists(elementsDumpFolder)) Directory.CreateDirectory(elementsDumpFolder);
            if (!Directory.Exists(recipesDumpFolder)) Directory.CreateDirectory(recipesDumpFolder);
            if (!Directory.Exists(terrainBlocksDumpFolder)) Directory.CreateDirectory(terrainBlocksDumpFolder);
            if (!Directory.Exists(buildingsDumpFolder)) Directory.CreateDirectory(buildingsDumpFolder);
            if (!Directory.Exists(researchDumpFolder)) Directory.CreateDirectory(researchDumpFolder);
            if (!Directory.Exists(biomeDumpFolder)) Directory.CreateDirectory(biomeDumpFolder);
            if (!Directory.Exists(iconsDumpFolder)) Directory.CreateDirectory(iconsDumpFolder);
            if (!Directory.Exists(tweaksFolder)) Directory.CreateDirectory(tweaksFolder);
            if (!Directory.Exists(iconsFolder)) Directory.CreateDirectory(iconsFolder);
            if (!Directory.Exists(texturesFolder)) Directory.CreateDirectory(texturesFolder);

            foreach (var path in Directory.GetFiles(tweaksFolder, "*.json"))
            {
                var patch = JSON.Load(File.ReadAllText(path));
                if (patchData == null)
                {
                    log.LogFormat("Loading patch {0}", Path.GetFileName(path));
                    patchData = patch;
                }
                else
                {
                    log.LogFormat("Merging patch {0}", Path.GetFileName(path));
                    patchData.Merge(patch);
                }
            }
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
        }

        [HarmonyPatch]
        public static class Patch
        {
            //            //[HarmonyPatch(typeof(ResourceDB), nameof(ResourceDB.InitOnApplicationStart))]
            //            //[HarmonyPrefix]
            //            public static void ResourceDBInitOnApplicationStart()
            //            {
            //                var dict_icons = typeof(ResourceDB).GetField("dict_icons", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<int, Dictionary<ulong, Sprite>>;

            //                var filenames = Directory.EnumerateFiles(iconsFolder, "*.png").ToArray<string>();
            //                log.Log(string.Format("Loading {0} custom icons.", filenames.Length));
            //                foreach (var filename in filenames)
            //                {
            //                    var iconPath = Path.Combine(iconsFolder, filename);
            //                    var identifier = Path.GetFileNameWithoutExtension(iconPath);
            //                    if (!dict_icons[0].ContainsKey(GameRoot.generateStringHash64(identifier)))
            //                    {
            //                        var watch = new System.Diagnostics.Stopwatch();
            //                        watch.Start();
            //                        var iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            //                        iconTexture.LoadImage(File.ReadAllBytes(iconPath), true);
            //                        int index = 0;
            //                        foreach (var entry in iconSizes)
            //                        {
            //                            var sizeId = entry.Key;
            //                            var size = entry.Value;
            //                            var sizeIdentifier = identifier + ((sizeId > 0) ? "_" + sizeId.ToString() : "");
            //                            var texture = (sizeId > 0) ? resizeTexture(iconTexture, size, size) : iconTexture;
            //                            texture.name = sizeIdentifier;
            //                            dict_icons[sizeId][GameRoot.generateStringHash64(sizeIdentifier)] = createSprite(texture);

            //                            ++index;
            //                        }

            //                        watch.Stop();
            //                        if (verbose.Get()) log.Log(string.Format("Loading icon '{0}' from '{1}' took {2}ms", identifier, iconPath, watch.ElapsedMilliseconds));
            //                    }
            //                }

            //                var listPath = Path.Combine(iconsDumpFolder, "__icons.txt");
            //                if (forceDump.Get() || !File.Exists(listPath))
            //                {
            //                    var iconNames = new List<string>();
            //                    foreach (var entry in dict_icons[0]) iconNames.Add(entry.Value.name);// string.Format("{0}: {1}", entry.Value.name, entry.Value.texture.format.ToString()));
            //                    File.WriteAllText(listPath, string.Join("\r\n", iconNames));
            //                }

            //                if (dumpIcons.Get())
            //                {
            //                    var cache = new Dictionary<string, Texture2D>();
            //                    foreach (var entry in dict_icons)
            //                    {
            //                        foreach (var entry2 in entry.Value)
            //                        {
            //                            var sprite = entry2.Value;
            //                            var path = Path.Combine(iconsDumpFolder, sprite.name + ".png");
            //                            if (!File.Exists(path))
            //                            {
            //                                if (!cache.TryGetValue(sprite.texture.name, out Texture2D texture))
            //                                {
            //                                    if (verbose.Get()) log.Log(string.Format("Converting icon texture '{0}'", sprite.texture.name));
            //                                    texture = duplicateTexture(sprite.texture);
            //                                    cache[sprite.texture.name] = texture;
            //                                }

            //                                if (verbose.Get()) log.Log(string.Format("Dumping icon '{0}'", sprite.name));
            //                                var croppedTexture = new Texture2D(Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), TextureFormat.RGBA32, false);
            //                                var pixels = texture.GetPixels(Mathf.FloorToInt(sprite.textureRect.x), Mathf.FloorToInt(sprite.textureRect.y), Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), 0);
            //                                croppedTexture.SetPixels(pixels);
            //                                croppedTexture.Apply();
            //                                var bytes = croppedTexture.EncodeToPNG();
            //                                File.WriteAllBytes(path, bytes);

            //                                Object.Destroy(croppedTexture);
            //                                System.GC.Collect();
            //                            }
            //                        }
            //                    }
            //                    foreach (var texture in cache.Values) Object.Destroy(texture);
            //                    cache.Clear();
            //                    System.GC.Collect();
            //                }
            //            }

            [HarmonyPatch(typeof(ItemTemplate), nameof(ItemTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadItemTemplate(ItemTemplate __instance)
            {
                hasLoaded_items = true;
                //log.LogFormat("onLoadItemTemplate: {0}", __instance.name);

                var path = Path.Combine(itemsDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<ItemDump, ItemTemplate>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataItemChanges != null && patchDataItemChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching item {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching item {0}", __instance.identifier);
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(ElementTemplate), nameof(ElementTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadElementTemplate(ElementTemplate __instance)
            {
                //log.LogFormat("onLoadElementTemplate: {0}", __instance.name);

                var path = Path.Combine(elementsDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<ElementDump, ElementTemplate>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataElementChanges != null && patchDataElementChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching element {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching element {0}", __instance.identifier);
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.onLoad))]
            [HarmonyPrefix]
            public static void onLoadRecipe(CraftingRecipe __instance)
            {
                //log.LogFormat("onLoadRecipe: {0}", __instance.name);

                var path = Path.Combine(recipesDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<RecipeDump, CraftingRecipe>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataRecipeChanges != null && patchDataRecipeChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching recipe {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching recipe {0}", __instance.identifier);
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(BuildableObjectTemplate), nameof(BuildableObjectTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadBuildableObjectTemplate(BuildableObjectTemplate __instance)
            {
                //log.LogFormat("onLoadBuildableObjectTemplate: {0}", __instance.name);

                var path = Path.Combine(buildingsDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<BuildableObjectDump, BuildableObjectTemplate>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataBuildingChanges != null && patchDataRecipeChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching building {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching building {0}", __instance.identifier);
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(ResearchTemplate), nameof(ResearchTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadResearchTemplate(ResearchTemplate __instance)
            {
                //log.LogFormat("onLoadResearchTemplate: {0}", __instance.name);

                var path = Path.Combine(researchDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<ResearchDump, ResearchTemplate>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataResearchChanges != null && patchDataRecipeChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching research {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching research {0}", __instance.identifier);
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(BiomeTemplate), nameof(BiomeTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadBiomeTemplate(BiomeTemplate __instance)
            {
                //log.LogFormat("onLoadBiomeTemplate: {0}", __instance.name);

                var path = Path.Combine(biomeDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<BiomeDump, BiomeTemplate>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataBiomeChanges != null && patchDataRecipeChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching biome {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching biome {0}", __instance.identifier);
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(TerrainBlockType), nameof(TerrainBlockType.onLoad))]
            [HarmonyPrefix]
            public static void onLoadTerrainBlockType(TerrainBlockType __instance)
            {
                //log.LogFormat("onLoadTerrainBlockType: {0}", __instance.name);

                var path = Path.Combine(terrainBlocksDumpFolder, __instance.identifier + ".json");
                if (forceDump.Get() || !File.Exists(path))
                {
                    File.WriteAllText(path, JSON.Dump(gatherDump<TerrainBlockDump, TerrainBlockType>(__instance), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
                }

                if (patchDataTerrainChanges != null && patchDataRecipeChanges is ProxyObject changeMap)
                {
                    foreach (var entry in changeMap)
                    {
                        if (buildPatternRegex(entry.Key).IsMatch(__instance.identifier))
                        {
                            if (entry.Value is ProxyObject changes)
                            {
                                if (verbose.Get())
                                {
                                    if (__instance.identifier != entry.Key)
                                        log.LogFormat("Patching terrain block {0}. Matched '{1}'", __instance.identifier, entry.Key);
                                    else
                                        log.LogFormat("Patching terrain block {0}", __instance.identifier);
                                }

                                if (changes.ContainsKey("texture_abledo"))
                                {
                                    __instance.texture_abledo = (Texture2D)ResourceExt.FindTexture(changes["texture_abledo"].ToString());
                                }
                                if (changes.ContainsKey("texture_bottom_abledo"))
                                {
                                    __instance.texture_bottom_abledo = (Texture2D)ResourceExt.FindTexture(changes["texture_bottom_abledo"].ToString());
                                }
                                if (changes.ContainsKey("texture_bottom_metalSmoothHeight"))
                                {
                                    __instance.texture_bottom_metalSmoothHeight = (Texture2D)ResourceExt.FindTexture(changes["texture_bottom_metalSmoothHeight"].ToString());
                                }
                                if (changes.ContainsKey("texture_height"))
                                {
                                    __instance.texture_metalHeightSmoothHeight = (Texture2D)ResourceExt.FindTexture(changes["texture_metalHeightSmoothHeight"].ToString());
                                }
                                if (changes.ContainsKey("texture_side_abledo"))
                                {
                                    __instance.texture_side_abledo = (Texture2D)ResourceExt.FindTexture(changes["texture_side_abledo"].ToString());
                                }
                                if (changes.ContainsKey("texture_side_metalSmoothHeight"))
                                {
                                    __instance.texture_side_metalSmoothHeight = (Texture2D)ResourceExt.FindTexture(changes["texture_side_metalSmoothHeight"].ToString());
                                }
                                if (changes.ContainsKey("texture_side_mask"))
                                {
                                    __instance.texture_side_mask = (Texture2D)ResourceExt.FindTexture(changes["texture_side_mask"].ToString());
                                }
                                if (changes.ContainsKey("texture_side_mask_metalSmoothHeight"))
                                {
                                    __instance.texture_side_mask_metalSmoothHeight = (Texture2D)ResourceExt.FindTexture(changes["texture_side_mask_metalSmoothHeight"].ToString());
                                }
                                changes.Populate(ref __instance);
                            }
                        }
                    }
                }
            }

            private static bool hasLoaded_items = false;
            private static bool hasRun_items = false;
            class SimpleEnumerator : IEnumerable
            {
                public IEnumerator enumerator;
                IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
                public IEnumerator GetEnumerator()
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        if (!hasRun_items && hasLoaded_items)
                        {
                            hasRun_items = true;

                            var itemTemplates = AssetManager.getAllAssetsOfType<ItemTemplate>();

                            foreach (var entry in itemTemplates.Values)
                            {
                            }
                        }
                        yield return item;
                    }
                }
            }

            [HarmonyPatch(typeof(ItemTemplateManager), nameof(ItemTemplateManager.InitOnApplicationStart))]
            [HarmonyPostfix]
            static void onItemTemplateManagerInitOnApplicationStart(ref IEnumerator __result)
            {
                log.LogFormat("onItemTemplateManagerInitOnApplicationStart");
                var myEnumerator = new SimpleEnumerator()
                {
                    enumerator = __result
                };
                __result = myEnumerator.GetEnumerator();
            }

            //            //[HarmonyPatch(typeof(ItemTemplate), nameof(ItemTemplate.LoadAllItemTemplatesInBuild))]
            //            //[HarmonyPrefix]
            //            public static void LoadAllItemTemplatesInBuild(ref ItemTemplate[] __result)
            //            {
            //                var result = new ItemTemplate[__result.Length + patchDataItemAdditions.Count];
            //                for (int i = 0; i < __result.Length; ++i) result[i] = __result[i];
            //                int index = __result.Length;
            //                foreach (var entry in patchDataItemAdditions)
            //                {
            //                    var source = (JObject)entry.Value ?? throw new System.Exception("Invalid item:\r\n" + entry.Value.ToString());
            //                    ItemTemplate template = null;
            //                    if (source.ContainsKey("__template"))
            //                    {
            //                        var templateIdentifier = source.Value<string>("__template");
            //                        for (int i = 0; i < __result.Length; ++i)
            //                        {
            //                            if (__result[i].identifier == templateIdentifier)
            //                            {
            //                                template = __result[i];
            //                                break;
            //                            }
            //                        }
            //                        if (template == null)
            //                        {
            //                            log.LogError(string.Format("Template item {0} not found!", templateIdentifier));
            //                        }
            //                    }

            //                    if (verbose.Get()) log.Log(string.Format("Adding item {0}", entry.Key));

            //                    ItemTemplate instance;
            //                    if (template != null)
            //                    {
            //                        if (verbose.Get()) log.Log(string.Format("Using template {0}", template.identifier));
            //                        instance = Object.Instantiate<ItemTemplate>(template);
            //                    }
            //                    else
            //                    {
            //                        instance = ScriptableObject.CreateInstance<ItemTemplate>();
            //                        instance.railMiner_terrainTargetList_str = new string[0];
            //                    }

            //                    instance.identifier = entry.Key;
            //                    JsonConvert.PopulateObject(entry.Value.ToString(), instance, serializerSettings);
            //                    result[index++] = instance;
            //                }

            //                log.Log(string.Format("Patched {0} items and added {1} items.", patchDataItemChanges != null ? patchDataItemChanges.Count : 0, patchDataItemAdditions != null ? patchDataItemAdditions.Count : 0));

            //                __result = result;
            //            }

            //            //public static void LoadAllCraftingRecipesInBuild(ref CraftingRecipe[] __result)
            //            //{
            //            //    var result = new CraftingRecipe[__result.Length + patchDataRecipeAdditions.Count];
            //            //    for (int i = 0; i < __result.Length; ++i) result[i] = __result[i];
            //            //    int index = __result.Length;
            //            //    foreach (var entry in patchDataRecipeAdditions)
            //            //    {
            //            //        var source = (JObject)entry.Value;
            //            //        if (source == null) throw new System.Exception("Invalid recipe:\r\n" + entry.Value.ToString());

            //            //        CraftingRecipe template = null;
            //            //        if (source.ContainsKey("__template"))
            //            //        {
            //            //            var templateIdentifier = source.Value<string>("__template");
            //            //            for (int i = 0; i < __result.Count; ++i)
            //            //            {
            //            //                if (__result[i].identifier == templateIdentifier)
            //            //                {
            //            //                    template = __result[i];
            //            //                    break;
            //            //                }
            //            //            }
            //            //            if (template == null)
            //            //            {
            //            //                log.LogError(string.Format("Template recipe {0} not found!", templateIdentifier));
            //            //            }
            //            //        }

            //            //        if (verbose.Get()) log.Log(string.Format("Adding recipe {0}", entry.Key));
            //            //        CraftingRecipe instance;
            //            //        if (template != null)
            //            //        {
            //            //            if (verbose.Get()) log.Log(string.Format("Using template {0}", template.identifier));
            //            //            instance = Instantiate<CraftingRecipe>(template);
            //            //        }
            //            //        else
            //            //        {
            //            //            instance = ScriptableObject.CreateInstance<CraftingRecipe>();
            //            //            instance.tags = new string[0];
            //            //            instance.input_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput>(0);
            //            //            instance.output_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeItemInput>(0);
            //            //            instance.inputElemental_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput>(0);
            //            //            instance.outputElemental_data = new Il2CppReferenceArray<CraftingRecipe.CraftingRecipeElementalInput>(0);
            //            //            instance.tagHashes = new ulong[0];
            //            //            instance.input = new KeyValuePair<ItemTemplate, uint>[0];
            //            //            instance.output = new KeyValuePair<ItemTemplate, uint>[0];
            //            //            instance.input_elemental = new KeyValuePair<ElementTemplate, long>[0];
            //            //            instance.output_elemental = new KeyValuePair<ElementTemplate, long>[0];
            //            //        }

            //            //        instance.identifier = entry.Key;
            //            //        JsonConvert.PopulateObject(entry.Value.ToString(), instance, serializerSettings);
            //            //        result[index++] = instance;
            //            //    }

            //            //    log.Log(string.Format("Patched {0} recipes and added {1} recipes.", patchDataRecipeChanges != null ? patchDataRecipeChanges.Count : 0, patchDataRecipeAdditions != null ? patchDataRecipeAdditions.Count : 0));

            //            //    __result = result;
            //            //}

            //            //private static bool hasRun_researchTemplates = false;
            //            //private static bool hasRun_terrainBlockTemplates = false;
            //            //private static bool hasRun_terrainBlockScratchGroups = false;
            //            //private static bool hasRun_biomeTemplates = false;
            //            //private static bool hasRun_craftingRecipes = false;
            //            private static bool hasRun_items = false;
            //            public static void onItemTemplateManagerInitOnApplicationStart()
            //            {
            //                if (!hasRun_items)
            //                {
            //                    var itemTemplates = AssetManager.getAllAssetsOfType<ItemTemplate>();
            //                    if (itemTemplates != null && itemTemplates.Count > 0)
            //                    {
            //                        hasRun_items = true;

            //                        foreach (var entry in itemTemplates)
            //                        {
            //                        }
            //                    }
            //                }

            //                //if (!hasRun_craftingRecipes && ItemTemplateManager.dict_craftingRecipes != null && ItemTemplateManager.dict_craftingRecipes.Count > 0)
            //                //{
            //                //    hasRun_craftingRecipes = true;

            //                //    foreach (var entry in ItemTemplateManager.dict_craftingRecipes)
            //                //    {
            //                //        if (entry.Value.tags.Contains("character")) checkForRecipeCycles(entry.Value);
            //                //    }
            //                //}

            //                //if (!hasRun_researchTemplates && ItemTemplateManager.dict_researchTemplates != null && ItemTemplateManager.dict_researchTemplates.Count > 0)
            //                //{
            //                //    hasRun_researchTemplates = true;

            //                //    foreach (var entry in BepInExLoader.patchDataResearchAdditions)
            //                //    {
            //                //        var source = (JObject)entry.Value;
            //                //        if (source == null) throw new System.Exception("Invalid research:\r\n" + entry.Value.ToString());

            //                //        ResearchTemplate template = null;
            //                //        if (source.ContainsKey("__template"))
            //                //        {
            //                //            var templateIdentifier = source.Value<string>("__template");
            //                //            foreach (var templateEntry in ItemTemplateManager.dict_researchTemplates.Values)
            //                //            {
            //                //                if (templateEntry.identifier == templateIdentifier)
            //                //                {
            //                //                    template = templateEntry;
            //                //                    break;
            //                //                }
            //                //            }
            //                //            if (template == null)
            //                //            {
            //                //                log.LogError(string.Format("Template research {0} not found!", templateIdentifier));
            //                //            }
            //                //        }

            //                //        if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding research {0}", entry.Key));

            //                //        ResearchTemplate instance;
            //                //        if (template != null)
            //                //        {
            //                //            if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
            //                //            instance = Instantiate<ResearchTemplate>(template);
            //                //        }
            //                //        else
            //                //        {
            //                //            instance = ScriptableObject.CreateInstance<ResearchTemplate>();
            //                //            instance.list_blastFurnaceModes_str = new List<string>(0);
            //                //            instance.list_craftingUnlocks_str = new List<string>(0);
            //                //            instance.list_researchDependencies_str = new List<string>(0);
            //                //            instance.input_data = new Il2CppReferenceArray<ResearchTemplate.ResearchTemplateItemInput>(0);
            //                //        }

            //                //        instance.identifier = entry.Key;
            //                //        instance.id = GameRoot.generateStringHash64("rt_" + instance.identifier);
            //                //        instance.id32 = GameRoot.generateStringHash32("rt_" + instance.identifier);

            //                //        JsonConvert.PopulateObject(source.ToString(), instance, serializerSettings);

            //                //        ItemTemplateManager.dict_researchTemplates[instance.id] = instance;

            //                //        instance.onLoad();
            //                //    }

            //                //    BepInExLoader.log.LogMessage(string.Format("Patched {0} research and added {1} research.", BepInExLoader.patchDataResearchChanges != null ? BepInExLoader.patchDataResearchChanges.Count : 0, BepInExLoader.patchDataResearchAdditions != null ? BepInExLoader.patchDataResearchAdditions.Count : 0));
            //                //}

            //                //if (!hasRun_terrainBlockScratchGroups && ItemTemplateManager.dict_terrainBlockScratchGroups != null && ItemTemplateManager.dict_terrainBlockScratchGroups.Count > 0)
            //                //{
            //                //    hasRun_terrainBlockScratchGroups = true;

            //                //    foreach (var entry in BepInExLoader.patchDataTerrainAdditions)
            //                //    {
            //                //        if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Generate scratch group for {0}", entry.Key));

            //                //        var source = (JObject)entry.Value;
            //                //        if (source == null) throw new System.Exception("Invalid terrain block:\r\n" + entry.Value.ToString());

            //                //        string terrainBlockIdentifier;
            //                //        if (source.ContainsKey("__template"))
            //                //        {
            //                //            terrainBlockIdentifier = source["__template"].Value<string>();
            //                //        }
            //                //        else
            //                //        {
            //                //            terrainBlockIdentifier = "_base_concrete";
            //                //        }

            //                //        var tbt = ItemTemplateManager.getTerrainBlockTemplate(TerrainBlockType.generateStringHash(terrainBlockIdentifier));
            //                //        Debug.Assert(tbt != null);
            //                //        var template = ItemTemplateManager.getTerrainBlockScratchGroupByTerrainBlockType(tbt);

            //                //        var instance = Instantiate<TerrainBlockScratchGroup>(template);
            //                //        instance.terrainBlockType_identifier = entry.Key;
            //                //        instance.id = GameRoot.generateStringHash64("tbsg_" + instance.terrainBlockType_identifier);
            //                //        instance.id32 = GameRoot.generateStringHash32("tbsg_" + instance.terrainBlockType_identifier);

            //                //        ItemTemplateManager.dict_terrainBlockScratchGroups[instance.id] = instance;

            //                //        instance.onLoad();
            //                //    }

            //                //    BepInExLoader.log.LogMessage(string.Format("Added {0} terrain block scratch groups.", BepInExLoader.patchDataTerrainAdditions != null ? BepInExLoader.patchDataTerrainAdditions.Count : 0));
            //                //}

            //                //if (!hasRun_terrainBlockTemplates && ItemTemplateManager.dict_terrainBlockTemplates != null && ItemTemplateManager.dict_terrainBlockTemplates.Count > 0)
            //                //{
            //                //    hasRun_terrainBlockTemplates = true;

            //                //    foreach (var entry in BepInExLoader.patchDataTerrainAdditions)
            //                //    {
            //                //        var source = (JObject)entry.Value;
            //                //        if (source == null) throw new System.Exception("Invalid terrain block:\r\n" + entry.Value.ToString());

            //                //        TerrainBlockType template = null;
            //                //        if (source.ContainsKey("__template"))
            //                //        {
            //                //            var templateIdentifier = source.Value<string>("__template");
            //                //            foreach (var templateEntry in ItemTemplateManager.dict_terrainBlockTemplates.Values)
            //                //            {
            //                //                if (templateEntry.identifier == templateIdentifier)
            //                //                {
            //                //                    template = templateEntry;
            //                //                    break;
            //                //                }
            //                //            }
            //                //            if (template == null)
            //                //            {
            //                //                log.LogError(string.Format("Template terrain block {0} not found!", templateIdentifier));
            //                //            }
            //                //        }

            //                //        if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Adding terrain block {0}", entry.Key));

            //                //        TerrainBlockType instance;
            //                //        if (template != null)
            //                //        {
            //                //            if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
            //                //            instance = Instantiate<TerrainBlockType>(template);
            //                //        }
            //                //        else
            //                //        {
            //                //            instance = ScriptableObject.CreateInstance<TerrainBlockType>();
            //                //            instance.surfaceOre_worldDecor_identifier = new string[0];
            //                //        }

            //                //        instance.identifier = entry.Key;
            //                //        instance.id = GameRoot.generateStringHash64("tbt_" + instance.identifier);
            //                //        instance.id32 = GameRoot.generateStringHash32("tbt_" + instance.identifier);

            //                //        if (source.ContainsKey("texture_abledo"))
            //                //        {
            //                //            instance.texture_abledo = (Texture2D)ResourceExt.FindTexture(source["texture_abledo"].Value<string>());
            //                //            source.Remove("texture_abledo");
            //                //        }
            //                //        if (source.ContainsKey("texture_side_abledo"))
            //                //        {
            //                //            instance.texture_side_abledo = (Texture2D)ResourceExt.FindTexture(source["texture_side_abledo"].Value<string>());
            //                //            source.Remove("texture_side_abledo");
            //                //        }
            //                //        if (source.ContainsKey("texture_bottom_abledo"))
            //                //        {
            //                //            instance.texture_bottom_abledo = (Texture2D)ResourceExt.FindTexture(source["texture_bottom_abledo"].Value<string>());
            //                //            source.Remove("texture_bottom_abledo");
            //                //        }
            //                //        if (source.ContainsKey("texture_height"))
            //                //        {
            //                //            instance.texture_height = (Texture2D)ResourceExt.FindTexture(source["texture_height"].Value<string>());
            //                //            source.Remove("texture_height");
            //                //        }
            //                //        if (source.ContainsKey("texture_side_height"))
            //                //        {
            //                //            instance.texture_side_height = (Texture2D)ResourceExt.FindTexture(source["texture_side_height"].Value<string>());
            //                //            source.Remove("texture_side_height");
            //                //        }
            //                //        if (source.ContainsKey("texture_side_mask"))
            //                //        {
            //                //            instance.texture_side_mask = (Texture2D)ResourceExt.FindTexture(source["texture_side_mask"].Value<string>());
            //                //            source.Remove("texture_side_mask");
            //                //        }
            //                //        JsonConvert.PopulateObject(source.ToString(), instance, serializerSettings);

            //                //        ItemTemplateManager.dict_terrainBlockTemplates[instance.id] = instance;

            //                //        instance.onLoad();
            //                //    }

            //                //    BepInExLoader.log.LogMessage(string.Format("Patched {0} terrain blocks and added {1} terrain blocks.", BepInExLoader.patchDataTerrainChanges != null ? BepInExLoader.patchDataTerrainChanges.Count : 0, BepInExLoader.patchDataTerrainAdditions != null ? BepInExLoader.patchDataTerrainAdditions.Count : 0));
            //                //}

            //                //if (!hasRun_biomeTemplates && ItemTemplateManager.dict_biomeTemplates != null && ItemTemplateManager.dict_biomeTemplates.Count > 0)
            //                //{
            //                //    hasRun_biomeTemplates = true;

            //                //    foreach (var entry in ItemTemplateManager.dict_biomeTemplates)
            //                //    {
            //                //        var biome = entry.Value;
            //                //        var path = Path.Combine(BepInExLoader.biomeDumpFolder, biome.identifier + ".json");
            //                //        if (BepInExLoader.forceDump.Value || !File.Exists(path))
            //                //        {
            //                //            File.WriteAllText(path, JsonConvert.SerializeObject(gatherDump<BiomeDump, BiomeTemplate>(biome), Formatting.Indented, serializerSettings));
            //                //        }

            //                //        if (BepInExLoader.patchDataBiomeChanges != null && BepInExLoader.patchDataBiomeChanges.ContainsKey(biome.identifier))
            //                //        {
            //                //            if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Patching biome {0}", biome.identifier));
            //                //            var changes = BepInExLoader.patchDataResearchChanges[biome.identifier] as JObject;
            //                //            JsonConvert.PopulateObject(changes.ToString(), biome, serializerSettings);
            //                //        }
            //                //    }
            //                //}
            //            }

            //            //public static void LoadAllBuildableObjectTemplatesInBuild(ref BuildableObjectTemplate[] __result)
            //            //{
            //            //    var result = new BuildableObjectTemplate[__result.Length + BepInExLoader.patchDataBuildingAdditions.Count];
            //            //    for (int i = 0; i < __result.Length; ++i) result[i] = __result[i];
            //            //    int index = __result.Length;
            //            //    foreach (var entry in patchDataBuildingAdditions)
            //            //    {
            //            //        var source = (JObject)entry.Value;
            //            //        if (source == null) throw new System.Exception("Invalid building:\r\n" + entry.Value.ToString());

            //            //        BuildableObjectTemplate template = null;
            //            //        if (source.ContainsKey("__template"))
            //            //        {
            //            //            var templateIdentifier = source.Value<string>("__template");
            //            //            for (int i = 0; i < __result.Length; ++i)
            //            //            {
            //            //                if (__result[i].identifier == templateIdentifier)
            //            //                {
            //            //                    template = __result[i];
            //            //                    break;
            //            //                }
            //            //            }
            //            //            if (template == null)
            //            //            {
            //            //                log.LogError(string.Format("Template building {0} not found!", templateIdentifier));
            //            //            }
            //            //        }

            //            //        if (verbose.Get()) log.Log(string.Format("Adding building {0}", entry.Key));

            //            //        BuildableObjectTemplate instance;
            //            //        if (template != null)
            //            //        {
            //            //            if (BepInExLoader.verbose.Value) log.LogInfo(string.Format("Using template {0}", template.identifier));
            //            //            instance = Instantiate(template);
            //            //        }
            //            //        else
            //            //        {
            //            //            instance = ScriptableObject.CreateInstance<BuildableObjectTemplate>();
            //            //        }

            //            //        instance.identifier = entry.Key;
            //            //        JsonConvert.PopulateObject(entry.Value.ToString(), instance, serializerSettings);
            //            //        result[index++] = instance;
            //            //    }

            //            //    BepInExLoader.log.LogMessage(string.Format("Patched {0} buildings and added {1} buildings.", BepInExLoader.patchDataBuildingChanges != null ? BepInExLoader.patchDataBuildingChanges.Count : 0, BepInExLoader.patchDataBuildingAdditions != null ? BepInExLoader.patchDataBuildingAdditions.Count : 0));

            //            //    __result = result;
            //            //}

            //            private static IEnumerable<CraftingRecipe> getRecipesForItem(string identifier)
            //            {
            //                foreach (var recipe in ItemTemplateManager.getCraftingRecipesByTag(GameRoot.generateStringHash64("character")))
            //                {
            //                    bool found = false;
            //                    foreach (var output in recipe.output_data)
            //                    {
            //                        if (output.identifier == identifier)
            //                        {
            //                            found = true;
            //                            break;
            //                        }
            //                    }
            //                    if (found)
            //                    {
            //                        yield return recipe;
            //                    }
            //                }
            //            }

            //            private static IEnumerable<CraftingRecipe> getCharacterSubRecipes(CraftingRecipe recipe)
            //            {
            //                return recipe.input_data.SelectMany(input => getRecipesForItem(input.identifier));//.Where(subRecipe => subRecipe != null && subRecipe.tags.Contains("character"));
            //            }

            //            private class RecipeNode
            //            {
            //                public CraftingRecipe recipe;
            //                public RecipeNode previous;

            //                public RecipeNode(CraftingRecipe recipe, RecipeNode previous)
            //                {
            //                    this.recipe = recipe;
            //                    this.previous = previous;
            //                }
            //            }

            //            private static void checkForRecipeCycles(CraftingRecipe rootRecipe)
            //            {
            //                var recipeQueue = new Queue<RecipeNode>();
            //                recipeQueue.Enqueue(new RecipeNode(rootRecipe, null));
            //                while (recipeQueue.Count > 0)
            //                {
            //                    var recipePath = recipeQueue.Dequeue();
            //                    foreach (var subRecipe in getCharacterSubRecipes(recipePath.recipe))
            //                    {
            //                        for (var node = recipePath; node != null; node = node.previous)
            //                        {
            //                            if (node.recipe == subRecipe)
            //                            {
            //                                var nodeNames = new List<string>
            //                            {
            //                                subRecipe.identifier
            //                            };
            //                                for (var mnode = recipePath; mnode != node; mnode = mnode.previous) nodeNames.Add(mnode.recipe.identifier);
            //                                nodeNames.Add(node.recipe.identifier);
            //                                log.LogError("Cyclic recipe detected! This will crash!");
            //                                log.LogError(string.Join(" ← ", nodeNames));
            //                                return;
            //                            }
            //                        }
            //                        recipeQueue.Enqueue(new RecipeNode(subRecipe, recipePath));
            //                    }
            //                }
            //            }

            internal static Regex buildPatternRegex(string pattern)
            {
                return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", @"(?:.*?)") + "$");
            }
        }

        private static readonly Dictionary<int, int> iconSizes = new Dictionary<int, int>() {
                        { 0, 1024 },
                        { 512, 512 },
                        { 256, 256 },
                        { 128, 128 },
                        { 96, 96 },
                        { 64, 64 }
                    };

        //public static object invokeValue(System.Type type, JToken token)
        //{
        //    //log.LogMessage(string.Format("{0} {1} '{2}'", type.FullName, token.GetType().FullName, token.ToString()));
        //    var property = token.GetType().GetProperty("Value");
        //    if (property != null) return property.GetGetMethod().Invoke(token, new object[] { });
        //    var method = token.GetType().GetMethod("Value");
        //    if (method != null)
        //    {
        //        if (method.IsGenericMethod) return method.MakeGenericMethod(type).Invoke(token, new object[] { });
        //        return method.Invoke(token, new object[] { });
        //    }
        //    throw new System.Exception(string.Format("Failed to get value of token '{0}'", token.ToString()));
        //}

        //public static object invokeValue(System.Type type, JObject token, string label)
        //{
        //    var property = token.GetType().GetProperty("Value");
        //    if (property != null) return property.GetGetMethod().Invoke(token, new object[] { label });
        //    var method = token.GetType().GetMethod("Value");
        //    if (method != null)
        //    {
        //        if (method.IsGenericMethod) return method.MakeGenericMethod(type).Invoke(token, new object[] { label });
        //        return method.Invoke(token, new object[] { label });
        //    }
        //    throw new System.Exception(string.Format("Failed to get value of token '{0}'", token.ToString()));
        //}

        public static D gatherDump<D, T>(T template) where D : new()
        {
            var dump = (System.Object)new D();
            foreach (var field in typeof(D).GetFields())
            {
                var templateProperty = template.GetType().GetField(field.Name);
                if (templateProperty != null)
                {
                    if (templateProperty.FieldType == typeof(Texture2D) && field.FieldType == typeof(Texture2DProxy))
                    {
                        field.SetValue(dump, new Texture2DProxy((Texture2D)templateProperty.GetValue(template)));
                    }
                    else if (templateProperty.FieldType == typeof(Vector3Int) && field.FieldType == typeof(Vector3IntProxy))
                    {
                        field.SetValue(dump, new Vector3IntProxy((Vector3Int)templateProperty.GetValue(template)));
                    }
                    else if (templateProperty.FieldType == typeof(List<Vector3Int>) && field.FieldType == typeof(List<Vector3IntProxy>))
                    {
                        var templateValues = (List<Vector3Int>)templateProperty.GetValue(template);
                        var dumpValues = new List<Vector3IntProxy>();
                        foreach (var value in templateValues) dumpValues.Add(new Vector3IntProxy(value));
                        field.SetValue(dump, dumpValues);
                    }
                    else if (templateProperty.FieldType == typeof(Vector3Int[]) && field.FieldType == typeof(Vector3IntProxy[]))
                    {
                        var templateValues = (Vector3Int[])templateProperty.GetValue(template);
                        var dumpValues = new List<Vector3IntProxy>();
                        foreach (var value in templateValues) dumpValues.Add(new Vector3IntProxy(value));
                        field.SetValue(dump, dumpValues.ToArray());
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
            Graphics.CopyTexture(inputTexture, outputTexture);
            return outputTexture;
        }

        internal static Sprite getIcon(string name)
        {
            return ResourceDB.getIcon(name, 256);
        }

#pragma warning disable CS0649
        private struct ItemDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public bool includeInDemo;
            public bool includeInCreativeMode;
            public string creativeModeCategory_str;
            public string entitlementIdentifier;
            public string icon_identifier;
            public uint stackSize;
            public bool isHiddenItem;
            public ItemTemplate.ItemTemplateFlags flags;
            public ItemTemplate.ItemTemplateToggleableModeTypes toggleableModeType;
            public ItemTemplate.ItemMode[] toggleableModes;
            public ItemTemplate.GenericItemActionButtons[] genericItemActionButtons;
            public bool skipForRunningIdxGeneration;
            public ItemTemplate.ItemDestroyFlags itemDestroyFlags;
            public Vector3 meshScale;
            public Quaternion meshRotation;
            public string buildableObjectIdentifer;
            public ItemTemplate.HandheldSubType handheldSubType;
            public bool handheld_miner_shakeRightArmOnUse;
            public string handheld_defaultPowerPoleItemTemplate_str;
            public bool supportsFocusMode;
            public float miningTimeReductionInSec;
            public float miningRange;
            public int explosionRadius;
            public string burnable_fuelValueKJ_str;
            public string burnable_residualItemTemplate_str;
            public uint burnable_residualItemTemplate_count;
            public string salesItem_currencyIdentifier;
            public uint salesItem_currencyAmount;
            public long salesItem_skyPlatformBaseCapacity;
            public long salesItem_sortOrderASC;
            public string salesItem_hiddenByResearchIdentifier;
            public long salesCurrency_skyPlatformBaseCapacity;
            public long salesCurrency_sortOrderASC;
            public string salesCurrency_hiddenByResearchIdentifier;
            public int sciencePack_researchFrameSortingOrder;
            public Color sciencePack_color;
            public int railminer_slotLength;
            public int railminer_speedInSlotsPerTick;
            public int railminer_minecartSpeedInSlotsPerTick;
            public string[] railMiner_terrainTargetList_str;
            public bool isMinecartItem;
            public string trainVehicle_templateIdentifier;
            public string alStarter_alotIdentifier;
            public string alStarter_sellItemTemplateIdentifier;
            public uint warehouse_stackSize;
            public string rubble_recyclingResultIdentifier;
            public ItemTemplate.AutoProducerAction[] autoProducerRecipes;
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
            public string fuel_fuelValueKJPerL_str;
            public string fuel_residualTemplate_identifier;
            public float fuel_residualAmountPerL;
            public ElementTemplate.ElementTemplateFuelFlags fuel_flags;
            public float fuel_minViableVolumeL;
        }

        private struct RecipeDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public string entitlementIdentifier;
            public string icon_identifier;
            public string category_identifier;
            public string rowGroup_identifier;
            public bool hideInDemo;
            public bool isHiddenInCharacterCraftingFrame;
            public bool isHiddenByNarrativeTrigger;
            public bool isNeverUnseenRecipe;
            public string narrativeTrigger;
            public CraftingRecipe.CraftingRecipeItemInput[] input_data;
            public CraftingRecipe.CraftingRecipeItemInput[] output_data;
            public CraftingRecipe.CraftingRecipeElementalInput[] inputElemental_data;
            public CraftingRecipe.CraftingRecipeElementalInput[] outputElemental_data;
            public string relatedItemTemplateIdentifier;
            public int sortingOrderWithinRowGroup;
            public int timeMs;
            public string[] tags;
            public bool forceShowOutputsAtTooltips;
            public int recipePriority;
            public string extraInfoTooltipText;
        }

        private struct TerrainBlockDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public TerrainBlockType.TerrainTypeFlags flags;
            public float movementSpeedModifier;
            public Color shatterColor;
            public Color scanColor;
            public bool destructible;
            public string yieldItemOnDig;
            public float miningTimeInSec;
            public int requiredMiningHardnessLevel;
            //public MovementSoundPack movementSoundPack;
            public Color mapColor;
            public TerrainBlockType.DecorFlags decorFlags;
            public bool hasTrim;
            public bool useTextureTiling;
            public bool useLayerNoise;
            public bool geologicalScanner_showOnScan;
            public bool unlockedForOreScannerByDefault;
            public string ore_yield_id;
            public int maxHeight;
            public int minHeight;
            public uint oreSpawn_chancePerChunk_ground;
            public uint oreSpawn_chancePerChunk_surface;
            public TerrainBlockType.OreSpawnFlags oreSpawnFlags;
            public Vector3IntProxy minSize;
            public Vector3IntProxy maxSize;
            public int sizeRng;
            public int averageYield;
            public string yieldVarietyPercent_str;
            public int depthIncreasePerTile;
            public WorldDecorSpawnInfo[] surfaceOre_worldDecor;
            public TerrainBlockType.FixedOrePatchData[] fixedOrePatchDataArray;
            public int rmd_ticksPerItem;
            public string rmd_miningYield;
            public int rmd_totalYield;
            public string oreVeinMineable_yieldItem_identifier;
            public int oreVeinMineable_averageYield;
            public int oreVeinMineable_yieldVariety;
            public Texture2DProxy texture_abledo;
            public Texture2DProxy texture_metalHeightSmoothHeight;
            public float blendSmoothness;
            public bool hasSideTextures;
            public Texture2DProxy texture_side_abledo;
            public Texture2DProxy texture_side_metalSmoothHeight;
            public Texture2DProxy texture_side_mask;
            public Texture2DProxy texture_side_mask_metalSmoothHeight;
            public bool hasBottomTextures;
            public Texture2DProxy texture_bottom_abledo;
            public Texture2DProxy texture_bottom_metalSmoothHeight;
        }

        public struct BuildableObjectDump
        {
            public string modIdentifier;
            public string identifier;
            public BuildableObjectTemplate.BuildableObjectType type;
            public BuildableObjectTemplate.SimulationType simulationType;
            public BuildableObjectTemplate.SimulationSleepFlags simulationSleepFlags;
            public bool simTypeSleep_initial;
            public bool isSuperBuilding;
            public Vector3IntProxy size;
            public bool validateTerrainTileColliders;
            public BuildableObjectTemplate.DragBuildType dragBuildType;
            public float dragModeOrientationSlope_planeAngle;
            public Quaternion dragModeOrientationSlope_planeAngleQuaternion;
            public int dragModeOrientationSlope_yOrientationModifier;
            public int dragModeOrientationSlope_yOffsetPerInstance;
            public bool dragModeOrientationSlope_allowSideways;
            public BuildableObjectTemplate.DragAxis dragModeLine_dragAxis;
            public float dragModeLine_nonUnlockedYCorrection;
            public BuildableObjectTemplate.DragAxis dragModePlane_dragAxis01;
            public BuildableObjectTemplate.DragAxis dragModePlane_dragAxis02;
            public BuildableObjectTemplate.DragMode[] dragModes;
            public BuildableObjectTemplate.CustomSnapMode customSnapMode;
            public float demolitionTimeSec;
            public bool canBeDestroyedByDynamite;
            public string conversionGroup_str;
            public bool isVisibleOnMap;
            public byte mapColorPriority;
            public bool skipForRunningIdxGeneration;
            //public AudioClip audioClip_customBuildSound;
            public bool hasNameOverride;
            public string nameOverride;
            public bool canBeCopiedByTablet;
            public Color defaultObjectColor;
            public BuildableObjectTemplate.AnalyticsTrackerId analyticsTrackerId;
            public bool hasToBeOnFoundation;
            public bool floorShouldOutlineBuilding;
            public BuildableObjectTemplate.FoundationConnectorType foundationConnection;
            public int loaderLevel;
            public bool disableLoaders;
            public bool hasPipeLoaderSupport;
            public v3i[] blockedLoaderPositions;
            public bool rotationAllowed;
            public bool canBeRotatedAroundXAxis;
            public BuildableObjectTemplate.AdditionalAABB3D[] additionalAABBs_input;
            public bool isModularBuilding;
            public BuildableObjectTemplate.ModularBuildingType modularBuildingType;
            public string modularBuildingModule_descriptionName;
            public uint modularBuildingModule_amountItemCost;
            public string modularBuildingModule_unlockedByResearchTemplateIdentifier;
            public BuildableObjectTemplate.ModularBuildingConnectionNode[] modularBuildingConnectionNodes;
            public BuildableObjectTemplate.ModularBuildingModuleLimit[] modularBuildingLimits;
            public bool modularBuilding_forceRequirePCM;
            public Vector3IntProxy modularBuildingLocalSearchAnchor;
            public BuildableObjectTemplate.ModularEntityItemCosts[] modularBuildingItemCost;
            public BuildableObjectTemplate.ModularEntityItemCosts[] modularBuildingRubble;
            public bool modularBuilding_hasNoEnabledState;
            public BuildableObjectTemplate.PowerComponentType powerComponentType;
            public BuildableObjectTemplate.BuildableObjectPowerSubType powerSubType;
            public string energyConsumptionKW_str;
            public int powerProducer_drawPriority;
            public bool spp_showToggleButton;
            public bool spp_showPowerButton;
            public bool spp_disablePowerCheck;
            public bool spp_disableGridCheck;
            public ScreenPanelProfile_Native.SPP_GridCheckType spp_gridCheckType;
            public bool hasEnergyGridConnection;
            public bool hasPoleGridConnection;
            public int poleGrid_connectionRange;
            public Vector3IntProxy poleGrid_connectorOffset;
            public int poleGrid_maxConnections;
            public int poleGrid_reservedConnections;
            public BuildableObjectTemplate.PoleGridTypes poleGridType;
            public BuildableObjectTemplate.PoleGridTypes poleGridConnectionMatrix;
            public bool hasFuelManagerSolid;
            public bool hasFuelManagerElemental;
            public ElementTemplate.ElementTemplateFuelFlags fme_fuelFlags;
            public string fme_lockedElementTemplateIdentifier;
            public bool hasLightSource;
            public BuildableObjectTemplate.LightEmitter[] lightEmitters;
            public bool spawnFlyingDebrisWhenExploding;
            public BuildableObjectTemplate.ItemBufferUIRow[] itemBufferSlotMap;
            public bool hasRailings;
            public WalkwayMeshEntry walkway_mesh_noRailings;
            public WalkwayMeshEntry walkway_mesh_xPos;
            public WalkwayMeshEntry walkway_mesh_xNeg;
            public WalkwayMeshEntry walkway_mesh_zPos;
            public WalkwayMeshEntry walkway_mesh_zNeg;
            public WalkwayMeshEntry walkway_mesh_xPos_xNeg;
            public WalkwayMeshEntry walkway_mesh_z_Pos_zNeg;
            public WalkwayMeshEntry walkway_mesh_xPos_zPos;
            public WalkwayMeshEntry walkway_mesh_xPos_zNeg;
            public WalkwayMeshEntry walkway_mesh_xNeg_zPos;
            public WalkwayMeshEntry walkway_mesh_xNeg_zNeg;
            public WalkwayMeshEntry walkway_mesh_xPos_zNeg_xNeg;
            public WalkwayMeshEntry walkway_mesh_zPos_xPos_zNeg;
            public WalkwayMeshEntry walkway_mesh_xPos_zPos_xNeg;
            public WalkwayMeshEntry walkway_mesh_zPos_xNeg_zNeg;
            public WalkwayMeshEntry walkway_mesh_allRailings;
            public bool hasIntraBuildingWalkways;
            public BuildableObjectTemplate.IntraBuildingWalkwayData[] intraBuildingWalkwayData;
            public bool hasAdjacentWalkwayOverrides;
            public BuildableObjectTemplate.IntraBuildingWalkwayData[] adjacentWalkwayOverridesPos;
            public bool hasConveyorConnectionManager;
            public BuildableObjectTemplate.ModularBuildingConveyorConnectionData[] ccm_connectionData;
            public bool hasStrutManager;
            public BuildableObjectTemplate.AdditionalAABB3D[] sm_strutPillars;
            public string sm_strutPillarBotIdentifier;
            public bool hasFluidBoxManager;
            public bool fbm_sendUpdateEventsForRegularFluidBoxes;
            public BuildableObjectTemplate.FluidBoxData[] fbm_fluidBoxes;
            public BuildableObjectTemplate.IOFluidBoxData[] fbm_ioFluidBoxes;
            public int droneMiner_oreSearchRadius;
            public int droneMiner_itemCapacityPerDrone;
            public string droneMiner_miningSpeed_str;
            public string droneMiner_droneCharge_str;
            public int droneMiner_droneCount;
            public Vector3IntProxy droneMiner_dockPositionInside;
            public Vector3IntProxy droneMiner_dockPositionOutside;
            //public AudioClip droneMiner_audioClip_droneHover;
            //public AudioClip droneMiner_audioClip_droneMining;
            public List<Vector3IntProxy> droneMiner_list_localBlocksAllowedToBeTraversed;
            public float dissolver_elementalConsumptionPerSecond;
            public string dissolver_solidsPerSecond_str;
            public bool dissolver_isElemental;
            public bool droneTransport_isStartStation;
            public string droneTransport_energyConsumptionPerSecondFlightTime_kj_str;
            public string droneTransport_energyRechargeRatePerSecond_kj_str;
            public float droneTransport_travelSpeed_mPerSec;
            public float droneTransport_rotationSpeedDegreePerSec;
            public float droneTransport_droneYOffset;
            public Vector3IntProxy loader_localBeltOffset;
            public int loader_ticksPerAction;
            public string loader_idlePowerConsumption_kjPerS_str;
            public bool loader_isFilter;
            public string pipeLoader_maxThroughputPerTickInLiter_str;
            public string pipeLoader_idlePowerConsumption_kjPerS_str;
            public List<string> pipeLoader_allowedPipeGroupIdentifiers;
            public v3i pipeLoader_fluidBoxPositionOrigin_localOffset;
            public v3i pipeLoader_fluidBoxPositionTarget_localOffset;
            public v3i pipeLoader_buildingPosition_localOffset;
            public uint storage_slotSize;
            public BuildableObjectTemplate.ProducerRecipeType producerRecipeType;
            public string producer_recipeTimeModifier_str;
            //public AudioClip producer_audioClip_active;
            public string producer_recipeType_fixed;
            public string[] producer_recipeType_tags;
            public string autoProducer_recipeTimeModifier_str;
            //public AudioClip autoProducer_audioClip_active;
            public string autoProducer_recipeType_tag;
            //public AudioClip producer_audioClip_customItemFinishSound;
            public bool conveyor_isSlope;
            public string conveyor_slopePartner_str;
            public int conveyor_speed_slotsPerTick;
            public Texture2DProxy buildingPart_texture_albedo;
            public bool buildingPart_hasSideTextures;
            public Texture2DProxy buildingPart_texture_side_albedo;
            public bool buildingPart_hasBottomTextures;
            public Texture2DProxy buildingPart_texture_bottom_albedo;
            public string battery_capacityKJ_str;
            public int shippingPad_inventorySlotCount;
            public string shippingPad_requiredChargeKJ_str;
            public string shippingPad_chargePerSecondKJ_str;
            public int shippingPad_timeInSpaceSec;
            public bool pipe_hasVisualUpdate;
            public uint pipe_visualUpdateFluidBoxTemplateIdx;
            public string transformer_transmissionRate_kjPerS_str;
            //public AudioClip transformer_audioClip_active;
            public string lvgGenerator_transmissionRate_kjPerS_str;
            public string lvgGenerator_lockedItemTemplate_identifier;
            //public AudioClip lvgGenerator_audioClip_active;
            public string lvgGenerator_efficiency_str;
            public string solarPanel_outputMax_str;
            public string solarPanel_outputMin_str;
            public bool solarPanel_rotatingPart;
            public BuildableObjectTemplate.WorldDecorMiningYield[] worldDecor_miningYield;
            public float worldDecor_miningTimeSec;
            //public AudioClip worldDecor_audioClip_afterHarvesting;
            public bool worldDecor_isDebris;
            public BuildableObjectTemplate.WorldDecorSpecialDrop[] worldDecor_specialDrops;
            public Color worldDecor_scratchColor;
            public bool worldDecor_canGrow;
            public int worldDecor_growTimeSec;
            public float worldDecor_growStartScale;
            public string worldDecor_plantSeedIdentifier;
            public string[] worldDecor_allowedGrowableTerrainBlocks;
            public BuildableObjectTemplate.SuperBuildingLevel[] superBuilding_levels;
            public string[] superBuilding_researchUnlocks_str;
            public Vector3IntProxy[] superBuilding_allowedLoaderPositions;
            public int superBuilding_loaderIndicator_rotY;
            public bool superBuilding_refundOnDemolish;
            public string researchLab_sciencePack_str;
            public string[] researchEntity_scienceItems;
            public string[] researchEntity_scienceItemsVisibilityResearch;
            public string researchEntity_powerDemand_kjPerSecond_data;
            public string pumpjack_amountPerSec_str;
            public Vector3IntProxy pumpjack_drillOffset;
            public int pumpjack_maxDrillDepth;
            //public AudioClip pumpjack_audioClip_active;
            public string burnerGenerator_powerGenertaionRate_kjPerS_str;
            public string burnerGenerator_efficiency_str;
            public string terrainBlock_tbtIdentifier;
            public Vector3IntProxy minecartDepot_connectionPoint;
            public BuildingManager.BuildOrientation minecartDepot_connectionSearchDirection;
            public uint minecartDepot_miningInventorySlots;
            public uint minecartDepot_cartInterval_sec;
            public string minecartDepot_autobuildTrackTemplate_str;
            public Vector3IntProxy[] minecartTracks_connectionPoints;
            public BuildingManager.BuildOrientation[] minecartTracks_connectionSearchDirection;
            public int minecartTracks_slotLength;
            public int freightContainer_speedPerTick;
            public long freightElevator_tierID;
            public string elevatorStation_structureBOT_str;
            //public AudioClip elevatorStation_audioClip_cabinMoving;
            //public AudioClip elevatorStation_audioClip_doorsOpening;
            //public AudioClip elevatorStation_audioClip_doorsClosing;
            //public AudioClip elevatorStation_audioClip_bell;
            public float door_secondsToOpen;
            //public AudioClip door_audioClip_openClose_trigger;
            //public AudioClip door_audioClip_openClose_loop;
            //public AudioClip geologicalScanner_audioClip_active;
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
            public string resourceConverter_powerConsumption_kjPerSec;
            public CraftingRecipe.CraftingRecipeElementalInput[] resourceConverter_input_elemental;
            public CraftingRecipe.CraftingRecipeElementalInput[] resourceConverter_output_elemental;
            public BuildableObjectTemplate.ResourceConverterModuleSpeedBonus[] resourceConverter_speedBonusModules;
            public bool resourceConverter_hasAdjacencyBonus;
            public string resourceConverter_powerDecreasePerAdjacentResourceConverter;
            public byte resourceConverter_adjacencyBonusAxis;
            //public AudioClip resourceConverter_audioClip_active;
            public v3i frackingTower_drillOffset;
            public uint frackingTower_maxDrillLength;
            public uint frackingTower_powerConsumptionPercentagePerOptionalTowerModule;
            public float frackingTower_fluidThroughputPerTowerPerSecond;
            public string frackingTower_towerModuleBOT_identifier;
            public float modularFluidTank_baseFluidCapacity;
            public string modularFluidTank_towerModuleBOT_identifier;
            public string modularFluidTank_topModuleBOT_identifier;
            public float modularFluidTank_fluidCapacityPerTower;
            public int trainLoadingStation_itemType;
            public Vector3IntProxy trainLoadingStation_trackSearchOffset01;
            public Vector3IntProxy trainLoadingStation_trackSearchOffset02;
            public BuildableObjectTemplate.TrainLoadingStationCompatibleTrackTemplate[] trainLoadingStation_compatibleTrackTemplates;
            public uint al_start_slotWidth;
            public uint al_start_speedInSlotsPerTick;
            public Vector3IntProxy al_start_localOffset_outputOrigin;
            public Vector3IntProxy al_start_localOffset_outputTarget;
            public string alStart_requiredPowerPerAction_kj_str;
            public SplineDataContainer al_start_splineDataContainer;
            public uint al_rail_slotWidth;
            public uint al_rail_speedInSlotsPerTick;
            public Vector3IntProxy al_rail_localOffset_inputOrigin;
            public Vector3IntProxy al_rail_localOffset_inputTarget;
            public Vector3IntProxy al_rail_localOffset_outputOrigin;
            public Vector3IntProxy al_rail_localOffset_outputTarget;
            public SplineDataContainer al_rail_splineDataContainer;
            public int al_rail_csm_outputOrientationModifier;
            public BuildableObjectTemplate.AL_ProducerMachineType al_producer_machineType;
            public uint al_producer_slotWidth_input;
            public uint al_producer_slotWidth_output;
            public uint al_producer_speedInSlotsPerTick;
            public Vector3IntProxy al_producer_localOffset_inputOrigin;
            public Vector3IntProxy al_producer_localOffset_inputTarget;
            public Vector3IntProxy al_producer_localOffset_outputOrigin;
            public Vector3IntProxy al_producer_localOffset_outputTarget;
            public SplineDataContainer al_producer_splineDataContainer_input;
            public SplineDataContainer al_producer_splineDataContainer_output;
            public uint al_endConsumer_itemCapacity;
            public BuildableObjectTemplate.InteractionPointData[] al_endConsumer_interactionPoints;
            public uint al_endConsumer_slotWidth;
            public uint al_endConsumer_speedInSlotsPerTick;
            public Vector3IntProxy al_endConsumer_localOffset_inputOrigin;
            public Vector3IntProxy al_endConsumer_localOffset_inputTarget;
            public SplineDataContainer al_endConsumer_splineDataContainer;
            public uint al_merger_speedInSlotsPerTick;
            public uint al_merger_slotWidth_input01;
            public uint al_merger_slotWidth_input02;
            public uint al_merger_slotWidth_input03;
            public uint al_merger_slotWidth_input01_internal;
            public uint al_merger_slotWidth_input02_internal;
            public uint al_merger_slotWidth_input03_internal;
            public uint al_merger_slotWidth_output;
            public Vector3IntProxy al_merger_localOffset_inputOrigin01;
            public Vector3IntProxy al_merger_localOffset_inputTarget01;
            public Vector3IntProxy al_merger_localOffset_inputOrigin02;
            public Vector3IntProxy al_merger_localOffset_inputTarget02;
            public Vector3IntProxy al_merger_localOffset_inputOrigin03;
            public Vector3IntProxy al_merger_localOffset_inputTarget03;
            public Vector3IntProxy al_merger_localOffset_outputOrigin;
            public Vector3IntProxy al_merger_localOffset_outputTarget;
            public SplineDataContainer al_merger_splineDataContainer_input01;
            public SplineDataContainer al_merger_splineDataContainer_input02;
            public SplineDataContainer al_merger_splineDataContainer_input03;
            public SplineDataContainer al_merger_splineDataContainer_input01_internal;
            public SplineDataContainer al_merger_splineDataContainer_input02_internal;
            public SplineDataContainer al_merger_splineDataContainer_input03_internal;
            public SplineDataContainer al_merger_splineDataContainer_output;
            public uint al_splitter_speedInSlotsPerTick;
            public uint al_splitter_slotWidth_output01;
            public uint al_splitter_slotWidth_output02;
            public uint al_splitter_slotWidth_output03;
            public uint al_splitter_slotWidth_input;
            public Vector3IntProxy al_splitter_localOffset_outputOrigin01;
            public Vector3IntProxy al_splitter_localOffset_outputTarget01;
            public Vector3IntProxy al_splitter_localOffset_outputOrigin02;
            public Vector3IntProxy al_splitter_localOffset_outputTarget02;
            public Vector3IntProxy al_splitter_localOffset_outputOrigin03;
            public Vector3IntProxy al_splitter_localOffset_outputTarget03;
            public Vector3IntProxy al_splitter_localOffset_inputOrigin;
            public Vector3IntProxy al_splitter_localOffset_inputTarget;
            public SplineDataContainer al_splitter_splineDataContainer_output01;
            public SplineDataContainer al_splitter_splineDataContainer_output02;
            public SplineDataContainer al_splitter_splineDataContainer_output03;
            public SplineDataContainer al_splitter_splineDataContainer_input;
            public BuildingManager.BuildOrientation[] escalator_localForwardDirection;
            public EscalatorGO.EscalatorType escalator_type;
            public string[] baseStation_craftingRecipeIdentifier;
            public uint oreVeinMiner_railMinerSlotLength;
            public uint oreVeinMiner_railMinerDrillSlotPadding;
            public uint oreVeinMiner_railMinerMoveSpeedSlotsPerTick;
            public string oreVeinMiner_ticksPerOre_str;
            public uint oreVeinMiner_minecartInventorySlotSize;
            public uint oreVeinMiner_minecartMoveSpeedSlotsPerTick;
            public uint oreVeinMiner_minecartSpawnOffset_toDepot;
            public uint oreVeinMiner_minecartSpawnOffset_toMiner;
            public uint oreVeinMiner_minecartDespawnDeadzone_toDepot;
            public uint oreVeinMiner_minecartDespawnDeadzone_toMiner;
            public float oreVeinMiner_minecartRenderingOffsetToDepot;
            public float oreVeinMiner_minecartRenderingOffsetToMiner;
            public uint oreVeinMiner_powerConsumptionBase_kjPerSec;
            public uint oreVeinMiner_powerConsumptionMining_kjPerSec;
            public string boiler_elementTemplateIdentifier_source;
            public string boiler_elementTemplateIdentifier_output;
            public float boiler_consumptionPerSecond;
            public float boiler_outputPerSecond;
            public string constructionDronePort_itemContributionPerSecond_str;
            public float constructionDronePort_droneYOffset;
            public float constructionDronePort_droneTravelHeight;
            public float constructionDronePort_droneTravelSpeed_mPerS;
            public float constructionDronePort_droneTravelRotation_degPerS;
            public string constructionDronePort_energyBuffer_kj_str;
            public string constructionDronePort_energyConsumptionPerItem_kj_str;
            public string constructionDronePort_energyRechargeRate_kjPerS_str;
            public v3i[] transportDronePort_dronePositions;
            public uint transportDronePort_droneInventoryCapacity;
            public float transportDronePort_droneTravelHeight;
            public float transportDronePort_droneTravelSpeed_mPerS;
            public float transportDronePort_droneTravelRotation_degPerS;
            public BuildableObjectTemplate.InteractionPointData[] constructionWarehouse_interactionPoints;
            public uint constructionWarehouse_recyclingSpeedItemsPerSecond;
            public string constructionWarehouse_powerDemandRecycling_kjPerS_str;
            public BuildableObjectTemplate.GreenhouseInput[] greenhouse_arrayInputs;
            public uint greenhouse_doubleOutputPercentChance;
            public uint greenhouse_craftingTimeMs;
            public BuildableObjectTemplate.InteractionPointData[] salesItemWarehouse_interactionPoints;
            public uint salesItemWarehouse_itemCapacity;
            public BuildableObjectTemplate.InteractionPointData[] salesCurrencyWarehouse_interactionPoints;
            public uint salesCurrencyWarehouse_itemCapacity;
            public string emergencyBeacon_yieldItemTemplateIdentifier;
            public string buddyKennel_critterTemplateIdentifier;
            public v3i buddyKennel_critterPos_localOffset;
            public uint scanningEntity_scanRange;
            public uint scanningEntity_secondsPerChunk;
            public uint scanningEntity_scanRangeAddPerModule;
            public uint scanningEntity_additionalScanSpeedPercentagePerModule;
            public string scanningEntity_speedModuleIdentifier;
            public string scanningEntity_distanceModuleIdentifier;
            public uint scanningEntity_additionalPowerConsumptionPercentagePerOptionalModule;
        }

        public struct ResearchDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public string icon_identifier;
            public bool includeInBuild;
            public bool includeInDemo;
            public string entitlementIdentifier;
            public ResearchTemplate.ResearchTemplateFlags flags;
            public bool isInternal;
            public string internalResearchUnlockDescription;
            public string manualEndlessResearchTemplate_str;
            public string description;
            public string researchContext;
            public uint secondsPerScienceItem;
            public ResearchTemplate.ResearchTemplateItemInput[] input_data;
            public List<string> list_researchDependencies_str;
            public List<string> list_craftingUnlocks_str;
            public List<string> list_blastFurnaceModes_str;
            public List<string> list_alots_str;
            public List<string> list_oreScannerUnlocks_str;
            public string mapScanner_ore_identifier;
            public string mapScanner_reservoir_identifier;
            public int inventorySize_additionalInventorySlots;
            public int endlessResearch_amountOfManualEndlessResearches;
            public string characterCraftingSpeed_additionalDecrementPercentage_str;
            public string miningDrillSpeed_miningTimeMultiplier_str;
            public int miningHardness_unlockedLevel;
            public string jetpackSpeed_speedIncreasmentPercent_str;
        }

        public struct BiomeDump
        {
            public string modIdentifier;
            public string identifier;
            public string name;
            public Color32 gridColor;
            public bool isHeightBased;
            public int lowestPossibleHeight;
            public string surfaceBlock_identifier;
            public string groundBlock_identifier;
            public EnvironmentEffectSpawnInfo[] effects;
            public uint effectChancePerBlockPercent;
            public uint temperature_min;
            public uint precipitation_min;
            public EnvironmentEvent environmentEvent;
            public BiomeLayerTemplate biomeLayer;
        }

        public struct Vector3IntProxy
        {
            public int x, y, z;

            public Vector3IntProxy(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Vector3IntProxy(Vector3Int vector)
            {
                x = vector.x;
                y = vector.y;
                z = vector.z;
            }
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

    public static class ResourceExt
    {
        static readonly Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        static Texture2D[] allTextures1 = null;
        static Texture2D[] allTextures2 = null;

        public static void RegisterTexture(string name, Texture2D texture)
        {
            loadedTextures[name] = texture;
        }

        public static Texture2D FindTexture(string name)
        {
            var tweakPath = Path.Combine(Plugin.texturesFolder, name + ".png");
            if (loadedTextures.TryGetValue(name, out Texture2D result))
            {
                return result;
            }
            else if (File.Exists(tweakPath))
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                var tempTexture = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
                tempTexture.LoadImage(File.ReadAllBytes(tweakPath), false);
                var texture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGB24, true, true);
                texture.SetPixels(tempTexture.GetPixels());
                texture.Apply(true);
                texture.Compress(false);
                texture.Apply();
                watch.Stop();
                if (Plugin.verbose.Get()) Plugin.log.LogFormat("Loading texture '{0}' from '{1}' took {2}ms", name, tweakPath, watch.ElapsedMilliseconds);
                loadedTextures.Add(name, texture);
                GameObject.Destroy(tempTexture);
                return texture;
            }
            else
            {
                if (Plugin.verbose.Get()) Plugin.log.LogFormat("Searching for texture '{0}'", name);

                if (allTextures1 == null) allTextures1 = Resources.FindObjectsOfTypeAll<Texture2D>();
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
                Plugin.log.LogError("Could not find texture: " + name);
                return null;
            }
        }
    }

    public static class ReaderExtensions {
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

    public static class VariantExtensions
    {
        public static Variant Merge(this Variant self, Variant other)
        {
            if (self == null) return other;
            if (other == null) return self;

            if (self is ProxyObject selfObject && other is ProxyObject otherObject)
            {
                foreach (var key in otherObject.Keys)
                {
                    selfObject[key] = selfObject[key].Merge(otherObject[key]);
                }

                return self;
            }

            return other;
        }
    }
}
