using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string nombreEscena;

    public void IrAEscena()
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("No has asignado un nombre de escena en el inspector.");
        }
    }
}
