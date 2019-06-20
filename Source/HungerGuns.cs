using RimWorld;
using Verse;

namespace HungerGuns
{
    public class Projectile_HungerBullet : Bullet
    {
        #region Overrides
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (this.def != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                var rand = Rand.Value;
                var shouldCauseCannibalism = LoadedModManager.GetMod<HungerGuns_Mod>().GetSettings<HungerGuns_Settings>().triggerCannibalism;
                var cannibalismTriggerChance = HungerGuns_Settings.cannibalismChance;
                var hungerTriggerChance = HungerGuns_Settings.percentChanceToTriggerHunger;
                if (rand <= hungerTriggerChance && !hitPawn.health.hediffSet.HasHediff(HediffDefOf.Malnutrition))
                {
                    Messages.Message("Bullet_HungerBullet_SuccessMessage".Translate(
                        this.launcher.Label
                    ), MessageTypeDefOf.NeutralEvent);

                    Need_Food foodNeed = hitPawn.needs.food;
                    foodNeed.CurLevelPercentage = 0;
                    foodNeed.CurLevel = 0;
                    hitPawn.needs.food = foodNeed;
                    hitPawn?.health?.AddHediff(HediffDefOf.Malnutrition);

                    if (shouldCauseCannibalism && Rand.Value <= cannibalismTriggerChance / 100)
                    {
                        Messages.Message("HungerGuns_cannibalismTriggeredMessage".Translate(hitPawn.Label), MessageTypeDefOf.NeutralEvent);
                        hitPawn?.story?.traits?.allTraits?.Add(new Trait(TraitDefOf.Cannibal, 0, true));
                        hitPawn?.mindState?.mentalStateHandler?.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("MurderousRage"));
                    }
                    else
                    {
                        Messages.Message("Bullet_HungerBullet_SuccessMessage".Translate(hitPawn.Label), MessageTypeDefOf.NeutralEvent);
                        hitPawn?.mindState?.mentalStateHandler?.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Binging_Food"));
                    }
                }
                else
                {
                    MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "Bullet_HungerBullet_FailureMote".Translate(hungerTriggerChance), 12f);
                }
            }
        }
        #endregion Overrides
    }
}