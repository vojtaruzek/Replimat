﻿using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Replimat
{
    public class Building_ReplimatFeedTank : Building
    {
        public virtual float StoredFeedstockMax => 250f; // 250L capacity for an *insulated* 0.5m diameter and 1.5m high liquid tank
                                                         // Originally 8000L capacity for a 2m diameter and 2m high liquid tank

        public float storedFeedstock;

        public float AmountCanAccept => this.IsBrokenDown() ? 0f : (StoredFeedstockMax - storedFeedstock);

        public float StoredFeedstock => storedFeedstock;
        
        public float StoredFeedstockPct => storedFeedstock / StoredFeedstockMax;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storedFeedstock, "storedFeedstock", 0f, false);
        }

        public void AddFeedstock(float amount)
        {
            if (amount < 0f)
            {
                return;
            }
            if (amount > AmountCanAccept)
            {
                amount = AmountCanAccept;
            }
            storedFeedstock += amount;
        }

        public void DrawFeedstock(float amount)
        {
            storedFeedstock -= amount;
            if (storedFeedstock < 0f)
            {
                storedFeedstock = 0f;
            }
        }

        public void SetStoredFeedstockPct(float pct)
        {
            pct = Mathf.Clamp01(pct);
            storedFeedstock = StoredFeedstockMax * pct;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.Append("FeedstockStored".Translate(storedFeedstock.ToString("0.00"), StoredFeedstockMax.ToString("0.00")));

            if (ParentHolder != null && !(ParentHolder is Map))
            {
                // If minified, don't show computer and feedstock check Inspector messages
            }
            else
            {
                if (!ReplimatUtility.CanFindComputer(this))
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("NotConnectedToComputer".Translate());
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate
                    {
                        SetStoredFeedstockPct(0f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -10L",
                    action = delegate
                    {
                        DrawFeedstock(10f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: -1L",
                    action = delegate
                    {
                        DrawFeedstock(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: +1L",
                    action = delegate
                    {
                        AddFeedstock(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: +10L",
                    action = delegate
                    {
                        AddFeedstock(10f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate
                    {
                        SetStoredFeedstockPct(1f);
                    }
                };
            }
        }
    }
}