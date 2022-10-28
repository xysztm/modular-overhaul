﻿namespace DaLion.Redux.Core.Events;

#region using directives

using DaLion.Shared.Events;
using StardewModdingAPI.Events;

#endregion using directives

[UsedImplicitly]
[AlwaysEnabled]
internal sealed class CoreDayEndingEvent : DayEndingEvent
{
    /// <summary>Initializes a new instance of the <see cref="CoreDayEndingEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    internal CoreDayEndingEvent(EventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc />
    protected override void OnDayEndingImpl(object? sender, DayEndingEventArgs e)
    {
        ModEntry.Reflector.NewCacheInterval();
    }
}
