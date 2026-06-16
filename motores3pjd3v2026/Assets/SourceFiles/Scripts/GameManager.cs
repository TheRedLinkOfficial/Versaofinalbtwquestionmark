using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{ 
     public enum GameState
     {
         Iniciando,
         MenuPrincipal,
         Gameplay
     }
 
     public static GameManager Instance { get; private set; }
 
     [Header("Configurações")]
     public GameState estadoAtual;
     
     [Header("Cenas")]
     public string CenaGUI = "CenaGUI"; // Declarada a variável que faltava

     private PlayerInput _playerInputNaCena;

     #region Singleton
     private void Awake()
     {
         if (Instance != null && Instance != this)
         { 
             Debug.Log("Destruindo cópia/outra instância do GameManager.");
             Destroy(this.gameObject); 
             return;
         }
 
         Instance = this;
         DontDestroyOnLoad(this.gameObject);
     }
     #endregion

     private void Start()
     {
         if (SceneManager.GetActiveScene().name == "_Boot")
         {
             MudarEstado(GameState.Iniciando);
             Debug.Log($"<color=cyan>[GameManager]</color> Estado: <b>{estadoAtual}</b>");
         }
         CarregarCena("Cena_Splash");
     }
 
     public void MudarEstado(GameState novoEstado)
     {
         if (estadoAtual == novoEstado) return;
 
         estadoAtual = novoEstado;
         Debug.Log($"<color=cyan>[GameManager]</color> Estado: <b>{estadoAtual}</b>");
 
         switch (estadoAtual)
         {
             case GameState.Gameplay:
                 if (this != null && gameObject.activeInHierarchy && this.enabled)
                 {
                     StopAllCoroutines(); 
                     StartCoroutine(AlocarInputAposCarregamento());
                     StartCoroutine(ConfigurarGameplayRoutine()); // Chamando a rotina aqui
                 }
                 else
                 {
                     Invoke(nameof(TentarReativarGameplay), 0.1f);
                 }
                 break;
         }
     }
 
     private void TentarReativarGameplay()
     {
         if (gameObject.activeInHierarchy)
         {
             StartCoroutine(AlocarInputAposCarregamento());
             StartCoroutine(ConfigurarGameplayRoutine());
         }
     }
 
     public void CarregarCena(string nomeDaCena)
     {
         SceneManager.LoadScene(nomeDaCena);
         SceneManager.sceneLoaded += AoTerminarDeCarregar;
     }
 
     private void AoTerminarDeCarregar(Scene cena, LoadSceneMode modo)
     {
         SceneManager.sceneLoaded -= AoTerminarDeCarregar;
 
         if (cena.name == "Menu") 
             MudarEstado(GameState.MenuPrincipal);
         else if (cena.name == "GetStarted_Scene") 
             MudarEstado(GameState.Gameplay);
     }
 
     // Corrotina movida para fora de CarregarCena()
     private IEnumerator ConfigurarGameplayRoutine()
     {
         yield return new WaitForEndOfFrame();

         // Agora a função IsSceneLoaded vai funcionar corretamente
         if (!IsSceneLoaded(CenaGUI))
         {
             Debug.Log($"<color=green>[GameManager]</color> Carregando CenaGUI de forma aditiva...");
             SceneManager.LoadSceneAsync(CenaGUI, LoadSceneMode.Additive);
         }

         _playerInputNaCena = Object.FindFirstObjectByType<PlayerInput>();
         if (_playerInputNaCena != null)
         {
             _playerInputNaCena.ActivateInput();
             Debug.Log("<color=green>[GameManager]</color> PlayerInput ativado.");
         }
     }
 
     private IEnumerator AlocarInputAposCarregamento()
     {
         yield return new WaitForEndOfFrame();
 
         _playerInputNaCena = FindFirstObjectByType<PlayerInput>();
 
         if (_playerInputNaCena != null)
         {
             Debug.Log("<color=green>[GameManager]</color> Input alocado com sucesso ao Player.");
             _playerInputNaCena.ActivateInput();
         }
         else
         {
             Debug.LogWarning("[GameManager] PlayerInput não encontrado na cena de Gameplay.");
         }
     }
 
     // FUNÇÃO EXTRA: Verifica se a cena aditiva já está carregada para não duplicar
     private bool IsSceneLoaded(string nomeDaCena)
     {
         for (int i = 0; i < SceneManager.sceneCount; i++)
         {
             Scene cena = SceneManager.GetSceneAt(i);
             if (cena.name == nomeDaCena)
             {
                 return true;
             }
         }
         return false;
     }

     public void SairDoJogo()
     {
         Debug.Log("Quitting");
         Application.Quit();
     }
}