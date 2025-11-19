using System;
using System.Collections.Generic;

[Serializable]
public class MaterialConfig
{
    public string id;
    public int rarity;
}

[Serializable]
public class MaterialList
{
    public List<MaterialConfig> materials;
}
