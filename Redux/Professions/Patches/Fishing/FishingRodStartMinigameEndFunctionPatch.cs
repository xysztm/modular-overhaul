﻿namespace DaLion.Redux.Professions.Patches.Fishing;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Redux.Professions.Extensions;
using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class FishingRodStartMinigameEndFunctionPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="FishingRodStartMinigameEndFunctionPatch"/> class.</summary>
    internal FishingRodStartMinigameEndFunctionPatch()
    {
        this.Target = this.RequireMethod<FishingRod>(nameof(FishingRod.startMinigameEndFunction));
    }

    #region harmony patches

    /// <summary>Patch to remove Pirate bonus treasure chance.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? FishingRodStartMinigameEndFunctionTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // Removed: lastUser.professions.Contains(<pirate_id>) ? baseChance ...
        try
        {
            helper // find index of pirate check
                .FindProfessionCheck(Farmer.pirate)
                .Retreat(2)
                .RemoveInstructionsUntil(new CodeInstruction(OpCodes.Add)); // remove this check
        }
        catch (Exception ex)
        {
            Log.E($"Failed while removing vanilla Pirate bonus treasure chance.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
