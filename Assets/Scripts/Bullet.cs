using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public Agent shooter;
    public int damage = 1;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        Destroy(gameObject, 10);
    }

   /* public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<Agent>())
        {
            collision.gameObject.GetComponentInParent<Agent>().TakeDamage(1);
            if (collision.gameObject.GetComponentInParent<Agent>().life == 0) shooter.kills += 1;
            Destroy(gameObject);
        }
    }*/
}
