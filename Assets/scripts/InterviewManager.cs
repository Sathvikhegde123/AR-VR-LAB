using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]

public class Option
{
    public string text;
    public int score;
}

[System.Serializable]
public class Question
{
    public string question;
    public List<Option> options;
}

[System.Serializable]
public class QuestionList
{
    public List<Question> questions;
}

public class InterviewManager : MonoBehaviour
{
    private bool isSpeaking = false;

    [Header("Interviewers")]
    public animationStateController[] interviewers;

    [Header("MCQ UI")]
    public GameObject mcqPanel;
    public TMP_Text questionText;
    public Button[] optionButtons;
    public Button startMCQButton;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text scoreText;
    public TMP_Text messageText;
    public Button restartButton;

    [Header("Speaking UI")]
    public Button startSpeakingButton;
    public Button startAnswerBtn;
    public Button stopAnswerBtn;

    [Header("Session Report UI")]
    public GameObject sessionPanel;
    public TMP_Text usernameText;
    public TMP_Text totalTimeText;
    public TMP_Text answersTimeText;

    private List<Question> mcqQuestions = new List<Question>();
    private int currentMCQ = 0;
    private int totalMCQScore = 0;

    private List<Question> speakingQuestions = new List<Question>();
    private int currentSpeaking = 0;
    private int answerStartTime;
    private List<int> speakingTimes = new List<int>();
    private int totalSpeakingDuration = 0;

    private Coroutine speakCoroutine;
    private Coroutine speakingFlowCoroutine;

    string baseURL = "http://127.0.0.1:5000";
    private bool isFlowRunning = false;
    void Start()
    {
        mcqPanel.SetActive(false);

        if (GameData.mode == "MCQ")
        {
            startMCQButton.gameObject.SetActive(true);
            startSpeakingButton.gameObject.SetActive(false);
        }
        else if (GameData.mode == "INTERVIEW")
        {
            startMCQButton.gameObject.SetActive(false);
            startSpeakingButton.gameObject.SetActive(true);
        }
        Debug.Log("Mode: " + GameData.mode);
        startAnswerBtn.gameObject.SetActive(false);
        stopAnswerBtn.gameObject.SetActive(false);
        resultPanel.SetActive(false);
        sessionPanel.SetActive(false);

        startAnswerBtn.interactable = false;
        stopAnswerBtn.interactable = false;

        startAnswerBtn.onClick.AddListener(StartAnswer);
        stopAnswerBtn.onClick.AddListener(StopAnswer);
    }

    animationStateController GetRandomInterviewer()
    {
        int index = Random.Range(0, interviewers.Length);
        return interviewers[index];
    }

    // ================= MCQ =================
    public void StartMCQ()
    {
        startMCQButton.gameObject.SetActive(false);
        startSpeakingButton.gameObject.SetActive(false);
        StartCoroutine(GetMCQ());

        startAnswerBtn.gameObject.SetActive(false);
        stopAnswerBtn.gameObject.SetActive(false);
    }

    IEnumerator GetMCQ()
    {
        UnityWebRequest req = UnityWebRequest.Get(baseURL + "/get_mcq/python");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("MCQ API Error: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        string wrapped = "{\"questions\":" + json + "}";
        mcqQuestions = JsonUtility.FromJson<QuestionList>(wrapped).questions;

        mcqPanel.SetActive(true);
        currentMCQ = 0;
        totalMCQScore = 0;

        ShowMCQQuestion();
    }

    void ShowMCQQuestion()
    {
        Question q = mcqQuestions[currentMCQ];
        questionText.text = q.question;

        if (speakCoroutine != null)
        {
            StopCoroutine(speakCoroutine);
        }

        SetOptionButtonsInteractable(false);
        speakCoroutine = StartCoroutine(SpeakAndEnableOptions(q.question));

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = q.options[i].text;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => SelectMCQOption(q.options[index].score));
        }
    }

    void SelectMCQOption(int score)
    {
        // Ignore clicks while TTS is running
        if (isSpeaking)
        {
            Debug.Log("Click ignored - TTS still playing");
            return;
        }

        totalMCQScore += score;
        currentMCQ++;

        if (currentMCQ < mcqQuestions.Count)
        {
            ShowMCQQuestion();
        }
        else
        {
            mcqPanel.SetActive(false);


            if (speakCoroutine != null)
            {
                StopCoroutine(speakCoroutine);
            }

            foreach (var i in interviewers)
            {
                var src = i.GetComponent<AudioSource>();
                if (src != null && src.isPlaying)
                    src.Stop();
            }


            // Show result panel
            resultPanel.SetActive(true);

            // Set score text
            scoreText.text = "Score: " + totalMCQScore + "/40";

            // Set message
            if (totalMCQScore == 40)
                messageText.text = "Excellent!";
            else if (totalMCQScore >= 30)
                messageText.text = "Good!";
            else if (totalMCQScore >= 20)
                messageText.text = "Can do better!";
            else
                messageText.text = "Need to work harder!";
        }
    }
    public void RestartFromMCQ()
    {
        totalMCQScore = 0;
        currentMCQ = 0;

        // Reset GameData if needed
        GameData.selectedDomains.Clear();
        GameData.difficulty = "";
        GameData.openModePanel = true;

        // Load Login Scene
        SceneManager.LoadScene("LoginUI");

    }
    public void RestartFromInterview()
    {
        totalMCQScore = 0;
        currentMCQ = 0;

        // Reset GameData if needed
        GameData.selectedDomains.Clear();
        GameData.difficulty = "";
        GameData.openModePanel = true;

        // Load Login Scene
        SceneManager.LoadScene("LoginUI");

    }

    // ================= SPEAKING =================
    public void StartSpeaking()
    {
        Debug.Log("START SPEAKING BUTTON CLICKED");
        startAnswerBtn.gameObject.SetActive(true);
        stopAnswerBtn.gameObject.SetActive(true);

        startAnswerBtn.interactable = false;
        stopAnswerBtn.interactable = false;

        startMCQButton.gameObject.SetActive(false);
        startSpeakingButton.gameObject.SetActive(false);

        StartCoroutine(GetSpeakingQuestions());
    }

    IEnumerator GetSpeakingQuestions()
    {
        speakingTimes.Clear();
        totalSpeakingDuration = 0; // reset total

        // Build URL with domain and difficulty from GameData
        string url = baseURL + "/get_speaking";

        // Use the first selected domain (interview uses one domain at a time)
        if (GameData.selectedDomains != null && GameData.selectedDomains.Count > 0)
        {
            // Map difficulty string to DB integer
            int difficultyInt = 1;
            if (GameData.difficulty == "Medium") difficultyInt = 2;
            else if (GameData.difficulty == "Hard") difficultyInt = 3;

            // Build domain_ids as comma-separated: e.g. "1,3,5"
            string domainIds = string.Join(",", GameData.selectedDomains);

            url += "?domain_ids=" + domainIds + "&difficulty=" + difficultyInt;
        }

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Speaking API Error: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        Debug.Log("RAW API RESPONSE: " + json);
        string wrapped = "{\"questions\":" + json + "}";

        speakingQuestions = JsonUtility.FromJson<QuestionList>(wrapped).questions;

        // limit to 10 AFTER loading
        speakingQuestions = speakingQuestions.GetRange(0, Mathf.Min(10, speakingQuestions.Count));

        currentSpeaking = 0;
        speakingTimes.Clear();

        PrepareSpeakingQuestion();
    }

    void PrepareSpeakingQuestion()
    {
        if (isFlowRunning)
        {
            Debug.LogWarning("FLOW BLOCKED - already running");
            return;
        }

        isFlowRunning = true;
       
        if (currentSpeaking >= speakingQuestions.Count)
        {
            Debug.Log("Speaking session complete.");

            StartCoroutine(FinishAfterAudio());

            // Hide buttons
            startMCQButton.gameObject.SetActive(false);
            startSpeakingButton.gameObject.SetActive(false);

            startAnswerBtn.gameObject.SetActive(false);
            stopAnswerBtn.gameObject.SetActive(false);
            startAnswerBtn.interactable = false;
            stopAnswerBtn.interactable = false;

            return;
        }

        Question q = speakingQuestions[currentSpeaking];

        // ONLY CONTROL INTERACTABLE
        startAnswerBtn.interactable = false;
        stopAnswerBtn.interactable = false;

        speakingFlowCoroutine = StartCoroutine(HandleSpeakingFlow(q));

        Debug.Log("PLAYING QUESTION: " + currentSpeaking);
    }

    IEnumerator EndInterview()
    {
        yield return StartCoroutine(SaveSession());

        sessionPanel.SetActive(true);

        usernameText.text = "User: " + GameData.username;

        int totalDuration = totalSpeakingDuration;
        int minutes = Mathf.FloorToInt(totalDuration / 60);
        int seconds = Mathf.FloorToInt(totalDuration % 60);
        totalTimeText.text = "Total Time: " + minutes + "m " + seconds + "s";

        string report = "";
        for (int i = 0; i < speakingTimes.Count; i++)
        {
            int t = speakingTimes[i];
            int qMin = Mathf.FloorToInt(t / 60);
            int qSec = Mathf.FloorToInt(t % 60);
            report += "Q" + (i + 1) + " :  " + qMin + "m " + qSec + "s\n";
        }

        answersTimeText.text = report;
    }
    IEnumerator FinishAfterAudio()
    {
        bool anyPlaying;

        do
        {
            anyPlaying = false;

            foreach (var i in interviewers)
            {
                var src = i.GetComponent<AudioSource>();
                if (src != null && src.isPlaying)
                {
                    anyPlaying = true;
                    break;
                }
            }

            yield return null;

        } while (anyPlaying);

        yield return StartCoroutine(EndInterview());
    }
    IEnumerator HandleSpeakingFlow(Question q)
    {
        yield return StartCoroutine(Speak(q.question));

        startAnswerBtn.interactable = true;
        stopAnswerBtn.interactable = false;

        yield return null; //  IMPORTANT (1 frame buffer)

        isFlowRunning = false;
    }

    IEnumerator Speak(string text)
    {
        Debug.Log("START SPEAK");
        while (isSpeaking)
        {
            yield return null; // wait instead of skipping
        }

        isSpeaking = true;

        animationStateController selected = GetRandomInterviewer();

        foreach (var i in interviewers)
            i.isActiveInterviewer = false;

        selected.isActiveInterviewer = true;

        WWWForm form = new WWWForm();
        form.AddField("text", text);

        int index = System.Array.IndexOf(interviewers, selected);
        form.AddField("avatar_id", index);

        Debug.Log("Sending TTS text: " + text);

        UnityWebRequest req = UnityWebRequest.Post(baseURL + "/speak", form);
        yield return req.SendWebRequest();

        bool played = false;

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("TTS Error: " + req.error);
        }
        else
        {
            string json = req.downloadHandler.text;
            var response = JsonUtility.FromJson<TTSResponse>(json);

            Debug.Log("Audio filename received: " + response.audio);

            if (!string.IsNullOrEmpty(response.audio))
            {
                yield return StartCoroutine(selected.PlayQuestionAudio(response.audio));
                played = true;
            }
        }

        // fallback delay if audio didn't play
        if (!played)
        {
            Debug.LogWarning("Fallback wait (audio failed)");
            yield return new WaitForSeconds(2f);
        }

        isSpeaking = false;
        Debug.Log("END SPEAK");
    }

    IEnumerator SpeakAndEnableOptions(string text)
    {
        yield return StartCoroutine(Speak(text));

        // Enable buttons AFTER speaking finishes
        SetOptionButtonsInteractable(true);
    }

    void SetOptionButtonsInteractable(bool state)
    {
        foreach (var btn in optionButtons)
        {
            btn.interactable = state;
        }
    }

    void StartAnswer()
    {
        answerStartTime = (int)Time.time;

        startAnswerBtn.interactable = false;
        stopAnswerBtn.interactable = true;
    }

    void StopAnswer()
    {
        if (isFlowRunning)
        {
            Debug.LogWarning("Blocked: Flow still running");
            return;
        }
        if (isSpeaking)
        {
            Debug.LogWarning("Blocked: Still speaking");
            return;
        }
        int duration = (int)Time.time - answerStartTime;

        speakingTimes.Add(duration);        
        totalSpeakingDuration += duration;

        currentSpeaking++;

        startAnswerBtn.interactable = false;
        stopAnswerBtn.interactable = false;

        PrepareSpeakingQuestion();
    }

    IEnumerator SaveSession()
    {
        SessionData session = new SessionData
        {
            username = GameData.username,
            duration = Mathf.RoundToInt(totalSpeakingDuration),
            times = speakingTimes.ToArray()
        };

        string jsonData = JsonUtility.ToJson(session);
        Debug.Log("Sending JSON: " + jsonData); // IMPORTANT

        UnityWebRequest req = new UnityWebRequest(baseURL + "/save_session", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        Debug.Log("Server Response: " + req.downloadHandler.text);

        if (req.result == UnityWebRequest.Result.Success)
        { 
            Debug.Log("Total speaking time: " + totalSpeakingDuration);
        }
        else
        {
            Debug.LogError("Session save failed: " + req.error);
        }
    }

    int GetTotalSpeakingDuration()
    {
        int total = 0;
        foreach (int t in speakingTimes)
            total += t;

        return total;
    }

    [System.Serializable]
    public class SessionData
    {
        public string username;
        public int duration;
        public int[] times;
    }
}

[System.Serializable]
public class TTSResponse
{
    public string status;
    public string audio;
}