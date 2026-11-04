using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManagerScriptFarm : MonoBehaviour
{
    public static GameOverManagerScriptFarm Instance { get; private set; }

    [Header("Game Over")]
    public TMP_Text gameOverText;
    public GameObject restartButton;
    public Image imagenGameOver;
    public GameObject gameOverRoot;

    [Header("Inicio")]
    public GameObject welcomeMenuRoot;
    public TMP_Text welcomeTitle;
    public TMP_Text welcomeBody;
    public Button[] welcomeButtons = new Button[7];
    public TMP_Text[] welcomeButtonLabels = new TMP_Text[7];
    public SpawnManager spawnManager;

    private static readonly Color Gold = new Color(1f, 0.78f, 0.02f, 1f);
    private static readonly Color Panel = new Color(0.035f, 0.045f, 0.055f, 0.96f);
    private static readonly Color Row = new Color(0.085f, 0.095f, 0.105f, 0.98f);

    private int objectsPassed;
    private bool gameOver;
    private bool started;
    private bool isPaused;
    private bool settingsPausedGame;
    private bool settingsFromStartScreen;
    private int selectedLevel = 1;
    private int selectedDifficulty = 1;
    private int applesFedThisLevel;
    private bool ambienceEnabled = true;
    private float volume = 1f;
    private float normalAnimalSpeed;
    private float normalSpawnInterval;
    private float normalFoxSpeed;
    private float normalFoxInterval;
    private int normalApples;
    private float normalAmbientIntensity = 1f;
    private TMP_FontAsset uiFont;
    private GameObject pauseRoot;
    private GameObject settingsRoot;
    private GameObject gameplayHud;
    private GameObject pauseButtonRoot;
    private GameObject settingsButtonRoot;
    private TMP_Text[] settingsValues;
    private TMP_Text levelHud;
    private TMP_Text foodHud;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 0f;
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (imagenGameOver != null) imagenGameOver.gameObject.SetActive(false);
        if (spawnManager == null) spawnManager = FindFirstObjectByType<SpawnManager>();
        CaptureGameplayDefaults();
        LoadPreferences();
        ConfigureStartScreen();
        CreateRuntimeUI();
        ShowStartScreen();
    }

    private void CaptureGameplayDefaults()
    {
        if (spawnManager != null)
        {
            normalAnimalSpeed = spawnManager.animalSpeed;
            normalSpawnInterval = spawnManager.spawnInterval;
            normalFoxSpeed = spawnManager.foxSpeed;
            normalFoxInterval = spawnManager.foxSpawnInterval;
            normalApples = spawnManager.applesToSatisfyAnimal;
        }
        normalAmbientIntensity = RenderSettings.ambientIntensity;
    }

    private void LoadPreferences()
    {
        volume = PlayerPrefs.GetFloat("HunterFood.Volume", 1f);
        selectedLevel = Mathf.Clamp(PlayerPrefs.GetInt("HunterFood.Level", 1), 1, 5);
        selectedDifficulty = Mathf.Clamp(PlayerPrefs.GetInt("HunterFood.Difficulty", 1), 0, 2);
        ambienceEnabled = PlayerPrefs.GetInt("HunterFood.Ambience", 1) != 0;
        AudioListener.volume = volume;
        RenderSettings.ambientIntensity = ambienceEnabled ? normalAmbientIntensity : 0f;
        ApplySelectedSettings();
    }

    private void ConfigureStartScreen()
    {
        uiFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
        if (uiFont == null && welcomeTitle != null) uiFont = welcomeTitle.font;

        RectTransform rootRect = welcomeMenuRoot != null ? welcomeMenuRoot.GetComponent<RectTransform>() : null;
        if (rootRect != null) Stretch(rootRect);
        Image rootImage = welcomeMenuRoot != null ? welcomeMenuRoot.GetComponent<Image>() : null;
        if (rootImage != null)
        {
            rootImage.color = new Color(0f, 0f, 0f, 0f);
            rootImage.raycastTarget = false;
        }

        Transform oldCard = welcomeMenuRoot != null ? welcomeMenuRoot.transform.Find("Welcome Menu Card") : null;
        if (oldCard != null) oldCard.gameObject.SetActive(false);

        if (welcomeTitle != null)
        {
            welcomeTitle.gameObject.SetActive(true);
            welcomeTitle.text = "HUNTERFOOD";
            SetRect(welcomeTitle.rectTransform, new Vector2(0f, 270f), new Vector2(1900f, 240f));
            StyleText(welcomeTitle, 190f, Gold, TextAlignmentOptions.Center, true);
        }

        if (welcomeBody != null)
        {
            welcomeBody.text = string.Empty;
            welcomeBody.gameObject.SetActive(false);
        }

        for (int i = 0; i < welcomeButtons.Length; i++)
        {
            Button button = welcomeButtons[i];
            if (button == null) continue;
            bool isPlay = i == 0;
            button.gameObject.SetActive(isPlay);
            button.onClick.RemoveAllListeners();
            if (!isPlay) continue;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = Panel;
                image.raycastTarget = true;
            }
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0f, -155f), new Vector2(700f, 120f));
            if (button.transform.Find("Yellow Angular Frame") == null)
                AddAngularFrame(button.transform, 5f, 0.16f);
            button.onClick.AddListener(StartGame);
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

            TMP_Text label = i < welcomeButtonLabels.Length ? welcomeButtonLabels[i] : button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = "JUGAR";
                label.gameObject.SetActive(true);
                Stretch(label.rectTransform);
                StyleText(label, 58f, Gold, TextAlignmentOptions.Center, true);
            }
        }
    }

    private void CreateRuntimeUI()
    {
        Transform canvas = welcomeMenuRoot != null ? welcomeMenuRoot.transform.parent : null;
        if (canvas == null) return;

        gameplayHud = CreateHud(canvas);
        pauseRoot = CreatePausePanel(canvas);
        settingsRoot = CreateSettingsPanel(canvas);
        pauseButtonRoot = BuildTopControl(canvas, "Pausa", new Vector2(-130f, -42f), false, TogglePause).gameObject;
        Transform savedSettingsButton = canvas.Find("Settings Button");
        Button settingsButton = savedSettingsButton != null ? savedSettingsButton.GetComponent<Button>() : null;
        if (settingsButton == null)
        {
            settingsButton = BuildTopControl(canvas, "Ajustes", new Vector2(-42f, -42f), true, OpenSettings);
        }
        else
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OpenSettings);
        }
        settingsButtonRoot = settingsButton.gameObject;

        gameplayHud.SetActive(false);
        pauseRoot.SetActive(false);
        settingsRoot.SetActive(false);
        pauseButtonRoot.SetActive(false);
    }

    private GameObject CreateHud(Transform parent)
    {
        GameObject hud = new GameObject("Gameplay HUD", typeof(RectTransform));
        hud.transform.SetParent(parent, false);
        Stretch(hud.GetComponent<RectTransform>());

        levelHud = CreateHudTag(hud.transform, "Level HUD", "NIVEL 1", new Vector2(46f, -42f), new Vector2(225f, 64f));
        foodHud = CreateHudTag(hud.transform, "Food HUD", "COMIDA 03", new Vector2(285f, -42f), new Vector2(280f, 64f));
        return hud;
    }

    private TMP_Text CreateHudTag(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        GameObject tag = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tag.transform.SetParent(parent, false);
        Image image = tag.GetComponent<Image>();
        image.sprite = null;
        image.color = Panel;
        SetTopLeftRect(tag.GetComponent<RectTransform>(), position, size);
        AddAngularFrame(tag.transform, 3f, 0.18f);
        TMP_Text text = CreateText(tag.transform, label, Vector2.zero, size - new Vector2(18f, 8f), 30f, Gold, TextAlignmentOptions.Center);
        return text;
    }

    private GameObject CreatePausePanel(Transform parent)
    {
        GameObject root = CreateOverlay(parent, "Pause Overlay");
        RectTransform card = CreatePanelCard(root.transform, "Pause Panel", new Vector2(780f, 560f));
        CreateText(card, "PAUSA", new Vector2(0f, 210f), new Vector2(680f, 100f), 72f, Gold, TextAlignmentOptions.Center, true);
        BuildModalButton(card, "CONTINUAR", new Vector2(0f, 80f), new Vector2(560f, 74f), ResumeGame);
        BuildModalButton(card, "REINICIAR", new Vector2(0f, -20f), new Vector2(560f, 74f), Reiniciar);
        BuildModalButton(card, "SALIR AL INICIO", new Vector2(0f, -120f), new Vector2(560f, 74f), ReturnToMenu);
        return root;
    }

    private GameObject CreateSettingsPanel(Transform parent)
    {
        GameObject root = CreateOverlay(parent, "Settings Overlay");
        RectTransform card = CreatePanelCard(root.transform, "Settings Panel", new Vector2(900f, 700f));
        CreateText(card, "AJUSTES", new Vector2(0f, 270f), new Vector2(700f, 96f), 68f, Gold, TextAlignmentOptions.Center, true);

        Button close = BuildModalButton(card, "X", new Vector2(365f, 270f), new Vector2(58f, 58f), CloseSettings, true);
        if (close.GetComponentInChildren<TMP_Text>() != null) close.GetComponentInChildren<TMP_Text>().fontSize = 38f;

        string[] labels = { "NIVEL", "SONIDO", "AMBIENTACIÓN", "DIFICULTAD" };
        float[] rowY = { 145f, 40f, -65f, -170f };
        settingsValues = new TMP_Text[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            int row = i;
            Button option = CreateSettingsRow(card, labels[i], rowY[i], () => ActivateSetting(row));
            Transform valueLabel = option.transform.Find("Setting Value");
            settingsValues[i] = valueLabel != null ? valueLabel.GetComponent<TMP_Text>() : null;
        }

        BuildModalButton(card, "GUARDAR", new Vector2(0f, -280f), new Vector2(330f, 76f), CloseSettings, false, true);
        UpdateSettingsLabels();
        return root;
    }

    private Button CreateSettingsRow(Transform parent, string label, float y, UnityEngine.Events.UnityAction action)
    {
        GameObject row = new GameObject(label + " Row", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        row.transform.SetParent(parent, false);
        SetRect(row.GetComponent<RectTransform>(), new Vector2(0f, y), new Vector2(730f, 76f));
        row.GetComponent<Image>().color = Row;
        Button button = row.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.AddListener(action);
        AddAngularFrame(row.transform, 2.5f, 0.12f);

        CreateText(row.transform, label, new Vector2(-170f, 0f), new Vector2(350f, 64f), 30f, Color.white, TextAlignmentOptions.Left);
        TMP_Text value = CreateText(row.transform, "", new Vector2(230f, 0f), new Vector2(170f, 64f), 30f, Gold, TextAlignmentOptions.Center);
        value.gameObject.name = "Setting Value";
        return button;
    }

    private void ActivateSetting(int index)
    {
        switch (index)
        {
            case 0: CycleLevel(); break;
            case 1: ToggleSound(); break;
            case 2: ToggleAmbience(); break;
            case 3: CycleDifficulty(); break;
        }
        UpdateSettingsLabels();
    }

    private GameObject CreateOverlay(Transform parent, string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());
        Image overlay = root.GetComponent<Image>();
        overlay.color = new Color(0.015f, 0.02f, 0.03f, 0.62f);
        overlay.raycastTarget = true;
        return root;
    }

    private RectTransform CreatePanelCard(Transform parent, string name, Vector2 size)
    {
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(parent, false);
        SetRect(card.GetComponent<RectTransform>(), Vector2.zero, size);
        Image image = card.GetComponent<Image>();
        image.sprite = null;
        image.color = Panel;
        image.raycastTarget = false;
        AddAngularFrame(card.transform, 5f, 0.12f);
        return card.GetComponent<RectTransform>();
    }

    private Button BuildTopControl(Transform parent, string name, Vector2 position, bool gear, UnityEngine.Events.UnityAction action)
    {
        GameObject control = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        control.transform.SetParent(parent, false);
        SetTopRightRect(control.GetComponent<RectTransform>(), position, new Vector2(66f, 66f));
        Image image = control.GetComponent<Image>();
        image.sprite = null;
        image.color = new Color(0.025f, 0.035f, 0.045f, 0.82f);
        Button button = control.GetComponent<Button>();
        button.onClick.AddListener(action);
        AddAngularFrame(control.transform, 3f, 0.16f);

        if (gear)
        {
            GameObject icon = new GameObject("Gear Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(GearIconGraphic));
            icon.transform.SetParent(control.transform, false);
            SetRect(icon.GetComponent<RectTransform>(), Vector2.zero, new Vector2(34f, 34f));
            icon.GetComponent<GearIconGraphic>().color = Gold;
            icon.GetComponent<GearIconGraphic>().raycastTarget = false;
        }
        else
        {
            CreatePauseBars(control.transform);
        }
        return button;
    }

    private void CreatePauseBars(Transform parent)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject bar = new GameObject("Pause Bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bar.transform.SetParent(parent, false);
            SetRect(bar.GetComponent<RectTransform>(), new Vector2(i == 0 ? -8f : 8f, 0f), new Vector2(8f, 30f));
            bar.GetComponent<Image>().color = Gold;
            bar.GetComponent<Image>().raycastTarget = false;
        }
    }

    private Button BuildModalButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, bool compact = false, bool accentFill = false)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        SetRect(buttonObject.GetComponent<RectTransform>(), position, size);
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = null;
        image.color = accentFill ? Gold : Row;
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);
        AddAngularFrame(buttonObject.transform, compact ? 2.5f : 3f, 0.16f, accentFill ? Color.black : Gold);
        CreateText(buttonObject.transform, label, Vector2.zero, size - new Vector2(20f, 12f), compact ? 32f : 34f, accentFill ? Color.black : Gold, TextAlignmentOptions.Center, true);
        return button;
    }

    private void AddAngularFrame(Transform parent, float thickness, float cut, Color? frameColor = null)
    {
        GameObject frame = new GameObject("Yellow Angular Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(AngularFrameGraphic));
        frame.transform.SetParent(parent, false);
        Stretch(frame.GetComponent<RectTransform>());
        AngularFrameGraphic graphic = frame.GetComponent<AngularFrameGraphic>();
        graphic.thickness = thickness;
        graphic.cornerCut = cut;
        graphic.color = frameColor ?? Gold;
        graphic.raycastTarget = false;
    }

    private TMP_Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment, bool outlined = false)
    {
        GameObject go = new GameObject("Text " + value, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), position, size);
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.text = value;
        StyleText(text, fontSize, color, alignment, outlined);
        text.raycastTarget = false;
        return text;
    }

    private void StyleText(TMP_Text text, float size, Color color, TextAlignmentOptions alignment, bool outlined)
    {
        if (uiFont != null) text.font = uiFont;
        text.fontSize = size;
        text.fontSizeMin = Mathf.Max(18f, size * 0.68f);
        text.fontSizeMax = size;
        text.enableAutoSizing = true;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.characterSpacing = 2f;
        text.margin = new Vector4(8f, 4f, 8f, 4f);
        if (outlined && text.fontMaterial != null)
        {
            Material material = new Material(text.fontMaterial);
            if (material.HasProperty("_OutlineWidth")) material.SetFloat("_OutlineWidth", 0.18f);
            if (material.HasProperty("_OutlineColor")) material.SetColor("_OutlineColor", Color.black);
            material.EnableKeyword("OUTLINE_ON");
            text.fontMaterial = material;
        }
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTopRightRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTopLeftRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ShowStartScreen()
    {
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(true);
        if (gameplayHud != null) gameplayHud.SetActive(false);
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(false);
        if (settingsButtonRoot != null) settingsButtonRoot.SetActive(true);
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(false);
        Time.timeScale = 0f;
    }

    private void StartGame()
    {
        started = true;
        isPaused = false;
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(false);
        if (gameplayHud != null) gameplayHud.SetActive(true);
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(true);
        if (settingsButtonRoot != null) settingsButtonRoot.SetActive(true);
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(false);
        UpdateHud();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (started && !gameOver && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))) TogglePause();
    }

    private void TogglePause()
    {
        if (isPaused) { ResumeGame(); return; }
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseRoot != null)
        {
            pauseRoot.SetActive(true);
            pauseRoot.transform.SetAsLastSibling();
        }
    }

    private void ResumeGame()
    {
        isPaused = false;
        if (pauseRoot != null) pauseRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OpenSettings()
    {
        settingsPausedGame = started && Time.timeScale > 0f;
        settingsFromStartScreen = !started && welcomeMenuRoot != null && welcomeMenuRoot.activeSelf;
        if (settingsFromStartScreen) welcomeMenuRoot.SetActive(false);
        Time.timeScale = 0f;
        UpdateSettingsLabels();
        if (settingsRoot != null)
        {
            settingsRoot.SetActive(true);
            settingsRoot.transform.SetAsLastSibling();
        }
    }

    private void CloseSettings()
    {
        PlayerPrefs.Save();
        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (settingsFromStartScreen && welcomeMenuRoot != null) welcomeMenuRoot.SetActive(true);
        if (started && !isPaused && settingsPausedGame) Time.timeScale = 1f;
    }

    private void CycleLevel()
    {
        selectedLevel = selectedLevel % 5 + 1;
        PlayerPrefs.SetInt("HunterFood.Level", selectedLevel);
        ApplySelectedSettings();
    }

    private void ToggleSound()
    {
        volume = volume > 0f ? 0f : 1f;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("HunterFood.Volume", volume);
    }

    private void ToggleAmbience()
    {
        ambienceEnabled = !ambienceEnabled;
        RenderSettings.ambientIntensity = ambienceEnabled ? normalAmbientIntensity : 0f;
        PlayerPrefs.SetInt("HunterFood.Ambience", ambienceEnabled ? 1 : 0);
    }

    private void CycleDifficulty()
    {
        selectedDifficulty = (selectedDifficulty + 1) % 3;
        PlayerPrefs.SetInt("HunterFood.Difficulty", selectedDifficulty);
        ApplySelectedSettings();
    }

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
        UpdateHud();
    }

    private void UpdateHud()
    {
        if (levelHud != null) levelHud.text = "NIVEL " + selectedLevel;
        if (foodHud != null) foodHud.text = "COMIDA " + applesFedThisLevel.ToString("00") + "/20";
    }

    public void RegisterAppleFed(Vector3 worldPosition)
    {
        applesFedThisLevel++;
        UpdateHud();
        HunterFoodVFX.FoodCollected(worldPosition, foodHud);
    }

    private void UpdateSettingsLabels()
    {
        if (settingsValues == null || settingsValues.Length < 4) return;
        settingsValues[0].text = selectedLevel.ToString();
        settingsValues[1].text = volume > 0f ? "SÍ" : "NO";
        settingsValues[2].text = ambienceEnabled ? "SÍ" : "NO";
        settingsValues[3].text = new[] { "FÁCIL", "NORMAL", "DIFÍCIL" }[selectedDifficulty];
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ObjetoPaso()
    {
        if (gameOver) return;
        objectsPassed++;
        if (objectsPassed < 3) return;

        gameOver = true;
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(false);
        if (settingsButtonRoot != null) settingsButtonRoot.SetActive(false);
        if (gameplayHud != null) gameplayHud.SetActive(false);
        if (welcomeMenuRoot != null) welcomeMenuRoot.SetActive(false);
        ConfigureGameOverScreen();
        if (gameOverRoot != null) gameOverRoot.SetActive(true);
        if (Time.timeScale != 0f) Time.timeScale = 0f;
    }

    private void ConfigureGameOverScreen()
    {
        if (imagenGameOver != null)
        {
            imagenGameOver.gameObject.SetActive(true);
            imagenGameOver.color = new Color(0.015f, 0.02f, 0.03f, 0.62f);
            Stretch(imagenGameOver.rectTransform);
        }

        if (gameOverRoot != null)
        {
            Transform card = gameOverRoot.transform.Find("Game Over Card");
            if (card != null) card.gameObject.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER";
            SetRect(gameOverText.rectTransform, new Vector2(0f, 130f), new Vector2(1000f, 130f));
            StyleText(gameOverText, 92f, Gold, TextAlignmentOptions.Center, true);
        }

        if (restartButton != null)
        {
            restartButton.SetActive(true);
            SetRect(restartButton.GetComponent<RectTransform>(), new Vector2(0f, -100f), new Vector2(470f, 92f));
            Image image = restartButton.GetComponent<Image>();
            if (image != null) { image.sprite = null; image.color = Panel; }
            AddAngularFrame(restartButton.transform, 4f, 0.16f);
            TMP_Text label = restartButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.text = "JUGAR DE NUEVO";
                StyleText(label, 40f, Gold, TextAlignmentOptions.Center, true);
            }
        }
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
