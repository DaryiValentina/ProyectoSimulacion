using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Música por nivel")]
    public AudioClip musicaMenu;    // Música por defecto
    public AudioClip musicaNivel1;
    public AudioClip musicaNivel2;
    public AudioClip musicaNivel3;

    [Header("Música especial")]
    public AudioClip musicaVictoria;

    [Header("Ajustes de volumen y fades")]
    [Tooltip("Volumen objetivo para las pistas normales (0..1)")]
    public float defaultVolume = 0.5f;

    [Tooltip("Volumen al que se baja la música de fondo durante la victoria")]
    public float victoryLowerVolume = 0.1f;

    [Tooltip("Tiempo de fade (segundos) para atenuar/restaurar")]
    public float victoryFadeTime = 0.5f;

    private AudioSource audioMusicaNormal; // Música base (usa TransicionMusical)
    private AudioSource audioVictory;      // Música de victoria (reproduce encima)
    private Coroutine transitionRoutine;
    private Coroutine victoryRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Obtener (o crear) dos AudioSources: el primero para la música normal, el segundo para victoria
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            audioMusicaNormal = sources[0];
            audioVictory = sources[1];
        }
        else
        {
            // Si solo hay uno o ninguno, crear los que faltan
            audioMusicaNormal = gameObject.AddComponent<AudioSource>();
            audioVictory = gameObject.AddComponent<AudioSource>();
        }

        audioMusicaNormal.loop = true;
        audioVictory.loop = false;
        audioVictory.playOnAwake = false;

        // Inicial volumen
        audioMusicaNormal.volume = defaultVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CambiarMusicaSegunEscena(scene.name);
    }

    public void CambiarMusicaSegunEscena(string nombreEscena)
    {
        AudioClip nuevaMusica = musicaMenu;  // música predeterminada

        switch (nombreEscena)
        {
            case "nivel1":
                nuevaMusica = musicaNivel1;
                break;

            case "nivel2":
                nuevaMusica = musicaNivel2;
                break;

            case "nivel3":
                nuevaMusica = musicaNivel3;
                break;

            default:
                nuevaMusica = musicaMenu;
                break;
        }

        // Si la nueva pista es distinta, lanza la transición (fade out / change clip / fade in)
        if (nuevaMusica != null && audioMusicaNormal.clip != nuevaMusica)
        {
            // Si hay una transición en curso, deténla (para evitar mezclas raras)
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(TransicionMusical(nuevaMusica));
        }
    }

    // Transición entre pistas normales (tu implementación original, adaptada para usar defaultVolume)
    private IEnumerator TransicionMusical(AudioClip nuevaMusica)
    {
        float tiempoFade = 2f;              // << AJUSTA LA VELOCIDAD AQUÍ si quieres
        float volumenObjetivo = defaultVolume;
        float volumenInicial = audioMusicaNormal.volume;

        // Si no había clip antes, solo fade in
        if (audioMusicaNormal.clip == null)
        {
            audioMusicaNormal.clip = nuevaMusica;
            audioMusicaNormal.volume = 0f;
            audioMusicaNormal.Play();

            for (float t = 0; t < tiempoFade; t += Time.deltaTime)
            {
                audioMusicaNormal.volume = Mathf.Lerp(0f, volumenObjetivo, t / tiempoFade);
                yield return null;
            }

            audioMusicaNormal.volume = volumenObjetivo;
            transitionRoutine = null;
            yield break;
        }

        // FADE OUT
        for (float t = 0; t < tiempoFade; t += Time.deltaTime)
        {
            audioMusicaNormal.volume = Mathf.Lerp(volumenInicial, 0f, t / tiempoFade);
            yield return null;
        }

        audioMusicaNormal.volume = 0f;
        audioMusicaNormal.clip = nuevaMusica;
        audioMusicaNormal.Play();

        // FADE IN
        for (float t = 0; t < tiempoFade; t += Time.deltaTime)
        {
            audioMusicaNormal.volume = Mathf.Lerp(0f, volumenObjetivo, t / tiempoFade);
            yield return null;
        }

        audioMusicaNormal.volume = volumenObjetivo;
        transitionRoutine = null;
    }

    // ----------------- VICTORY BEHAVIOR -----------------

    // Llamar esto cuando ganas
    public void ReproducirVictoria()
    {
        if (musicaVictoria == null)
        {
            Debug.LogWarning("MusicManager: musicaVictoria no asignada.");
            return;
        }

        // Si ya se está reproduciendo una victoria, no lanzamos otra
        if (victoryRoutine != null) return;

        victoryRoutine = StartCoroutine(HandleVictory());
    }

    private IEnumerator HandleVictory()
    {
        // Si hay una transición de pista normal en curso, esperamos a que termine para no pelear con el volumen
        if (transitionRoutine != null) yield return transitionRoutine;

        float volumenAntes = audioMusicaNormal.volume;
        float tiempo = victoryFadeTime;

        // Fade a volumen bajo (victoryLowerVolume), sin detener la música de fondo
        for (float t = 0; t < tiempo; t += Time.deltaTime)
        {
            audioMusicaNormal.volume = Mathf.Lerp(volumenAntes, victoryLowerVolume, t / tiempo);
            yield return null;
        }
        audioMusicaNormal.volume = victoryLowerVolume;

        // Reproducir la pista de victoria en el audioVictory
        audioVictory.clip = musicaVictoria;
        audioVictory.volume = 1f;
        audioVictory.Play();

        // Esperamos a que termine la música de victoria (o su duración real)
        yield return new WaitForSeconds(audioVictory.clip.length);

        // Restaurar volumen de la música normal
        for (float t = 0; t < tiempo; t += Time.deltaTime)
        {
            audioMusicaNormal.volume = Mathf.Lerp(victoryLowerVolume, volumenAntes, t / tiempo);
            yield return null;
        }
        audioMusicaNormal.volume = volumenAntes;

        victoryRoutine = null;
    }
}
