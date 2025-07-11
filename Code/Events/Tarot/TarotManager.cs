﻿using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Events.Tarot;

internal static class TarotManager
{
    internal const string TarotAssetPath = "SunberryTeam.SBV/Tarot";
    internal const string TarotRequiredEventId = "skellady.SBVCP_20031411";
    internal static Texture2D TarotBuffIcons;

    #region Card Pool

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
    static List<TarotCard> m_CardPool = null;
    public static List<TarotCard> CardPool
    {
        get
        {
            if (m_CardPool == null)
                m_CardPool = CreateCardPool;

            return m_CardPool;
        }
    }
    static List<TarotCard> CreateCardPool =>
   [
       new(
            id: "AceOfSwords",
            buffEffects: new() { Attack = { 3 } },
            iconIndex: 2,
            condition: null),
        new(
            id: "Sun",
            buffId: "28",	// Squid ink ravioli buff - can't be debuffed
			iconIndex: 3,
            condition: Game1.player.isMarriedOrRoommates),
        new(
            id: "ThreeOfWands",
            buffEffects: new() { MiningLevel = { 3 } },
            iconIndex: 4,
            condition: null),
        new(
            id: "AceOfCups",
            buffEffects: new() { MagneticRadius = { 75 } },
            iconIndex: 5,
            condition: null),
        new(
            id: "Empress",
            buffEffects : new() { Speed = { 1 } },
            iconIndex: 6,
            condition: null),
        new(
            id: "Moon",
            buffEffects: new() { Speed = { -1 } },
            iconIndex: 7,
            condition: null),
        new(
            id: "Lovers",
            buffEffects: new() { MaxStamina = { 50 } },
            iconIndex: 8,
            condition: () => Game1.player.isMarriedOrRoommates() || Game1.player.isEngaged()),
        new(
            id: "TowerReversed",
            buffId: "26",	// darkness - would like to implement a custom effect but this is fine for now
			iconIndex: 9,
            condition: () => Utils.Random.NextSingle() < 0.5f),	// lowered chance for this one to show up bc it's kind of a pain
		new(
            id: "AceOfPentacles",
            buffEffects: new() { ForagingLevel = { 3 } },
            iconIndex: 10,
            condition: null),
        new(
            id: "ThreeOfSwordsReversed",
            buffEffects: new() { Defense = { 3 } },
            iconIndex: 11,
            condition: null),
        new(
            id: "WheelOfFortune",
            buffEffects: new() { LuckLevel = { 3 } },
            iconIndex: 12,
            condition: null),
        new(
            id: "Fool",
            buffEffects: new() { LuckLevel = { 1 } },
            iconIndex: 12,
            condition: () => Game1.player.eventsSeen.Contains("JonghyukCoffee")),
        new(
            id: "Temperance",
            buffEffects: null,	// custom buff for Temperance
			iconIndex: 13,
            condition: () => Game1.player.eventsSeen.Contains("JonghyukCoffee")),
        new(
            id: "World",
            buffEffects: new() { FarmingLevel = { 1 }, MiningLevel = { 1 }, ForagingLevel = { 1 }, FishingLevel = { 1 }, CombatLevel = { 1 } },
            iconIndex: 14,
            condition: () => Game1.player.eventsSeen.Contains("JonghyukCoffee"))
    ];

    #endregion

    #region Logic

    public static List<TarotCard> GetAllTarotCards()
    {
        return CardPool.ToList();
    }

    public static List<TarotCard> GetAllTarotCardsWithConditionsMet()
    {
        return CardPool.Where(card => card.Condition is null || card.Condition()).ToList();
    }

    public static TarotCard GetTarotCardById(string id)
    {
        return CardPool.FirstOrDefault(card => card.Id == id);
    }

    #endregion

    #region Event Hooks

    /// <summary>
    /// Adds Tarot event hooks.
    /// </summary>
    public static void AddEventHooks()
    {
        Globals.EventHelper.Content.AssetRequested += Tarot_AssetRequested;
        //ready to load any asset
        TarotBuffIcons = Game1.content.Load<Texture2D>("SunberryTeam.SBV/Tarot/BuffIcons");

        // AssetInvalidated hook isn't necessary because nothing is cached, it's all loaded on demand
        // Init hook also isn't necessary for the same reason
        // At the end of each day, clears the mod data flag which prevents getting multiple tarot readings in one day.
        Globals.EventHelper.GameLoop.DayEnding += (_, _) => Game1.player.modData.Remove("SunberryTeam.SBVSMAPI_TarotReadingDoneForToday");
    }

    /// <summary>
    /// Provides default assets.
    /// </summary>
    private static void Tarot_AssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (!e.NameWithoutLocale.StartsWith(TarotAssetPath))
            return;

        if (e.NameWithoutLocale.IsEquivalentTo($"{TarotAssetPath}/CardBack"))
        {
            e.LoadFromModFile<Texture2D>("Assets/Tarot/cardBack.png", AssetLoadPriority.Medium);
        }
        else if (e.NameWithoutLocale.IsEquivalentTo($"{TarotAssetPath}/Event"))
        {
            e.LoadFrom(
                () => new Dictionary<string, string>
                {
                    ["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/SBVTarotCutscene/Cutscene SBVTarotCutscene/pause 1000/end"
                }, AssetLoadPriority.Low);
        }
        else if (e.NameWithoutLocale.StartsWith($"{TarotAssetPath}/Texture"))
        {
            string id = e.NameWithoutLocale.ToString()?.Replace($"{TarotAssetPath}/Texture/", "");
            e.LoadFromModFile<Texture2D>($"Assets/Tarot/{id}.png", AssetLoadPriority.Medium);
        }
        else if (e.NameWithoutLocale.StartsWith($"{TarotAssetPath}/BuffIcons"))
        {
            e.LoadFromModFile<Texture2D>($"Assets/Tarot/Buffs.png", AssetLoadPriority.Medium);
        }
    }

    #endregion
}