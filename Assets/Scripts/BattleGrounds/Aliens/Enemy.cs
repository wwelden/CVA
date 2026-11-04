using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{

    private Rigidbody2D rb;

    public float speed = 0.5f;
    public float lifeTime = 30f;
    public float lives = 3;
    public SpriteRenderer spriteRenderer;
    private bool stuck;

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
        if (collision.gameObject.CompareTag("Pancakes") && !stuck){

            GameData.playerScore += 100;
            BattleFeedback.ScorePopup(transform.position + Vector3.up * 0.4f, "+100");
            StartCoroutine(StuckInSyrup());

        }
        if (collision.gameObject.CompareTag("Food"))
        {
            lives--;
            if (lives <= 0)
            {
                GameData.playerScore += 100;
                GameData.enemies.Remove(this);
                BattleFeedback.AlienKilled(gameObject, 100, false);
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

    public void SetTrajectory(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;
    }

    public void GameOver()
    {
        rb.linearVelocity = Vector2.zero;
        GameData.RecordHighScore();
        SceneFader.Load("GameOver");
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
        GetComponent<SpriteRenderer>().color = stuck ? StatusTint.Syrup : Color.white;
    }

    // Syrup holds the alien in place for a while, then wears off and it walks on.
    private IEnumerator StuckInSyrup()
    {
        stuck = true;
        float walkSpeed = speed;
        speed = 0;
        var motion = GetComponent<BattlegroundAlienMotion>();
        motion.Pancake();
        spriteRenderer.color = StatusTint.Syrup;
        yield return new WaitForSeconds(GameData.syrupStuckSeconds);
        speed = walkSpeed;
        stuck = false;
        motion.EndPancake();
        if (GameData.freeze) TurnBlue(); else TurnWhite();
    }


}
