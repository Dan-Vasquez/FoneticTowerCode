using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clase que contiene todos los datos estáticos del juego
/// </summary>
public class GameData : MonoBehaviour
{
    // Singleton instance
    private static GameData instance;
    public static GameData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameData>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameData");
                    instance = go.AddComponent<GameData>();
                }
            }
            return instance;
        }
    }

    // Arrays para las categorías de textos
    public string[] vegetales = { "Tomate", "Pimiento", "Pepino", "Zanahoria", "Repollo" };
    public string[] frutas = { "Sandía", "Fresa", "Naranja", "Banano", "Manzana" };
    public string[] pociones = { "Amor", "Moco", "Lágrima", "Vida", "Tierra", "Fuego", "Sombra" };

    // Diccionario con las particiones predefinidas para cada palabra
    public Dictionary<string, List<string>> particionesPredefinidas = new Dictionary<string, List<string>>();

    // Colores para las emociones
    public Color[] coloresEmociones = new Color[] {
        new Color(1.0f, 0.0f, 0.0f),   // Rojo (rabia)
        new Color(0.0f, 0.8f, 0.0f),   // Verde (desagrado)
        new Color(0.0f, 0.0f, 1.0f),   // Azul (tristeza)
        new Color(1.0f, 1.0f, 0.0f),   // Amarillo (alegría)
        new Color(0.5f, 0.0f, 0.5f)    // Morado (miedo)
    };

    // Nombres de las emociones
    public string[] nombresEmociones = new string[] {
        "RABIA",
        "DESAGRADO",
        "TRISTEZA",
        "ALEGRÍA",
        "MIEDO"
    };

    // Lista de niveles de dificultad
    public List<NivelDificultad> nivelesDificultad = new List<NivelDificultad>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InicializarDatos();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicializa todos los datos del juego
    /// </summary>
    private void InicializarDatos()
    {
        InicializarParticiones();
        InicializarNivelesDificultad();
    }

    /// <summary>
    /// Inicializa el diccionario de particiones de palabras
    /// </summary>
    private void InicializarParticiones()
    {
        // Vegetales
        particionesPredefinidas.Add("Tomate", new List<string> { "To", "ma", "te" });
        particionesPredefinidas.Add("Pimiento", new List<string> { "Pi", "mi", "en", "to" });
        particionesPredefinidas.Add("Pepino", new List<string> { "Pe", "pi", "no" });
        particionesPredefinidas.Add("Zanahoria", new List<string> { "Za", "na", "ho", "ri" });
        particionesPredefinidas.Add("Repollo", new List<string> { "Re", "po", "llo" });
        
        // Frutas
        particionesPredefinidas.Add("Sandía", new List<string> { "Sa", "dí" });
        particionesPredefinidas.Add("Fresa", new List<string> { "Fre", "sa" });
        particionesPredefinidas.Add("Naranja", new List<string> { "Na", "ra", "ja" });
        particionesPredefinidas.Add("Banano", new List<string> { "Ba", "na", "no" });
        particionesPredefinidas.Add("Manzana", new List<string> { "Ma", "za", "na" });
        
        // Pociones
        particionesPredefinidas.Add("Amor", new List<string> { "mo" });
        particionesPredefinidas.Add("Moco", new List<string> { "Mo", "co" });
        particionesPredefinidas.Add("Lágrima", new List<string> { "Lá", "gri", "ma" });
        particionesPredefinidas.Add("Vida", new List<string> { "Vi", "da" });
        particionesPredefinidas.Add("Tierra", new List<string> { "Ti", "rra" });
        particionesPredefinidas.Add("Fuego", new List<string> { "Fu", "go" });
        particionesPredefinidas.Add("Sombra", new List<string> { "So", "bra"});
    }

    /// <summary>
    /// Inicializa los niveles de dificultad
    /// </summary>
    private void InicializarNivelesDificultad()
    {
        // Nivel 1: Primera sílaba al azar
        NivelDificultad nivel1 = new NivelDificultad();
        nivel1.nombre = "Nivel 1: Sílaba inicial";
        nivel1.descripcion = "Pronunciar una sílaba de la palabra";
        nivel1.ejemplos.Clear();
        nivelesDificultad.Add(nivel1);

        // Nivel 2: Repetición de una sílaba varias veces
        NivelDificultad nivel2 = new NivelDificultad();
        nivel2.nombre = "Nivel 2: Repetición";
        nivel2.descripcion = "Repetir la sílaba varias veces";
        nivel2.ejemplos.Clear();
        nivelesDificultad.Add(nivel2);

        // Nivel 3: Variación de vocales en una sílaba
        NivelDificultad nivel3 = new NivelDificultad();
        nivel3.nombre = "Nivel 3: Variación de vocales";
        nivel3.descripcion = "Cambiar la vocal manteniendo la consonante";
        nivel3.ejemplos.Clear();
        nivelesDificultad.Add(nivel3);

        // Nivel 4: Alternancia de sílabas con diferentes consonantes
        NivelDificultad nivel4 = new NivelDificultad();
        nivel4.nombre = "Nivel 4: Diferentes consonantes";
        nivel4.descripcion = "Alternancia de sílabas de diferentes palabras";
        nivel4.ejemplos.Clear();
        nivelesDificultad.Add(nivel4);

        // Nivel 5: Repetición de una sílaba con tamaños variables
        NivelDificultad nivel5 = new NivelDificultad();
        nivel5.nombre = "Nivel 5: Sílaba con tamaños variables";
        nivel5.descripcion = "Repetir una sílaba con diferentes tamaños";
        nivel5.velocidadHabla = 0.8f;
        nivel5.ejemplos.Clear();
        nivelesDificultad.Add(nivel5);

        // Nivel 6: Mostrar todas las sílabas de una palabra con tamaños variables
        NivelDificultad nivel6 = new NivelDificultad();
        nivel6.nombre = "Nivel 6: Palabra completa con sílabas";
        nivel6.descripcion = "Mostrar todas las sílabas con diferentes tamaños";
        nivel6.velocidadHabla = 0.9f;
        nivel6.ejemplos.Clear();
        nivelesDificultad.Add(nivel6);

        // Nivel 7: Palabra completa repetida varias veces
        NivelDificultad nivel7 = new NivelDificultad();
        nivel7.nombre = "Nivel 7: Palabra completa";
        nivel7.descripcion = "Repetir la palabra completa";
        nivel7.velocidadHabla = 1.0f;
        nivel7.ejemplos.Clear();
        nivelesDificultad.Add(nivel7);
    }
}

[System.Serializable]
public class NivelDificultad
{
    public string nombre;
    public string descripcion;
    public float velocidadHabla = 1.0f;
    public List<string> ejemplos = new List<string>();
} 