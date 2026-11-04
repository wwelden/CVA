using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipScaler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ship;
    public float scale = 1f;

    void Start()
    {
        ship.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void ScaleShip(float scale)
    {
        ship.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void MoveShipLeft(float distance)
    {
        ship.transform.position = new Vector3(ship.transform.position.x - distance, ship.transform.position.y, ship.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        MoveShipLeft(0.01f);
        ship.transform.localScale = new Vector3(scale, scale, scale);
        scale -= 0.01f;
        if (scale <= 0.5f)
        {
            scale = 0.5f;
        }
        if (ship.transform.position.x <= -6f)
        {
            SceneFader.Load("CutScene2");
        }
    }
}
