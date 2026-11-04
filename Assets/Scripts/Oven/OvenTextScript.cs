using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;




public class TextScript : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text scoretext;

    public TMP_Text pieNumber;

    public int score = 0;

    void Start()
    {
        //elsewhere = new Vector3 (100f,100f,1f);
        //Panel.SetActive(false);
        //loc = Panel.transform.position;
        //Panel.transform.position = elsewhere;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseScore(int amt) {
        score = score+amt;
        scoretext.SetText(""+score);
    }

    

    public void Menu()
    { 
        int oldScore = PlayerPrefs.GetInt("Pie", 0);
        oldScore+=score;
        PlayerPrefs.SetInt("Pie", oldScore);
        PlayerPrefs.SetInt("inMinigame", 0);
        SceneFader.Load("KitchenPrep");
    }

    public void Complete(){
        Debug.Log("Game Over");
        int oldScore = PlayerPrefs.GetInt("Pie", 0);
        oldScore+=score;
        PlayerPrefs.SetInt("Pie", oldScore);
        PlayerPrefs.SetInt("inMinigame", 0);
        SceneFader.Load("KitchenPrep");
    }

}