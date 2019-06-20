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

            if (def != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                var rand = Rand.Value;
                if (rand <= 1f && !hitPawn.health.hediffSet.HasHediff(HediffDefOf.Malnutrition))
                {
                    Messages.Message("Bullet_HungerBullet_SuccessMessage".Translate(
                        this.launcher.Label
                    ), MessageTypeDefOf.NeutralEvent);

                    Need_Food foodNeed = hitPawn.needs.food;
                    foodNeed.CurLevelPercentage = 0;
                    foodNeed.CurLevel = 0;
                    hitPawn.needs.food = foodNeed;
                    hitPawn?.health?.AddHediff(HediffDefOf.Malnutrition);

                    hitPawn?.mindState?.priorityWork?.ClearPrioritizedWorkAndJobQueue();
                    hitPawn?.mindState?.mentalStateHandler?.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Binging_Food"));
                }
                else
                {
                    MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "Bullet_HungerBullet_FailureMote".Translate(0), 12f);
                }
            }
        }
        #endregion Overrides
    }
}