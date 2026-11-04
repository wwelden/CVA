using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class veggieButtons : MonoBehaviour
{
   
   public void StartGame(){
    SceneFader.Load("SaladGame");
   }
   
    public void ExitGame(){
    SceneFader.Load("KitchenPrep");
   }
    void Update(){
        if(Input.GetKeyDown(KeyCode.Escape)){
        SceneFader.Load("KitchenPrep");
       }
    }
}
