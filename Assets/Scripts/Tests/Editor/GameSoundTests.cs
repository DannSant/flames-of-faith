using Game.Audio;
using Game.Audio.Editor;
using Game.Control;
using Game.Feel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Game sounds: the XP pickup pitch chain, the restart interval, the heartbeat rule, the sound table
    /// guard (one sane entry per GameSound) and that level-up only sounds on a real level-up.
    /// </summary>
    public class GameSoundTests
    {
        private const float Window = 0.6f;
        private const float Step = 0.03f;
        private const float Max = 1.5f;

        // --- XP pitch chain --------------------------------------------------------------------------------

        [Test]
        public void PitchChain_StartsAtOneAndRisesWithinTheWindow()
        {
            var chain = new PickupPitchChain();
            Assert.IsTrue(chain.TryNext(0f, 1, Window, Step, Max, out float first));
            Assert.AreEqual(1f, first, 1e-5f);
            Assert.IsTrue(chain.TryNext(0.2f, 2, Window, Step, Max, out float second));
            Assert.AreEqual(1.03f, second, 1e-5f);
            Assert.IsTrue(chain.TryNext(0.7f, 3, Window, Step, Max, out float third));
            Assert.AreEqual(1.06f, third, 1e-5f);
        }

        [Test]
        public void PitchChain_CapsThenStartsOver()
        {
            var chain = new PickupPitchChain();
            float pitch = 0f;
            float highest = 0f;
            for (int i = 0; i < 40; i++)
            {
                Assert.IsTrue(chain.TryNext(i * 0.1f, i, Window, Step, Max, out pitch));
                highest = Math.Max(highest, pitch);
                Assert.LessOrEqual(pitch, Max + 1e-4f);
            }
            Assert.Greater(highest, Max - Step, "The chain never got near its cap");

            // 17 steps reach 1.51 > 1.5, so the 18th pickup (index 17) starts over
            var fresh = new PickupPitchChain();
            for (int i = 0; i < 17; i++) fresh.TryNext(i * 0.1f, i, Window, Step, Max, out pitch);
            Assert.AreEqual(1.48f, pitch, 1e-4f);
            fresh.TryNext(1.7f, 17, Window, Step, Max, out pitch);
            Assert.AreEqual(1f, pitch, 1e-4f);
        }

        [Test]
        public void PitchChain_ResetsAfterThePauseOutlastsTheWindow()
        {
            var chain = new PickupPitchChain();
            chain.TryNext(0f, 1, Window, Step, Max, out _);
            chain.TryNext(0.1f, 2, Window, Step, Max, out _);
            Assert.IsTrue(chain.TryNext(0.1f + Window + 0.01f, 3, Window, Step, Max, out float pitch));
            Assert.AreEqual(1f, pitch, 1e-5f);
        }

        [Test]
        public void PitchChain_PlaysOncePerFrame()
        {
            // CollectAllInstantly can sweep every token in one frame: one sound covers them all
            var chain = new PickupPitchChain();
            Assert.IsTrue(chain.TryNext(1f, 10, Window, Step, Max, out _));
            Assert.IsFalse(chain.TryNext(1f, 10, Window, Step, Max, out _));
            Assert.IsFalse(chain.TryNext(1f, 10, Window, Step, Max, out _));
            Assert.IsTrue(chain.TryNext(1.02f, 11, Window, Step, Max, out float next));
            Assert.AreEqual(1.03f, next, 1e-5f);
        }

        [Test]
        public void RestartInterval_BlocksOnlyInsideIt()
        {
            Assert.IsFalse(GameSoundTable.CanRestart(1f, 1.04f, 0.05f));
            Assert.IsTrue(GameSoundTable.CanRestart(1f, 1.05f, 0.05f));
            Assert.IsTrue(GameSoundTable.CanRestart(1f, 1f, 0f));
        }

        [Test]
        public void Heartbeat_OnlyOnStrongPulsesAndNeverPaused()
        {
            Assert.IsTrue(LowHealthRules.PlaysHeartbeat(0.8f, 0.6f, false));
            Assert.IsTrue(LowHealthRules.PlaysHeartbeat(0.6f, 0.6f, false));
            Assert.IsFalse(LowHealthRules.PlaysHeartbeat(0.5f, 0.6f, false));
            Assert.IsFalse(LowHealthRules.PlaysHeartbeat(1f, 0.6f, true));
        }

        // --- Sound table -----------------------------------------------------------------------------------

        private static GameSoundTable Table()
        {
            var table = AssetDatabase.LoadAssetAtPath<GameSoundTable>(SilentSoundsReport.TablePath);
            Assert.IsNotNull(table, $"Missing {SilentSoundsReport.TablePath}");
            return table;
        }

        [Test]
        public void SoundTable_HasOneEntryPerSound()
        {
            var table = Table();
            Assert.AreSame(table, GameSoundTable.Instance, "GameSoundTable.Instance doesn't load the asset");
            Assert.IsEmpty(SilentSoundsReport.FindMissingEntries(table), "Sounds with no table entry");

            var duplicates = table.Entries.GroupBy(e => e.sound).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.IsEmpty(duplicates, "Sounds with more than one entry");
        }

        [Test]
        public void SoundTable_ValuesAreSane()
        {
            var table = Table();
            Assert.Greater(table.RandomPitchMin, 0f);
            Assert.LessOrEqual(table.RandomPitchMax, 2f);
            foreach (var entry in table.Entries)
            {
                Assert.That(entry.volume, Is.InRange(0.05f, 1f), $"{entry.sound} volume");
                // Above 0 for everything: many of these can fire several times in one frame
                Assert.That(entry.minInterval, Is.InRange(0.01f, 1f), $"{entry.sound} min interval");
                if (entry.clips != null)
                {
                    Assert.IsFalse(entry.clips.Any(c => c == null), $"{entry.sound} has an empty clip slot");
                }
            }
        }

        [Test]
        public void SoundTable_ClassVoicesAreSane()
        {
            foreach (var entry in Table().Entries)
            {
                if (entry.classClips == null) continue;
                Assert.IsFalse(entry.classClips.Any(c => c == null || c.characterClass == CharacterClass.None),
                    $"{entry.sound} has a class voice with no class");
                var twice = entry.classClips.GroupBy(c => c.characterClass).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                Assert.IsEmpty(twice, $"{entry.sound} lists a class more than once");
                foreach (var classEntry in entry.classClips)
                {
                    Assert.IsFalse(classEntry.clips != null && classEntry.clips.Any(c => c == null),
                        $"{entry.sound} ({classEntry.characterClass}) has an empty clip slot");
                }
            }
        }

        [Test]
        public void ClassVoice_WinsOverSharedClipsAndFallsBack()
        {
            var shared = AudioClip.Create("Shared", 1, 1, 44100, false);
            var crusader = AudioClip.Create("Crusader", 1, 1, 44100, false);
            try
            {
                var entry = new GameSoundTable.Entry
                {
                    sound = GameSound.PlayerHurt,
                    clips = new[] { shared },
                    classClips = new List<GameSoundTable.ClassClips>
                    {
                        new GameSoundTable.ClassClips { characterClass = CharacterClass.Crusader, clips = new[] { crusader } },
                        new GameSoundTable.ClassClips { characterClass = CharacterClass.Sentinel, clips = new AudioClip[0] },
                    },
                };
                Assert.AreSame(crusader, entry.ClipsFor(CharacterClass.Crusader)[0]);
                Assert.AreSame(shared, entry.ClipsFor(CharacterClass.Sentinel)[0], "An empty class voice falls back");
                Assert.AreSame(shared, entry.ClipsFor(CharacterClass.Augur)[0], "A class with no voice falls back");
                Assert.AreSame(shared, entry.ClipsFor(CharacterClass.None)[0]);

                entry.clips = new AudioClip[0];
                Assert.IsTrue(entry.HasClips, "Class clips alone make the sound not silent");
                Assert.IsTrue(entry.HasClipsFor(CharacterClass.Crusader));
                Assert.IsFalse(entry.HasClipsFor(CharacterClass.Augur));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(shared);
                UnityEngine.Object.DestroyImmediate(crusader);
            }
        }

        [Test]
        public void SilentReport_ListsClassesLeftWithoutAVoice()
        {
            var table = ScriptableObject.CreateInstance<GameSoundTable>();
            var clip = AudioClip.Create("Voice", 1, 1, 44100, false);
            try
            {
                var entries = new List<GameSoundTable.Entry>
                {
                    new GameSoundTable.Entry
                    {
                        sound = GameSound.Dash,
                        classClips = new List<GameSoundTable.ClassClips>
                        {
                            new GameSoundTable.ClassClips { characterClass = CharacterClass.Crusader, clips = new[] { clip } },
                        },
                    },
                };
                typeof(GameSoundTable).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(table, entries);

                var classes = new[] { CharacterClass.Crusader, CharacterClass.Sentinel, CharacterClass.Augur };
                CollectionAssert.AreEqual(new[] { "Dash: Sentinel", "Dash: Augur" }, SilentSoundsReport.FindMissingVoices(table, classes));

                entries[0].clips = new[] { clip };
                Assert.IsEmpty(SilentSoundsReport.FindMissingVoices(table, classes), "Shared clips cover every class");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void SoundTable_XpChainKeepsItsOwnPitch()
        {
            Assert.IsTrue(Table().TryGet(GameSound.PickupXp, out var entry));
            Assert.IsFalse(entry.randomizePitch, "A random pitch would hide the XP chain's rise");
        }

        [Test]
        public void XpChainSettings_AreSane()
        {
            var settings = GameFeelSettings.Instance;
            Assert.Greater(settings.XpChainWindow, 0f);
            Assert.Greater(settings.XpChainStep, 0f);
            Assert.Greater(settings.XpChainMaxPitch, 1f + settings.XpChainStep);
            Assert.LessOrEqual(settings.XpChainMaxPitch, 2f);
        }

        // --- Hooks -----------------------------------------------------------------------------------------

        [Test]
        public void LevelUpSound_IsOnlyInTheRealLevelUp()
        {
            // onLevelUp also fires when the level is loaded or reset, so the sound mustn't hang off the event
            string source = File.ReadAllText("Assets/Scripts/Progression/PlayerExperience.cs");
            int addExperience = source.IndexOf("public void AddExperience", StringComparison.Ordinal);
            Assert.GreaterOrEqual(addExperience, 0);
            int sound = source.IndexOf("GameSound.LevelUp", StringComparison.Ordinal);
            Assert.Greater(sound, addExperience, "The level-up sound must be played inside AddExperience");
            Assert.AreEqual(sound, source.LastIndexOf("GameSound.LevelUp", StringComparison.Ordinal), "Level-up sound played twice");

            foreach (string path in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                if (path.Replace('\\', '/').EndsWith("Progression/PlayerExperience.cs") || path.Contains("Tests")) continue;
                StringAssert.DoesNotContain("GameSound.LevelUp", File.ReadAllText(path), $"{path} plays the level-up sound");
            }
        }
    }
}
