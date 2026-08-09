using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class animationStateController : MonoBehaviour
{
    // ── Component References ─────────────────────────────────────────
    private Animator animator;
    public AudioSource audioSource;

    private Coroutine currentAudioCoroutine;

    [Header("Interviewer Settings")]
    public bool isActiveInterviewer = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Make sure there is an AudioSource component
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Keep talking animation in sync with audio (moved this code below)
        bool isTalking = animator.GetBool("isTalking");
        bool isPlaying = audioSource != null && audioSource.isPlaying;

        if (!isTalking && isPlaying) animator.SetBool("isTalking", true);
        if (isTalking && !isPlaying) animator.SetBool("isTalking", false);


        // ------------------------------------- CHANGES FROM HERE----------------------------------- //

        if (!isTalking && isPlaying)
            animator.SetBool("isTalking", true);

        if (isTalking && !isPlaying)
            animator.SetBool("isTalking", false);

    }
    IEnumerator LoadAndPlay(string fileName)
    {

        string path = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "audio", fileName);

        Debug.Log("Trying to load audio from: " + path);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio load failed: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                // STOP any previous audio
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                audioSource.clip = clip;
                audioSource.Play();

                // Wait until audio finishes
                yield return new WaitForSeconds(clip.length);
            }
        }
    }

    // ----------------------------------- TILL HERE----------------------------------- //


    // ── Public Coroutine called by InterviewManager ──────────────────

    public IEnumerator PlayQuestionAudio(string fileName)
    {
        // Stop previous coroutine if running
        if (currentAudioCoroutine != null)
        {
            StopCoroutine(currentAudioCoroutine);
        }

        currentAudioCoroutine = StartCoroutine(PlayAudioRoutine(fileName));
        yield return currentAudioCoroutine;
    }

    IEnumerator PlayAudioRoutine(string fileName)
    {
        string path = "file://" + System.IO.Path.Combine(
            Application.streamingAssetsPath, "audio", fileName);

        Debug.Log("Loading audio from: " + path);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Audio load failed ({fileName}): " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            // STOP previous audio
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = clip;
            audioSource.Play();

            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }
    }
}

