using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class GameSelector : MonoBehaviour
{

    // public int WC = 1;
    // public TextMeshProUGUI waveCountText;

    // public void DisplayWaveCount()
    // {
    //     // int wc = FindFirstObjectByType<EnemySpawner>().WaveCount++;
    //     waveCountText.text = "Wave " + WC;
    // }
    // public void Start()
    // {
    //     DisplayWaveCount();
    //     WC++;
    // }
    // public void Update()
    // {
    //     DisplayWaveCount();
    // }
    public void BattleGround()
    {
        SceneFader.Load("Kitchen");
    }
    public void Menu()
    {
        SceneFader.Load("GameSelector");
    }
    public void Lasanga()
    {
        SceneFader.Load("LasagnaStartGame");
    }
    public void Pizza()
    {
        SceneFader.Load("Pizza");
    }
    public void ChopVeggies()
    {
        SceneFader.Load("SaladGame");
    }
    public void Oven()
    {
        SceneFader.Load("OvenMinigame");
    }
    


}
