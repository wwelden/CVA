using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonScript : MonoBehaviour
{

    void Update(){
       if(Input.GetKeyDown(KeyCode.Escape)){
        SceneFader.Load("KitchenPrep");
       }
    }
    public void StartGame(){
          SceneFader.Load("LasagnaMiniGame");
    }

    public void ExitGame(){
           SceneFader.Load("KitchenPrep");
    }
    
}