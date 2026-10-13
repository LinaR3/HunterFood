using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction moveAction;
    public Vector2 moveInput;
    public float speed = 10.0f;
    public float xRange = 10.0f;
    public GameObject projectilePrefab;
    public InputAction fireAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction.Enable();
        fireAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (transform.position.x < -xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        if (transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }
        moveInput = moveAction.ReadValue<Vector2>();
        transform.Translate(Vector3.right * moveInput.x * speed * Time.deltaTime);

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -xRange, xRange);
        transform.position = position;

        if (fireAction.triggered)
        {
            Debug.Log("Fired an Apple!");
            Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        }
    }
}
