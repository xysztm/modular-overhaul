﻿namespace DaLion.Stardew.Rings.Framework.Patches;

#region using directives

using HarmonyLib;
using StardewValley.Objects;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class CraftingRecipeCtorPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="CraftingRecipeCtorPatch"/> class.</summary>
    internal CraftingRecipeCtorPatch()
    {
        this.Target = this.RequireConstructor<CraftingRecipe>(typeof(string), typeof(bool));
    }

    #region harmony patches

    /// <summary>Fix localized display name for custom ring recipes.</summary>
    [HarmonyPostfix]
    private static void CraftingRecipeCtorPrefix(CraftingRecipe __instance, string name, bool isCookingRecipe)
    {
        if (isCookingRecipe || !__instance.name.Contains("Ring") || LocalizedContentManager.CurrentLanguageCode ==
            LocalizedContentManager.LanguageCode.en)
        {
            return;
        }

        __instance.DisplayName = name switch
        {
            "Glow Ring" => new Ring(Constants.GlowRingIndex).DisplayName,
            "Magnet Ring" => new Ring(Constants.MagnetRingIndex).DisplayName,
            "Emerald Ring" => new Ring(Constants.EmeraldRingIndex).DisplayName,
            "Aquamarine Ring" => new Ring(Constants.AquamarineRingIndex).DisplayName,
            "Ruby Ring" => new Ring(Constants.RubyRingIndex).DisplayName,
            "Amethyst Ring" => new Ring(Constants.AmethystRingIndex).DisplayName,
            "Topaz Ring" => new Ring(Constants.TopazRingIndex).DisplayName,
            "Jade Ring" => new Ring(Constants.JadeRingIndex).DisplayName,
            "Garnet Ring" => new Ring(ModEntry.GarnetRingIndex).DisplayName,
            _ => __instance.DisplayName,
        };
    }

    #endregion harmony patches
}