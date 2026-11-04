using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


public class GlobalTime : MonoBehaviour
{    
    public TMP_Text clock;
    public float timer;
    public int inMinigame;
  
    public void Start(){

      if (!PlayerPrefs.HasKey("Timer")){
       int waveNumber = GameData.waveNumber;
       float prepTime =  Mathf.Max(100 - 5 * (waveNumber - 1), 30);
       PlayerPrefs.SetFloat("Timer", prepTime);
       PlayerPrefs.Save();
      }
    }
   public void Update(){
        inMinigame = PlayerPrefs.GetInt("inMinigame");
        timer = PlayerPrefs.GetFloat("Timer");
        clock.text = timer.ToString().Split('.')[0];
          if (timer > 0){
            timer -= Time.deltaTime;
               if (timer < 0) {
               timer = 0;
              } 
            PlayerPrefs.SetFloat("Timer", timer);
            PlayerPrefs.Save();
        }

        else if (timer == 0){
          inMinigame = PlayerPrefs.GetInt("inMinigame");
          if (inMinigame == 0){
            Invoke("EndPrep", 2f);
          }
        
        }
    }

    public void EndPrep(){
       SceneFader.Load("EndPrepTime");
    }

}