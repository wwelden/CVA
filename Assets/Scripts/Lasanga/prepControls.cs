using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class prepControls : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        Invoke("StartBattle", 6f);
    }


    public void StartBattle(){
      SceneFader.Load("Kitchen");
    }
}
