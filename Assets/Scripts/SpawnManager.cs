using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] animalPrefabs;
    public InputAction spawnAction;
    private float spawnRangeX = 20;
    private float spawnRangeZ = 20;
    public float startDelay = 2;
    public float spawnInterval = 1.5f;

    void Start()
    {
        spawnAction.Enable();
        InvokeRepeating("SpawnRandomAnimal", startDelay, spawnInterval);   
    }

    void SpawnRandomAnimal()
    {
        int animalIndex = Random.Range(0, animalPrefabs.Length);
        Vector3 spawnPosition = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, spawnRangeZ);
        Instantiate(animalPrefabs[animalIndex], spawnPosition, animalPrefabs[animalIndex].transform.rotation);
    }
}
