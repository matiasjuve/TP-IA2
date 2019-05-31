using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public Agent shooter;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        Destroy(gameObject, 3);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Agent>()) collision.gameObject.GetComponent<Agent>().TakeDamage(1);
        if (collision.gameObject.GetComponent<Agent>().life <= 0) shooter.kills += 1;
        Destroy(gameObject);
    }
}
