using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// go through plate mechanics, glitching at sometimes, when u catch a certain food plate moves down, food has force. check this
public class plateMovement : MonoBehaviour
{
    public float speed = 1f;
    public bool isMoving = false;
    public int stackCount = 0;
    public float startMousePos;
    public float mouseMove;
    public float time = 2.7f;
    public float cutTime;
    public List<String> foodOrder = new List<string> { "Pasta", "Cheese", "Sauce" };
    public List<GameObject> stack;

    private Camera mainCamera;
    public UnityEngine.Vector3 worldMousePos;
    public UnityEngine.Vector3 plateTransform;

    public bool inMinigame;
    void Start()
    {
        PlayerPrefs.SetInt("inMinigame", 1);
        mainCamera = Camera.main;
    }
    private int heardStackCount = -1;

    public void Update()
    {
        // Every ingredient that lands on the stack gets a catch sound, whichever script added it.
        if (stack != null && heardStackCount >= 0 && stack.Count > heardStackCount) GameAudio.Play(Sfx.Catch, 0.7f);
        if (stack != null) heardStackCount = stack.Count;

        if (!GameObject.FindGameObjectWithTag("Timer").GetComponent<timer>().gameOver)
        {
            time -= Time.deltaTime;
            cutTime = float.Parse(time.ToString().Split('.')[0]);

            if (cutTime == 0f)
            {
                spawnFoods();
                if (stack.Count > 5)
                {
                    time = 3.5f;
                }
                else
                {
                    time = 2.7f;
                }
            }

            // if (isMoving){
            // Get the current mouse position in screen space (X is in pixels)
            float currPos = Input.mousePosition.x;

            // Convert the mouse position from screen space to world space
            worldMousePos = mainCamera.ScreenToWorldPoint(new UnityEngine.Vector3(currPos, Input.mousePosition.y, mainCamera.nearClipPlane));

            // Set the plate's position to follow the mouse's world position (only in the X axis)
            plateTransform = transform.position;
            plateTransform.x = worldMousePos.x;

            // Clamp the X position to ensure it doesn't move beyond the screen boundaries
            plateTransform.x = Mathf.Clamp(plateTransform.x, -3, 8);

            // Update the plate's position
            transform.position = plateTransform;
            //  }

        }
    }

    void spawnFoods()
    {
        float randNum = UnityEngine.Random.Range(-3f, 1f);
        float randNum2 = UnityEngine.Random.Range(5f, 8f);
        int rand = UnityEngine.Random.Range(1, 3);
        int randIndex = UnityEngine.Random.Range(0, 2);
        GameObject foodObject;



        String spawnedFood = foodOrder[randIndex];
        UnityEngine.Vector3 foodPos = new UnityEngine.Vector3(randNum, 5, 0);
        foodObject = (GameObject)Resources.Load($"Prefabs/{spawnedFood}", typeof(GameObject));
        Instantiate(foodObject, foodPos, UnityEngine.Quaternion.identity);

        //spawn second needed item
        UnityEngine.Vector3 foodPos2 = new UnityEngine.Vector3(randNum2, 6, 0);
        foodObject = (GameObject)Resources.Load($"Prefabs/{foodOrder[0]}", typeof(GameObject));
        Instantiate(foodObject, foodPos2, UnityEngine.Quaternion.identity);
        //spawn 3rd needed item
        if (rand == 1)
        {
            UnityEngine.Vector3 foodPos3 = new UnityEngine.Vector3(3f, 7, 0);
            foodObject = (GameObject)Resources.Load($"Prefabs/{foodOrder[1]}", typeof(GameObject));
            Instantiate(foodObject, foodPos3, UnityEngine.Quaternion.identity);
        }


    }

}
