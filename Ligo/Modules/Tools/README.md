<table align="center"><tr><td align="center" width="9999">

# ![](https://i.imgur.com/6sWaRit.png) Ligo :: Tools ![](https://i.imgur.com/4rYYYCD.png)

</td></tr></table>

## Features

This module is inspired by the tool progression system of old Harvest Moon: Friends of Mineral Town, where the Axe and Hammer tools were also chargeable, and their ultimate upgrades could destroy all debris on-screen.

<figure align="center" width="9999" id="fig1">
  <img src="resources/cover.gif" align="center" height="auto" width="80%" alt="Logo">
</figure>

This module provides three main features:
1. Allows the Axe and Pickaxe to be charged according to the tool's upgrade level.
2. Allows customizing the area of effect of the Hoe and Watering Can.
3. Extends certain tool enchantments, allowing them to be applied to other tools.
4. Causes the farmer to face the mouse cursor before using a tool.

All features can be toggled on or off.

## Resource Tools

Charging up the Axe or Pickaxe will release a shockwave which spreads the tool's effect around an area. The shape of the shockwave is similar to a bomb explosion, but the radius can be configured for each upgrade level.

Up to **seven** upgrade levels are supported, which includes the Reaching Enchantment and the two extra levels from [Moon Misadventures](https://www.nexusmods.com/stardewvalley/mods/10612).
All radius values should be positive whole numbers (obviously). By default, the radius at each levels is equal to the tool's upgrade level.

Like the Tractor Mod, what the shockwave actually does can also be configured. By default it is set to only clear debris (like stones and twigs), weeds, dead crops and resource clumps (like stumps, logs and boulders), as well as mining nodes. You can optionally choose to let them affect other objects or terrain features, such as trees, live crops, and flooring; anything their corresponding tools ordinarily can do.

## Farming Tools

The area of effect of Hoe and Watering Can may be customized by setting a length and radius for each upgrade level. Note that the radius adds to both side of the farmer, such that a radius of 1 yields an area 3 tiles wide.

## Enchantments

All tool enchantments are compatible. The Reaching Enchantment will work on chargeable resource tools as it ordinarily does for farming tools, increasing the maximum charge level by **one**. The Powerful Enchantment likewise continues to increase the power of resource tools, and that extends to every affected tile in the shockwave.

In addition, this module will allow the Swift Enchantment to be applied on the Watering Can, and the Master Enchantment to be applied on all tools, boosting the corresponding skill level by **one**.

## Configs

This section describes some of the configurable settings provided in configs.json:

- **'RequiredUpgradeLevelForCharging':** This is the minimum upgrade level your tool must be at in order to enable charging. Accepted values are "Copper", "Steel", "Gold", "Iridium", "Radioactive" and "Mythicite" (the last two require Mood Misadventures).
- **'RadiusAtEachLevel':**  Allows you to specify the shockwave radius at each charging level. Note that your charging level is separate from your upgrade level. For instance, if 'RequiredUpgradeLevelForCharging' is set to Iridium, and 'RadiusAtEachLevel' set to [ 1, 2, 3, 4 ], then you will not be able to charge until the tool is Iridium level, but once it is, then your charging progression will be similar to the gif above (starting at 1, and increase by 1 until 4). If you wanted to skip charging up and instantly get the max radius, you could set all four values to the same number (and set 'ShowAffectedTiles' to false to avoid the overlay instantly appearing). Only accepts positive integers.
- **'RequireModKey':** Set to false if you want charging behavior to be the default when holding down the tool button. Set to true if you prefer the vanilla tool spamming behavior.
- **'ModKey':** If 'RequireModKey' is true, you must hold this key in order to charge (default LeftShift). If you play with a gamepad controller you can set this to LeftTrigger or LeftShoulder. Check [here](https://stardewcommunitywiki.com/Modding:Player_Guide/Key_Bindings) for a list of available keybinds. You can set multiple comma-separated keys.
- **'HideAffectedTiles':** If enabled, will not display the green overlay showing the size of the shockwave.
- **'StaminaCostMultiplier':** By default, charging multiplies your tool's base stamina cost by the charging level. Use this multiplier to adjust the cost of the shockwave *only*. Set to zero to make it free (you will still lose stamina equal to the base tool cost). Accepts any real number greater than zero.
- **'TicksBetweenWaves':** The number of game ticks before the shockwave grows by 1 tile. Higher numbers cause the shockwave to travel slower. Setting this to 0 replicates the original behavior from older versions.
- Other settings are self explanatory. Use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) if you need verbatim explanations.

## Compatibility

The tools are compatible withThis mod uses Harmony to patch the behavior of Axe and Pickaxe. Any mods that also directly patch Tool behavior might be incompatible.

- Compatible with [Moon Misadventures](https://www.nexusmods.com/stardewvalley/mods/10612).
- Compatible with [Harvest Moon FoMT-like Watering Can And Hoe Area](https://www.nexusmods.com/stardewvalley/mods/7851) as long as you don't touch Hoe and Watering Can settings (although you can just set them to the same values used by that mod to achieve the same effect).
- Compatible with [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
- **Not** compatible with the likes of [Combat Controls - Fixed Mouse Click](https://www.nexusmods.com/stardewvalley/mods/2590) or [Combat Controls Ligo](https://www.nexusmods.com/stardewvalley/mods/10496), as those features are already included in this and other Ligo modules.

## Uninstallation

Safe to uninstall or disable without special measures.

## Special Thanks

- [Pathoschild](https://www.nexusmods.com/stardewvalley/users/1552317) for their [TractorMod](https://www.nexusmods.com/stardewvalley/mods/1401), from which this mod ~~steals~~ borrows much of its code.