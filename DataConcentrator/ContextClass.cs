using System;
using System.Data.Entity;

namespace DataConcentrator
{
    public class ContextClass : DbContext
    {
        static ContextClass()
        {
            Database.SetInitializer(new PSUSUDbInitializer());
        }

        // singleton pattern
        private static ContextClass instance;

        public static ContextClass Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ContextClass();
                }
                return instance;
            }
        }

        // "PSUSUConnectionString" - definisan u App.config, koristi SQL Server LocalDB.
        public ContextClass() : base("name=PSUSUConnectionString")
        {
        }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Alarm> Alarms { get; set; }

        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // TPH (Table-Per-Hierarchy): AnalogInput/AnalogOutput/DigitalInput/DigitalOutput
            // se sve cuvaju u jednoj "Tags" tabeli, razlikuju se preko diskriminator kolone "TagType".
            modelBuilder.Entity<Tag>()
                .Map<AnalogInput>(m => m.Requires("TagType").HasValue("AI"))
                .Map<AnalogOutput>(m => m.Requires("TagType").HasValue("AO"))
                .Map<DigitalInput>(m => m.Requires("TagType").HasValue("DI"))
                .Map<DigitalOutput>(m => m.Requires("TagType").HasValue("DO"));

            modelBuilder.Entity<Tag>().ToTable("Tags");
            modelBuilder.Entity<Alarm>().ToTable("Alarms");
            modelBuilder.Entity<ActivatedAlarm>().ToTable("ActivatedAlarms");

            base.OnModelCreating(modelBuilder);
        }
    }

    // Kreira bazu ako ne postoji, ili je ponovo kreira ako se model promeni
    // (dovoljno za studentski projekat - izbegava potrebu za rucnim migracijama).
    public class PSUSUDbInitializer : DropCreateDatabaseIfModelChanges<ContextClass>
    {
    }
}
