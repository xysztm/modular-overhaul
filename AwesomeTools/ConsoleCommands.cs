﻿namespace DaLion.Stardew.Tools;

#region using directives

using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

#endregion using directives

internal static class ConsoleCommands
{
    internal static void Register(IModHelper helper)
    {
        helper.ConsoleCommands.Add("player_upgradetools",
            "Set the upgrade level of all upgradeable tools in the player's inventory." + GetUpgradeToolsUsage(),
            UpgradeTools);

        helper.ConsoleCommands.Add("tool_addenchantment",
            "Add the specified enchantment to the player's current tool." + GetAddEnchantmentUsage(),
            AddEnchantment);
    }

    #region command handlers

    /// <summary>Set the upgrade level of all upgradeable tools in the player's inventory.</summary>
    /// <param name="command">The console command.</param>
    /// <param name="args">The supplied arguments.</param>
    private static void UpgradeTools(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (args.Length < 1)
        {
            Log.W("Missing argument." + GetUpgradeToolsUsage());
            return;
        }

        if (!Enum.TryParse<Framework.UpgradeLevel>(args[0], true, out var upgradeLevel))
        {
            Log.W("Invalid argument." + GetUpgradeToolsUsage());
            return;
        }

        if (upgradeLevel > Framework.UpgradeLevel.Iridium && !ModEntry.HasMoonMod)
        {
            Log.W("You must have 'Moon Misadventures' mod installed to set this upgrade level.");
            return;
        }

        foreach (var item in Game1.player.Items)
            if (item is Axe or Hoe or Pickaxe or WateringCan)
                (item as Tool).UpgradeLevel = (int) upgradeLevel;

        Log.I($"Upgraded all tools to {upgradeLevel}.");
    }

    /// <summary>Add the specified enchantment to the player's current tool.</summary>
    /// <param name="command">The console command.</param>
    /// <param name="args">The supplied arguments.</param>
    private static void AddEnchantment(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var tool = Game1.player.CurrentTool;
        if (tool is null)
        {
            Log.W("You must select a tool first.");
            return;
        }

        BaseEnchantment enchantment = args[0].ToLower() switch
        {
            // weapon enchants
            "artful" => new ArchaeologistEnchantment(),
            "bugkiller" => new BugKillerEnchantment(),
            "crusader" => new CrusaderEnchantment(),
            "vampiric" => new VampiricEnchantment(),
            "haymaker" => new HaymakerEnchantment(),
            "magic" or "starburst" => new MagicEnchantment(), // not implemented
            // tool enchants
            "auto-hook" or "autohook" => new AutoHookEnchantment(),
            "archaeologist" => new ArchaeologistEnchantment(),
            "bottomless" => new BottomlessEnchantment(),
            "efficient" => new EfficientToolEnchantment(),
            "generous" => new GenerousEnchantment(),
            "master" => new MasterEnchantment(),
            "powerful" => new PowerfulEnchantment(),
            "preserving" => new PreservingEnchantment(),
            "reaching" => new ReachingToolEnchantment(),
            "shaving" => new ShavingEnchantment(),
            "swift" => new SwiftToolEnchantment(),
            _ => null
        };

        if (enchantment is null)
        {
            Log.W($"Unknown enchantment type {args[0]}. Please enter a valid enchantment.");
            return;
        }

        if (!enchantment.CanApplyTo(tool))
        {
            Log.W($"Cannot apply {enchantment.GetDisplayName()} enchantment to {tool.DisplayName}.");
            return;
        }

        tool.enchantments.Add(enchantment);
        Log.I($"Applied {enchantment.GetDisplayName()} enchantment to {tool.DisplayName}.");
    }

    #endregion command handlers

    #region private methods

    /// <summary>Tell the dummies how to use the console command.</summary>
    private static string GetUpgradeToolsUsage()
    {
        var result = "\n\nUsage: player_upgradetools <level>";
        result += "\n\nParameters:";
        result += "\n\t- <level>: one of 'copper', 'steel', 'gold', 'iridium'";
        if (ModEntry.HasMoonMod)
            result += ", 'radioactive', 'mythicite'";

        result += "\n\nExample:";
        result += "\n\t- player_upgradetools iridium";
        return result;
    }

    /// <summary>Tell the dummies how to use the console command.</summary>
    private static string GetAddEnchantmentUsage()
    {
        var result = "\n\nUsage: tool_addenchantment <enchantment>";
        result += "\n\nParameters:";
        result += "\n\t- <enchantment>: a tool enchantment";
        result += "\n\nExample:";
        result += "\n\t- tool_addenchantment powerful";
        return result;
    }

    #endregion private methods
}