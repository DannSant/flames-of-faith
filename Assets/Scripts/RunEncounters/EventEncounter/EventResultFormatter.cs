using Game.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Game.RunEncounters
{
    /// <summary>One narrated piece of an event's result: a branch's text and what its actions did.</summary>
    public class EventNarration
    {
        public string text;
        public List<EventActionResult> results = new();
    }

    /// <summary>Everything that happened along an event's chain, in order, for the Results window.</summary>
    public class EventResolution
    {
        public List<EventNarration> parts = new();

        public IEnumerable<EventActionResult> AllResults => parts.SelectMany(p => p.results);
    }

    /// <summary>The hex colors of the summary line (no '#').</summary>
    public struct EventSummaryColors
    {
        public string positive;
        public string negative;
        public string grace;
        public string favors;
    }

    /// <summary>
    /// Turns result texts into the narration and the summary line. Pure, so tests can run it.
    /// - Placeholders {health}, {grace}, {chronos}, {favors} and {item} take the absolute amount (or the item's name
    ///   in its quality color) from the first action of that kind.
    /// - A sentence in [square brackets] shows only if every placeholder in it gave something; otherwise it's
    ///   dropped, and a comma left dangling before it takes the sentence's closing punctuation.
    /// </summary>
    public static class EventResultFormatter
    {
        public const string SummarySeparator = "  /  ";

        private static readonly Regex OptionalSentence = new(@"\[([^\[\]]*)\]");
        private static readonly Regex AnyPlaceholder = new(@"\{(health|grace|chronos|favors|item)\}");

        public static string Placeholder(EventResultKind kind) => kind switch
        {
            EventResultKind.Health => "{health}",
            EventResultKind.Grace => "{grace}",
            EventResultKind.Chronos => "{chronos}",
            EventResultKind.Favors => "{favors}",
            _ => "{item}"
        };

        /// <summary>The kinds whose placeholders appear in a text (brackets included).</summary>
        public static HashSet<EventResultKind> PlaceholdersIn(string text)
        {
            var kinds = new HashSet<EventResultKind>();
            if (string.IsNullOrEmpty(text)) return kinds;
            foreach (Match m in AnyPlaceholder.Matches(text))
            {
                kinds.Add(KindOf(m.Groups[1].Value));
            }
            return kinds;
        }

        /// <summary>A rounded, absolute amount for the text: anything above 0 shows as at least 1.</summary>
        public static int DisplayAmount(float amount)
        {
            float magnitude = Math.Abs(amount);
            if (magnitude <= 0f) return 0;
            return Math.Max(1, (int)Math.Round(magnitude, MidpointRounding.AwayFromZero));
        }

        public static string Format(string text, IReadOnlyList<EventActionResult> results)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            results ??= Array.Empty<EventActionResult>();

            var sb = new StringBuilder();
            int last = 0;
            foreach (Match m in OptionalSentence.Matches(text))
            {
                sb.Append(text, last, m.Index - last);
                string sentence = m.Groups[1].Value;
                if (EveryPlaceholderGave(sentence, results))
                {
                    sb.Append(sentence);
                }
                else
                {
                    DropDanglingComma(sb, sentence);
                }
                last = m.Index + m.Length;
            }
            sb.Append(text, last, text.Length - last);

            string result = sb.ToString();
            result = Regex.Replace(result, @"[ \t]{2,}", " ");
            result = Regex.Replace(result, @"[ \t]+([,.;:!?])", "$1");
            result = AnyPlaceholder.Replace(result, m => ValueText(KindOf(m.Groups[1].Value), results));
            return result.Trim();
        }

        /// <summary>Every part's narration, in order, separated by blank lines.</summary>
        public static string FormatResolution(EventResolution resolution)
        {
            if (resolution == null) return string.Empty;
            var texts = resolution.parts
                .Select(p => Format(p.text, p.results))
                .Where(t => !string.IsNullOrEmpty(t));
            return string.Join("\n\n", texts);
        }

        /// <summary>
        /// "-8 health  /  +3 Grace  /  +40 Golden Chronos  /  +1 Favor  /  item": totals per kind, gains and losses in
        /// their colors, items by name in their quality color. Empty when nothing changed.
        /// </summary>
        public static string BuildSummary(IEnumerable<EventActionResult> results, EventSummaryColors colors)
        {
            var list = results?.ToList() ?? new List<EventActionResult>();
            var entries = new List<string>();

            AddTotal(entries, list, EventResultKind.Health, "health", "health", colors.positive, colors.negative);
            AddTotal(entries, list, EventResultKind.Grace, "Grace", "Grace", colors.grace, colors.negative);
            AddTotal(entries, list, EventResultKind.Chronos, "Golden Chronos", "Golden Chronos", colors.positive, colors.negative);
            AddTotal(entries, list, EventResultKind.Favors, "Favor", "Favors", colors.favors, colors.negative);

            foreach (var r in list)
            {
                if (r.kind == EventResultKind.Item && r.item != null) entries.Add(EffectQualityDisplayHelper.ColorizeName(r.item));
            }

            return string.Join(SummarySeparator, entries);
        }

        private static void AddTotal(List<string> entries, List<EventActionResult> results, EventResultKind kind,
            string singular, string plural, string gainHex, string lossHex)
        {
            float total = results.Where(r => r.kind == kind).Sum(r => r.amount);
            int shown = DisplayAmount(total);
            if (shown == 0) return;

            string sign = total > 0f ? "+" : "-";
            string hex = total > 0f ? gainHex : lossHex;
            string text = $"{sign}{shown} {(shown == 1 ? singular : plural)}";
            entries.Add(string.IsNullOrEmpty(hex) ? text : $"<color=#{hex}>{text}</color>");
        }

        private static bool EveryPlaceholderGave(string sentence, IReadOnlyList<EventActionResult> results)
        {
            foreach (var kind in PlaceholdersIn(sentence))
            {
                if (!TryFind(results, kind, out var r) || !r.Gave) return false;
            }
            return true;
        }

        /// <summary>"…never sent, [and {item}.] The ink…" without its item reads "…never sent. The ink…".</summary>
        private static void DropDanglingComma(StringBuilder sb, string droppedSentence)
        {
            int i = sb.Length - 1;
            while (i >= 0 && char.IsWhiteSpace(sb[i])) i--;
            if (i < 0 || sb[i] != ',') return;

            string trimmed = droppedSentence.TrimEnd();
            char end = trimmed.Length > 0 ? trimmed[trimmed.Length - 1] : '\0';
            if (end == '.' || end == '!' || end == '?')
            {
                sb[i] = end;
            }
        }

        private static string ValueText(EventResultKind kind, IReadOnlyList<EventActionResult> results)
        {
            if (!TryFind(results, kind, out var r))
            {
                return kind == EventResultKind.Item ? "nothing" : "0";
            }
            if (kind == EventResultKind.Item)
            {
                return r.item != null ? EffectQualityDisplayHelper.ColorizeName(r.item) : "nothing";
            }
            return DisplayAmount(r.amount).ToString();
        }

        private static bool TryFind(IReadOnlyList<EventActionResult> results, EventResultKind kind, out EventActionResult found)
        {
            foreach (var r in results)
            {
                if (r.kind == kind)
                {
                    found = r;
                    return true;
                }
            }
            found = default;
            return false;
        }

        private static EventResultKind KindOf(string name) => name switch
        {
            "health" => EventResultKind.Health,
            "grace" => EventResultKind.Grace,
            "chronos" => EventResultKind.Chronos,
            "favors" => EventResultKind.Favors,
            _ => EventResultKind.Item
        };
    }
}
