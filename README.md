# Alien Hunter (AR Slingshot Game)

## Descripción
Alien Hunter es un videojuego de realidad aumentada (AR) tipo resortera, donde tu objetivo es proteger a un alien bueno de los aliens malos que intentan capturarlo. El juego está desarrollado en Unity usando AR Foundation y está diseñado para dispositivos móviles con soporte AR.

## Características principales
- Juego en realidad aumentada: busca superficies reales para jugar.
- Mecánica de resortera: lanza esferas de energía para eliminar aliens malos.
- Alien bueno estático: protégelo de los enemigos.
- Aliens malos se acercan lentamente al alien bueno.
- Sistema de vidas y munición limitada.
- Pantallas de victoria y derrota con puntaje acumulado.
- Menú principal y panel de contexto antes de iniciar la partida.

## Instalación
* Si quieres editar el juego:
  * Descarga Unity desde su sitio oficial: https://unity3d.com/es/get-unity/download
  * Clona este repositorio: `git clone https://github.com/santiagopemo/AR_slingshot_game/`
  * Abre el proyecto en Unity y ve a File -> Build Settings -> Build

* Si solo quieres jugar, descarga la versión para tu dispositivo móvil e instálala:
  * **Android:** [Enlace de descarga disponible en la página del proyecto]
  * **iOS:** [Enlace de descarga disponible en la página del proyecto]

## Cómo jugar
1. Al iniciar la app:
   - Acepta los términos en el panel de contexto.
   - Presiona "Jugar" en el menú principal.
2. Búsqueda de superficies:
   - Apunta la cámara a una superficie horizontal real hasta que se detecte.
   - Selecciona la superficie para colocar el escenario.
3. Inicio del juego:
   - El alien bueno aparece en el centro, los aliens malos empiezan a acercarse.
   - Usa la resortera para lanzar esferas de energía y eliminar a los aliens malos antes de que lleguen al alien bueno.
   - Tienes un número limitado de esferas (munición) y el alien bueno tiene vidas.
4. Fin de la partida:
   - Ganas si sobrevives todos los niveles.
   - Pierdes si el alien bueno pierde todas sus vidas.
   - Puedes ver tu puntaje, jugar de nuevo o salir.

## Controles
- Toca y arrastra la esfera de energía para apuntar y suelta para disparar.
- Botón "Jugar": inicia la partida.
- Botón "Salir": cierra la aplicación.
- Botón "Continuar": acepta el contexto y pasa al juego.
- Botón "Reintentar": vuelve a jugar tras perder o ganar.

## Requisitos
- Dispositivo móvil compatible con ARCore (Android) o ARKit (iOS).
- Unity 2020.3.5f1 o superior para edición.

## Estructura técnica
- Unity AR Foundation para detección de planos y AR.
- GameManager.cs: controla el flujo del juego, UI y lógica de niveles.
- SlingShot.cs: mecánica de disparo.
- Ammo.cs: comportamiento de las esferas.
- GoodAlien.cs: lógica del alien bueno (vidas, invulnerabilidad).
- BadAlien.cs: lógica de los aliens malos (movimiento, ataque).
- UI Canvas: gestión de menús, HUD, pantallas de victoria/derrota.

## Créditos
Proyecto original adaptado y modificado por la comunidad. Inspirado en mecánicas de juegos AR populares, pero con temática propia de defensa de alienígenas.

## Contacto y contribución
¿Tienes dudas, sugerencias o quieres contribuir? ¡Abre un issue o un pull request en este repositorio!
