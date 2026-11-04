using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class WaveTransition : MonoBehaviour
{
    public GameObject background1;
    public GameObject background2;
    public GameObject ship;
    public GameObject alien1;
    public GameObject alien2;
    public GameObject alien3;
    private bool shouldMoveRight = true;

    public void Start(){
        GameData.isFast = false;
        GameData.paused = false;
        GameData.freeze = false;
        GameData.ResumeGame();
        GameData.burn = false;
    }

    void Update()
    {
        if (shouldMoveRight){
            moveRight();
        } else {
            moveLeft();
        }

        if (background1.transform.position.x <= 0){
            shouldMoveRight = false;
        }

    }
    void moveRight(){
        float speed = 5f * Time.deltaTime;
        background1.transform.Translate(Vector3.left * speed);
        background2.transform.Translate(Vector3.left * speed);
        ship.transform.Translate(Vector3.left * speed);
        alien1.transform.Translate(Vector3.left * speed);
        alien2.transform.Translate(Vector3.left * speed);
        alien3.transform.Translate(Vector3.left * speed);
    }
    void moveLeft(){
        float speed = 5f * Time.deltaTime;
        background1.transform.Translate(Vector3.right * speed);
        background2.transform.Translate(Vector3.right * speed);
        ship.transform.Translate(Vector3.right * speed);
        alien1.transform.Translate(Vector3.right * speed);
        alien2.transform.Translate(Vector3.right * speed);
        alien3.transform.Translate(Vector3.right * speed);
        if (background2.transform.position.x >= 0){
            SceneFader.Load("WaveScreen");
        }

    }
}

