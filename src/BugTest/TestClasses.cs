using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
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
            var id = Guid.NewGuid();
            // prepare
            _db.Set<A>().Add(new A {Id = id, Items = new List<B>(new[] {new B {Value = "1"}, new B {Value = "2"}})});
            _db.SaveChanges();

            _logger.LogInformation($"ChangeTracker.AutoDetectChangesEnabled: {_db.ChangeTracker.AutoDetectChangesEnabled}");
            DumpChangeTracker("Init state", _db.ChangeTracker);

            var a = _db.Set<A>().Include(x => x.Items).FirstOrDefault(x => x.Id == id);
            DumpChangeTracker("After fetch", _db.ChangeTracker);
            a.Items.RemoveAt(0);
            DumpChangeTracker("After remove", _db.ChangeTracker);
            _db.SaveChanges(false);
            DumpChangeTracker("After save", _db.ChangeTracker);
        }

        private void DumpChangeTracker(string label, ChangeTracker changeTracker)
        {
            _logger.LogInformation("Start dumping change tracker states: " + label);
            var entires = changeTracker.Entries();
            foreach (var entry in entires)
            {
                _logger.LogInformation($"Name: {entry.Metadata.Name} id: {entry.Property("Id")} state: {entry.State}");
            }
            _logger.LogInformation("End dumping change tracker states.");
        }
    }

    public class Db : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<A>(b =>
            {
                b.HasKey(p => p.Id);
                b.HasMany(p => p.Items)
                    .WithOne().IsRequired();
            });

            modelBuilder.Entity<B>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Value);
            });
        }
    }

    public class A
    {
        public Guid Id { get; set; }
        public List<B> Items { get; set; }
    }

    public class B
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
}