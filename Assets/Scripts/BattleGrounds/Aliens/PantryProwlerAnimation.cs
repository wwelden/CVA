using UnityEngine;

// Sprite-only walk and antenna charge/throw animation.
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(Shooter))]
public sealed class PantryProwlerAnimation : MonoBehaviour
{
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Shooting = Animator.StringToHash("Shooting");
    public const float ShotReleaseTime = 0.625f;
    public const float ShotDuration = 0.75f;
    public bool IsShooting { get; private set; }
    private Animator animator;
    private Shooter shooter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        shooter = GetComponent<Shooter>();
    }

    private void Update()
    {
        if (!shooter.isActiveAndEnabled || shooter.pancaked)
            EndShot();
        animator.SetBool(Walking, shooter.isActiveAndEnabled &&
            Mathf.Abs(shooter.speed) > 0.0001f && !shooter.reloading && !shooter.pancaked);
    }

    public void BeginShot()
    {
        IsShooting = true;
        animator.SetBool(Walking, false);
        animator.SetBool(Shooting, true);
    }

    public void EndShot()
    {
        IsShooting = false;
        if (animator != null)
            animator.SetBool(Shooting, false);
    }

    public Vector3 ShotPosition
    {
        // Match the detached orb in the throw pose while keeping the lane's Y.
        get { return transform.TransformPoint(new Vector3(-2.980f, 0f, 0f)); }
    }

    private void OnDisable()
    {
        IsShooting = false;
        if (animator != null)
        {
            animator.SetBool(Walking, false);
            animator.SetBool(Shooting, false);
        }
    }
}
