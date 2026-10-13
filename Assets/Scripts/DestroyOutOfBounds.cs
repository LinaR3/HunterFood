using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    public float topBound = 30f;
    public float lowerBound = -10f;
    public GameOverManager gameOverManager;

    void Update()
    {
        if (transform.position.z > topBound)
        {
            Destroy(gameObject);
        }
        else if (transform.position.z < lowerBound)
        {
            gameOverManager.ObjetoPaso();
            Destroy(gameObject);
        }
    }
}