using UnityEngine;

public class DetectCollisions : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody appleBody = other.attachedRigidbody;
        if (appleBody == null || appleBody.GetComponent<MoveFoward>() == null)
        {
            return;
        }

        Destroy(appleBody.gameObject);

        AnimalHunger animalHunger = GetComponent<AnimalHunger>();
        if (animalHunger != null)
        {
            animalHunger.FeedWithApple();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
