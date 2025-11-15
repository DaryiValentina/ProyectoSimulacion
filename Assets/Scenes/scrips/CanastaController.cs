using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CanastaController : MonoBehaviour
{
    public float velocidad = 5f;
    private float limiteX = 7f;

    void Start()
    {
        #if UNITY_EDITOR
        // En el editor, forza el foco en la ventana del juego
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        #endif
    }

    void Update()
    {
        float movimiento = Input.GetAxis("Horizontal") * velocidad * Time.deltaTime;
        transform.Translate(movimiento, 0, 0);

        float x = Mathf.Clamp(transform.position.x, -limiteX, limiteX);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
