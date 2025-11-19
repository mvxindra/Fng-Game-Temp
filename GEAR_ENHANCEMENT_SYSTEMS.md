# Gear Enhancement & Crafting Systems

Comprehensive gear depth systems with rarity tiers, random affixes, socket/rune system, and advanced crafting.

## Table of Contents

1. [Enhanced Rarity System](#enhanced-rarity-system)
2. [Affix System](#affix-system)
3. [Socket & Rune System](#socket--rune-system)
4. [Enhanced Crafting System](#enhanced-crafting-system)
5. [Configuration](#configuration)
6. [Usage Examples](#usage-examples)

---

## Enhanced Rarity System

### Rarity Tiers

The system extends beyond the basic 3-4-5 star system with 7 distinct rarity tiers:

| Tier | Name | Color | Drop Weight | Features |
|------|------|-------|-------------|----------|
| 1 | Common | White | 50% | 0-1 affixes, 0-1 sockets |
| 2 | Uncommon | Green | 30% | 1-2 affixes, 0-2 sockets |
| 3 | Rare | Blue | 15% | 2-3 affixes, 1-2 sockets, Can have set bonus |
| 4 | Epic | Purple | 4% | 3-4 affixes, 1-3 sockets, Can have set bonus |
| 5 | Legendary | Orange | 0.9% | 4-5 affixes, 2-4 sockets, Can have set bonus |
| 6 | Unique | Gold | 0.09% | 5-6 affixes, 2-4 sockets, Unique passive, Named items |
| 7 | Mythic | Red | 0.01% | 6-8 affixes, 3-6 sockets, Unique passive, Extremely rare |

### Unique Gear

Named unique items with special properties:

```csharp
// Example: Frostmourne
{
    "uniqueName": "Frostmourne",
    "loreText": "The cursed runeblade of the Lich King",
    "requiredElement": "ice",
    "guaranteedAffixes": ["ice_damage", "life_steal"],
    "uniquePassive": "Soul Reaper" // Restore 20% HP on kill
}
```

Features:
- **Named Items**: Unique names with lore
- **Guaranteed Affixes**: Specific affixes always present
- **Unique Passives**: Special abilities only available on these items
- **Requirements**: Level, class, or element requirements
- **Soulbound Options**: Can be bound to specific heroes

### Named Gear Sets

Complete armor sets with progressive bonuses:

**Example: Dragon Slayer's Regalia**
- **2-Piece**: +25% Fire Resistance, +15% All Stats
- **4-Piece**: +50% damage to Dragons, Fire immunity
- **6-Piece**: Transform into dragon at low HP

Set bonuses stack and provide powerful synergies.

---

## Affix System

Random substats that roll when gear drops, similar to Diablo/Path of Exile.

### Affix Types

- **Prefix**: Appears before item name (e.g., "Deadly Sword")
- **Suffix**: Appears after item name (e.g., "Sword of the Titan")

### Affix Categories

1. **Offensive**: ATK, CRIT, Damage multipliers
2. **Defensive**: HP, DEF, Damage reduction, Resistances
3. **Utility**: SPD, CDR, Gold/EXP gain, Drop rate
4. **Elemental**: Elemental damage and resistances

### Affix Properties

Each affix has:
- **Tier** (1-5): Determines power level
- **Roll Quality** (0.7-1.3): Random multiplier on final values
- **Slot Restrictions**: Which gear slots it can appear on
- **Rarity Requirements**: Minimum rarity tier needed

### Example Affixes

```json
{
    "affixName": "of the Titan",
    "affixType": "suffix",
    "affixCategory": "offensive",
    "statMod": {
        "flatAtk": 50,
        "percentAtk": 0.15
    },
    "affixTier": 3
}
```

```json
{
    "affixName": "Vampiric",
    "affixType": "prefix",
    "statMod": {
        "lifeSteal": 0.10,
        "flatAtk": 20
    }
}
```

### Affix Generation

- Number of affixes determined by rarity tier
- Weighted random selection from affix pools
- Each affix rolls quality (70-130% of base value)
- Item level scales affix power

### Rerolling Affixes

Players can reroll affixes using materials:
- Clears existing affixes
- Generates new random affixes
- Cost scales with reroll count
- Can be done at Enchanter facility

---

## Socket & Rune System

Inspired by Diablo's rune system - socket gems into gear for bonuses.

### Socket Types

- **Red**: Offensive stats (ATK, CRIT)
- **Blue**: Defensive stats (HP, DEF)
- **Green**: Utility stats (SPD, Evasion)
- **Yellow**: Elemental damage
- **Prismatic**: Accepts any rune

### Rune Tiers

Runes come in 5 tiers that can be upgraded:

1. **Chipped** (Tier 1): Basic bonuses
2. **Regular** (Tier 2): Improved bonuses
3. **Flawless** (Tier 3): Strong bonuses
4. **Perfect** (Tier 4): Very strong bonuses
5. **Radiant** (Tier 5): Maximum bonuses + special effects

### Rune Families

- **Might** (Red): ATK, CRIT chance/damage
- **Vitality** (Blue): HP, DEF
- **Speed** (Green): SPD, Evasion
- **Elemental** (Yellow): Elemental damage/resistance
- **Omnipotent** (Diamond): All stats

### Rune Upgrading

Combine 3 runes of same tier to attempt upgrade:
- Ruby Tier 1 x3 → Ruby Tier 2 (80% success)
- Ruby Tier 2 x3 → Ruby Tier 3 (70% success)
- Ruby Tier 3 x3 → Ruby Tier 4 (60% success)
- Ruby Tier 4 x3 → Ruby Tier 5 (40% success)

### Legendary Runes

Ultra-rare runes with unique abilities:

**Phoenix Heart**
- Revive once per battle with 50% HP
- +2000 HP, +30% HP, +15% Life Steal

**Timekeeper's Essence**
- All skills have no cooldown
- +200 ATK, +50% SPD

**Storm Core**
- Summon lightning storm every 3 turns
- +50% Elemental Damage, +80% Lightning Damage

### Socket Management

```csharp
// Socket a rune
RuneSocketManager.Instance.SocketRune(gear, socketIndex, "ruby_tier3");

// Unsocket a rune (returns to inventory)
RuneSocketManager.Instance.UnsocketRune(gear, socketIndex, destroyRune: false);

// Unlock a socket (costs materials)
RuneSocketManager.Instance.UnlockSocket(gear, socketIndex, materials);

// Calculate total bonuses from socketed runes
RuneStatBonus bonuses = RuneSocketManager.Instance.CalculateRuneBonuses(gear);
```

---

## Enhanced Crafting System

Comprehensive crafting with recipes, facilities, transmutation, and salvage.

### Crafting Recipes

Create gear from materials:
- **Material Requirements**: Specific materials needed
- **Success Chance**: Can fail and consume materials
- **Critical Crafts**: Chance to produce better version
- **Crafting Time**: Instant or time-gated
- **Recipe Discovery**: Some recipes must be found

### Critical Craft Bonuses

When a critical craft occurs:
- **Bonus Rarity**: +1 or +2 rarity tiers
- **Bonus Quality**: +20-50% item quality
- **Guaranteed Socket**: Extra socket
- **Guaranteed Affix**: Extra random affix

### Crafting Facilities

Players can build and upgrade facilities:

**Forge** (Level 1-10)
- Craft weapons and armor
- Unlock legendary recipes at high levels
- Bonuses: +Success chance, +Crit chance, -Cost

**Enchanting Tower** (Level 1-10)
- Add and reroll affixes
- Modify existing gear
- Specializes in magical enhancements

**Jeweler's Workshop** (Level 1-10)
- Craft runes
- Add sockets to gear
- Upgrade runes with better success rates

### Transmutation

Convert gear into materials:
- Base yield scales with rarity
- Bonus materials for affixes
- Bonus materials for sockets
- Extra rewards for unique gear

```csharp
// Transmute gear for materials
List<MaterialReward> rewards = EnhancedCraftingManager.Instance.TransmuteGear(gear);
```

### Salvage

Break down gear for materials:
- Returns base materials
- Can extract socketed runes (chance-based)
- Yields scale with level, enhance, and quality
- Affix materials returned

```csharp
// Salvage gear
List<MaterialReward> rewards = EnhancedCraftingManager.Instance.SalvageGear(gear);
```

### Forge Upgrades

Improve existing gear:

**Quality Upgrade**
- Increase item quality (+10%)
- Improves all stats proportionally

**Socket Addition**
- Add 1 socket to gear
- Requires Socket Crystal

**Affix Reroll**
- Reroll all affixes
- Can get better or worse rolls

**Rarity Upgrade**
- 30% chance to increase rarity tier
- Expensive but powerful

### Material Conversion

Convert materials to other types:
- Steel → Mithril (10:1 ratio)
- Essence Dust → Essence Crystal (100:1)
- Optional catalyst for better yield

---

## Configuration

### Files

All systems use JSON configuration files in `Assets/Resources/Config/`:

- **gear_rarity.json**: Rarity tiers, unique items, named sets
- **gear_affixes.json**: Affix definitions, pools, generation rules
- **runes_sockets.json**: Socket types, runes, legendary runes, upgrade recipes
- **crafting_recipes.json**: Recipes, transmutation, forge upgrades, salvage, facilities

### Manager Systems

All managers are Singletons:

- **EnhancedGearManager**: Gear generation, affix rolling, set bonuses
- **RuneSocketManager**: Socket/unsocket runes, rune crafting, upgrades
- **EnhancedCraftingManager**: Recipes, transmutation, salvage, facilities

---

## Usage Examples

### Generate Random Loot

```csharp
// Roll random rarity
int rarityTier = EnhancedGearManager.Instance.RollRandomRarity();

// Set up generation parameters
GearGenerationParams genParams = new GearGenerationParams
{
    gearConfigId = "weapon_sword_base",
    rarityTier = rarityTier,
    itemLevel = playerLevel,
    qualityRoll = Random.Range(0.8f, 1.2f),
    affixCount = Random.Range(2, 5),
    socketCount = Random.Range(1, 3)
};

// Generate gear
EnhancedGearInstance gear = EnhancedGearManager.Instance.GenerateGear(genParams);
```

### Craft Gear

```csharp
// Craft using recipe
EnhancedGearInstance craftedGear = EnhancedCraftingManager.Instance.CraftGear("craft_rare_sword");

if (craftedGear != null)
{
    Debug.Log($"Successfully crafted {craftedGear.rarityId} gear!");
}
```

### Socket Runes

```csharp
// Socket a Tier 3 Ruby into first socket
bool success = RuneSocketManager.Instance.SocketRune(gear, 0, "ruby_tier3");

// Upgrade 3 Tier 2 Rubies to Tier 3
bool upgraded = RuneSocketManager.Instance.UpgradeRune("ruby_tier2");
```

### Reroll Affixes

```csharp
// Reroll all affixes on gear
bool success = EnhancedGearManager.Instance.RerollAffixes(gear, materialCost: 1000);
```

### Check Set Bonuses

```csharp
// Get all equipped gear
List<EnhancedGearInstance> equippedGear = GetEquippedGear();

// Calculate active set bonuses
List<SetBonus> activeBonuses = EnhancedGearManager.Instance.CalculateSetBonuses(equippedGear);

foreach (var bonus in activeBonuses)
{
    Debug.Log($"Active: {bonus.bonusName} - {bonus.bonusDescription}");
}
```

### Transmute or Salvage

```csharp
// Transmute unwanted legendary for materials
List<MaterialReward> transmuted = EnhancedCraftingManager.Instance.TransmuteGear(gear);

// Salvage common gear (tries to extract runes)
List<MaterialReward> salvaged = EnhancedCraftingManager.Instance.SalvageGear(gear);
```

### Upgrade Facility

```csharp
// Upgrade forge to level 2
bool upgraded = EnhancedCraftingManager.Instance.UpgradeFacility("forge");

// Check current facility level
int forgeLevel = EnhancedCraftingManager.Instance.GetFacilityLevel("forge");
```

---

## Integration with Existing Systems

### EnhancedGearInstance

Extends the existing `GearInstance` class with:
- Rarity tier and ID
- Unique gear reference
- List of affixes
- List of sockets
- Item quality multiplier
- Soulbound status

### Gear Stats Calculation

Total stats = Base Stats + Affixes + Socketed Runes + Set Bonuses

```csharp
// Get total stats
GearStatBlock totalStats = gear.GetTotalStats();

// Check for empty sockets
bool hasEmpty = gear.HasEmptySocket();

// Count socketed runes
int runeCount = gear.GetSocketedRuneCount();
```

---

## Progression Systems

### Early Game (Levels 1-20)
- Common and Uncommon gear
- 0-2 affixes
- 0-1 sockets
- Basic rune tiers (1-2)
- Simple crafting recipes

### Mid Game (Levels 21-50)
- Rare and Epic gear
- 2-4 affixes
- 1-2 sockets
- Rune tiers 2-4
- Set bonuses become important
- Forge upgrades available

### End Game (Levels 51+)
- Legendary, Unique, Mythic gear
- 4-8 affixes
- 2-6 sockets
- Rune tier 5 + Legendary runes
- Full set bonuses (6-piece)
- Min-maxing affix combinations
- Rarity upgrades
- Unique gear hunting

---

## Best Practices

### For Designers

1. **Balance Drop Rates**: Adjust rarity weights to match desired progression pace
2. **Affix Variety**: Create diverse affixes for different build types
3. **Set Synergy**: Design sets that complement specific playstyles
4. **Unique Items**: Make unique gear truly special with build-defining effects
5. **Material Economy**: Balance material costs with acquisition rates

### For Developers

1. **Save Progress**: Persist gear instances, rune inventory, facility levels
2. **Validate Configs**: Check JSON files load correctly on startup
3. **Cache Calculations**: Cache affix and rune bonuses for performance
4. **UI Integration**: Create interfaces for socketing, crafting, salvaging
5. **Drop System**: Integrate gear generation with loot tables

---

## Future Enhancements

Potential additions:

1. **Corrupted Items**: High-risk, high-reward modifications
2. **Ancient Items**: Superior versions of existing gear
3. **Primal Items**: Perfect-rolled ancient items
4. **Runewords**: Specific rune combinations unlock special effects
5. **Transmogrification**: Change appearance while keeping stats
6. **Seasonal Affixes**: Time-limited affixes for events
7. **Crafting Mastery**: Player progression in crafting skills
8. **Legendary Crafting**: Craft-only unique items
9. **Set Dungeons**: Special challenges for each gear set
10. **Gem Augments**: Temporary socketing for specific challenges

---

## Credits

Enhanced Gear Systems:
- 7-tier rarity system with unique/mythic tiers
- Random affix generation with 20+ affixes
- Socket and rune system with 5 tiers + legendaries
- Comprehensive crafting with facilities, transmutation, and salvage
- Named gear sets with progressive bonuses

All systems designed for deep customization and endless build variety.
