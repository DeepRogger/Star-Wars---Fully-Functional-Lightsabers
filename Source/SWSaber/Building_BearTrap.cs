﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse;

namespace Test
{
    public class Building_BearTrap : Building_Trap
    {
        private bool autoRearm;

        private bool armedInt = true;

        private Graphic graphicUnarmedInt;

        private static readonly FloatRange TrapDamageFactor = new FloatRange(0.7f, 1.3f);

        private static readonly IntRange DamageCount = new IntRange(1, 2);

        public override bool Armed
        {
            get
            {
                return this.armedInt;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (this.armedInt)
                {
                    return base.Graphic;
                }
                if (this.graphicUnarmedInt == null)
                {
                    this.graphicUnarmedInt = this.def.building.trapUnarmedGraphicData.GraphicColoredFor(this);
                }
                return this.graphicUnarmedInt;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.armedInt, "armed", false, false);
            Scribe_Values.Look<bool>(ref this.autoRearm, "autoRearm", false, false);
        }

        protected override void SpringSub(Pawn p)
        {
            this.armedInt = false;
            if (p != null)
            {
                this.DamagePawn(p);
            }
            if (this.autoRearm)
            {
                base.Map.designationManager.AddDesignation(new Designation(this, DesignationDefOf.RearmTrap));
            }
        }

        public void Rearm()
        {
            this.armedInt = true;
            SoundDef.Named("TrapArm").PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        private void DamagePawn(Pawn p)
        {
            BodyPartHeight height = (Rand.Value >= 0.666f) ? BodyPartHeight.Middle : BodyPartHeight.Top;
            int num = Mathf.RoundToInt(this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * Building_BearTrap.TrapDamageFactor.RandomInRange);
            int randomInRange = Building_BearTrap.DamageCount.RandomInRange;
            for (int i = 0; i < randomInRange; i++)
            {
                if (num <= 0)
                {
                    break;
                }
                int num2 = Mathf.Max(1, Mathf.RoundToInt(Rand.Value * (float)num));
                num -= num2;
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, num2, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                dinfo.SetBodyRegion(height, BodyPartDepth.Outside);
                p.TakeDamage(dinfo);
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Toggle
            {
                defaultLabel = "CommandAutoRearm".Translate(),
                defaultDesc = "CommandAutoRearmDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc3,
                icon = TexCommand.RearmTrap,
                isActive = (() => this.autoRearm),
                toggleAction = delegate
                {
                    this.autoRearm = !this.autoRearm;
                }
            };
        }
    }
}
