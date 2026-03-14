namespace MagicGirls.Core.Enums
{
    public enum TimeSegment
    {
        Morning1 = 0,
        Morning2 = 1,
        Day1 = 2,
        Day2 = 3,
        Night1 = 4,
        Night2 = 5
    }

    public enum ActivityType
    {
        Rest,
        Train,
        School,
        DungeonCrawl,
        Recreation,
        Work
    }

    public enum BaseStat
    {
        STR,
        DEX,
        CON,
        INT,
        WIS,
        CHA
    }

    public enum MagicalStat
    {
        MagicPower,
        MagicAffinity,
        Corruption,
        Morale
    }

    public enum DieType
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12,
        D20 = 20,
        D100 = 100
    }

    public enum PersonalityTrait
    {
        Brave,
        Timid,
        Studious,
        Reckless,
        Kind,
        Aloof,
        Cheerful,
        Serious,
        Lazy,
        Diligent
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Holy,
        Dark
    }

    public enum BattleActionType
    {
        Attack,
        Defend,
        Spell,
        Item
    }

    public enum StatTargetType
    {
        Base,
        Magical
    }
}
