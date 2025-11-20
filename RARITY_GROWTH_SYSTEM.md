# Rarity-Based Growth Rate System

## Overview

The Rarity-Based Growth Rate System automatically scales unit/hero growth rates based on their rarity tier. Higher rarity units have significantly better stat growth, making them more powerful at higher levels.

## How It Works

### Growth Rate Multipliers

Each rarity tier has a growth multiplier that scales the base growth rates:

| Rarity Tier | Name | Growth Multiplier |
|-------------|------|-------------------|
| 1 | Common | 1.0x (baseline) |
| 2 | Uncommon | 1.2x (+20%) |
| 3 | Rare | 1.5x (+50%) |
| 4 | Epic | 1.8x (+80%) |
| 5 | Legendary | 2.0x (2x) |
| 6 | Unique | 2.5x (2.5x) |
| 7 | Mythic | 3.0x (3x) |

### Example

Given a hero with:
- Base ATK: 100
- Growth per level: 2 ATK
- Rarity: 5 (Legendary, 2.0x multiplier)

**Without rarity-based growth (old system):**
- Level 1: 100 ATK
- Level 50: 100 + (2 × 49) = 198 ATK

**With rarity-based growth (new system):**
- Level 1: 100 ATK
- Level 50: 100 + (2 × 2.0 × 49) = 296 ATK

A Common (Rarity 1) hero with the same base stats would only have 198 ATK at level 50, making the Legendary hero **49% stronger** due to rarity-based growth.

### Stat Progression Comparison (Base ATK: 100, Growth: 2 ATK/level)

| Level | Common (R1) | Legendary (R5) | Difference |
|-------|-------------|----------------|------------|
| 1 | 100 ATK | 100 ATK | 0 (+0%) |
| 30 | 158 ATK | 216 ATK | +58 (+37%) |
| 60 | 218 ATK | 336 ATK | +118 (+54%) |
| 90 | 278 ATK | 456 ATK | +178 (+64%) |
| 120 | 338 ATK | 576 ATK | +238 (+70%) |

At max level (120), a Legendary hero is **70% stronger** than a Common hero with identical base stats!

## Implementation Files

### Core Files

1. **RarityGrowthConfig.cs** (`Assets/Scripts/DataModels/RarityGrowthConfig.cs`)
   - Defines rarity growth multipliers
   - `RarityGrowthManager` singleton for accessing configs
   - Methods for applying rarity multipliers

2. **rarity_growth.json** (`Assets/Resources/Config/rarity_growth.json`)
   - Configuration file with growth multipliers per rarity
   - Customizable for game balance

### Updated Files

3. **HeroConfig.cs** (`Assets/Scripts/DataModels/HeroConfig.cs`)
   - Added `GetEffectiveGrowth()` - returns growth with rarity multiplier
   - Added `GetStatsAtLevel(int level)` - calculates stats at any level

4. **EnemyConfig.cs** (`Assets/Scripts/DataModels/EnemyConfig.cs`)
   - Added `GetEffectiveGrowth()` - returns growth with rarity multiplier
   - Added `GetStatsAtLevel(int level)` - calculates stats at any level

5. **CombatUnit.cs** (`Assets/Scripts/battle/CombatUnit.cs`)
   - Added `Level` field for tracking unit level
   - Updated `InitializeFromHeroConfig(HeroConfig, int level)` to apply rarity-based growth
   - Added `InitializeFromEnemyConfig(EnemyConfig, int level)` for enemy initialization

## Usage

### In Code

#### Initialize a hero with rarity-based stats

```csharp
// Load hero config
HeroConfig heroCfg = /* load from JSON */;

// Create combat unit at level 50
GameObject unitObj = new GameObject("Hero");
CombatUnit unit = unitObj.AddComponent<CombatUnit>();
unit.InitializeFromHeroConfig(heroCfg, 50);

// Stats are automatically calculated with rarity multiplier
Debug.Log($"ATK at level 50: {unit.BaseATK}");
```

#### Get stats at a specific level (without creating a unit)

```csharp
HeroConfig heroCfg = /* load from JSON */;

// Calculate what stats would be at level 120 (max level)
Stats statsAt120 = heroCfg.GetStatsAtLevel(120);
Debug.Log($"Projected ATK at level 120: {statsAt120.atk}");
```

#### Initialize an enemy with rarity-based stats

```csharp
EnemyConfig enemyCfg = /* load from JSON */;

CombatUnit enemy = enemyObj.AddComponent<CombatUnit>();
enemy.InitializeFromEnemyConfig(enemyCfg, 25);
```

### Customizing Growth Multipliers

Edit `Assets/Resources/Config/rarity_growth.json`:

```json
{
  "configs": [
    {
      "rarityTier": 5,
      "growthMultiplier": 2.5,  // Change from 2.0 to 2.5
      "description": "Legendary - Boosted!",
      "atkGrowthMultiplier": 1.2,  // Optional: 20% extra ATK growth
      "defGrowthMultiplier": 1.0,
      "hpGrowthMultiplier": 1.0,
      "spdGrowthMultiplier": 0.8   // Optional: Reduce SPD growth
    }
  ]
}
```

**Per-stat multipliers** allow fine-tuning. For example:
- `atkGrowthMultiplier: 1.2` means ATK grows at `2.5 × 1.2 = 3.0x` for Legendary heroes
- This allows making Legendary units favor offense over defense

## Testing

Use the provided test script to verify the system:

1. Add `RarityGrowthTest.cs` component to any GameObject
2. In the Inspector, click the context menu options:
   - **Run Rarity Growth Tests** - Display multipliers and stat comparisons
   - **Test CombatUnit Initialization** - Verify CombatUnit integration
   - **Compare Growth Curves** - See stat progression differences

Or run from code:
```csharp
RarityGrowthTest tester = gameObject.AddComponent<RarityGrowthTest>();
tester.RunTests();
```

## Design Notes

### Hero vs Enemy Growth

- **Heroes** use **multiplicative growth** (growth values are typically 1.0-1.5)
  - Example: growth = 1.2 means stats multiply by 1.2 each level
  - Formula: `stat = baseStats + (growth × rarityMultiplier × levelDiff)`

- **Enemies** use **additive growth** (growth values are flat increases like +2 ATK)
  - Example: growth = 2 means +2 ATK per level
  - Formula: `stat = baseStats + (growth × rarityMultiplier × levelDiff)`

Both systems benefit from rarity multipliers, but heroes scale more dramatically at high levels.

### Balance Considerations

1. **Early Game** (Level 1-30): Rarity differences are small (a few stat points)
2. **Mid Game** (Level 30-70): Rarity becomes noticeable (20-40% difference)
3. **Late Game** (Level 70-120): Rarity is crucial (50-100%+ difference)

This creates a natural progression where:
- Common heroes are viable early
- Rare/Epic heroes dominate mid-game
- Legendary/Mythic heroes are end-game powerhouses (especially at max level 120)

### Backwards Compatibility

The system is **fully backwards compatible**:
- Old code calling `InitializeFromHeroConfig(heroCfg)` defaults to level 1
- Existing configs work without modification
- If `rarity_growth.json` is missing, uses default multipliers

## Future Enhancements

Possible additions:
1. **Ascension Synergy**: Ascension could further boost rarity multipliers
2. **Element-Specific Growth**: Fire units grow faster in ATK, Water in DEF, etc.
3. **Soft Caps**: Diminishing returns at very high levels to prevent infinite scaling
4. **Rarity Upgrade System**: Allow promoting units to higher rarities

## Configuration Reference

### RarityGrowthConfig Fields

```csharp
public class RarityGrowthConfig
{
    public int rarityTier;              // 1-7
    public float growthMultiplier;      // Base multiplier for all stats
    public string description;          // Display name

    // Optional per-stat multipliers (default 1.0)
    public float atkGrowthMultiplier;   // ATK-specific multiplier
    public float defGrowthMultiplier;   // DEF-specific multiplier
    public float hpGrowthMultiplier;    // HP-specific multiplier
    public float spdGrowthMultiplier;   // SPD-specific multiplier
}
```

### RarityGrowthManager API

```csharp
// Get overall growth multiplier for a rarity
float multiplier = RarityGrowthManager.Instance.GetGrowthMultiplier(5); // 2.0 for Legendary

// Get stat-specific multiplier
float atkMult = RarityGrowthManager.Instance.GetStatGrowthMultiplier(5, "atk");

// Apply rarity growth to a value
float modifiedGrowth = RarityGrowthManager.Instance.ApplyRarityGrowth(2.0f, 5, "atk");

// Get full config
RarityGrowthConfig config = RarityGrowthManager.Instance.GetConfig(5);
```

## Troubleshooting

**Issue**: Stats don't seem to scale with rarity
- **Solution**: Ensure you're using the new `InitializeFromHeroConfig(config, level)` with level parameter
- Check that `rarity_growth.json` is in `Assets/Resources/Config/`

**Issue**: Growth multipliers not loading
- **Solution**: Check Unity console for warnings from RarityGrowthManager
- Verify JSON format is valid
- System will fall back to default multipliers if config is missing

**Issue**: Stats are too high/low
- **Solution**: Adjust multipliers in `rarity_growth.json`
- Remember: higher rarity should feel significantly stronger at high levels

## Credits

System designed to provide meaningful progression and make rarity feel impactful without invalidating lower-rarity units in early game.
