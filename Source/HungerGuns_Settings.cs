using RimWorld;
using Verse;
using UnityEngine;

namespace HungerGuns
{
    public class HungerGuns_Settings : ModSettings
    {
        public bool triggerCannibalism = false;
        public static float cannibalismChance = 100f;
        public static float percentChanceToTriggerHunger = 100f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.triggerCannibalism, "triggerCannibalism", false);
            Scribe_Values.Look(ref cannibalismChance, "cannibalismChance", 100);
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
            ls.CheckboxLabeled("HungerGuns_triggerCannibalism".Translate(), ref settings.triggerCannibalism, null);
            ls.Gap(12f);
            ls.Gap(12f);
            var cannibalismLabel = "HungerGuns_cannibalismChance".Translate((HungerGuns_Settings.cannibalismChance / 100).ToStringPercent());
            HungerGuns_Settings.cannibalismChance = Widgets.HorizontalSlider(inRect.TopHalf().TopHalf().TopHalf(), HungerGuns_Settings.cannibalismChance, 0f, 100f, false, cannibalismLabel, "0%", "100%", -1);
            ls.Gap(12f);
            ls.Gap(12f);
            var triggerLabel = "HungerGuns_chanceToTrigger".Translate((HungerGuns_Settings.percentChanceToTriggerHunger / 100).ToStringPercent());
            HungerGuns_Settings.cannibalismChance = Widgets.HorizontalSlider(inRect.TopHalf().TopHalf(), HungerGuns_Settings.cannibalismChance, 0f, 100f, false, triggerLabel, "0%", "100%", -1);
            ls.Gap(12f);

            settings.Write();
            ls.End();
        }
    }
}