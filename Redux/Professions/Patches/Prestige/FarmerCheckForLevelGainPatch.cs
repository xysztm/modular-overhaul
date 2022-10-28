﻿namespace DaLion.Redux.Professions.Patches.Prestige;

#region using directives

using HarmonyLib;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class FarmerCheckForLevelGainPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="FarmerCheckForLevelGainPatch"/> class.</summary>
    internal FarmerCheckForLevelGainPatch()
    {
        this.Target = this.RequireMethod<Farmer>(nameof(Farmer.checkForLevelGain));
    }

    #region harmony patches

    /// <summary>Patch to allow level increase up to 20.</summary>
    [HarmonyPostfix]
    private static void FarmerCheckForLevelGainPostfix(ref int __result, int oldXP, int newXP)
    {
        if (!ModEntry.Config.Professions.EnablePrestige)
        {
            return;
        }

        for (var i = 1; i <= 10; ++i)
        {
            var requiredExpForThisLevel = Constants.ExpAtLevel10 + (ModEntry.Config.Professions.RequiredExpPerExtendedLevel * i);
            if (oldXP >= requiredExpForThisLevel)
            {
                continue;
            }

            if (newXP < requiredExpForThisLevel)
            {
                return;
            }

            __result = i + 10;
        }
    }

    #endregion harmony patches
}
