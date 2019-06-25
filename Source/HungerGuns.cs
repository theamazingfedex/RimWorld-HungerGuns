using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

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
                    var cannibalTrait = new Trait(TraitDefOf.Cannibal, 0, true);

                    if (shouldCauseCannibalism && Rand.Value <= cannibalismTriggerChance / 100)
                    {
                        var isInRage = hitPawn.jobs.curJob.def == HungerGuns_JobDefOf.CannibalisticRage;
                        if (!isInRage)
                        {
                            Messages.Message("HungerGuns_cannibalismTriggeredMessage".Translate(hitPawn.Label), MessageTypeDefOf.NeutralEvent);
                            hitPawn.ClearAllReservations();
                            hitPawn.jobs.ClearQueuedJobs();
                            hitPawn?.story?.traits?.allTraits?.Add(cannibalTrait);
                            // hitPawn?.mindState?.mentalStateHandler?.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("MurderousRage"));
                            var foodToEat = FindFoodToEat(hitPawn);
                            if (foodToEat != null)
                            {
                                // Go beat someone to death, to create a corpse
                            }
                            var newJob = new Job(HungerGuns_JobDefOf.CannibalisticRage, foodToEat);
                            hitPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                            hitPawn.jobs.TryTakeOrderedJob(newJob, new JobTag?(JobTag.SatisfyingNeeds).Value);
                        }
                        else
                        {
                            Messages.Message($"Already raging bruh! CurJob:: {hitPawn.jobs.curJob.def.label}", MessageTypeDefOf.NeutralEvent);
                        }
                    }
                    else
                    {
                        // hitPawn?.story?.traits?.allTraits?.Remove(cannibalTrait);
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
        private Thing FindFoodToEat(Pawn pawn)
        {
            var corpseDef = DefDatabase<ThingDef>.GetNamed("Corpse_Human");
            // var corpses = pawn.Map.listerThings.ThingsOfDef(corpseDef);
            List<Thing> corpses = new List<Thing>();
            var cellsAround = GenRadial.RadialCellsAround(pawn.Map.Center, 54, true);
            foreach (var cell in cellsAround)
            {
                var vec = new IntVec3(cell.x, cell.y, cell.z);
                var thing = pawn.Map.thingGrid.ThingAt(vec, corpseDef);
                if (thing != null)
                {
                    corpses.Add(thing);
                }
            }
            corpses.OrderBy(x => pawn.Position - x.Position);
            Messages.Message($"FoundCorpse:: {corpses[0].def.label}", MessageTypeDefOf.NeutralEvent);
            return corpses.First();
        }
    }
    [DefOf]
    public static class HungerGuns_JobDefOf
    {
        public static JobDef CannibalisticRage;
    }
}