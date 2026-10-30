using Game.Effects;
using Game.RunEncounters;
using Game.UI.RunEncounters;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The narrated event results: placeholders, optional [sentences], rounding and the summary line.
    /// </summary>
    public class EventResultFormatterTests
    {
        private static readonly EventSummaryColors Colors = new()
        {
            positive = "00FF00",
            negative = "FF0000",
            grace = "00FFFF",
            favors = "FFD700"
        };

        private static Effect Item(string name, EffectQuality quality)
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            effect.effectName = name;
            effect.Quality = quality;
            return effect;
        }

        private static List<EventActionResult> Results(params EventActionResult[] results) => new(results);

        [Test]
        public void Placeholders_TakeAbsoluteAmounts()
        {
            var results = Results(
                EventActionResult.Of(EventResultKind.Health, -7.6f),
                EventActionResult.Of(EventResultKind.Grace, 3f),
                EventActionResult.Of(EventResultKind.Chronos, -25f),
                EventActionResult.Of(EventResultKind.Favors, 1f));
            Assert.AreEqual("Lost 8 health, 3 Grace, 25 Golden Chronos and 1 Favor.",
                EventResultFormatter.Format("Lost {health} health, {grace} Grace, {chronos} Golden Chronos and {favors} Favor.", results));
        }

        [Test]
        public void Item_IsItsNameInQualityColor()
        {
            var sword = Item("Old Blade", EffectQuality.Rare);
            string text = EventResultFormatter.Format("You found {item}.", Results(EventActionResult.Item(sword)));
            Assert.AreEqual($"You found {EffectQualityDisplayHelper.ColorizeName(sword)}.", text);
        }

        [Test]
        public void OptionalSentence_ShowsWhenItsActionGaveSomething()
        {
            var sword = Item("Old Blade", EffectQuality.Common);
            string text = EventResultFormatter.Format(
                "You lose only {health} health, [and a saddlebag holds {item}.]",
                Results(EventActionResult.Of(EventResultKind.Health, -4f), EventActionResult.Item(sword)));
            Assert.AreEqual($"You lose only 4 health, and a saddlebag holds {EffectQualityDisplayHelper.ColorizeName(sword)}.", text);
        }

        [Test]
        public void OptionalSentence_IsDropped_AndTheDanglingCommaTakesItsPeriod()
        {
            string text = EventResultFormatter.Format(
                "You lose only {health} health, [and a saddlebag holds {item}.]",
                Results(EventActionResult.Of(EventResultKind.Health, -4f), EventActionResult.Item(null)));
            Assert.AreEqual("You lose only 4 health.", text);

            text = EventResultFormatter.Format(
                "Inside are notes, a letter never sent, [and {item}.] The ink has run. +{grace} Grace.",
                Results(EventActionResult.Item(null), EventActionResult.Of(EventResultKind.Grace, 2f)));
            Assert.AreEqual("Inside are notes, a letter never sent. The ink has run. +2 Grace.", text);
        }

        [Test]
        public void OptionalSentence_WithoutPlaceholders_AlwaysShows()
        {
            Assert.AreEqual("Hello there.", EventResultFormatter.Format("Hello [there].", Results()));
        }

        [Test]
        public void DisplayAmount_RoundsAndShowsAtLeastOne()
        {
            Assert.AreEqual(0, EventResultFormatter.DisplayAmount(0f));
            Assert.AreEqual(1, EventResultFormatter.DisplayAmount(-0.2f));
            Assert.AreEqual(3, EventResultFormatter.DisplayAmount(2.5f));
            Assert.AreEqual(12, EventResultFormatter.DisplayAmount(-12.4f));
        }

        [Test]
        public void Summary_TotalsPerKind_InOrder_WithColors()
        {
            var sword = Item("Old Blade", EffectQuality.Rare);
            string summary = EventResultFormatter.BuildSummary(Results(
                EventActionResult.Of(EventResultKind.Chronos, -20f),
                EventActionResult.Of(EventResultKind.Health, -8f),
                EventActionResult.Of(EventResultKind.Grace, 3f),
                EventActionResult.Of(EventResultKind.Favors, 1f),
                EventActionResult.Item(sword)), Colors);

            string sep = EventResultFormatter.SummarySeparator;
            Assert.AreEqual(
                "<color=#FF0000>-8 health</color>" + sep +
                "<color=#00FFFF>+3 Grace</color>" + sep +
                "<color=#FF0000>-20 Golden Chronos</color>" + sep +
                "<color=#FFD700>+1 Favor</color>" + sep +
                EffectQualityDisplayHelper.ColorizeName(sword), summary);
        }

        [Test]
        public void Summary_SkipsWhatDidntChange_AndLostGraceIsNegative()
        {
            string summary = EventResultFormatter.BuildSummary(Results(
                EventActionResult.Of(EventResultKind.Grace, -4f),
                EventActionResult.Of(EventResultKind.Health, 0f),
                EventActionResult.Item(null)), Colors);
            Assert.AreEqual("<color=#FF0000>-4 Grace</color>", summary);
            Assert.AreEqual(string.Empty, EventResultFormatter.BuildSummary(Results(), Colors));
        }

        [Test]
        public void ResultText_IsTheNarrationThenTheSummary()
        {
            var resolution = new EventResolution();
            resolution.parts.Add(new EventNarration { text = "'A smith,' he says.", results = Results() });
            resolution.parts.Add(new EventNarration
            {
                text = "They beat you: {health} health.",
                results = Results(EventActionResult.Of(EventResultKind.Health, -20f))
            });

            Assert.AreEqual("'A smith,' he says.\n\nThey beat you: 20 health.\n\n<color=#FF0000>-20 health</color>",
                RunEncounterResultUI.BuildEventResultText(resolution, Colors));
        }

        [Test]
        public void ResultText_WithoutNarration_IsTheSummaryOnly()
        {
            var resolution = new EventResolution();
            resolution.parts.Add(new EventNarration { results = Results(EventActionResult.Of(EventResultKind.Chronos, 40f)) });
            Assert.AreEqual("<color=#00FF00>+40 Golden Chronos</color>", RunEncounterResultUI.BuildEventResultText(resolution, Colors));
        }

        [Test]
        public void PlaceholdersIn_FindsEveryKind()
        {
            CollectionAssert.AreEquivalent(new[] { EventResultKind.Health, EventResultKind.Item },
                EventResultFormatter.PlaceholdersIn("Lost {health} health, [found {item}.]"));
        }
    }
}
