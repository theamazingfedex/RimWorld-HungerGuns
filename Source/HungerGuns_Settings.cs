using RimWorld;
using Verse;
using UnityEngine;

namespace HungerGuns
{
    public class HungerGuns_Settings : ModSettings
    {
        public static float percentChanceToTriggerHunger = 100f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref percentChanceToTriggerHunger, "percentChanceToTriggerHunger", 100);
        }
    }

    public class HungerGuns_Mod : Mod
    {
        public static HungerGuns_Settings settings;
        public HungerGuns_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<HungerGuns_Settings>();
        }
        public override string SettingsCategory() => "HungerGuns";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.Gap(12f);
            var triggerLabel = "HungerGuns_chanceToTrigger".Translate(((int)(HungerGuns_Settings.percentChanceToTriggerHunger)));
            HungerGuns_Settings.percentChanceToTriggerHunger = Widgets.HorizontalSlider(inRect.TopHalf().TopHalf(), HungerGuns_Settings.percentChanceToTriggerHunger, 0f, 100f, false, triggerLabel, "0%", "100%", -1);
            ls.Gap(12f);

            settings.Write();
            ls.End();
        }
    }
}