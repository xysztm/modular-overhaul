﻿namespace DaLion.Redux.Professions.Events.Input;

#region using directives

using DaLion.Shared.Events;
using StardewModdingAPI.Events;
using StardewValley.Tools;

#endregion using directives

[UsedImplicitly]
internal sealed class RascalButtonPressedEvent : ButtonPressedEvent
{
    /// <summary>Initializes a new instance of the <see cref="RascalButtonPressedEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    internal RascalButtonPressedEvent(EventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc />
    protected override void OnButtonPressedImpl(object? sender, ButtonPressedEventArgs e)
    {
        var player = Game1.player;
        if (Game1.activeClickableMenu is not null || player.CurrentTool is not Slingshot || !player.CanMove ||
            player.canOnlyWalk || player.isRidingHorse() || player.onBridge.Value || player.usingSlingshot)
        {
            return;
        }

        if (e.Button.IsActionButton())
        {
            ModEntry.State.Professions.UsingSecondaryAmmo = true;
            Game1.player.BeginUsingTool();
            Log.D("Charging secondary ammo!");
        }
        else if (e.Button.IsUseToolButton())
        {
            ModEntry.State.Professions.UsingPrimaryAmmo = true;
            Log.D("Charging primary ammo!");
        }
    }
}
