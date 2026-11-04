using UnityEngine;

public class AlienShot : MonoBehaviour
{
    public float mSpeed = 5f;
    public GameObject shot;
    public GameObject impactPrefab;
    private bool impacted;
    private const float ImpactLifetime = 0.3f;

    void Start()
    {
        // Older Beam prefabs serialize zero for the default speed.
        if (mSpeed <= 0f) mSpeed = 5f;
    }

    void Update()
    {
        if (impacted) return;
        transform.Translate(Vector2.left * mSpeed * Time.deltaTime);
        if (transform.position.x < -10 || transform.position.x > 10 ||
            transform.position.y < -5 || transform.position.y > 5)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (impacted) return;

        // Resolve the actual target, including colliders on visual children.
        var salad = other.GetComponentInParent<Salad>();
        var lasagna = other.GetComponentInParent<Lasagna>();
        var iceCream = other.GetComponentInParent<IceCream>();
        var salsa = other.GetComponentInParent<Salsa>();
        var pancakes = other.GetComponentInParent<Pancakes>();
        var pie = other.GetComponentInParent<Pie>();
        var tomato = other.GetComponentInParent<Tomato>();
        MonoBehaviour target = salad;
        if (target == null) target = lasagna;
        if (target == null) target = iceCream;
        if (target == null) target = salsa;
        if (target == null) target = pancakes;
        if (target == null) target = pie;
        if (target == null) target = tomato;
        if (target == null && !other.CompareTag("Food")) return;

        // Claim the hit before damage or destruction can produce more callbacks.
        impacted = true;
        foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        if (pancakes != null) BattlegroundPlantHit.Flash(pancakes.gameObject);
        if (salad != null) salad.TakeDamage(1f);
        else if (lasagna != null) lasagna.TakeDamage(2f);
        else if (iceCream != null) iceCream.TakeDamage(1f);
        else if (salsa != null) salsa.TakeDamage(1f);
        else if (pancakes != null) pancakes.Die();
        else if (pie != null) pie.TakeHit();
        else if (tomato != null) tomato.TakeHit();

        if (impactPrefab != null)
        {
            GameObject impact = Instantiate(impactPrefab, transform.position, Quaternion.identity);
            Destroy(impact, ImpactLifetime);
        }
        Destroy(gameObject);
    }
}
