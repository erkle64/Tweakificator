using System;
using TinyJSON;
using UnityEngine;

namespace Tweakificator
{
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
            name = (texture is null) ? "" : texture.name;
        }
    }

    public struct SpriteProxy
    {
        public string name;

        public SpriteProxy(string name)
        {
            this.name = name;
        }

        public SpriteProxy(Sprite sprite)
        {
            name = (sprite is null) ? "" : sprite.name;
        }
    }

    public struct ItemModeProxy
    {
        public string name;
        public string icon_identifier;
        public bool isDefault;

        public ItemModeProxy(string name, string icon_identifier, bool isDefault)
        {
            this.name = name;
            this.icon_identifier = icon_identifier;
            this.isDefault = isDefault;
        }

        public ItemModeProxy(ItemTemplate.ItemMode itemMode)
        {
            name = itemMode.name;
            icon_identifier = itemMode.icon?.name ?? "";
            isDefault = itemMode.isDefault;
        }
    }

    public struct CraftingRecipeItemInputProxy
    {
        public int amount;
        public string percentage_str;

        public CraftingRecipeItemInputProxy(int amount, string percentage_str)
        {
            this.amount = amount;
            this.percentage_str = percentage_str;
        }

        public CraftingRecipeItemInputProxy(CraftingRecipe.CraftingRecipeItemInput craftingRecipeItemInput)
        {
            amount = craftingRecipeItemInput.amount;
            percentage_str = craftingRecipeItemInput.percentage_str;
        }
    }

    public struct CraftingRecipeElementalInputProxy
    {
        public string amount_str;

        public CraftingRecipeElementalInputProxy(string amount_str)
        {
            this.amount_str = amount_str;
        }

        public CraftingRecipeElementalInputProxy(CraftingRecipe.CraftingRecipeElementalInput craftingRecipeElementalInput)
        {
            amount_str = craftingRecipeElementalInput.amount_str;
        }
    }

    public struct ResearchTemplateItemInputProxy
    {
        public int amount;

        public ResearchTemplateItemInputProxy(int amount)
        {
            this.amount = amount;
        }

        public ResearchTemplateItemInputProxy(ResearchTemplate.ResearchTemplateItemInput researchTemplateItemInput)
        {
            amount = researchTemplateItemInput.amount;
        }
    }

    public struct PrefabVizObjectProxy
    {
        public int amountCompleteRequired;
        public string prefabIdentifier;
        public int prefabPriority;
        public ProxyObject stationPrefab;
        public Vector3 stationPrefab_offset;
        public Vector3 stationPrefab_orientation;
        public ulong prefabIdentifierHash;

        public PrefabVizObjectProxy(SkyPlatformUpgradeTemplate.PrefabVizObject prefabVizObject)
        {
            amountCompleteRequired = prefabVizObject.amountCompleteRequired;
            prefabIdentifier = prefabVizObject.prefabIdentifier;
            prefabPriority = prefabVizObject.prefabPriority;
            stationPrefab = Plugin.GatherPrefabDump(prefabVizObject.stationPrefab);
            stationPrefab_offset = prefabVizObject.stationPrefab_offset;
            stationPrefab_orientation = prefabVizObject.stationPrefab_orientation;
            prefabIdentifierHash = prefabVizObject.prefabIdentifierHash;
        }
    }
}
