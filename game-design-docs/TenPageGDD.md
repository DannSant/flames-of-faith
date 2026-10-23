# Flames of Faith – Ten Page GDD

## 1 – Título

Flames of Faith
**Género:** RPG con historia y survivor like

## 2 – Historia y Gameplay

### Historia

La historia ocurre poco después del año 1000 de la edad decadente, o eso se piensa pues el caos durante esta época ha hecho que los registros de los años sean un poco confusos. En una época difícil donde el hambre, la delincuencia y la corrupción abundan, cosas extrañas empezaron a ocurrir, enfermedades espontaneas, plagas nunca vistas, alimentos que se echaban a perder de un día a otro. Los augures de la Iglesia Blanca determinan que la causa podría ser que la Peregrinación de la Flama, un ritual antiguo y sagrado, tiene siglos que no se lleva a cabo debido a la guerra que terminó recientemente, por lo que deducen que el mal ha comenzado a echar raíces en el mundo. Ayteel Alamoh una Guardiana descendiente del mítico e infame Gabien Alamoh se ha unido a la Peregrinación para intentar enmendar los pecados de su ancestro, pero la gente la rechaza pues se piensa que Gabien fue el principal causante del desastre actual. Es por esto que es forzada a viajar sola por el peligroso Bosque de Jester, las heladas Estepas de Uf’Faraclario, las desoladas Montañas Sinfa y finalmente llegar hasta las Ruinas de Ashemdrek para encender los Altares de Malien y restaurar las Flamas de la Fe, la única esperanza de que el orden vuelva al mundo.

### Gameplay

En Flames of Faith el jugador empieza con un simple objetivo: encender la Flama de la Fe en todas las iglesias y altares de la región. Deberá seguir avanzando la historia para lograr su objetivo, pero encontrará siempre obstáculos: mercenarios, bandidos, animales corrompidos y por supuesto, los misteriosos Drekos y sus torcidas creaciones.
El juego estará dividido en actos, el objetivo de cada acto es avanzar por una serie de caminos bifurcados hasta llegar al jefe final del acto. Los actos estarán compuestos por nodos y los tipos de nodos son estos:
- Combate contra 1-3 oleadas de enemigos. Deberá matar los más enemigos que pueda para progresar. Cada oleada otorga dinero y resta un punto de Gracia (empiezas con 10)
- Tienda: Permite comprar un item o ignorar y continuar.
- Descanso: Restaura +10 puntos de Gracia (a balancear)
- Eventos: Situaciones aleatorias que contienen piezas de lore del mundo. Presentan decisiones al jugador, como por ejemplo encontrar una reliquia maldita, si la agarras te da un beneficio, pero con un efecto negativo o puedes no agarrarla y no arriesgarte.
- Jefe: El nodo final siempre es el jefe y al matarlo se prosigue al siguiente acto.

## 3 – Gameflow

Durante cada nivel el jugador va a encontrar los siguientes objetos:
- Monedas: Todos los enemigos las dan. Permiten comprar objetos como llaves, pociones de curación, pociones etc. También permiten comprar objetos de mejoras permanentes
- Gemas. También las dan todos los enemigos, funcionan como la experiencia y al subir de nivel te proporcionan una mejora permanente.
Mecánica principal del jugador:
Las acciones que el jugador tendrá son únicamente el movimiento, ataque normal(automático) y ataque especial.  El jugador tiene un recurso llamado Gracia que aumenta todo el daño que hace. Cada oleada que limpia se pierde un punto de Gracia. Hay lugares especiales donde se puede descansar y recuperar un poco de Gracia y algunos pickups la van a recuperar tambien
Al inicio de cada partida el jugador elige su arma inicial (espada, arco o cetro)

## 4- Personaje y controles.

Ayteel Alamoh será el único personaje jugable, aunque usará diferentes armas.

**En PC**
Se mueve con WASD y apuntará su ataque hacia donde el mouse esté, siempre mirará hacia el mouse.
Ataque básico en automático, click derecho para ataque especial

**En Control**
Se mueve con el joystick izquierdo
Y para ataque especial.

## 5 – Conceptos principales de Gameplay

Tipo de gameplay: Combate de acción en 2D.
Secuencia de gameplay: Actos divididos en zonas o nodos

### Sistema de Combate – Devoción

**El combate será muy sencillo**, ataques en **automático** al enemigo más cercano. Cada cierto tiempo se puede utilizar un ataque especial que dependerá del arma elegida. El jugador inicia con 10 puntos de Gracia, los cuales otorgan 100% de daño adicional. Cada oleada limpiada restará un punto de Gracia. Los nodos de tipo descanso restauran los puntos de Gracia y algunas mejoras u objetos también.
Espada – Ataca a todos los enemigos alrededor
Arco – Lanza 6-8 flechas en todas las direcciones
Cetro 1 – Ataque primario: misil de fuego que deja un dot. Ataque secundario: Invoca 4 llamas, una arriba, abajo, derecha e izquierda, que van a los enemigos más cercanos y explotan haciendo daño a todos alrededor.
*o*
Cetro 2 – ataque primario: misil mágico que deja un debuff que hace lentos a los enemigos. Ataque secundario: lanza rayos de magia que atraviesan todo hacia los enemigos que tengan el debuff

### Atributos del jugador

Los atributos del jugador son los valores que van a cambiar poder subirse cuando se suba de nivel. Objetos pueden modificar estos atributos y algunos efectos de los enemigos podrían reducirlos
- Vida máxima.
- Armadura (reduce daño restándolo)
- Velocidad de ataque.
- Chance de bloqueo.
- Chance de golpe crítico.
- Potencia de critico
- Daño cuerpo a cuerpo.
- Daño a distancia.
- Daño mágico.
- Puntos de Gracia (aumenta daño en porcentaje)
- Robar vida (porcentaje de chance de curarse al hacer daño)
- Invocación (daño de las unidades invocadas)
- Velocidad de movimiento.
- % experiencia ganada.
- Retribución (daña a los enemigos que te dañan)
- Gracia máxima.
- Pickup range
- Debuff strenght (aumenta el tiempo y efecto de los debuffs)

### Stat especial

Agregar un stat especial o item que mejore la habilidad secundaria de cada arma. Ejemplo, para el arco en lugar de lanzar 6-8 flechas, subir a 8-10.

### Estadísticas del jugador

Las estadísticas son los valores que irán cambiando en el combate y usualmente derivan de un atributo. Ejemplo la vida, deriva de la vida máxima y durante el combate se irá reduciendo con forme se reciba daño.

### Daño elemental

Algunos objetos o armas causarán daño elemental, lo cual dependiendo del daño colocará ciertos debuffs en los enemigos.
Fuego: Daño por segundo, empezando con poco daño y aumentando cada segundo.
Fórmula para daño: 1 + (x * y) + z
Donde x = tiempo, y= stacks, z = Stat fuerza de debuff.
Fórmula para tiempo: 3 + (x * 0.25)
Donde x = stat fuerza de debuff
Escarcha: Hace lentos a los enemigos y aumenta el daño que reciben.
Energía: Hace que cuando el enemigo reciba daño, dañe a un enemigo adyacente y lo afecte con el mismo debuff
Purificado: Chance de aturdir al enemigo cuando recibe daño y curar al jugador
Caos: Chance de causar una explosión que daña a enemigos adyacentes, si estos tienen el debuff de Caos también tendrán 100% de chance de explotar, y si no entonces adquieren el Debuff. Stats aumentan rango y daño de explosiones.

### Mecánica: Corrupción

Esta mecánica esta pensaba para balancear el juego y para introducir decisiones interesantes, así como darle una segunda utilidad al concepto de Gracia.
Cada nivel por el que pase el jugador va a aumentar un poco de puntos de Corrupción. La Corrupción reduce la curación que el jugador recibe y la armadura. Si el jugador pasa por 4 niveles, tendría 4 puntos de Corrupción, por lo que, si recibe 4 puntos de curación, serían negados y su armadura se reduce en -4. Hay algunos nodos especiales que reducen los puntos de Corrupción.
La Gracia tendría además una utilidad secundaria que sería proteger de la Corrupción. Siguiendo el mismo ejemplo, si el jugador adquiere 4 puntos de Corrupción, y tiene 3 puntos de Gracia, contaría como si solo tuviera un punto de Corrupción (4-3=1)

## 6 – Mundo del juego

El juego va contar con 4 diferentes regiones, y los niveles se van a cambiar de forma aleatoria en cada partida. Se formará un camino con algunas bifurcaciones que lleguen hasta un nodo donde estará el altar junto con el jefe de la zona.
Una vez avanzando de un nodo a otro, no se puede retroceder.
Van a existir 4 regiones del juego (4 actos)
Bosque de Jester - Bosque de fantasía regular, solitario y un poco lugubre
Estepa Uf’Faraclario. – Nieve, frio, desolado
Montañas Sinfa – montañoso, desolado, acantilados, rocoso
Ruinas de Ashemdrek – Urbano, destruido, ruinas, oscuro

## 7 – Jefes

### Acto 1

#### Taio’Sandir

### Historia

Su nombre original se desconoce, por lo que se le dio este sobrenombre que en el Lenguaje Olvidado significa “impío”
Es un Dreko antiguo que llegó durante la Edad Oscura, como todos. Desde entonces entró en el cuerpo de un soldado y se quedó allí, por lo que el cuerpo original ha cambiado y mutado por todos esos años (aproximadamente 6mil, sino es que más) quedando irreconocible y la monstruosidad que es hoy en día.
Se cuentan relatos de la Edad oscura que sus gritos infernales solían atraer criaturas corruptas y alimañas, lanzaba poderosos ataques que desquebrajaban armaduras y hasta pequeñas estructuras, pero lo más terrorífico era que a pesar de su gran tamaño, lograba desaparecer entre la neblina que solía aparecer en su presencia, atacando por sorpresa a los soldados que defendían el mundo de estos seres.

### Gameplay

##### Fase 1

El encuentro empieza con el Altar de Malien y el jugador intentando encender la Flama de la Fe. Deberá sobrevivir hasta que el fuego se encienda por completo, entonces pasará a la segunda fase.
Durante la primera fase, algunos enemigos aparecerán e intentarán detener al jugador, como si de una oleada normal se tratase. Salen enemigos cada 5 segundos
Ataque: Rayo de Oscuridad. De vez en cuando, cada 5-8 segundos, el jefe aparece desde la nada (como emergiendo de la niebla) y lanza un proyectil morado hacia el jugador, que después de 3 segundos se rompe en 8 ataques pequeños hacia todas las direcciones.
Ataque: Invocar Ojos Dreko. Cada 30 segundos, el jefe aparece de nuevo e invoca un grupo de 4 enemigos de tipo Ojo Dreko, pero en lugar de atacar al jugador, intentarán ir al Altar y apagar el juego sagrado. Al contacto con el altar, el progreso de la flama de la fe se reduce un 5% (20% si los cuatro llegan)
Mientras los ojos estan yendo al altar, el jefe empieza Rayos de Oscuridad cada 3 segundos.
Al morir los ojos, el jefe se enoja y desaparece. La frecuencia con la que los enemigos aumenta en 1 segundo y ahora cada vez que aparezca el jefe a lanzar un proyectil, lanzará uno adicional. La siguiente vez que haga Invocar Ojos Dreko invocará uno adicional.
La fase se repetirá, cada vez que los ojos mueran todo aumenta en rapidez hasta que el jugador muera o el altar se encienda por completo.
Una vez se encienda, empieza la fase dos.

##### Fase 2

Durante la fase dos el jefe aparece y los enemigos dejan de aparecer. El altar se ha llenado y el jefe ahora recibe más daño que antes (200% más) y se moverá de izquierda a derecha.
Se detendrá cada 2 segundos a lanzar un Rayo de Oscuridad.
Ataque: Ráfaga Corrupta. Cada 10 segundos se detendrá para hacer una habilidad que lanza una ráfaga de pequeños proyectiles hacia el jugador, los lanza en direcciones aleatorias, pero siempre en un arco hacia donde esté el jugador. Esta habilidad dura 5 segundos.
Ataque: Zángano del abismo. Cada 30 segundos se detiene y lanza esta habilidad, creando un proyectil que lentamente sigue al jugador y dura hasta que el jefe muera.
Los ataques se repiten hasta que el jefe llegue a 0 de vida y muera.

#### Animaciones

Idle
Walk
Attack1: para Rayo de Oscuridad y Zangano del abismo. Deberá hacer un brazo hacia atrás y luego agresivamente hacia adelante, como lanzando una bola de beisbol.
Attack2: Para Invocar Ojos Dreko y Ráfaga Corrupta. El jefe mira hacia en frente (hacia la camara) y dobla sus brazos, creando una mancha brillosa morada en cada puño.
Die.

#### Animaciones opcionales

Damage: Para la transición entre fase 1 y 2, la idea es que la Flama de la Fe daña al jefe y hace que el daño recibido aumente.
StealthOff: Para la primera fase, cuando el jefe aparece de pronto para atacar al jugador, podría ser una animación de una nube de neblina que de pronto el jefe se haga visible, como saliendo de la neblina.
StealthOn: Para la primera fase, una vez el jefe aparece y ataca al jugador, la neblina regresa y el jefe poco a poco se mueve hacia ella, despareciendo.

##### Phase 1

The encounter begins at the Altar of Malien, with the player attempting to ignite the Flame of Faith. They must survive until the flame is fully lit, at which point the second phase will begin.
During the first phase, several enemies will appear and attempt to stop the player, similar to a normal wave of enemies. Enemies spawn every 5 seconds at first.
Attack: Volt of Darkness. Occasionally, every 5-8 seconds, the boss appears out of nowhere (as if emerging from the fog) and launches a purple projectile at the player. After 3 seconds, this projectile breaks into 8 smaller attacks in all directions.
Attack: Summon Dreko Eyes. Every 30 seconds, the boss reappears and summons a group of 4 Dreko Eye enemies. Instead of attacking the player, these enemies will attempt to reach the Altar and extinguish the sacred game. Upon contact with the altar, the Flame of Faith's progress is reduced by 5% (20% if all four eyes reach it). While the eyes are moving to the altar, the boss casts Volt of Darkness every 3 seconds.
When the eyes die, the boss becomes enraged and disappears. The enemy spawn rate increases by 1 second, and now, each time the boss appears to launch a projectile, it will launch an additional one. The next time it uses Summon Eyes, Dreko will summon an additional eye.
This phase will repeat; each time the eyes die, everything speeds up until the player dies or the altar is fully lit.
Once lit, phase two begins.

##### Phase 2

During phase two, the boss appears, and enemies stop spawning. The altar is full, and the boss now takes 200% more damage and moves from left to right. It will stop every 2 seconds to cast a Volt of Darkness.
Attack: Corrupted Barrage. Every 10 seconds, it will stop to unleash an ability that fires a barrage of small projectiles at the player, launching them in random directions, but always in an arc toward the player's position. This ability lasts 5 seconds.
Attack: Abyssal Drone. Every 30 seconds, it will stop and unleash this ability, creating a projectile that slowly tracks the player and lasts until the boss dies.
The attacks repeat until the boss reaches 0 HP and dies.
