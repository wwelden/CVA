using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SliderScript : MonoBehaviour
{
    // Start is called before the first frame update
    public bool clicked;
    public float speed;
    public Vector3 startloc;
    private bool isFacingRight = true;
    TextScript texting;
    BarScripts barScripts;
    public int level;
    public string barstatus;
    public GameObject Life1, Life2, Life3;
    public int lives = 3;


   
    void Start()
    {
        texting = GameObject.FindGameObjectWithTag("OvenText").GetComponent<TextScript>();
        barScripts = GameObject.FindGameObjectWithTag("GoodBar").GetComponent<BarScripts>();
        clicked = false;
        startloc = transform.position;
        level = 1;
        speed = 1;
        PlayerPrefs.SetInt("inMinigame", 1);
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        if (isFacingRight && transform.position.x > 5.42) Flip();
        if (!isFacingRight && transform.position.x < -5.39) Flip();
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Attempt();
        }
        
        
        
    }

    private void Flip() {
        isFacingRight = !isFacingRight;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.z);
        /*Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;*/
    }

    void Attempt()
    {
        clicked = true;
        //texting.IncreaseScore(1);
        Debug.Log("Clicked");
        GameAudio.Play(barstatus == "Good" ? Sfx.Catch : barstatus == "Bad" ? Sfx.Error : Sfx.Click, 0.8f, 0f);
        if(barstatus == "Good"){
            texting.IncreaseScore(level);
            barScripts.LevelUp();
            level = level+1;
            Debug.Log("Level?" + level);
            speed = speed+level;
        }
        if(barstatus == "Mid"){
            transform.position = startloc;
        }
        if(barstatus == "Bad"){
            //texting.IncreaseScore(-1);
            lives = lives-1;
            if (lives == 2){
            Life3.GetComponent<Renderer>().material.color = Color.red;
        }
        if (lives == 1){
            Life2.GetComponent<Renderer>().material.color = Color.red;
        }
        if (lives == 0){
            Life1.GetComponent<Renderer>().material.color = Color.red;
            speed = 0;
            texting.Complete();
        }
        }

        
    }
    private void OnTriggerEnter2D(Collider2D other){
        Debug.Log("Trigger Detector is zooming");
         if (other.gameObject.CompareTag("GoodBar")){
            barstatus = "Good";
            Debug.Log("Good Bar Detected!");
         }
         if (other.gameObject.CompareTag("MidBar")) {
            barstatus = "Mid";
            Debug.Log("Mid Bar DetecteD!");
         }
         if (other.gameObject.CompareTag("BadBar")){
            barstatus = "Bad";
            Debug.Log("Bad Bar Detected!");
         }
    }
}