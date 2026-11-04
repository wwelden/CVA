using UnityEngine;

public class Pancakes : MonoBehaviour
{
    private Vector2 Position;

    public Vector2 GetPosition(){
      return Position;
    }
    public void SetPosition(Vector2 position){
      Position = position;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    public void Die(){
      GameData.pancakesLocations.Remove(this);
      Destroy(gameObject);
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
