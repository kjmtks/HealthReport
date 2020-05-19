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

        public (int, IQueryable<Health>) Filtering(DatabaseContext context, int courseId, IEnumerable<TimeFrame> timeframes, DateTime startDate, int? numOfDaysToSearch = null)
        {
            if (query == null)
            {
                return (0, null);
            }
            if(timeframes == null || timeframes.Count() < 2)
            {
                timeframes = new TimeFrame[] { };
            }
            var sql_for_count  = buildSql(courseId, @"h.""Id""", timeframes, startDate, numOfDaysToSearch);
            var sql_for_search = buildSql(courseId, "*", timeframes, startDate, numOfDaysToSearch);
            var count = context.HealthList.FromSqlRaw(sql_for_count).AsNoTracking().Count();
            // Console.WriteLine(sql_for_search);
            return (count, Sort(context.HealthList.FromSqlRaw(sql_for_search).Include(x => x.Student)).AsNoTracking());
        }

        private string buildSql(int courseId, string columns, IEnumerable<TimeFrame> timeframes, DateTime startDate, int? numOfDaysToSearch = null)
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
    DISTINCT on (h.""MeasuredAt"", h.""TimeFrame"", h.""StudentId"")
    h.*,
    s.""Account"",
    s.""Tags""
  FROM ((
    (SELECT
      (SELECT COALESCE(max(""Id""), 0) FROM ""Health"") + row_number() OVER () AS ""Id"",
        0 AS ""BodyTemperature"",
        TRUE AS ""IsEmptyData"",
        FALSE AS ""IsInfected"",
        {stc}
        t.regexp_split_to_table AS ""TimeFrame"",
        u.""Id"" AS ""StudentId"",
        FALSE AS ""HasError"",
        FALSE AS ""HasWarning"",
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
        (SELECT ""Id"" FROM ""Student"" AS s INNER JOIN ""CourseStudentAssignment"" AS a ON a.""StudentId"" = s.""Id"" AND a.""CourseId"" = {courseId}) as u
      )
      UNION ALL
      (
        SELECT
          h.""Id"" AS ""Id"",
          h.""BodyTemperature"" AS ""BodyTemperature"",
          h.""IsEmptyData"" AS ""IsEmptyData"",
          h.""IsInfected"" AS ""IsInfected"",
          h.""MeasuredAt"" AS ""MeasuredAt"",
          COALESCE(h.""TimeFrame"", '') AS ""TimeFrame"",
          h.""StudentId"" AS ""StudentId"",
          h.""HasError"" AS ""HasError"",
          h.""HasWarning"" AS ""HasWarning"",
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
  )
  INNER JOIN
    ""CourseStudentAssignment"" AS a
  ON
    a.""StudentId"" = s.""Id"" AND a.""CourseId"" = {courseId}
  ORDER BY
    h.""MeasuredAt"", h.""TimeFrame"", h.""StudentId"", h.""IsInfected"", h.""IsEmptyData""
) as h
");
            var conditions = query.Value.Item1.Select(x => $"({QueryParser.BooleanExprToPgsqlExprString(x)})").ToList();
            // var conditions = query.Value.Item1.Select(x => $"({toSqlBooleanExpr(x)})").ToList();
            if (numOfDaysToSearch.HasValue)
            {
                conditions.Add(string.Format(@"(h.""MeasuredAt"" >= (date '{0:00}-{1:00}-{2:00}'))", startDate.Year, startDate.Month, startDate.Day));
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

    }
}
