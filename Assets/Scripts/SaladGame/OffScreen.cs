using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class OffScreen : MonoBehaviour
{

    public void Start(){
        livesText.text = $"LIVES: {lives}";
    }

    public int lives = 5;
    public TextMeshProUGUI livesText;
    public GameObject knife;
    public void GameOver(){
        PlayerPrefs.SetInt("inMinigame", 0);
        int totalSalad = PlayerPrefs.GetInt("Veggie");
        int saladMade = knife.GetComponent<knifeBehavior>().foodCollected + totalSalad;
        PlayerPrefs.SetInt("Veggie", saladMade);
        SceneFader.Load("KitchenPrep");
    }

    void OnCollisionEnter2D(Collision2D collision){
        if(collision.gameObject.tag == "Cabbage" || collision.gameObject.tag == "Brussel Sprout"|| collision.gameObject.tag == "Bok Choy"|| collision.gameObject.tag == "Tomato"){
            lives--;
            livesText.text = $"LIVES: {lives}";
            Destroy(collision.gameObject);
            if(lives == 0){
                GameOver();
            }
        }

    }

}
