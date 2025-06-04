using ToSic.Sys.Configuration;
#if NETFRAMEWORK
using Microsoft.EntityFrameworkCore.Metadata;
#else
using Microsoft.EntityFrameworkCore.Diagnostics;
#endif
namespace ToSic.Eav.Persistence.Efc.Models;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class EavDbContext(DbContextOptions<EavDbContext> options, IGlobalConfiguration globalConfig, ILogStore logStore)
    : DbContext(options)
{
    private ILog Log { get; } = new Log("EF.DbCtx");

    //public bool DebugMode = false;

    public virtual DbSet<TsDynDataApp> TsDynDataApps { get; set; }
    public virtual DbSet<TsDynDataTargetType> TsDynDataTargetTypes { get; set; }
    public virtual DbSet<TsDynDataAttribute> TsDynDataAttributes { get; set; }
    public virtual DbSet<TsDynDataContentType> TsDynDataContentTypes { get; set; }
    public virtual DbSet<TsDynDataAttributeType> TsDynDataAttributeTypes { get; set; }
    public virtual DbSet<TsDynDataTransaction> TsDynDataTransactions { get; set; }
    public virtual DbSet<TsDynDataHistory> TsDynDataHistories { get; set; }
    public virtual DbSet<TsDynDataDimension> TsDynDataDimensions { get; set; }
    public virtual DbSet<TsDynDataEntity> TsDynDataEntities { get; set; }
    public virtual DbSet<TsDynDataRelationship> TsDynDataRelationships { get; set; }
    public virtual DbSet<TsDynDataValue> TsDynDataValues { get; set; }
    public virtual DbSet<TsDynDataValueDimension> TsDynDataValueDimensions { get; set; }
    public virtual DbSet<TsDynDataZone> TsDynDataZones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Note: this only takes a few MS, but is called many times, so logging is disabled again
        //logStore.Add("boot-log", Log);
        var l = Log.Fn("starting with options builder", timer: true);

        var connectionString = globalConfig.ConnectionString(); // dbConfig.ConnectionString;
        if (!connectionString.ToLowerInvariant().Contains("multipleactiveresultsets")) // this is needed to allow querying data while preparing new data on the same DbContext
            connectionString += ";MultipleActiveResultSets=True";
#if NETFRAMEWORK
        optionsBuilder.UseSqlServer(connectionString);
#else
        
        optionsBuilder
            .UseSqlServer(
                connectionString,
                options => options
                    // https://learn.microsoft.com/en-gb/ef/core/querying/single-split-queries
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)

                    // Timeout in seconds
                    .CommandTimeout(180)

                    // Bug: 2025-03-10 2dm v19.03.03 problem with ContentTypeLoader.cs
                    // Entity Framework converts certain Where-In queries to FROM OPENJSON(@__sharedAttribIds_0) WITH ([value] int ''$'') AS [s]
                    // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/breaking-changes#contains-in-linq-queries-may-stop-working-on-older-sql-server-versions
                    // The following line should disable that conversion
                    //.TranslateParameterizedCollectionsToConstants()
                    // Note: we didn't apply it, as we just updated the compatibility level of the DB to 130 (SQL Server 2016)
            )
            .ConfigureWarnings(w => w.Log(RelationalEventId.MultipleCollectionIncludeWarning));
#endif
        l.Done();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        logStore.Add("boot-log", Log);
        var l = Log.Fn(timer: true);

        modelBuilder.Entity<TsDynDataDimension>(entity =>
        {
            entity.HasKey(e => e.DimensionId)
                .HasName("PK_TsDynDataDimension");

            entity.ToTable("TsDynDataDimension");

            entity.Property(e => e.DimensionId);

            entity.Property(e => e.Active)/*.HasDefaultValueSql("1")*/.ValueGeneratedNever();

            entity.Property(e => e.EnvironmentKey).HasColumnName("ExternalKey").HasMaxLength(100);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Key).HasColumnName("SystemKey").HasMaxLength(100);

            entity.Property(e => e.ZoneId);

            entity.HasOne(d => d.ParentNavigation)
                .WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("FK_TsDynDataDimension_TsDynDataDimension");

            entity.HasOne(d => d.Zone)
                .WithMany(p => p.TsDynDataDimensions)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataDimension_TsDynDataZone");
        });

        modelBuilder.Entity<TsDynDataEntity>(entity =>
        {
            entity.HasKey(e => e.EntityId)
                .HasName("PK_TsDynDataEntity");

            entity.ToTable("TsDynDataEntity");

#pragma warning disable CS0618 // Type or member is obsolete
            entity.HasIndex(e => e.KeyNumber)
                .HasName("IX_TsDynDataEntity_KeyNumber");
#pragma warning restore CS0618 // Type or member is obsolete

            entity.Property(e => e.EntityId);

            entity.Property(e => e.TargetTypeId);

            entity.Property(e => e.ContentTypeId);

            entity.Property(e => e.ContentType).HasMaxLength(250);

            entity.Property(e => e.EntityGuid)
                .HasDefaultValueSql("newid()");

            // 2dm: this was autogenerated, but it causes a big bug - see https://github.com/aspnet/EntityFramework/issues/7089
            //entity.Property(e => e.IsPublished).HasDefaultValueSql("1");

            entity.Property(e => e.KeyString).HasMaxLength(100);

            entity.Property(e => e.Owner).HasMaxLength(250);

            // 2017-10-10 2dm new with entity > app mapping
            entity.HasOne(d => d.App)
                .WithMany(p => p.TsDynDataEntities)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataEntity_TsDynDataApp");

            entity.HasOne(d => d.TargetType)
                .WithMany(p => p.TsDynDataEntities)
                .HasForeignKey(d => d.TargetTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataEntity_TsDynDataTargetType");

            entity.HasOne(d => d.ContentTypeNavigation)
                .WithMany(p => p.TsDynDataEntities)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataEntity_TsDynDataContentType");

            entity.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataEntitiesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataEntity_TsDynDataTransactionCreated");

            entity.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataEntitiesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataEntity_TsDynDataTransactionModified");

            entity.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataEntitiesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataEntity_TsDynDataTransactionDeleted");
        });

        modelBuilder.Entity<TsDynDataRelationship>(entity =>
        {
            entity.HasKey(e => new { e.AttributeId, e.ParentEntityId, e.SortOrder })
                .HasName("PK_TsDynDataRelationship");

            entity.ToTable("TsDynDataRelationship");

            entity.Property(e => e.AttributeId);

            entity.Property(e => e.ParentEntityId);

            entity.Property(e => e.ChildEntityId);

            entity.HasOne(d => d.Attribute)
                .WithMany(p => p.TsDynDataRelationships)
                .HasForeignKey(d => d.AttributeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TsDynDataRelationship_TsDynDataAttribute");

            entity.HasOne(d => d.ChildEntity)
                .WithMany(p => p.RelationshipsWithThisAsChild)
                .HasForeignKey(d => d.ChildEntityId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataRelationship_TsDynDataEntityChild");

            entity.HasOne(d => d.ParentEntity)
                .WithMany(p => p.RelationshipsWithThisAsParent)
                .HasForeignKey(d => d.ParentEntityId)
                // Commented for efcore 2.1.1 to fix DbUpdateConcurrencyException
                // "Database operation expected to affect 1 row(s) but actually affected 0 row(s)."
                //.OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataRelationship_TsDynDataEntityParent");
        });

        modelBuilder.Entity<TsDynDataValue>(entity =>
        {
            entity.HasKey(e => e.ValueId)
                .HasName("PK_TsDynDataValue");

            entity.ToTable("TsDynDataValue");

            entity.Property(e => e.ValueId);

            entity.Property(e => e.AttributeId);

            entity.Property(e => e.EntityId);

            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.HasOne(d => d.Attribute)
                .WithMany(p => p.TsDynDataValues)
                .HasForeignKey(d => d.AttributeId)
                .HasConstraintName("FK_TsDynDataValue_TsDynDataAttribute");

            entity.HasOne(d => d.Entity)
                .WithMany(p => p.TsDynDataValues)
                .HasForeignKey(d => d.EntityId)
                //.OnDelete(DeleteBehavior.Restrict)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TsDynDataValue_TsDynDataEntity");
        });

        modelBuilder.Entity<TsDynDataValueDimension>(entity =>
        {
            entity.HasKey(e => new { e.ValueId, e.DimensionId })
                .HasName("PK_TsDynDataValueDimension");

            entity.ToTable("TsDynDataValueDimension");

            entity.Property(e => e.ValueId);

            entity.Property(e => e.DimensionId);

            entity.Property(e => e.ReadOnly)/*.HasDefaultValueSql("0")*/.ValueGeneratedNever();

            entity.HasOne(d => d.Dimension)
                .WithMany(p => p.TsDynDataValueDimensions)
                .HasForeignKey(d => d.DimensionId)
                .OnDelete(DeleteBehavior.Cascade)// DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataValueDimension_TsDynDataDimension");

            entity.HasOne(d => d.Value)
                .WithMany(p => p.TsDynDataValueDimensions)
                .HasForeignKey(d => d.ValueId)
                .OnDelete(DeleteBehavior.Cascade)// DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataValueDimension_TsDynDataValue");
        });

        modelBuilder.Entity<TsDynDataApp>(entity =>
        {
            entity.HasKey(e => e.AppId)
                .HasName("PK_TsDynDataApp");

            entity.ToTable("TsDynDataApp");

#pragma warning disable CS0618 // Type or member is obsolete
            entity.HasIndex(e => new { e.Name, e.ZoneId })
                .HasName("UQ_TsDynDataApp_Name_ZoneId")
                .IsUnique();
#pragma warning restore CS0618 // Type or member is obsolete

            entity.Property(e => e.AppId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ZoneId);

            entity.HasOne(d => d.Zone)
                .WithMany(p => p.TsDynDataApps)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataZone");

            entity.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataAppsTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataTransactionCreated");

            entity.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataAppsTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataTransactionModified");

            entity.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataAppsTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataTransactionDeleted");
        });

        modelBuilder.Entity<TsDynDataAttribute>(entity =>
        {
            entity.HasKey(e => e.AttributeId)
                .HasName("PK_TsDynDataAttribute");

            entity.ToTable("TsDynDataAttribute");

            entity.Property(e => e.AttributeId);

            entity.Property(e => e.StaticName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Guid)
                .HasColumnType("uniqueidentifier");

            entity.Property(e => e.SysSettings)
                .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ContentTypeId);

            entity.Property(e => e.IsTitle)/*.HasDefaultValueSql("0")*/.ValueGeneratedNever();

            entity.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataAttributesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataTransactionCreated");

            entity.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataAttributesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataTransactionModified");

            entity.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataAttributesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataTransactionDeleted");

            entity.HasOne(d => d.TypeNavigation)
                .WithMany(p => p.TsDynDataAttributes)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataAttributeType");

            entity.HasOne(d => d.ContentType)
                .WithMany(p => p.TsDynDataAttributes)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataContentType");
        });

        modelBuilder.Entity<TsDynDataAttributeType>(entity =>
        {
            entity.HasKey(e => e.Type)
                .HasName("PK_TsDynDataAttributeType");

            entity.ToTable("TsDynDataAttributeType");

            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<TsDynDataContentType>(entity =>
        {
            entity.HasKey(e => e.ContentTypeId)
                .HasName("PK_TsDynDataContentType");

            entity.ToTable("TsDynDataContentType");

            entity.Property(e => e.ContentTypeId);

            entity.Property(e => e.IsGlobal)/*.HasDefaultValueSql("0")*/.ValueGeneratedNever();

            entity.Property(e => e.AppId);

            entity.Property(e => e.Name).HasMaxLength(150);

            entity.Property(e => e.Scope).HasMaxLength(50);

            entity.Property(e => e.StaticName)
                .HasMaxLength(150)
                .HasDefaultValueSql("newid()");

            entity.HasOne(d => d.App)
                .WithMany(p => p.TsDynDataContentTypes)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataApp");

            entity.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataContentTypesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataTransactionCreated");

            entity.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataContentTypesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataTransactionModified");

            entity.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataContentTypesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataTransactionDeleted");

            entity.HasOne(d => d.InheritContentTypeNavigation)
                .WithMany(p => p.InverseInheritContentTypesNavigation)
                .HasForeignKey(d => d.InheritContentTypeId)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataContentType");

            //entity.Property(e => e.SysSettings)
            //    .HasColumnName("SysSettings")
            //    .HasColumnType("nvarchar(MAX)");
        });

        modelBuilder.Entity<TsDynDataHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId)
                .HasName("PK_TsDynDataHistory");

            entity.ToTable("TsDynDataHistory");

            entity.Property(e => e.HistoryId);

            entity.Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(1)
                .IsFixedLength()
                .HasDefaultValueSql("N'I'");

            entity.Property(e => e.SourceId);

            entity.Property(e => e.SourceTable)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.Property(e => e.TransactionId);

#pragma warning disable CS0618 // Type or member is obsolete
            entity.HasIndex(e => e.SourceId)
                .HasName("IX_TsDynDataHistory_SourceId");

            entity.HasIndex(e => e.SourceGuid)
                .HasName("IX_TsDynDataHistory_SourceGuid");
#pragma warning restore CS0618 // Type or member is obsolete

            entity.HasOne(d => d.Transaction)
                .WithMany(p => p.TsDynDataHistories)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_TsDynDataHistory_TsDynDataTransaction");
        });

        modelBuilder.Entity<TsDynDataTargetType>(entity =>
        {
            entity.HasKey(e => e.TargetTypeId)
                .HasName("PK_TsDynDataTargetType");

            entity.ToTable("TsDynDataTargetType");

#pragma warning disable CS0618 // Type or member is obsolete
            entity.HasIndex(e => e.Name)
                .HasName("IX_TsDynDataTargetType_Name");
#pragma warning restore CS0618 // Type or member is obsolete

            entity.Property(e => e.TargetTypeId);

            entity.Property(e => e.Description).IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<TsDynDataTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId)
                .HasName("PK_TsDynDataTransaction");

            entity.ToTable("TsDynDataTransaction");

            entity.Property(e => e.TransactionId);

            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime")
                .HasDefaultValueSql("getdate()");

            entity.Property(e => e.User).HasMaxLength(255);
        });

        modelBuilder.Entity<TsDynDataZone>(entity =>
        {
            entity.HasKey(e => e.ZoneId)
                .HasName("PK_TsDynDataZone");

            entity.ToTable("TsDynDataZone");

            entity.Property(e => e.ZoneId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataZonesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .HasConstraintName("FK_TsDynDataZone_TsDynDataTransactionCreated");

            entity.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataZonesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .HasConstraintName("FK_TsDynDataZone_TsDynDataTransactionModified");

            entity.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataZonesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataZone_TsDynDataTransactionDeleted");
        });

        l.Done("done with own code");

        l = Log.Fn($"calling internal code {nameof(OnModelCreatingPartial)}", timer: true);
        OnModelCreatingPartial(modelBuilder);
        l.Done();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
