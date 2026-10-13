using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnManager1 : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;
    public InputAction spawnAction;

    private float spawnRangeX = 12;
    private float spawnPosZ = 25;
    private float startDelay = 2;
    private float spawnInterval = 1.5f;

    void Start()
    {
        InvokeRepeating("SpawnRandomAsteroid", startDelay, spawnInterval);
    }

    void Update()
    {
        // Asteroide extra con la tecla S (para pruebas)
        if (spawnAction.triggered)
        {
            SpawnRandomAsteroid();
        }
    }

    void SpawnRandomAsteroid()
    {
        int asteroidIndex = Random.Range(0, asteroidPrefabs.Length - 1);
        Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, spawnPosZ);
        Instantiate(asteroidPrefabs[asteroidIndex], spawnPos, asteroidPrefabs[asteroidIndex].transform.rotation);
    }
}
