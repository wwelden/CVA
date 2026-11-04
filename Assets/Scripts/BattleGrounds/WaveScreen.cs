using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class WaveScreen : MonoBehaviour
{
    public TextMeshProUGUI waveText; 
    public Button goToMenuBtn;  

    private void Start()
    {
        waveText.text = "Wave " + GameData.waveNumber;
        goToMenuBtn.onClick.AddListener(GoToMenu);
        GameData.isFast = false;
        GameData.paused = false;
        GameData.freeze = false;
        GameData.ResumeGame();
        GameData.burn = false;
        GameData.ClearFoodBoard();
    }

    public void GoToMenu(){
        GameData.ClearPrepProgress(); // reset kitchen-prep results; keeps high score and volume
        SceneFader.Load("KitchenPrep");//SourceInput1
    }
    void Update(){
        if (Input.GetKeyDown(KeyCode.L)){
        //     Music music = FindObjectOfType<Music>();
        // music.StopMusic();
        //     Destroy(music.gameObject);
        //     Source source = FindObjectOfType<Source>();
        //     Destroy(source.gameObject);
            GameData.SkipKitchenPrep();
        }
        if (Input.GetKeyDown(KeyCode.G)){
            GameData.globalFoodReserves += 1000;
        }
    }
}
