using UnityEngine;

public static class DangerLevelCalculator
{
    [System.Serializable]
    public struct Tuning
    {
        [Header("Expected Ranges (for normalization)")]
        public float minDamage;
        public float maxDamage;
        public float minSpeed;
        public float maxSpeed;

        [Header("Weights (should add to 1, but code will normalize anyway)")]
        [Range(0f, 1f)] public float damageWeight;
        [Range(0f, 1f)] public float speedWeight;

        [Header("Output")]
        public int maxLevel; // e.g. 10 for 1-10

        public static Tuning Default => new Tuning
        {
            minDamage = 1f,
            maxDamage = 30f,
            minSpeed  = 1f,
            maxSpeed  = 6f,
            damageWeight = 0.6f,
            speedWeight  = 0.4f,
            maxLevel = 10
        };
    }

    // 0..1 score
    public static float Score01(float damage, float speed, Tuning tuning)
    {
        float dN = Normalize(damage, tuning.minDamage, tuning.maxDamage);
        float sN = Normalize(speed, tuning.minSpeed, tuning.maxSpeed);

        float wSum = Mathf.Max(0.0001f, tuning.damageWeight + tuning.speedWeight);
        float score = (dN * tuning.damageWeight + sN * tuning.speedWeight) / wSum;

        return Mathf.Clamp01(score);
    }

    // 0..100 score
    public static float Score100(float damage, float speed, Tuning tuning)
    {
        return Score01(damage, speed, tuning) * 100f;
    }

    // 1..maxLevel (default 1..10)
    public static int Level(float damage, float speed, Tuning tuning)
    {
        float s01 = Score01(damage, speed, tuning);
        int level = Mathf.CeilToInt(s01 * tuning.maxLevel);
        return Mathf.Clamp(level, 1, tuning.maxLevel);
    }

    private static float Normalize(float value, float min, float max)
    {
        if (max <= min) return 0f;
        return Mathf.Clamp01(Mathf.InverseLerp(min, max, value));
    }
}
