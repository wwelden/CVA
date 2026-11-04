
using UnityEngine;

public class Lasagna : MonoBehaviour
{
    private void Awake()
    {
        BattlegroundPlantHit.Attach(gameObject, Health);
    }

    public float speed = 2f;
    public int lives = 3;
    public float Health = 30;
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
            TakeDamage(1 * Time.deltaTime);
        }
        
    }

    public void TakeDamage(float damage){
      Health -= damage;
      if (damage > 0f) BattlegroundPlantHit.ReportDamage(gameObject, Health);
      if(Health <= 0){
        Die();
      }
    }
    public void Die(){
      GameData.lasagnaLocations.Remove(this);
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
