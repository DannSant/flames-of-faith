using Game.Control;
using UnityEngine;

public class TransparencyOnTrigger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float transparentAlpha = 0.3f;

    private float originalAlpha;

    private void Awake()
    {        
        originalAlpha = spriteRenderer.color.a;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }
        Color color = spriteRenderer.color;
        color.a = transparentAlpha;
        spriteRenderer.color = color;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }
        Color color = spriteRenderer.color;
        color.a = originalAlpha;
        spriteRenderer.color = color;
    }
}
