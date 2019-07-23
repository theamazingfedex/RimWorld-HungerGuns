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
                if (rand <= hungerTriggerChance)
                {
                    Messages.Message("Bullet_HungerBullet_SuccessMessage".Translate(
                        this.launcher.Label
                    ), MessageTypeDefOf.NeutralEvent);
                    if (!hitPawn.health.hediffSet.HasHediff(HediffDefOf.Malnutrition))
                    {
                        hitPawn?.health?.AddHediff(HediffDefOf.Malnutrition);
                    }
                    if (hitPawn.needs.food.CurLevel > 0)
                    {
                        Need_Food foodNeed = hitPawn.needs.food;
                        foodNeed.CurLevelPercentage = 0;
                        foodNeed.CurLevel = 0;
                        hitPawn.needs.food = foodNeed;
                    }


                    if (shouldCauseCannibalism && Rand.Value <= cannibalismTriggerChance / 100)
                    {
                        var isInRage = hitPawn.jobs.curJob.def == HungerGuns_JobDefOf.CannibalisticRage;
                        var cannibalTrait = new Trait(TraitDefOf.Cannibal, 0, true);

                        if (!isInRage)
                        {
                            Messages.Message("HungerGuns_cannibalismTriggeredMessage".Translate(hitPawn.Label), MessageTypeDefOf.NeutralEvent);
                            hitPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                            hitPawn.ClearAllReservations();
                            hitPawn.jobs.ClearQueuedJobs();
                            if (!hitPawn.story.traits.HasTrait(TraitDefOf.Cannibal))
                            {
                                hitPawn?.story?.traits?.allTraits?.Add(cannibalTrait);
                            }
                            // hitPawn?.mindState?.mentalStateHandler?.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("MurderousRage"));
                            var foodToEat = HungerGuns_Utils.FindNearbyThing(hitPawn, DefDatabase<ThingDef>.GetNamed("Corpse_Human"));
                            if (foodToEat == null)
                            {
                                var personToMurder = HungerGuns_Utils.FindClosestPawn(hitPawn);
                                Job murderJob = new Job(HungerGuns_JobDefOf.HGMurder, personToMurder);
                                // var newJob = new Job(HungerGuns_JobDefOf.CannibalisticRage, foodToEat);
                                murderJob.killIncappedTarget = true;
                                murderJob.maxNumMeleeAttacks = 999;
                                murderJob.attackDoorIfTargetLost = true;
                                hitPawn.jobs.TryTakeOrderedJob(murderJob, new JobTag?(JobTag.SatisfyingNeeds).Value);
                            }
                            // Job cannibalJob = new Job(HungerGuns_JobDefOf.CannibalisticRage, foodToEat);
                            // hitPawn.jobs.TryTakeOrderedJob(cannibalJob, new JobTag?(JobTag.SatisfyingNeeds).Value);
                        }
                        else
                        {
                            Messages.Message($"Already raging bruh! CurJob:: {hitPawn.jobs.curJob.def.label}", MessageTypeDefOf.NeutralEvent);
                            if (holdingOwner != null && holdingOwner.First() is Pawn sourcePawn)
                            {
                            Messages.Message($"{sourcePawn.Label} fired at {hitPawn.Label}", MessageTypeDefOf.NeutralEvent);
                            }
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
    }

    [DefOf]
    public static class HungerGuns_JobDefOf
    {
        public static JobDef CannibalisticRage;
        public static JobDef HGMurder;
    }

    public static class HungerGuns_Utils
    {
        public static Thing FindNearbyThing(Pawn pawn, ThingDef thingDef)
        {
            var cellsAround = GenRadial.RadialCellsAround(pawn.Map.Center, 54, true);
            List<Thing> foundThings = new List<Thing>();
            foreach (var cell in cellsAround)
            {
                var vec = new IntVec3(cell.x, cell.y, cell.z);
                Messages.Message($"Finding thing:: {thingDef.label}", MessageTypeDefOf.NeutralEvent);
                var thing = pawn.Map.thingGrid.ThingAt(vec, thingDef);
                if (thing != null)
                {
                    foundThings.Add(thing);
                }
            }
            if (foundThings.Count() <= 0)
            {
                return null;
            }
            else
            {
                Messages.Message($"Found thing:: {thingDef.label}", MessageTypeDefOf.NeutralEvent);
                var orderedThings = foundThings.OrderBy(thing => pawn.Position - thing.Position);
                return orderedThings.First();
            }
        }
        public static Pawn FindClosestPawn(Pawn pawn)
        {
            System.Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn3 = (Pawn)t;
				return pawn3.Downed
                    && pawn3.RaceProps.Humanlike
                    && pawn.CanReserve(pawn3, 1, -1, null, false)
                    && !pawn3.IsForbidden(pawn);
			};
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.OnCell,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false),
                54f,
                validator,
                null,
                0,
                -1,
                false,
                RegionType.Set_Passable,
                false);
            return pawn2;
        }
    }
}