using RimWorld;
using Verse;

namespace HungerGuns
{
    public class ThingDef_HungerBullet : ThingDef
    {
        public float AddHediffChance = 0.9f;
    }

    public class Projectile_HungerBullet : Bullet
    {
        #region Properties
        //
        public ThingDef_HungerBullet Def
        {
            get
            {
                return this.def as ThingDef_HungerBullet;
            }
        }
        #endregion Properties

        #region Overrides
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                var rand = Rand.Value;
                if (rand <= Def.AddHediffChance)
                {
                    Messages.Message("Bullet_HungerBullet_SuccessMessage".Translate(
                        this.launcher.Label, hitPawn.Name
                    ), MessageTypeDefOf.NeutralEvent);

                    Need_Food foodNeed = hitPawn.needs.food;
                    foodNeed.CurLevelPercentage = 0;
                    foodNeed.CurLevel = 0;
                    hitPawn.needs.food = foodNeed;
                    if (!hitPawn.health.hediffSet.HasHediff(HediffDefOf.Malnutrition))
                    {
                        hitPawn?.health?.AddHediff(HediffDefOf.Malnutrition);
                    }

                    hitPawn?.mindState?.priorityWork?.ClearPrioritizedWorkAndJobQueue();
                    hitPawn?.mindState?.mentalStateHandler?.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Binging_Food"));
                }
                else
                {
                    MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "Bullet_HungerBullet_FailureMote".Translate(Def.AddHediffChance), 12f);
                }
            }
        }
        #endregion Overrides
    }
}