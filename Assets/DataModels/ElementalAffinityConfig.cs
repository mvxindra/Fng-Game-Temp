using System;
using System.Collections.Generic;

[Serializable]
public class ElementalAffinityConfig
{
    public List<ElementConfig> elements;
    public List<ElementalRelationship> relationships;

    // Damage multipliers
    public float advantageMultiplier = 1.5f;      // Strong against
    public float disadvantageMultiplier = 0.75f;   // Weak against
    public float neutralMultiplier = 1.0f;         // Neutral
    public float immuneMultiplier = 0.0f;          // Immune

    // Additional bonuses
    public float sameElementReduction = 0.5f;      // Damage reduction when same element
}

[Serializable]
public class ElementConfig
{
    public string elementId;                       // "fire", "water", "earth", "wind", "light", "dark", "lightning", "ice"
    public string elementName;                     // Display name
    public string description;                     // Element description
    public string color;                           // Hex color for UI

    // Elemental characteristics
    public ElementalStats baseStats;               // Stat bonuses for this element
    public List<string> commonStatusEffects;       // Common status effects (burn, freeze, etc.)
}

[Serializable]
public class ElementalStats
{
    public float atkBonus;                         // Base ATK bonus %
    public float defBonus;                         // Base DEF bonus %
    public float hpBonus;                          // Base HP bonus %
    public float spdBonus;                         // Base SPD bonus %
    public float critBonus;                        // CRIT chance bonus
    public string specialAbility;                  // Unique elemental ability
}

[Serializable]
public class ElementalRelationship
{
    public string attackingElement;                // Element dealing damage
    public string defendingElement;                // Element receiving damage
    public string relationshipType;                // "advantage", "disadvantage", "neutral", "immune"
    public float customMultiplier;                 // Custom multiplier (optional)
}

[Serializable]
public class HeroElementalAffinity
{
    public string heroId;
    public string primaryElement;                  // Main element
    public string secondaryElement;                // Secondary element (unlocked via ascension)
    public int elementalMastery;                   // Mastery level (0-100)

    // Element-specific bonuses
    public Dictionary<string, float> elementalResistances;  // Resistance to each element
    public Dictionary<string, float> elementalPenetration;  // Penetration for each element

    public HeroElementalAffinity()
    {
        elementalResistances = new Dictionary<string, float>();
        elementalPenetration = new Dictionary<string, float>();
    }
}

// Elemental reaction system (like Genshin Impact)
[Serializable]
public class ElementalReactionConfig
{
    public string reactionId;                      // "vaporize", "melt", "overload", "freeze", etc.
    public string reactionName;
    public string description;

    public List<string> requiredElements;          // Elements needed to trigger
    public string triggerOrder;                    // "any" or "specific"

    // Reaction effects
    public string effectType;                      // "damage_bonus", "status_effect", "damage_over_time", "crowd_control"
    public float damageMultiplier;                 // Bonus damage multiplier
    public string statusEffectId;                  // Status effect to apply
    public int duration;                           // Effect duration
    public float bonusValue;                       // Additional effect value
}
