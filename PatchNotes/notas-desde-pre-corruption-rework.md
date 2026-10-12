# 🔥 Flames of Faith — Notas del parche

¡Una actualización enorme! Rediseñamos por completo la **Gracia y la Corrupción**, añadimos **guardado de partida**, **fijación de objetivo**, una nueva estadística (**Evasion**) y muchísimos ajustes basados en vuestros comentarios.

---

## ✨ Contenido nuevo

### Gracia y Corrupción (rediseño completo)
- **La Gracia ahora va de −50 a +50.** Con Gracia positiva haces más daño. Si baja de 0 quedas **Corrompido**: pierdes Armor y recibes menos curación, y es más probable que aparezcan enemigos Corrompidos.
- **La Corrupción se acumula durante cada oleada:** al recibir daño de fuentes Corrompidas y según cómo te vaya contra el Corruptor. Ya no se arrastra entre niveles.
- **Al final de cada oleada:** tu nueva Gracia = Gracia + **Grace Affinity** − Corrupción de la oleada. Un resumen animado te muestra el cálculo; pulsa para continuar.
- **El Corruptor:** al acabar el tiempo de cada oleada aparece un enemigo especial que huye. La cámara te lo enseña, una flecha en el borde de la pantalla te indica dónde está y una barra te muestra el tiempo que queda.
  - Cuanto antes lo mates, menos Corrupción te deja.
  - Si no lo matas a tiempo, escapa por un portal y te deja la máxima.
- **Enemigos Corrompidos:** hacen más daño y tienen más vida. Su daño cuenta como daño Corrompido.
- **Taint Level:** cada nodo del mapa muestra su nivel de corrupción (con tooltip). Cuanto más alto, más enemigos Corrompidos.
- **El jefe también está Corrompido** y sus habilidades golpean más fuerte.
- **Grace Affinity** (antes Max Grace): te da Gracia al final de cada oleada y aumenta la probabilidad de encontrar orbes de Gracia.
- Los eventos **Cursed Chest** y **Holy Altar** ahora cambian tu Gracia directamente.
- Nuevos tooltips en las etiquetas de Gracia y Corrupción. Con mando se seleccionan desde el menú de pausa.

### Guardar y continuar partida
- **La partida se guarda sola** en el mapa: al llegar y en cada movimiento.
- **Load Game** en el menú principal para continuar. **New Game** te avisa antes de borrar tu partida.
- **Save & Exit** en el menú de pausa (avisa de que se pierde el progreso del nivel actual). Salir desde el mapa también guarda.
- Si sales en mitad de un nivel, vuelves al mapa tal como estaba antes de entrar.

### Nueva estadística: Evasion
- **Probabilidad de esquivar por completo un golpe.**
- 0.5% por punto hasta 50 puntos, después 0.1% por punto, hasta un máximo del 30%.
- Al esquivar aparece **"Dodge"**, tienes un instante de invulnerabilidad y el golpe no genera Corrupción.
- Disponible al subir de nivel y en objetos.

### Fijar objetivo (lock-on)
- **F** (teclado) o **LT** (mando) fija al enemigo actual o al más cercano. Un triángulo sobre su cabeza indica que está fijado.
- Por defecto funciona como interruptor: pulsa para fijar y otra vez para soltar. En **Settings** puedes cambiarlo a "mantener pulsado".
- Mientras está fijado, tus ataques y tus especiales van a por ese enemigo. Perfecto para cazar al Corruptor.

### Especiales mejorados
- **Archer:** además de la ráfaga circular, el especial dispara **6 flechas teledirigidas** al objetivo (50% del daño del especial).
- **Augur:** además de los tornados, el especial provoca **explosiones sobre el objetivo** cada 2.5 s mientras dura.

### Objetos iniciales con bonificaciones únicas
Cada objeto inicial da a su clase una identidad propia. Los tooltips de los objetos lo indican.
- **Steeldawn Sword:** por cada punto de Retribution, +1 Health y +1 Armor, pero −1 Move Speed.
- **Steeldawn Bow:** por cada punto de Evasion, +1 Evasion y +1 Pierce. Por cada 5 de Evasion, −1 Health Regen.
- **Akeros Graceful Scepter:** por cada punto de Mastow Affinity, +1 Skill Duration y +1 Dash Recovery, pero −1 Armor.

Estos objetos no aparecen en tiendas, cofres ni eventos.

### Pierce rediseñado
- Ahora se llama **Pierce**. Cada 4 puntos tu proyectil atraviesa a un enemigo más.
- **Nuevo:** cada punto da un 3% de probabilidad (máx. 40%) de que las flechas del Archer y las de **Scroll of Strafe** **se partan** al impactar. Sale una flecha extra con la mitad de daño en dirección aleatoria.

### Descripciones de estadísticas vivas
- Al pasar el ratón por una estadística ves **lo que te da ahora mismo**: "Take 28.6% less damage", "Heal 1 HP every 2.65s", "Basic attack every 0.86s"…
- Mantén **Shift** para ver qué objetos te la están dando.
- Al subir de nivel, cada opción muestra **antes → después**.
- Todas las descripciones se han reescrito. Algunas eran incorrectas, como Life Steal, Luck, Range o Cleave.
- **Experience Reduction** ahora se llama **Experience Gain**, porque hace que los enemigos suelten más experiencia.

### Experiencia
- Las gemas de experiencia se ajustan a tu nivel, así que subes aproximadamente un nivel por oleada. Los enemigos más fuertes pueden soltar gemas más grandes.
- **Al terminar la última oleada de un nivel, todas las gemas que queden vuelan hacia ti** y se recogen automáticamente.

### Nuevo objeto
- **Necrotic Potion** (Rare): +3 Life Steal, −5 Health, −2 Health Regen.

---

## ⚔️ Balance y ajustes

### Daño de los enemigos
- **Rediseñado por completo:** el principio de la partida se siente parecido, pero a mitad y al final los enemigos ya no se disparan. Invertir en Health y Armor por encima de lo normal ahora se nota de verdad.
- El daño ya no va a saltos de 1 en 1. Puede ser fraccionario (los números en pantalla se redondean), así que cada enemigo pega con la fuerza que le corresponde.
- **El Armor negativo ahora aumenta el daño recibido:** −5 = +17%, −25 = +50%.

### Clases
- **Archer:**
  - Sus flechas básicas **corrigen el rumbo** suavemente hacia el objetivo y apuntan a donde está el enemigo al disparar.
  - Sus **trampas ya no caducan**; puedes tener hasta 3 y al poner una cuarta desaparece la más antigua.
- **Augur:**
  - El especial hace un **25% menos de daño**.
  - Los tornados son más lentos pero giran mejor.
  - Vuelve a poder **dirigir su dash** mientras dashea.
- **Armas:** el daño base de las armas casi se duplica (Bow 8 → 15, Scepter 6 → 12, especiales también).
- Atacar ya no está bloqueado durante todo el enfriamiento del especial, solo mientras se reproduce su animación.

### Enemigos
- **DrekoEye:** un 30% menos de vida y dispara más despacio.
- Menos daño y vida en los enemigos en general, menos oleadas por nivel y más monedas por oleada.
- El Corruptor da más margen: 30 s en total y 15 s para la "muerte rápida".

### Objetos
- Revisión de precios y calidades: varios objetos suben de precio y algunos ganan pequeñas desventajas.
- **Grace Affinity** reducida en la mayoría de objetos y eliminada de los objetos iniciales.
- Menos daño en el fuego de **Scroll of Swords of Fire**.
- El bonus de daño por Gracia es menor.

---

## 🎮 Mandos y controles
- **Ataque automático activable** con **Ctrl** / **RT**, o desde Settings.
- Con el ataque automático apagado, Archer y Augur disparan hacia donde apuntas (ratón o stick derecho), con una pequeña ayuda de puntería.
- **F** / **LT** para fijar objetivo (ver arriba).
- Muévete entre nodos del mapa con **WASD** o las flechas.
- Muchos arreglos de navegación con mando: menús, tienda, pantalla de muerte, mapa y pestañas de estadísticas.
- La ventana de **Settings** está reorganizada en secciones.

---

## 🔧 Otras correcciones y mejoras
- **Confirmación al salir de la tienda**, porque no se puede volver a ella.
- **Arreglado:** las explosiones del Augur duraban 3 segundos o más y seguían haciendo daño. Ahora duran medio segundo.
- **Arreglado:** con mucha Attack Speed o Dash Recovery negativa, los ataques y el dash se volvían casi instantáneos. Ahora solo te ralentizan.
- **Arreglado:** el daño de los eventos ya no puede matarte (te deja como mínimo con 1 de vida).
- **Arreglado:** la animación de caminar seguía reproduciéndose al final de la oleada.
- **Arreglado:** se podía dashear y moverse con teclado dentro de la tienda.
- **Arreglado:** la regeneración de vida seguía activa durante el resumen de fin de oleada.

---

¡Gracias por seguir probando! Contadnos qué os parecen los cambios, sobre todo el nuevo sistema de Gracia y el daño de los enemigos. 🙏
