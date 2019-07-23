using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerGuns
{
    public class JobDriver_HGMurder : JobDriver
	{
		// Token: 0x06003B7A RID: 15226 RVA: 0x001BFE8B File Offset: 0x001BE28B
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.numMeleeAttacksMade, "numMeleeAttacksMade", 0, false);
		}

		// Token: 0x06003B7B RID: 15227 RVA: 0x001BFEA8 File Offset: 0x001BE2A8
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			IAttackTarget attackTarget = this.job.targetA.Thing as IAttackTarget;
			if (attackTarget != null)
			{
				this.pawn.Map.attackTargetReservationManager.Reserve(this.pawn, this.job, attackTarget);
			}
			return true;
		}

		// Token: 0x06003B7C RID: 15228 RVA: 0x001BFEF4 File Offset: 0x001BE2F4
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.DoAtomic(delegate
			{
				Pawn pawn = this.job.targetA.Thing as Pawn;
				if (pawn != null && pawn.Downed && this.pawn.Starving())
				{
					this.job.killIncappedTarget = true;
				}
			});
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
			{
				Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
				if (this.pawn.meleeVerbs.TryMeleeAttack(thing, this.job.verbToUse, false))
				{
					if (this.pawn.CurJob == null || this.pawn.jobs.curDriver != this)
					{
						return;
					}
					this.numMeleeAttacksMade++;
					if (((Pawn) thing).health.Dead)
					{
						base.EndJobWith(JobCondition.Succeeded);

                        var foodToEat = HungerGuns_Utils.FindNearbyThing(this.pawn, DefDatabase<ThingDef>.GetNamed("Corpse_Human"));
                        var eatJob = new Job(HungerGuns_JobDefOf.CannibalisticRage, foodToEat);
                        this.pawn.jobs.TryTakeOrderedJob(eatJob, new JobTag?(JobTag.SatisfyingNeeds).Value);

						return;
					}
				}
			}).FailOnDespawnedOrNull(TargetIndex.A);
			yield break;
		}

		// Token: 0x06003B7D RID: 15229 RVA: 0x001BFF18 File Offset: 0x001BE318
		public override void Notify_PatherFailed()
		{
			if (this.job.attackDoorIfTargetLost)
			{
				Thing thing;
				using (PawnPath pawnPath = base.Map.pathFinder.FindPath(this.pawn.Position, base.TargetA.Cell, TraverseParms.For(this.pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
				{
					if (!pawnPath.Found)
					{
						return;
					}
					IntVec3 intVec;
					thing = pawnPath.FirstBlockingBuilding(out intVec, this.pawn);
				}
				if (thing != null)
				{
					this.job.targetA = thing;
					this.job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
					this.job.expiryInterval = Rand.Range(2000, 4000);
					return;
				}
			}
			base.Notify_PatherFailed();
		}

		// Token: 0x06003B7E RID: 15230 RVA: 0x001C0000 File Offset: 0x001BE400
		public override bool IsContinuation(Job j)
		{
			return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}

		// Token: 0x04002637 RID: 9783
		private int numMeleeAttacksMade;
	}
}