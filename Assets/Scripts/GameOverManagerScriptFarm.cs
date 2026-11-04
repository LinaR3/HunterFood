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
    private GameObject pauseRoot;
    private GameObject settingsRoot;
    private bool isPaused;
    private bool started;
    private int selectedLevel = 1;
    private int selectedDifficulty = 1;
    private bool ambienceEnabled = true;
    private bool settingsPausedGame;
    private float normalAmbientIntensity = 1f;
    private GameObject pauseButtonRoot;
    private GameObject settingsButtonRoot;

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
        normalAmbientIntensity = RenderSettings.ambientIntensity;
        volume = PlayerPrefs.GetFloat("HunterFood.Volume", 1f);
        selectedLevel = Mathf.Clamp(PlayerPrefs.GetInt("HunterFood.Level", 1), 1, 5);
        selectedDifficulty = Mathf.Clamp(PlayerPrefs.GetInt("HunterFood.Difficulty", 1), 0, 2);
        ambienceEnabled = PlayerPrefs.GetInt("HunterFood.Ambience", 1) != 0;
        AudioListener.volume = volume;
        RenderSettings.ambientIntensity = ambienceEnabled ? normalAmbientIntensity : 0f;
        ApplySelectedSettings();
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
        CreateGameplayControls();
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(false);
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

            CreateCard(welcomeMenuRoot.transform, "Welcome Menu Card", new Vector2(1000f, 620f));
            SetRect(welcomeTitle != null ? welcomeTitle.rectTransform : null, new Vector2(0f, 170f), new Vector2(820f, 150f));
            if (welcomeTitle != null) StyleText(welcomeTitle, 112f, 64f, 120f, new Color(1f, 0.78f, 0.04f));
            SetRect(welcomeBody != null ? welcomeBody.rectTransform : null, new Vector2(0f, 65f), new Vector2(720f, 100f));
            if (welcomeBody != null) StyleText(welcomeBody, 30f, 22f, 34f, new Color(1f, 0.94f, 0.78f));

            for (int i = 0; i < welcomeButtons.Length; i++)
            {
                Button button = welcomeButtons[i];
                if (button == null) continue;
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                SetRect(buttonRect, new Vector2(0f, -100f), new Vector2(560f, 92f));
                buttonRect.localScale = Vector3.one;
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null) buttonImage.color = new Color(0.92f, 0.59f, 0.06f, 1f);
                button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
                if (i < welcomeButtonLabels.Length && welcomeButtonLabels[i] != null)
                    StyleText(welcomeButtonLabels[i], 42f, 28f, 46f, new Color(0.12f, 0.10f, 0.07f));
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
        SetMenu("HUNTERFOOD", "Alimenta a los animales. ¡Cuidado con el zorro!", new[] { "JUGAR" }, 1, 0);
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
        if (title == "HUNTERFOOD")
        {
            BindActions(new[] { 0 }, count);
        }
        else if (title == "Dificultad")
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
        started = true;
        isPaused = false;
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(false);
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(true);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (started && !gameOver && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
            TogglePause();
    }

    private void CreateGameplayControls()
    {
        Transform canvas = welcomeMenuRoot != null ? welcomeMenuRoot.transform.parent : null;
        if (canvas == null) return;
        pauseRoot = BuildPanel(canvas, "Pause Panel", "PAUSA", new[] { "CONTINUAR", "REINICIAR", "SALIR AL INICIO" }, new System.Action[] { ResumeGame, Reiniciar, ReturnToMenu }, new Vector2(760f, 560f));
        settingsRoot = BuildPanel(canvas, "Settings Panel", "AJUSTES", new[] { "NIVEL: 1", "SONIDO: SÍ", "AMBIENTACIÓN: SÍ", "DIFICULTAD: NORMAL", "CERRAR" }, new System.Action[] { CycleLevel, ToggleSound, ToggleAmbience, CycleDifficulty, CloseSettings }, new Vector2(820f, 680f));
        pauseRoot.SetActive(false);
        settingsRoot.SetActive(false);
        pauseButtonRoot = BuildButton(canvas, "Pausa", "II", new Vector2(-82f, -76f), TogglePause).gameObject;
        settingsButtonRoot = BuildButton(canvas, "Ajustes", "", new Vector2(-82f, -164f), OpenSettings).gameObject;
    }

    private GameObject BuildPanel(Transform parent, string name, string title, string[] labels, System.Action[] actions, Vector2 size)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(parent, false);
        RectTransform full = root.GetComponent<RectTransform>();
        full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = Vector2.zero; full.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.025f, 0.68f);
        GameObject card = new GameObject(name + " Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(root.transform, false);
        SetRect(card.GetComponent<RectTransform>(), Vector2.zero, size);
        card.GetComponent<Image>().color = new Color(0.055f, 0.085f, 0.075f, 0.98f);
        MakeText(card.transform, title, new Vector2(0f, size.y * 0.36f), new Vector2(size.x - 80f, 90f), 60f, new Color(1f, 0.78f, 0.04f));
        float rowGap = 82f;
        float startY = (labels.Length - 1) * rowGap * 0.5f - 20f;
        for (int i = 0; i < labels.Length; i++)
        {
            int index = i;
            BuildButton(card.transform, name + " " + i, labels[i], new Vector2(0f, startY - i * rowGap), () => actions[index]());
        }
        return root;
    }

    private Button BuildButton(Transform parent, string name, string label, Vector2 position, System.Action action)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        bool compact = parent == (welcomeMenuRoot != null ? welcomeMenuRoot.transform.parent : null);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if (compact)
        {
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = new Vector2(70f, 70f);
        }
        else SetRect(buttonRect, position, new Vector2(530f, 68f));
        Image image = buttonObject.GetComponent<Image>();
        image.color = compact ? new Color(0.04f, 0.07f, 0.06f, 0.9f) : new Color(0.19f, 0.39f, 0.24f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() => action());
        if (compact && name == "Ajustes")
        {
            GameObject icon = new GameObject("Settings Gear", typeof(RectTransform), typeof(CanvasRenderer), typeof(GearIconGraphic));
            icon.transform.SetParent(buttonObject.transform, false);
            SetRect(icon.GetComponent<RectTransform>(), Vector2.zero, new Vector2(38f, 38f));
            icon.GetComponent<GearIconGraphic>().color = new Color(1f, 0.78f, 0.18f, 1f);
        }
        else
        {
            MakeText(buttonObject.transform, label, Vector2.zero, compact ? new Vector2(68f, 68f) : new Vector2(500f, 60f), compact ? 34f : 30f, new Color(1f, 0.9f, 0.55f));
        }
        return button;
    }

    private void MakeText(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject go = new GameObject(value + " Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.text = value; text.font = menuFont; text.fontSize = fontSize; text.color = color;
        text.alignment = TextAlignmentOptions.Center; text.enableAutoSizing = true; text.fontSizeMin = 18f; text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        SetRect(text.rectTransform, position, size);
    }

    private void TogglePause()
    {
        if (isPaused) ResumeGame();
        else
        {
            isPaused = true;
            Time.timeScale = 0f;
            if (pauseRoot != null) { pauseRoot.SetActive(true); pauseRoot.transform.SetAsLastSibling(); }
        }
    }

    private void ResumeGame() { isPaused = false; if (pauseRoot != null) pauseRoot.SetActive(false); Time.timeScale = 1f; }
    private void OpenSettings()
    {
        settingsPausedGame = Time.timeScale > 0f;
        Time.timeScale = 0f;
        UpdateSettingsLabels();
        if (settingsRoot != null) { settingsRoot.SetActive(true); settingsRoot.transform.SetAsLastSibling(); }
    }
    private void CloseSettings()
    {
        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (!isPaused && settingsPausedGame) Time.timeScale = 1f;
    }
    private void ToggleSound() { volume = volume > 0f ? 0f : 1f; AudioListener.volume = volume; PlayerPrefs.SetFloat("HunterFood.Volume", volume); UpdateSettingsLabels(); }
    private void ToggleAmbience() { ambienceEnabled = !ambienceEnabled; RenderSettings.ambientIntensity = ambienceEnabled ? normalAmbientIntensity : 0f; PlayerPrefs.SetInt("HunterFood.Ambience", ambienceEnabled ? 1 : 0); UpdateSettingsLabels(); }
    private void CycleLevel() { selectedLevel = selectedLevel % 5 + 1; PlayerPrefs.SetInt("HunterFood.Level", selectedLevel); ApplySelectedSettings(); UpdateSettingsLabels(); }
    private void CycleDifficulty() { selectedDifficulty = (selectedDifficulty + 1) % 3; PlayerPrefs.SetInt("HunterFood.Difficulty", selectedDifficulty); ApplySelectedSettings(); UpdateSettingsLabels(); }
    private void ApplySelectedSettings()
    {
        if (spawnManager == null) return;
        float[] speeds = { 0.8f, 1f, 1.2f };
        float[] intervals = { 1.25f, 1f, 0.8f };
        float levelFactor = 1f + (selectedLevel - 1) * 0.08f;
        spawnManager.animalSpeed = normalAnimalSpeed * speeds[selectedDifficulty] * levelFactor;
        spawnManager.spawnInterval = normalSpawnInterval * intervals[selectedDifficulty] / levelFactor;
        spawnManager.foxSpeed = normalFoxSpeed * speeds[selectedDifficulty] * levelFactor;
        spawnManager.foxSpawnInterval = normalFoxInterval * intervals[selectedDifficulty] / levelFactor;
        spawnManager.applesToSatisfyAnimal = Mathf.Max(1, normalApples + selectedDifficulty - 1 + (selectedLevel - 1) / 2);
        spawnManager.UpdateSpawnSchedule();
    }
    private void UpdateSettingsLabels()
    {
        if (settingsRoot == null) return;
        TMP_Text[] texts = settingsRoot.GetComponentsInChildren<TMP_Text>(true);
        string[] values = { "NIVEL: " + selectedLevel, "SONIDO: " + (volume > 0f ? "SÍ" : "NO"), "AMBIENTACIÓN: " + (ambienceEnabled ? "SÍ" : "NO"), "DIFICULTAD: " + new[] { "FÁCIL", "NORMAL", "DIFÍCIL" }[selectedDifficulty], "CERRAR" };
        int found = 0;
        foreach (TMP_Text text in texts) if (text.text != "AJUSTES" && found < values.Length) text.text = values[found++];
    }
    private void ReturnToMenu() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    public void ObjetoPaso()
    {
        if (gameOver) return;
        objectsPassed++;
        if (objectsPassed >= 3)
        {
            gameOver = true;
            if (pauseButtonRoot != null) pauseButtonRoot.SetActive(false);
            if (settingsButtonRoot != null) settingsButtonRoot.SetActive(false);
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
