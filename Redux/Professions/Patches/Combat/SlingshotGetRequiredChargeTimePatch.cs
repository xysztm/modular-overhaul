﻿namespace DaLion.Redux.Professions.Patches.Combat;

#region using directives

using DaLion.Redux.Professions.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class SlingshotGetRequiredChargeTimePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="SlingshotGetRequiredChargeTimePatch"/> class.</summary>
    internal SlingshotGetRequiredChargeTimePatch()
    {
        this.Target = this.RequireMethod<Slingshot>(nameof(Slingshot.GetRequiredChargeTime));
        this.Postfix!.after = new[] { ReduxModule.Arsenal.Name };
    }

    #region harmony patches

    /// <summary>Patch to reduce Slingshot charge time for Desperado.</summary>
    [HarmonyPostfix]
    [HarmonyBefore("DaLion.Redux.Arsenal")]
    private static void SlingshotGetRequiredChargeTimePostfix(Slingshot __instance, ref float __result)
    {
        var firer = __instance.getLastFarmerToUse();
        if (!firer.IsLocalPlayer || !firer.HasProfession(Profession.Desperado))
        {
            return;
        }

        __result *= 1f - MathHelper.Lerp(0f, 0.5f, (float)firer.health / firer.maxHealth);
    }

    #endregion harmony patches
}