using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Loading Settings")]
    [SerializeField] private float additionalWaitTime = 3f; // Tiempo adicional en segundos

    public void LoadLevelBtn(string levelToLoad)
    {
        // Desactivar menú principal
        mainMenu.SetActive(false);

        // Activar pantalla de carga 
        loadingScreen.SetActive(true);

        // Iniciar la carga con tiempo de espera
        StartCoroutine(LoadLevelWithDelay(levelToLoad));
    }

    IEnumerator LoadLevelWithDelay(string levelToLoad)
    {
        // Esperar un frame para asegurar que la pantalla de carga se muestre
        yield return null;

        // Iniciar la carga de escena pero no permitir activación automática
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);
        asyncLoad.allowSceneActivation = false;

        // Esperar hasta que la carga llegue al 90% (que es cuando isDone sería true normalmente)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // La carga ha completado, ahora esperamos el tiempo adicional
        Debug.Log("Carga completa, esperando tiempo adicional: " + additionalWaitTime + " segundos");

        // Esperar el tiempo adicional para mostrar la pantalla de carga
        yield return new WaitForSeconds(additionalWaitTime);

        // Finalmente permitir que la escena se active
        asyncLoad.allowSceneActivation = true;
    }
}