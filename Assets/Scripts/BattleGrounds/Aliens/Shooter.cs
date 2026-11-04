using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shooter : MonoBehaviour
{
    //public Sprite[] sprites;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PantryProwlerAnimation attackAnimation;

    public float speed = 0.5f;
    public float lifeTime = 30f;
    public float lives = 10;
    public bool reloading = false;
    public bool pancaked = false;
    public float reloadtime = 3f;
    public float loadprog = 0f;
    public GameObject shotPrefab;

    public GameObject audioSource;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        attackAnimation = GetComponent<PantryProwlerAnimation>();
        BattlegroundAlienMotion.Attach(gameObject);
    }

    void Update(){

        if (reloading == false){
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pancakes")){
            pancaked = true;
            GameData.playerScore += 250;
            BattleFeedback.ScorePopup(transform.position + Vector3.up * 0.4f, "+250");
            speed = 0;
            GetComponent<BattlegroundAlienMotion>().Pancake();
            spriteRenderer.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown color

        }
        if (collision.gameObject.CompareTag("Food"))
        {
            lives--;
            if (lives <= 0)
            {
                GameData.playerScore += 250;
                GameData.shooters.Remove(this);
                BattleFeedback.AlienKilled(gameObject, 250, false);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reloading || pancaked) return;
        bool isPlant = other.GetComponentInParent<Salad>() != null ||
            other.GetComponentInParent<Lasagna>() != null ||
            other.GetComponentInParent<IceCream>() != null ||
            other.GetComponentInParent<Salsa>() != null ||
            other.GetComponentInParent<Pancakes>() != null;
        if (isPlant || other.CompareTag("Food"))
        {
            reloading = true;
            StartCoroutine(FireAndReload());
        }
    }

    IEnumerator FireAndReload()
    {
        bool animated = attackAnimation != null && attackAnimation.isActiveAndEnabled;
        if (animated)
        {
            attackAnimation.BeginShot();
            yield return new WaitForSeconds(PantryProwlerAnimation.ShotReleaseTime);
        }

        // A pancake during the wind-up cancels the pending projectile.
        if (!pancaked && (!animated || (attackAnimation.isActiveAndEnabled && attackAnimation.IsShooting)))
        {
            Vector3 origin = animated ? attackAnimation.ShotPosition : transform.position;
            Instantiate(shotPrefab, origin, transform.rotation);
            audioSource = GameObject.FindGameObjectWithTag("AudioManagerLaser");
            if (audioSource != null)
            {
                AudioManager2 audio = audioSource.GetComponent<AudioManager2>();
                if (audio != null)
                    audio.playLaser = true;
            }
        }

        if (animated)
        {
            yield return new WaitForSeconds(PantryProwlerAnimation.ShotDuration -
                PantryProwlerAnimation.ShotReleaseTime);
            attackAnimation.EndShot();
        }
        yield return new WaitForSeconds(reloadtime);
        reloading = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        reloading = false;
        if (attackAnimation != null)
            attackAnimation.EndShot();
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
