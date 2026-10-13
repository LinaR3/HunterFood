using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    public float topBound = 30f;
    public float lowerBound = -10f;

    private GameOverManagerScriptFarm gameOverManager;

    void Start()
    {
        gameOverManager = FindFirstObjectByType<GameOverManagerScriptFarm>();

        if (gameOverManager == null)
        {
            Debug.LogError("No se encontró GameOverManagerScriptFarm en la escena.");
        }
    }

    void Update()
    {
        if (transform.position.z > topBound)
        {
            Destroy(gameObject);
        }
        else if (transform.position.z < lowerBound)
        {
            if (gameOverManager != null)
            {
                gameOverManager.ObjetoPaso();
            }

            Destroy(gameObject);
        }
    }
}

