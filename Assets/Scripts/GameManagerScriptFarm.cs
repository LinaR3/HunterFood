using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManagerScriptFarm : MonoBehaviour
{
    public TMP_Text gameOverText;
    public GameObject restartButton;
    private int objetosQuePasaron;
    private bool juegoTerminado;

    void Start()
    {
        gameOverText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ObjetoPaso()
    {
        if (juegoTerminado) return;

        objetosQuePasaron++;

        if (objetosQuePasaron >= 3)
        {
            juegoTerminado = true;
            gameOverText.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}