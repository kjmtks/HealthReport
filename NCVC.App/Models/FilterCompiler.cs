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

        public (int, IQueryable<Health>) Filtering(DatabaseContext context, IEnumerable<TimeFrame> timeframes, DateTime startDate, int? numOfDaysToSearch = null)
        {
            if (query == null)
            {
                return (0, null);
            }

            var sql_for_count  = buildSql(@"h.""Id""", timeframes, startDate, numOfDaysToSearch);
            var sql_for_search = buildSql("*", timeframes, startDate, numOfDaysToSearch);
            var count = context.HealthList.FromSqlRaw(sql_for_count).Count();
            return (count, Sort(context.HealthList.FromSqlRaw(sql_for_search).Include(x => x.Student)));
        }

        private string buildSql(string columns, IEnumerable<TimeFrame> timeframes, DateTime startDate, int? numOfDaysToSearch = null)
        {
            if (numOfDaysToSearch.HasValue)
            {
                var d = DateTime.Today.AddDays(-numOfDaysToSearch.Value);
                if (startDate < d)
                {
                    startDate = d;
                }
            }
            var span = DateTime.Today - startDate;
            string stc = string.Format(@"(date '{0:0000}-{1:00}-{2:00}' + arr.i) AS ""MeasuredAt"",", startDate.Year, startDate.Month, startDate.Day);

            var sql = new System.Text.StringBuilder();
            sql.Append(@$"
SELECT
  {columns}
FROM
  (SELECT
    DISTINCT on (h.""MeasuredAt"", h.""TimeFrame"", h.""StudentId"", h.""IsInfected"")
    h.*,
    s.""Account"",
    s.""Tags""
  FROM (
    (SELECT
      (SELECT COALESCE(max(""Id""), 0) FROM ""Health"") + row_number() OVER () AS ""Id"",
      0 AS ""BodyTemperature"",
      TRUE AS ""IsEmptyData"",
      FALSE AS ""IsInfected"",
      {stc}
      t.regexp_split_to_table AS ""TimeFrame"",
      u.""Id"" AS ""StudentId"",
      0 AS ""InfectedBodyTemperature1"",
      0 AS ""InfectedBodyTemperature2"",
      '00:00:00' AS ""InfectedMeasuredTime1"",
      '00:00:00' AS ""InfectedMeasuredTime2"",
      0 AS ""InfectedOxygenSaturation1"",
      0 AS ""InfectedOxygenSaturation2"",
      '' AS ""InfectedStringColumn1"",
      '' AS ""InfectedStringColumn2"",
      '' AS ""InfectedStringColumn3"",
      '' AS ""InfectedStringColumn4"",
      '' AS ""InfectedStringColumn5"",
      '' AS ""InfectedStringColumn6"",
      '' AS ""InfectedStringColumn7"",
      '' AS ""InfectedStringColumn8"",
      '' AS ""InfectedStringColumn9"",
      '' AS ""InfectedStringColumn10"",
      0 AS ""MailIndex"",
      '' AS ""RawUserId"",
      '' AS ""RawUserName"",
      '' AS ""StringColumn1"",
      '' AS ""StringColumn2"",
      '' AS ""StringColumn3"",
      '' AS ""StringColumn4"",
      '' AS ""StringColumn5"",
      '' AS ""StringColumn6"",
      '' AS ""StringColumn7"",
      '' AS ""StringColumn8"",
      '' AS ""StringColumn9"",
      '' AS ""StringColumn10"",
      '' AS ""StringColumn11"",
      '' AS ""StringColumn12"",
      (date '2020-01-01') AS ""UploadedAt""
    FROM
      generate_series(0, {span.Days}) AS arr(i),
      (SELECT regexp_split_to_table('{string.Join(";", timeframes.Select(x => x.Name))}', ';')) AS t
    CROSS JOIN
      ""Student"" AS u
  )
  UNION ALL
  (
    SELECT
      h.""Id"" AS ""Id"",
      h.""BodyTemperature"" AS ""BodyTemperature"",
      h.""IsEmptyData"" AS ""IsEmptyData"",
      h.""IsInfected"" AS ""IsInfected"",
      h.""MeasuredAt"" AS ""MeasuredAt"",
      h.""TimeFrame"" AS ""TimeFrame"",
      h.""StudentId"" AS ""StudentId"",
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
      h.""MailIndex"" AS ""MailIndex"",
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
      h.""UploadedAt"" AS ""UploadedAt""
    FROM
      ""Health"" AS h
    )
  ) as h
  INNER JOIN
    ""Student"" AS s
  ON
    h.""StudentId"" = s.""Id""
  ORDER BY
    h.""MeasuredAt"", h.""TimeFrame"", h.""StudentId"", h.""IsInfected"", h.""IsEmptyData""
  ) as h
");

            var conditions = query.Value.Item1.Select(x => $"({toSqlBooleanExpr(x)})").ToList();
            if (numOfDaysToSearch.HasValue)
            {
                conditions.Add(string.Format(@"h.""MeasuredAt"" >= (date '{0:00}-{1:00}-{2:00}')", startDate.Year, startDate.Month, startDate.Day));
            }
            var where = string.Join(" AND ", conditions);
            if (!string.IsNullOrWhiteSpace(where))
            {
                sql.Append(" WHERE ");
                sql.Append(where);
            }
            return sql.ToString();
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

        private static string toSqlStringAtom(QueryParser.StringAtom atom)
        {
            if (atom.IsTimeFrame)
            {
                return "h.\"TimeFrame\"";
            }
            if (atom.IsUserId)
            {
                return "h.\"Account\"";
            }
            if (atom.IsTag)
            {
                return "(' ' || h.\"Tags\" || ' ')";
            }
            if (atom is QueryParser.StringAtom.StringLiteral literal)
            {
                return $"'{literal.Item.Replace("'", "''")}'";
            }
            throw new NotImplementedException();
        }
        private static string toSqlStringExpr(QueryParser.StringExpr expr)
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
                return $"({lhs} = {rhs})";
            }

            if (atom is QueryParser.BooleanAtom.SStartWith ssw)
            {
                var lhs = toSqlStringExpr(ssw.Item1);
                var rhs = toSqlStringExpr(ssw.Item2);
                return $"({lhs} LIKE '{rhs.Trim('\'').Replace("'", "''").Replace("_", "\\_").Replace("%", "\\%")}%' AND {lhs} IS NOT NULL)";
            }
            if (atom is QueryParser.BooleanAtom.SEndWith sew)
            {
                var lhs = toSqlStringExpr(sew.Item1);
                var rhs = toSqlStringExpr(sew.Item2);
                return $"({lhs} LIKE '%{rhs.Trim('\'').Replace("'", "''").Replace("_", "\\_").Replace("%", "\\%")}' AND {lhs} IS NOT NULL)";
            }
            if (atom is QueryParser.BooleanAtom.SHas shs)
            {
                var lhs = toSqlStringExpr(shs.Item1);
                var rhs = toSqlStringExpr(shs.Item2);
                return $"({lhs} LIKE '% {rhs.Trim('\'').Replace("'", "''").Replace("_", "\\_").Replace("%", "\\%")} %' AND {lhs} IS NOT NULL)";
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
