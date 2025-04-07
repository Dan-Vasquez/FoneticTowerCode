using UnityEngine;

public class ExitGame : MonoBehaviour
{
    /// <summary>
    /// Método para salir del juego.
    /// </summary>
    public void QuitGame()
    {
        // Si estamos en el editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Si estamos en una build del juego
        Application.Quit();
#endif
    }
}