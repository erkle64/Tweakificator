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
using System.Reflection;
using System.Linq;

namespace Tweakificator
{
    [UnfoundryMod(GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "Tweakificator",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "2.0.6";

        public static LogSource log;

        public static TypedConfigEntry<bool> dumpTemplates;
        public static TypedConfigEntry<bool> forceDump;
        public static TypedConfigEntry<bool> dumpIcons;
        public static TypedConfigEntry<int> iconResolution;
        public static TypedConfigEntry<bool> verbose;

        public static string tweakificatorFolder;
        public static string dumpFolder;
        public static string itemsDumpFolder;
        public static string elementsDumpFolder;
        public static string recipesDumpFolder;
        public static string recipeCategoriesDumpFolder;
        public static string recipeCategoryRowsDumpFolder;
        public static string terrainBlocksDumpFolder;
        public static string buildingsDumpFolder;
        public static string researchDumpFolder;
        public static string biomeDumpFolder;
        public static string blastFurnaceModeDumpFolder;
        public static string assemblyLineObjectDumpFolder;
        public static string reservoirDumpFolder;
        public static string iconsDumpFolder;
        public static string tweaksFolder;
        public static string iconsFolder;
        public static string texturesFolder;

        public static bool firstRun = false;

        public static Variant patchData = null;
        public static ProxyObject patchDataItemChanges = null;
        public static ProxyObject patchDataElementChanges = null;
        public static ProxyObject patchDataRecipeChanges = null;
        public static ProxyObject patchDataRecipeCategoryChanges = null;
        public static ProxyObject patchDataRecipeCategoryRowChanges = null;
        public static ProxyObject patchDataTerrainChanges = null;
        public static ProxyObject patchDataBuildingChanges = null;
        public static ProxyObject patchDataResearchChanges = null;
        public static ProxyObject patchDataBiomeChanges = null;
        public static ProxyObject patchDataBlastFurnaceModeChanges = null;
        public static ProxyObject patchDataAssemblyLineObjectChanges = null;
        public static ProxyObject patchDataReservoirChanges = null;
        public static ProxyObject patchDataItemAdditions = null;
        public static ProxyObject patchDataElementAdditions = null;
        public static ProxyObject patchDataRecipeAdditions = null;
        public static ProxyObject patchDataRecipeCategoryAdditions = null;
        public static ProxyObject patchDataRecipeCategoryRowAdditions = null;
        public static ProxyObject patchDataTerrainAdditions = null;
        public static ProxyObject patchDataBuildingAdditions = null;
        public static ProxyObject patchDataResearchAdditions = null;
        public static ProxyObject patchDataBiomeAdditions = null;
        public static ProxyObject patchDataBlastFurnaceModeAdditions = null;
        public static ProxyObject patchDataAssemblyLineObjectAdditions = null;
        public static ProxyObject patchDataReservoirAdditions = null;

        public static Dictionary<System.Type, JSON.PopulateOverride> populateOverrides = new Dictionary<System.Type, JSON.PopulateOverride>();

        public static Dictionary<ulong, Texture2D[]> botIdToTextureArray = null;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            tweakificatorFolder = Path.Combine(Path.GetFullPath("."), MODNAME.ToLower());
            dumpFolder = Path.Combine(tweakificatorFolder, "dumps");
            itemsDumpFolder = Path.Combine(dumpFolder, "items");
            elementsDumpFolder = Path.Combine(dumpFolder, "elements");
            recipesDumpFolder = Path.Combine(dumpFolder, "recipes");
            recipeCategoriesDumpFolder = Path.Combine(dumpFolder, "recipeCategories");
            recipeCategoryRowsDumpFolder = Path.Combine(dumpFolder, "recipeCategoryRows");
            terrainBlocksDumpFolder = Path.Combine(dumpFolder, "terrain");
            buildingsDumpFolder = Path.Combine(dumpFolder, "buildings");
            researchDumpFolder = Path.Combine(dumpFolder, "research");
            biomeDumpFolder = Path.Combine(dumpFolder, "biomes");
            blastFurnaceModeDumpFolder = Path.Combine(dumpFolder, "blastFurnaceModes");
            assemblyLineObjectDumpFolder = Path.Combine(dumpFolder, "assemblyLineObjects");
            reservoirDumpFolder = Path.Combine(dumpFolder, "reservoirs");
            iconsDumpFolder = Path.Combine(dumpFolder, "icons");
            tweaksFolder = Path.Combine(Path.GetFullPath("."), "tweaks");
            iconsFolder = Path.Combine(tweaksFolder, "icons");
            texturesFolder = Path.Combine(tweaksFolder, "textures");

            new Config(GUID)
                .Group("Dump")
                    .Entry(out dumpTemplates, "dumpTemplates", false, true, "Dump templates. (items, elements, recipes, terrainBlocks, buildings, research, biomes)")
                    .Entry(out forceDump, "forceDump", false, true, "Overwrite existing dump files.")
                    .Entry(out dumpIcons, "dumpIcons", false, true, "Dump icon files. (very slow)")
                    .Entry(out iconResolution, "iconResolution", 64, true, "Dumped icon resolution. Valid options are 0(max), 64, 96, 128, 256, 512")
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
            if (!Directory.Exists(recipeCategoriesDumpFolder)) Directory.CreateDirectory(recipeCategoriesDumpFolder);
            if (!Directory.Exists(recipeCategoryRowsDumpFolder)) Directory.CreateDirectory(recipeCategoryRowsDumpFolder);
            if (!Directory.Exists(terrainBlocksDumpFolder)) Directory.CreateDirectory(terrainBlocksDumpFolder);
            if (!Directory.Exists(buildingsDumpFolder)) Directory.CreateDirectory(buildingsDumpFolder);
            if (!Directory.Exists(researchDumpFolder)) Directory.CreateDirectory(researchDumpFolder);
            if (!Directory.Exists(biomeDumpFolder)) Directory.CreateDirectory(biomeDumpFolder);
            if (!Directory.Exists(blastFurnaceModeDumpFolder)) Directory.CreateDirectory(blastFurnaceModeDumpFolder);
            if (!Directory.Exists(assemblyLineObjectDumpFolder)) Directory.CreateDirectory(assemblyLineObjectDumpFolder);
            if (!Directory.Exists(reservoirDumpFolder)) Directory.CreateDirectory(reservoirDumpFolder);
            if (!Directory.Exists(iconsDumpFolder)) Directory.CreateDirectory(iconsDumpFolder);
            if (!Directory.Exists(tweaksFolder)) Directory.CreateDirectory(tweaksFolder);
            if (!Directory.Exists(iconsFolder)) Directory.CreateDirectory(iconsFolder);
            if (!Directory.Exists(texturesFolder)) Directory.CreateDirectory(texturesFolder);

            foreach (var path in Directory.GetFiles(tweaksFolder, "*.json", SearchOption.AllDirectories))
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

            FetchChangesObject(ref patchDataItemChanges, "items");
            FetchChangesObject(ref patchDataElementChanges, "elements");
            FetchChangesObject(ref patchDataRecipeChanges, "recipes");
            FetchChangesObject(ref patchDataRecipeCategoryChanges, "recipeCategories");
            FetchChangesObject(ref patchDataRecipeCategoryRowChanges, "recipeCategoryRows");
            FetchChangesObject(ref patchDataTerrainChanges, "terrain");
            FetchChangesObject(ref patchDataResearchChanges, "research");
            FetchChangesObject(ref patchDataBiomeChanges, "biomes");
            FetchChangesObject(ref patchDataBuildingChanges, "buildings");
            FetchChangesObject(ref patchDataBlastFurnaceModeChanges, "blastFurnaceModes");
            FetchChangesObject(ref patchDataAssemblyLineObjectChanges, "assemblyLineObjects");
            FetchChangesObject(ref patchDataReservoirChanges, "reservoirs");
            FetchAdditionsObject(ref patchDataItemAdditions, "items");
            FetchAdditionsObject(ref patchDataElementAdditions, "elements");
            FetchAdditionsObject(ref patchDataRecipeAdditions, "recipes");
            FetchAdditionsObject(ref patchDataRecipeCategoryAdditions, "recipeCategories");
            FetchAdditionsObject(ref patchDataRecipeCategoryRowAdditions, "recipeCategoryRows");
            FetchAdditionsObject(ref patchDataTerrainAdditions, "terrain");
            FetchAdditionsObject(ref patchDataResearchAdditions, "research");
            FetchAdditionsObject(ref patchDataBiomeAdditions, "biomes");
            FetchAdditionsObject(ref patchDataBuildingAdditions, "buildings");
            FetchAdditionsObject(ref patchDataBlastFurnaceModeAdditions, "blastFurnaceModes");
            FetchAdditionsObject(ref patchDataAssemblyLineObjectAdditions, "assemblyLineObjects");
            FetchAdditionsObject(ref patchDataReservoirAdditions, "reservoirs");

            populateOverrides.Add(typeof(ItemTemplate.ItemMode[]), (Variant data) =>
            {
                if (!(data is ProxyObject dataObject))
                {
                    log.LogError("Invalid item modes object");
                    return new ItemTemplate.ItemMode[0];
                }

                var modes = new ItemTemplate.ItemMode[dataObject.Count];
                var index = 0;
                foreach (var kv in dataObject)
                {
                    if (!(kv.Value is ProxyObject modeObject)) throw new System.Exception("Invalid item mode object");
                    var mode = new ItemTemplate.ItemMode
                    {
                        identifier = kv.Key
                    };
                    if (modeObject.TryGetValue("name", out var name)) mode.name = name;
                    else mode.name = kv.Key;
                    if (modeObject.TryGetValue("isDefault", out var isDefault)) mode.isDefault = isDefault?.ToBoolean(System.Globalization.CultureInfo.InvariantCulture) ?? false;
                    if (modeObject.TryGetValue("icon_identifier", out var icon_identifier)) mode.icon = ResourceDB.getIcon(icon_identifier.ToString());
                    else if (modeObject.TryGetValue("icon", out var icon)) mode.icon = ResourceDB.getIcon(icon.ToString());
                    modes[index++] = mode;
                }

                return modes;
            });

            populateOverrides.Add(typeof(ResearchTemplate.ResearchTemplateItemInput[]), (Variant data) =>
            {
                if (!(data is ProxyObject dataObject))
                {
                    log.LogError("Invalid research item object");
                    return new ResearchTemplate.ResearchTemplateItemInput[0];
                }

                var values = new ResearchTemplate.ResearchTemplateItemInput[dataObject.Count];
                var index = 0;
                foreach (var kv in dataObject)
                {
                    if (!(kv.Value is ProxyObject modeObject)) throw new System.Exception("Invalid research item object");
                    var value = new ResearchTemplate.ResearchTemplateItemInput
                    {
                        identifier = kv.Key
                    };
                    if (modeObject.TryGetValue("amount", out var amount)) value.amount = amount.ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                    values[index++] = value;
                }

                return values;
            });

            populateOverrides.Add(typeof(CraftingRecipe.CraftingRecipeItemInput[]), (Variant data) =>
            {
                if (!(data is ProxyObject dataObject))
                {
                    log.LogError("Invalid crafing recipe item object");
                    return new CraftingRecipe.CraftingRecipeItemInput[0];
                }

                var values = new CraftingRecipe.CraftingRecipeItemInput[dataObject.Count];
                var index = 0;
                foreach (var kv in dataObject)
                {
                    if (!(kv.Value is ProxyObject modeObject)) throw new System.Exception("Invalid crafing recipe item object");
                    var value = new CraftingRecipe.CraftingRecipeItemInput
                    {
                        identifier = kv.Key
                    };
                    if (modeObject.TryGetValue("amount", out var amount)) value.amount = amount.ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                    if (modeObject.TryGetValue("percentage_str", out var percentage_str)) value.percentage_str = percentage_str.ToString();
                    values[index++] = value;
                }

                return values;
            });

            populateOverrides.Add(typeof(CraftingRecipe.CraftingRecipeElementalInput[]), (Variant data) =>
            {
                if (!(data is ProxyObject dataObject))
                {
                    log.LogError("Invalid item modes object");
                    return new CraftingRecipe.CraftingRecipeElementalInput[0];
                }

                var values = new CraftingRecipe.CraftingRecipeElementalInput[dataObject.Count];
                var index = 0;
                foreach (var kv in dataObject)
                {
                    if (!(kv.Value is ProxyObject modeObject)) throw new System.Exception("Invalid crafing recipe element object");
                    var value = new CraftingRecipe.CraftingRecipeElementalInput
                    {
                        identifier = kv.Key
                    };
                    if (modeObject.TryGetValue("amount_str", out var amount_str)) value.amount_str = amount_str.ToString();
                    values[index++] = value;
                }

                return values;
            });

            populateOverrides.Add(typeof(Texture2D), (Variant data) =>
            {
                if (!(data is ProxyString dataString))
                {
                    log.LogError("Invalid texture");
                    return null;
                }

                return ResourceExt.FindTexture(dataString.ToString());
            });
        }

        private static void FetchChangesObject(ref ProxyObject patchDataChanges, string key)
        {
            if (patchData is ProxyObject patchDataObject)
            {
                if (patchDataObject.TryGetValue("changes", out var changes) && changes is ProxyObject changesObject)
                {
                    if (changesObject.TryGetValue(key, out var templates) && templates is ProxyObject templatesObject)
                    {
                        patchDataChanges = templatesObject;
                    }
                }
            }
        }

        private static void FetchAdditionsObject(ref ProxyObject patchDataAdditions, string key)
        {
            if (patchData is ProxyObject patchDataObject)
            {
                if (patchDataObject.TryGetValue("additions", out var additions) && additions is ProxyObject additionsObject)
                {
                    if (additionsObject.TryGetValue(key, out var templates) && templates is ProxyObject templatesObject)
                    {
                        patchDataAdditions = templatesObject;
                    }
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
            //private const int terrainBlockCount = GameRoot.BUILDING_PART_ARRAY_IDX_START - 1;
            //private static bool done = false;
            //[HarmonyPatch(typeof(GameRoot), "Update")]
            //[HarmonyPostfix]
            //public static void GameRootUpdate()
            //{
            //    if (!GameRoot.IsGameInitDone) return;
            //    if (done) return;
            //    done = true;
            //    log.Log("Dumping terrain.");

            //    var terrainCount = GameRoot.TerrainCount;
            //    var buildingPartCount = GameRoot.BuildingPartCount;
            //    log.Log($"{terrainCount} {buildingPartCount}");

            //    var sb = new StringBuilder();
            //    sb.AppendLine($"Terrain Count: {terrainCount}");
            //    sb.AppendLine($"Building Part Count: {buildingPartCount}");
            //    for (int i = 0; i < terrainCount; i++)
            //    {
            //        log.Log($"{i}");
            //        var template = ItemTemplateManager.getTerrainBlockTemplateByByteIdx((byte)i);
            //        if (template != null)
            //        {
            //            log.Log($"{template.name}");
            //            sb.AppendLine($"\"{i}\": \"{template.name}\",");
            //        }
            //    }
            //    for (int i = GameRoot.BUILDING_PART_ARRAY_IDX_START; i < buildingPartCount; i++)
            //    {
            //        log.Log($"{i}");
            //        var template = ItemTemplateManager.getBuildingPartTemplate(GameRoot.BuildingPartIdxLookupTable.table[i]);
            //        if (template != null)
            //        {
            //            log.Log($"{template.name}");
            //            sb.AppendLine($"\"{i}\": \"{template.name}\",");
            //        }
            //    }
            //    File.WriteAllText("terrain.txt", sb.ToString());
            //}

            [HarmonyPatch(typeof(TextureStreamingProcessor), nameof(TextureStreamingProcessor.OnAddedToManager))]
            [HarmonyPostfix]
            public static void TextureStreamingProcessorOnAddedToManager(TextureStreamingProcessor __instance)
            {
                botIdToTextureArray = typeof(TextureStreamingProcessor).GetField("botIdToTextureArray", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Dictionary<ulong, Texture2D[]>;
            }

            [HarmonyPatch(typeof(ResourceDB), nameof(ResourceDB.InitOnApplicationStart))]
            [HarmonyPostfix]
            public static void ResourceDBInitOnApplicationStart()
            {
                var dict_icons = typeof(ResourceDB).GetField("dict_icons", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<int, Dictionary<ulong, Sprite>>;

                var filenames = Directory.GetFiles(iconsFolder, "*.png");
                log.Log(string.Format("Loading {0} custom icons.", filenames.Length));
                foreach (var filename in filenames)
                {
                    var iconPath = Path.Combine(iconsFolder, filename);
                    var identifier = Path.GetFileNameWithoutExtension(iconPath);
                    if (!dict_icons[0].ContainsKey(GameRoot.generateStringHash64(identifier)))
                    {
                        if (verbose.Get())
                        {
                            log.LogFormat("Loading icon {0}", Path.GetFileName(iconPath));
                        }

                        var iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                        iconTexture.LoadImage(File.ReadAllBytes(iconPath), false);
                        int index = 0;
                        foreach (var entry in iconSizes)
                        {
                            var sizeId = entry.Key;
                            var size = entry.Value;
                            var sizeIdentifier = identifier + ((sizeId > 0) ? "_" + sizeId.ToString() : "_0");
                            var texture = (sizeId > 0) ? resizeTexture(iconTexture, size, size) : iconTexture;
                            texture.name = sizeIdentifier;
                            dict_icons[sizeId][GameRoot.generateStringHash64(sizeIdentifier)] = createSprite(texture);

                            ++index;
                        }
                    }
                }

                var listPath = Path.Combine(iconsDumpFolder, "__icons.txt");
                if (forceDump.Get() || !File.Exists(listPath))
                {
                    var iconNames = new List<string>();
                    foreach (var entry in dict_icons[0]) iconNames.Add(entry.Value.name);// string.Format("{0}: {1}", entry.Value.name, entry.Value.texture.format.ToString()));
                    File.WriteAllText(listPath, string.Join("\r\n", iconNames));
                }

                if (dumpIcons.Get())
                {
                    var targetResolution = iconResolution.Get();
                    targetResolution = new int[] { 0, 64, 96, 128, 256, 512 }.OrderBy(x => Mathf.Abs(x - targetResolution)).First();

                    log.LogFormat("Dumping icons. Resolution: {0}", targetResolution);

                    var cache = new Dictionary<string, Texture2D>();
                    foreach (var entry in dict_icons[0])
                    {
                        var spriteName = entry.Value.name;
                        if (spriteName.EndsWith("_0")) spriteName = spriteName.Substring(0, spriteName.Length - 2);
                        spriteName = $"{spriteName}_{targetResolution}";
                        var stringHash64 = GameRoot.generateStringHash64(spriteName);
                        var sprite = dict_icons[targetResolution].ContainsKey(stringHash64) ? dict_icons[targetResolution][stringHash64] : entry.Value;
                        var path = Path.Combine(iconsDumpFolder, sprite.name + ".png");
                        if (!File.Exists(path))
                        {
                            if (!cache.TryGetValue(sprite.texture.name, out Texture2D texture))
                            {
                                if (verbose.Get()) log.Log(string.Format("Converting icon texture '{0}'", sprite.texture.name));
                                texture = duplicateTexture(sprite.texture);
                                cache[sprite.texture.name] = texture;
                            }

                            if (verbose.Get()) log.Log(string.Format("Dumping icon '{0}'", sprite.name));
                            var croppedTexture = new Texture2D(Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), TextureFormat.RGBA32, false);
                            var pixels = texture.GetPixels(Mathf.FloorToInt(sprite.textureRect.x), Mathf.FloorToInt(sprite.textureRect.y), Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), 0);
                            croppedTexture.SetPixels(pixels);
                            croppedTexture.Apply();
                            var bytes = croppedTexture.EncodeToPNG();
                            File.WriteAllBytes(path, bytes);

                            Object.Destroy(croppedTexture);
                            System.GC.Collect();
                        }
                    }
                    foreach (var texture in cache.Values) Object.Destroy(texture);
                    cache.Clear();
                    System.GC.Collect();
                }
            }

            [HarmonyPatch(typeof(ItemTemplate), nameof(ItemTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadItemTemplate(ItemTemplate __instance)
            {
                if (hasRun_items) return;
                hasLoaded_items = true;

                //if (!string.IsNullOrEmpty(__instance.icon_identifier))
                //{
                //    var sprite = ResourceDB.getIcon(__instance.icon_identifier, 96);
                //    if (sprite != null)
                //    {
                //        var hash = ItemTemplate.generateStringHash(__instance.identifier);
                //        var path = Path.Combine(iconsDumpFolder, "web", $"{(hash >> 32) & 0xFFFFFFFFUL}-{hash & 0xFFFFFFFFUL}.png");
                //        if (!File.Exists(path))
                //        {
                //            var texture = duplicateTexture(sprite.texture);

                //            var croppedTexture = new Texture2D(Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), TextureFormat.RGBA32, false);
                //            var pixels = texture.GetPixels(Mathf.FloorToInt(sprite.textureRect.x), Mathf.FloorToInt(sprite.textureRect.y), Mathf.CeilToInt(sprite.textureRect.width), Mathf.CeilToInt(sprite.textureRect.height), 0);
                //            croppedTexture.SetPixels(pixels);
                //            croppedTexture.Apply();
                //            var bytes = croppedTexture.EncodeToPNG();
                //            File.WriteAllBytes(path, bytes);
                //            Object.Destroy(croppedTexture);
                //            System.GC.Collect();
                //        }
                //    }
                //    else
                //    {
                //        log.LogWarning($"Icon not found: {__instance.identifier}");
                //    }
                //}

                ProcessOnLoad<ItemDump, ItemTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "item",
                    patchDataItemChanges,
                    itemsDumpFolder,
                    ApplyItemTextures);
            }

            [HarmonyPatch(typeof(ElementTemplate), nameof(ElementTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadElementTemplate(ElementTemplate __instance)
            {
                if (hasRun_elements) return;
                hasLoaded_elements = true;

                ProcessOnLoad<ElementDump, ElementTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "element",
                    patchDataElementChanges,
                    elementsDumpFolder);
            }

            [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.onLoad))]
            [HarmonyPrefix]
            public static void onLoadRecipe(CraftingRecipe __instance)
            {
                if (hasRun_recipes) return;
                hasLoaded_recipes = true;

                ProcessOnLoad<RecipeDump, CraftingRecipe>(
                    ref __instance,
                    __instance.identifier,
                    "recipe",
                    patchDataRecipeChanges,
                    recipesDumpFolder);
            }

            [HarmonyPatch(typeof(CraftingRecipeCategory), nameof(CraftingRecipeCategory.onLoad))]
            [HarmonyPrefix]
            public static void onLoadRecipeCategory(CraftingRecipeCategory __instance)
            {
                if (hasRun_recipeCategories) return;
                hasLoaded_recipeCategories = true;

                ProcessOnLoad<RecipeCategoryDump, CraftingRecipeCategory>(
                    ref __instance,
                    __instance.identifier,
                    "recipe category",
                    patchDataRecipeCategoryChanges,
                    recipeCategoriesDumpFolder);
            }

            [HarmonyPatch(typeof(CraftingRecipeRowGroup), nameof(CraftingRecipeRowGroup.onLoad))]
            [HarmonyPrefix]
            public static void onLoadRecipeCategoryRow(CraftingRecipeRowGroup __instance)
            {
                if (hasRun_recipeCategoryRows) return;
                hasLoaded_recipeCategoryRows = true;

                ProcessOnLoad<RecipeCategoryRowDump, CraftingRecipeRowGroup>(
                    ref __instance,
                    __instance.identifier,
                    "recipe category row",
                    patchDataRecipeCategoryRowChanges,
                    recipeCategoryRowsDumpFolder);
            }

            [HarmonyPatch(typeof(BuildableObjectTemplate), nameof(BuildableObjectTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadBuildableObjectTemplate(BuildableObjectTemplate __instance)
            {
                if (hasRun_buildings) return;
                hasLoaded_buildings = true;

                ProcessOnLoad<BuildableObjectDump, BuildableObjectTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "building",
                    patchDataBuildingChanges,
                    buildingsDumpFolder,
                    ApplyBuildingTextures);
            }

            [HarmonyPatch(typeof(ResearchTemplate), nameof(ResearchTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadResearchTemplate(ResearchTemplate __instance)
            {
                if (hasRun_research) return;
                hasLoaded_research = true;

                ProcessOnLoad<ResearchDump, ResearchTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "research",
                    patchDataResearchChanges,
                    researchDumpFolder);
            }

            [HarmonyPatch(typeof(BiomeTemplate), nameof(BiomeTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadBiomeTemplate(BiomeTemplate __instance)
            {
                if (hasRun_biomes) return;
                hasLoaded_biomes = true;

                ProcessOnLoad<BiomeDump, BiomeTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "biome",
                    patchDataBiomeChanges,
                    biomeDumpFolder);
            }

            [HarmonyPatch(typeof(TerrainBlockType), nameof(TerrainBlockType.onLoad))]
            [HarmonyPrefix]
            public static void onLoadTerrainBlockType(TerrainBlockType __instance)
            {
                if (hasRun_terrain) return;
                hasLoaded_terrain = true;

                ProcessOnLoad<TerrainBlockDump, TerrainBlockType>(
                    ref __instance,
                    __instance.identifier,
                    "terrain block",
                    patchDataTerrainChanges,
                    terrainBlocksDumpFolder);
            }

            [HarmonyPatch(typeof(BlastFurnaceModeTemplate), nameof(BlastFurnaceModeTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadBlastFurnaceModeTemplate(BlastFurnaceModeTemplate __instance)
            {
                if (hasRun_blastFurnaceModes) return;
                hasLoaded_blastFurnaceModes = true;

                ProcessOnLoad<BlastFurnaceModeDump, BlastFurnaceModeTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "blast furnace mode",
                    patchDataBlastFurnaceModeChanges,
                    blastFurnaceModeDumpFolder);
            }

            [HarmonyPatch(typeof(AssemblyLineObjectTemplate), nameof(AssemblyLineObjectTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadAssemblyLineObjectTemplate(AssemblyLineObjectTemplate __instance)
            {
                if (hasRun_assemblyLineObjects) return;
                hasLoaded_assemblyLineObjects = true;
                ProcessOnLoad<AssemblyLineObjectDump, AssemblyLineObjectTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "assembly line object",
                    patchDataAssemblyLineObjectChanges,
                    assemblyLineObjectDumpFolder);
            }

            [HarmonyPatch(typeof(ReservoirTemplate), nameof(ReservoirTemplate.onLoad))]
            [HarmonyPrefix]
            public static void onLoadReservoirTemplate(ReservoirTemplate __instance)
            {
                if (hasRun_reservoirs) return;
                hasLoaded_reservoirs = true;
                ProcessOnLoad<ReservoirDump, ReservoirTemplate>(
                    ref __instance,
                    __instance.identifier,
                    "reservoir",
                    patchDataReservoirChanges,
                    reservoirDumpFolder);
            }

            private static bool hasLoaded_items = false;
            private static bool hasRun_items = false;
            private static bool hasLoaded_elements = false;
            private static bool hasRun_elements = false;
            private static bool hasLoaded_recipes = false;
            private static bool hasRun_recipes = false;
            private static bool hasLoaded_recipeCategories = false;
            private static bool hasRun_recipeCategories = false;
            private static bool hasLoaded_recipeCategoryRows = false;
            private static bool hasRun_recipeCategoryRows = false;
            private static bool hasLoaded_terrain = false;
            private static bool hasRun_terrain = false;
            private static bool hasLoaded_research = false;
            private static bool hasRun_research = false;
            private static bool hasLoaded_biomes = false;
            private static bool hasRun_biomes = false;
            private static bool hasLoaded_buildings = false;
            private static bool hasRun_buildings = false;
            private static bool hasLoaded_blastFurnaceModes = false;
            private static bool hasRun_blastFurnaceModes = false;
            private static bool hasLoaded_assemblyLineObjects = false;
            private static bool hasRun_assemblyLineObjects = false;
            private static bool hasLoaded_reservoirs = false;
            private static bool hasRun_reservoirs = false;

            class InitOnApplicationStartEnumerator : IEnumerable
            {
                private IEnumerator _enumerator;

                public InitOnApplicationStartEnumerator(IEnumerator enumerator)
                {
                    _enumerator = enumerator;
                }

                IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

                public IEnumerator GetEnumerator()
                {
                    while (_enumerator.MoveNext())
                    {
                        var enumerated = _enumerator.Current;

                        if (!hasRun_items && hasLoaded_items)
                        {
                            hasRun_items = true;

                            ProcessItemAdditions();
                        }

                        if (!hasRun_buildings && hasLoaded_buildings)
                        {
                            hasRun_buildings = true;

                            ProcessBuildingAdditions();
                        }

                        if (!hasRun_terrain && hasLoaded_terrain)
                        {
                            hasRun_terrain = true;

                            ProcessTerrainAdditions();
                        }

                        if (!hasRun_recipes && hasLoaded_recipes)
                        {
                            hasRun_recipes = true;

                            ProcessRecipeAdditions();
                        }

                        if (!hasRun_research && hasLoaded_research)
                        {
                            hasRun_research = true;

                            ProcessResearchAdditions();
                        }

                        if (!hasRun_elements && hasLoaded_elements)
                        {
                            hasRun_elements = true;

                            ProcessElementAdditions();
                        }

                        if (!hasRun_recipeCategories && hasLoaded_recipeCategories)
                        {
                            hasRun_recipeCategories = true;

                            ProcessRecipeCategoryAdditions();
                        }

                        if (!hasRun_recipeCategoryRows && hasLoaded_recipeCategoryRows)
                        {
                            hasRun_recipeCategoryRows = true;

                            ProcessRecipeCategoryRowAdditions();
                        }

                        if (!hasRun_blastFurnaceModes && hasLoaded_blastFurnaceModes)
                        {
                            hasRun_blastFurnaceModes = true;

                            ProcessBlastFurnaceModeAdditions();
                        }

                        yield return enumerated;
                    }
                }
            }

            [HarmonyPatch(typeof(ItemTemplateManager), nameof(ItemTemplateManager.InitOnApplicationStart))]
            [HarmonyPostfix]
            static void onItemTemplateManagerInitOnApplicationStart(ref IEnumerator __result)
            {
                log.LogFormat("onItemTemplateManagerInitOnApplicationStart");
                var myEnumerator = new InitOnApplicationStartEnumerator(__result);
                __result = myEnumerator.GetEnumerator();
            }
        }

        private static void ProcessOnLoad<D, T>(ref T instance, string identifier, string displayName, ProxyObject patchDataChanges, string dumpFolderPath, System.Action<T, ProxyObject> callback = null) where D : new()
        {
            var path = Path.Combine(dumpFolderPath, identifier + ".json");
            if (forceDump.Get() || !File.Exists(path))
            {
                D data = gatherDump<D, T>(instance);
                File.WriteAllText(path, JSON.Dump(data, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints));
            }

            if (patchDataChanges != null)
            {
                foreach (var entry in patchDataChanges)
                {
                    if (buildPatternRegex(entry.Key).IsMatch(identifier))
                    {
                        if (entry.Value is ProxyObject changes)
                        {
                            if (verbose.Get())
                            {
                                if (identifier != entry.Key)
                                    log.LogFormat("Patching {0} {1}. Matched '{2}'", displayName, identifier, entry.Key);
                                else
                                    log.LogFormat("Patching {0} {1}", displayName, identifier);
                            }

                            changes.Populate(ref instance, populateOverrides);

                            callback?.Invoke(instance, changes);
                        }
                    }
                }
            }
        }

        private static void ProcessItemAdditions()
        {
            if (patchDataItemAdditions != null)
            {
                var dict_itemTemplates_sciencePacks = typeof(ItemTemplateManager).GetField("dict_itemTemplates_sciencePacks", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ItemTemplate>;
                var dict_itemTemplates_constructionMaterial = typeof(ItemTemplateManager).GetField("dict_itemTemplates_constructionMaterial", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ItemTemplate>;
                var dict_itemTemplates_constructionRubble = typeof(ItemTemplateManager).GetField("dict_itemTemplates_constructionRubble", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ItemTemplate>;
                var dict_itemTemplates_salesItems = typeof(ItemTemplateManager).GetField("dict_itemTemplates_salesItems", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ItemTemplate>;
                var dict_itemTemplates_salesCurrencies = typeof(ItemTemplateManager).GetField("dict_itemTemplates_salesCurrencies", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ItemTemplate>;

                var itemTemplates = ItemTemplateManager.getAllItemTemplates();
                foreach (var entry in patchDataItemAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid item:\r\n" + entry.Value.ToString());
                    }
                    ItemTemplate template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = ItemTemplate.generateStringHash(templateIdentifier);
                        if (!itemTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template item {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding item {0}", entry.Key));

                    ItemTemplate instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.Log(string.Format("Using template {0}", template.identifier));
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<ItemTemplate>();
                        instance.railMiner_terrainTargetList_str = new string[0];
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);

                    ApplyItemTextures(instance, source);

                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                    if (instance.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.SCIENCE_ITEM))
                    {
                        dict_itemTemplates_sciencePacks.Add(instance.id, instance);
                    }

                    if (instance.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.CONSTRUCTION_MATERIAL))
                    {
                        dict_itemTemplates_constructionMaterial.Add(instance.id, instance);
                    }

                    if (instance.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.CONSTRUCTION_RUBBLE))
                    {
                        dict_itemTemplates_constructionRubble.Add(instance.id, instance);
                    }

                    if (instance.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.SALES_ITEM))
                    {
                        dict_itemTemplates_salesItems.Add(instance.id, instance);
                    }

                    if (instance.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.SALES_ITEM_ASSEMBLY_LINE))
                    {
                        dict_itemTemplates_salesItems.Add(instance.id, instance);
                    }

                    if (instance.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.SALES_CURRENCY))
                    {
                        dict_itemTemplates_salesCurrencies.Add(instance.id, instance);
                    }
                }
            }
        }

        private static void ProcessBuildingAdditions()
        {
            if (patchDataBuildingAdditions != null)
            {
                var dict_buildingPartTemplates = typeof(ItemTemplateManager).GetField("dict_buildingPartTemplates", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, BuildableObjectTemplate>;

                var buildingTemplates = ItemTemplateManager.getAllBuildableObjectTemplates();
                foreach (var entry in patchDataBuildingAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid building:\r\n" + entry.Value.ToString());
                    }

                    BuildableObjectTemplate template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = BuildableObjectTemplate.generateStringHash(templateIdentifier);
                        if (!buildingTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template building {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding building {0}", entry.Key));

                    BuildableObjectTemplate instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<BuildableObjectTemplate>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);

                    ApplyBuildingTextures(instance, source);

                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                    if (instance.type == BuildableObjectTemplate.BuildableObjectType.BuildingPart)
                        dict_buildingPartTemplates.Add(instance.id, instance);

                    botIdToTextureArray[instance.id] = new Texture2D[] {
                        instance.buildingPart_texture_albedo,
                        instance.buildingPart_texture_bottom_albedo,
                        instance.buildingPart_texture_side_albedo
                    }.Where(x => x != null).ToArray();
                }
            }
        }

        private static void ProcessTerrainAdditions()
        {
            if (patchDataTerrainAdditions != null)
            {
                var list_terrainBlockTypesSorted = typeof(ItemTemplateManager).GetField("list_terrainBlockTypesSorted", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as List<TerrainBlockType>;

                var terrainTemplates = ItemTemplateManager.getAllTerrainTemplates();
                foreach (var entry in patchDataTerrainAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid terrain:\r\n" + entry.Value.ToString());
                    }

                    TerrainBlockType template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = TerrainBlockType.generateStringHash(templateIdentifier);
                        if (!terrainTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template terrain {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding terrain {0}", entry.Key));

                    TerrainBlockType instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<TerrainBlockType>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                    list_terrainBlockTypesSorted.Add(instance);
                }
            }
        }

        private static void ProcessRecipeAdditions()
        {
            if (patchDataRecipeAdditions != null)
            {
                var recipeTemplates = ItemTemplateManager.getAllCraftingRecipes();
                foreach (var entry in patchDataRecipeAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid recipe:\r\n" + entry.Value.ToString());
                    }

                    CraftingRecipe template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = CraftingRecipe.generateStringHash(templateIdentifier);
                        if (!recipeTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template recipe {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding recipe {0}", entry.Key));

                    CraftingRecipe instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<CraftingRecipe>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                }
            }
        }

        private static void ProcessRecipeCategoryAdditions()
        {
            if (patchDataRecipeCategoryAdditions != null)
            {
                var recipeCategoryTemplates = ItemTemplateManager.getCraftingRecipeCategoryDictionary();
                foreach (var entry in patchDataRecipeCategoryAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid recipe category:\r\n" + entry.Value.ToString());
                    }

                    CraftingRecipeCategory template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = CraftingRecipeCategory.generateStringHash(templateIdentifier);
                        if (!recipeCategoryTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template recipe {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding recipe category {0}", entry.Key));

                    CraftingRecipeCategory instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<CraftingRecipeCategory>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                }
            }
        }

        private static void ProcessRecipeCategoryRowAdditions()
        {
            if (patchDataRecipeCategoryRowAdditions != null)
            {
                var recipeCategoryRowTemplates = typeof(ItemTemplateManager)
                    .GetField("dict_craftingRecipeRowGroups", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null) as Dictionary<ulong, CraftingRecipeRowGroup>;
                foreach (var entry in patchDataRecipeCategoryRowAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid recipe category row:\r\n" + entry.Value.ToString());
                    }

                    CraftingRecipeRowGroup template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = CraftingRecipeRowGroup.generateStringHash(templateIdentifier);
                        if (!recipeCategoryRowTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template recipe {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding recipe category row {0}", entry.Key));

                    CraftingRecipeRowGroup instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<CraftingRecipeRowGroup>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                }
            }
        }

        private static void ProcessResearchAdditions()
        {
            if (patchDataResearchAdditions != null)
            {
                var dict_researchTemplates = typeof(ItemTemplateManager).GetField("dict_researchTemplates", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ResearchTemplate>;

                var researchTemplates = ItemTemplateManager.getResearchTemplateDictionary();
                foreach (var entry in patchDataResearchAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid research:\r\n" + entry.Value.ToString());
                    }

                    ResearchTemplate template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = ResearchTemplate.generateStringHash(templateIdentifier);
                        if (!researchTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template research {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding research {0}", entry.Key));

                    ResearchTemplate instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<ResearchTemplate>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    instance.onLoad();
                    dict_researchTemplates.Add(instance.id, instance);
                }
            }
        }

        private static void ProcessElementAdditions()
        {
            if (patchDataElementAdditions != null)
            {
                var dict_elementTemplates = typeof(ItemTemplateManager).GetField("dict_elementTemplates", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, ElementTemplate>;

                var elementTemplates = ItemTemplateManager.getAllElementTemplates();
                foreach (var entry in patchDataElementAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        throw new System.Exception("Invalid element:\r\n" + entry.Value.ToString());
                    }

                    ElementTemplate template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = ElementTemplate.generateStringHash(templateIdentifier);
                        if (!elementTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template element {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding element {0}", entry.Key));

                    ElementTemplate instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<ElementTemplate>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    instance.onLoad();
                    dict_elementTemplates.Add(instance.id, instance);
                }
            }
        }

        private static void ProcessBlastFurnaceModeAdditions()
        {
            if (patchDataBlastFurnaceModeAdditions != null)
            {
                var dict_blastFurnaceModeTemplates = typeof(ItemTemplateManager).GetField("dict_blastFurnaceModeTemplates", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<ulong, BlastFurnaceModeTemplate>;

                var blastFurnaceModeTemplates = ItemTemplateManager.getAllBlastFurnaceModeTemplates();
                foreach (var entry in patchDataBlastFurnaceModeAdditions)
                {
                    if (!(entry.Value is ProxyObject source))
                    {
                        log.LogError("Invalid blast furnace mode:\r\n" + entry.Value.ToString());
                        continue;
                    }

                    BlastFurnaceModeTemplate template = null;
                    if (source.ContainsKey("__template"))
                    {
                        var templateIdentifier = source["__template"].ToString();
                        var templateHash = BlastFurnaceModeTemplate.generateStringHash(templateIdentifier);
                        if (!blastFurnaceModeTemplates.TryGetValue(templateHash, out template))
                        {
                            log.LogError(string.Format("Template blast furnace mode {0} not found!", templateIdentifier));
                        }
                    }

                    if (verbose.Get()) log.Log(string.Format("Adding blast furnace mode {0}", entry.Key));

                    BlastFurnaceModeTemplate instance;
                    if (template != null)
                    {
                        if (verbose.Get()) log.LogFormat("Using template {0}", template.identifier);
                        instance = Object.Instantiate(template);
                    }
                    else
                    {
                        instance = ScriptableObject.CreateInstance<BlastFurnaceModeTemplate>();
                    }

                    instance.identifier = entry.Key;
                    entry.Value.Populate(ref instance, populateOverrides);
                    AssetManager.registerAsset(instance, true);
                    instance.onLoad();
                    dict_blastFurnaceModeTemplates.Add(instance.id, instance);
                }
            }
        }

        private static void ApplyItemTextures(ItemTemplate instance, ProxyObject source)
        {
            if (instance.meshMaterial != null && source.TryGetValue("__conveyorItemTexture", out var textureNameValue) && !string.IsNullOrEmpty(textureNameValue.ToString()))
            {
                var textureName = textureNameValue.ToString();
                var material = new Material(instance.meshMaterial);
                material.SetTexture("_MainTex", ResourceExt.FindTexture(textureName));
                instance.meshMaterial = material;
            }
        }

        private static void ApplyBuildingTextures(BuildableObjectTemplate building, ProxyObject source)
        {
            if (source.TryGetValue("__mainTexture", out var textureNameValue)
                && !string.IsNullOrEmpty(textureNameValue.ToString()))
            {
                if (verbose.Get()) log.Log($"Adding texture {textureNameValue.ToString()} to {building.identifier}");
                if (building.prefabOnDisk != null)
                {
                    ApplyTextureToPrefab(building.prefabOnDisk, 0, "_MainTex", textureNameValue.ToString(), ref building.conveyor_material, ref building.conveyor_material_inv);
                }
            }

            if (source.TryGetValue("__conveyorTexture", out var conveyorTextureNameValue)
                && !string.IsNullOrEmpty(conveyorTextureNameValue.ToString()))
            {
                if (verbose.Get()) log.Log($"Adding conveyor texture {conveyorTextureNameValue.ToString()} to {building.identifier}");
                if (building.prefabOnDisk != null)
                {
                    ApplyTextureToPrefab(building.prefabOnDisk, 1, "_MainTex", conveyorTextureNameValue.ToString(), ref building.conveyor_material, ref building.conveyor_material_inv);
                    for (int i = 1; i <= 4; i++)
                    {
                        var subConveyor = building.prefabOnDisk.transform.Find($"convey_01_straight ({i})")?.gameObject;
                        if (subConveyor != null)
                        {
                            ApplyTextureToPrefab(subConveyor, 1, "_MainTex", conveyorTextureNameValue.ToString(), ref building.conveyor_material, ref building.conveyor_material_inv);
                        }
                    }
                }
            }

            if (source.TryGetValue("__conveyorEmissionTexture", out var conveyorEmissionTextureNameValue)
                && !string.IsNullOrEmpty(conveyorEmissionTextureNameValue.ToString()))
            {
                if (verbose.Get()) log.Log($"Adding conveyor emission texture {conveyorEmissionTextureNameValue.ToString()} to {building.identifier}");
                if (building.prefabOnDisk != null)
                {
                    ApplyTextureToPrefab(building.prefabOnDisk, 1, "_Emission", conveyorEmissionTextureNameValue.ToString(), ref building.conveyor_material, ref building.conveyor_material_inv);
                    for (int i = 1; i <= 4; i++)
                    {
                        var subConveyor = building.prefabOnDisk.transform.Find($"convey_01_straight ({i})")?.gameObject;
                        if (subConveyor != null)
                        {
                            ApplyTextureToPrefab(subConveyor, 1, "_Emission", conveyorEmissionTextureNameValue.ToString(), ref building.conveyor_material, ref building.conveyor_material_inv);
                        }
                    }
                }
            }

            if (source.TryGetValue("__conveyorEmissionColour", out var conveyorEmissionColourValue)
                && conveyorEmissionColourValue is ProxyObject colourObject)
            {
                if (verbose.Get()) log.Log($"Adding conveyor emission colour to {building.identifier}");
                var emissionColor = Color.white;
                if (colourObject.TryGetValue("r", out var rValue)) emissionColor.r = rValue.ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                if (colourObject.TryGetValue("g", out var gValue)) emissionColor.g = gValue.ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                if (colourObject.TryGetValue("b", out var bValue)) emissionColor.b = bValue.ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                if (colourObject.TryGetValue("a", out var aValue)) emissionColor.a = aValue.ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                if (building.prefabOnDisk != null)
                {
                    ApplyColourToPrefab(building.prefabOnDisk, 1, "_EmissionColor", emissionColor, ref building.conveyor_material, ref building.conveyor_material_inv);
                    for (int i = 1; i <= 4; i++)
                    {
                        var subConveyor = building.prefabOnDisk.transform.Find($"convey_01_straight ({i})")?.gameObject;
                        if (subConveyor != null)
                        {
                            ApplyColourToPrefab(subConveyor, 1, "_EmissionColor", emissionColor, ref building.conveyor_material, ref building.conveyor_material_inv);
                        }
                    }
                }
            }

            if (source.TryGetValue("__conveyorTextureSpeed", out var conveyorTextureSpeed))
            {
                if (verbose.Get()) log.Log($"Adding conveyor texture speed to {building.identifier}");
                var speed = conveyorTextureSpeed.ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                if (building.prefabOnDisk != null)
                {
                    ApplyVector4ToPrefab(building.prefabOnDisk, 1, "_Speed", new Vector4(0.0f, speed * 1.33333f / 160.0f, 0.0f, 0.0f), ref building.conveyor_material, ref building.conveyor_material_inv);
                    for (int i = 1; i <= 4; i++)
                    {
                        var subConveyor = building.prefabOnDisk.transform.Find($"convey_01_straight ({i})")?.gameObject;
                        if (subConveyor != null)
                        {
                            ApplyVector4ToPrefab(subConveyor, 1, "_Speed", new Vector4(0.0f, speed * 1.33333f / 160.0f, 0.0f, 0.0f), ref building.conveyor_material, ref building.conveyor_material_inv);
                        }
                    }
                }
            }
        }

        private static void ApplyVector4ToPrefab(GameObject gameObject, int materialIndex, string propertyName, Vector4 value, ref Material conveyor_material, ref Material conveyor_material_inv)
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.sharedMaterials.Length > materialIndex && meshRenderer.sharedMaterials[materialIndex] != null)
            {
                var material = new Material(meshRenderer.sharedMaterials[materialIndex]);
                material.SetVector(propertyName, value);
                var sharedMaterials = meshRenderer.sharedMaterials;
                sharedMaterials[materialIndex] = material;
                meshRenderer.sharedMaterials = sharedMaterials;

                if (conveyor_material != null && material.name == conveyor_material.name) conveyor_material = material;
                if (conveyor_material_inv != null && material.name == conveyor_material_inv.name) conveyor_material_inv = material;
            }
        }

        private static void ApplyColourToPrefab(GameObject gameObject, int materialIndex, string propertyName, Color emissionColor, ref Material conveyor_material, ref Material conveyor_material_inv)
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.sharedMaterials.Length > materialIndex && meshRenderer.sharedMaterials[materialIndex] != null)
            {
                var material = new Material(meshRenderer.sharedMaterials[materialIndex]);
                material.SetColor(propertyName, emissionColor);
                var sharedMaterials = meshRenderer.sharedMaterials;
                sharedMaterials[materialIndex] = material;
                meshRenderer.sharedMaterials = sharedMaterials;

                if (conveyor_material != null && material.name == conveyor_material.name) conveyor_material = material;
                if (conveyor_material_inv != null && material.name == conveyor_material_inv.name) conveyor_material_inv = material;
            }
        }

        private static void ApplyTextureToPrefab(GameObject gameObject, int materialIndex, string propertyName, string textureName, ref Material conveyor_material, ref Material conveyor_material_inv)
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.sharedMaterials.Length > materialIndex && meshRenderer.sharedMaterials[materialIndex] != null)
            {
                var material = new Material(meshRenderer.sharedMaterials[materialIndex]);
                material.SetTexture(propertyName, ResourceExt.FindTexture(textureName));
                var sharedMaterials = meshRenderer.sharedMaterials;
                sharedMaterials[materialIndex] = material;
                meshRenderer.sharedMaterials = sharedMaterials;

                if (conveyor_material != null && material.name == conveyor_material.name) conveyor_material = material;
                if (conveyor_material_inv != null && material.name == conveyor_material_inv.name) conveyor_material_inv = material;
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

        private static Regex buildPatternRegex(string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", @"(?:.*?)") + "$");
        }

        public static D gatherDump<D, T>(T template) where D : new()
        {
            var dump = (object)new D();
            foreach (var field in typeof(D).GetFields())
            {
                if (!field.IsStatic && field.IsPublic && !field.IsNotSerialized)
                {
                    if (field.Name == "__conveyorTextureSpeed")
                    {
                        var gameObject = typeof(T).GetField("prefabOnDisk")?.GetValue(template) as GameObject;
                        if (gameObject != null)
                        {
                            var speedID = Shader.PropertyToID("_Speed");
                            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                            if (meshRenderer != null  && meshRenderer.sharedMaterials.Length > 1 && meshRenderer.sharedMaterials[1] != null)
                            {
                                if (meshRenderer.sharedMaterials[1].HasVector(speedID))
                                {
                                    var speed = meshRenderer.sharedMaterials[1].GetVector(speedID);
                                    field.SetValue(dump, speed.y * 160.0f / 1.33333f);
                                }
                            }
                            else
                            {
                                var subConveyor = gameObject.transform.Find($"convey_01_straight (1)")?.gameObject;
                                if (subConveyor != null)
                                {
                                    meshRenderer = subConveyor.GetComponent<MeshRenderer>();
                                    if (meshRenderer != null && meshRenderer.sharedMaterials.Length > 1 && meshRenderer.sharedMaterials[1] != null)
                                    {
                                        if (meshRenderer.sharedMaterials[1].HasVector(speedID))
                                        {
                                            var speed = meshRenderer.sharedMaterials[1].GetVector(speedID);
                                            field.SetValue(dump, speed.y * 160.0f / 1.33333f);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var templateField = template.GetType().GetField(field.Name);
                        if (templateField != null)
                        {
                            var value = gatherDumpValue(templateField.GetValue(template), field.FieldType, templateField.FieldType);
                            field.SetValue(dump, value);
                        }
                        else
                        {
                            log.LogError(string.Format("Failed to dump {0}", field.Name));
                        }
                    }
                }
            }
            return (D)dump;
        }

        public static object gatherDumpValue(object template, System.Type dumpType, System.Type templateType)
        {
            if (template == null)
            {
                return null;
            }
            else if (templateType == typeof(Texture2D) && dumpType == typeof(Texture2DProxy))
            {
                return new Texture2DProxy((Texture2D)template);
            }
            else if (templateType == typeof(SpriteProxy) && dumpType == typeof(SpriteProxy))
            {
                return new SpriteProxy((Sprite)template);
            }
            else if (templateType == typeof(Vector3Int) && dumpType == typeof(Vector3IntProxy))
            {
                return new Vector3IntProxy((Vector3Int)template);
            }
            else if (templateType == typeof(ResearchTemplate.ResearchTemplateItemInput[]) && dumpType == typeof(Dictionary<string, ResearchTemplateItemInputProxy>))
            {
                var templateValues = (ResearchTemplate.ResearchTemplateItemInput[])template;
                var dumpValues = new Dictionary<string, ResearchTemplateItemInputProxy>();
                foreach (var templateValue in templateValues)
                {
                    dumpValues[templateValue.identifier] = new ResearchTemplateItemInputProxy(templateValue);
                }
                return dumpValues;
            }
            else if (templateType == typeof(CraftingRecipe.CraftingRecipeItemInput[]) && dumpType == typeof(Dictionary<string, CraftingRecipeItemInputProxy>))
            {
                var templateValues = (CraftingRecipe.CraftingRecipeItemInput[])template;
                var dumpValues = new Dictionary<string, CraftingRecipeItemInputProxy>();
                foreach (var templateValue in templateValues)
                {
                    dumpValues[templateValue.identifier] = new CraftingRecipeItemInputProxy(templateValue);
                }
                return dumpValues;
            }
            else if (templateType == typeof(CraftingRecipe.CraftingRecipeElementalInput[]) && dumpType == typeof(Dictionary<string, CraftingRecipeElementalInputProxy>))
            {
                var templateValues = (CraftingRecipe.CraftingRecipeElementalInput[])template;
                var dumpValues = new Dictionary<string, CraftingRecipeElementalInputProxy>();
                foreach (var templateValue in templateValues)
                {
                    dumpValues[templateValue.identifier] = new CraftingRecipeElementalInputProxy(templateValue);
                }
                return dumpValues;
            }
            else if (templateType == typeof(ItemTemplate.ItemMode[]) && dumpType == typeof(Dictionary<string, ItemModeProxy>))
            {
                var templateValues = (ItemTemplate.ItemMode[])template;
                var dumpValues = new Dictionary<string, ItemModeProxy>();
                foreach (var templateValue in templateValues)
                {
                    dumpValues[templateValue.identifier] = new ItemModeProxy(templateValue);
                }
                return dumpValues;
            }
            else if (templateType.IsPrimitive || templateType == typeof(string) || templateType.IsEnum)
            {
                return template;
            }
            else if (templateType.IsGenericType && (templateType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var dumpValues = typeof(Plugin).GetMethod("gatherDumpList")
                    .MakeGenericMethod(dumpType.GetGenericArguments()[0], templateType.GetGenericArguments()[0])
                    .Invoke(null, new object[] { template });
                return dumpValues;
            }
            else if (templateType.IsArray)
            {
                var dumpValues = typeof(Plugin).GetMethod("gatherDumpArray")
                    .MakeGenericMethod(dumpType.GetElementType(), templateType.GetElementType())
                    .Invoke(null, new object[] { template });
                return dumpValues;
            }
            else if ((dumpType.GetConstructor(System.Type.EmptyTypes) != null || dumpType.IsValueType && dumpType != templateType) && templateType.Assembly == typeof(ItemTemplate).Assembly && !typeof(Object).IsAssignableFrom(templateType))
            {
                return typeof(Plugin).GetMethod("gatherDump")
                    .MakeGenericMethod(dumpType, templateType)
                    .Invoke(null, new object[] { template });
            }

            return template;
        }

        public static D[] gatherDumpArray<D, T>(T[] template)
        {
            var dumpType = typeof(D);
            var templateType = typeof(T);
            var dump = new D[template.Length];
            for (int i = 0; i < template.Length; i++)
            {
                dump[i] = (D)gatherDumpValue(template[i], dumpType, templateType);
            }

            return dump;
        }

        public static List<D> gatherDumpList<D, T>(List<T> template)
        {
            var dumpType = typeof(D);
            var templateType = typeof(T);
            var dump = new List<D>();
            foreach (var item in template)
            {
                dump.Add((D)gatherDumpValue(item, dumpType, templateType));
            }

            return dump;
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
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture.active = rt;
            Graphics.Blit(inputTexture, rt);
            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            return result;
        }

        internal static Sprite getIcon(string name)
        {
            return ResourceDB.getIcon(name, 256);
        }
    }
}
