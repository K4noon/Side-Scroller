using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // Méthode appelée lors du clic sur le bouton "Start"
    public void OnStartClick()
    { 
        SceneManager.LoadScene("SampleScene"); // Charge la Scene
    }
    // Méthode appelée lors du clic sur le bouton "Quit"
    public void OnQuitClick()
    {
        
       // if (Application.isEditor) // Vérifie si le jeu est en cours d'exécution dans l'éditeur Unity
       // {
           // UnityEditor.EditorApplication.isPlaying = false; // Arrête le mode Play dans l'éditeur
       // }
        

        Application.Quit(); // Ferme l'application 
    }
}
