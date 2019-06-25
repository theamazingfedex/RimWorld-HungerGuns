using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerGuns
{
    public class JobDriver_CannibalisticRage : JobDriver
    {
        private const TargetIndex FoodToEat = TargetIndex.A;
        private const int BreakDuration = 1000;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(FoodToEat), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(FoodToEat);
            this.AddEndCondition(() => {
                this.EndOnDespawnedOrNull(TargetIndex.A);
                this.EndJobWith(JobCondition.Succeeded);
                return JobCondition.Succeeded;
                });


            yield return Toils_Goto.GotoThing(FoodToEat, PathEndMode.Touch);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
            yield return Toils_Interpersonal.GotoInteractablePosition(FoodToEat);

            Toil gotoTarget = Toils_Goto.GotoThing(FoodToEat, PathEndMode.Touch);
            gotoTarget.socialMode = RandomSocialMode.Off;

            Toil eat = Toils_Ingest.ChewIngestible(this.pawn, 1f, FoodToEat);
            eat.socialMode = RandomSocialMode.Off;

            yield return eat;

            // yield return Toils_General.Do(delegate
            // {
            //     Pawn eater = this.pawn;
            //     Thing food = this.pawn.CurJob.targetA.Thing;
            //     food.interactions.TryInteractWith(eater, InteractionDefOf.Nuzzle);
            // });
        }
    }

    public class JobGiver_CannibalisticRage : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Thing food = FindFoodToEat(pawn);

            if (food == null || !pawn.CanReserveAndReach(food, PathEndMode.Touch, Danger.Deadly))
            {
                return null;
            }

            return new Job(HungerGuns_JobDefOf.CannibalisticRage, food);
        }

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

        [DefOf]
        public static class HungerGuns_JobDefOf
        {
            public static JobDef CannibalisticRage;
        }
    }
}