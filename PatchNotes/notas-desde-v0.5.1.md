# 🔥 Flames of Faith — Notas del parche v0.5.2

¡Los **eventos del mapa son ahora pequeñas historias**! Hay diez eventos reescritos, con ilustraciones, decisiones que dependen de tus stats y líneas propias para cada personaje.

> ✅ Las partidas guardadas con la v0.5.1 se pueden continuar.

---

## ✨ Contenido nuevo

### Eventos rediseñados
- **Pistas en lugar de números:** las opciones ya no te dicen qué vas a ganar o perder. Ahora te dan una pista dentro de la historia, así que toca leer y decidir.
- **Resultados narrados:** después de elegir, el texto cuenta lo que ha pasado, y al final una línea resume lo que has ganado y perdido.
- **Pruebas de stats:** algunas opciones dependen de tus stats y ya no solo de **Luck**. Una pelea puede depender de tu stat principal o de **Armor**, y una huida de **Move Speed**. Algunas también miran tu **Grace** actual.
- **Eventos con varios pasos:** una respuesta puede llevar a otra pregunta. Hay que conocer la historia del mundo: responder bien tiene premio y responder mal siempre cuesta algo.
- **Cada personaje vive el evento a su manera:** Ayteel, Faelin y Werlem tienen frases propias y algunas opciones exclusivas.
- **Cuatro eventos nuevos:** **A Hut in the Jesterwoods**, **Laughter in the Leaves**, **The Star Reader** y **The Last Torch**. Se suman a los cinco de antes, reescritos, y al **Wayside Shrine of Malien**.
- **Sin repeticiones:** un evento no se repite en la misma Pilgrimage hasta que hayas visto todos.
- **Ilustraciones:** casi todos los eventos tienen ahora su imagen junto a la ventana.

### Otras novedades
- **Aviso de ataque cuerpo a cuerpo:** antes de golpear, el **Light Devourer** marca en el suelo la zona del golpe con un círculo rojo que se va llenando. El ataque va más lento y mantiene su dirección, así que puedes salir de la zona a tiempo.
- **Corruptor:** tiene sprites y animaciones propios, y se detiene para disparar.
- **Favors a la vista:** aparecen junto a las monedas en el Inventory y en el mapa, por ejemplo «42 (+5)»: los que tienes guardados y, entre paréntesis, los ganados en esta Pilgrimage. Los dos tienen tooltip.
- **Textos de la historia:** muchas descripciones de objetos y nombres se han corregido para que encajen con la historia del mundo. Ahora verás **Plerian Shop**, **Wayside Shrine of Malien**, **Jesterwood Cliffs**, **Quiver of the Sentinel** y **Sacred Writings**.

---

## ⚔️ Balance y ajustes

- **Attack Speed:** rinde mucho menos por punto, porque antes subía demasiado rápido. Además tiene límites: a partir de 40 puntos cada punto vale la mitad, y a partir de 80 ya no suma. Como máximo atacas 2,2 veces más rápido. El tooltip te avisa cuando llegas a esos límites.
- **Glyph of Whirlwind:** ahora es un giro de **2 segundos** que golpea al contacto y luego cada 0,7 s (hasta cada 0,4 s en +3), con daño base 10 / 13 / 15 / 20. **Ya no eres invulnerable mientras giras**, y un dash lo interrumpe. El escudo de las mejoras dura 3 / 3,5 / 4 s.
- **Tiempos de recarga:** Sword 1,5 s, Whirlwind 10 s, Scepter 2 s, Fire Tornadoes 8 s, Barrage 5 s.
- **Glyph of Fireball:** daño 14 / 16 / 18 / 22.
- **Frost:** antes duplicaba cada golpe por error. Ahora añade el daño extra que le corresponde, así que los objetos de Frost pegan menos que antes.
- **Objetos repetidos:** las copias extra de **Swords of Fire** ahora aumentan su daño, y **Consecrate** y **Holy Missiles** también mejoran con cada copia.
- **Objetos aleatorios:** todos salen ahora de una sola tabla. **Luck** también mejora los objetos que sueltan los enemigos y los cofres. Si un cofre no puede dar ningún objeto, da monedas.
- **Max Health:** al subirla, ganas exactamente esos puntos de vida. Antes se aplicaba un porcentaje que redondeaba a la baja.
- **Vida de los enemigos** y **Corruptor:** valores ajustados.

---

## 🎮 Mandos y controles

- **Lock-on mejorado:** pulsar F / LT fija al enemigo más cercano. Si vuelves a pulsar, pasas al siguiente en sentido horario. Si ya no hay enemigos cerca, se suelta.
- **Ratón:** haz clic en un enemigo para fijarlo y atacarlo.
- **Selección de personaje con mando:** moverte por las tarjetas ya solo muestra la información. Eliges con A. Antes, al bajar hasta Start Run podías acabar eligiendo a otro personaje sin querer.
- **Vista con Shift:** los objetos que dan la stat se marcan con un anillo amarillo y el resto se oscurece mucho más.
- **Objetos encontrados:** el tooltip funciona como en el Inventory y solo aparece mientras señalas un objeto.

---

## 💀 Jefes

- **Taio'Sandir** ya no se vuelve transparente durante la primera fase. Es provisional: el equipo de arte prepara otra forma de mostrar que es invulnerable.
- **IMMUNE:** al golpear a un enemigo invulnerable verás **IMMUNE** en lugar de un 0, y el daño por quemadura, Bleed o Ley Charge ya no muestra números sobre él.

---

## 🔧 Otras correcciones

- **Arreglado:** con mucho **Attack Speed**, los ataques básicos dejaban de impactar. La animación ahora se acelera con la velocidad de ataque.
- **Arreglado:** la tienda se quedaba en la pantalla de carga con Luck en -2 o -3. Con Luck negativa los mejores objetos salen menos, y siempre hay al menos 3 objetos.
- **Arreglado:** **Fire Arrows** había dejado de quemar a los enemigos.
- **Arreglado:** Esc abría el menú de pausa encima de la ventana de objetos, la de subida de nivel y Wave Results, y el juego se comportaba mal. Ahora no hace nada en esas ventanas.
- **Arreglado:** el tiempo de recarga de Whirlwind apenas bajaba con Attack Speed.
- **Arreglado:** el tooltip de una stat podía quedarse en pantalla al cerrar el panel de Stats.
- **Arreglado:** aparecía un icono de moneda suelto en la ventana de subida de nivel, después de la de objetos.

---

¡Gracias por seguir probando el juego! Contadnos sobre todo qué os parecen los eventos nuevos: si las pistas se entienden, si los textos se hacen largos y qué decisiones os han gustado más. 🙏
