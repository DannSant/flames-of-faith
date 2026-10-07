using Game.Control;
using Game.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Overworld
{
    // Gamepad and keyboard navigation for the overworld map. The map's nodes are world-space sprites driven by
    // OnMouseDown, so there is nothing for uGUI navigation to select - instead a stick/d-pad/WASD/arrow push
    // steps the player to the neighbouring node in that direction (the same move a click performs).
    // On gamepad, Submit enters the level exactly as clicking the node the player stands on does.
    public class OverworldGamepadNavigator : MonoBehaviour
    {
        [SerializeField] private OverworldMapRenderer mapRenderer;
        [Tooltip("Let keyboard players step between nodes with WASD / arrow keys (UI Navigate action).")]
        [SerializeField] private bool allowKeyboardMovement = true;

        [Header("Direction")]
        [Tooltip("How far the stick must be pushed to count as a direction.")]
        [SerializeField] private float stickThreshold = 0.5f;
        [Tooltip("How far a node may sit off the pushed direction and still be chosen.")]
        [SerializeField] private float maxAngle = 70f;

        [Header("Repeat while held")]
        [SerializeField] private float repeatDelay = 0.4f;
        [SerializeField] private float repeatRate = 0.2f;

        private PlayerInputHandler inputHandler;
        private InputAction navigateAction;
        private InputAction submitAction;

        private bool directionHeld = false;
        private float nextRepeatTime = 0f;

        // Node whose Taint tooltip is shown because the gamepad player stands on it (no hover on a gamepad)
        private OverworldNodeView tooltipNode;

        private void Awake()
        {
            if (mapRenderer == null)
            {
                mapRenderer = GetComponent<OverworldMapRenderer>();
            }
        }

        private void Update()
        {
            bool gamepadActive = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            var mapController = MapRunController.Instance;
            bool mapReady = mapController != null && mapController.IsInitialized;

            UpdateNodeTooltip(gamepadActive && mapReady ? mapController.CurrentNode : null);

            bool canMove = gamepadActive || allowKeyboardMovement;
            if (!canMove || !mapReady)
            {
                directionHeld = false;
                return;
            }

            AcquireActions();
            if (navigateAction == null) return;

            // Stick, d-pad, WASD and arrow keys are all bound to the UI Navigate action
            HandleDirection(mapController);

            // Keyboard players enter a level by clicking it, as before
            if (gamepadActive)
            {
                HandleSubmit(mapController);
            }
        }

        private void HandleDirection(MapRunController mapController)
        {
            Vector2 direction = navigateAction.ReadValue<Vector2>();

            if (direction.magnitude < stickThreshold)
            {
                directionHeld = false;
                return;
            }

            if (!directionHeld)
            {
                directionHeld = true;
                nextRepeatTime = Time.unscaledTime + repeatDelay;
                MoveTowards(mapController, direction);
                return;
            }

            if (Time.unscaledTime >= nextRepeatTime)
            {
                nextRepeatTime = Time.unscaledTime + repeatRate;
                MoveTowards(mapController, direction);
            }
        }

        private void MoveTowards(MapRunController mapController, Vector2 direction)
        {
            RunNode current = mapController.CurrentNode;
            if (current == null) return;

            direction.Normalize();
            RunNode best = null;
            float bestAngle = 0f;
            float bestDistance = 0f;

            foreach (var node in mapController.GetAvailableMoves())
            {
                Vector2 toNode = node.worldPosition - current.worldPosition;
                if (toNode.sqrMagnitude < 0.0001f) continue;

                float angle = Vector2.Angle(direction, toNode);
                if (angle > maxAngle) continue;

                float distance = toNode.magnitude;
                // Closest match to the pushed direction wins; ties go to the nearer node.
                if (best == null || angle < bestAngle || (Mathf.Approximately(angle, bestAngle) && distance < bestDistance))
                {
                    best = node;
                    bestAngle = angle;
                    bestDistance = distance;
                }
            }

            if (best != null)
            {
                mapController.TryMoveTo(best.id);
            }
        }

        private void HandleSubmit(MapRunController mapController)
        {
            if (submitAction == null || !submitAction.WasPressedThisFrame()) return;
            if (mapRenderer == null) return;

            // Reuses the click path, so the "revealed and not cleared" rule lives in one place.
            mapRenderer.OnNodeClicked(mapController.CurrentNode.id);
        }

        // Shows the tooltip of the node the player stands on; hides it when moving off it or switching to mouse
        private void UpdateNodeTooltip(RunNode node)
        {
            OverworldNodeView view = node != null && mapRenderer != null ? mapRenderer.GetNodeView(node.id) : null;
            if (view == tooltipNode) return;

            if (tooltipNode != null)
            {
                tooltipNode.SetTaintTooltipShown(false);
            }

            tooltipNode = view;
            if (tooltipNode != null)
            {
                tooltipNode.SetTaintTooltipShown(true);
            }
        }

        private void OnDisable()
        {
            UpdateNodeTooltip(null);
        }

        private void AcquireActions()
        {
            if (inputHandler != null && navigateAction != null) return;
            if (PlayerManager.Instance == null) return;

            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            if (inputHandler == null) return;

            InputActionMap uiMap = inputHandler.UI.Get();
            navigateAction = uiMap.FindAction("Navigate");
            submitAction = uiMap.FindAction("Submit");
        }
    }
}
