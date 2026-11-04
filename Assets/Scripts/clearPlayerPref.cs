using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clearPlayerPref : MonoBehaviour
{
    public void Awake(){
        // Reset kitchen-prep progress for a fresh run, but keep the saved high score and volume.
        GameData.ClearPrepProgress();
    }
}
