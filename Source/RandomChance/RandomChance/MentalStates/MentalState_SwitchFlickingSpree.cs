using RimWorld;
using Verse.AI;

namespace RandomChance
{
    public class MentalState_SwitchFlickingSpree : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}
