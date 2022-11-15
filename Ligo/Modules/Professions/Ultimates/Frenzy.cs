﻿namespace DaLion.Ligo.Modules.Professions.Ultimates;

#region using directives

using DaLion.Ligo.Modules.Professions.Sounds;
using Microsoft.Xna.Framework;

#endregion using directives

/// <summary>Handles Brute ultimate activation.</summary>
public sealed class Frenzy : Ultimate
{
    internal const float PercentIncrementPerRage = 0.01f;

    /// <summary>Initializes a new instance of the <see cref="Frenzy"/> class.</summary>
    internal Frenzy()
        : base("Frenzy", 26, Color.OrangeRed, Color.OrangeRed)
    {
    }

    /// <inheritdoc />
    internal override int MillisecondsDuration =>
        (int)(15000 * ((double)this.MaxValue / BaseMaxValue) / ModEntry.Config.Professions.SpecialDrainFactor);

    /// <inheritdoc />
    internal override Sfx ActivationSfx => Sfx.BruteRage;

    /// <inheritdoc />
    internal override Color GlowColor => Color.OrangeRed;

    /// <inheritdoc />
    internal override void Activate()
    {
        base.Activate();

        ModEntry.State.Professions.BruteKillCounter = 0;

        Game1.buffsDisplay.removeOtherBuff(this.BuffId);
        Game1.buffsDisplay.addOtherBuff(
            new Buff(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                1,
                this.GetType().Name,
                this.DisplayName)
            {
                which = this.BuffId,
                sheetIndex = this.BuffSheetIndex,
                glow = this.GlowColor,
                millisecondsDuration = this.MillisecondsDuration,
                description = this.Description,
            });
    }

    /// <inheritdoc />
    internal override void Deactivate()
    {
        base.Deactivate();

        Game1.buffsDisplay.removeOtherBuff(this.BuffId);

        var who = Game1.player;
        var healed = (int)(who.maxHealth * ModEntry.State.Professions.BruteKillCounter * 0.05f);
        who.health = Math.Min(who.health + healed, who.maxHealth);
        who.currentLocation.debris.Add(new Debris(
            healed,
            new Vector2(who.getStandingX() + 8, who.getStandingY()),
            Color.Lime,
            1f,
            who));
    }

    /// <inheritdoc />
    internal override void Countdown()
    {
        this.ChargeValue -= this.MaxValue / 900d; // lasts 15s * 60 ticks/s -> 900 ticks
    }
}