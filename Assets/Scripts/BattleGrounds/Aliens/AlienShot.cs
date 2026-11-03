using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienShot : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    public float mSpeed;
    public GameObject shot;
    public GameObject impactPrefab;
    private bool impacted;
    private const float ImpactLifetime = 0.3f;
    int count;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mSpeed = 5;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.left * mSpeed * Time.deltaTime);

        if(transform.position.x < -10 || transform.position.x > 10 || transform.position.y < -5 || transform.position.y > 5) {
            Destroy(shot.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (!impacted && other.gameObject.CompareTag("Food")){
            impacted = true;
            if (impactPrefab != null)
            {
                GameObject impact = Instantiate(impactPrefab, transform.position, Quaternion.identity);
                Destroy(impact, ImpactLifetime);
            }
            Destroy(shot.gameObject);
        }
    }
}
