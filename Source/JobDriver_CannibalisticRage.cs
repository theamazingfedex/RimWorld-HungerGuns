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
        }
    }
}