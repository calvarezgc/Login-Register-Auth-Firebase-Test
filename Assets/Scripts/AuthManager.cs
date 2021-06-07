using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    void Awake()
    {
        //Verificar que todas las dependencias necesarias para Firebase están presentes en el sistema
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //Si están disponibles, inicialice Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("No se pudieron resolver todas las dependencias de Firebase:" + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Configuración de Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
    }

    //Función para el botón de inicio de sesión
    public void LoginButton()
    {
        //Llamar a la corrutina de inicio de sesión pasando el correo electrónico y la contraseña
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Función para el botón de registro
    public void RegisterButton()
    {
        //Llamar a la rutina de registro pasando el correo electrónico, la contraseña y el nombre de usuario
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Llamar a la función de inicio de sesión de autenticación de Firebase pasando el correo electrónico y la contraseña
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Espera hasta que se complete la tarea
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //Si hay errores, manéjalos.
            Debug.LogWarning(message: $"No se pudo registrar la tarea con {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Inicio de sesión Fallido!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Falta el correo electrónico ";
                    break;
                case AuthError.MissingPassword:
                    message = "Falta la Contraseña";
                    break;
                case AuthError.WrongPassword:
                    message = "Contraseña Incorrecta";
                    break;
                case AuthError.InvalidEmail:
                    message = "Correo Electronico Inválido";
                    break;
                case AuthError.UserNotFound:
                    message = "La Cuenta No Existe";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            // El usuario ahora está conectado
            // Ahora obtén el resultado
            User = LoginTask.Result;
            Debug.LogFormat("El usuario inició sesión correctamente: { 0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Conectado";
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            // Si el campo de nombre de usuario está en blanco, muestra una advertencia
            warningRegisterText.text = "Falta el nombre de usuario";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            // Si la contraseña no coincide, muestra una advertencia
            warningRegisterText.text = "¡Las contraseñas no coinciden!";
        }
        else 
        {
            // Llame a la función de inicio de sesión de autenticación de Firebase pasando el correo electrónico y la contraseña
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            // Espere hasta que se complete la tarea
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // Si hay errores, manipúlelos
                Debug.LogWarning(message: $"No se pudo registrar la tarea con {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Registro fallido!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Falta el correo electrónico";
                        break;
                    case AuthError.MissingPassword:
                        message = "Falta la Contraseña";
                        break;
                    case AuthError.WeakPassword:
                        message = "Contraseña debil";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Correo electrónico ya en uso";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                // El usuario ya ha sido creado
                // Ahora obtén el resultado
                User = RegisterTask.Result;

                if (User != null)
                {
                    // Crea un perfil de usuario y establece el nombre de usuario
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    // Llamar a la función de perfil de usuario de actualización de autenticación de Firebase pasando el perfil con el nombre de usuario
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    // Espera hasta que se complete la tarea
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // Si hay errores, manipúlalos
                        Debug.LogWarning(message: $"No se pudo registrar la tarea con {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "¡Error al establecer el nombre de usuario!";
                    }
                    else
                    {
                        // El nombre de usuario ahora está configurado
                        // Ahora regresa a la pantalla de inicio de sesión
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }
}
