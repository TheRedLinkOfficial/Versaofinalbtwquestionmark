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
             Debug.Log("Destruindo instância duplicada do GameManager.");
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
                     StopAllCoroutines(); // Limpa corrotinas anteriores para evitar conflitos
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
         // Em vez de mudar o estado imediatamente, vamos usar um evento da Unity
         // que avisa quando a cena terminou de carregar.
         SceneManager.LoadScene(nomeDaCena);
         
         // Subscrevemos temporariamente a um evento da Unity
         SceneManager.sceneLoaded += AoTerminarDeCarregar;
     }
 
     private void AoTerminarDeCarregar(Scene cena, LoadSceneMode modo)
     {
         // 1. Removemos o evento para não disparar de novo na próxima cena
         SceneManager.sceneLoaded -= AoTerminarDeCarregar;
 
         // 2. Agora que a cena carregou, mudamos o estado com segurança
         if (cena.name == "Cena_MenuPrincipal") 
             MudarEstado(GameState.MenuPrincipal);
         else if (cena.name == "GetStarted_Scene") 
             MudarEstado(GameState.Gameplay);
     }
 
     // 5. Alocação de Inputs (Input System)
     private IEnumerator AlocarInputAposCarregamento()
     {
         // Espera um frame para garantir que os objetos da cena foram instanciados
         yield return new WaitForEndOfFrame();
 
         _playerInputNaCena = FindFirstObjectByType<PlayerInput>();
 
         if (_playerInputNaCena != null)
         {
             Debug.Log("<color=green>[GameManager]</color> Input alocado com sucesso ao Player.");
             // No single player, o PlayerInput já costuma pegar o input padrão, 
             // mas aqui você pode forçar esquemas de controle se necessário:
             _playerInputNaCena.ActivateInput();
         }
         else
         {
             Debug.LogWarning("[GameManager] PlayerInput não encontrado na cena de Gameplay.");
         }
     }
 
     // Função para o botão Sair
     public void SairDoJogo()
     {
         Debug.Log("Saindo do jogo...");
         Application.Quit();
     }
}
