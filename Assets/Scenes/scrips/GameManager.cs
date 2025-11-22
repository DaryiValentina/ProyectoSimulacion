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

    [HideInInspector] public bool juegoTerminado = false;

    private int indiceActual = 0;

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
                Debug.Log("🎉 ¡Comida completada!");
                EncenderLuzFinal(); // ← prender la luz final
                MusicManager.Instance.ReproducirVictoria();
                juegoTerminado = true;
            }
        }
        else
        {
            vidas--;
            ActualizarVidasUI();

            Debug.Log($"❌ Orden incorrecto. Pierdes una vida. Vidas restantes: {vidas}");

            if (vidas <= 0)
            {
                juegoTerminado = true;
                Debug.Log("💀 Has perdido el nivel.");
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
}