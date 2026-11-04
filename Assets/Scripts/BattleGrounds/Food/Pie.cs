using UnityEngine;

public class Pie : MonoBehaviour
{
    private void Awake()
    {
        BattlegroundPlantHit.Attach(gameObject, lives);
    }

    public float speed = 2f;
    public int lives = 1;
    public int damage = 3;

    public int lifetime = 30;
    private bool consumed;
  

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) TakeHit();
    }

    public void TakeHit()
    {
        if (consumed) return;
        lives--;
        BattlegroundPlantHit.ReportDamage(gameObject, lives);
        if (lives <= 0)
        {
            SpawnImpact();
            Destroy(gameObject);
        }
    }
    private void SpawnImpact()
    {
        consumed = true;
        BattlegroundFoodBurst.Spawn(transform.position, GetComponent<SpriteRenderer>(), BattlegroundFoodBurst.Flavor.Cream);
    }

    public void TurnBlue()
    {
        GetComponent<SpriteRenderer>().color = StatusTint.Frozen;
    }
    public void TurnRed()
    {
        GetComponent<SpriteRenderer>().color = StatusTint.Burning;
    }
    public void TurnWhite()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

}
