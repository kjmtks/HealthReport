using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NCVC.Parser;

namespace NCVC.App.Models
{
    public class FilterCompiler
    {
        private Microsoft.FSharp.Core.FSharpOption<Tuple<Microsoft.FSharp.Collections.FSharpList<QueryParser.BooleanExpr>, Microsoft.FSharp.Core.FSharpOption<Microsoft.FSharp.Collections.FSharpList<Tuple<QueryParser.SortingAttribute, QueryParser.SortingOrder>>>>> query;

        public FilterCompiler(string query_string)
        {
            query = QueryParser.ParseQuery(query_string);
        }
        public bool IncludeToday()
        {
            if(query?.Value == null)
            {
                return false;
            }
            var dummy = new Health()
            {
                MeasuredAt = DateTime.Today,
            };
            foreach (var cndt in query.Value.Item1)
            {
                if(!evalBooleanExpr(dummy, cndt))
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerable<QueryParser.Reference> GetReferences()
        {
            if (query?.Value == null)
            {
                return null;
            }
            var rs = new List<QueryParser.Reference>();
            foreach (var condt in query?.Value?.Item1)
            {
                foreach (var rf in QueryParser.GetReferences(condt))
                {
                    rs.Add(rf);
                }
            }
            return rs;
        }

        public IEnumerable<Health> Filtering(IEnumerable<Health> healthList)
        {
            if (query?.Value == null)
            {
                return healthList;
            }
            foreach (var cndt in query.Value.Item1)
            {
                healthList = healthList.Where(health => evalBooleanExpr(health, cndt));
            }
            return healthList;
        }

        public IEnumerable<Health> Sort(IEnumerable<Health> healthList)
        {
            if (query?.Value?.Item2?.Value == null)
            {
                return healthList;
            }

            IOrderedEnumerable<Health> OrderedHealthList = null;

            foreach (var order in query.Value.Item2.Value)
            {
                if(OrderedHealthList == null)
                {
                    if (order.Item2.IsAsc)
                    {
                        OrderedHealthList = healthList.OrderBy(health => ordering(health, order.Item1));
                    }
                    else
                    {
                        OrderedHealthList = healthList.OrderByDescending(health => ordering(health, order.Item1));
                    }
                }
                else
                {
                    if (order.Item2.IsAsc)
                    {
                        OrderedHealthList = OrderedHealthList.ThenBy(health => ordering(health, order.Item1));
                    }
                    else
                    {
                        OrderedHealthList = OrderedHealthList.ThenByDescending(health => ordering(health, order.Item1));
                    }
                }

            }
            return (OrderedHealthList ?? healthList)?.ToArray();
        }

        //---

        private object ordering(Health health, QueryParser.SortingAttribute attr)
        {
            if(attr.IsHasError)
            {
                return health.HasWrongValue();
            }
            if (attr.IsHasWarning)
            {
                return health.HasWarnValue();
            }
            if (attr.IsBodyTemperature)
            {
                return health.BodyTemperature;
            }
            if (attr.IsIsInfected)
            {
                return health.IsInfected;
            }
            if (attr.IsIsSubmitted)
            {
                return !health.IsEmptyData;
            }
            if (attr.IsMeasuredDate)
            {
                return health.MeasuredAt;
            }
            if (attr.IsTimeFrame)
            {
                return health.TimeFrame;
            }
            if (attr.IsUserId)
            {
                return health?.Student?.Account ?? "";
            }
            throw new NotImplementedException();
        }

        private enum StringMatchMethod { Equals, FirstMatch, HasOne }
        private enum DateTimeSpan { Days, Months }

        private (string, StringMatchMethod) evalStringAtom(Health health, QueryParser.StringAtom atom)
        {
            if (atom.IsTimeFrame)
            {
                return (health.TimeFrame, StringMatchMethod.Equals);
            }
            if (atom.IsUserId)
            {
                return (health.Student?.Account ?? "", StringMatchMethod.FirstMatch);
            }
            if (atom.IsHasTag)
            {
                return (health.Student?.Tags ?? "", StringMatchMethod.HasOne);
            }
            if (atom is QueryParser.StringAtom.StringLiteral literal)
            {
                return (literal.Item, StringMatchMethod.Equals);
            }
            throw new NotImplementedException();
        }
        private (string, StringMatchMethod) evalStringExpr(Health health, QueryParser.StringExpr expr)
        {
            if (expr.Item is QueryParser.StringAtom atom)
            {
                return evalStringAtom(health, atom);
            }
            throw new NotImplementedException();
        }


        private decimal evalDecimalAtom(Health health, QueryParser.DecimalAtom atom)
        {
            if (atom.IsBodyTemperature)
            {
                return health.BodyTemperature;
            }
            if (atom is QueryParser.DecimalAtom.DecimalLiteral literal)
            {
                return literal.Item;
            }
            throw new NotImplementedException();
        }
        private decimal evalDecimalExpr(Health health, QueryParser.DecimalExpr expr)
        {
            if (expr is QueryParser.DecimalExpr.DecimalAtom atom)
            {
                return evalDecimalAtom(health, atom.Item);
            }
            if(expr is QueryParser.DecimalExpr.Sum sum)
            {
                return evalDecimalExpr(health, sum.Item1) + evalDecimalExpr(health, sum.Item2);
            }
            if (expr is QueryParser.DecimalExpr.Sub sub)
            {
                return evalDecimalExpr(health, sub.Item1) - evalDecimalExpr(health, sub.Item2);
            }
            if (expr is QueryParser.DecimalExpr.Mul mul)
            {
                return evalDecimalExpr(health, mul.Item1) * evalDecimalExpr(health, mul.Item2);
            }
            if (expr is QueryParser.DecimalExpr.Div div)
            {
                return evalDecimalExpr(health, div.Item1) / evalDecimalExpr(health, div.Item2);
            }
            if (expr is QueryParser.DecimalExpr.Minus minus)
            {
                return -evalDecimalExpr(health, minus.Item);
            }
            throw new NotImplementedException();
        }


        private (DateTime, DateTimeSpan) evalDateAtom(Health health, QueryParser.DateAtom atom)
        {
            if (atom.IsMeasuredDate)
            {
                return (health.MeasuredAt, DateTimeSpan.Days);
            }
            if (atom is QueryParser.DateAtom.DateLiteral literal)
            {
                return (new DateTime(literal.Item1, literal.Item2, literal.Item3), DateTimeSpan.Days);
            }
            if (atom.IsToday)
            {
                return (DateTime.Today, DateTimeSpan.Days);
            }
            if (atom.IsThisWeek)
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var start = today.AddDays(-1 * diff).Date;
                return (start, DateTimeSpan.Days);
            }
            if (atom.IsThisMonth)
            {
                var today = DateTime.Today;
                var start = new DateTime(today.Year, today.Month, 1);
                return (start, DateTimeSpan.Months);
            }
            throw new NotImplementedException();
        }
        private (DateTime, DateTimeSpan) evalDateExpr(Health health, QueryParser.DateExpr expr)
        {
            if (expr is QueryParser.DateExpr.DateAtom atom)
            {
                return evalDateAtom(health, atom.Item);
            }
            if (expr is QueryParser.DateExpr.AddDate add)
            {
                var inc = (int)evalDecimalExpr(health, add.Item2);
                var date = evalDateExpr(health, add.Item1);
                switch (date.Item2)
                {
                    case DateTimeSpan.Days:
                        return (date.Item1.AddDays(inc), DateTimeSpan.Days);
                    case DateTimeSpan.Months:
                        return (date.Item1.AddMonths(inc), DateTimeSpan.Months);
                }
            }
            if (expr is QueryParser.DateExpr.ReduceDate red)
            {
                var dec = (int)evalDecimalExpr(health, red.Item2);
                var date = evalDateExpr(health, red.Item1);
                switch (date.Item2)
                {
                    case DateTimeSpan.Days:
                        return (date.Item1.AddDays(-dec), DateTimeSpan.Days);
                    case DateTimeSpan.Months:
                        return (date.Item1.AddMonths(-dec), DateTimeSpan.Months);
                }
            }
            throw new NotImplementedException();
        }


        private bool evalBooleanAtom(Health health, QueryParser.BooleanAtom atom)
        {
            if (atom.IsHasError)
            {
                return health.HasWrongValue();
            }
            if (atom.IsHasWarning)
            {
                return health.HasWarnValue();
            }
            if (atom.IsIsSubmitted)
            {
                return !health.IsEmptyData;
            }
            if (atom.IsIsInfected)
            {
                return health.IsInfected;
            }
            if (atom.IsTrue)
            {
                return true;
            }
            if (atom.IsFalse)
            {
                return false;
            }

            if (atom is QueryParser.BooleanAtom.SEq seq)
            {
                var lhs = evalStringExpr(health, seq.Item1);
                var rhs = evalStringExpr(health, seq.Item2);
                switch( (lhs.Item2, rhs.Item2) )
                {
                    case (StringMatchMethod.Equals, StringMatchMethod.Equals):
                        return lhs.Item1 == rhs.Item1;
                    case (StringMatchMethod.Equals, StringMatchMethod.FirstMatch):
                        return rhs.Item1.StartsWith(lhs.Item1);
                    case (StringMatchMethod.Equals, StringMatchMethod.HasOne):
                        return rhs.Item1?.Split()?.Contains(lhs.Item1) ?? false;
                    case (StringMatchMethod.FirstMatch, StringMatchMethod.Equals):
                        return lhs.Item1.StartsWith(rhs.Item1);
                    case (StringMatchMethod.FirstMatch, StringMatchMethod.FirstMatch):
                        return lhs.Item1.StartsWith(rhs.Item1) || rhs.Item1.StartsWith(lhs.Item1);
                    case (StringMatchMethod.FirstMatch, StringMatchMethod.HasOne):
                        return rhs.Item1?.Split()?.Any(x => x.StartsWith(lhs.Item1)) ?? false;
                    case (StringMatchMethod.HasOne, StringMatchMethod.Equals):
                        return lhs.Item1?.Split()?.Contains(rhs.Item1) ?? false;
                    case (StringMatchMethod.HasOne, StringMatchMethod.FirstMatch):
                        return lhs.Item1?.Split()?.Any(x => x.StartsWith(rhs.Item1)) ?? false;
                    case (StringMatchMethod.HasOne, StringMatchMethod.HasOne):
                        return rhs.Item1?.Split().Intersect(rhs.Item1?.Split() ?? new string[0] { })?.Count() > 0;
                }
            }
            if (atom is QueryParser.BooleanAtom.SNe sne)
            {
                return !evalBooleanAtom(health, QueryParser.BooleanAtom.NewSEq(sne.Item1, sne.Item2));
            }

            if (atom is QueryParser.BooleanAtom.DeEq deeq)
            {
                return evalDecimalExpr(health, deeq.Item1) == evalDecimalExpr(health, deeq.Item2);
            }
            if (atom is QueryParser.BooleanAtom.DeNe dene)
            {
                return evalDecimalExpr(health, dene.Item1) != evalDecimalExpr(health, dene.Item2);
            }
            if (atom is QueryParser.BooleanAtom.DeGt degt)
            {
                return evalDecimalExpr(health, degt.Item1) > evalDecimalExpr(health, degt.Item2);
            }
            if (atom is QueryParser.BooleanAtom.DeGe dege)
            {
                return evalDecimalExpr(health, dege.Item1) >= evalDecimalExpr(health, dege.Item2);
            }
            if (atom is QueryParser.BooleanAtom.DeLt delt)
            {
                return evalDecimalExpr(health, delt.Item1) < evalDecimalExpr(health, delt.Item2);
            }
            if (atom is QueryParser.BooleanAtom.DeLe dele)
            {
                return evalDecimalExpr(health, dele.Item1) <= evalDecimalExpr(health, dele.Item2);
            }

            if (atom is QueryParser.BooleanAtom.DtEq dteq)
            {
                return evalDateExpr(health, dteq.Item1).Item1 == evalDateExpr(health, dteq.Item2).Item1;
            }
            if (atom is QueryParser.BooleanAtom.DtNe dtne)
            {
                return evalDateExpr(health, dtne.Item1).Item1 != evalDateExpr(health, dtne.Item2).Item1;
            }
            if (atom is QueryParser.BooleanAtom.DtGt dtgt)
            {
                return evalDateExpr(health, dtgt.Item1).Item1 > evalDateExpr(health, dtgt.Item2).Item1;
            }
            if (atom is QueryParser.BooleanAtom.DtGe dtge)
            {
                return evalDateExpr(health, dtge.Item1).Item1 >= evalDateExpr(health, dtge.Item2).Item1;
            }
            if (atom is QueryParser.BooleanAtom.DtLt dtlt)
            {
                return evalDateExpr(health, dtlt.Item1).Item1 < evalDateExpr(health, dtlt.Item2).Item1;
            }
            if (atom is QueryParser.BooleanAtom.DtLe dtle)
            {
                return evalDateExpr(health, dtle.Item1).Item1 <= evalDateExpr(health, dtle.Item2).Item1;
            }

            throw new NotImplementedException();
        }
        private bool evalBooleanExpr(Health health, QueryParser.BooleanExpr expr)
        {
            if (expr is QueryParser.BooleanExpr.BooleanAtom atom)
            {
                return evalBooleanAtom(health, atom.Item);
            }
            if (expr is QueryParser.BooleanExpr.Eq eq)
            {
                return evalBooleanExpr(health, eq.Item1) == evalBooleanExpr(health, eq.Item2);
            }
            if (expr is QueryParser.BooleanExpr.Ne ne)
            {
                return evalBooleanExpr(health, ne.Item1) != evalBooleanExpr(health, ne.Item2);
            }
            if (expr is QueryParser.BooleanExpr.Neg neg)
            {
                return !evalBooleanExpr(health, neg.Item);
            }
            if (expr is QueryParser.BooleanExpr.Conj conj)
            {
                return evalBooleanExpr(health, conj.Item1) && evalBooleanExpr(health, conj.Item2);
            }
            if (expr is QueryParser.BooleanExpr.Disj disj)
            {
                return evalBooleanExpr(health, disj.Item1) || evalBooleanExpr(health, disj.Item2);
            }
            if (expr is QueryParser.BooleanExpr.Impl impl)
            {
                return !evalBooleanExpr(health, impl.Item1) || evalBooleanExpr(health, impl.Item2);
            }
            throw new NotImplementedException();
        }

    }
}
