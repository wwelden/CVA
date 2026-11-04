using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectedFood
{
    public string food;

    public void SetFood(int num)
    {
        if (num == 2)
        {
            food = "salad";
        }
        else if (num == 1)
        {
            food = "pie";
        }
        else if (num == 3)
        {
            food = "lasagna";
        }
        else if (num == 4)
        {
            food = "pancakes";
        }
        else if (num == 5)
        {
            food = "icecream";
        }
        else if (num == 6)
        {
            food = "salsa";
        }
    }
    public string GetFood()
    {
        return food;
    }
    public void salad()
    {
        food = "salad";
    }
    public void pie()
    {
        food = "pie";
    }
    public void lasagna()
    {
        food = "lasagna";
    }
    public bool isSalad()
    {
        return food == "salad";
    }
    public bool isPie()
    {
        return food == "pie";
    }
    public bool isLasagna()
    {
        return food == "lasagna";
    }
    public bool isPancakes()
    {
        return food == "pancakes";
    }
    public void pancakes()
    {
        food = "pancakes";
    }
    public void icecream()
    {
        food = "icecream";
    }
    public bool isIcecream()
    {
        return food == "icecream";
    }
    public void salsa()
    {
        food = "salsa";
    }
    public bool isSalsa()
    {
        return food == "salsa";
    }
}

public class Player : MonoBehaviour
{
    public int pieCost = 1;
    public int saladCost = 10;
    public int lasagnaCost = 5;
    public int pancakesCost = 2;
    public int iceCreamCost = 20;
    public int salsaCost = 20;

    public SelectedFood selectedFood;


    public Salad saladPrefab;
    public Pie piePrefab;
    public Lasagna lasagnaPrefab;
    public Pancakes pancakesPrefab;
    public IceCream iceCreamPrefab;
    public Salsa salsaPrefab;
    public Table tablePrefab;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public void Start()
    {
        selectedFood = new SelectedFood();
        selectedFood.pie();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        PlaceFoodWithKey();
    }

    public void SetCost(int num, int cost)
    {
        if (num == 1)
        {
            pieCost = cost;
        }
        else if (num == 2)
        {
            saladCost = cost;
        }
        else if (num == 3)
        {
            lasagnaCost = cost;
        }
        else if (num == 4)
        {
            pancakesCost = cost;
        }
        else if (num == 5)
        {
            iceCreamCost = cost;
        }
        else if (num == 6)
        {
            salsaCost = cost;
        }
    }

    public void PlaceFoodWithKey(){
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            SelectPie();
        }

        if(Input.GetKeyDown(KeyCode.Alpha2)){
            SelectSalad();
        }

        if(Input.GetKeyDown(KeyCode.Alpha3)){
            SelectLasagna();
        }

        if(Input.GetKeyDown(KeyCode.Alpha4) && GameData.waveNumber >= 3){
            SelectPancakes();
        }
        if(Input.GetKeyDown(KeyCode.Alpha5) && GameData.waveNumber >= 4){
            SelectIceCream();
        }
        if(Input.GetKeyDown(KeyCode.Alpha6) && GameData.waveNumber >= 5){
            SelectSalsa();
        }
    }

    public int GetSelectedCost()
    {
        if (selectedFood == null) return 0;
        if (selectedFood.isSalad()) return saladCost;
        if (selectedFood.isLasagna()) return lasagnaCost;
        if (selectedFood.isPancakes()) return pancakesCost;
        if (selectedFood.isPie()) return pieCost;
        if (selectedFood.isIcecream()) return iceCreamCost;
        if (selectedFood.isSalsa()) return salsaCost;
        return 0;
    }

    public bool SelectedIsPie()
    {
        return selectedFood != null && selectedFood.isPie();
    }

    public void SelectFood(int num)
    {
        selectedFood.SetFood(num);
    }
    public void SelectSalad()
    {
        selectedFood.salad();
    }
    public void SelectPie()
    {
        selectedFood.pie();
    }
    public void SelectLasagna()
    {
        selectedFood.lasagna();
    }
    public void SelectPancakes()
    {
        selectedFood.pancakes();
    }
    public void SelectIceCream()
    {
        selectedFood.icecream();
    }
    public void SelectSalsa()
    {
        selectedFood.salsa();
    }

    public void PlaceTable(Vector2 position)
    {
        Table table = Instantiate(tablePrefab, position, Quaternion.identity);
        table.SetPosition(position);
        GameData.tables.Add(table);
    }
    public void PlaceSalad(Vector2 position)
    {
        Salad salad = Instantiate(saladPrefab, position, Quaternion.identity);
        salad.SetPosition(position);
        GameData.saladLocations.Add(salad);
    }
    public void ThrowPie(Vector2 position)
    {
        Pie pie = Instantiate(piePrefab, position, Quaternion.identity);
    }
    public void PlaceLasagna(Vector2 position)
    {
        Lasagna lasagna = Instantiate(lasagnaPrefab, position, Quaternion.identity);
        lasagna.SetPosition(position);
        GameData.lasagnaLocations.Add(lasagna);
    }
    public void PlacePancakes(Vector2 position)
    {
        Pancakes pancakes = Instantiate(pancakesPrefab, position, Quaternion.identity);
        pancakes.SetPosition(position);
        GameData.pancakesLocations.Add(pancakes);
    }
    public void PlaceIceCream(Vector2 position)
    {
        IceCream iceCream = Instantiate(iceCreamPrefab, position, Quaternion.identity);
        iceCream.SetPosition(position);
        GameData.icecreamLocations.Add(iceCream);
    }
    public void PlaceSalsa(Vector2 position)
    {
        Salsa salsa = Instantiate(salsaPrefab, position, Quaternion.identity);
        salsa.SetPosition(position);
        GameData.salsaLocations.Add(salsa);
    }

    public int PlaceFood(Vector2 input)
    {
        if (input.x > 5.7f || input.x < -6.3f)
        {
            return 0;
        }
        Vector2 position = GameData.GridLockPosition(input);
        if (selectedFood.isSalad() && !GameData.isOccupied(position))
        {
            PlaceSalad(position);
            return saladCost;
        }
        else if (selectedFood.isLasagna() && !GameData.isOccupied(position))
        {
            PlaceLasagna(position);
            return lasagnaCost;
        }
        else if (selectedFood.isPancakes() && !GameData.isOccupied(position))
        {
            PlacePancakes(position);
            return pancakesCost;
        }
        else if (selectedFood.isPie())
        {
            ThrowPie(position);
            return pieCost;
        }
        else if (selectedFood.isIcecream() && !GameData.isOccupied(position))
        {
            PlaceIceCream(position);
            return iceCreamCost;
        }
        else if (selectedFood.isSalsa() && !GameData.isOccupied(position))
        {
            PlaceSalsa(position);
            return salsaCost;
        }
        else
        {
            return 0;
        }
    }

}
