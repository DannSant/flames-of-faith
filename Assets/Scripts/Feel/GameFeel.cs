using Game.Control;
using Game.GameSettings;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.Feel
{
    /// <summary>
    /// The one entry point of hit feel: <see cref="Play"/> looks the moment's row up on GameFeelSettings and sends
    /// its camera shake (Cinemachine Impulse), hit stop / slow motion (TimeDilation) and gamepad rumble, each
    /// scaled by its Settings slider and behind its master switch. Only the moments in FeelMoment exist, never
    /// routine hits or kills. The runner object is made at runtime, so no scene or prefab holds it.
    /// </summary>
    public class GameFeel : MonoBehaviour
    {
        private static GameFeel instance;

        private readonly Dictionary<FeelMoment, float> lastPlayed = new Dictionary<FeelMoment, float>();
        private CinemachineImpulseSource impulseSource;
        private float rumbleStopAt;
        private bool rumbling;

        private static float Now => Time.realtimeSinceStartup;

        /// <summary>Plays a moment's feedback, unless it is still on its cooldown. Safe from anywhere, and never throws.</summary>
        public static void Play(FeelMoment moment)
        {
            if (!Application.isPlaying) return;
            var settings = GameFeelSettings.Instance;
            if (!settings.TryGetMoment(moment, out var row)) return;

            var runner = GetOrCreate();
            if (runner.lastPlayed.TryGetValue(moment, out float last) && !GameFeelRules.IsOffCooldown(last, Now, row.cooldown)) return;
            runner.lastPlayed[moment] = Now;

            try
            {
                if (settings.ShakeEnabled) runner.Shake(row.shakeForce, row.shakeDuration);
                if (settings.HitStopEnabled)
                {
                    TimeDilation.Request(settings.HitStopScale, row.hitStop);
                    if (row.slowMotionScale < 1f) TimeDilation.Request(row.slowMotionScale, row.slowMotionDuration);
                }
                if (settings.RumbleEnabled) runner.Rumble(row.rumbleLow, row.rumbleHigh, row.rumbleDuration);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameFeel] {moment} failed: {e.Message}");
            }
        }

        /// <summary>A hit on the player: the HeavyHit moment when it takes a large enough share of max health.</summary>
        public static void OnPlayerDamaged(float damage, float maxHealth)
        {
            if (GameFeelRules.IsHeavyHit(damage, maxHealth, GameFeelSettings.Instance.HeavyHitFraction))
            {
                Play(FeelMoment.HeavyHit);
            }
        }

        /// <summary>
        /// Lets a level camera feel the shakes: adds an impulse listener at runtime (no scene change). The map's
        /// camera never gets one, so nothing shakes there.
        /// </summary>
        public static void AttachShakeListener(CinemachineCamera camera)
        {
            if (camera == null) return;
            var listener = camera.GetComponent<CinemachineImpulseListener>();
            if (listener != null) return;
            listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();
            // AddComponent skips Reset, so set the defaults by hand
            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1;
            listener.Gain = 1f;
            listener.Use2DDistance = true;
            listener.UseCameraSpace = true;
        }

        private static GameFeel GetOrCreate()
        {
            if (instance != null) return instance;
            var go = new GameObject("GameFeel");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameFeel>();
            return instance;
        }

        private void Awake()
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            var definition = impulseSource.ImpulseDefinition;
            definition.ImpulseChannel = 1;
            definition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            definition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            StopRumble();
            if (instance == this) instance = null;
        }

        private void Update()
        {
            if (!rumbling) return;
            bool paused = PauseManager.Instance != null && PauseManager.Instance.IsPaused;
            if (paused || Now >= rumbleStopAt) StopRumble();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) StopRumble();
        }

        private void OnApplicationQuit() => StopRumble();

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) => StopRumble();

        private void Shake(float force, float duration)
        {
            float slider = SettingsManager.Instance != null ? SettingsManager.Instance.ScreenShake : 1f;
            float scaled = GameFeelRules.Scaled(force, slider);
            if (scaled <= 0f || duration <= 0f) return;

            // Shakes keep running through a hit stop
            if (CinemachineImpulseManager.Instance != null) CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
            impulseSource.ImpulseDefinition.ImpulseDuration = duration;
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction == Vector2.zero) direction = Vector2.down;
            impulseSource.GenerateImpulseWithVelocity(direction * scaled);
        }

        private void Rumble(float low, float high, float duration)
        {
            if (duration <= 0f) return;
            if (InputDeviceManager.Instance == null || !InputDeviceManager.Instance.IsGamepadActive) return;
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            float slider = SettingsManager.Instance != null ? SettingsManager.Instance.Rumble : 1f;
            low = GameFeelRules.Scaled(low, slider);
            high = GameFeelRules.Scaled(high, slider);
            if (low <= 0f && high <= 0f) return;

            gamepad.SetMotorSpeeds(low, high);
            rumbling = true;
            rumbleStopAt = Mathf.Max(rumbleStopAt, Now + duration);
        }

        // Motors must never stay on: a real-time timer, pause, scene load, losing focus and quitting all stop them
        private void StopRumble()
        {
            if (!rumbling) return;
            rumbling = false;
            rumbleStopAt = 0f;
            InputSystem.ResetHaptics();
        }
    }
}
