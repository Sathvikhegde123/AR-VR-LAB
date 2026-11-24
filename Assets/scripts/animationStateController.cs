
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class animationStateController : MonoBehaviour
{
    // Component references
    private Animator animator;

    public AudioSource audioSource;

    private Coroutine currentAudioCoroutine;

    [Header("Interviewer Settings")]
    public bool isActiveInterviewer = false;


    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    void Awake()
    {
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        // Add AudioSource if one does not already exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }


    // --------------------------------------------------
    // UPDATE TALKING ANIMATION
    // --------------------------------------------------

    void Update()
    {
        if (animator == null || audioSource == null)
        {
            return;
        }

        bool isTalking = animator.GetBool("isTalking");
        bool isPlaying = audioSource.isPlaying;

        if (isPlaying && !isTalking)
        {
            animator.SetBool("isTalking", true);
        }
        else if (!isPlaying && isTalking)
        {
            animator.SetBool("isTalking", false);
        }
    }


    // --------------------------------------------------
    // PLAY QUESTION AUDIO
    // Called by InterviewManager
    // --------------------------------------------------

    public IEnumerator PlayQuestionAudio(string fileName)
    {
        // Stop previous audio coroutine if it is running
        if (currentAudioCoroutine != null)
        {
            StopCoroutine(currentAudioCoroutine);
            currentAudioCoroutine = null;
        }

        // Stop any currently playing audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Start loading and playing the new audio
        currentAudioCoroutine = StartCoroutine(PlayAudioRoutine(fileName));

        yield return currentAudioCoroutine;

        currentAudioCoroutine = null;
    }


    // --------------------------------------------------
    // LOAD AUDIO FILE AND PLAY IT
    // --------------------------------------------------

    IEnumerator PlayAudioRoutine(string fileName)
    {
        // Construct the full audio file path
        string path = Path.Combine(
            Application.streamingAssetsPath,
            "audio",
            fileName
        );

        // Debug information
        Debug.Log("StreamingAssets folder: " +
                  Application.streamingAssetsPath);

        Debug.Log("Full audio file path: " + path);

        Debug.Log("File exists: " + File.Exists(path));

        // Convert Windows file path into a proper file URL
        string url = new System.Uri(path).AbsoluteUri;

        Debug.Log("Audio URL: " + url);


        // Load the WAV file
        using (UnityWebRequest www =
            UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            // Check whether the audio loaded successfully
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Audio load failed ({fileName}): {www.error}"
                );

                yield break;
            }

            // Get the loaded audio clip
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            if (clip == null)
            {
                Debug.LogError("Audio clip is null: " + fileName);
                yield break;
            }

            // Stop previous audio
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Assign and play the audio
            audioSource.clip = clip;
            audioSource.Play();

            Debug.Log("Playing audio: " + fileName);

            // Wait until audio finishes playing
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            Debug.Log("Audio finished: " + fileName);
        }
    }
}
