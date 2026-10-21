using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManagerScriptFarm : MonoBehaviour
{
    public TMP_Text gameOverText;
    public GameObject restartButton;
    public Image imagenGameOver;

    private int objetosQuePasaron;
    private bool juegoTerminado;

    void Start()
    {
        Time.timeScale = 1f;
        gameOverText.gameObject.SetActive(false);
        restartButton.SetActive(false);
        imagenGameOver.gameObject.SetActive(false);
    }

    public void ObjetoPaso()
    {
        if (juegoTerminado) return;

        objetosQuePasaron++;

        if (objetosQuePasaron >= 3)
        {
            juegoTerminado = true;
            gameOverText.gameObject.SetActive(true);
            restartButton.SetActive(true);
            Time.timeScale = 0f;
            if (imagenGameOver != null)
            {
                imagenGameOver.gameObject.SetActive(true);
            }

        }
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}