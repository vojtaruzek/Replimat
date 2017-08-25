﻿using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Replimat
{
    public class JobGiver_GetFoodReplimat : ThinkNode_JobGiver
    {
        private HungerCategory minCategory;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetFoodReplimat jobGiver_GetFoodReplimat = (JobGiver_GetFoodReplimat)base.DeepCopy(resolve);
            jobGiver_GetFoodReplimat.minCategory = this.minCategory;
            return jobGiver_GetFoodReplimat;
        }

        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null)
            {
                return 0f;
            }
            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                return 0f;
            }
            if (food.CurCategory < this.minCategory)
            {
                return 0f;
            }
            if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
            {
                return 9.5f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null || food.CurCategory < this.minCategory)
            {
                return null;
            }
            bool flag;
            if (pawn.RaceProps.Animal)
            {
                flag = true;
            }
            else
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
                flag = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
            }
            bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
            bool allowCorpse = flag;
            Thing thing;
            ThingDef def;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out def, true, true, false, allowCorpse, false))
            {
                return null;
            }
            Pawn pawn2 = thing as Pawn;
            if (pawn2 != null)
            {
                return new Job(JobDefOf.PredatorHunt, pawn2)
                {
                    killIncappedTarget = true
                };
            }
            Building_ReplimatTerminal building_ReplimatTerminal = thing as Building_ReplimatTerminal;
            if (building_ReplimatTerminal != null)
            {
                Building building = building_ReplimatTerminal;
                thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, FoodPreferability.MealLavish, false, !pawn.IsTeetotaler(), false, false, false, false, false);
                if (thing == null)
                {
                    return null;
                }
                def = thing.def;
            }
            return new Job(JobDefOf.Ingest, thing)
            {
                count = FoodUtility.WillIngestStackCountOf(pawn, def)
            };
        }
    }
}
