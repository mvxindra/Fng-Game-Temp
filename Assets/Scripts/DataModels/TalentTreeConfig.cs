using System;

using System.Collections.Generic;

 

[Serializable]

public class TalentTreeConfig

{

    public string treeId;                    // Unique tree identifier

    public string heroId;                    // Hero this tree belongs to

    public string treeName;                  // e.g., "Path of the Berserker"

    public string description;               // Tree theme description

 

    public List<TalentPath> paths;           // Multiple specialization paths

    public List<TalentNodeConfig> nodes;     // All talent nodes in this tree

 

    // Progression settings

    public int maxTalentPoints;              // Maximum points a hero can spend

    public int talentPointsPerLevel;         // Points gained per hero level (typically 1)

    public int talentPointsPerAscension;     // Bonus points from ascension

}

 

[Serializable]

public class TalentPath

{

    public string pathId;                    // e.g., "berserker_damage"

    public string pathName;                  // e.g., "Berserker Path"

    public string description;               // Path specialization description

    public string pathType;                  // "damage", "support", "tank", "utility"

    public List<string> nodeIds;             // Nodes that belong to this path

    public string keystoneNodeId;            // Final powerful node in path

    public int requiredNodesForKeystone;     // Nodes needed in path to unlock keystone

}