
using System.Collections.Generic;
using UnityEngine;


public class Salad : MonoBehaviour
{
    private void Awake()
    {
        BattlegroundPlantHit.Attach(gameObject, Health);
    }

  private Vector2 Position;
  public float Health = 10;
  public Tomato tomatoPrefab;
  private int throwRate = 3;


  public void SetPosition(Vector2 position){
    Position = position;
  }
  public Vector2 GetPosition(){
    return Position;
  }

  public void ThrowTomato(){
   Tomato tomato = Instantiate(tomatoPrefab, Position, Quaternion.identity);
   GameData.tomatos.Add(tomato);
  }

  public void TakeDamage(float damage){
    Health -= damage;
    if (damage > 0f) BattlegroundPlantHit.ReportDamage(gameObject, Health);
    if(Health <= 0){
      Die();
    }
  }
  public void Die(){
    GameData.saladLocations.Remove(this);
    Destroy(gameObject);
  }

  public void Start(){
    InvokeRepeating("ThrowTomato", 0, throwRate);
  }

  void OnCollisionStay2D(Collision2D collision)
  {
      if (collision.gameObject.CompareTag("Enemy"))
      {
          TakeDamage(1 * Time.deltaTime);
      }
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
    public void SetTimeScale(float timeScale)
    {
        throwRate = (int)(throwRate * timeScale);
    }
}
