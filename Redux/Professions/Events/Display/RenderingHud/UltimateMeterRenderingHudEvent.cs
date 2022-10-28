﻿namespace DaLion.Redux.Professions.Events.Display;

#region using directives

using DaLion.Redux.Professions.Ultimates;
using DaLion.Redux.Professions.VirtualProperties;
using DaLion.Shared.Events;
using StardewModdingAPI.Events;

#endregion using directives

[UltimateEvent]
[UsedImplicitly]
internal sealed class UltimateMeterRenderingHudEvent : RenderingHudEvent
{
    /// <summary>Initializes a new instance of the <see cref="UltimateMeterRenderingHudEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    internal UltimateMeterRenderingHudEvent(EventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc />
    protected override void OnRenderingHudImpl(object? sender, RenderingHudEventArgs e)
    {
        var ultimate = Game1.player.Get_Ultimate();
        if (ultimate is null)
        {
            this.Disable();
            return;
        }

        ultimate.Hud.Draw(e.SpriteBatch);
    }
}
