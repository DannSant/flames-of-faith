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
- Refactor class abilities/bonuses into data-driven architecture with upgradable attributes. 
  - Note: This is for later on, but it is good to know the changes ahead so we don't implement something incompatible
  - Deliberately I removed from the plan the refactor for the classes because I have something bigger planned.
  - We will move ALL classes abilities into data-driven behavior effects AND different items.
  - Example: Currently the warrior has its starting item a sword with some stat modifiers and deals damage which has chance to cause bleed, for the special attack it has a whirlwind ability and for dash it deals a small amount of damage and knockbacks enemies. Instead of that we will break into items: Sword with the stat modifiers, new item that enables bleed damage, new item that enables whirlwind and a new item that enables the warrior dash.
  - In theory if we leave it like that it will be the same, but the idea is to add higher tier items of the same time, for example a +2 sword that has better starting stats and more base damage, or the +2 whirlwind item will cause additional damage or even have additional effects or stats if we want.
  - This big change will be the first step to introduce meta-progression into the game. The upgrades for the starting items will be aquired in a new shop that will accept a new currency gained only after finishing a run


## More details on the Refactor class abilities/bonuses

For this important refactor, we want to have a Skil Tree-like for each class.
The loop will be:
- After a run, depending on how levels you have beaten and other goals we will set later, you will gain a new currency: Favors
- Favors will be used in the MainMenu to unlock slots of this skill tree
- After a slot is unlocked, we will only swap it with the higer tier version of the item
  - Example Steeldawn Sword  in the new system will give +5 melee damage and +2 Cleave, base damage will be 10. After upgrading this slot we will replace it with Steeldawn Sword +1 which will have +6 Melee damage +3 Cleave and base damage 11 and so on...
- We will evaluate if the favors for a class can be used to unlock slots for a different class. So if possible let's implement both options and a setting to toggle (developer setting, not user setting)
- From the top of the skill tree, the parent node will be the name of the class and this will be unlockable too, except for the Warrior which is unlocked by default. After unlocking the parent node, the class becomes available to play and the skill tree can be used.
- Additional starting items. The items described below are the ones I have planned, but we will not discard that the Skill Tree aallows the player to unlock new starting items with new effects


### Warrior
See the new items for this class
  - Steeldawn Sword    
     - Melee damage +5, upgradable.
     - Cleave +2, upgradable.
  - Shield of the Crusader.
    - MaxHealth +10 upgradable
    - Armor + 1 upgradable
    - Move speed -2 upgradable (each point will push it towards 0)
  - Warrior Emblem
    - All stat bonuses will be moved into this item
    - This includes the +1 Melee Damage the warrior gains when leveling up
  - Scroll of Whirlwind (Skill item) - Current special ability of the Warrior will be moved into this item
    - Base damage 20, will be upgradable.
    - Upon using it, a holy shield will be created for a few seconds. While it is up, enemy projectiles are absorbed: they deal no damage and heal the player for a fraction of the damage they would have dealt (through the normal healing path, so Healing Received and the Corrupted penalty apply). Melee and contact hits still land. The base scroll and +1 don't create the shield; from +2 it lasts 1 second, and its duration is upgradable. (Changed from reflecting projectiles: absorbing fits the tanky Warrior better and is far less risky to build.)
  - Scroll of Crusader strike (Skill item) - Current basic attack of the Warrior will be moved into this item.
    - Base damage 10, upgradable
    - Have a chance to inflict bleed. Chance will start at the base bleed debuff chance (25%) and this ability will add additional percentage per upgrade
  - Scroll of Crusader Charge (Skill item) -  Current WarriorDash ability will be moved into this item
    - Base damage is 3. upgradable
    - Knockback force will be upgradable too

### Archer
See the new items for this class
  - Steeldawn Bow     
     - Range damage +5, upgradable.
     - Attack speed +2, upgradable.
  - Quiver of the Centinel.
    - Range + 1 upgradable
    - MaxHealth -5 upgradable (each point will push it towards 0)
  - Archer Emblem
    - All stat bonuses will be moved into this item
    - The effect that increased RangeDamage on Level up should be moved here too
  - Scroll of Barrage (Skill item) - Current special ability of the Archer will be moved into this item
    - Base damage 30, will be upgradable.
    - Same effect as of now, the difference will be that at the beginning less arrows will be shot and upgrades will increase the number of arrows
  - Scroll of Holy Shot (Skill item) - Current basic attack of the Archer will be moved into this item.
    - Base damage 15, upgradable
    - One shot at the start, after upgrading it each attack will add an additional shot, but they will form a cone around the main shot.
  - Scroll of Trap Step (Skill item) -  Current ArcherDash ability will be moved into this item
    - Duration of the dash can be upgraded too
    - Base damage of the traps will be upgradable
    - Max number of traps will be upgradable

### Augur
See the new items for this class
  - Akeros Graceful Scepter    
     - Magic damage +5, upgradable.
     - Skill duration +1, upgradable.
  - Ley Shield.
    - MAstow Affinity + 2 upgradable
    - Armor -3 upgradable (each point will push it towards 0)
  - Augur Emblem
    - All stat bonuses will be moved into this item
    - The effect that increased MAgicDamage on Level up should be moved here too
  - Scroll of Fire Tornadoes (Skill item) - Current special ability of the Augur will be moved into this item
    - Base damage 30, will be upgradable.
    - Same effect as of now, the difference will be that at the beginning less tornadoes will be spawned and upgrades will increase the number of tornadoes as well as their damage
  - Scroll of Fireball (Skill item) - Current basic attack of the Augur will be moved into this item.
    - Base damage 12, upgradable
    - At the start it will work as it is now, upon upgrading it on impact the projectile will spawn the same explotion prefab as the tornadoes with more damage every upgrade
  - Scroll of Flame Dash (Skill item) -  Current Augur ability will be moved into this item
    - Duration of the dash can be upgraded too
    - Base damage of the flames will be upgradable
    - Max number of charges will be upgradable, starting at 1 up to 3
    - The interval of the spawning of the flames can be upgrades too, the more upgrades the more flames the dash will spawn
  
   



