using System;
using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    ///     Configuration class for level selection system settings
    ///     Simple serializable class that appears directly in the inspector
    /// </summary>
    [Serializable]
    public class LevelSelectionConfig
    {
        [Header("Navigation Settings")] public int gridWidth = 4;

        public float selectorMoveSpeed = 5f;
        public float snapThreshold = 0.1f;

        [Header("Audio Settings")] public AudioClip navigationSound;

        public AudioClip selectionSound;
        public AudioClip lockedSound;

        [Header("Item Select Screen")] public float itemSelectDisplayDuration = 2f;

        public bool waitForInputOnItemSelect = true;

        [Header("Transition")] public float transitionDuration = 1f;

        public Color[] nesTransitionColors = { Color.black, new(0.2f, 0.2f, 0.3f), new(0.1f, 0.1f, 0.2f) };
    }
}
