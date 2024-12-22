using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabLoginSystem : MonoBehaviour
{
    // Input fields for Email and Password
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;


    // Buttons for Login, Register, and Reset Password
    public Button loginButton;
    public Button registerButton;
    public Button resetPasswordButton;

    // Text to display feedback messages
    public TextMeshProUGUI messageText;

    void Start()
    {
        // Add listeners for button clicks
        loginButton.onClick.AddListener(Login);
        registerButton.onClick.AddListener(Register);
        resetPasswordButton.onClick.AddListener(ResetPassword);

        // Display a default message
        messageText.text = "Please enter your email and password.";
    }

    // Login function
    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInputField.text,
            Password = passwordInputField.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        messageText.text = "Login successful! Welcome, user ID: " + result.PlayFabId;
    }

    // Register function
    public void Register()
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInputField.text,
            Password = passwordInputField.text,
            RequireBothUsernameAndEmail = false // Make sure this is set to false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }


    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration successful!");
        messageText.text = "Registration successful! You can now log in.";
    }

    // Reset password function
    public void ResetPassword()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInputField.text,
            TitleId = PlayFabSettings.staticSettings.TitleId // Ensure Title ID is set
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordResetSuccess, OnError);
    }

    void OnPasswordResetSuccess(SendAccountRecoveryEmailResult result)
    {
        Debug.Log("Password reset email sent successfully!");
        messageText.text = "Password reset email sent! Please check your inbox.";
    }

    // Error handling
    void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
        messageText.text = "Error: " + error.ErrorMessage;
    }
}
