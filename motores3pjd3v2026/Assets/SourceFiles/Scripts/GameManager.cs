using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Escuta mudança de cena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    #endregion

    #region Game State

    public enum GameState
    {
        Iniciando,
        MenuPrincipal,
        Gameplay
    }

    public GameState CurrentState;

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Estado atual: " + CurrentState);
    }

    #endregion

    #region Scene Management

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    #region Input Allocation

    [SerializeField] private PlayerInput playerInput;

    public void AssignPlayerInput(PlayerInput input)
    {
        playerInput = input;
        Debug.Log("Input atribuído ao jogador.");
    }

    void FindPlayerInput()
    {
        PlayerInput input = FindObjectOfType<PlayerInput>();

        if (input != null)
        {
            AssignPlayerInput(input);
        }
        else
        {
            Debug.LogWarning("Nenhum PlayerInput encontrado na cena.");
        }
    }

    #endregion

    #region Boot Flow

    private void Start()
    {
        // Só roda lógica se estiver na cena _Boot
        if (SceneManager.GetActiveScene().name == "_Boot")
        {
            SetState(GameState.Iniciando);

            // Vai direto pro Splash sem Invoke bugado
            StartCoroutine(BootSequence());
        }
    }

    System.Collections.IEnumerator BootSequence()
    {
        yield return new WaitForSeconds(2f);
        LoadScene("Cena_Splash");
    }

    #endregion

    #region Scene Events

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sempre limpa qualquer coisa pendente
        StopAllCoroutines();

        // Define estado automaticamente por cena
        if (scene.name == "MenuPrincipal")
        {
            SetState(GameState.MenuPrincipal);
        }
        else if (scene.name == "GetStarted_Scene")
        {
            SetState(GameState.Gameplay);

          
      

           
            if (!IsSceneLoaded("GUI"))
            {
                SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
            }

            // Aqui faz a alocação de input quando o player existir
            Invoke(nameof(FindPlayerInput), 0.5f);
        }
    }

    #endregion

    #region Utils

    bool IsSceneLoaded(string name)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == name)
                return true;
        }
        return false;
    }

    #endregion
}