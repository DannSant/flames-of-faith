# Corruption and Grace mechanics rework

## Context

Currently Corruption and Grace mechanic interaction is not very clear, and testers mention they don't understand when you gain corruption or grace.

## Current behavior

- Right now, every time you beat a level you gain +1 corruption
- Some run encounters/events grant or cleanse corruption
- When you clear a wave you lose -1 grace
- There are some pickups that grant Grace
- Retribution stat has a chance to grant Grace
- Holy debuff will have a chance to grant Grace

* The interaction 
- Grace grants +10% additional damage per point
- If Corruption is greater than Grace, you become Corrupted.
- Corrupted removes the damage bonus from Grace and aditionally you lose armor (-1 per difference) and reduces healing received to 0

## New behavior

- The interaction between Grace/Corruption will stay the same, we probably only need to tweak the numbers
- Corruption will become a temporal value, every wave starts with 0 and depending on the outcome it will increase
- Now Corruption will be used as a chance to spawn a new type of enemy: Corrupted
- Example at the beggining, 0 Corruption, 0% chance these enemies spawn
- Corrupted enemies are same as normal enemies with a visual effect and deal +50% damage
- At the end of every wave a special Corrupted enemy will spawn. This enemy will be of a new type called Corruptor and will shot projectiles and run away from the player
- The player has 15 seconds to kill the Corruptor. if it takes 5 seconds or less to kill it you will gain +1 Corruption. If it takes more than 5 seconds you will gain an additional +1 corruption for each 2 seconds passed. Up to +5 per wave.
- Spawned Corrupted enemies will add Corrupted Damage to the player. At the end of the wave the player will gain +1 corruption per 10 Corrupted Damage, up to +5
- At the end of the wave display on the UI clearly how much corruption you gained. Show clearly the subtraction between the Grace - Corruption and the end value will be the new Grace, it can be positive or negative. Corruption becomes 0 again
- Remove the max Grace stat. Grace will have hardcoded limits now +50 and -50
- Change the Grace +10% damage bonus, now it will be +10% damage Grace that does not offset Corruption. Example Corruption 10, Grace 15 currently you gain +150% damage. In the new system you substract 15 - 10, then you gain +50% damage. If the susbtraction is negative, you will lose that amount of damage. 
- Change Corruption/Grace bars. It will become 1 bar Grace bar and it will allow negative values. 



