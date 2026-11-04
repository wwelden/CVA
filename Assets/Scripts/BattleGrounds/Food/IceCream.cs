using UnityEngine;

public class IceCream : MonoBehaviour
{
    private void Awake()
    {
        BattlegroundPlantHit.Attach(gameObject, Health);
    }

    private Vector2 Position;
    private float Health = 10;
    // private float duration = 20;

    public Vector2 GetPosition()
    {
        return Position;
    }
    public void SetPosition(Vector2 position)
    {
        Position = position;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1 * Time.deltaTime);
        }
    }
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (damage > 0f) BattlegroundPlantHit.ReportDamage(gameObject, Health);
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        GameData.setTimeScale(0.5f);
        GameData.freeze = true;
        GameData.icecreamLocations.Remove(this);
        Destroy(gameObject);
    }
    public void Die2()
    {
        GameData.icecreamLocations.Remove(this);
        Destroy(gameObject);
    }
    public void TurnBlue()
    {
        GetComponent<SpriteRenderer>().color = Color.blue;
    }
    public void TurnRed()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }
    public void TurnWhite()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

}
