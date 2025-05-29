using UnityEngine;

[CreateAssetMenu(fileName = "BaseStat", menuName = "Base Stat SO")]
public class BaseStatSO : ScriptableObject
{
    [Header("Growth Stats")]
    [SerializeField, Tooltip("Base Hit Points"), Range(0, 9999)] private int baseHP;
    [SerializeField, Tooltip("Base Numo"), Range(0, 9999)] private int baseNumo;
    [SerializeField, Tooltip("Base Defense"), Range(0, 9999)] private int baseDef;

    [Header("Level Stats")]
    [SerializeField, Tooltip("Base Vitality"), Range(0, 99)] private int baseVitality;
    [SerializeField, Tooltip("Base Strength"), Range(0, 99)] private int baseStrength;
    [SerializeField, Tooltip("Base Wisdom"), Range(0, 99)] private int baseWisdom;
    [SerializeField, Tooltip("Base Agility"), Range(0, 99)] private int baseAgility;
    [SerializeField, Tooltip("Base Luck"), Range(0, 99)] private int baseLuck;

    [Header("Modifiers")]
    [SerializeField, Tooltip("Base Critical Hit Chance (%)"), Range(0, 100)] private int baseCritChance;
    [SerializeField, Tooltip("Base Zeal"), Range(0, 999)] private int baseZeal;
    [SerializeField, Tooltip("Base Potency"), Range(0, 999)] private int basePotency;

    [Header("Damage Bonus")]
    [SerializeField, Tooltip("Base Melee Damage Bonus"), Range(0, 999)] private int baseMeleeDmg;
    [SerializeField, Tooltip("Base Ranged Damage Bonus"), Range(0, 999)] private int baseRangedDmg;
    [SerializeField, Tooltip("Base Corporeal Damage Bonus"), Range(0, 999)] private int baseCorporealDmg;
    [SerializeField, Tooltip("Base Frigid Damage Bonus"), Range(0, 999)] private int baseFrigidDmg;
    [SerializeField, Tooltip("Base Scorch Damage Bonus"), Range(0, 999)] private int baseScorchDmg;
    [SerializeField, Tooltip("Base Caustic Damage Bonus"), Range(0, 999)] private int baseCausticDmg;
    [SerializeField, Tooltip("Base Arc Damage Bonus"), Range(0, 999)] private int baseArcDmg;
    [SerializeField, Tooltip("Base Divine Damage Bonus"), Range(0, 999)] private int baseDivineDmg;

    [Header("Resistances")]
    [SerializeField, Tooltip("Base Corporeal Resistance"), Range(0, 90)] private int baseCorporealRes;
    [SerializeField, Tooltip("Base Frigid Resistance"), Range(0, 90)] private int baseFrigidRes;
    [SerializeField, Tooltip("Base Scorch Resistance"), Range(0, 90)] private int baseScorchRes;
    [SerializeField, Tooltip("Base Caustic Resistance"), Range(0, 90)] private int baseCausticRes;
    [SerializeField, Tooltip("Base Arc Resistance"), Range(0, 90)] private int baseArcRes;
    [SerializeField, Tooltip("Base Divine Resistance"), Range(0, 90)] private int baseDivineRes;

    [Header("Special Attributes")]
    [SerializeField, Tooltip("Base Special (Unique Attribute)")] private int baseSpecial;

    // Public Getters
    public int BaseHP => baseHP;
    public int BaseNumo => baseNumo;
    public int BaseDef => baseDef;

    public int BaseVitality => baseVitality;
    public int BaseStrength => baseStrength;
    public int BaseWisdom => baseWisdom;
    public int BaseAgility => baseAgility;
    public int BaseLuck => baseLuck;

    public int BaseCritChance => baseCritChance;
    public int BaseZeal => baseZeal;
    public int BasePotency => basePotency;

    public int BaseMeleeDmg => baseMeleeDmg;
    public int BaseRangedDmg => baseRangedDmg;
    public int BaseCorporealDmg => baseCorporealDmg;
    public int BaseFrigidDmg => baseFrigidDmg;
    public int BaseScorchDmg => baseScorchDmg;
    public int BaseCausticDmg => baseCausticDmg;
    public int BaseArcDmg => baseArcDmg;
    public int BaseDivineDmg => baseDivineDmg;

    public int BaseCorporealRes => baseCorporealRes;
    public int BaseFrigidRes => baseFrigidRes;
    public int BaseScorchRes => baseScorchRes;
    public int BaseCausticRes => baseCausticRes;
    public int BaseArcRes => baseArcRes;
    public int BaseDivineRes => baseDivineRes;

    public int BaseSpecial => baseSpecial;
}
