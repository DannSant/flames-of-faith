using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animator != null)
        {
            animator.SetTrigger("Start");
        }
    }
}
