using UnityEngine;

public class MovimientoIngrediente : MonoBehaviour
{
    [Header("Movimiento (Euler)")]
    public float gravedad = -9.8f;              // Aceleración hacia abajo
    public Vector2 velocidad = Vector2.zero;    // Velocidad actual

    [Header("Interacción con tenedores")]
    [Range(0f, 1f)] public float coeficienteRestitucion = 0.6f; 
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

    void Update()
    {
        if (!activo || GameManager.Instance.juegoTerminado) return;

        // --- 1) Movimiento por gravedad ---
        velocidad.y += gravedad * Time.deltaTime;
        transform.position += (Vector3)(velocidad * Time.deltaTime);

        // --- 2) Detectar forks ---
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

        // --- 3) Detectar contacto con la canasta ---
        if (canasta != null && Vector2.Distance(transform.position, canasta.position) < distanciaCanasta)
        {
            Debug.Log($"🍞 {nombreIngrediente} llegó a la canasta.");
            GameManager.Instance.IntentarRecolectar(nombreIngrediente);
            ReiniciarPosicion();
            return;
        }

        // --- 4) Si cae bajo el límite, reaparece ---
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
        if (nuevaVel.y > -0.05f)
            nuevaVel.y = -Mathf.Max(Mathf.Abs(nuevaVel.y), 0.2f);

        if (nuevaVel.magnitude < minSpeedAfterHit)
        {
            nuevaVel = tangent.normalized * minSpeedAfterHit;
            if (nuevaVel.y > -0.05f) nuevaVel.y = -Mathf.Abs(nuevaVel.y);
        }

        velocidad = nuevaVel;
        transform.position += (Vector3)(dirNormal * (radioColision * 0.55f));
    }

    void ReiniciarPosicion()
    {
        float xRandom = Random.Range(-6f, 6f);
        transform.position = new Vector3(xRandom, alturaReinicio, 0f);
        velocidad = Vector2.zero;
        Debug.Log($"{nombreIngrediente} reaparece.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioColision);
        if (canasta != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(canasta.position, distanciaCanasta);
        }
    }
}
