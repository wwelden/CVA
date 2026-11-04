using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class startController : MonoBehaviour
{
    public GameObject rulesPanel;
    public GameObject credits;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void OpenCredits(){
      credits.SetActive(true);
    }
     public void CloseCredits(){
        credits.SetActive(false);
    }
    // Update is called once per frame
    public void StartGame()
    {
        SceneFader.Load("CutScene");
    }
    public void OpenRules()
    {
        rulesPanel.SetActive(true);
    }
    public void ExitRules()
    {
        rulesPanel.SetActive(false);
    }
    public void StartTutorial()
    {
        SceneFader.Load("Tutorial");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameData.SkipKitchenPrep();
        }
    }


}
