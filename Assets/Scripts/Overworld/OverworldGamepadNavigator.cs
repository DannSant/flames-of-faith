using Game.Control;
using Game.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Overworld
{
    // Gamepad navigation for the overworld map. The map's nodes are world-space sprites driven by
    // OnMouseDown, so there is nothing for uGUI navigation to select - instead a stick/d-pad push
    // steps the player to the neighbouring node in that direction (the same move a click performs),
    // and Submit enters the level exactly as clicking the node the player stands on does.
    public class OverworldGamepadNavigator : MonoBehaviour
    {
        [SerializeField] private OverworldMapRenderer mapRenderer;

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

        private void Awake()
        {
            if (mapRenderer == null)
            {
                mapRenderer = GetComponent<OverworldMapRenderer>();
            }
        }

        private void Update()
        {
            if (InputDeviceManager.Instance == null || !InputDeviceManager.Instance.IsGamepadActive)
            {
                directionHeld = false;
                return;
            }

            var mapController = MapRunController.Instance;
            if (mapController == null || !mapController.IsInitialized) return;

            AcquireActions();
            if (navigateAction == null) return;

            HandleDirection(mapController);
            HandleSubmit(mapController);
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
