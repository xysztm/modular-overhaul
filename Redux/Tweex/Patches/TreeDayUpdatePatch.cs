﻿namespace DaLion.Redux.Tweex.Patches;

#region using directives

using DaLion.Redux.Tweex.Extensions;
using DaLion.Shared.Extensions.Stardew;
using HarmonyLib;
using StardewValley.TerrainFeatures;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class TreeDayUpdatePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="TreeDayUpdatePatch"/> class.</summary>
    internal TreeDayUpdatePatch()
    {
        this.Target = this.RequireMethod<Tree>(nameof(Tree.dayUpdate));
    }

    #region harmony patches

    /// <summary>Ages tapper trees.</summary>
    [HarmonyPostfix]
    private static void TreeDayUpdatePostfix(Tree __instance)
    {
        if (__instance.growthStage.Value >= Tree.treeStage && __instance.CanBeTapped() &&
            ModEntry.Config.Tweex.AgeImprovesTreeSap)
        {
            __instance.Increment(DataFields.Age);
        }
    }

    #endregion harmony patches
}
