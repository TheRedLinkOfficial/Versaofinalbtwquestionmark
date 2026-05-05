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
             StartCoroutine(AlocarInputAposCarregamento());
     }
 
    
    public void CarregarCena(string nomeDaCena)
     {
         
         SceneManager.LoadScene(nomeDaCena);
         
       
         SceneManager.sceneLoaded += AoTerminarDeCarregar;
     }
 
     private void AoTerminarDeCarregar(Scene cena, LoadSceneMode modo)
     {
        
         SceneManager.sceneLoaded -= AoTerminarDeCarregar;
 
        
         if (cena.name == "Cena_MenuPrincipal") 
             MudarEstado(GameState.MenuPrincipal);
         else if (cena.name == "GetStarted_Scene") 
             MudarEstado(GameState.Gameplay);
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
 
     
     public void SairDoJogo()
     {
         Debug.Log("Quitting");
         Application.Quit();
     }
}
