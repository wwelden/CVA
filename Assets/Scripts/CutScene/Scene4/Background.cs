using UnityEngine;
using UnityEngine.SceneManagement;
public class Background : MonoBehaviour
{
    public GameObject background1;
    public GameObject background2;
    public GameObject ship;

    void Update()
    {
        float speed = 5f * Time.deltaTime;
        background1.transform.Translate(Vector3.right * speed);
        background2.transform.Translate(Vector3.right * speed);
        ship.transform.Translate(Vector3.right * speed);
        if (background2.transform.position.x >= 0)
        {
            SceneFader.Load("WaveScreen");
        }

    }
}
