using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BugTest
{
    public class TestRunner
    {
        private readonly Db _db;
        private readonly ILogger<TestRunner> _logger;

        public TestRunner(Db db, ILogger<TestRunner> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Run()
        {
            if (_db.TableA.Count() == 0)
            {
                _db.TableA.Add(new TableA { Name = "Adam", Type = "Person" });
                _db.TableA.Add(new TableA { Name = "Ben", Type = "Person" });
                _db.TableA.Add(new TableA { Name = "Fabrikam", Type = "Company" });
                _db.SaveChanges();
            }

            {
                var result = _db.TableA.ToList(); // Returns all results
                PrintResult("Query 0", result);
                /*
warn: BugTest.TestRunner[0]
      Query 0 created the following result
      { Name: "Adam", Type: "Person" }
      { Name: "Ben", Type: "Person" }
      { Name: "Fabrikam", Type: "Company" }


dbug: Microsoft.EntityFrameworkCore.Query[10111]
      Compiling query expression:
      'DbSet<TableA>()'
dbug: Microsoft.EntityFrameworkCore.Query[10107]
      Generated query execution expression:
      'queryContext => new SingleQueryingEnumerable<TableA>(
          (RelationalQueryContext)queryContext,
          RelationalCommandCache.SelectExpression(
              Projection Mapping:
                  EmptyProjectionMember -> Dictionary<IProperty, int> { [Property: TableA.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd, 0], [Property: TableA.Name (string), 1], [Property: TableA.Type (string), 2], }
              SELECT t.Id, t.Name, t.Type
              FROM TableA AS t),
          Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TableA>,
          BugTest.Db,
          False,
          False
      )'
dbug: Microsoft.EntityFrameworkCore.Database.Command[20100]
      Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [t].[Id], [t].[Name], [t].[Type]
      FROM [TableA] AS [t]
                 */
            }
            {
                var result = _db.TableA.Where(x => x.Name != "Adam" && x.Type != "Person").ToList(); // Wrong result, expecting to get Ben included in the result
                PrintResult("Query 1", result);
                /*
warn: BugTest.TestRunner[0]
      Query 1 created the following result
      { Name: "Fabrikam", Type: "Company" }


dbug: Microsoft.EntityFrameworkCore.Query[10111]
      Compiling query expression:
      'DbSet<TableA>()
          .Where(x => x.Name != "Adam" && x.Type != "Person")'
dbug: Microsoft.EntityFrameworkCore.Query[10107]
      Generated query execution expression:
      'queryContext => new SingleQueryingEnumerable<TableA>(
          (RelationalQueryContext)queryContext,
          RelationalCommandCache.SelectExpression(
              Projection Mapping:
                  EmptyProjectionMember -> Dictionary<IProperty, int> { [Property: TableA.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd, 0], [Property: TableA.Name (string), 1], [Property: TableA.Type (string), 2], }
              SELECT t.Id, t.Name, t.Type
              FROM TableA AS t
              WHERE (t.Name != N'Adam') && (t.Type != N'Person')),
          Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TableA>,
          BugTest.Db,
          False,
          False
      )'
dbug: Microsoft.EntityFrameworkCore.Database.Command[20100]
      Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [t].[Id], [t].[Name], [t].[Type]
      FROM [TableA] AS [t]
      WHERE (([t].[Name] <> N'Adam') OR [t].[Name] IS NULL) AND (([t].[Type] <> N'Person') OR [t].[Type] IS NULL)
                 */
            }
            {
                var result = _db.TableA.Where(x => !(x.Name == "Adam") && !(x.Type == "Person")).ToList(); // Wrong result, expecting to get Ben included in the result
                PrintResult("Query 2", result);
                /*
warn: BugTest.TestRunner[0]
      Query 2 created the following result
      { Name: "Fabrikam", Type: "Company" }


dbug: Microsoft.EntityFrameworkCore.Query[10111]
      Compiling query expression:
      'DbSet<TableA>()
          .Where(x => !(x.Name == "Adam") && !(x.Type == "Person"))'
dbug: Microsoft.EntityFrameworkCore.Query[10107]
      Generated query execution expression:
      'queryContext => new SingleQueryingEnumerable<TableA>(
          (RelationalQueryContext)queryContext,
          RelationalCommandCache.SelectExpression(
              Projection Mapping:
                  EmptyProjectionMember -> Dictionary<IProperty, int> { [Property: TableA.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd, 0], [Property: TableA.Name (string), 1], [Property: TableA.Type (string), 2], }
              SELECT t.Id, t.Name, t.Type
              FROM TableA AS t
              WHERE Not(t.Name == N'Adam') && Not(t.Type == N'Person')),
          Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TableA>,
          BugTest.Db,
          False,
          False
      )'
dbug: Microsoft.EntityFrameworkCore.Database.Command[20100]
      Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [t].[Id], [t].[Name], [t].[Type]
      FROM [TableA] AS [t]
      WHERE (([t].[Name] <> N'Adam') OR [t].[Name] IS NULL) AND (([t].[Type] <> N'Person') OR [t].[Type] IS NULL)
                */
            }
            {
                var result = _db.TableA.Where(x => !(x.Name == "Adam" && x.Type == "Person")).ToList(); // <------ Correct result
                PrintResult("Query 3", result);
                /*
warn: BugTest.TestRunner[0]
      Query 3 created the following result
      { Name: "Ben", Type: "Person" }
      { Name: "Fabrikam", Type: "Company" }


dbug: Microsoft.EntityFrameworkCore.Query[10111]
      Compiling query expression:
      'DbSet<TableA>()
          .Where(x => !(x.Name == "Adam" && x.Type == "Person"))'
dbug: Microsoft.EntityFrameworkCore.Query[10107]
      Generated query execution expression:
      'queryContext => new SingleQueryingEnumerable<TableA>(
          (RelationalQueryContext)queryContext,
          RelationalCommandCache.SelectExpression(
              Projection Mapping:
                  EmptyProjectionMember -> Dictionary<IProperty, int> { [Property: TableA.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd, 0], [Property: TableA.Name (string), 1], [Property: TableA.Type (string), 2], }
              SELECT t.Id, t.Name, t.Type
              FROM TableA AS t
              WHERE Not((t.Name == N'Adam') && (t.Type == N'Person'))),
          Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TableA>,
          BugTest.Db,
          False,
          False
      )'
dbug: Microsoft.EntityFrameworkCore.Database.Command[20100]
      Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [t].[Id], [t].[Name], [t].[Type]
      FROM [TableA] AS [t]
      WHERE (([t].[Name] <> N'Adam') OR [t].[Name] IS NULL) OR (([t].[Type] <> N'Person') OR [t].[Type] IS NULL)
                 */
            }
            {
                var result = _db.TableA.Where(x => !(x.Name == "Adam" || x.Type == "Person")).ToList(); // Correct result
                PrintResult("Query 4", result);
                /*
warn: BugTest.TestRunner[0]
      Query 4 created the following result
      { Name: "Fabrikam", Type: "Company" }


dbug: Microsoft.EntityFrameworkCore.Query[10111]
      Compiling query expression:
      'DbSet<TableA>()
          .Where(x => !(x.Name == "Adam" || x.Type == "Person"))'
dbug: Microsoft.EntityFrameworkCore.Query[10107]
      Generated query execution expression:
      'queryContext => new SingleQueryingEnumerable<TableA>(
          (RelationalQueryContext)queryContext,
          RelationalCommandCache.SelectExpression(
              Projection Mapping:
                  EmptyProjectionMember -> Dictionary<IProperty, int> { [Property: TableA.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd, 0], [Property: TableA.Name (string), 1], [Property: TableA.Type (string), 2], }
              SELECT t.Id, t.Name, t.Type
              FROM TableA AS t
              WHERE Not((t.Name == N'Adam') || (t.Type == N'Person'))),
          Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TableA>,
          BugTest.Db,
          False,
          False
      )'
dbug: Microsoft.EntityFrameworkCore.Database.Command[20100]
      Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [t].[Id], [t].[Name], [t].[Type]
      FROM [TableA] AS [t]
      WHERE (([t].[Name] <> N'Adam') OR [t].[Name] IS NULL) AND (([t].[Type] <> N'Person') OR [t].[Type] IS NULL)
                 */
            }

            void PrintResult(string queryName, List<TableA> result)
            {
                var sb = new StringBuilder();

                foreach (var item in result)
                {
                    sb.AppendLine();
                    sb.AppendFormat("{{ Name: \"{0}\", Type: \"{1}\" }}", item.Name, item.Type);
                }
                if (result.Count > 0)
                {
                    _logger.LogWarning("{Query} created the following result {Result}", queryName, sb.ToString());
                }
                else
                {
                    _logger.LogWarning("{Query} returned no result", queryName);
                }
            }
        }
    }

    public class TableA
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public class Db : DbContext
    {
        public Db()
        {
        }

        public Db(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=(local); Integrated Security=True; Initial Catalog=BugTest;MultipleActiveResultSets=True");
        }

        public DbSet<TableA> TableA { get; set; }
    }
}