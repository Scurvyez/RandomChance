using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace RandomChance
{
    [UsedImplicitly]
    public class RandomProductExtension : DefModExtension
    {
        public float? randomProductChance = 0f;
        public List<RCRandomProductData> randomProducts = [];
        
        public float GetRandomProductWeight(ThingDef productDef)
        {
            foreach (RCRandomProductData productData in randomProducts)
            {
                if (productData.randomProduct.defName == productDef.defName)
                {
                    return productData.randomProductWeight;
                }
            }
            return 0f; // default weight if not found
        }
        
        public FloatRange GetRandomProductSpawnRange(ThingDef productDef)
        {
            foreach (RCRandomProductData productData in randomProducts)
            {
                if (productData.randomProduct.defName == productDef.defName)
                {
                    return productData.randomProductAmountRange;
                }
            }
            return FloatRange.Zero; // default spawn range if not found
        }
    }
}