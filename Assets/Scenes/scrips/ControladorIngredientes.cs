using UnityEngine;
using UnityEngine.UI;

public class ControladorIngredientes : MonoBehaviour
{
    public GameObject prefabIngrediente;
    public int vidas = 3;
    public Slider barraProgreso;
    private int ingredientesRecogidos = 0;
    public int totalIngredientes = 5;

    void Start()
    {
        // Empieza generando ingredientes
        InvokeRepeating("CrearIngrediente", 1f, 2f);
    }

    void CrearIngrediente()
    {
        float posX = Random.Range(-7f, 7f);
        Vector3 spawnPos = new Vector3(posX, 6f, 0);
        Instantiate(prefabIngrediente, spawnPos, Quaternion.identity);
    }

    public void IngredienteRecogido(bool correcto)
    {
        if (correcto)
        {
            ingredientesRecogidos++;
            barraProgreso.value = (float)ingredientesRecogidos / totalIngredientes;

            if (ingredientesRecogidos >= totalIngredientes)
            {
                Debug.Log("¡Ganaste!");
                CancelInvoke();
            }
        }
        else
        {
            vidas--;
            Debug.Log("Perdiste una vida. Vidas restantes: " + vidas);
            if (vidas <= 0)
            {
                Debug.Log("¡Perdiste el juego!");
                CancelInvoke();
            }
        }
    }
}
