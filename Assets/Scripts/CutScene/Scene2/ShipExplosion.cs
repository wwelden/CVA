using UnityEngine;
using UnityEngine.SceneManagement;
public class ShipExplosion : MonoBehaviour
{
    private float speed = 15f;
    public Vector2 pos;
    public Blast blastPrefab;
    public GameObject brokenShipPrefab;
    void Update()
    {
        Vector2 screenCenter = new Vector2((Screen.width/2) + 100, Screen.height/2);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, 0));
        worldCenter.z = transform.position.z;
        
        Vector3 direction = (worldCenter - transform.position).normalized;
        
        transform.Translate(direction * speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, worldCenter) < 0.1f)
        {
            if (speed > 0f)
            {
                SpawnBlast();
                speed = 0f;
                enabled = false;
            }
            Destroy(gameObject);
            Instantiate(brokenShipPrefab, transform.position, transform.rotation);
            SceneFader.Load("CutScene3");
        }
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90);
    }

    public void SpawnBlast()
    {
        Blast blast = Instantiate(blastPrefab, transform.position, Quaternion.identity);
    }

}
