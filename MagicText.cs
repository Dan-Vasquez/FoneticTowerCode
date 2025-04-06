/*
 * MagicText.cs
 * 
 * Este script es el componente principal del sistema de juego educativo que maneja
 * la lógica de niveles, efectos de texto e interacción con objetos.
 */

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MagicText : MonoBehaviour
{
    #region Variables Públicas
    [Header("Referencias")]
    public TextMeshPro floatingText;
    public Transform playerTransform;
    public TextMeshProUGUI nivelTexto;
    public GameObject panelSeleccionNivel;
    public ParticleSystem particleSystem2;

    [Header("Configuración de Animación")]
    public float finalHeight = 7.0f;
    public float animationDuration = 1.0f;
    public float initialTextSize = 1.0f;
    public float finalTextSize = 4.0f;

    [Header("Efectos de Texto")]
    public float wobbleSpeed = 1.0f;
    public float wobbleAmount = 0.003f;
    public bool useTypewriterEffect = false;
    public float typewriterSpeed = 20.0f;
    #endregion

    #region Variables Privadas
    private int nivelActual = 0;
    private int categoriaActual = 0;
    private bool isAnimating = false;
    private float elapsedTime = 0f;
    private Vector3 initialPosition;
    private int contadorPulsaciones = 1;
    private string textoBase = "";
    private string fullText = "";
    private float currentTypewriterProgress = 0f;
    private bool wobbleEffectInitialized = false;
    private readonly int outlineLayer = 6;
    private ObjetoInteractivo ultimoObjetoIluminado = null;
    private string palabraSeleccionada = "";
    private List<string> silabasDePalabra = new List<string>();
    private List<float> tamañosTexto = new List<float>();
    private List<string> silabasMezcladas = new List<string>();
    private List<string> palabrasOriginalesMezcladas = new List<string>();
    private List<ObjetoInteractivo> objetosInteractivos = new List<ObjetoInteractivo>();

    // Variables específicas de niveles
    private int indiceNivel5Actual = 0;
    private int indiceNivel6Actual = 0;
    private int indiceNivel6Resaltado = 0;
    private List<string> silabasNivel6Mostradas = new List<string>();
    #endregion

    #region Métodos Unity
    private void Start()
    {
        InicializarComponentes();
    }

    private void Update()
    {
        ProcesarInputUsuario();
        ActualizarAnimacion();
    }
    #endregion

    #region Inicialización
    private void InicializarComponentes()
    {
        floatingText.gameObject.SetActive(false);
        initialPosition = transform.position;
        CargarObjetosInteractivos();
        ActualizarTextoNivel();
        InicializarSistemaParticulas();
    }

    private void CargarObjetosInteractivos()
    {
        ObjetoInteractivo[] objetosEnEscena = FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None);
        objetosInteractivos.AddRange(objetosEnEscena);
    }

    private void InicializarSistemaParticulas()
    {
        if (particleSystem2 != null)
        {
            particleSystem2.Stop();
        }
    }
    #endregion

    #region Procesamiento de Input
    private void ProcesarInputUsuario()
    {
        ProcesarInputNiveles();
        ProcesarInputPanelNivel();
        ProcesarInputAnimacion();
        ProcesarInputEliminacion();
    }

    private void ProcesarInputNiveles()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) CambiarNivel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CambiarNivel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CambiarNivel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CambiarNivel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) CambiarNivel(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) CambiarNivel(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) CambiarNivel(6);
    }

    private void ProcesarInputPanelNivel()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && panelSeleccionNivel != null)
        {
            panelSeleccionNivel.SetActive(!panelSeleccionNivel.activeSelf);
        }
    }

    private void ProcesarInputAnimacion()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
        {
            StartAnimation();
        }

        if (isAnimating && Input.GetKeyDown(KeyCode.Q))
        {
            ProcesarPulsacionQ();
        }
    }

    private void ProcesarInputEliminacion()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            EliminarObjetosEnLayerOutline();
        }
    }
    #endregion

    #region Gestión de Niveles
    public void CambiarNivel(int nuevoNivel)
    {
        if (nuevoNivel >= 0 && nuevoNivel < GameData.Instance.nivelesDificultad.Count)
        {
            nivelActual = nuevoNivel;
            ActualizarTextoNivel();
            Debug.Log($"Nivel cambiado a: {GameData.Instance.nivelesDificultad[nivelActual].nombre}");
        }
    }

    private void ActualizarTextoNivel()
    {
        if (nivelTexto != null && GameData.Instance.nivelesDificultad.Count > 0)
        {
            var nivel = GameData.Instance.nivelesDificultad[nivelActual];
            nivelTexto.text = $"{nivel.nombre}\n{nivel.descripcion}";
        }
    }
    #endregion

    #region Gestión de Animación
    private void ActualizarAnimacion()
    {
        if (!isAnimating) return;

        ActualizarPosicionTexto();
        ActualizarEfectoEscritura();
        AplicarEfectosVisuales();
        ActualizarRotacionHaciaJugador();
    }

    private void ActualizarPosicionTexto()
    {
        if (!isAnimating) return;

        elapsedTime += Time.deltaTime;
        float t = elapsedTime / animationDuration;
        
        if (t >= 1f)
        {
            t = 1f;
            isAnimating = false;
        }

        float altura = Mathf.Lerp(initialPosition.y, finalHeight, t);
        transform.position = new Vector3(initialPosition.x, altura, initialPosition.z);
        
        float escala = Mathf.Lerp(initialTextSize, finalTextSize, t);
        floatingText.fontSize = escala;
    }

    private void ActualizarEfectoEscritura()
    {
        if (!useTypewriterEffect || currentTypewriterProgress >= fullText.Length) return;

        float velocidadAjustada = typewriterSpeed * GameData.Instance.nivelesDificultad[nivelActual].velocidadHabla;
        currentTypewriterProgress += velocidadAjustada * Time.deltaTime;
        int charactersToShow = Mathf.Min(Mathf.FloorToInt(currentTypewriterProgress), fullText.Length);
        floatingText.text = fullText.Substring(0, charactersToShow);
        floatingText.ForceMeshUpdate();
        wobbleEffectInitialized = false;
    }

    private void AplicarEfectosVisuales()
    {
        if (!floatingText.gameObject.activeInHierarchy) return;

        ApplyWobbleEffect();
    }

    private void ActualizarRotacionHaciaJugador()
    {
        if (playerTransform == null || !floatingText.gameObject.activeInHierarchy) return;

        Vector3 directionToPlayer = playerTransform.position - floatingText.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer);
            floatingText.transform.rotation = lookRotation;
        }
    }
    #endregion

    #region Efectos de Texto
    private void ApplyWobbleEffect()
    {
        if (string.IsNullOrEmpty(floatingText.text)) return;

        if (!wobbleEffectInitialized)
        {
            floatingText.ForceMeshUpdate();
            wobbleEffectInitialized = true;
        }

        TMP_TextInfo textInfo = floatingText.textInfo;
        if (textInfo.characterCount == 0) return;

        Vector3[] vertices = textInfo.meshInfo[0].vertices;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            float timeOffset = Time.time * wobbleSpeed + i * 0.1f;
            Vector3 offset = new Vector3(
                Mathf.Sin(timeOffset * 2.0f) * wobbleAmount,
                Mathf.Cos(timeOffset * 1.5f) * wobbleAmount,
                0
            );

            int vertexIndex = charInfo.vertexIndex;
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] += offset;
            }
        }

        ActualizarMallaTexto(textInfo);
    }

    private void ActualizarMallaTexto(TMP_TextInfo textInfo)
    {
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            floatingText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    #endregion

    #region Gestión de Objetos
    private void EliminarObjetosEnLayerOutline()
    {
        int layerMask = 1 << outlineLayer;
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);
        int objetosEliminados = 0;

        foreach (Collider collider in objectsInLayer)
        {
            GameObject objToDestroy = collider.gameObject;
            string nombreObjeto = objToDestroy.name;
            Destroy(objToDestroy);
            objetosEliminados++;
            Debug.Log($"Objeto eliminado: {nombreObjeto}");
        }

        if (objetosEliminados > 0)
        {
            ProcesarEliminacionExitosa();
        }
        else
        {
            Debug.Log("No se encontraron objetos en el layer Outline para eliminar");
        }
    }

    private void ProcesarEliminacionExitosa()
    {
        ultimoObjetoIluminado = null;
        Debug.Log($"Se eliminaron objetos del layer Outline");

        if (particleSystem2 != null)
        {
            particleSystem2.Play();
            Invoke(nameof(StopParticleSystem), 5f);
        }
    }

    private void StopParticleSystem()
    {
        if (particleSystem2 != null)
        {
            particleSystem2.Stop();
        }
    }

    private void CambiarLayerObjeto()
    {
        List<ObjetoInteractivo> objetosInteractivos = new List<ObjetoInteractivo>(
            FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None)
        );

        List<ObjetoInteractivo> objetosCoincidentes = new List<ObjetoInteractivo>();

        Debug.Log($"Palabra seleccionada: {palabraSeleccionada}");
        Debug.Log($"Texto base: {textoBase}");
        
        if (nivelActual == 3)
        {
            Debug.Log($"Nivel 4 - Palabras originales: {string.Join(", ", palabrasOriginalesMezcladas)}");
        }
        
        // Buscar coincidencia exacta
        var coincidenciaExacta = BuscarCoincidenciaExacta(objetosInteractivos);
        if (coincidenciaExacta != null && nivelActual != 3)
        {
            AplicarCoincidenciaExacta(coincidenciaExacta);
            return;
        }

        // Buscar coincidencias según el nivel
        objetosCoincidentes = BuscarCoincidenciasSegunNivel(objetosInteractivos);

        // Procesar objetos coincidentes
        ProcesarObjetosCoincidentes(objetosCoincidentes);
    }

    private ObjetoInteractivo BuscarCoincidenciaExacta(List<ObjetoInteractivo> objetosInteractivos)
    {
        foreach (ObjetoInteractivo obj in objetosInteractivos)
        {
            if (string.Equals(obj.nombreObjeto, palabraSeleccionada, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"¡Coincidencia exacta encontrada! {obj.nombreObjeto}");
                return obj;
            }
        }
        return null;
    }

    private void AplicarCoincidenciaExacta(ObjetoInteractivo coincidenciaExacta)
    {
        coincidenciaExacta.AplicarOutline();
        ultimoObjetoIluminado = coincidenciaExacta;
        Debug.Log($"Objeto exacto '{coincidenciaExacta.nombreObjeto}' iluminado");
    }

    private List<ObjetoInteractivo> BuscarCoincidenciasSegunNivel(List<ObjetoInteractivo> objetosInteractivos)
    {
        List<ObjetoInteractivo> coincidentes = new List<ObjetoInteractivo>();

        switch (nivelActual)
        {
            case 0:
            case 1:
                coincidentes.AddRange(BuscarCoincidenciasSilabas(objetosInteractivos));
                break;
            case 2:
                coincidentes.AddRange(BuscarCoincidenciasVocales(objetosInteractivos));
                break;
            case 3:
                coincidentes.AddRange(BuscarCoincidenciasNivel4(objetosInteractivos));
                break;
            case 4:
                ConfigurarNivel5();
                break;
            case 5:
                ConfigurarNivel6();
                break;
            case 6:
                ConfigurarNivel7();
                break;
        }

        return coincidentes;
    }

    private IEnumerable<ObjetoInteractivo> BuscarCoincidenciasSilabas(List<ObjetoInteractivo> objetosInteractivos)
    {
        foreach (ObjetoInteractivo obj in objetosInteractivos)
        {
            Debug.Log($"Verificando objeto: {obj.nombreObjeto}");
            List<string> silabasObjeto = ObtenerSilabas(obj.nombreObjeto);
            Debug.Log($"Sílabas del objeto: {string.Join(", ", silabasObjeto)}");
            
            if (silabasObjeto.Any(silaba => 
                string.Equals(silaba, textoBase, System.StringComparison.OrdinalIgnoreCase)))
            {
                Debug.Log($"Sílaba '{textoBase}' encontrada en objeto '{obj.nombreObjeto}'");
                yield return obj;
            }
        }
    }

    private IEnumerable<ObjetoInteractivo> BuscarCoincidenciasVocales(List<ObjetoInteractivo> objetosInteractivos)
    {
        if (textoBase.Length == 0 || EsVocal(textoBase[0])) yield break;

        string consonantePrincipal = textoBase[0].ToString();
        
        foreach (ObjetoInteractivo obj in objetosInteractivos)
        {
            List<string> silabasObjeto = ObtenerSilabas(obj.nombreObjeto);
            if (silabasObjeto.Any(silaba => 
                silaba.Length > 0 && silaba.StartsWith(consonantePrincipal)))
            {
                yield return obj;
            }
        }
    }

    private IEnumerable<ObjetoInteractivo> BuscarCoincidenciasNivel4(List<ObjetoInteractivo> objetosInteractivos)
    {
        Debug.Log($"Nivel 4: Buscando objetos que coincidan con las palabras originales: {string.Join(", ", palabrasOriginalesMezcladas)}");
        
        var objetosPorPalabra = new Dictionary<string, ObjetoInteractivo>();
        
        foreach (string palabraOriginal in palabrasOriginalesMezcladas)
        {
            string palabraClave = palabraOriginal.ToLower();
            string nombrePocion = palabraOriginal;
            
            if (palabraOriginal.StartsWith("Poción de"))
            {
                nombrePocion = palabraOriginal.Substring(10).Trim();
                palabraClave = nombrePocion.ToLower();
            }
            
            if (objetosPorPalabra.ContainsKey(palabraClave)) continue;
            
            var objetoCoincidente = objetosInteractivos.FirstOrDefault(obj =>
                string.Equals(obj.nombreObjeto, palabraOriginal, System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(obj.nombreObjeto, nombrePocion, System.StringComparison.OrdinalIgnoreCase));
                
            if (objetoCoincidente != null)
            {
                objetosPorPalabra[palabraClave] = objetoCoincidente;
                Debug.Log($"Palabra '{palabraOriginal}' será representada por objeto '{objetoCoincidente.nombreObjeto}'");
            }
        }
        
        return objetosPorPalabra.Values;
    }

    private void ProcesarObjetosCoincidentes(List<ObjetoInteractivo> objetosCoincidentes)
    {
        Debug.Log($"Objetos coincidentes encontrados: {objetosCoincidentes.Count}");
        foreach (ObjetoInteractivo obj in objetosCoincidentes)
        {
            Debug.Log($"- {obj.nombreObjeto}");
        }

        if (objetosCoincidentes.Count > 0)
        {
            if (nivelActual == 3)
            {
                ProcesarCoincidentesNivel4(objetosCoincidentes);
            }
            else
            {
                ProcesarCoincidentesNivelRegular(objetosCoincidentes);
            }
        }
        else
        {
            FinalizarSinCoincidencias();
        }
    }

    private void ProcesarCoincidentesNivel4(List<ObjetoInteractivo> objetosCoincidentes)
    {
        Debug.Log($"Nivel 4: Iluminando todos los objetos coincidentes ({objetosCoincidentes.Count})");
        
        var nombresObjetos = new List<string>();
        foreach (ObjetoInteractivo objCoincidente in objetosCoincidentes)
        {
            objCoincidente.AplicarOutline();
            nombresObjetos.Add(objCoincidente.nombreObjeto);
        }
        
        Debug.Log($"OBJETOS SELECCIONADOS: {string.Join(", ", nombresObjetos)}");
        
        if (objetosCoincidentes.Count > 0)
        {
            ultimoObjetoIluminado = objetosCoincidentes[0];
        }
    }

    private void ProcesarCoincidentesNivelRegular(List<ObjetoInteractivo> objetosCoincidentes)
    {
        int indiceAleatorio = Random.Range(0, objetosCoincidentes.Count);
        ObjetoInteractivo objetoSeleccionado = objetosCoincidentes[indiceAleatorio];

        if (nivelActual == 1)
        {
            objetoSeleccionado.AplicarParpadeoMultiple(contadorPulsaciones, 0.5f);
        }
        else
        {
            objetoSeleccionado.AplicarOutline();
        }

        ultimoObjetoIluminado = objetoSeleccionado;
        Debug.Log($"Objeto final seleccionado: '{objetoSeleccionado.nombreObjeto}' iluminado");
    }

    private void FinalizarSinCoincidencias()
    {
        Debug.LogWarning($"No se encontró ningún objeto que coincida con: {textoBase}");
        Invoke(nameof(EndAnimation), 0.5f);
        isAnimating = false;
        Debug.Log("Finalizando animación porque no se encontraron objetos coincidentes");
    }
    #endregion

    #region Utilidades
    private bool EsVocal(char c)
    {
        return "aeiouáéíóúü".Contains(char.ToLower(c).ToString());
    }

    private List<string> ObtenerSilabas(string palabra)
    {
        if (GameData.Instance.particionesPredefinidas.TryGetValue(palabra, out string[] silabas))
        {
            return new List<string>(silabas);
        }

        Debug.LogWarning($"Palabra '{palabra}' no encontrada en el diccionario de particiones. Usando fallback.");
        return new List<string>(palabra.ToCharArray().Select(c => c.ToString()));
    }
    #endregion

    #region Lógica de Niveles
    private void ProcesarPulsacionQ()
    {
        contadorPulsaciones--;
        Debug.Log($"Contador: {contadorPulsaciones}");

        ProcesarComportamientoNivel();
        ActualizarTextoContador();
        VerificarFinalizacionNivel();
    }

    private void ProcesarComportamientoNivel()
    {
        switch (nivelActual)
        {
            case 4: // Nivel 5: Tamaños variables
                ProcesarNivel5();
                break;
            case 5: // Nivel 6: Sílabas progresivas
                ProcesarNivel6();
                break;
            case 6: // Nivel 7: Emociones
                ActualizarColorEmocion();
                break;
        }
    }

    private void ProcesarNivel5()
    {
        indiceNivel5Actual++;
        if (indiceNivel5Actual >= tamañosTexto.Count)
        {
            indiceNivel5Actual = 0;
        }
        Debug.Log($"Nivel 5: Cambiando al tamaño {indiceNivel5Actual + 1} de {tamañosTexto.Count} ({tamañosTexto[indiceNivel5Actual]})");
    }

    private void ProcesarNivel6()
    {
        if (indiceNivel6Actual < silabasDePalabra.Count - 1)
        {
            MostrarNuevaSilabaNivel6();
        }
        else
        {
            RotarSilabaResaltadaNivel6();
        }
    }

    private void MostrarNuevaSilabaNivel6()
    {
        indiceNivel6Actual++;
        silabasNivel6Mostradas.Add(silabasDePalabra[indiceNivel6Actual]);
        indiceNivel6Resaltado = indiceNivel6Actual;
        Debug.Log($"Nivel 6: Mostrando sílaba {indiceNivel6Actual + 1} de {silabasDePalabra.Count} ({silabasDePalabra[indiceNivel6Actual]})");
    }

    private void RotarSilabaResaltadaNivel6()
    {
        indiceNivel6Resaltado = (indiceNivel6Resaltado + 1) % silabasDePalabra.Count;
        Debug.Log($"Nivel 6: Resaltando sílaba {indiceNivel6Resaltado + 1} de {silabasDePalabra.Count} ({silabasDePalabra[indiceNivel6Resaltado]})");
    }

    private void VerificarFinalizacionNivel()
    {
        if (contadorPulsaciones == 0)
        {
            if (ultimoObjetoIluminado == null)
            {
                CambiarLayerObjeto();
            }
        }
        else if (contadorPulsaciones < 0)
        {
            FinalizarNivelSiPosible();
        }
    }

    private void FinalizarNivelSiPosible()
    {
        if (!ExistenObjetosEnLayerOutline())
        {
            Invoke(nameof(EndAnimation), 0);
            isAnimating = false;
        }
        else
        {
            Debug.Log("No se puede finalizar la animación porque aún existen objetos en el layer Outline");
        }
    }

    private void ActualizarTextoContador()
    {
        switch (nivelActual)
        {
            case 0: // Nivel 1: Una sílaba
            case 2: // Nivel 3: Variaciones de vocales
            case 4: // Nivel 5: Una sílaba con tamaños variables
                fullText = textoBase;
                break;

            case 1: // Nivel 2: Repetir sílaba
            case 3: // Nivel 4: Secuencia de sílabas mezcladas
                fullText = contadorPulsaciones > 1 ? $"{textoBase} x{contadorPulsaciones}" : textoBase;
                break;

            case 5: // Nivel 6: Todas las sílabas con una resaltada
                ResaltarSilabaNivel6();
                return; // Salimos aquí porque ResaltarSilabaNivel6 maneja el texto directamente

            case 6: // Nivel 7: Palabra completa con emoción
                ActualizarTextoNivel7();
                break;
        }

        if (!useTypewriterEffect)
        {
            floatingText.text = fullText;
            floatingText.ForceMeshUpdate();
        }
        else
        {
            currentTypewriterProgress = 0f;
        }
        wobbleEffectInitialized = false;
    }

    private void ActualizarTextoNivel7()
    {
        if (contadorPulsaciones > 0 && contadorPulsaciones <= GameData.Instance.coloresEmociones.Length)
        {
            int indiceEmocion = GameData.Instance.coloresEmociones.Length - contadorPulsaciones;
            fullText = $"{textoBase} ({GameData.Instance.nombresEmociones[indiceEmocion]})";
        }
        else
        {
            fullText = textoBase;
        }
    }

    private void ResaltarSilabaNivel6()
    {
        if (silabasNivel6Mostradas.Count == 0) return;

        var textoFormateado = new System.Text.StringBuilder();
        
        for (int i = 0; i <= indiceNivel6Actual && i < silabasDePalabra.Count; i++)
        {
            if (i > 0) textoFormateado.Append(" ");
            
            if (i == indiceNivel6Resaltado)
            {
                float tamañoResaltado = tamañosTexto[1];
                textoFormateado.Append($"<size={Mathf.RoundToInt(tamañoResaltado * 100)}%>{silabasDePalabra[i]}</size>");
            }
            else
            {
                textoFormateado.Append(silabasDePalabra[i]);
            }
        }
        
        floatingText.text = textoFormateado.ToString();
        floatingText.ForceMeshUpdate();
        wobbleEffectInitialized = false;
    }

    private bool ExistenObjetosEnLayerOutline()
    {
        int layerMask = 1 << outlineLayer;
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);
        return objectsInLayer.Length > 0;
    }

    private void StartAnimation()
    {
        // Reiniciar variables de animación
        isAnimating = true;
        elapsedTime = 0f;
        initialPosition = transform.position;
        
        // Activar el texto flotante
        floatingText.gameObject.SetActive(true);
        
        // Seleccionar palabra y configurar nivel
        SeleccionarPalabraAleatoria();
        ConfigurarNivel();
    }

    private void ConfigurarNivel()
    {
        switch (nivelActual)
        {
            case 4: // Nivel 5: Tamaños variables
                ConfigurarNivel5();
                break;
            case 5: // Nivel 6: Sílabas progresivas
                ConfigurarNivel6();
                break;
            case 6: // Nivel 7: Emociones
                ConfigurarNivel7();
                break;
            default:
                ConfigurarNivelBasico();
                break;
        }
    }

    private void ConfigurarNivel5()
    {
        if (silabasDePalabra.Count > 0)
        {
            int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
            textoBase = silabasDePalabra[indiceSilaba];
        }
        else
        {
            textoBase = palabraSeleccionada.Substring(0, 1);
        }
        
        GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
        contadorPulsaciones = tamañosTexto.Count;
        indiceNivel5Actual = 0;
    }

    private void ConfigurarNivel6()
    {
        if (silabasDePalabra.Count > 0)
        {
            indiceNivel6Actual = 0;
            indiceNivel6Resaltado = 0;
            silabasNivel6Mostradas.Clear();
            silabasNivel6Mostradas.Add(silabasDePalabra[0]);
            
            tamañosTexto.Clear();
            tamañosTexto.Add(3.0f);     // Tamaño base
            tamañosTexto.Add(5.0f);     // Tamaño resaltado
            
            contadorPulsaciones = silabasDePalabra.Count + 3;
        }
        else
        {
            textoBase = palabraSeleccionada;
            contadorPulsaciones = 1;
        }
    }

    private void ConfigurarNivel7()
    {
        textoBase = palabraSeleccionada;
        contadorPulsaciones = 5;
        ActualizarColorEmocion();
    }

    private void ConfigurarNivelBasico()
    {
        textoBase = palabraSeleccionada;
        contadorPulsaciones = nivelActual == 1 ? 3 : 1; // Más pulsaciones para nivel 2
    }

    private void GenerarTamañosProporcionalesNivel5(int cantidad)
    {
        tamañosTexto.Clear();
        float tamañoInicial = 2.0f;
        float factorProporcional = 1.5f;
        
        for (int i = 0; i < cantidad; i++)
        {
            float nuevoTamaño = tamañoInicial * Mathf.Pow(factorProporcional, i);
            tamañosTexto.Add(nuevoTamaño);
        }
        
        Debug.Log($"Tamaños proporcionales generados para nivel 5: {string.Join(", ", tamañosTexto)}");
    }

    private void SeleccionarPalabraAleatoria()
    {
        categoriaActual = Random.Range(0, 3);
        int indiceAleatorio;

        switch (categoriaActual)
        {
            case 0: // Vegetales
                indiceAleatorio = Random.Range(0, GameData.Instance.vegetales.Length);
                palabraSeleccionada = GameData.Instance.vegetales[indiceAleatorio];
                break;

            case 1: // Frutas
                indiceAleatorio = Random.Range(0, GameData.Instance.frutas.Length);
                palabraSeleccionada = GameData.Instance.frutas[indiceAleatorio];
                break;

            case 2: // Pociones
                indiceAleatorio = Random.Range(0, GameData.Instance.pociones.Length);
                palabraSeleccionada = nivelActual < 6 ? 
                    GameData.Instance.pociones[indiceAleatorio] : 
                    $"Poción de {GameData.Instance.pociones[indiceAleatorio]}";
                break;
        }
        
        silabasDePalabra = ObtenerSilabas(palabraSeleccionada);
        Debug.Log($"Palabra seleccionada: {palabraSeleccionada}");
        Debug.Log($"Sílabas: {string.Join("-", silabasDePalabra)}");
    }

    private void EndAnimation()
    {
        floatingText.text = "";
        floatingText.gameObject.SetActive(false);
    }
    #endregion
}