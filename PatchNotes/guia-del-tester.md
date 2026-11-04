# 🔥 Flames of Faith — Guía para testers

¡Bienvenido al equipo de pruebas! Esta guía resume qué es el juego, cómo funciona cada sistema y, sobre todo, **en qué necesitamos tu opinión**. No hace falta leerla entera antes de jugar: juega una primera partida a ciegas (esa primera impresión vale mucho) y vuelve aquí después.

> **Sobre tu versión:** las partes marcadas con 🆕 llegan en la build siguiente a la v0.5.2. Si tu build es la v0.5.2, todavía no las tiene.
>
> **Idioma:** el juego está en inglés por ahora (el español llegará en la versión 1.0). Por eso los nombres del juego aparecen aquí en inglés, tal y como los verás en pantalla.

---

## 1. El juego en dos minutos

**Flames of Faith** es un roguelite de acción en 2D con vista isométrica. El objetivo es lanzar una **demo en Steam en diciembre de 2026** con el **Acto I** completo. El juego final (1.0) tendrá cuatro actos.

**La historia.** Hace sesenta años cayó el reino de Ashemdrek y se apagó la última **Flame of Faith**, el fuego blanco que mantenía bajo tierra a los **Drekos**, unos seres que vinieron de las estrellas. Desde entonces una niebla oscura, el **Miasma**, se extiende por el continente. Tres peregrinos, nietos de quienes estuvieron allí cuando cayó Ashemdrek, emprenden la **Pilgrimage of the Flame** para volver a encender los **Altars of Malien**.

**Cómo se juega.** Cada intento es una **Pilgrimage**. Avanzas por un mapa de caminos que se ramifican, eliges ruta y te enfrentas a oleadas de enemigos durante un tiempo fijo. Por el camino encuentras tiendas, hogueras, cofres y eventos. Subes de nivel, eliges mejoras de stats y reúnes objetos. Si mueres, empiezas de cero, pero los **Favors** que hayas ganado se guardan para desbloquear personajes y mejorar su equipo inicial.

---

## 2. Los tres peregrinos

En las builds de pruebas los tres personajes están desbloqueados. En el juego normal, Archer y Augur cuestan 50 Favors cada uno.

| | **Warrior** | **Archer** | **Augur** |
|---|---|---|---|
| **Personaje** | Ayteel Alamoh | Faelin Vidal | Werlem Feth |
| **Estilo** | Cuerpo a cuerpo, aguanta golpes | A distancia, móvil | Magia, daño en área |
| **Arma** | Steeldawn Sword | Steeldawn Bow | Akeros Graceful Scepter |
| **Ataque básico** | Un tajo en arco que puede causar Bleed | Una flecha bendecida; las mejoras añaden un cono de flechas | Una bola de fuego que puede quemar; las mejoras añaden una explosión |
| **Especial** | **Whirlwind:** gira 2 s golpeando a todo lo que toca. Sigues pudiendo recibir daño. Las mejoras añaden un escudo que absorbe proyectiles y cura | **Barrage:** un anillo de flechas y después flechas que persiguen al objetivo fijado. Eres invulnerable mientras dura | **Fire Tornadoes:** tornados de fuego que buscan enemigos y explosiones sobre el objetivo fijado |
| **Dash** | **Crusader Charge:** carga atravesando enemigos, los daña y los empuja | **Trap Step:** deja una trampa que daña al primer enemigo que la pisa | **Flame Dash:** atraviesa enemigos y deja un rastro de llamas |

Cada personaje empieza con un **kit de seis objetos**: el arma, una pieza secundaria, un **Emblem** (+1 a su stat principal por nivel) y tres **Glyphs**, uno por habilidad. Cada pieza del kit tiene mejoras de +1 a +3 que se compran con Favors en el **Skill Tree**, y se aplican a partir de la siguiente Pilgrimage.

Cada clase tiene una **stat clave** (estrella dorada) y varias **complementarias** (estrella plateada), para que sepas qué elegir al subir de nivel.

---

## 3. Controles

| Acción | Teclado y ratón | Mando (Xbox) |
|---|---|---|
| Moverse | WASD / flechas | Stick izquierdo |
| Apuntar | Ratón | Stick derecho (o la dirección en la que te mueves) |
| Ataque básico manual | Clic izquierdo / Enter | X |
| Ataque especial | Clic derecho | Y |
| Dash | Espacio | A |
| Activar / desactivar el ataque automático | Ctrl | RT |
| Fijar objetivo (lock-on) | F | LT |
| Ver de dónde sale cada stat (en menús) | Shift izquierdo | LT |
| Pausa | Esc | Start |
| Cambiar de ventana | Ratón | LB / RB |
| Volver / cancelar | Esc | B |

- **Ataque automático** (activado por defecto): atacas solo al enemigo más cercano dentro de tu alcance. Si lo desactivas, atacas tú, con una ligera ayuda al apuntar.
- **Lock-on:** pulsa F / LT para fijar al enemigo más cercano. Si vuelves a pulsar, pasas al siguiente en el sentido de las agujas del reloj. Si no hay ningún enemigo cerca, el objetivo se suelta. Con el ratón también puedes hacer clic sobre un enemigo. Los especiales del Archer y del Augur apuntan al objetivo fijado. En Settings puedes cambiar al modo «mantener pulsado».
- Todo el juego se puede jugar con mando, menús incluidos. Un marco que late indica qué está seleccionado.

---

## 4. Una Pilgrimage, paso a paso

1. **Menú principal:** New Game, Load Game (continúa la partida guardada; solo hay una), Skill Tree, Pilgrim's Chronicle 🆕, Settings y Exit.
2. **Elegir personaje:** pasar por una tarjeta solo muestra su información. Se elige con un clic o con A, y después Start Run.
3. **Escena de apertura** 🆕: la primera vez que empiezas una partida nueva se cuenta la historia en 7 páginas. Mantén pulsado Esc / B para saltarla. Después puedes verla otra vez desde la **Pilgrim's Chronicle** del menú principal.
4. **El mapa del acto:** el Acto I tiene 3 mapas distintos, y cada Pilgrimage usa uno al azar. Las casillas se van descubriendo a medida que avanzas, y puedes volver atrás por los caminos que ya conoces. Cada combate muestra su **Taint Level**, que indica lo corrupto que está ese lugar.
5. **Tipos de casilla:**
   - **Combate:** 2 oleadas de enemigos, de 30 a 45 segundos cada una.
   - **Plerian Shop:** compra objetos con **Golden Chronos**, la moneda de la Pilgrimage.
   - **Campfire:** recuperas vida y Grace.
   - **Treasure:** un cofre con una recompensa.
   - **Strange Encounter:** un evento al azar con 2 o 3 opciones.
   - **Wayside Shrine of Malien:** un evento fijo centrado en la Grace.
   - **Jefe:** la última casilla del acto.
6. **Dentro de un combate:**
   1. Aparecen enemigos desde unos portales mientras dura el tiempo de la oleada.
   2. Cuando el tiempo se acaba aparece el **Corruptor**, y tienes que matarlo antes de que escape (ver la sección 5).
   3. Al terminar la oleada, los enemigos que quedan mueren. En la última oleada se recoge toda la experiencia del suelo.
   4. La ventana **Wave Results** explica cómo ha cambiado tu Grace.
   5. Después decides qué hacer con los objetos que has recogido (quedártelos o venderlos) y eliges mejoras por cada nivel que hayas subido.
7. **Guardado:** se guarda solo al llegar al mapa y al moverte por él. Si sales en mitad de un combate, vuelves al mapa tal y como estaba antes de entrar.
8. **Fin:** la Pilgrimage termina si mueres o si derrotas al jefe del Acto I. En ese momento se guardan los Favors que hayas ganado.

---

## 5. Sistemas principales

### Grace y Corruption (el corazón del juego)
- **Grace** es un valor de **−50 a +50** que te acompaña toda la Pilgrimage. Cuanta más tengas, más daño haces. Por debajo de 0, haces menos.
- **Corrupted:** con la Grace por debajo de 0 quedas **Corrupted**. Bajan tu Armor y tu curación, aparecen más enemigos corruptos y te sale un contorno morado.
- **Corruption** se cuenta **solo durante cada oleada** y sale de dos sitios:
  - el daño que recibes de enemigos corruptos (los que tienen contorno morado);
  - el **Corruptor**: cuanto más tardas en matarlo, más Corruption te da, y si escapa por su portal te da aún más.
- **Al final de cada oleada:** Grace = Grace + Grace Affinity − Corruption. La ventana Wave Results lo explica paso a paso.
- **Otras fuentes de Grace:** la Campfire, el Wayside Shrine, algunos eventos, los objetos de Grace que sueltan los enemigos y la stat Retribution.

### Favors y progresión entre partidas
- Ganas **+1 Favor** por cada oleada que termines con la Grace por encima de 0, **+5** por matar al jefe, y algunos más en eventos.
- Solo cuentan los niveles que terminas, y se guardan cuando acaba la Pilgrimage. Si abandonas una partida empezando otra nueva, los pierdes.
- En el **Skill Tree** desbloqueas personajes (50) y mejoras cada pieza del kit (10 / 15 / 20 por nivel de mejora). Todos los personajes comparten la misma reserva de Favors.
- Una partida perdida normal da unos 12-18 Favors.

### Stats y subir de nivel
Cada nivel cuesta un 20 % más de experiencia que el anterior. Al subir eliges una mejora de stat, con una vista previa del valor de **ahora** y el de **después**. Las mejoras vienen en distintos niveles de calidad, y Luck mejora esa calidad.

| Stat | Qué hace |
|---|---|
| Max Health | Vida máxima |
| Armor | Reduce todo el daño recibido; en negativo, lo aumenta |
| Evasion | Probabilidad de esquivar un golpe entero |
| Health Regen | Cura con el tiempo |
| Healing Received | Suma vida a cada curación |
| Life Steal | Probabilidad de curarte al golpear |
| Melee / Ranged / Magic Damage | Daño cuerpo a cuerpo, a distancia y mágico |
| Attack Speed | Reduce los tiempos de recarga. A partir de 40 puntos rinde la mitad, y a partir de 80 ya no suma |
| Range | Alcance del arma y de la selección de objetivo |
| Pierce | Los proyectiles atraviesan más enemigos; las flechas pueden dividirse |
| Skill Duration | Las habilidades y los efectos de los objetos duran más |
| Mastow Affinity | Los efectos elementales son más fuertes y duran más |
| Retribution | Devuelve parte del daño recibido; puede dar Grace |
| Grace Affinity | Grace extra al final de cada oleada, y más objetos de Grace |
| Move Speed | Velocidad de movimiento |
| Dash Cooldown | El dash se recarga antes |
| Pickup Range | Recoges objetos y experiencia desde más lejos |
| Experience to Level Up | Los enemigos sueltan más experiencia |
| Luck | Mejores mejoras al subir de nivel, más y mejores objetos, mejores resultados en los eventos |
| Shop Item Discount | Precios más bajos en la tienda |

### Objetos
- Hay **43 objetos** además de los kits. Las calidades que verás son Common, Uncommon, Rare y Heroic.
- Dan stats fijas o en porcentaje, efectos automáticos (espadas que orbitan, misiles sagrados, dagas al recibir golpes, rayos...), infusiones elementales o habilidades.
- Salen de las tiendas, de los cofres, de los eventos y de los enemigos.
- Algunos objetos tienen Luck negativa (**Cursed Coin**, **Faikan Boots**): son más fuertes en otras cosas, pero empeoran lo que te toca.
- **Truco:** en los menús, mantén Shift / LT sobre una stat y se marcan los objetos que la suben.

### Efectos elementales
| Efecto | Qué hace |
|---|---|
| **Fire** | Quema con el tiempo y puede contagiarse a los enemigos cercanos |
| **Frost** | Ralentiza al enemigo y hace que reciba más daño |
| **Bleed** | Daño con el tiempo que se acumula (hasta 5) |
| **Ley Charge** | Cuando un enemigo cargado recibe daño, un rayo salta a otro enemigo cercano |

### Eventos
Son pequeñas historias. Las opciones no te dicen qué vas a ganar o perder: te dan una **pista** dentro de la historia. Algunas dependen de tus stats o de tu Grace, y otras llevan a más preguntas. Si respondes bien según la historia del mundo, ganas algo; si respondes mal, te cuesta algo. Cada personaje tiene frases propias y algunas opciones exclusivas. En una misma Pilgrimage un evento no se repite hasta que has visto todos.

### Aviso de vida baja 🆕
Cuando bajas del **30 % de vida** en un combate, los bordes de la pantalla laten en rojo. El latido es más fuerte y más rápido cuanto menos vida te queda, y con muy poca vida oirás un latido de corazón. Lo hemos añadido porque muchos testers no miraban la barra de vida, que está arriba a la izquierda. El dibujo es provisional.

### Sonido y sensación de golpe 🆕
- **Sonido nuevo:** recibir golpes, el dash, recoger cosas (la experiencia sube de tono si recoges varias seguidas), subir de nivel, el inicio y el final de cada oleada, la tienda, los menús, el Corruptor y el jefe. Los personajes tienen voces para los golpes, el dash, la muerte y la subida de nivel.
- **Momentos importantes:** los golpes fuertes, tu muerte, matar al Corruptor y las fases del jefe hacen temblar la cámara, congelan el juego un instante o lo ralentizan, y hacen vibrar el mando. Los golpes normales no hacen nada de esto, a propósito.
- En **Settings > Gameplay** hay dos controles nuevos, **Screen shake** y **Rumble**, que van de 0 (desactivado) a 1.

---

## 6. Enemigos y jefe del Acto I

| Enemigo | Qué hace |
|---|---|
| **Slime Chaser** | Salta hacia ti y hace daño al tocarte |
| **Slime Roamer** | Salta en direcciones al azar |
| **Explosive Slime** | Salta hacia ti y explota |
| **Flame Shooter** | Flota detrás de ti y dispara bolas de fuego |
| **Dreko Eye** | Camina, se para y dispara en abanico de tres |
| **Light Devourer** | Golpea cuerpo a cuerpo y tiene mucha vida. Antes de golpear marca la zona en el suelo con un círculo rojo |
| **Corruptor** | Aparece al final de cada oleada, huye de ti y dispara. Si no lo matas a tiempo, escapa |

Cualquier enemigo puede aparecer **corrupto** (contorno morado): tiene más vida y hace más daño.

**Jefe: Taio'Sandir**, un antiguo Dreko junto al Altar of Malien.
- **Fase 1:** quédate junto al altar mientras su llama se llena (unos 100 s). En esta fase el jefe es invulnerable y verás **IMMUNE**. Aparece entre la niebla para lanzarte rayos de oscuridad e invoca Dreko Eyes que caminan hacia el altar: si llegan, la llama baja.
- **Fase 2:** con la llama llena, el jefe se vuelve vulnerable. Lanza rayos, ráfagas de proyectiles en arco y un dron que te persigue.

---

## 7. La interfaz

- **Arriba a la izquierda:** la vida, la barra de Grace (con la Corruption pendiente de la oleada), el número de oleada y su tiempo, y una pista opcional para principiantes.
- **Arriba a la derecha:** tu nivel y la barra de experiencia.
- **Abajo a la izquierda:** tus tres habilidades con su recarga, y los indicadores de ataque automático y lock-on.
- **Ventanas centrales:** con cada una se abren a los lados el **Inventory** (izquierda, con tus Favors y Golden Chronos) y el panel de **Stats** (derecha).
- **Settings:**
  - **Gameplay:** ataque automático, modo de lock-on, pistas, Screen shake 🆕 y Rumble 🆕;
  - **Audio:** volumen general, de la música y de los efectos;
  - **Graphics** y **Language**.

---

## 8. En qué necesitamos tu opinión

Esta es la parte más importante. No hace falta contestar a todo: céntrate en lo que más te llame la atención y cuéntanos qué **sentiste**, no solo qué pasó.

### Primera impresión
- ¿La escena de apertura 🆕 te deja claro quién eres y qué haces? ¿Se hace larga?
- En tu primera partida, ¿entendiste qué hacer en el mapa y en el primer combate? ¿Te ayudaron las pistas?
- ¿Algo que no entendiste hasta mucho después?

### Grace, Corruption y el Corruptor
- ¿Entiendes por qué sube o baja tu Grace? ¿Te ayuda la ventana Wave Results?
- ¿Sabías qué era el Corruptor y por qué conviene matarlo rápido?
- ¿Te ha pasado quedar Corrupted sin saber por qué?

### Aviso de vida baja 🆕
- ¿Te das cuenta a tiempo de que tienes poca vida?
- ¿Molesta, tapa la acción o se queda corto? ¿El latido de corazón ayuda o cansa?

### Sonido y sensación de golpe 🆕
- ¿Hay alguna acción que se sienta muda o que suene raro?
- ¿Las voces encajan con cada personaje? ¿Se repiten demasiado?
- ¿Los sonidos de los menús son agradables o pesados?
- ¿El temblor de cámara, las pausas y la cámara lenta (al morir y al matar al jefe) se sienten bien, se pasan o se quedan cortos? ¿La vibración del mando?

### Combate y dificultad
- ¿La dificultad sube de forma justa a lo largo del acto? ¿Algún enemigo te parece injusto?
- ¿Se ve venir a tiempo el golpe del Light Devourer?
- **Jefe:** ¿demasiado fácil o difícil? Otros testers ya nos han dicho que tiene poca vida. ¿Estás de acuerdo? ¿Se entiende la fase 1?
- ¿Qué personaje te parece más fuerte y cuál más débil? ¿Por qué?
- ¿Cuánto tarda una Pilgrimage completa hasta el jefe? Algunos mapas tienen caminos mucho más cortos que otros: ¿lo notas?

### Habilidades y stats
- **Whirlwind** (Warrior): ¿cómo se siente ahora que dura 2 segundos y ya no te hace invulnerable?
- **Attack Speed:** después del ajuste de la v0.5.2, ¿compensa elegirla?
- ¿Las estrellas de stat clave y complementaria te ayudan a elegir mejoras?
- ¿Alguna stat que nunca eliges porque no sirve, o una que siempre eliges porque es la mejor?

### Objetos
- ¿Algún objeto inútil o demasiado fuerte?
- ¿Los objetos de **Frost** pegan lo suficiente? (Corregimos un error que los hacía demasiado fuertes.)
- ¿Las copias repetidas de **Swords of Fire**, **Consecrate** y **Holy Missiles** se notan?
- ¿Compensan **Cursed Coin** y **Faikan Boots**, con su Luck negativa?
- ¿Se entienden los tooltips?
- ¿Los precios de la tienda son razonables?

### Eventos
- ¿Las pistas se entienden? ¿Los textos se hacen largos?
- ¿Algún resultado te pareció injusto o no tuvo sentido?
- ¿Echas en falta opciones propias de tu personaje en algún evento?

### Progresión entre partidas
- ¿Los Favors que ganas te parecen suficientes? ¿Las mejoras del Skill Tree se notan?

### Mando y Steam Deck
- ¿Algún menú donde el mando se atasca o no llega a un botón?
- Si tienes Steam Deck, ¿se lee bien el texto?

---

## 9. Cómo informar

Para cada problema, intenta incluir:
- **Qué pasó** y **qué esperabas** que pasara.
- **Dónde:** personaje, acto, nombre del nivel o evento, número de oleada.
- **Cómo repetirlo,** si sabes hacerlo.
- **Captura o vídeo,** si puedes.
- **La versión de la build.**

**Si el juego se queda en la pantalla de carga** (negra, con el icono girando), envíanos el archivo de registro antes de volver a abrir el juego. Al reiniciar se sobrescribe, aunque la copia anterior queda en `Player-prev.log`. Está aquí:

```
%USERPROFILE%\AppData\LocalLow\QuestMarkerGames\FlamesOfFaith\Player.log
```

---

## 10. Glosario rápido

| En el juego | Qué es |
|---|---|
| **Pilgrimage** | Un intento completo, desde una partida nueva hasta morir o vencer al jefe |
| **Golden Chronos** | La moneda de la Pilgrimage; se pierde al terminarla |
| **Grace** | Tu recurso principal: más Grace, más daño |
| **Corruption** | Lo que te quita Grace al final de cada oleada |
| **Corrupted** | Estado con la Grace por debajo de 0 |
| **Corruptor** | El enemigo que aparece al final de cada oleada |
| **Favors** | La moneda que se guarda entre partidas, para el Skill Tree |
| **Taint Level** | Lo corrupto que está un nivel: más enemigos corruptos |
| **Glyph** | Pieza del kit que da una habilidad (ataque, especial o dash) |
| **Emblem** | Pieza del kit que sube la stat principal en cada nivel |
| **Drekos** | Los seres de las estrellas, los grandes enemigos |
| **Altar of Malien** | El altar que hay que volver a encender al final de cada acto |

---

¡Gracias por ayudarnos a mejorar Flames of Faith! Toda opinión cuenta, también la que parece pequeña. 🙏
