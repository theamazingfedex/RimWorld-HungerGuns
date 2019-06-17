using RimWorld;
using Verse;
using static Verse.TranslatorFormattedStringExtensions;

namespace HungerGuns
{
    public class ThingDef_HungerBullet : Verse.ThingDef
    {
        public float AddHediffChance = 0.9f;
        public Verse.HediffDef HediffToAdd = HediffDefOf.Plague;
        public Verse.Def HungerRateMultiplier = StatDefOf.HungerRateMultiplier;
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
        protected override void Impact(Verse.Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def != null && hitThing != null && hitThing is Verse.Pawn hitPawn) //Fancy way to declare a variable inside an if statement. - Thanks Erdelf.
            {
                var rand = Verse.Rand.Value; // This is a random percentage between 0% and 100%
                if (rand <= Def.AddHediffChance) // If the percentage falls under the chance, success!
                {
                    /*
                     * Messages.Message flashes a message on the top of the screen.
                     * You may be familiar with this one when a colonist dies, because
                     * it makes a negative sound and mentioneds "So and so has died of _____".
                     * 
                     * Here, we're using the "Translate" function. More on that later in
                     * the localization section.
                     */
                    var messageLabel = "Bullet_HungerBullet_SuccessMessage";
                    var named = new NamedArgument(new object[] { this.launcher.Label, hitPawn.Label }, messageLabel);
                    var successMessage = new Message(
                        $"{hitPawn.Label} was stricken with Plague!".Translate(named),
                        new MessageTypeDef());
                    Messages.Message(successMessage);
                    //Verse.Messages.Message("Bullet_HungerBullet_SuccessMessage".Translate(new object[]
                    //{
                    //    this.launcher.Label, hitPawn.Label
                    //}), def: MessageSound.Standard);

                    //This checks to see if the character has a heal differential, or hediff on them already.
                    var plagueOnPawn = hitPawn?.health?.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);
                    var randomSeverity = Verse.Rand.Range(0.15f, 0.30f);
                    if (plagueOnPawn != null)
                    {
                        //If they already have plague, add a random range to its severity.
                        //If severity reaches 1.0f, or 100%, plague kills the target.
                        plagueOnPawn.Severity += randomSeverity;
                    }
                    else
                    {
                        //These three lines create a new health differential or Hediff,
                        //put them on the character, and increase its severity by a random amount.
                        //Verse.Hediff hediff = Verse.HediffMaker.MakeHediff(Def.HediffToAdd, hitPawn, null);

                        Pawn_NeedsTracker oldNeeds = hitPawn.needs;
                        var foodNeed = new Need_Food(hitPawn);
                        foodNeed.CurLevelPercentage = 0;
                        foodNeed.CurLevel = 0;
                        oldNeeds.food = foodNeed;
                        hitPawn.needs = oldNeeds;
                        //hediff.Severity = randomSeverity;
                        //hitPawn.health.AddHediff(hediff, null, null);
                    }
                }
                else //failure!
                {
                    /*
                     * Motes handle all the smaller visual effects in RimWorld.
                     * Dust plumes, symbol bubbles, and text messages floating next to characters.
                     * This mote makes a small text message next to the character.
                     */
                    MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "Bullet_HungerBullet_FailureMote".Translate(Def.AddHediffChance), 12f);
                }
            }
        }
        #endregion Overrides
    }
}