using System;
using System.Collections.Generic;

[Serializable]
public class Party
{
    public List<string> heroIds = new();  // 5 hero IDs from hero.json

    public Party() {}

    public Party(List<string> ids)
    {
        heroIds = ids;
    }
}
