using RimWorld;
using System;
using Verse;

namespace RandomChance
{
    public static class RCQualityUtil
    {
        public static float GetQualityValue(QualityCategory quality, SimpleCurve factors)
        {
            return quality switch
            {
                QualityCategory.Awful => factors.Evaluate(0f),       // 10.0f
                QualityCategory.Poor => factors.Evaluate(1f),        // 8.0f
                QualityCategory.Normal => factors.Evaluate(2f),      // 6.0f
                QualityCategory.Good => factors.Evaluate(3f),        // 4.0f
                QualityCategory.Excellent => factors.Evaluate(4f),   // 2.0f
                QualityCategory.Masterwork => factors.Evaluate(5f),  // 0.5f
                QualityCategory.Legendary => factors.Evaluate(6f),   // 0.0f
                _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, null)
            };
        }
    }
}
