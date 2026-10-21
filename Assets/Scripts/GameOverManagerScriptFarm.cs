using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManagerScriptFarm : MonoBehaviour
{
    [Header("Game Over")]
    public TMP_Text gameOverText;
    public GameObject restartButton;
    public Image imagenGameOver;
    public GameObject gameOverRoot;

    [Header("Welcome menu (Canvas objects assigned in the Inspector)")]
    public GameObject welcomeMenuRoot;
    public TMP_Text welcomeTitle;
    public TMP_Text welcomeBody;
    public Button[] welcomeButtons = new Button[7];
    public TMP_Text[] welcomeButtonLabels = new TMP_Text[7];
    public SpawnManager spawnManager;

    private readonly string[] mainLabels = { "Jugar", "Dificultad", "Cómo jugar", "Objetivo", "Opciones", "Créditos", "Salir" };
    private int objectsPassed;
    private bool gameOver;
    private float normalAnimalSpeed, normalSpawnInterval, normalFoxSpeed, normalFoxInterval;
    private int normalApples;
    private float volume = 1f;
    private string initialTitle;
    private string initialBody;
    private string[] initialButtonLabels;
    private TMP_Text gameOverSubtitle;
    private TMP_FontAsset menuFont;

    private void Start()
    {
        Time.timeScale = 0f;
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (imagenGameOver != null) imagenGameOver.gameObject.SetActive(false);
        if (spawnManager == null) spawnManager = FindFirstObjectByType<SpawnManager>();
        ConfigureMenuPresentation();
        if (spawnManager != null)
        {
            normalAnimalSpeed = spawnManager.animalSpeed;
            normalSpawnInterval = spawnManager.spawnInterval;
            normalFoxSpeed = spawnManager.foxSpeed;
            normalFoxInterval = spawnManager.foxSpawnInterval;
            normalApples = spawnManager.applesToSatisfyAnimal;
        }
        for (int i = 0; i < welcomeButtons.Length; i++)
        {
            int index = i;
            if (welcomeButtons[i] != null) welcomeButtons[i].onClick.AddListener(() => OnMenuButton(index));
        }
        initialTitle = welcomeTitle != null ? welcomeTitle.text : "HunterFood";
        initialBody = welcomeBody != null ? welcomeBody.text : "Alimenta a los animales, protege tus manzanas y ten cuidado con el zorro.";
        initialButtonLabels = new string[welcomeButtonLabels.Length];
        for (int i = 0; i < welcomeButtonLabels.Length; i++)
            initialButtonLabels[i] = welcomeButtonLabels[i] != null ? welcomeButtonLabels[i].text : mainLabels[Mathf.Min(i, mainLabels.Length - 1)];
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(true);
        ShowMainMenu();
    }

    private void ConfigureMenuPresentation()
    {
        if (welcomeTitle != null) menuFont = welcomeTitle.font;
        if (welcomeMenuRoot != null)
        {
            RectTransform menuRect = welcomeMenuRoot.GetComponent<RectTransform>();
            if (menuRect != null)
            {
                menuRect.anchorMin = Vector2.zero;
                menuRect.anchorMax = Vector2.one;
                menuRect.anchoredPosition = Vector2.zero;
                menuRect.sizeDelta = Vector2.zero;
            }

            Image backdrop = welcomeMenuRoot.GetComponent<Image>();
            if (backdrop != null)
            {
                backdrop.color = new Color(0.025f, 0.055f, 0.04f, 0.68f);
                backdrop.raycastTarget = true;
            }

            CreateCard(welcomeMenuRoot.transform, "Welcome Menu Card", new Vector2(1000f, 1040f));
            SetRect(welcomeTitle != null ? welcomeTitle.rectTransform : null, new Vector2(0f, 382f), new Vector2(740f, 108f));
            if (welcomeTitle != null) StyleText(welcomeTitle, 82f, 56f, 90f, new Color(1f, 0.78f, 0.3f));
            SetRect(welcomeBody != null ? welcomeBody.rectTransform : null, new Vector2(0f, 290f), new Vector2(720f, 130f));
            if (welcomeBody != null) StyleText(welcomeBody, 30f, 22f, 34f, new Color(1f, 0.94f, 0.78f));

            for (int i = 0; i < welcomeButtons.Length; i++)
            {
                Button button = welcomeButtons[i];
                if (button == null) continue;
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                SetRect(buttonRect, new Vector2(0f, 174f - i * 76f), new Vector2(560f, 62f));
                buttonRect.localScale = Vector3.one;
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null) buttonImage.color = new Color(0.19f, 0.39f, 0.24f, 1f);
                button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
                if (i < welcomeButtonLabels.Length && welcomeButtonLabels[i] != null)
                    StyleText(welcomeButtonLabels[i], 29f, 22f, 32f, new Color(1f, 0.96f, 0.86f));
            }
        }

        if (gameOverRoot != null)
        {
            CanvasScaler scaler = gameOverRoot.GetComponent<CanvasScaler>();
            if (scaler != null) scaler.matchWidthOrHeight = 0.5f;
            Image backdrop = imagenGameOver != null ? imagenGameOver : gameOverRoot.GetComponentInChildren<Image>(true);
            if (backdrop != null)
            {
                backdrop.color = new Color(0.025f, 0.055f, 0.04f, 0.72f);
                backdrop.raycastTarget = true;
                backdrop.rectTransform.anchorMin = Vector2.zero;
                backdrop.rectTransform.anchorMax = Vector2.one;
                backdrop.rectTransform.anchoredPosition = Vector2.zero;
                backdrop.rectTransform.sizeDelta = Vector2.zero;
            }

            CreateCard(gameOverRoot.transform, "Game Over Card", new Vector2(780f, 460f));
            SetRect(gameOverText != null ? gameOverText.rectTransform : null, new Vector2(0f, 116f), new Vector2(690f, 110f));
            if (gameOverText != null) StyleText(gameOverText, 76f, 58f, 82f, new Color(1f, 0.67f, 0.24f));

            if (gameOverSubtitle == null)
            {
                GameObject subtitleObject = new GameObject("Game Over Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                subtitleObject.transform.SetParent(gameOverRoot.transform, false);
                gameOverSubtitle = subtitleObject.GetComponent<TextMeshProUGUI>();
                gameOverSubtitle.text = "Tres animales escaparon";
                if (gameOverText != null) gameOverSubtitle.font = gameOverText.font;
            }
            SetRect(gameOverSubtitle.rectTransform, new Vector2(0f, 20f), new Vector2(660f, 64f));
            StyleText(gameOverSubtitle, 32f, 24f, 36f, new Color(1f, 0.94f, 0.78f));

            if (restartButton != null)
            {
                RectTransform restartRect = restartButton.GetComponent<RectTransform>();
                SetRect(restartRect, new Vector2(0f, -96f), new Vector2(500f, 70f));
                restartRect.localScale = Vector3.one;
                Image restartImage = restartButton.GetComponent<Image>();
                if (restartImage != null) restartImage.color = new Color(0.19f, 0.39f, 0.24f, 1f);
                TMP_Text restartLabel = restartButton.GetComponentInChildren<TMP_Text>(true);
                if (restartLabel != null)
                {
                    restartLabel.text = "Jugar de nuevo";
                    StyleText(restartLabel, 31f, 24f, 34f, new Color(1f, 0.96f, 0.86f));
                }
            }
        }

        CanvasScaler menuScaler = welcomeMenuRoot != null ? welcomeMenuRoot.GetComponentInParent<CanvasScaler>() : null;
        if (menuScaler != null) menuScaler.matchWidthOrHeight = 0.5f;
    }

    private void CreateCard(Transform parent, string cardName, Vector2 size)
    {
        Transform existing = parent.Find(cardName);
        GameObject card = existing != null ? existing.gameObject : new GameObject(cardName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (existing == null)
        {
            card.transform.SetParent(parent, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, size);
            rect.localScale = Vector3.one;
            Image image = card.GetComponent<Image>();
            image.color = new Color(0.07f, 0.19f, 0.13f, 0.98f);
            image.raycastTarget = false;
        }
        // Keep the modal above the dim overlay and below all existing controls.
        card.transform.SetSiblingIndex(cardName == "Welcome Menu Card" ? 0 : 1);
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void StyleText(TMP_Text text, float size, float minSize, float maxSize, Color color)
    {
        if (text == null) return;
        if (menuFont != null) text.font = menuFont;
        text.fontSize = size;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.enableAutoSizing = true;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.margin = new Vector4(8f, 4f, 8f, 4f);
    }

    private void OnMenuButton(int index)
    {
        if (index >= 0 && index < 7) HandleMenuAction(index);
    }

    private void HandleMenuAction(int index)
    {
        switch (index)
        {
            case 0: StartGame(); break;
            case 1: ShowPage("Dificultad", "Elige el ritmo de la partida.", new[] { "Fácil", "Normal", "Difícil", "Volver" }, 4); break;
            case 2: ShowPage("Cómo jugar", "Mueve al granjero con WASD o las flechas. Lanza manzanas con la tecla configurada. Alimenta a los animales antes de que escapen y evita que el zorro intercepte tus manzanas.", new[] { "Volver" }, 1); break;
            case 3: ShowPage("Objetivo", "Alimenta a los animales con manzanas para llenar su barra de hambre. Evita que escapen y que el zorro robe tus lanzamientos.", new[] { "Volver" }, 1); break;
            case 4: ShowPage("Opciones", "Volumen: " + Mathf.RoundToInt(volume * 100f) + "%\nPantalla completa: " + (Screen.fullScreen ? "Sí" : "No"), new[] { "- Volumen", "+ Volumen", "Pantalla completa", "Volver" }, 4); break;
            case 5: ShowPage("Créditos", "HunterFood\nCreado como proyecto del curso Unity Junior Programmer.", new[] { "Volver" }, 1); break;
            case 6: Application.Quit(); break;
            case 7: ApplyDifficulty(0.75f, 1.4f, -1, 0.85f, 1.4f); ShowMainMenu(); break;
            case 8: ApplyDifficulty(1f, 1f, 0, 1f, 1f); ShowMainMenu(); break;
            case 9: ApplyDifficulty(1.25f, 0.75f, 1, 1.15f, 0.7f); ShowMainMenu(); break;
            case 10: ShowMainMenu(); break;
            case 11: volume = Mathf.Clamp01(volume - 0.1f); AudioListener.volume = volume; UpdateOptionsText(); break;
            case 12: volume = Mathf.Clamp01(volume + 0.1f); AudioListener.volume = volume; UpdateOptionsText(); break;
            case 13: Screen.fullScreen = !Screen.fullScreen; UpdateOptionsText(); break;
        }
    }

    private void ShowMainMenu()
    {
        SetMenu(initialTitle, initialBody, initialButtonLabels, 7, 0);
    }

    private void ShowPage(string title, string body, string[] labels, int count)
    {
        SetMenu(title, body, labels, count, 0);
    }

    private void SetMenu(string title, string body, string[] labels, int count, int actionOffset)
    {
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(true);
        if (welcomeTitle != null) welcomeTitle.text = title;
        if (welcomeBody != null) welcomeBody.text = body;
        for (int i = 0; i < welcomeButtons.Length; i++)
        {
            bool active = i < count && i < labels.Length;
            if (welcomeButtons[i] != null) welcomeButtons[i].gameObject.SetActive(active);
            if (i < welcomeButtonLabels.Length && welcomeButtonLabels[i] != null && active)
                welcomeButtonLabels[i].text = labels[i];
            if (welcomeButtons[i] != null)
            {
                welcomeButtons[i].onClick.RemoveAllListeners();
                int action = actionOffset + i;
                welcomeButtons[i].onClick.AddListener(() => HandleMenuAction(action));
            }
        }
        // Page-specific buttons map to actions rather than their visible row order.
        if (title == "Dificultad")
        {
            BindActions(new[] { 7, 8, 9, 10 }, count);
        }
        else if (title == "Opciones")
        {
            BindActions(new[] { 11, 12, 13, 10 }, count);
        }
        else if (count == 1) BindActions(new[] { 10 }, count);
        else BindActions(new[] { 0, 1, 2, 3, 4, 5, 6 }, count);
    }

    private void BindActions(int[] actions, int count)
    {
        for (int i = 0; i < welcomeButtons.Length; i++)
        {
            if (welcomeButtons[i] == null) continue;
            welcomeButtons[i].onClick.RemoveAllListeners();
            if (i < count && i < actions.Length)
            {
                int action = actions[i];
                welcomeButtons[i].onClick.AddListener(() => HandleMenuAction(action));
            }
        }
    }

    private void UpdateOptionsText()
    {
        if (welcomeBody != null)
            welcomeBody.text = "Volumen: " + Mathf.RoundToInt(volume * 100f) + "%\nPantalla completa: " + (Screen.fullScreen ? "Si" : "No");
    }

    private void ApplyDifficulty(float speed, float interval, int applesDelta, float foxSpeed, float foxInterval)
    {
        if (spawnManager == null) return;
        spawnManager.animalSpeed = normalAnimalSpeed * speed;
        spawnManager.spawnInterval = normalSpawnInterval * interval;
        spawnManager.applesToSatisfyAnimal = Mathf.Max(1, normalApples + applesDelta);
        spawnManager.foxSpeed = normalFoxSpeed * foxSpeed;
        spawnManager.foxSpawnInterval = normalFoxInterval * foxInterval;
        spawnManager.UpdateSpawnSchedule();
    }

    private void StartGame()
    {
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(false);
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ObjetoPaso()
    {
        if (gameOver) return;
        objectsPassed++;
        if (objectsPassed >= 3)
        {
            gameOver = true;
            if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(false);
            if (gameOverRoot != null) gameOverRoot.SetActive(true);
            if (gameOverText != null) gameOverText.gameObject.SetActive(true);
            if (restartButton != null) restartButton.SetActive(true);
            if (imagenGameOver != null) imagenGameOver.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
