using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AuthUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject modeselectPanel;
    public GameObject dropDownMenuPanel;
    public GameObject difficultyPanel;
    public GameObject passwordHintBG;
    public GameObject otpPanel;
    public GameObject forgotPasswordPanel;
    public GameObject resetPasswordPanel;

    [Header("Login Inputs")]
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;

    [Header("Signup Inputs")]
    public TMP_InputField signupUsername;
    public TMP_InputField signupEmail;   
    public TMP_InputField signupPassword;
    public TMP_InputField signupConfirmPassword;

    [Header("Message Texts")]
    public TMP_Text messageText;

    [Header("OTP Panel")]
    private string pendingEmail = "";
    public TMP_InputField otpInputField;

    [Header("Forgot Password Inputs")]
    public TMP_InputField forgotEmailInput;
    public TMP_InputField resetOtpInput;
    public TMP_InputField newPasswordInput;
    public TMP_InputField confirmNewPasswordInput;
    private string forgotEmail = "";

    [Header("Domain Selection")]
    public List<Toggle> domainToggles = new List<Toggle>();
    public int maxSelection = 3;
    public int minSelection = 1;
    private List<int> selectedDomains = new List<int>();

    [Header("Difficulty Selection")]
    public TMP_Dropdown difficultyDropdown;
    private string selectedDifficulty = "";

    // ---- Message Colors (Soft Modern UI) ----
    Color successColor = new Color(0.45f, 0.85f, 0.55f);   // soft green
    Color errorColor = new Color(0.9f, 0.4f, 0.4f);     // soft red
    Color warningColor = new Color(0.95f, 0.75f, 0.3f);   // soft amber
    Color infoColor = new Color(0.4f, 0.7f, 0.95f);    // soft blue

    void SetMessage(string msg, Color color)
    {
        messageText.text = msg;
        messageText.color = color;
    }

    string baseURL = "http://127.0.0.1:5000/";
    void Awake()
    {
        // if (GameData.openModePanel)
        // {
        //     loginPanel.SetActive(false);
        //     signupPanel.SetActive(false);
        //     modeselectPanel.SetActive(true);
        //     dropDownMenuPanel.SetActive(false);
        //     difficultyPanel.SetActive(false);
        //     otpPanel.SetActive(false);
        //     forgotPasswordPanel.SetActive(false);
        //     resetPasswordPanel.SetActive(false);
        //     GameData.openModePanel = false;
        // }
        // else
        // {
        //     loginPanel.SetActive(true);
        //     signupPanel.SetActive(false);
        //     dropDownMenuPanel.SetActive(false);
        //     difficultyPanel.SetActive(false);
        //     modeselectPanel.SetActive(false);
        //     otpPanel.SetActive(false);
        //     forgotPasswordPanel.SetActive(false);
        //     resetPasswordPanel.SetActive(false);
        // }
        // passwordHintBG.SetActive(false);

        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        otpPanel.SetActive(false);
        forgotPasswordPanel.SetActive(false);
        resetPasswordPanel.SetActive(false);

        // Hide other panels initially
        dropDownMenuPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        passwordHintBG.SetActive(false);

        // Show mode selection directly
        modeselectPanel.SetActive(true);

        // Use guest as the default user
        if (string.IsNullOrEmpty(GameData.username))
        {
            GameData.username = "guest";
        }

        GameData.openModePanel = false;

        Debug.Log("VantageVR started in guest mode");
    }
    void Start()
    {
        signupUsername.onValueChanged.AddListener((value) =>
        {
            signupUsername.text = value.Replace(" ", "");
        });
        foreach (Toggle t in domainToggles)
        {
            t.isOn = false;
        }
        //Clear selected list
        selectedDomains.Clear();
        foreach (Toggle t in domainToggles)
        {
            Toggle currentToggle = t;
            currentToggle.onValueChanged.AddListener(
                (isOn) => OnDomainToggleChanged(currentToggle)
            );
        }

    }


    // ---- PANEL SWITCHING ----

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        messageText.text = "";
    }

    public void ShowSignup()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        messageText.text = "";
    }

    public void ShowPasswordHint()
    {
        passwordHintBG.gameObject.SetActive(true);
    }

    public void HidePasswordHint()
    {
        passwordHintBG.gameObject.SetActive(false);
    }

    //Domain Selection
    void OnDomainToggleChanged(Toggle changedToggle)
    {
        selectedDomains.Clear();

        for (int i = 0; i < domainToggles.Count; i++)
        {
            if (domainToggles[i].isOn)
            {
                selectedDomains.Add(i + 1); 
            }
        }
        // Max selection check
        if (selectedDomains.Count > maxSelection)
        {
            changedToggle.isOn = false;
            SetMessage("You can select maximum 3 domains only!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }
    }
    

    // ---- BUTTON ACTIONS ----

    public void OnLoginClicked()
    {
        // Check empty fields first
        if (string.IsNullOrEmpty(loginUsername.text) && string.IsNullOrEmpty(loginPassword.text))
        {
            SetMessage("Please enter username and password!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }
        else if (string.IsNullOrEmpty(loginUsername.text))
        {
            SetMessage("Please enter username!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }
        else if (string.IsNullOrEmpty(loginPassword.text))
        {
            SetMessage("Please enter password!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (loginUsername.text.Length > 30)
        {
            SetMessage("Username too long (max 30 characters)", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (loginPassword.text.Length > 25)
        {
            SetMessage("Password too long (max 25 characters)", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        //call API if all conditions satisfy
        StartCoroutine(LoginRequest());
    }


    bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    bool IsValidPassword(string pass)
    {
        if (pass.Length < 6) return false;
        bool hasUpper = false;
        bool hasNumber = false;

        foreach (char c in pass)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsDigit(c)) hasNumber = true;
        }

        return hasUpper && hasNumber;
    }


    public void OnSignupClicked()
    {
        string username = signupUsername.text;
        string email = signupEmail.text;
        string password = signupPassword.text;
        string confirmPassword = signupConfirmPassword.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
        {
            SetMessage("All fields are required!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        // Username check
        if (!char.IsLetter(username[0]))
        {
            SetMessage("Username must start with a letter!", errorColor);
            return;
        }
        if (username.Contains(" "))
        {
            SetMessage("Username cannot contain spaces!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (!IsValidEmail(email))
        {
            SetMessage("Enter a valid email address!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (!IsValidPassword(password))
        {
            SetMessage("Password must have 1 uppercase, 1 number, min 6 characters", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (password != confirmPassword)
        {
            SetMessage("Confirm password again!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (username.Length > 30)
        {
            SetMessage("Username cannot exceed 30 characters!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (password.Length > 25)
        {
            SetMessage("Password cannot exceed 25 characters!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        SetMessage("Creating account...", infoColor);
        StartCoroutine(ClearMessageAfterDelay(3f));

        // Call API if conditions satisfy
        StartCoroutine(SignupRequest(username, password, email));
    }

    public void OnVerifyOTPClicked()
    {
        string otp = otpInputField.text.Trim();

        if (string.IsNullOrEmpty(otp))
        {
            SetMessage("Please enter the OTP!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (otp.Length != 6)
        {
            SetMessage("OTP must be 6 digits!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        StartCoroutine(VerifyOTPRequest(otp));
    }

    public void OnForgotPasswordLinkClicked()
    {
        loginPanel.SetActive(false);
        forgotPasswordPanel.SetActive(true);
        messageText.text = "";
    }

    public void OnBackToLoginFromForgot()
    {
        forgotPasswordPanel.SetActive(false);
        loginPanel.SetActive(true);
        messageText.text = "";
    }

    public void OnForgotPasswordOTPClicked()
    {
        string email = forgotEmailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            SetMessage("Please enter your email!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        SetMessage("Sending OTP...", infoColor);
        StartCoroutine(ForgotPasswordRequest(email));
    }

    public void OnResetPasswordClicked()
    {
        string otp = resetOtpInput.text.Trim();
        string newPass = newPasswordInput.text;
        string confirmPass = confirmNewPasswordInput.text;

        if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
        {
            SetMessage("All fields are required!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (newPass != confirmPass)
        {
            SetMessage("Passwords do not match!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        if (!IsValidPassword(newPass))
        {
            SetMessage("Password must have 1 uppercase, 1 number, min 6 characters", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        StartCoroutine(ResetPasswordRequest(otp, newPass));
    }

    public void OnContinueFromDomain()
    {
        selectedDomains.Clear();

        for (int i = 0; i < domainToggles.Count; i++)
        {
            if (domainToggles[i].isOn)
            {
                selectedDomains.Add(i + 1);
            }
        }

        if (selectedDomains.Count < minSelection)
        {
            SetMessage("Please select at least 1 domain!", warningColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
            return;
        }

        GameData.selectedDomains = new List<int>(selectedDomains);

        dropDownMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);

        Debug.Log("Domains Selected: " + selectedDomains.Count);
        Debug.Log("BUTTON CLICKED");
    }

    public void OnInterviewClicked()
    {
        GameData.mode = "INTERVIEW";
        modeselectPanel.SetActive(false);
        dropDownMenuPanel.SetActive(true);
    }
    
    public void OnMCQClicked()
    {
        GameData.mode = "MCQ";
        SceneManager.LoadScene("interviewRoom");
    }

    public void OnBackFromDomain()
    {
        dropDownMenuPanel.SetActive(false); 
        modeselectPanel.SetActive(true);
        
    }
    public void OnBackFromDifficulty()
    {
        difficultyPanel.SetActive(false);
        dropDownMenuPanel.SetActive(true);

    }


    public void OnStartInterview()
    {
        if (difficultyDropdown.value == 0)
            selectedDifficulty = "Easy";
        else if (difficultyDropdown.value == 1)
            selectedDifficulty = "Medium";
        else
            selectedDifficulty = "Hard";

        Debug.Log("Selected Difficulty: " + selectedDifficulty);

        GameData.difficulty = selectedDifficulty;

        // ✅ Load interview scene
        GameData.mode = "INTERVIEW";
        SceneManager.LoadScene("interviewRoom");
    }


    public void OnLogoutClicked()
    {
        // Hide menu
        dropDownMenuPanel.SetActive(false);
        modeselectPanel.SetActive(false);

        // IMPORTANT: hide difficulty UI
        difficultyPanel.SetActive(false);

        // Clear input fields
        loginUsername.text = "";
        loginPassword.text = "";
        signupUsername.text = "";
        signupEmail.text = "";
        signupPassword.text = "";
        signupConfirmPassword.text = "";

        // Clear message
        messageText.text = "";

        // Show login panel again
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
    }


    // ---- BACKEND CALLS ----

    IEnumerator ClearMessageAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        messageText.text = "";
    }


    IEnumerator GoToModeAfterDelay()
    {
        SetMessage("Login Successful!", successColor);
        StartCoroutine(ClearMessageAfterDelay(3f));
        GameData.username = loginUsername.text;
        yield return new WaitForSeconds(1.5f);   // show message briefly
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        modeselectPanel.SetActive(true);
    }



    IEnumerator LoginRequest()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", loginUsername.text);
        form.AddField("password", loginPassword.text);

        UnityWebRequest www = UnityWebRequest.Post(baseURL + "login.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetMessage("Server error!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
        }
        else
        {
            string res = www.downloadHandler.text.Trim();

            if (res == "SUCCESS")
            {
                StartCoroutine(GoToModeAfterDelay());
            }


            else
            {
                SetMessage("Invalid username or password!", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
        }
    }

    IEnumerator ShowLoginAfterDelay()
    {
        SetMessage("Account created! Please login.", successColor);
        StartCoroutine(ClearMessageAfterDelay(3f));
        yield return new WaitForSeconds(2f);   // 2 seconds delay
        ShowLogin();
    }



    IEnumerator SignupRequest(string username, string password, string email)
    {
    WWWForm form = new WWWForm();
    form.AddField("username", username);
    form.AddField("password", password);
    form.AddField("email", email);

    using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/unity_api/signup1.php", form))
    {
        yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                SetMessage("Server error!", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
            else
            {
                string response = www.downloadHandler.text.Trim();

                if (response == "OTP_SENT")
                {
                    pendingEmail = email; // save email for OTP verification
                    SetMessage("OTP sent to your email! Check inbox.", successColor);
                    signupPanel.SetActive(false);
                    otpPanel.SetActive(true);
                }

                else if (response.Contains("USERNAME_INVALID"))
                {
                    SetMessage("Username must start with a letter!", errorColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }

                else if (response == "INVALID_EMAIL")
                {
                    SetMessage("Invalid email format!", errorColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }
                else if (response == "EXISTS")
                {
                    SetMessage("Username already exists!", errorColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }
                else if (response == "WEAK_PASSWORD")
                {
                    SetMessage("Password must have 1 uppercase, 1 number, min 6 characters", errorColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }
                else if (response == "EMPTY")
                {
                    SetMessage("All fields are required!", warningColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }
                else if (response == "EMAIL_EXISTS")
                {
                    SetMessage("Email already registered!", errorColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }
                else
                {
                    SetMessage("Signup failed: " + response, errorColor);
                    StartCoroutine(ClearMessageAfterDelay(3f));
                }
            }

        }
    }

    IEnumerator VerifyOTPRequest(string otp)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", pendingEmail);
        form.AddField("otp", otp);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/unity_api/verify_otp.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetMessage("Server error!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
        }
        else
        {
            string res = www.downloadHandler.text.Trim();

            if (res == "SUCCESS")
            {
                otpPanel.SetActive(false);
                StartCoroutine(ShowLoginAfterDelay()); // account verified, go to login
            }
            else if (res == "INVALID_OTP")
            {
                SetMessage("Incorrect OTP! Try again.", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
            else if (res == "OTP_EXPIRED")
            {
                SetMessage("OTP expired! Please sign up again.", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
                otpPanel.SetActive(false);
                signupPanel.SetActive(true);
            }
            else
            {
                SetMessage("Verification failed: " + res, errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
        }
    }

    IEnumerator ForgotPasswordRequest(string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);

        UnityWebRequest www = UnityWebRequest.Post(baseURL + "forgot_password.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetMessage("Server error!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
        }
        else
        {
            string res = www.downloadHandler.text.Trim();

            if (res == "OTP_SENT")
            {
                forgotEmail = email;
                SetMessage("OTP sent to your email!", successColor);
                forgotPasswordPanel.SetActive(false);
                resetPasswordPanel.SetActive(true);
            }
            else if (res == "EMAIL_NOT_FOUND")
            {
                SetMessage("No account found with this email!", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
            else if (res == "INVALID_EMAIL")
            {
                SetMessage("Enter a valid email address!", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
            else
            {
                SetMessage("Error: " + res, errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
        }
    }

    IEnumerator ResetPasswordRequest(string otp, string newPassword)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", forgotEmail);
        form.AddField("otp", otp);
        form.AddField("password", newPassword);

        UnityWebRequest www = UnityWebRequest.Post(baseURL + "reset_password.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetMessage("Server error!", errorColor);
            StartCoroutine(ClearMessageAfterDelay(3f));
        }
        else
        {
            string res = www.downloadHandler.text.Trim();

            if (res == "SUCCESS")
            {
                SetMessage("Password reset successful! Please login.", successColor);
                yield return new WaitForSeconds(2f);
                resetPasswordPanel.SetActive(false);
                loginPanel.SetActive(true);
                messageText.text = "";
            }
            else if (res == "INVALID_OTP")
            {
                SetMessage("Incorrect OTP! Try again.", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
            else if (res == "OTP_EXPIRED")
            {
                SetMessage("OTP expired! Request a new one.", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
                resetPasswordPanel.SetActive(false);
                forgotPasswordPanel.SetActive(true);
            }
            else if (res == "WEAK_PASSWORD")
            {
                SetMessage("Password must have 1 uppercase, 1 number, min 6 characters", errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
            else
            {
                SetMessage("Error: " + res, errorColor);
                StartCoroutine(ClearMessageAfterDelay(3f));
            }
        }
    }

}
