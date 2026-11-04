using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class controlButtons : MonoBehaviour
{
    void Update(){
        if(Input.GetKeyDown(KeyCode.Escape)){
        SceneFader.Load("KitchenPrep");
       }
    }
    public void StartGame(){
        SceneFader.Load("OvenMinigame");

    }

     public void ExitScreen(){
        SceneFader.Load("KitchenPrep");

    }
   
}
