﻿namespace DaLion.Stardew.Rings.Framework.Events;

#region using directives

using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Wrapper for <see cref="IGameLoopEvents.AssetRequested"/> that can be hooked or unhooked.</summary>
[UsedImplicitly]
internal class AssetRequestedEvent : IEvent
{
    /// <inheritdoc />
    public void Hook()
    {
        ModEntry.ModHelper.Events.Content.AssetRequested += OnAssetRequested;
        Log.D("[Rings] Hooked AssetRequested event.");
    }

    /// <inheritdoc />
    public void Unhook()
    {
        ModEntry.ModHelper.Events.Content.AssetRequested -= OnAssetRequested;
        Log.D("[Rings] Unhooked AssetRequested event.");
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo($"{ModEntry.Manifest.UniqueID}/Rings"))
        {
            e.LoadFromModFile<Texture2D>("assets/rings.png", AssetLoadPriority.Medium);
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                string[] fields;
                if (ModEntry.Config.ImmersiveGlowstoneRecipe)
                {
                    fields = data["Glowstone Ring"].Split('/');
                    fields[0] = "517 1 519 1 768 20 769 20";
                    data["Glowstone Ring"] = string.Join('/', fields);
                }

                if (ModEntry.Config.CraftableGlowAndMagnetRings)
                {
                    data["Glow Ring"] = "516 2 768 10/Home/517/Ring/Mining 2";
                    data["Magnet Ring"] = "518 2 769 10/Home/519/Ring/Mining 2";
                }

                if (ModEntry.Config.CraftableGemRings)
                {
                    data["Amethyst Ring"] = "66 1 334 5/Home/529/Ring/Combat 2";
                    data["Topaz Ring"] = "68 1 334 5/Home/530/Ring/Combat 2";
                    data["Aquamarine Ring"] = "62 1 335 5/Home/531/Ring/Combat 4";
                    data["Jade Ring"] = "70 1 335 5/Home/532/Ring/Combat 4";
                    data["Emerald Ring"] = "60 1 336 5/Home/533/Ring/Combat 6";
                    data["Ruby Ring"] = "64 1 336 5/Home/534/Ring/Combat 6";
                }

                if (ModEntry.Config.ForgeableIridiumBand)
                {
                    fields = data["Iridium Band"].Split('/');
                    fields[0] = "337 5 768 100 769 100";
                    data["Iridium Band"] = string.Join('/', fields);
                }
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<int, string>().Data;
                string[] fields;

                if (ModEntry.Config.RebalancedRings)
                {
                    fields = data[Constants.TOPAZ_RING_INDEX_I].Split('/');
                    fields[5] = ModEntry.ModHelper.Translation.Get("rings.topaz");
                    data[Constants.TOPAZ_RING_INDEX_I] = string.Join('/', fields);

                    fields = data[Constants.JADE_RING_INDEX_I].Split('/');
                    fields[5] = ModEntry.ModHelper.Translation.Get("rings.jade");
                    data[Constants.JADE_RING_INDEX_I] = string.Join('/', fields);
                }

                if (ModEntry.Config.ForgeableIridiumBand)
                {
                    fields = data[Constants.IRIDIUM_BAND_INDEX_I].Split('/');
                    fields[5] = ModEntry.ModHelper.Translation.Get("rings.iridium");
                    data[Constants.IRIDIUM_BAND_INDEX_I] = string.Join('/', fields);
                }
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Maps/springobjects"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                Rectangle srcArea, targetArea;

                var rings = ModEntry.ModHelper.GameContent.Load<Texture2D>($"{ModEntry.Manifest.UniqueID}/Rings");
                if (ModEntry.Config.CraftableGemRings)
                {
                    srcArea = new(18, 0, 88, 12);
                    targetArea = new(21, 353, 88, 12);
                    editor.PatchImage(rings, srcArea, targetArea);
                }

                if (ModEntry.Config.ForgeableIridiumBand)
                {
                    srcArea = new(0, 2, 12, 12);
                    targetArea = new(371, 339, 12, 12);
                    editor.PatchImage(rings, srcArea, targetArea);
                }
            });
        }
    }
}