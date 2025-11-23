using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración del juego")]
    public List<string> ordenCorrecto; // ← aquí pones tus 7 ingredientes en orden
    public int vidas = 3;

    [Header("UI de vidas")]
    public List<GameObject> corazones; // ← tus 3 corazones

    [Header("Luces del orden correcto")]
    public List<GameObject> lucesOrden; // ← tus 8 luces (7 ingredientes + luz final)

    [Header("Escenas de Victoria y Derrota")]
    public string escenaVictoria;
    public string escenaDerrota;

    [HideInInspector] public bool juegoTerminado = false;

    [Header("Nivel actual (1, 2 o 3)")]
    public int nivelActual = 1;

    [Header("Multiplicadores de velocidad por nivel")]
    public float velocidadNivel1 = 1f;
    public float velocidadNivel2 = 2f;
    public float velocidadNivel3 = 2.8f;

    private int indiceActual = 0;

    public void CargarEscenaVictoria(float delay = 1.5f)
    {
        StartCoroutine(LoadWin(delay));
    }

    public void CargarEscenaDerrota(float delay = 1.5f)
    {
        StartCoroutine(LoadLose(delay));
    }

    private IEnumerator LoadWin(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(escenaVictoria))
            SceneManager.LoadScene(escenaVictoria);
        else
            Debug.LogError("No asignaste escena de victoria en el GameManager.");
    }

    private IEnumerator LoadLose(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(escenaDerrota))
            SceneManager.LoadScene(escenaDerrota);
        else
            Debug.LogError("No asignaste escena de derrota en el GameManager.");
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Apagar todas las luces al iniciar
        ApagarTodasLasLuces();
    }

    public void IntentarRecolectar(string nombreComida)
    {
        if (juegoTerminado) return;

        if (ordenCorrecto[indiceActual] == nombreComida)
        {
            Debug.Log($"✅ Recolectado correctamente: {nombreComida}");
            indiceActual++;

            EncenderLuz(); // ← prender la luz correspondiente

            if (indiceActual >= ordenCorrecto.Count)
            {
                MusicManager.Instance.ReproducirVictoria();
                Debug.Log("🎉 ¡Comida completada!");
                EncenderLuzFinal(); // ← prender la luz final
                juegoTerminado = true;
                CargarEscenaVictoria(0.5f);
            }
        }
        else
        {
            vidas--;
            ActualizarVidasUI();

            Debug.Log($"❌ Orden incorrecto. Pierdes una vida. Vidas restantes: {vidas}");

            if (vidas <= 0)
            {
                MusicManager.Instance.ReproducirDerrota();
                juegoTerminado = true;
                Debug.Log("💀 Has perdido el nivel.");
                CargarEscenaDerrota(2f);
            }

            indiceActual = 0;        // reiniciar orden
            ApagarTodasLasLuces();   // apagar luces
        }
    }

    // ----------- MANEJO DE VIDAS -------------
    void ActualizarVidasUI()
    {
        if (vidas < 0) vidas = 0;

        for (int i = 0; i < corazones.Count; i++)
        {
            corazones[i].SetActive(i < vidas);
        }
    }

    // ----------- MANEJO DE LUCES -------------
    void EncenderLuz()
    {
        int index = indiceActual - 1;

        if (index >= 0 && index < lucesOrden.Count - 1) // las primeras 7 luces
        {
            lucesOrden[index].SetActive(true);
        }
    }

    void EncenderLuzFinal()
    {
        int finalIndex = lucesOrden.Count - 1; // última luz (luz final)
        lucesOrden[finalIndex].SetActive(true);
    }

    void ApagarTodasLasLuces()
    {
        foreach (var luz in lucesOrden)
        {
            luz.SetActive(false);
        }
    }

    //-----------DIFICULTAD-----------------
    public float GetMultiplicadorVelocidad()
    {
        switch (nivelActual)
        {
            case 2: return velocidadNivel2;
            case 3: return velocidadNivel3;
            default: return velocidadNivel1;
        }
    }

}