﻿namespace DaLion.Shared.Events;

#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DaLion.Shared.Attributes;
using DaLion.Shared.Extensions.Collections;
using HarmonyLib;
using StardewModdingAPI.Events;

#endregion using directives

/// <summary>
///     Instantiates and manages dynamic enabling and disabling of <see cref="IManagedEvent"/> classes in an
///     assembly or namespace.
/// </summary>
internal sealed class EventManager
{
    /// <summary>Gets the cached <see cref="IManagedEvent"/> instances by type.</summary>
    private readonly ConditionalWeakTable<Type, IManagedEvent> _eventCache = new();

    private readonly IModRegistry _modRegistry;

    /// <summary>Initializes a new instance of the <see cref="EventManager"/> class.</summary>
    /// <param name="modEvents">The <see cref="IModEvents"/> API for the current mod.</param>
    /// <param name="modRegistry">API for fetching metadata about loaded mods.</param>
    internal EventManager(IModEvents modEvents, IModRegistry modRegistry)
    {
        this._modRegistry = modRegistry;
        this.ModEvents = modEvents;
    }

    /// <inheritdoc cref="IModEvents"/>
    internal IModEvents ModEvents { get; }

    /// <summary>Gets an enumerable of all <see cref="IManagedEvent"/>s instances.</summary>
    internal IEnumerable<IManagedEvent> Managed => this._eventCache.Select(pair => pair.Value).AsEnumerable();

    /// <summary>Gets an enumerable of all <see cref="IManagedEvent"/>s currently enabled for the local player.</summary>
    internal IEnumerable<IManagedEvent> Enabled => this.Managed.Where(e => e.IsEnabled);

    /// <summary>Enumerates all <see cref="IManagedEvent"/>s currently enabled for the specified screen.</summary>
    /// <param name="screenId">The screen ID.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of enabled <see cref="IManagedEvent"/>s in the specified screen.</returns>
    internal IEnumerable<IManagedEvent> EnabledForScreen(int screenId)
    {
        return this.Managed.Where(e => e.IsEnabledForScreen(screenId));
    }

    /// <summary>Adds the <paramref name="event"/> instance to the cache.</summary>
    /// <param name="event">An <see cref="IManagedEvent"/> instance.</param>
    internal void Manage(IManagedEvent @event)
    {
        this._eventCache.Add(@event.GetType(), @event);
        Log.D($"[EventManager]: Now managing {@event.GetType().Name}.");
    }

    /// <summary>Instantiates one of each <see cref="IManagedEvent"/> instance to the set of <see cref="_eventCache"/>.</summary>
    /// <param name="namespace">An optional namespace within which to limit the scope of managed <see cref="IManagedEvent"/>s.</param>
    internal void ManageNamespace(string @namespace)
    {
        Log.D($"[EventManager]: Gathering events in {@namespace}...");
        var eventTypes = AccessTools
            .GetTypesFromAssembly(Assembly.GetAssembly(typeof(IManagedEvent)))
            .Where(t => t.IsAssignableTo(typeof(IManagedEvent)) && !t.IsAbstract &&
                        t.Namespace?.StartsWith(@namespace) == true &&
                        // event classes may or not have the required internal parameterized constructor accepting only the manager instance, depending on whether they are SMAPI or mod-handled
                        // we only want to construct SMAPI events at this point, so we filter out the rest
                        t.GetConstructor(
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null,
                            new[] { this.GetType() },
                            null) is not null &&
                        (t.GetCustomAttribute<AlwaysEnabledAttribute>() is not null ||
                         t.GetProperty(nameof(IManagedEvent.IsEnabled))?.DeclaringType == t))
            .ToArray();

        Log.D($"[EventManager]: Found {eventTypes.Length} event classes that should be enabled. Instantiating events...");
        foreach (var e in eventTypes)
        {
#if RELEASE
            var debugAttribute = e.GetCustomAttribute<DebugAttribute>();
            if (debugAttribute is not null) continue;
#endif

            var deprecatedAttr = e.GetCustomAttribute<DeprecatedAttribute>();
            if (deprecatedAttr is not null)
            {
                continue;
            }

            var integrationAttr = e.GetCustomAttribute<IntegrationAttribute>();
            if (integrationAttr is not null)
            {
                if (!this._modRegistry.IsLoaded(integrationAttr.UniqueId))
                {
                    Log.D(
                        $"[EventManager]: The target mod {integrationAttr.UniqueId} is not loaded. {e.Name} will be ignored.");
                    continue;
                }

                if (!string.IsNullOrEmpty(integrationAttr.Version) &&
                    this._modRegistry.Get(integrationAttr.UniqueId)!.Manifest.Version.IsOlderThan(
                        integrationAttr.Version))
                {
                    Log.W(
                        $"[EventManager]: The integration event {e.Name} will be ignored because the installed version of {integrationAttr.UniqueId} is older than minimum supported version." +
                        $" Please update {integrationAttr.UniqueId} in order to enable integrations with this mod.");
                    continue;
                }
            }

            var instance = this.CreateEventInstance(e);
            if (instance is null)
            {
                Log.E($"[EventManager]: Failed to create {e.Name}.");
                continue;
            }

            this._eventCache.Add(e, instance);
            Log.D($"[EventManager]: Now managing {e.Name}.");

            instance.Enable();
        }
    }

    /// <summary>Enable a single <see cref="IManagedEvent"/>.</summary>
    /// <param name="type">A <see cref="IManagedEvent"/> type to enable.</param>
    internal void Enable(Type type)
    {
        if (this.GetCachedEvent(type)?.Enable() == true)
        {
            Log.D($"[EventManager]: Enabled {type.Name}.");
            return;
        }

        Log.D($"[EventManager]: {type.Name} was not enabled.");
    }

    /// <summary>Enables the specified <see cref="IManagedEvent"/> types.</summary>
    /// <param name="eventTypes">The <see cref="IManagedEvent"/> types to enable.</param>
    internal void Enable(params Type[] eventTypes)
    {
        foreach (var type in eventTypes)
        {
            this.Enable(type);
        }
    }

    /// <summary>Enable a single <see cref="IManagedEvent"/>.</summary>
    /// <typeparam name="TEvent">AA <see cref="IManagedEvent"/> type to enable.</typeparam>
    internal void Enable<TEvent>()
        where TEvent : IManagedEvent
    {
        this.Enable(typeof(TEvent));
    }

    /// <summary>Enables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <param name="type">A <see cref="IManagedEvent"/> type to enable.</param>
    /// <param name="screenId">A local peer's screen ID.</param>
    internal void EnableForScreen(Type type, int screenId)
    {
        if (this.GetCachedEvent(type)?.EnableForScreen(screenId) == true)
        {
            Log.D($"[EventManager]: Enabled {type.Name}.");
            return;
        }

        Log.D($"[EventManager]: {type.Name} was not enabled.");
    }

    /// <summary>Enables the specified <see cref="IManagedEvent"/> types for the specified screen.</summary>
    /// <param name="screenId">A local peer's screen ID.</param>
    /// <param name="eventTypes">The <see cref="IManagedEvent"/> types to enable.</param>
    internal void EnableForScreen(int screenId, params Type[] eventTypes)
    {
        foreach (var type in eventTypes)
        {
            this.EnableForScreen(type, screenId);
        }
    }

    /// <summary>Enables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <typeparam name="TEvent">A <see cref="IManagedEvent"/> type to enable.</typeparam>
    /// <param name="screenId">A local peer's screen ID.</param>
    internal void EnableForScreen<TEvent>(int screenId)
        where TEvent : IManagedEvent
    {
        this.EnableForScreen(typeof(TEvent), screenId);
    }

    /// <summary>Enables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <param name="type">A <see cref="IManagedEvent"/> type to enable.</param>
    internal void EnableForAllScreens(Type type)
    {
        this.GetCachedEvent(type)?.EnableForAllScreens();
        Log.D($"[EventManager]: Enabled {type.Name} for all screens.");
    }

    /// <summary>Enables the specified <see cref="IManagedEvent"/> types for the specified screen.</summary>
    /// <param name="eventTypes">The <see cref="IManagedEvent"/> types to enable.</param>
    internal void EnableForAllScreens(params Type[] eventTypes)
    {
        foreach (var type in eventTypes)
        {
            this.EnableForAllScreens(type);
        }
    }

    /// <summary>Enables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <typeparam name="TEvent">An <see cref="IManagedEvent"/> type to enable.</typeparam>
    internal void EnableForAllScreens<TEvent>()
        where TEvent : IManagedEvent
    {
        this.EnableForAllScreens(typeof(TEvent));
    }

    /// <summary>Disables a single <see cref="IManagedEvent"/>.</summary>
    /// <param name="type">A <see cref="IManagedEvent"/> type to disable.</param>
    internal void Disable(Type type)
    {
        if (this.GetCachedEvent(type)?.Disable() == true)
        {
            Log.D($"[EventManager]: Disabled {type.Name}.");
            return;
        }

        Log.D($"[EventManager]: {type.Name} was not disabled.");
    }

    /// <summary>Disables the specified <see cref="IManagedEvent"/>s events.</summary>
    /// <param name="eventTypes">The <see cref="IManagedEvent"/> types to disable.</param>
    internal void Disable(params Type[] eventTypes)
    {
        foreach (var type in eventTypes)
        {
            this.Disable(type);
        }
    }

    /// <summary>Disables a single <see cref="IManagedEvent"/>.</summary>
    /// <typeparam name="TEvent">A <see cref="IManagedEvent"/> type to disable.</typeparam>
    internal void Disable<TEvent>()
        where TEvent : IManagedEvent
    {
        this.Disable(typeof(TEvent));
    }

    /// <summary>Disables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <param name="type">A <see cref="IManagedEvent"/> type to disable.</param>
    /// <param name="screenId">A local peer's screen ID.</param>
    internal void DisableForScreen(Type type, int screenId)
    {
        if (this.GetCachedEvent(type)?.DisableForScreen(screenId) == true)
        {
            Log.D($"[EventManager]: Disabled {type.Name}.");
            return;
        }

        Log.D($"[EventManager]: {type.Name} was not disabled.");
    }

    /// <summary>Disables the specified <see cref="IManagedEvent"/>s for the specified screen.</summary>
    /// <param name="screenId">A local peer's screen ID.</param>
    /// <param name="eventTypes">The <see cref="IManagedEvent"/> types to disable.</param>
    internal void DisableForScreen(int screenId, params Type[] eventTypes)
    {
        foreach (var type in eventTypes)
        {
            this.DisableForScreen(type, screenId);
        }
    }

    /// <summary>Disables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <typeparam name="TEvent">An <see cref="IManagedEvent"/> type to disable.</typeparam>
    /// <param name="screenId">A local peer's screen ID.</param>
    internal void DisableForScreen<TEvent>(int screenId)
        where TEvent : IManagedEvent
    {
        this.DisableForScreen(typeof(TEvent), screenId);
    }

    /// <summary>Disables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <param name="type">A <see cref="IManagedEvent"/> type to disable.</param>
    internal void DisableForAllScreens(Type type)
    {
        this.GetCachedEvent(type)?.DisableForAllScreens();
        Log.D($"[EventManager]: Enabled {type.Name} for all screens.");
    }

    /// <summary>Disables the specified <see cref="IManagedEvent"/>s for the specified screen.</summary>
    /// <param name="eventTypes">The <see cref="IManagedEvent"/> types to disable.</param>
    internal void DisableForAllScreens(params Type[] eventTypes)
    {
        foreach (var type in eventTypes)
        {
            this.DisableForAllScreens(type);
        }
    }

    /// <summary>Disables a single <see cref="IManagedEvent"/> for the specified screen.</summary>
    /// <typeparam name="TEvent">A <see cref="IManagedEvent"/> type to disable.</typeparam>
    internal void DisableForAllScreens<TEvent>()
        where TEvent : IManagedEvent
    {
        this.DisableForAllScreens(typeof(TEvent));
    }

    /// <summary>Enables all <see cref="IManagedEvent"/>s.</summary>
    internal void EnableAll()
    {
        var count = this._eventCache.Count(pair => pair.Value.Enable());
        Log.D($"[EventManager]: Enabled {count} events.");
    }

    /// <summary>Disables all <see cref="IManagedEvent"/>s.</summary>
    internal void DisableAll()
    {
        var count = this._eventCache.Count(pair => pair.Value.Disable());
        Log.D($"[EventManager]: Disabled {count} events.");
    }

    /// <summary>Enables all <see cref="IManagedEvent"/> types starting with attribute <typeparamref name="TAttribute"/>.</summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    internal void EnableWithAttribute<TAttribute>()
        where TAttribute : Attribute
    {
        var count = this._eventCache
            .Where(pair => pair.Key.GetCustomAttribute<TAttribute>() is not null)
            .Count(pair => pair.Value.Enable());
        Log.D($"[EventManager]: Enabled {count} events.");
    }

    /// <summary>Disables all <see cref="IManagedEvent"/> types starting with attribute <typeparamref name="TAttribute"/>.</summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    internal void DisableWithAttribute<TAttribute>()
        where TAttribute : Attribute
    {
        var count = this._eventCache
            .Where(pair => pair.Key.GetCustomAttribute<TAttribute>() is not null)
            .Count(pair => pair.Value.Disable());
        Log.D($"[EventManager]: Disabled {count} events.");
    }

    /// <summary>Resets the enabled status of all <see cref="IManagedEvent"/>s in the assembly for the current screen.</summary>
    internal void Reset()
    {
        this._eventCache.ForEach(pair => pair.Value.Reset());
        Log.D("[EventManager]: Reset all managed events for the current screen.");
    }

    /// <summary>Resets the enabled status of all <see cref="IManagedEvent"/>s in the assembly for all screens.</summary>
    internal void ResetForAllScreens()
    {
        this._eventCache.ForEach(pair => pair.Value.ResetForAllScreens());
        Log.D("[EventManager]: Reset all managed events for all screens.");
    }

    /// <summary>Gets the instance of the specified <see cref="IManagedEvent"/> type.</summary>
    /// <typeparam name="TEvent">A type implementing <see cref="IManagedEvent"/>.</typeparam>
    /// <returns>The instance of type <typeparamref name="TEvent"/>.</returns>
    internal TEvent? Get<TEvent>()
        where TEvent : IManagedEvent
    {
        return this._eventCache.TryGetValue(typeof(TEvent), out var instance)
            ? (TEvent)instance
            : (TEvent?)this.CreateEventInstance(typeof(TEvent));
    }

    /// <summary>Determines whether the specified <see cref="IManagedEvent"/> type is enabled.</summary>
    /// <typeparam name="TEvent">A type implementing <see cref="IManagedEvent"/>.</typeparam>
    /// <returns><see langword="true"/> if the <see cref="IManagedEvent"/> is enabled for the local screen, otherwise <see langword="false"/>.</returns>
    internal bool IsEnabled<TEvent>()
        where TEvent : IManagedEvent
    {
        return this.Get<TEvent>()?.IsEnabled == true;
    }

    /// <summary>Determines whether the specified <see cref="IManagedEvent"/> type is enabled for a specific screen.</summary>
    /// <typeparam name="TEvent">A type implementing <see cref="IManagedEvent"/>.</typeparam>
    /// <param name="screenId">The screen ID.</param>
    /// <returns><see langword="true"/> if the <see cref="IManagedEvent"/> is enabled for the specified screen, otherwise <see langword="false"/>.</returns>
    internal bool IsEnabledForScreen<TEvent>(int screenId)
        where TEvent : IManagedEvent
    {
        return this.Get<TEvent>()?.IsEnabledForScreen(screenId) == true;
    }

    /// <summary>Retrieves an existing event instance from the cache, or caches a new instance.</summary>
    /// <param name="type">A type implementing <see cref="IManagedEvent"/>.</param>
    /// <returns>The cached <see cref="IManagedEvent"/> instance, or <see langword="null"/> if one could not be created.</returns>
    private IManagedEvent? GetCachedEvent(Type type)
    {
        if (!this._eventCache.TryGetValue(type, out var instance))
        {
            instance = this.CreateEventInstance(type);
            if (instance is null)
            {
                return null;
            }

            this._eventCache.Add(type, instance);
            Log.D($"[EventManager]: Now managing {type.Name}.");
        }

        return instance;
    }

    /// <summary>Instantiates a new <see cref="IManagedEvent"/> instance of the specified <paramref name="type"/>.</summary>
    /// <param name="type">A type implementing <see cref="IManagedEvent"/>.</param>
    /// <returns>A <see cref="IManagedEvent"/> instance of the specified <paramref name="type"/>.</returns>

    private IManagedEvent? CreateEventInstance(Type type)
    {
        if (!type.IsAssignableTo(typeof(IManagedEvent)) || type.IsAbstract || type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { this.GetType() },
                null) is null)
        {
            Log.E($"[EventManager]: {type.Name} is not a valid event type.");
            return null;
        }

#if RELEASE
            var debugAttribute = type.GetCustomAttribute<DebugAttribute>();
            if (debugAttribute is not null) return null;
#endif

        var deprecatedAttr = type.GetCustomAttribute<DeprecatedAttribute>();
        if (deprecatedAttr is not null)
        {
            Log.D($"[EventManager]: {type.Name} is deprecated.");
            return null;
        }

        var integrationAttr = type.GetCustomAttribute<IntegrationAttribute>();
        if (integrationAttr is not null)
        {
            if (!this._modRegistry.IsLoaded(integrationAttr.UniqueId))
            {
                Log.D(
                    $"[EventManager]: The target mod {integrationAttr.UniqueId} is not loaded. {type.Name} will be ignored.");
                return null;
            }

            if (!string.IsNullOrEmpty(integrationAttr.Version) &&
                this._modRegistry.Get(integrationAttr.UniqueId)!.Manifest.Version.IsOlderThan(
                    integrationAttr.Version))
            {
                Log.W(
                    $"[EventManager]: The integration event {type.Name} will be ignored because the installed version of {integrationAttr.UniqueId} is older than minimum supported version." +
                    $" Please update {integrationAttr.UniqueId} in order to enable integrations with this mod.");
                return null;
            }
        }

        return (IManagedEvent)type
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { this.GetType() },
                null)!
            .Invoke(new object?[] { this });
    }
}
