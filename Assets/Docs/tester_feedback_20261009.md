# Priority 1
- Add new stat Evasion. Add a small chance to avoid 100% of the incoming damage. 
  - Notes: Chance should be 0.5% chance per point up to 50, then it will increase only 0.1% per point up to 30%. This should be tuneable
- At the end of the level, on the last wave automatically collect all experience gems as a reward
- Add a confirmation dialog before leaving the store
  - Notes: Reuse the DialogWindowUI with the message "Are you sure you want to leave? You cannot go back to this shop"
- Lower damage of Augur class
- Traps spawned by the archer dash should not go away after sometime. Instead have a fixed limit of a number of traps, say 3 and if the player tries to set another one, remove the oldest.
- Archer has a hard time hitting enemies with the basic attack. MAke a change to the projectiles to correct course 
  - Notes: Linear projectiles could have this capability, every x amount of time the arrow could recalculate the direction to make it easier to hit targets. Although I am afraid that would look a bit unnatural.
- Reduce health and attack speed of the enemy DrekoEye

# Priority 2
- In order to improve balance and differentiate classes more, add stats dependency enhacers (SDE)
  - These SDEs will help classes to gain an identity
  - For example for the Warrior class, for each retribution point gained you will gain +1 armor and +1 health
  - Have these SDEs as effect behaviors, Currently effects/items have a behavior that is used for skill items, we could add another behavior type to define these rules. Or if it is to complex to add them as effect behaviors, we could create another component StatDependencyEnhacer and add it to the Effect Scriptable object
  - We want this approach to be very data-driven
  - These SDEs will be added to the starting items only. Will not be able to get them in the store, treasures or drops
  - So far I have these planned:
    - Warrior class: for each retribution point gained you will gain +1 armor and +1 health but -1 Move Speed
    - Archer class: for each evasion point you will get an additional +1 evasion and +1 prierce amount and -1 attack speed
    - Augur class: for each Mastow affinity point get and additional +1 skill duration and +1 dash cooldown reduction but -1 armor
  - Each effect can have more than one of these SDE rules
  - The SDE rules will be linked to the item not the class
- Re-balance pierce amount stat. After a points, benefits become almost useless. 
  - First rename the stat to Pierce only
  - Each point of Pierce will gain 0.25 of amount of enemies pierces, meaning only after having 4 points will add an additional pierce target
  - This change will make this stat weaker, so to compensate add a chance to an additional projecilte (of the same type) to spawn and move into a random direction, as if the projectile is breaking from the first one. Damage of the second projectile will be halved. Only applies to linear projectiles, so that is the arrows of the archer and arrows from the strafe skill item
  - The Augur shoots a linear projectile too when auto attack is off, but this will not be considered for any effects of the Pierce stat
- Dynamic stat description. Some stats formulas are a bit obfuscated. Add a way to calculate the benefits when hovering the stat on the statsPane and show the actual benefits. Example, for the armor show the exact % reduction and maybe the hardcap

# Priority 3
- Refactor warrior abilities/bonuses into data-driven architecture with upgradable attributes. 
  - Note: This is for later on, but it is good to know the changes ahead so we don't implement something incompatible
  - Deliberately I removed from the plan the refactor for the classes because I have something bigger planned.
  - We will move ALL classes abilities into data-driven behavior effects AND different items.
  - Example: Currently the warrior has its starting item a sword with some stat modifiers and deals damage which has chance to cause bleed, for the special attack it has a whirlwind ability and for dash it deals a small amount of damage and knockbacks enemies. Instead of that we will break into items: Sword with the stat modifiers, new item that enables bleed damage, new item that enables whirlwind and a new item that enables the warrior dash.
  - In theory if we leave it like that it will be the same, but the idea is to add higher tier items of the same time, for example a +2 sword that has better starting stats and more base damage, or the +2 whirlwind item will cause additional damage or even have additional effects or stats if we want.
  - This big change will be the first step to introduce meta-progression into the game. The upgrades for the starting items will be aquired in a new shop that will accept a new currency gained only after finishing a run
  - More details will be to come, we will focus on other changes for now.