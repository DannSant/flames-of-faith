using Game.Control;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    private bool playerInside = false;
    private Collider2D col;

    public bool IsPlayerInside => playerInside;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
            playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
            playerInside = false;
    }

    public Vector2 GetRandomPointInside()
    {
        Bounds bounds = col.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }
}
