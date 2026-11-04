using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SkipButton : MonoBehaviour
{

    public Button skipButton;

    void Start()
    {
        skipButton.onClick.AddListener(Skip);
    }

    void Skip()
    {
        // Music/Source are destroyed by the first skip, so they're absent on later playthroughs.
        Music music = FindObjectOfType<Music>();
        if (music != null) music.StopMusic();
        Source source = FindObjectOfType<Source>();
        if (source != null) Destroy(source.gameObject);
        SceneFader.Load("KitchenPrep");
    }
}
