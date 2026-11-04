using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Brute : MonoBehaviour
{
    public float speed = 0.5f;
    public float lifeTime = 30f;
    public float lives = 15;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        BattlegroundAlienMotion.Attach(gameObject);
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pancakes")){

            GameData.playerScore += 500;
            BattleFeedback.ScorePopup(transform.position + Vector3.up * 0.5f, "+500");
            speed = 0;
            GetComponent<BattlegroundAlienMotion>().Pancake();
            spriteRenderer.color = new Color(0.4f, 0.075f, 0.075f, 1f); // Dark red color

        }
        if (collision.gameObject.CompareTag("Food"))
        {
            lives--;
            if (lives <= 0)
            {
                GameData.playerScore += 500;
                GameData.brutes.Remove(this);
                BattleFeedback.AlienKilled(gameObject, 500, true);
                Destroy(gameObject);
            }
            else
            {
                BattleFeedback.AlienHit(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("GameEnd"))
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        rb.linearVelocity = Vector2.zero;
        GameData.RecordHighScore();
        SceneFader.Load("GameOver");
    }
    public void TurnBlue()
    {
        // The brute's normal look is its red prefab tint, so frozen is a purple-blue blend of the two.
        GetComponent<SpriteRenderer>().color = new Color(0.55f, 0.45f, 1f, 1f);
    }
    public void TurnWhite()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        // GetComponent<SpriteRenderer>().color = Color.white;
    }
}
