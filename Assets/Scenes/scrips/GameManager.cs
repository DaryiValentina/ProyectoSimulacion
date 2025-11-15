using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración del juego")]
    public List<string> ordenCorrecto = new List<string> { "pan", "queso", "lechuga" };
    public int vidas = 3;

    [HideInInspector] public bool juegoTerminado = false;
    private int indiceActual = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IntentarRecolectar(string nombreComida)
    {
        if (juegoTerminado) return;

        if (ordenCorrecto[indiceActual] == nombreComida)
        {
            Debug.Log($"✅ Recolectado correctamente: {nombreComida}");
            indiceActual++;

            if (indiceActual >= ordenCorrecto.Count)
            {
                Debug.Log("🎉 Nivel completado correctamente!");
                juegoTerminado = true;
            }
        }
        else
        {
            vidas--;
            Debug.Log($"❌ Orden incorrecto. Pierdes una vida. Vidas restantes: {vidas}");

            if (vidas <= 0)
            {
                juegoTerminado = true;
                Debug.Log("💀 Has perdido el nivel.");
            }

            indiceActual = 0; // reinicia el orden
        }
    }
}
