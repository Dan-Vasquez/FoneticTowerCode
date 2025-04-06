# Sistema de Juego Educativo de Pronunciación

Este proyecto es un juego educativo desarrollado en Unity 6 que ayuda a practicar la pronunciación a través de diferentes niveles y mecánicas interactivas.

## 🎮 Características Principales

### Sistema de Niveles
El juego cuenta con 7 niveles progresivos de dificultad:

1. **Nivel 1: Sílaba inicial**
   - Práctica de pronunciación de sílabas individuales
   - Enfoque en la pronunciación clara y precisa

2. **Nivel 2: Repetición**
   - Repetición de sílabas múltiples veces
   - Ayuda a reforzar la pronunciación correcta

3. **Nivel 3: Variación de vocales**
   - Práctica de diferentes sonidos vocálicos
   - Mantiene la consonante y varía las vocales

4. **Nivel 4: Diferentes consonantes**
   - Alternancia entre sílabas de diferentes palabras
   - Mejora la flexibilidad en la pronunciación

5. **Nivel 5: Sílaba con tamaños variables**
   - Práctica de intensidad y énfasis
   - Visualización dinámica del tamaño de las sílabas

6. **Nivel 6: Palabra completa con sílabas**
   - Construcción progresiva de palabras completas
   - Énfasis en sílabas específicas

7. **Nivel 7: Palabra completa con emociones**
   - Práctica de entonación emocional
   - Incluye diferentes estados emocionales

### Categorías de Palabras
- **Vegetales**: Tomate, Pimiento, Pepino, Zanahoria, Repollo
- **Frutas**: Sandía, Fresa, Naranja, Banano, Manzana
- **Pociones**: Amor, Moco, Lágrima, Vida, Tierra, Fuego, Sombra

## 🎯 Mecánicas de Juego

### Controles
- **E**: Inicia la animación de texto
- **Q**: Reduce el contador y avanza en los ejercicios
- **V**: Quita el resaltado sin eliminar objetos
- **F**: Elimina objetos resaltados
- **1-7**: Selección directa de niveles
- **Tab**: Muestra/oculta el panel de selección de nivel

### Efectos Visuales
- Animaciones de texto flotante
- Efectos de ondulación en el texto
- Sistema de partículas para feedback visual
- Cambios dinámicos de tamaño y color

## 🔧 Componentes Principales

### GeneradorObjetos
- Genera objetos interactivos en el juego
- Gestiona la cantidad y posición de objetos
- Controla el ciclo de vida de los objetos

### DestroyOutlineObjects
- Maneja la destrucción de objetos resaltados
- Incluye efectos visuales y sonoros opcionales

### MoveCamera y MovePlayer
- Sistema de movimiento del jugador
- Control de cámara en primera persona
- Interacción con el entorno

### ObjetoInteractivo
- Define el comportamiento de objetos interactivos
- Gestiona efectos visuales y estados
- Implementa sistema de categorías

### MagicText
- Sistema principal de texto y animaciones
- Gestión de niveles y dificultad
- Implementación de efectos especiales

## 🛠️ Requisitos Técnicos
- Unity (versión recomendada: 2020.3 o superior)
- TextMeshPro para efectos de texto
- Sistema de física y colisiones de Unity

## 🎓 Uso Educativo
Este sistema está diseñado para:
- Práctica de pronunciación
- Desarrollo de habilidades lingüísticas
- Ejercicios de articulación
- Trabajo con emociones y entonación

## 🔄 Flujo de Juego
1. Seleccionar nivel de dificultad
2. Iniciar ejercicio con tecla E
3. Seguir las instrucciones en pantalla
4. Usar Q para avanzar en el ejercicio
5. Completar la secuencia de pronunciación
6. Usar F o V según se desee eliminar o mantener objetos

## 🎨 Personalización
El sistema permite personalizar:
- Palabras y categorías
- Efectos visuales y sonoros
- Niveles de dificultad
- Comportamiento de objetos
- Efectos de texto y animaciones 