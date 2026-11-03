using UnityEngine;

// Follow Shooter's existing movement state without changing combat or physics.
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(Shooter))]
public sealed class PantryProwlerAnimation : MonoBehaviour
{
    private static readonly int Walking = Animator.StringToHash("Walking");
    private Animator animator;
    private Shooter shooter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        shooter = GetComponent<Shooter>();
    }

    private void Update()
    {
        animator.SetBool(Walking, shooter.isActiveAndEnabled &&
            Mathf.Abs(shooter.speed) > 0.0001f && !shooter.reloading && !shooter.pancaked);
    }

    private void OnDisable()
    {
        if (animator != null)
            animator.SetBool(Walking, false);
    }
}
