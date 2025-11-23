using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Música por nivel")]
    public AudioClip musicaMenu;
    public AudioClip musicaNivel1;
    public AudioClip musicaNivel2;
    public AudioClip musicaNivel3;

    [Header("Música especial")]
    public AudioClip musicaVictoria;
    public AudioClip musicaDerrota;

    [Header("Ajustes de volumen y fades")]
    public float defaultVolume = 0.5f;
    public float victoryLowerVolume = 0.1f;
    public float specialFadeTime = 0.5f;

    private AudioSource audioMusicaNormal;
    private AudioSource audioSpecial;

    private Coroutine transitionRoutine;
    private Coroutine specialRoutine;

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

        // AUDIO SOURCES
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            audioMusicaNormal = sources[0];
            audioSpecial = sources[1];
        }
        else
        {
            audioMusicaNormal = gameObject.AddComponent<AudioSource>();
            audioSpecial = gameObject.AddComponent<AudioSource>();
        }

        audioMusicaNormal.loop = true;
        audioSpecial.loop = false;
        audioSpecial.playOnAwake = false;

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
        AudioClip nuevaMusica = musicaMenu;

        switch (nombreEscena)
        {
            case "nivel1": nuevaMusica = musicaNivel1; break;
            case "nivel2": nuevaMusica = musicaNivel2; break;
            case "nivel3": nuevaMusica = musicaNivel3; break;
            default: nuevaMusica = musicaMenu; break;
        }

        if (nuevaMusica != null && audioMusicaNormal.clip != nuevaMusica)
        {
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(TransicionMusical(nuevaMusica));
        }
    }


    private IEnumerator TransicionMusical(AudioClip nuevaMusica)
    {
        float tiempoFade = 2f;
        float volumenInicial = audioMusicaNormal.volume;

        // Si no hay clip actualmente → solo fade in
        if (audioMusicaNormal.clip == null)
        {
            audioMusicaNormal.clip = nuevaMusica;
            audioMusicaNormal.volume = 0f;
            audioMusicaNormal.Play();

            for (float t = 0; t < tiempoFade; t += Time.deltaTime)
            {
                audioMusicaNormal.volume = Mathf.Lerp(0f, defaultVolume, t / tiempoFade);
                yield return null;
            }

            audioMusicaNormal.volume = defaultVolume;
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
            audioMusicaNormal.volume = Mathf.Lerp(0f, defaultVolume, t / tiempoFade);
            yield return null;
        }

        audioMusicaNormal.volume = defaultVolume;
        transitionRoutine = null;
    }

    // ----------------- MÚSICA ESPECIAL -----------------

    public void ReproducirVictoria()
    {
        if (musicaVictoria == null) return;

        if (specialRoutine != null) return;
        specialRoutine = StartCoroutine(HandleSpecialMusic(musicaVictoria));
    }

    public void ReproducirDerrota()
    {
        if (musicaDerrota == null) return;

        if (specialRoutine != null) return;
        specialRoutine = StartCoroutine(HandleSpecialMusic(musicaDerrota));
    }


    private IEnumerator HandleSpecialMusic(AudioClip clipEspecial)
    {
        if (transitionRoutine != null) yield return transitionRoutine;

        float volumenAntes = audioMusicaNormal.volume;

        // BAJAR VOLUMEN MÚSICA DE FONDO
        for (float t = 0; t < specialFadeTime; t += Time.deltaTime)
        {
            audioMusicaNormal.volume = Mathf.Lerp(volumenAntes, victoryLowerVolume, t / specialFadeTime);
            yield return null;
        }
        audioMusicaNormal.volume = victoryLowerVolume;

        // REPRODUCIR AUDIO ESPECIAL
        audioSpecial.Stop();
        audioSpecial.clip = clipEspecial;
        audioSpecial.volume = 1f;
        audioSpecial.Play();

        // 🔥 CORREGIDO: AudioClip NO tiene .clip.length
        yield return new WaitForSeconds(clipEspecial.length);

        // SUBIR DE NUEVO VOLUMEN DE FONDO
        for (float t = 0; t < specialFadeTime; t += Time.deltaTime)
        {
            audioMusicaNormal.volume = Mathf.Lerp(victoryLowerVolume, volumenAntes, t / specialFadeTime);
            yield return null;
        }

        audioMusicaNormal.volume = volumenAntes;

        specialRoutine = null;
    }
}
