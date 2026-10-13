using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public TMP_Text gameOverText;
    private int objetosQuePasaron;
    private bool juegoTerminado;

    void Start()
    {
        gameOverText.gameObject.SetActive(false);
    }

    public void ObjetoPaso()
    {
        if (juegoTerminado) return;

        objetosQuePasaron++;

        if (objetosQuePasaron >= 3)
        {
            juegoTerminado = true;
            gameOverText.gameObject.SetActive(true);
        }
    }
}