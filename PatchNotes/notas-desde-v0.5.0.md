# 🔥 Flames of Faith — Notas del parche v0.5.1

¡La **interfaz entera está rediseñada**! Todas las ventanas comparten ahora el mismo estilo, tamaños y colores, y se adaptan a la resolución de tu pantalla. Además: una **guía de estadísticas para cada clase**, **eventos renovados** con dos nuevos, adiós a **Cleave** y un buen puñado de arreglos de combate, sobre todo para el **Warrior**.

> ✅ Las partidas guardadas con la v0.5.0 se pueden continuar.

---

## ✨ Contenido nuevo

### Interfaz renovada
- **Un solo estilo para todo:** dos tamaños de letra, marcos decorados en todas las ventanas y márgenes iguales. La interfaz ahora **escala con la resolución**, así que ya no se ve diminuta en pantallas grandes ni gigante en pequeñas.
- **Los botones para avanzar** (Continue, Resume, Retry, Take, Start Run…) están siempre en el mismo sitio: abajo en el centro de la ventana.
- **Colores con significado:** **Grace** en cian, **Corruption** en morado, **Favors** en dorado, amarillo para destacar, verde para lo positivo y rojo para lo negativo. El indicador de auto-ataque **OFF** ahora es rojo, para que se vea bien cuando está apagado.
- **Mando:** un nuevo marco de selección rodea el elemento seleccionado, en lugar del pequeño triángulo que a veces quedaba mal colocado.
- **Tooltips:** más grandes, se ajustan a su texto y aparecen **junto a lo que describen**. Las estadísticas de un objeto salen en una columna a la derecha.
- **HUD:** los paneles de vida, habilidades y pistas tienen marco. Grace y Corruption usan sus colores.
- **Inventory:** cuadrícula más grande (8 × 10), con el marco de cada objeto del color de su calidad y un contador en los objetos repetidos.
- **Stats** (antes «Attributes»): la lista ahora **se desplaza** con la rueda o el mando, así que caben todas las estadísticas.
- **Subida de nivel:** muestra «You are now Level N», y si subes varios niveles de golpe, el número avanza con cada elección.
- **Pausa:** nuevo botón **Settings**, para cambiar las opciones sin salir de la partida. Settings tiene dos columnas y una opción nueva, **Tutorial hints**, para ocultar las pistas de oleada.
- **Objetos encontrados:** la ventana de Take / Sell se abre en el centro con el Inventory y Stats a los lados. Con mando, basta con moverse a un objeto para seleccionarlo.
- **Tesoro:** el objeto del cofre llega a la ventana de objetos encontrados, así que puedes quedártelo o venderlo.
- **Tienda:** cuadrícula con el precio bajo cada objeto. Los que no puedes pagar aparecen oscurecidos y con el precio en rojo. Ahora **pide confirmación antes de comprar**.
- **Resultados de la oleada:** un resumen de lo que ha pasado con tu Grace, con una barra animada y números que vuelan desde el HUD para que se vea de dónde salen. Un botón **Skip / Continue** sustituye a «pulsa A para continuar».
- **Campfire, eventos y resultados:** ventanas centrales con el nuevo estilo. En los resultados, lo que ganas sale en verde y lo que pierdes en rojo.
- **Muerte y victoria:** al morir o al ganar se abren tus Stats y tu Inventory al lado, para revisar la build. La pantalla de victoria ya funciona con mando.
- **Ventanas de confirmación:** oscurecen el fondo y no dejan pulsar lo que hay detrás.
- **Árbol de habilidades:** los costes se ven en verde si te llegan los Favors y en rojo si no. Los Favors, en dorado.
- **Mapa:** nueva ventana arriba a la derecha con el nombre del acto («Act I - Jesterwoods»), el **nivel en el que estás** (cambia al moverte), tus monedas y Save & Exit. Con mando, aparece abajo a la izquierda una guía de botones. Los niveles tienen nombres legibles: **Holy Altar**, **Strange Encounter**, **Campfire**, **Elven Shop**.
- **Menú principal:** debajo de Load Game se ve tu partida guardada («Warrior - Act I - Level 7»). La **selección de clase** es nueva:
  - fichas de cada clase y un panel con su descripción y su arma inicial;
  - un botón **Skill Tree** que abre el árbol de esa clase;
  - eliges con un clic y empiezas con **Start Run**;
  - las transiciones entre menús ahora son suaves.

### Guía de estadísticas por clase
Para saber qué elegir al subir de nivel, cada clase marca su estadística principal con una **estrella dorada** y las que la complementan con una **estrella plateada**. Aparecen arriba del todo en Stats y en las opciones de subida de nivel, y su tooltip explica por qué te interesan.
- **Warrior:** Melee Damage ⭐, con Retribution, Armor, Max Health y Dash Cooldown.
- **Archer:** Ranged Damage ⭐, con Evasion, Pierce y Range.
- **Augur:** Magic Damage ⭐, con Mastow Affinity y Skill Duration.

### Eventos renovados
- **Se acabó «Ignore»:** todas las opciones cuestan algo y dan algo a cambio. Textos y opciones reescritos y reequilibrados.
- **Dos eventos nuevos:** **The Faraclarian Clan** y **Gift of the House of Stars**, que puede darte un Favor. Ya son 5 los eventos aleatorios, junto a **The Vega Casket**, **Raiders from Gracoy** y **The Weeping Chest**.

### Contorno de Corrupción
- Todo lo que está corrompido (enemigos, el jefe, tu personaje y tu marcador en el mapa) se marca con un **contorno morado que late**, en lugar del efecto anterior, que llenaba la pantalla cuando había muchos.

---

## ⚔️ Balance y ajustes
- **Cleave eliminado:** casi nadie lo elegía, y los ataques cuerpo a cuerpo ya golpean a todos los enemigos del arco. Desaparece de Stats y de todos los objetos.
- **Steeldawn Sword:** pierde Cleave y gana **Dash Cooldown Reduction** (+1 / +2 / +2 / +3 según la mejora). Sus mejoras +2 y +3 dan ahora **+8 / +10 Melee Damage** (antes +7 / +8).
- **Glyph of Swords of Fire:** ahora tus ataques cuerpo a cuerpo también tienen un **50 % de probabilidad de aplicar Fire**.
- **Glyph of Crusader Charge** (dash del Warrior): **siempre golpea** a los enemigos que atraviesas. Su daño suma el **150 % de tu Melee Damage**, y los objetos que dan efectos elementales a los golpes cuerpo a cuerpo también funcionan con el dash.
- **Experiencia:** al ganar mucha experiencia de golpe, cada nivel extra salía más barato de lo que debía. Ahora cada nivel cuesta lo suyo.

---

## 🎮 Mandos y controles
- **Arreglado:** en la pantalla de muerte no funcionaban **Shift / LT** (origen de las estadísticas) ni **LB/RB** (cambiar de ventana).
- **Menú principal:** **B** vuelve atrás desde la selección de clase.
- **Mapa:** con mando, **Y** entra en la interfaz y **LB/RB** llega al panel de Stats.
- **Lock-on** viene en modo **Toggle** por defecto en instalaciones nuevas (se puede cambiar en Settings).

---

## 🔧 Otras correcciones
- **Arreglado:** cada golpe del **Warrior** aplicaba sus efectos **8 veces**. Por eso, con **Ley Charge**, el rayo saltaba entre los mismos enemigos con varios rayos y números en cada salto. Ahora es un solo rayo por salto.
- **Arreglado:** el rayo de **Ley Charge** parpadeaba varias veces y parecía que golpeaba otras tantas. Ahora se dibuja una vez y se desvanece.
- **Arreglado:** **DrekoEye** y **FlameShooter** disparaban cada ráfaga 8 veces, una encima de otra. Solo una te hacía daño, pero el escudo sagrado de **Glyph of Whirlwind** curaba 8 veces por disparo. Ahora es una sola ráfaga, así que ese escudo cura menos contra ellos.
- **Arreglado:** tu personaje seguía mirando al último enemigo que atacó aunque estuviera en la otra punta del nivel. Ahora, en cuanto sale de tu alcance, vuelves a mirar hacia el ratón o el stick.
- **Arreglado:** en la última oleada de un nivel, cerrar rápido los resultados podía dejar gemas de experiencia sin recoger.
- **Arreglado:** el panel de **Stats** no aparecía en el mapa, y el nombre del nivel del mapa no se actualizaba.
- **Arreglado:** el barrido de enfriamiento de las habilidades se veía más pequeño que el icono y a veces se quedaba un trocito al terminar.
- **Arreglado:** la palanca de **The Vega Casket** decía que perdías Grace, pero te la daba.
- **Arreglado:** un tesoro con varios objetos solo te daba uno.
- **Arreglado:** se podía abrir el menú de pausa encima de la pantalla de muerte o de victoria.
- **Pergaminos → Glyphs:** todos los objetos «Scroll of …» se llaman ahora **«Glyph of …»**, y se han corregido varios nombres: **Glyph of Consecrate**, **Glyph of Holy Missiles**, **Glyph of Vengeance Daggers** y **Glyph of Ether Thunder**.

---

¡Gracias por seguir probando! Contadnos qué os parece la nueva interfaz y cómo se siente el Warrior sin Cleave. 🙏
