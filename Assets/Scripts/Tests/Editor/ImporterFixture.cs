using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>A small asset for the balance importer's tests: a nested record, a list of rows and a text field.</summary>
    public class ImporterFixture : ScriptableObject
    {
        [Serializable]
        public struct Profile
        {
            [Tooltip("Profile base.")]
            public int baseValue;
            [Tooltip("Profile scale.")]
            public float scale;
        }

        [Serializable]
        public class Row
        {
            [Tooltip("A row's amount.")]
            public int amount;
        }

        public int top = 1;
        public Profile profile = new Profile { baseValue = 2, scale = 0.5f };
        public List<Row> rows = new();
        [TextArea] public string text = "";
    }
}
