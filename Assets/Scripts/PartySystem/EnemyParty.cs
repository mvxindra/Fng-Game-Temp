using System;
using System.Collections.Generic;

[Serializable]
public class EnemyParty
{
    public List<string> enemyIds = new();   // IDs from enemy.json

    public EnemyParty() {}

    public EnemyParty(List<string> ids)
    {
        enemyIds = ids;
    }
}
