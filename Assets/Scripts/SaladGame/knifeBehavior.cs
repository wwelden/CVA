using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;


public class knifeBehavior : MonoBehaviour
{
    public GameObject cabbagePrefab;
    public GameObject bokChoyPrefab;
    public GameObject brusselSproutPrefab;
    public GameObject tomatoPrefab;

    public chopped chopped;

    public int choppedCabbage = 0;
    public int choppedBokChoy = 0;
    public int choppedbrusselSprout = 0;
    public int choppedTomato = 0;
    public int saladMade = 0;

    public int foodCollected = 0;

    public List<String> foodOrder = new List<string> {"Cabbage", "Brussel Sprout", "Bok Choy", "Tomato"};

    void Update() {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // mousePosition.z = 0; // Set z to 0 since we're in 2D
        transform.position = mousePosition;
    }
    void Start(){
        InvokeRepeating("spawnIngredients", 1.1f, 2f);
        InvokeRepeating("spawnIngredients", 0.7f, 3f);
        InvokeRepeating("spawnIngredients", 0.5f, 5f);
        InvokeRepeating("spawnIngredients", 0.3f, 7f);
        InvokeRepeating("spawnIngredients", 0.2f, 11f);
    }

    private void addFoodReserves(int amount){
        // GameData.globalFoodReserves += amount;
        foodCollected += amount;
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (foodOrder.Contains(collision.gameObject.tag)) GameAudio.Play(Sfx.Chop, 0.8f, 0.15f);
        if(collision.gameObject.CompareTag("Cabbage")){
            choppedCabbage++;
            Debug.Log("CHOPPED CABBAGE!!");
            addFoodReserves(2);
            chopped.chopCabbage(collision.transform.position);
            Destroy(collision.gameObject);
        }
        if(collision.gameObject.CompareTag("Brussel Sprout")){
            choppedbrusselSprout++;

            Debug.Log("CHOPPED BRUSSEL SPROUT!!");
            addFoodReserves(10);
            chopped.chopBrusselSprout(collision.transform.position);
            Destroy(collision.gameObject);
        }
        if(collision.gameObject.CompareTag("Bok Choy")){
            choppedBokChoy++;

            Debug.Log("CHOPPED BOK CHOY!!");
            addFoodReserves(4);
            chopped.chopBokChoy(collision.transform.position);
            Destroy(collision.gameObject);
        }
        if(collision.gameObject.CompareTag("Tomato")){
            choppedTomato++;

            Debug.Log("CHOPPED TOMATO!!");
            addFoodReserves(6);
            chopped.chopTomato(collision.transform.position);
            Destroy(collision.gameObject);
        }

        if(choppedCabbage >= 1 && choppedbrusselSprout >= 1 && choppedBokChoy >= 1 && choppedTomato >= 1){
            saladMade += 1;
            choppedCabbage = 0;
            choppedbrusselSprout = 0;
            choppedBokChoy = 0;
            choppedTomato = 0;

        }

        // Destroy(collision.gameObject);
    }

    void spawnIngredients(){
        int randNum = UnityEngine.Random.Range(-4, 4);
        int randIndex = UnityEngine.Random.Range(0, 4);
        GameObject foodObject;

        String spawnedFood = foodOrder[randIndex];
        if (spawnedFood == "Cabbage"){
                foodObject = cabbagePrefab;
        }
        else if (spawnedFood == "Brussel Sprout"){
            foodObject = brusselSproutPrefab;

        }
        else if (spawnedFood == "Bok Choy"){
            foodObject = bokChoyPrefab;
        }
        else {
            foodObject = tomatoPrefab;
        }

        // UnityEngine.Vector3 foodPos = new UnityEngine.Vector3(randNum , 4, 0);
        Instantiate(foodObject, GetRandomPosition(), Quaternion.identity);
    }
    public float spawnOffsetY = 10f;
    public float minX = -5f;
    public float maxX = 5f;

    public Vector2 GetRandomPosition()
    {
        float randomY = UnityEngine.Random.Range(1, 5)+spawnOffsetY;
        float randomX = UnityEngine.Random.Range(minX, maxX);
        return new Vector2(randomX, randomY);
    }
    // public void GameOver(){
    //     PlayerPrefs.SetInt("inMinigame", 0);
    //     PlayerPrefs.SetInt("Veggie", foodCollected);
    //     UnityEngine.SceneManagement.SceneFader.Load("KitchenPrep");
    // }

}
