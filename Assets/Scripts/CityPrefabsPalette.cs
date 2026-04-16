using UnityEngine;

[CreateAssetMenu(fileName = "CityPrefabsPalette", menuName = "Prefabs/CityPrefabsPalette")]
public class CityPrefabsPalette : ScriptableObject
{
    public Street StreetPrefab;
    public Address AddressPrefab;
}
