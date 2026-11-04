using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tomato : MonoBehaviour
{
    private void Awake()
    {
        BattlegroundPlantHit.Attach(gameObject, lives);
    }

  public float speed = 1;
  public int hp;

  public int damage = 1;
  public Vector2 Target;
  public Vector2 Position;
  public int lives = 1;
  private bool consumed;
  public int GetDamage(){
    return damage;
  }
  public void SetHP(int num){
      hp = num;
  }
  public void SetDamage(int dmg){
    damage = dmg;
  }
  public void move(){
    transform.Translate(Vector3.right * speed * Time.deltaTime);

  }
  public void Update(){
    move();
  }
  public void SetTarget(Vector2 target){
    Target = target;
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
          consumed = true;
          BattlegroundFoodBurst.Spawn(transform.position, GetComponent<SpriteRenderer>(), BattlegroundFoodBurst.Flavor.Tomato);
          GameData.tomatos.Remove(this);
          Destroy(gameObject);
      }
  }
  public void SetSpeed(float speed){
    this.speed = speed;
  }
  public void TurnBlue(){
    GetComponent<SpriteRenderer>().color = Color.blue;
  }
  public void TurnWhite(){
    GetComponent<SpriteRenderer>().color = Color.white;
  }
  public void TurnRed(){
    GetComponent<SpriteRenderer>().color = Color.red;
  }
}
