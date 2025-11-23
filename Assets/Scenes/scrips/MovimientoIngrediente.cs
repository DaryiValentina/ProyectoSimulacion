using UnityEngine;

public class MovimientoIngrediente : MonoBehaviour
{
    [Header("Movimiento (Euler)")]
    public float gravedad = -9.8f;           // Aceleración hacia abajo
    public Vector2 velocidad = Vector2.zero; // Velocidad actual

    [Header("Movimiento Natural")]
    public float dragAire = 0.2f;
    public float amplitudOscilacion = 0.5f;
    public float frecuenciaOscilacion = 2f;
    public float velocidadRotacion = 45f;

    private float tiempo;

    [Header("Interacción con tenedores")]
    [Range(0f, 1f)] public float coeficienteRestitucion = 0.4f;
    [Range(0f, 1f)] public float friccionTangencial = 0.15f;
    public float radioColision = 0.35f;
    public LayerMask capaCubiertos;

    [Header("Reaparición / límites")]
    public Transform limiteInferior;
    public float alturaReinicio = 5f;
    public float minSpeedAfterHit = 0.2f;

    [Header("Canasta")]
    public Transform canasta;                   // referencia a la canasta
    public float distanciaCanasta = 0.45f;      // radio de detección de la canasta
    public string nombreIngrediente;            // nombre que debe coincidir con el del GameManager

    private bool activo = true;
    private float xInicial;

    // --------------------------
    // NUEVO: inercia de rotación
    // --------------------------
    private float rotacionAngular = 0f;


    void Start()
    {
        xInicial = transform.position.x;
    }

    void Update()
    {
        if (!activo || GameManager.Instance.juegoTerminado) return;

        tiempo += Time.deltaTime;

        // --- 1) Gravedad realista ---
        velocidad.y += gravedad * Time.deltaTime;

        // --- 2) Movimiento natural ---
        //float empuje = Mathf.Sin(tiempo * frecuenciaOscilacion) * amplitudOscilacion * 0.15f;
        //velocidad.x += empuje;
        velocidad *= (1f - dragAire * Time.deltaTime);
        float mult = GameManager.Instance.GetMultiplicadorVelocidad();
        transform.position += (Vector3)(velocidad * mult * Time.deltaTime);

        ////transform.position += (Vector3)(velocidad * Time.deltaTime);

        // --- Rotación con inercia ---
        rotacionAngular *= 0.97f; // fricción angular
        transform.Rotate(0, 0, rotacionAngular * Time.deltaTime);

        // --- 3) Colisiones ---
        Collider2D[] cubiertos = Physics2D.OverlapCircleAll(transform.position, radioColision, capaCubiertos);
        if (cubiertos.Length > 0)
        {
            Collider2D closer = cubiertos[0];
            float bestDist = Vector2.Distance(transform.position, closer.transform.position);

            foreach (var c in cubiertos)
            {
                float d = Vector2.Distance(transform.position, c.transform.position);
                if (d < bestDist) { bestDist = d; closer = c; }
            }

            ProcesarContactoConFork(closer);
        }

        // --- 4) Canasta ---
        if (canasta != null && Vector2.Distance(transform.position, canasta.position) < distanciaCanasta)
        {
            GameManager.Instance.IntentarRecolectar(nombreIngrediente);
            ReiniciarPosicion();
            return;
        }

        // --- 5) Límite inferior ---
        if (limiteInferior != null && transform.position.y < limiteInferior.position.y)
        {
            ReiniciarPosicion();
        }
    }


    void ProcesarContactoConFork(Collider2D fork)
    {
        Vector2 puntoMasCercano = fork.ClosestPoint(transform.position);
        Vector2 dirNormal = (Vector2)transform.position - puntoMasCercano;

        if (dirNormal.sqrMagnitude < 1e-6f)
        {
            float angleRad = fork.transform.eulerAngles.z * Mathf.Deg2Rad;
            dirNormal = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        dirNormal.Normalize();
        Vector2 tangent = new Vector2(-dirNormal.y, dirNormal.x);
        if (Vector2.Dot(tangent, Vector2.down) < 0f) tangent = -tangent;

        float vNormalScalar = Vector2.Dot(velocidad, dirNormal);
        Vector2 vNormal = -vNormalScalar * coeficienteRestitucion * dirNormal;
        Vector2 vTangent = velocidad - vNormal;
        vTangent *= (1f - friccionTangencial);

        Vector2 nuevaVel = vTangent + vNormal;

        if (nuevaVel.magnitude < minSpeedAfterHit)
            nuevaVel = tangent.normalized * minSpeedAfterHit;

        velocidad = nuevaVel;

        transform.position += (Vector3)(dirNormal * 0.05f);

        // Reinicio del oscilador
        xInicial = transform.position.x;
        tiempo = 0;

        // ----------------------------------------------
        // NUEVO: ROTACIÓN FÍSICA BASADA EN EL IMPACTO
        // ----------------------------------------------

        float fuerzaImpacto = nuevaVel.magnitude;
        float direccion = Mathf.Sign(Vector2.Dot(tangent, Vector2.right));

        // Aplicar torque
        float torque = fuerzaImpacto * direccion * 20f;
        rotacionAngular += torque;
        rotacionAngular = Mathf.Clamp(rotacionAngular, -100f, 100f);
    }


    void ReiniciarPosicion()
    {
        float xRandom = Random.Range(-6f, 6f);
        transform.position = new Vector3(xRandom, alturaReinicio, 0f);
        velocidad = Vector2.zero;

        rotacionAngular = 0f; // reset de spin

        xInicial = xRandom;
        tiempo = 0;
    }
}

