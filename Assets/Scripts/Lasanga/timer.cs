using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System;


public class timer : MonoBehaviour {
    public GameObject winPanel;
    public TMP_Text text;

    public TMP_Text loseText;
     public TMP_Text winPanelText;
    float countdown = 25;
    public GameObject losePanel; 
    public GameObject plate; 
    public int earnedMonsters = 0;
    public bool gameOver = false;

    void Update(){
      if(!gameOver){
        if (countdown > 0){
            countdown -= Time.deltaTime;
        }

        if (countdown < 0){
            winPanel.SetActive(true);
            gameOver = true;
           
            if (plate.GetComponent<plateMovement>().stack.Count <= 3){
                earnedMonsters = 15;
            }
            else if (plate.GetComponent<plateMovement>().stack.Count <= 6){
                earnedMonsters = 40;
            }
            else if(plate.GetComponent<plateMovement>().stack.Count <= 8){
                earnedMonsters = 55;
            }
            else {
                earnedMonsters = 65;
            }
          int oldTot = PlayerPrefs.GetInt("Lasagna");
          int newTot = oldTot + earnedMonsters;
          PlayerPrefs.SetInt("Lasagna", newTot);
          winPanelText.text = $"You earned {earnedMonsters} Food Reserves!";
          PlayerPrefs.SetInt("inMinigame", 0);
          Invoke("MainMenu", 3f);
        }

        text.text = countdown.ToString().Split('.')[0];
      }
    }

    public void EndGame(){
         gameOver = true;
         losePanel.SetActive(true);
         PlayerPrefs.SetInt("inMinigame", 0);

        if (plate.GetComponent<plateMovement>().stack.Count <= 4){
                earnedMonsters = 15;
            }
         else if(plate.GetComponent<plateMovement>().stack.Count <= 5){
            earnedMonsters = 30;
         }
         else {
            earnedMonsters = 40;
         }
          int oldTot = PlayerPrefs.GetInt("Lasagna");
          int newTot = oldTot + earnedMonsters;
          PlayerPrefs.SetInt("Lasagna", newTot);
          loseText.text = $"You earned {earnedMonsters} Food Reserves.";
          PlayerPrefs.SetInt("inMinigame", 0);
          Invoke("MainMenu", 3f);
        }

     public void MainMenu(){
         Debug.Log("startGame");
         SceneFader.Load("KitchenPrep");
        }
    }

