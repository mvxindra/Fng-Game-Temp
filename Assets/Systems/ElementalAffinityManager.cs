using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ElementalAffinityManager : Singleton<ElementalAffinityManager>
{
    private ElementalAffinityConfig affinityConfig;
    private Dictionary<string, ElementConfig> elements;
    private Dictionary<string, HeroElementalAffinity> heroAffinities;
    private Dictionary<string, ElementalReactionConfig> reactions;

    // Element relationship lookup: [attacker][defender] = multiplier
    private Dictionary<string, Dictionary<string, float>> elementalChart;

    protected override void Awake()
    {
        base.Awake();
        elements = new Dictionary<string, ElementConfig>();
        heroAffinities = new Dictionary<string, HeroElementalAffinity>();
        reactions = new Dictionary<string, ElementalReactionConfig>();
        elementalChart = new Dictionary<string, Dictionary<string, float>>();
        LoadElementalConfigs();
        BuildElementalChart();
    }

    private void LoadElementalConfigs()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/elemental_affinity");
        if (configFile != null)
        {
            affinityConfig = JsonUtility.FromJson<ElementalAffinityConfig>(configFile.text);

            foreach (var element in affinityConfig.elements)
            {
                elements[element.elementId] = element;
            }

            Debug.Log($"Loaded {elements.Count} elemental types");
        }
        else
        {
            Debug.LogWarning("elemental_affinity.json not found in Resources/Config");
            // Create default config
            affinityConfig = new ElementalAffinityConfig
            {
                advantageMultiplier = 1.5f,
                disadvantageMultiplier = 0.75f,
                neutralMultiplier = 1.0f,
                immuneMultiplier = 0.0f
            };
        }

        // Load reactions
        TextAsset reactionsFile = Resources.Load<TextAsset>("Config/elemental_reactions");
        if (reactionsFile != null)
        {
            ElementalReactionDatabase db = JsonUtility.FromJson<ElementalReactionDatabase>(reactionsFile.text);
            foreach (var reaction in db.reactions)
            {
                reactions[reaction.reactionId] = reaction;
            }
            Debug.Log($"Loaded {reactions.Count} elemental reactions");
        }
    }

    private void BuildElementalChart()
    {
        if (affinityConfig?.relationships == null) return;

        foreach (var relationship in affinityConfig.relationships)
        {
            if (!elementalChart.ContainsKey(relationship.attackingElement))
            {
                elementalChart[relationship.attackingElement] = new Dictionary<string, float>();
            }

            float multiplier = affinityConfig.neutralMultiplier;

            switch (relationship.relationshipType)
            {
                case "advantage":
                    multiplier = affinityConfig.advantageMultiplier;
                    break;
                case "disadvantage":
                    multiplier = affinityConfig.disadvantageMultiplier;
                    break;
                case "neutral":
                    multiplier = affinityConfig.neutralMultiplier;
                    break;
                case "immune":
                    multiplier = affinityConfig.immuneMultiplier;
                    break;
                case "custom":
                    multiplier = relationship.customMultiplier;
                    break;
            }

            elementalChart[relationship.attackingElement][relationship.defendingElement] = multiplier;
        }
    }

    // Initialize hero elemental affinity
    public void InitializeHeroElement(string heroId, string primaryElement, string secondaryElement = null)
    {
        if (!heroAffinities.ContainsKey(heroId))
        {
            HeroElementalAffinity affinity = new HeroElementalAffinity
            {
                heroId = heroId,
                primaryElement = primaryElement,
                secondaryElement = secondaryElement,
                elementalMastery = 0
            };
            heroAffinities[heroId] = affinity;
        }
    }

    // Calculate elemental damage multiplier
    public float GetElementalMultiplier(string attackerHeroId, string defenderHeroId)
    {
        HeroElementalAffinity attackerAffinity = GetHeroAffinity(attackerHeroId);
        HeroElementalAffinity defenderAffinity = GetHeroAffinity(defenderHeroId);

        if (attackerAffinity == null || defenderAffinity == null)
        {
            return affinityConfig.neutralMultiplier;
        }

        string attackElement = attackerAffinity.primaryElement;
        string defenseElement = defenderAffinity.primaryElement;

        // Check if same element
        if (attackElement == defenseElement)
        {
            return affinityConfig.sameElementReduction;
        }

        // Get base multiplier from chart
        float multiplier = GetElementMultiplier(attackElement, defenseElement);

        // Apply elemental mastery bonus (0-100 mastery = 0-20% bonus)
        float masteryBonus = (attackerAffinity.elementalMastery / 100f) * 0.2f;
        multiplier *= (1f + masteryBonus);

        // Apply resistance
        float resistance = GetElementalResistance(defenderHeroId, attackElement);
        multiplier *= (1f - resistance);

        // Apply penetration
        float penetration = GetElementalPenetration(attackerHeroId, defenseElement);
        multiplier *= (1f + penetration);

        return multiplier;
    }

    // Get multiplier between two elements
    private float GetElementMultiplier(string attackElement, string defenseElement)
    {
        if (elementalChart.ContainsKey(attackElement) &&
            elementalChart[attackElement].ContainsKey(defenseElement))
        {
            return elementalChart[attackElement][defenseElement];
        }

        return affinityConfig.neutralMultiplier;
    }

    // Check and trigger elemental reactions
    public ElementalReactionResult CheckElementalReaction(string attackerElement, List<string> targetElements)
    {
        ElementalReactionResult result = new ElementalReactionResult();

        foreach (var reaction in reactions.Values)
        {
            if (CanTriggerReaction(reaction, attackerElement, targetElements))
            {
                result.reactionTriggered = true;
                result.reactionId = reaction.reactionId;
                result.reactionName = reaction.reactionName;
                result.damageMultiplier = reaction.damageMultiplier;
                result.statusEffectId = reaction.statusEffectId;
                result.duration = reaction.duration;

                Debug.Log($"Triggered elemental reaction: {reaction.reactionName}");
                return result;
            }
        }

        return result;
    }

    private bool CanTriggerReaction(ElementalReactionConfig reaction, string newElement, List<string> existingElements)
    {
        if (reaction.requiredElements == null || reaction.requiredElements.Count == 0)
        {
            return false;
        }

        List<string> allElements = new List<string>(existingElements) { newElement };

        // Check if all required elements are present
        foreach (string required in reaction.requiredElements)
        {
            if (!allElements.Contains(required))
            {
                return false;
            }
        }

        return true;
    }

    // Increase elemental mastery
    public void IncreaseElementalMastery(string heroId, int amount)
    {
        HeroElementalAffinity affinity = GetHeroAffinity(heroId);
        if (affinity != null)
        {
            affinity.elementalMastery = Mathf.Min(affinity.elementalMastery + amount, 100);
            Debug.Log($"Hero {heroId} elemental mastery increased to {affinity.elementalMastery}");
        }
    }

    // Set secondary element (from ascension)
    public void UnlockSecondaryElement(string heroId, string secondaryElement)
    {
        HeroElementalAffinity affinity = GetHeroAffinity(heroId);
        if (affinity != null)
        {
            affinity.secondaryElement = secondaryElement;
            Debug.Log($"Hero {heroId} unlocked secondary element: {secondaryElement}");
        }
    }

    // Elemental resistance
    public void SetElementalResistance(string heroId, string element, float resistance)
    {
        HeroElementalAffinity affinity = GetHeroAffinity(heroId);
        if (affinity != null)
        {
            affinity.elementalResistances[element] = Mathf.Clamp01(resistance);
        }
    }

    public float GetElementalResistance(string heroId, string element)
    {
        HeroElementalAffinity affinity = GetHeroAffinity(heroId);
        if (affinity != null && affinity.elementalResistances.ContainsKey(element))
        {
            return affinity.elementalResistances[element];
        }
        return 0f;
    }

    // Elemental penetration
    public void SetElementalPenetration(string heroId, string element, float penetration)
    {
        HeroElementalAffinity affinity = GetHeroAffinity(heroId);
        if (affinity != null)
        {
            affinity.elementalPenetration[element] = Mathf.Clamp01(penetration);
        }
    }

    public float GetElementalPenetration(string heroId, string element)
    {
        HeroElementalAffinity affinity = GetHeroAffinity(heroId);
        if (affinity != null && affinity.elementalPenetration.ContainsKey(element))
        {
            return affinity.elementalPenetration[element];
        }
        return 0f;
    }

    // Get elemental stat bonuses
    public ElementalStats GetElementalStatBonus(string element)
    {
        if (elements.ContainsKey(element))
        {
            return elements[element].baseStats;
        }
        return new ElementalStats();
    }

    // Utility methods
    public HeroElementalAffinity GetHeroAffinity(string heroId)
    {
        return heroAffinities.ContainsKey(heroId) ? heroAffinities[heroId] : null;
    }

    public ElementConfig GetElementConfig(string elementId)
    {
        return elements.ContainsKey(elementId) ? elements[elementId] : null;
    }

    // Get weakness/advantage info for UI
    public List<string> GetElementAdvantages(string element)
    {
        List<string> advantages = new List<string>();

        if (elementalChart.ContainsKey(element))
        {
            foreach (var kvp in elementalChart[element])
            {
                if (kvp.Value > affinityConfig.neutralMultiplier)
                {
                    advantages.Add(kvp.Key);
                }
            }
        }

        return advantages;
    }

    public List<string> GetElementWeaknesses(string element)
    {
        List<string> weaknesses = new List<string>();

        if (elementalChart.ContainsKey(element))
        {
            foreach (var kvp in elementalChart[element])
            {
                if (kvp.Value < affinityConfig.neutralMultiplier)
                {
                    weaknesses.Add(kvp.Key);
                }
            }
        }

        return weaknesses;
    }
}

// Result of an elemental reaction
public class ElementalReactionResult
{
    public bool reactionTriggered = false;
    public string reactionId;
    public string reactionName;
    public float damageMultiplier = 1f;
    public string statusEffectId;
    public int duration;
}

[System.Serializable]
public class ElementalReactionDatabase
{
    public List<ElementalReactionConfig> reactions;
}
