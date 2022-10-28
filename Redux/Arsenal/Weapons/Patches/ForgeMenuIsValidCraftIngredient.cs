﻿namespace DaLion.Redux.Arsenal.Weapons.Patches;

#region using directives

using DaLion.Redux.Arsenal.Weapons.Extensions;
using HarmonyLib;
using StardewValley.Menus;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class ForgeMenuIsValidCraftIngredient : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="ForgeMenuIsValidCraftIngredient"/> class.</summary>
    internal ForgeMenuIsValidCraftIngredient()
    {
        this.Target = this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.IsValidCraftIngredient));
    }

    #region harmony patches

    /// <summary>Allow forging with Hero Soul.</summary>
    [HarmonyPostfix]
    private static void ForgeMenuIsValidCraftIngredientPostfix(ref bool __result, Item item)
    {
        if (item.IsHeroSoul())
        {
            __result = true;
        }
    }

    #endregion harmony patches
}
