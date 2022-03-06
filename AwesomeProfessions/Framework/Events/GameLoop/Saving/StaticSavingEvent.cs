﻿namespace DaLion.Stardew.Professions.Framework.Events.GameLoop;

#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

using Common.Extensions;
using Extensions;

#endregion using directives

[UsedImplicitly]
internal class StaticSavingEvent : SavingEvent
{
    /// <summary>Construct an instance.</summary>
    internal StaticSavingEvent()
    {
        Enable();
    }

    /// <inheritdoc />
    protected override void OnSavingImpl(object sender, SavingEventArgs e)
    {
        // clean rogue data
        Log.D("[ModData]: Checking for rogue data fields...");
        var data = Game1.player.modData;
        var count = 0;
        if (!Context.IsMainPlayer)
            for (var i = data.Keys.Count() - 1; i >= 0; --i)
            {
                var key = data.Keys.ElementAt(i);
                if (!key.StartsWith(ModEntry.Manifest.UniqueID)) continue;

                data.Remove(key);
                ++count;
            }
        else
            for (var i = data.Keys.Count() - 1; i >= 0; --i)
            {
                var key = data.Keys.ElementAt(i);
                if (!key.StartsWith(ModEntry.Manifest.UniqueID)) continue;

                var split = key.Split('/');
                if (split.Length != 3 || !split[1].TryParse<long>(out var id))
                {
                    data.Remove(key);
                    ++count;
                    continue;
                }

                var who = Game1.getFarmerMaybeOffline(id);
                if (who is null)
                {
                    data.Remove(key);
                    ++count;
                    continue;
                }

                if (!Enum.TryParse<DataField>(split[2], out var field))
                {
                    data.Remove(key);
                    ++count;
                    continue;
                }

                if (field >= DataField.ForgottenRecipesDict) continue;

                var profession = Enum.Parse<Profession>(field.ToString().SplitCamelCase()[0]);
                if (Game1.player.HasProfession(profession)) continue;

                data.Remove(key);
                ++count;
            }

        Log.D($"[ModData]: Found {count} rogue data fields.");

        // save fish pond quality data
        if (ModEntry.Config.EnableFishPondRebalance && Context.IsMainPlayer)
        {
            var pondQualityDict = new Dictionary<int, int>();
            var familyQualityDict = new Dictionary<int, int>();
            var familyCountDict = new Dictionary<int, int>();
            foreach (var pond in Game1.getFarm().buildings.OfType<FishPond>().Where(p => !p.isUnderConstruction()))
            {
                var qualityRating = pond.ReadDataAs<int>("QualityRating");
                var familyQualityRating = pond.ReadDataAs<int>("FamilyQualityRating");
                var familyCount = pond.ReadDataAs<int>("FamilyCount");
                var pondId = pond.GetCenterTile().ToString().GetDeterministicHashCode();
                pondQualityDict[pondId] = qualityRating;
                if (familyQualityRating > 0) familyQualityDict[pondId] = familyQualityRating;
                if (familyCount > 0) familyCountDict[pondId] = familyCount;
            }

            Game1.player.WriteData(DataField.FishPondQualityDict, pondQualityDict.ToString(',', ';'));
            Game1.player.WriteData(DataField.FishPondFamilyQualityDict, familyQualityDict.ToString(',', ';'));
            Game1.player.WriteData(DataField.FishPondFamilyCountDict, familyCountDict.ToString(',', ';'));
        }
    }
}