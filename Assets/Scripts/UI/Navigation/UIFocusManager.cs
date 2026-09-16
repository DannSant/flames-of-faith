using Game.Common;
using Game.Control;
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI.Navigation
{
    // Gamepad focus layer on top of uGUI navigation. Tracks open UIWindows and keeps exactly one of
    // them focused while the gamepad is active: only the focused window is interactable (so the stick
    // can't wander into other windows), LB/RB cycle focus left/right, Browse focuses on-demand windows
    // (Shop), Cancel backs out. With keyboard/mouse nothing is focused and every window is interactable.
    public class UIFocusManager : Singleton<UIFocusManager>
    {
        private const string FocusNextActionName = "FocusNext";
        private const string FocusPreviousActionName = "FocusPrevious";
        private const string BrowseActionName = "Browse";
        private const string CancelActionName = "Cancel";

        [Tooltip("Logs every focus change and window open/close, with the window's full hierarchy path.")]
        [SerializeField] private bool logFocusChanges = false;

        private readonly List<UIWindow> openWindows = new();
        private UIWindow focusedWindow;
        // Primary windows in the order they were focused (most recent last), so closing Pause
        // returns focus to the Shop the player was browsing instead of dropping it.
        private readonly List<UIWindow> primaryFocusHistory = new();
        private readonly HashSet<UIWindow> warnedAboutEmptyWindows = new();
        private bool gameplayInputBlocked = false;
        private bool subscribedToDeviceManager = false;

        private PlayerInputHandler inputHandler;
        private InputAction focusNextAction;
        private InputAction focusPreviousAction;
        private InputAction browseAction;
        private InputAction cancelAction;

        public UIWindow FocusedWindow => focusedWindow;
        public bool IsGameplayInputBlocked => gameplayInputBlocked;

        public event Action<UIWindow> OnFocusChanged;
        public event Action OnOpenWindowsChanged;
        public event Action<bool> OnGameplayInputBlockChanged;

        private static bool IsGamepadActive => InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;

        private void Start()
        {
            TrySubscribeToDeviceManager();
        }

        private void OnDisable()
        {
            if (subscribedToDeviceManager && InputDeviceManager.Instance != null)
            {
                InputDeviceManager.Instance.OnInputSchemeChanged -= HandleInputSchemeChanged;
            }
            subscribedToDeviceManager = false;
        }

        // Update still runs while Time.timeScale is 0, so this keeps working in paused menus.
        private void Update()
        {
            TrySubscribeToDeviceManager();
            if (!IsGamepadActive) return;

            AcquireActions();
            HandleNavigationInput();
            EnsureSelectionInFocusedWindow();
        }

        #region Window registration

        public void Register(UIWindow window)
        {
            if (window == null || openWindows.Contains(window)) return;
            openWindows.Add(window);

            if (logFocusChanges)
            {
                Debug.Log($"[UIFocusManager] Opened {Describe(window)} - {openWindows.Count} window(s) open.", window);
            }
            WarnIfNested(window);

            if (IsGamepadActive && ShouldTakeFocusOnOpen(window))
            {
                SetFocus(window);
            }
            else
            {
                ApplyInteractability();
            }
            OnOpenWindowsChanged?.Invoke();
        }

        public void Unregister(UIWindow window)
        {
            if (!openWindows.Remove(window)) return;

            if (logFocusChanges)
            {
                Debug.Log($"[UIFocusManager] Closed {Describe(window)} - {openWindows.Count} window(s) open.", window);
            }

            // Leave a closed window usable by the mouse the next time it opens.
            window.SetInteractable(true);
            primaryFocusHistory.Remove(window);

            if (window == focusedWindow)
            {
                focusedWindow = null;
                window.SetFocused(false);
                SetFocus(IsGamepadActive ? FindFallbackFocus() : null, force: true);
            }
            else
            {
                UpdateGameplayBlock();
            }
            OnOpenWindowsChanged?.Invoke();
        }

        // How many open windows the player can actually cycle through with LB/RB - used by prompts
        // so "switch window" hints only appear when there is somewhere to switch to.
        public int CountCyclableWindows()
        {
            int count = 0;
            foreach (var window in openWindows)
            {
                if (window == focusedWindow || HasNavigableContent(window)) count++;
            }
            return count;
        }

        private bool ShouldTakeFocusOnOpen(UIWindow window)
        {
            if (window.Role != UIWindowRole.Primary || window.FocusMode != UIWindowFocusMode.AutoFocus) return false;
            if (focusedWindow == null) return true;
            if (focusedWindow.Role == UIWindowRole.Secondary) return true;
            return window.Priority > focusedWindow.Priority;
        }

        #endregion

        #region Focus

        public void SetFocus(UIWindow window)
        {
            SetFocus(window, force: false);
        }

        private void SetFocus(UIWindow window, bool force)
        {
            if (!force && window == focusedWindow) return;

            if (focusedWindow != null && focusedWindow != window)
            {
                focusedWindow.SetFocused(false);
            }

            focusedWindow = window;
            if (focusedWindow != null)
            {
                focusedWindow.SetFocused(true);
                if (focusedWindow.Role == UIWindowRole.Primary)
                {
                    primaryFocusHistory.Remove(focusedWindow);
                    primaryFocusHistory.Add(focusedWindow);
                }
            }

            // Interactability must be applied before selecting: non-interactable selectables can't be selected.
            ApplyInteractability();
            SelectDefault();
            UpdateGameplayBlock();
            OnFocusChanged?.Invoke(focusedWindow);

            if (logFocusChanges)
            {
                var eventSystem = EventSystem.current;
                GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
                Debug.Log($"[UIFocusManager] Focus -> {Describe(focusedWindow)}; selected: {(selected != null ? selected.name : "<none>")}",
                    focusedWindow != null ? focusedWindow : this);
            }
        }

        // A UIWindow inside another UIWindow means the same content belongs to two windows: both get
        // focused separately and only one shows its focus frame, which looks like an "invisible window".
        private static void WarnIfNested(UIWindow window)
        {
            for (Transform parent = window.transform.parent; parent != null; parent = parent.parent)
            {
                var parentWindow = parent.GetComponent<UIWindow>();
                if (parentWindow != null)
                {
                    Debug.LogWarning($"[UIFocusManager] {Describe(window)} is nested inside {Describe(parentWindow)}. " +
                        "Keep one UIWindow per window, on the panel the controller shows/hides.", window);
                    return;
                }
            }
        }

        private static string Describe(UIWindow window)
        {
            if (window == null) return "<no window>";
            return $"'{GetHierarchyPath(window.transform)}' (role {window.Role}, mode {window.FocusMode}, priority {window.Priority}, active {window.gameObject.activeInHierarchy})";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }
            return path;
        }

        public void ClearFocus()
        {
            SetFocus(null);
        }

        private UIWindow FindFallbackFocus()
        {
            UIWindow recentPrimary = GetMostRecentOpenPrimary();
            return recentPrimary != null ? recentPrimary : FindBestWindow(UIWindowFocusMode.AutoFocus);
        }

        private UIWindow GetMostRecentOpenPrimary()
        {
            for (int i = primaryFocusHistory.Count - 1; i >= 0; i--)
            {
                if (openWindows.Contains(primaryFocusHistory[i])) return primaryFocusHistory[i];
            }
            return null;
        }

        private UIWindow FindBestWindow(UIWindowFocusMode mode)
        {
            UIWindow best = null;
            foreach (var window in openWindows)
            {
                if (window.Role != UIWindowRole.Primary || window.FocusMode != mode) continue;
                if (best == null || window.Priority > best.Priority)
                {
                    best = window;
                }
            }
            return best;
        }

        private void ApplyInteractability()
        {
            bool gamepad = IsGamepadActive;
            foreach (var window in openWindows)
            {
                window.SetInteractable(!gamepad || window == focusedWindow);
            }
        }

        private void SelectDefault()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return;

            if (!IsGamepadActive || focusedWindow == null)
            {
                eventSystem.SetSelectedGameObject(null);
                return;
            }

            Selectable selectable = focusedWindow.GetDefaultSelectable();
            eventSystem.SetSelectedGameObject(selectable != null ? selectable.gameObject : null);

            // A window with nothing selectable can be focused but never navigated, and LB/RB skip it
            // afterwards - almost always a Selectable left on Navigation: None.
            if (selectable == null && warnedAboutEmptyWindows.Add(focusedWindow))
            {
                Debug.LogWarning($"[UIFocusManager] {Describe(focusedWindow)} has no navigable Selectable. " +
                    "Check its buttons aren't set to Navigation: None.", focusedWindow);
            }
        }

        // Window contents are often destroyed and rebuilt (shop grid, upgrade choices, item bag),
        // which drops the selection - reselect so the stick never ends up controlling nothing.
        private void EnsureSelectionInFocusedWindow()
        {
            if (focusedWindow == null) return;
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return;

            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected != null && selected.activeInHierarchy && selected.transform.IsChildOf(focusedWindow.transform))
            {
                return;
            }
            SelectDefault();
        }

        private void UpdateGameplayBlock()
        {
            UIWindow recentPrimary = GetMostRecentOpenPrimary();
            bool blocked = IsGamepadActive && focusedWindow != null
                && (focusedWindow.BlocksGameplayInput
                    // Cycling from the Shop to Inventory/Stats shouldn't hand control back to the player.
                    || (focusedWindow.Role == UIWindowRole.Secondary
                        && recentPrimary != null
                        && recentPrimary.BlocksGameplayInput));

            if (blocked == gameplayInputBlocked) return;
            gameplayInputBlocked = blocked;
            OnGameplayInputBlockChanged?.Invoke(gameplayInputBlocked);
        }

        #endregion

        #region Input

        private void HandleNavigationInput()
        {
            if (focusNextAction != null && focusNextAction.WasPressedThisFrame())
            {
                CycleFocus(1);
            }
            else if (focusPreviousAction != null && focusPreviousAction.WasPressedThisFrame())
            {
                CycleFocus(-1);
            }
            else if (browseAction != null && browseAction.WasPressedThisFrame())
            {
                HandleBrowse();
            }
            else if (cancelAction != null && cancelAction.WasPressedThisFrame())
            {
                HandleCancel();
            }
        }

        private void CycleFocus(int direction)
        {
            // Cycling only makes sense once the player is "in" the UI - e.g. the Shop needs Browse first.
            if (focusedWindow == null) return;

            var candidates = new List<UIWindow>();
            foreach (var window in openWindows)
            {
                if (window == focusedWindow || HasNavigableContent(window))
                {
                    candidates.Add(window);
                }
            }
            if (candidates.Count < 2) return;

            candidates.Sort((a, b) => a.GetScreenX().CompareTo(b.GetScreenX()));
            int index = candidates.IndexOf(focusedWindow);
            int nextIndex = (index + direction + candidates.Count) % candidates.Count;
            SetFocus(candidates[nextIndex]);
        }

        private static bool HasNavigableContent(UIWindow window)
        {
            // Can't use IsInteractable() here: unfocused windows are deliberately non-interactable.
            foreach (var selectable in window.GetComponentsInChildren<Selectable>())
            {
                if (selectable.isActiveAndEnabled && selectable.navigation.mode != UnityEngine.UI.Navigation.Mode.None)
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleBrowse()
        {
            if (focusedWindow != null) return;
            UIWindow onDemandWindow = FindBestWindow(UIWindowFocusMode.OnDemand);
            if (onDemandWindow != null)
            {
                SetFocus(onDemandWindow);
            }
        }

        private void HandleCancel()
        {
            if (focusedWindow == null) return;

            if (focusedWindow.Role == UIWindowRole.Secondary)
            {
                SetFocus(FindFallbackFocus());
                return;
            }

            UIWindow window = focusedWindow;
            window.OnCancel?.Invoke();

            // OnCancel may have closed the window (which already moved focus). Otherwise a window
            // that only borrows control (Shop) hands it back to gameplay while staying open.
            if (window == focusedWindow
                && (window.FocusMode == UIWindowFocusMode.OnDemand || window.ReleaseFocusOnCancel))
            {
                // Forget it so closing some later window doesn't re-enter browsing without asking.
                primaryFocusHistory.Remove(window);
                ClearFocus();
            }
        }

        private void AcquireActions()
        {
            if (inputHandler != null && focusNextAction != null) return;
            if (PlayerManager.Instance == null) return;

            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            if (inputHandler == null) return;

            // Looked up by name so this doesn't depend on the generated wrapper being regenerated.
            InputActionMap uiMap = inputHandler.UI.Get();
            focusNextAction = uiMap.FindAction(FocusNextActionName);
            focusPreviousAction = uiMap.FindAction(FocusPreviousActionName);
            browseAction = uiMap.FindAction(BrowseActionName);
            cancelAction = uiMap.FindAction(CancelActionName);
        }

        #endregion

        #region Input scheme

        private void TrySubscribeToDeviceManager()
        {
            if (subscribedToDeviceManager || InputDeviceManager.Instance == null) return;
            InputDeviceManager.Instance.OnInputSchemeChanged += HandleInputSchemeChanged;
            subscribedToDeviceManager = true;
        }

        private void HandleInputSchemeChanged(InputScheme scheme)
        {
            // Mouse: nothing focused, everything clickable. Gamepad: focus the best auto-focus window.
            primaryFocusHistory.Clear();
            SetFocus(scheme == InputScheme.Gamepad ? FindBestWindow(UIWindowFocusMode.AutoFocus) : null, force: true);
        }

        #endregion
    }
}
