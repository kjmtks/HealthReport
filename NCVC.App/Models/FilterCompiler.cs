﻿using Microsoft.EntityFrameworkCore;
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
        private enum StringMatchMethod { Equals, FirstMatch, HasOne }
        private enum DateTimeSpan { Days, Months }

        private Microsoft.FSharp.Core.FSharpOption<Tuple<Microsoft.FSharp.Collections.FSharpList<QueryParser.BooleanExpr>, Microsoft.FSharp.Core.FSharpOption<Microsoft.FSharp.Collections.FSharpList<Tuple<QueryParser.SortingAttribute, QueryParser.SortingOrder>>>>> query;

        public FilterCompiler(string query_string)
        {
            query = QueryParser.ParseQuery(query_string);
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

        public IQueryable<Health> Filtering(DatabaseContext context)
        {
            if (query == null)
            {
                return null;
            }
            var sql = new System.Text.StringBuilder();
            sql.Append(@"SELECT 
    h.""Id"" AS ""Id"",
    h.""BodyTemperature"" AS ""BodyTemperature"",
    h.""InfectedBodyTemperature1"" AS ""InfectedBodyTemperature1"",
    h.""InfectedBodyTemperature2"" AS ""InfectedBodyTemperature2"",
    h.""InfectedMeasuredTime1"" AS ""InfectedMeasuredTime1"",
    h.""InfectedMeasuredTime2"" AS ""InfectedMeasuredTime2"",
    h.""InfectedOxygenSaturation1"" AS ""InfectedOxygenSaturation1"",
    h.""InfectedOxygenSaturation2"" AS ""InfectedOxygenSaturation2"",
    h.""InfectedStringColumn1"" AS ""InfectedStringColumn1"",
    h.""InfectedStringColumn2"" AS ""InfectedStringColumn2"",
    h.""InfectedStringColumn3"" AS ""InfectedStringColumn3"",
    h.""InfectedStringColumn4"" AS ""InfectedStringColumn4"",
    h.""InfectedStringColumn5"" AS ""InfectedStringColumn5"",
    h.""InfectedStringColumn6"" AS ""InfectedStringColumn6"",
    h.""InfectedStringColumn7"" AS ""InfectedStringColumn7"",
    h.""InfectedStringColumn8"" AS ""InfectedStringColumn8"",
    h.""InfectedStringColumn9"" AS ""InfectedStringColumn9"",
    h.""InfectedStringColumn10"" AS ""InfectedStringColumn10"",
    h.""IsEmptyData"" AS ""IsEmptyData"",
    h.""IsInfected"" AS ""IsInfected"",
    h.""MailIndex"" AS ""MailIndex"",
    h.""MeasuredAt"" AS ""MeasuredAt"",
    h.""RawUserId"" AS ""RawUserId"",
    h.""RawUserName"" AS ""RawUserName"",
    h.""StringColumn1"" AS ""StringColumn1"",
    h.""StringColumn2"" AS ""StringColumn2"",
    h.""StringColumn3"" AS ""StringColumn3"",
    h.""StringColumn4"" AS ""StringColumn4"",
    h.""StringColumn5"" AS ""StringColumn5"",
    h.""StringColumn6"" AS ""StringColumn6"",
    h.""StringColumn7"" AS ""StringColumn7"",
    h.""StringColumn8"" AS ""StringColumn8"",
    h.""StringColumn9"" AS ""StringColumn9"",
    h.""StringColumn10"" AS ""StringColumn10"",
    h.""StringColumn11"" AS ""StringColumn11"",
    h.""StringColumn12"" AS ""StringColumn12"",
    h.""StudentId"" AS ""StudentId"",
    h.""TimeFrame"" AS ""TimeFrame"",
    h.""UploadedAt"" AS ""UploadedAt""
FROM ""Health"" AS h INNER JOIN ""Student"" AS s ON h.""StudentId"" = s.""Id""");

            var where = string.Join(" AND ", query.Value.Item1.Select(x => toSqlBooleanExpr(x)));
            if(!string.IsNullOrWhiteSpace(where))
            {
                sql.Append(" WHERE ");
                sql.Append(where);
            }
            return Sort(context.HealthList.FromSqlRaw(sql.ToString()).Include(x => x.Student));
        }

        //---

        public IQueryable<Health> Sort(IQueryable<Health> healthList)
        {
            var orderings = new List<(QueryParser.SortingAttribute, QueryParser.SortingOrder)>();

            if (query?.Value?.Item2?.Value != null)
            {
                foreach(var x in (query.Value.Item2.Value.Select(x => (x.Item1, x.Item2))))
                {
                    orderings.Add(x);
                }
            }
            if(!orderings.Any(x => x.Item1 == QueryParser.SortingAttribute.MeasuredDate))
            {
                orderings.Add((QueryParser.SortingAttribute.MeasuredDate, QueryParser.SortingOrder.Asc));
            }
            if (!orderings.Any(x => x.Item1 == QueryParser.SortingAttribute.TimeFrame))
            {
                orderings.Add((QueryParser.SortingAttribute.TimeFrame, QueryParser.SortingOrder.Asc));
            }
            if (!orderings.Any(x => x.Item1 == QueryParser.SortingAttribute.UserId))
            {
                orderings.Add((QueryParser.SortingAttribute.UserId, QueryParser.SortingOrder.Asc));
            }            

            IOrderedQueryable<Health> OrderedHealthList = null;

            foreach (var order in orderings)
            {
                if (OrderedHealthList == null)
                {
                    if (order.Item2.IsAsc)
                    {
                        if (order.Item1.IsBodyTemperature)
                        {
                            OrderedHealthList = healthList.OrderBy(health => health.BodyTemperature);
                        }
                        if (order.Item1.IsIsInfected)
                        {
                            OrderedHealthList = healthList.OrderBy(health => health.IsInfected);
                        }
                        if (order.Item1.IsIsSubmitted)
                        {
                            OrderedHealthList = healthList.OrderBy(health => !health.IsEmptyData);
                        }
                        if (order.Item1.IsMeasuredDate)
                        {
                            OrderedHealthList = healthList.OrderBy(health => health.MeasuredAt);
                        }
                        if (order.Item1.IsTimeFrame)
                        {
                            OrderedHealthList = healthList.OrderBy(health => health.TimeFrame);
                        }
                        if (order.Item1.IsUserId)
                        {
                            OrderedHealthList = healthList.OrderBy(health => health.Student.Account);
                        }
                    }
                    else
                    {
                        if (order.Item1.IsBodyTemperature)
                        {
                            OrderedHealthList = healthList.OrderByDescending(health => health.BodyTemperature);
                        }
                        if (order.Item1.IsIsInfected)
                        {
                            OrderedHealthList = healthList.OrderByDescending(health => health.IsInfected);
                        }
                        if (order.Item1.IsIsSubmitted)
                        {
                            OrderedHealthList = healthList.OrderByDescending(health => !health.IsEmptyData);
                        }
                        if (order.Item1.IsMeasuredDate)
                        {
                            OrderedHealthList = healthList.OrderByDescending(health => health.MeasuredAt);
                        }
                        if (order.Item1.IsTimeFrame)
                        {
                            OrderedHealthList = healthList.OrderByDescending(health => health.TimeFrame);
                        }
                        if (order.Item1.IsUserId)
                        {
                            OrderedHealthList = healthList.OrderByDescending(health => health.Student.Account);
                        }
                    }
                }
                else
                {
                    if (order.Item2.IsAsc)
                    {
                        if (order.Item1.IsBodyTemperature)
                        {
                            OrderedHealthList = OrderedHealthList.ThenBy(health => health.BodyTemperature);
                        }
                        if (order.Item1.IsIsInfected)
                        {
                            OrderedHealthList = OrderedHealthList.ThenBy(health => health.IsInfected);
                        }
                        if (order.Item1.IsIsSubmitted)
                        {
                            OrderedHealthList = OrderedHealthList.ThenBy(health => !health.IsEmptyData);
                        }
                        if (order.Item1.IsMeasuredDate)
                        {
                            OrderedHealthList = OrderedHealthList.ThenBy(health => health.MeasuredAt);
                        }
                        if (order.Item1.IsTimeFrame)
                        {
                            OrderedHealthList = OrderedHealthList.ThenBy(health => health.TimeFrame);
                        }
                        if (order.Item1.IsUserId)
                        {
                            OrderedHealthList = OrderedHealthList.ThenBy(health => health.Student.Account);
                        }
                    }
                    else
                    {
                        if (order.Item1.IsBodyTemperature)
                        {
                            OrderedHealthList = OrderedHealthList.ThenByDescending(health => health.BodyTemperature);
                        }
                        if (order.Item1.IsIsInfected)
                        {
                            OrderedHealthList = OrderedHealthList.ThenByDescending(health => health.IsInfected);
                        }
                        if (order.Item1.IsIsSubmitted)
                        {
                            OrderedHealthList = OrderedHealthList.ThenByDescending(health => !health.IsEmptyData);
                        }
                        if (order.Item1.IsMeasuredDate)
                        {
                            OrderedHealthList = OrderedHealthList.ThenByDescending(health => health.MeasuredAt);
                        }
                        if (order.Item1.IsTimeFrame)
                        {
                            OrderedHealthList = OrderedHealthList.ThenByDescending(health => health.TimeFrame);
                        }
                        if (order.Item1.IsUserId)
                        {
                            OrderedHealthList = OrderedHealthList.ThenByDescending(health => health.Student.Account);
                        }
                    }
                }

            }
            return (OrderedHealthList ?? healthList);
        }

        private object ordering(Health health, QueryParser.SortingAttribute attr)
        {
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

        //---

        private static (string, StringMatchMethod) toSqlStringAtom(QueryParser.StringAtom atom)
        {
            if (atom.IsTimeFrame)
            {
                return ("h.\"TimeFrame\"", StringMatchMethod.Equals);
            }
            if (atom.IsUserId)
            {
                return ("s.\"Account\"", StringMatchMethod.FirstMatch);
            }
            if (atom.IsHasTag)
            {
                return ("(' ' || s.\"Tags\" || ' ')", StringMatchMethod.HasOne);
            }
            if (atom is QueryParser.StringAtom.StringLiteral literal)
            {
                return ($"'{literal.Item.Replace('\'', ' ')}'", StringMatchMethod.Equals);
            }
            throw new NotImplementedException();
        }
        private static (string, StringMatchMethod) toSqlStringExpr(QueryParser.StringExpr expr)
        {
            if (expr.Item is QueryParser.StringAtom atom)
            {
                return toSqlStringAtom(atom);
            }
            throw new NotImplementedException();
        }


        private static string toSqlDecimalAtom(QueryParser.DecimalAtom atom)
        {
            if (atom.IsBodyTemperature)
            {
                return "h.\"BodyTemperature\"";
            }
            if (atom is QueryParser.DecimalAtom.DecimalLiteral literal)
            {
                return $"{literal.Item}";
            }
            throw new NotImplementedException();
        }
        private static string toSqlDecimalExpr(QueryParser.DecimalExpr expr)
        {
            if (expr is QueryParser.DecimalExpr.DecimalAtom atom)
            {
                return $"{toSqlDecimalAtom(atom.Item)}";
            }
            if (expr is QueryParser.DecimalExpr.Sum sum)
            {
                return $"({toSqlDecimalExpr(sum.Item1)} + {toSqlDecimalExpr(sum.Item2)})";
            }
            if (expr is QueryParser.DecimalExpr.Sub sub)
            {
                return $"({toSqlDecimalExpr(sub.Item1)} - {toSqlDecimalExpr(sub.Item2)})";
            }
            if (expr is QueryParser.DecimalExpr.Mul mul)
            {
                return $"({toSqlDecimalExpr(mul.Item1)} * {toSqlDecimalExpr(mul.Item2)})";
            }
            if (expr is QueryParser.DecimalExpr.Div div)
            {
                return $"({toSqlDecimalExpr(div.Item1)} / {toSqlDecimalExpr(div.Item2)})";
            }
            if (expr is QueryParser.DecimalExpr.Minus minus)
            {
                return $"(-{ toSqlDecimalExpr(minus.Item)})";
            }
            throw new NotImplementedException();
        }


        private static (string, DateTimeSpan) toSqlDateAtom(QueryParser.DateAtom atom)
        {
            if (atom.IsMeasuredDate)
            {
                return ("h.\"MeasuredAt\"", DateTimeSpan.Days);
            }
            if (atom is QueryParser.DateAtom.DateLiteral literal)
            {
                return (string.Format("(date '{0:00}-{1:00}-{2:00} 00:00:00+09')", literal.Item1, literal.Item2,literal.Item3), DateTimeSpan.Days);
            }
            if (atom.IsToday)
            {
                return (string.Format("(date '{0:00}-{1:00}-{2:00} 00:00:00+09')", DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day), DateTimeSpan.Days);
            }
            if (atom.IsThisWeek)
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var start = today.AddDays(-1 * diff).Date;
                return (string.Format("(date '{0:00}-{1:00}-{2:00} 00:00:00+09')", start.Year, start.Month, start.Day), DateTimeSpan.Days);
            }
            if (atom.IsThisMonth)
            {
                var today = DateTime.Today;
                var start = new DateTime(today.Year, today.Month, 1);
                return (string.Format("(date '{0:00}-{1:00}-{2:00} 00:00:00+09')", start.Year, start.Month, start.Day), DateTimeSpan.Months);
            }
            throw new NotImplementedException();
        }
        private static (string, DateTimeSpan) toSqlDateExpr(QueryParser.DateExpr expr)
        {
            if (expr is QueryParser.DateExpr.DateAtom atom)
            {
                return toSqlDateAtom(atom.Item);
            }
            if (expr is QueryParser.DateExpr.AddDate add)
            {
                var inc = toSqlDecimalExpr(add.Item2);
                var date = toSqlDateExpr(add.Item1);
                switch (date.Item2)
                {
                    case DateTimeSpan.Days:
                        return ($"({date.Item1} + (interval '{inc} day'))", DateTimeSpan.Days);
                    case DateTimeSpan.Months:
                        return ($"({date.Item1} + (interval '{inc} month'))", DateTimeSpan.Months);
                }
            }
            if (expr is QueryParser.DateExpr.ReduceDate red)
            {
                var dec = toSqlDecimalExpr(red.Item2);
                var date = toSqlDateExpr(red.Item1);
                switch (date.Item2)
                {
                    case DateTimeSpan.Days:
                        return ($"({date.Item1} - (interval '{dec} day'))", DateTimeSpan.Days);
                    case DateTimeSpan.Months:
                        return ($"({date.Item1} - (interval '{dec} month'))", DateTimeSpan.Months);
                }
            }
            throw new NotImplementedException();
        }


        private static string toSqlBooleanAtom(QueryParser.BooleanAtom atom)
        {
            if (atom.IsHasError)
            {
                return @"(h.""BodyTemperature"" >= 37.5
OR (h.""StringColumn1"" <> '無' AND h.""StringColumn1"" <> 'N')
OR (h.""StringColumn2"" <> '無' AND h.""StringColumn2"" <> 'N')
OR (h.""StringColumn3"" <> '無' AND h.""StringColumn3"" <> 'N')
OR (h.""StringColumn4"" <> '無' AND h.""StringColumn4"" <> 'N')
OR (h.""StringColumn5"" <> '無' AND h.""StringColumn5"" <> 'N')
OR (h.""StringColumn6"" <> '無' AND h.""StringColumn6"" <> 'N')
OR (h.""StringColumn7"" <> '無' AND h.""StringColumn7"" <> 'N')
OR (h.""StringColumn8"" <> '無' AND h.""StringColumn8"" <> 'N')
OR (h.""StringColumn9"" <> '' OR h.""StringColumn9"" IS NULL)
OR (h.""StringColumn10"" <> '無' AND h.""StringColumn10"" <> 'N')
OR (h.""StringColumn11"" <> '無' AND h.""StringColumn11"" <> 'N')
OR (h.""StringColumn12"" <> '' OR h.""StringColumn12"" IS NULL) )";
            }
            if (atom.IsHasWarning)
            {
                return "(h.\"BodyTemperature\" >= 37.0)";
            }
            if (atom.IsIsSubmitted)
            {
                return "(NOT h.\"IsEmptyData\")";
            }
            if (atom.IsIsInfected)
            {
                return "h.\"IsInfected\"";
            }
            if (atom.IsTrue)
            {
                return "TRUE";
            }
            if (atom.IsFalse)
            {
                return "FALSE";
            }

            if (atom is QueryParser.BooleanAtom.SEq seq)
            {
                var lhs = toSqlStringExpr(seq.Item1);
                var rhs = toSqlStringExpr(seq.Item2);
                switch ((lhs.Item2, rhs.Item2))
                {
                    case (StringMatchMethod.Equals, StringMatchMethod.Equals):
                        return $"({lhs.Item1} = {rhs.Item1})";
                    case (StringMatchMethod.Equals, StringMatchMethod.FirstMatch):
                        return $"({rhs.Item1} LIKE '{lhs.Item1.Trim('\'')}%' AND {rhs.Item1} IS NOT NULL)";
                    case (StringMatchMethod.Equals, StringMatchMethod.HasOne):
                        return $"({rhs.Item1} LIKE '%{lhs.Item1.Trim('\'')}%' AND {rhs.Item1} IS NOT NULL)";

                    case (StringMatchMethod.FirstMatch, StringMatchMethod.Equals):
                        return $"({lhs.Item1} LIKE '{rhs.Item1.Trim('\'')}%' AND {lhs.Item1} IS NOT NULL)";
                    case (StringMatchMethod.FirstMatch, StringMatchMethod.FirstMatch):
                        return $"({lhs.Item1} LIKE '%{rhs.Item1.Trim('\'')}%' AND {lhs.Item1} IS NOT NULL)";
                    case (StringMatchMethod.FirstMatch, StringMatchMethod.HasOne):
                        return $"({lhs.Item1} LIKE '%{rhs.Item1.Trim('\'')}%' AND {lhs.Item1} IS NOT NULL)";

                    case (StringMatchMethod.HasOne, StringMatchMethod.Equals):
                        return $"({lhs.Item1} LIKE '%{rhs.Item1.Trim('\'')}%' AND {lhs.Item1} IS NOT NULL)";
                    case (StringMatchMethod.HasOne, StringMatchMethod.FirstMatch):
                        return $"({lhs.Item1} LIKE '%{rhs.Item1.Trim('\'')}%' AND {lhs.Item1} IS NOT NULL)";
                    case (StringMatchMethod.HasOne, StringMatchMethod.HasOne):
                        return $"({lhs.Item1} LIKE '%{rhs.Item1.Trim('\'')}%' AND {lhs.Item1} IS NOT NULL)";
                }
            }
            if (atom is QueryParser.BooleanAtom.SNe sne)
            {
                return $"(NOT {toSqlBooleanAtom(QueryParser.BooleanAtom.NewSEq(sne.Item1, sne.Item2))})";
            }

            if (atom is QueryParser.BooleanAtom.DeEq deeq)
            {
                return $"({toSqlDecimalExpr(deeq.Item1)} = {toSqlDecimalExpr(deeq.Item2)})";
            }
            if (atom is QueryParser.BooleanAtom.DeNe dene)
            {
                return $"({toSqlDecimalExpr(dene.Item1)} <> {toSqlDecimalExpr(dene.Item2)})";
            }
            if (atom is QueryParser.BooleanAtom.DeGt degt)
            {
                return $"({toSqlDecimalExpr(degt.Item1)} > {toSqlDecimalExpr(degt.Item2)})";
            }
            if (atom is QueryParser.BooleanAtom.DeGe dege)
            {
                return $"({toSqlDecimalExpr(dege.Item1)} >= {toSqlDecimalExpr(dege.Item2)})";
            }
            if (atom is QueryParser.BooleanAtom.DeLt delt)
            {
                return $"({toSqlDecimalExpr(delt.Item1)} < {toSqlDecimalExpr(delt.Item2)})";
            }
            if (atom is QueryParser.BooleanAtom.DeLe dele)
            {
                return $"({toSqlDecimalExpr(dele.Item1)} <= {toSqlDecimalExpr(dele.Item2)})";
            }

            if (atom is QueryParser.BooleanAtom.DtEq dteq)
            {
                return $"({toSqlDateExpr(dteq.Item1).Item1} = {toSqlDateExpr(dteq.Item2).Item1})";
            }
            if (atom is QueryParser.BooleanAtom.DtNe dtne)
            {
                return $"({toSqlDateExpr(dtne.Item1).Item1} <> {toSqlDateExpr(dtne.Item2).Item1})";
            }
            if (atom is QueryParser.BooleanAtom.DtGt dtgt)
            {
                return $"({toSqlDateExpr(dtgt.Item1).Item1} > {toSqlDateExpr(dtgt.Item2).Item1})";
            }
            if (atom is QueryParser.BooleanAtom.DtGe dtge)
            {
                return $"({toSqlDateExpr(dtge.Item1).Item1} >= {toSqlDateExpr(dtge.Item2).Item1})";
            }
            if (atom is QueryParser.BooleanAtom.DtLt dtlt)
            {
                return $"({toSqlDateExpr(dtlt.Item1).Item1} < {toSqlDateExpr(dtlt.Item2).Item1})";
            }
            if (atom is QueryParser.BooleanAtom.DtLe dtle)
            {
                return $"({toSqlDateExpr(dtle.Item1).Item1} <= {toSqlDateExpr(dtle.Item2).Item1})";
            }

            throw new NotImplementedException();
        }
        private static string toSqlBooleanExpr(QueryParser.BooleanExpr expr)
        {
            if (expr is QueryParser.BooleanExpr.BooleanAtom atom)
            {
                return $"({toSqlBooleanAtom(atom.Item)})";
            }
            if (expr is QueryParser.BooleanExpr.Eq eq)
            {
                return $"({toSqlBooleanExpr(eq.Item1)} == {toSqlBooleanExpr(eq.Item2)})";
            }
            if (expr is QueryParser.BooleanExpr.Ne ne)
            {
                return $"({toSqlBooleanExpr(ne.Item1)} != {toSqlBooleanExpr(ne.Item2)})";
            }
            if (expr is QueryParser.BooleanExpr.Neg neg)
            {
                return $"(NOT {toSqlBooleanExpr(neg.Item)})";
            }
            if (expr is QueryParser.BooleanExpr.Conj conj)
            {
                return $"({toSqlBooleanExpr(conj.Item1)} AND {toSqlBooleanExpr(conj.Item2)})";
            }
            if (expr is QueryParser.BooleanExpr.Disj disj)
            {
                return $"({toSqlBooleanExpr(disj.Item1)} OR {toSqlBooleanExpr(disj.Item2)})";
            }
            if (expr is QueryParser.BooleanExpr.Impl impl)
            {
                return $"(NOT {toSqlBooleanExpr(impl.Item1)} || {toSqlBooleanExpr(impl.Item2)})";
            }
            throw new NotImplementedException();
        }

    }
}
