using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
public class MenuManager : MonoBehaviour
{
    public void Startgame()
    {
        GameManager.Instance.CarregarCena("GetStarted_Scene");
    }
  public void quitGame()
  {
    Debug.Log("Quit Game");
    Application.Quit();
  }
  
}
