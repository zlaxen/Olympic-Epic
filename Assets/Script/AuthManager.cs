using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Header("Login")]
    public TMP_InputField email_Login_Field;
    public TMP_InputField pass_Login_Field;
    public TMP_Text warning_Text;
    public TMP_Text confirm_Text;

    [Header("")]
    public TMP_InputField email_Regis_Field;
    public TMP_InputField username_Regis_Field;
    public TMP_InputField pass_Regis_Field;
    public TMP_InputField pass_Regis_Verify_Field;
    public TMP_Text warning_Regis_Text;

    [Header("UI Changer")]
    public GameObject Logins;
    public GameObject Registers;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if(dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.Log("Could not resolve Firebase dependencies : " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Seeting Up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginButton()
    {
        StartCoroutine(Login(email_Login_Field.text, pass_Login_Field.text));
    }

    public void ChangeLogin()
    {
        Registers.gameObject.SetActive(false);
        Logins.gameObject.SetActive(true);
        warning_Regis_Text.text = "";
    }

    public void RegisButton()
    {
        StartCoroutine(Register(email_Regis_Field.text, username_Regis_Field.text, pass_Regis_Field.text));
    }

    public void ChangeRegister()
    {
        Logins.gameObject.SetActive(false);
        Registers.gameObject.SetActive(true);
        warning_Text.text = "";
    }

    private IEnumerator Login(string _email, string _password)
    {
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if(LoginTask.Exception != null)
        {
            Debug.Log(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseException = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError error = (AuthError)firebaseException.ErrorCode;

            string message = "Login Failed !";
            switch (error)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Missing Email";
                    break;
                case AuthError.UserNotFound:
                    message = "User Not Found";
                    break;
            }
        }
        else
        {
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully : {0} ({1})", user.DisplayName, user.Email);
            warning_Text.text = "";
            warning_Text.text = "Logged In";
        }
    }

    private IEnumerator Register(string _email, string _username, string _password)
    {
        if(_username == "")
        {
            warning_Regis_Text.text = "Missing Username";
        }
        else if (pass_Regis_Field.text != pass_Regis_Verify_Field.text)
        {
            warning_Regis_Text.text = "Password Does Not Match";
        }
        else
        {
            var RegisTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => RegisTask.IsCompleted);

            if(RegisTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {RegisTask.Exception}");
                FirebaseException firebaseException = RegisTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string message = "Register Failed !";
                switch (error)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
            }
            else
            {
                user = RegisTask.Result;

                if(user != null)
                {
                    UserProfile profile = new UserProfile { DisplayName = _username };
                    var profileTask = user.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {profileTask.Exception}");
                        FirebaseException firebaseException = profileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError error = (AuthError)firebaseException.ErrorCode;
                        warning_Regis_Text.text = "Username Set Failed !";
                    }
                    else
                    {
                        //UIManager.instance.LoginScreen();
                        warning_Regis_Text.text = "";
                        warning_Regis_Text.text = "Register Successfully";
                    }
                }
            }
        }
    }
}
