using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnimalHunger : MonoBehaviour
{
    private static readonly Dictionary<int, Queue<GameObject>> BarPools = new Dictionary<int, Queue<GameObject>>();
    private int applesRequired = 3;
    private int applesEaten;
    private RectTransform fillRect;
    private GameObject barObject;
    private float heightAboveAnimal = 1.5f;

    public void Initialize(int requiredApples)
    {
        applesRequired = Mathf.Max(1, requiredApples);
        applesEaten = 0;
        UpdateBar();
    }

    void Start()
    {
        CalculateBarHeight();
        CreateBar();
        UpdateBar();
    }

    public void FeedWithApple()
    {
        if (barObject == null)
        {
            CalculateBarHeight();
            CreateBar();
        }

        applesEaten = Mathf.Min(applesEaten + 1, applesRequired);
        UpdateBar();

        if (applesEaten >= applesRequired)
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        if (barObject == null)
        {
            return;
        }

        barObject.transform.position = transform.position + Vector3.up * heightAboveAnimal;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 directionToCamera = mainCamera.transform.position - barObject.transform.position;
            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                // Usa la orientación de la cámara como referencia para evitar giros
                // inestables cuando la cámara está casi directamente sobre el animal.
                barObject.transform.rotation = Quaternion.LookRotation(
                    -mainCamera.transform.forward,
                    mainCamera.transform.up);
            }
        }
    }

    void OnDestroy()
    {
        if (barObject != null)
        {
            barObject.SetActive(false);
            if (!BarPools.TryGetValue(applesRequired, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                BarPools.Add(applesRequired, pool);
            }
            pool.Enqueue(barObject);
            barObject = null;
        }
    }

    void CalculateBarHeight()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            heightAboveAnimal = 1.5f;
            return;
        }

        Bounds animalBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            animalBounds.Encapsulate(renderers[i].bounds);
        }

        heightAboveAnimal = animalBounds.max.y - transform.position.y + 0.5f;
    }

    void CreateBar()
    {
        if (barObject != null)
        {
            return;
        }

        if (BarPools.TryGetValue(applesRequired, out Queue<GameObject> pool))
        {
            while (pool.Count > 0 && barObject == null)
                barObject = pool.Dequeue();
        }
        if (barObject == null) barObject = BuildBar(applesRequired);
        barObject.transform.SetParent(null, false);
        barObject.SetActive(true);
        Canvas canvas = barObject.GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        fillRect = barObject.transform.Find("Background/Satisfaction")?.GetComponent<RectTransform>();
        if (fillRect != null) fillRect.gameObject.SetActive(false);
    }

    GameObject BuildBar(int requiredApples)
    {
        GameObject bar = new GameObject("Animal Satisfaction Bar", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = bar.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 50;

        RectTransform canvasRect = bar.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(120f, 16f);
        bar.transform.localScale = Vector3.one * 0.02f;

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundObject.transform.SetParent(canvasRect, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundObject.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.05f, 0.95f);

        GameObject fillObject = new GameObject("Satisfaction", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObject.transform.SetParent(backgroundRect, false);
        fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = new Vector2(1f, 1f);
        fillRect.offsetMax = new Vector2(-1f, -1f);

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.25f, 0.85f, 0.25f, 1f);
        fillImage.type = Image.Type.Simple;
        fillImage.raycastTarget = false;

        // Las divisiones hacen evidente cuánto aporta cada manzana al progreso.
        for (int i = 1; i < requiredApples; i++)
        {
            GameObject divider = new GameObject("Progress divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            divider.transform.SetParent(backgroundRect, false);

            RectTransform dividerRect = divider.GetComponent<RectTransform>();
            float progressPoint = (float)i / requiredApples;
            dividerRect.anchorMin = new Vector2(progressPoint, 0f);
            dividerRect.anchorMax = new Vector2(progressPoint, 1f);
            dividerRect.sizeDelta = new Vector2(2f, 0f);
            dividerRect.anchoredPosition = Vector2.zero;

            Image dividerImage = divider.GetComponent<Image>();
            dividerImage.color = new Color(0.12f, 0.08f, 0.05f, 0.95f);
            dividerImage.raycastTarget = false;
        }

        return bar;
    }

    void UpdateBar()
    {
        if (fillRect != null)
        {
            float progress = Mathf.Clamp01((float)applesEaten / applesRequired);
            fillRect.anchorMax = new Vector2(progress, 1f);
            fillRect.gameObject.SetActive(progress > 0f);
        }
    }
}
