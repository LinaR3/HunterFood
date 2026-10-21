using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveFoward : MonoBehaviour
{
    public float speed = 40.0f;
    private Rigidbody body;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        if (body != null)
        {
            // Las manzanas se mueven por física para que sus triggers no atraviesen
            // el collider del zorro entre dos actualizaciones.
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    void Update()
    {
        if (body == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (body != null)
        {
            body.linearVelocity = transform.forward * speed;
        }
    }
}
