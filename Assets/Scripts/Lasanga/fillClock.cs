
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class fillClock : MonoBehaviour
{
   public UnityEngine.UI.Image fill;
   public float currTime;
   public float totTime;
    // Update is called once per frame
    public void Start(){
     
     totTime = 100f;
    }
    public void Update(){
        currTime = PlayerPrefs.GetFloat("Timer");

        fill.fillAmount = Mathf.InverseLerp(0, totTime, currTime);
      
    }
}
