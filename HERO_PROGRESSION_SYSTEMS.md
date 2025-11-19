# Hero Progression & Specialization Systems

This document describes the comprehensive hero progression and specialization systems implemented for the game.

## Table of Contents

1. [Talent Tree System](#talent-tree-system)
2. [Hero Ascension / Limit Break](#hero-ascension--limit-break)
3. [Elemental Affinity & Counter System](#elemental-affinity--counter-system)
4. [Hero Quest & Origin Mission System](#hero-quest--origin-mission-system)
5. [Integration with Combat](#integration-with-combat)
6. [Configuration Files](#configuration-files)

---

## Talent Tree System

The Talent Tree system allows heroes to specialize in different combat paths through branching upgrade trees.

### Key Features

- **Multiple Specialization Paths**: Each hero can choose between different playstyles (damage, tank, support, hybrid)
- **Talent Points**: Earned through leveling up and ascension
- **Progressive Unlocks**: Higher-tier talents require prerequisites and ascension levels
- **Stat Bonuses**: Incremental improvements to core stats
- **Skill Unlocks**: Powerful keystone talents unlock new abilities
- **Passive Abilities**: Special triggered effects

### Files

- **TalentNodeConfig.cs** - Individual talent node configuration
- **TalentTreeConfig.cs** - Complete talent tree structure with paths
- **HeroTalentProgress.cs** - Tracks player's talent selections per hero
- **TalentTreeManager.cs** - Singleton manager for all talent operations

### Usage

```csharp
// Initialize hero talents
TalentTreeManager.Instance.InitializeHeroTalents("hero_warrior", level: 1, ascension: 0);

// Grant talent points on level up
TalentTreeManager.Instance.GrantTalentPoints("hero_warrior", 1);

// Unlock a talent node
bool success = TalentTreeManager.Instance.UnlockTalentNode("hero_warrior", "war_atk1");

// Get stat bonuses from talents
Dictionary<string, float> bonuses = TalentTreeManager.Instance.GetTalentStatBonuses("hero_warrior");

// Reset talents (for respec)
TalentTreeManager.Instance.ResetTalents("hero_warrior");
```

### Configuration

See `Assets/Resources/Config/talent_trees.json` for configuration examples.

---

## Hero Ascension / Limit Break

Heroes can ascend through multiple tiers, breaking level caps and unlocking powerful new abilities.

### Key Features

- **10+ Ascension Tiers**: Progressive power increases
- **Level Cap Increases**: Each ascension raises the maximum level
- **Stat Bonuses**: Both flat and percentage-based improvements
- **Skill Unlocks**: New abilities at each tier
- **Skill Evolution**: Transform basic skills into powerful versions
- **Ultimate Skills**: Unlock game-changing ultimate abilities
- **Special Features**:
  - Skill Evolution (upgrade existing skills)
  - Dual Element (gain secondary element affinity)
  - Stat Break (exceed normal stat caps)
  - Passive Upgrades

### Files

- **AscensionTierConfig.cs** - Configuration for each ascension tier
- **HeroAscensionProgress.cs** - Tracks hero's ascension progress
- **HeroAscensionManager.cs** - Singleton manager for ascension system

### Usage

```csharp
// Initialize hero ascension
HeroAscensionManager.Instance.InitializeHeroAscension("hero_warrior", startingLevel: 1);

// Level up a hero
bool leveled = HeroAscensionManager.Instance.LevelUpHero("hero_warrior", levelsToGain: 1);

// Ascend to next tier (requires materials and level)
bool ascended = HeroAscensionManager.Instance.AscendHero("hero_warrior");

// Get stat bonuses from ascension
AscensionStatBonus bonuses = HeroAscensionManager.Instance.GetAscensionStatBonuses("hero_warrior");

// Check if skill has evolved
string evolvedSkill = HeroAscensionManager.Instance.GetEvolvedSkill("hero_warrior", "skill_power_slash");
```

### Ascension Requirements

Each tier requires:
- **Minimum Level**: Hero must be at previous tier's cap
- **Materials**: Specific ascension materials
- **Gold**: Increasing gold costs

### Configuration

See `Assets/Resources/Config/hero_ascension.json` for configuration examples.

---

## Elemental Affinity & Counter System

A rock-paper-scissors style elemental system with counters, reactions, and synergies.

### Elements

1. **Fire** - High damage, burn effects
2. **Water** - Healing and support
3. **Earth** - Defense and durability
4. **Wind** - Speed and evasion
5. **Lightning** - Burst damage and paralysis
6. **Ice** - Crowd control and slowing
7. **Light** - Holy damage and purification
8. **Dark** - Debuffs and life drain

### Key Features

- **Type Advantages**: 1.5x damage against weak elements
- **Type Disadvantages**: 0.75x damage against resistant elements
- **Elemental Resistances**: Heroes can build resistance to specific elements
- **Elemental Penetration**: Bypass enemy resistances
- **Elemental Mastery**: 0-100 scale improves elemental effectiveness
- **Dual Elements**: Unlock via ascension for versatility
- **Elemental Reactions**: Combining elements creates powerful effects

### Elemental Reactions

- **Vaporize** (Fire + Water): Massive damage explosion
- **Melt** (Fire + Ice): Defense-melting damage
- **Freeze** (Water + Ice): Immobilize enemies
- **Electrocute** (Water + Lightning): Chain damage
- **Overload** (Fire + Lightning): Explosive DOT
- **Storm** (Wind + Lightning): Devastating area damage
- **Shatter** (Earth + Ice): Critical damage to frozen targets
- And more...

### Files

- **ElementalAffinityConfig.cs** - Element definitions and relationships
- **ElementalAffinityManager.cs** - Singleton manager for elemental system

### Usage

```csharp
// Initialize hero's element
ElementalAffinityManager.Instance.InitializeHeroElement("hero_warrior", "fire", secondaryElement: null);

// Get damage multiplier for elemental matchup
float multiplier = ElementalAffinityManager.Instance.GetElementalMultiplier("attacker_hero", "defender_hero");

// Check for elemental reactions
List<string> targetElements = new List<string> { "water" };
ElementalReactionResult reaction = ElementalAffinityManager.Instance.CheckElementalReaction("fire", targetElements);

// Increase elemental mastery
ElementalAffinityManager.Instance.IncreaseElementalMastery("hero_warrior", 10);

// Unlock secondary element (from ascension)
ElementalAffinityManager.Instance.UnlockSecondaryElement("hero_warrior", "earth");
```

### Configuration

See `Assets/Resources/Config/elemental_affinity.json` and `elemental_reactions.json`.

---

## Hero Quest & Origin Mission System

Unique story-driven quests for each hero that unlock abilities, lore, and exclusive rewards.

### Quest Types

1. **Origin Quests**: Hero's backstory and character development
2. **Character Story**: Ongoing character narrative
3. **Mastery Quests**: Skill-based challenges
4. **Awakening Quests**: Unlock ultimate forms

### Key Features

- **Story Chapters**: Multi-part narrative arcs
- **Objective Tracking**: Defeat enemies, complete stages, use skills, etc.
- **Battle Conditions**: Special requirements (solo, no deaths, time limits)
- **Unique Rewards**:
  - Exclusive soulbound equipment
  - Skill unlocks
  - Passive abilities
  - Cosmetic titles
  - Lore reveals
- **Repeatable Quests**: With cooldown timers
- **Progressive Unlocks**: Quests unlock new quests

### Quest Objectives

- **defeat_enemies**: Kill specific or any enemies
- **complete_stage**: Clear specific stages
- **use_skill**: Use skills X times
- **survive_turns**: Survive for X turns
- **collect_item**: Gather quest items

### Files

- **HeroQuestConfig.cs** - Quest configuration
- **HeroQuestProgress.cs** - Tracks quest progress
- **HeroQuestManager.cs** - Singleton manager for quest system

### Usage

```csharp
// Initialize hero quests
HeroQuestManager.Instance.InitializeHeroQuests("hero_warrior");

// Start a quest
bool started = HeroQuestManager.Instance.StartQuest("hero_warrior", "warrior_origin_1");

// Update quest progress
HeroQuestManager.Instance.UpdateQuestProgress("hero_warrior", "warrior_origin_1", "defeat_bandits", amount: 1);

// Track enemy defeats
HeroQuestManager.Instance.OnEnemyDefeated("hero_warrior", "enemy_bandit");

// Track skill usage
HeroQuestManager.Instance.OnSkillUsed("hero_warrior", "skill_power_slash");

// Track stage completion
HeroQuestManager.Instance.OnStageCompleted("hero_warrior", "stage_ancient_ruins");

// Get available quests
List<HeroQuestConfig> available = HeroQuestManager.Instance.GetAvailableQuests("hero_warrior");
```

### Configuration

See `Assets/Resources/Config/hero_quests.json` and `origin_stories.json`.

---

## Integration with Combat

All progression systems integrate seamlessly with the existing combat system.

### HeroProgressionIntegration

This singleton manager handles applying all progression bonuses to combat units.

### Usage in Combat

```csharp
// When initializing a combat unit
CombatUnit unit = GetComponent<CombatUnit>();
HeroProgressionIntegration.Instance.ApplyProgressionToCombatUnit(unit, heroId, heroLevel);

// Calculate damage with elemental multipliers
int finalDamage = HeroProgressionIntegration.Instance.CalculateDamageWithElemental(attacker, defender, baseDamage);

// When hero levels up
HeroProgressionIntegration.Instance.OnHeroLevelUp(heroId, newLevel);

// When battle completes
List<string> defeatedEnemies = new List<string> { "enemy1", "enemy2" };
HeroProgressionIntegration.Instance.OnBattleComplete(heroId, stageId, defeatedEnemies, victory: true);

// Get complete progression summary
HeroProgressionSummary summary = HeroProgressionIntegration.Instance.GetProgressionSummary(heroId);
```

### Auto-Application

The integration system automatically:
- Applies ascension stat bonuses
- Applies talent tree stat bonuses
- Applies elemental stat bonuses
- Adds unlocked skills from talents and ascension
- Handles skill evolution
- Tracks quest progress during battles

---

## Configuration Files

All systems use JSON configuration files located in `Assets/Resources/Config/`:

### Talent Trees
**File**: `talent_trees.json`
- Defines talent trees for each hero
- Specifies paths and nodes
- Sets requirements and effects

### Hero Ascension
**File**: `hero_ascension.json`
- Defines ascension tiers per hero
- Material requirements
- Stat bonuses and unlocks

### Elemental Affinity
**File**: `elemental_affinity.json`
- Element definitions
- Type matchup chart
- Elemental stat bonuses

### Elemental Reactions
**File**: `elemental_reactions.json`
- Reaction definitions
- Required element combinations
- Effect multipliers

### Hero Quests
**File**: `hero_quests.json`
- Quest definitions
- Objectives and requirements
- Rewards

### Origin Stories
**File**: `origin_stories.json`
- Story chapters
- Dialogue
- Quest progression

---

## Extended HeroConfig

The `HeroConfig` class has been extended with progression fields:

```csharp
public class HeroConfig
{
    // ... existing fields ...

    // Hero Progression Extensions
    public string primaryElement;     // Elemental affinity
    public int baseLevel = 1;         // Starting level
    public int baseAscension = 0;     // Starting ascension tier
}
```

---

## Manager Initialization

All managers are Singletons and initialize automatically:

1. **TalentTreeManager** - Loads talent tree configs
2. **HeroAscensionManager** - Loads ascension configs
3. **ElementalAffinityManager** - Loads element and reaction configs
4. **HeroQuestManager** - Loads quest and story configs
5. **HeroProgressionIntegration** - Coordinates all systems

---

## Best Practices

### For Designers

1. **Balance Talent Trees**: Ensure no single path is dominant
2. **Progressive Difficulty**: Make ascension requirements scale appropriately
3. **Meaningful Choices**: Each talent should feel impactful
4. **Element Variety**: Give heroes diverse elemental options
5. **Story Quality**: Make origin quests compelling and rewarding

### For Developers

1. **Initialize on Hero Acquisition**: Call init methods when hero is obtained
2. **Track Progress**: Use quest tracking methods during battles
3. **Apply Bonuses**: Always use HeroProgressionIntegration when creating combat units
4. **Save Data**: Persist all progress dictionaries
5. **Validate Configs**: Check JSON configs are properly formatted

---

## Example: Complete Hero Setup

```csharp
// 1. Hero is obtained
string heroId = "hero_warrior";
int heroLevel = 1;

// 2. Initialize all progression systems
TalentTreeManager.Instance.InitializeHeroTalents(heroId, heroLevel, 0);
HeroAscensionManager.Instance.InitializeHeroAscension(heroId, heroLevel);
ElementalAffinityManager.Instance.InitializeHeroElement(heroId, "fire");
HeroQuestManager.Instance.InitializeHeroQuests(heroId);

// 3. Player plays the game...
// Level up grants talent points automatically
HeroAscensionManager.Instance.LevelUpHero(heroId, 1);

// Player spends talent points
TalentTreeManager.Instance.UnlockTalentNode(heroId, "war_atk1");

// Player ascends at level 20
HeroAscensionManager.Instance.AscendHero(heroId);

// 4. In combat, apply all bonuses
CombatUnit combatUnit = CreateCombatUnit(heroConfig);
HeroProgressionIntegration.Instance.ApplyProgressionToCombatUnit(combatUnit, heroId, heroLevel);

// 5. Track quest progress during battle
HeroQuestManager.Instance.OnEnemyDefeated(heroId, "enemy_bandit");
HeroQuestManager.Instance.OnSkillUsed(heroId, "skill_power_slash");
```

---

## Future Enhancements

Potential additions to the system:

1. **Talent Loadouts**: Save and swap talent configurations
2. **Prestige System**: Reset progression for permanent bonuses
3. **Hero Bonds**: Unlock bonuses for using heroes together
4. **Constellation System**: Additional talent unlocks from duplicates
5. **Artifact System**: Equippable items that enhance talents
6. **Guild Talents**: Shared progression with guild members
7. **PvP Talent Restrictions**: Balance for competitive play
8. **Seasonal Talents**: Limited-time special paths

---

## Credits

Hero Progression & Specialization Systems
- Talent Tree System
- Hero Ascension/Limit Break System
- Elemental Affinity & Counter System
- Hero Quest & Origin Mission System
- Integration Layer

All systems designed for scalability and easy content addition.
