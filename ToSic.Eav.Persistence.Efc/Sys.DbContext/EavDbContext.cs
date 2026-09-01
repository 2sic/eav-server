#if !NETFRAMEWORK
using Microsoft.EntityFrameworkCore.Diagnostics;
#endif
using Microsoft.EntityFrameworkCore.Metadata;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Sys.Configuration;

#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbContext;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class EavDbContext(
    DbContextOptions<EavDbContext> options,
    IGlobalConfiguration globalConfig,
    ILogStore logStore,
    IEavDbContextConfigurator dbContextConfigurator)
    : Microsoft.EntityFrameworkCore.DbContext(options)
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

        // 2026-05-18 stv: we used to add "MultipleActiveResultSets=True" here, but we probably do not need it for our use cases, so we removed it.
        // disabling MARS looks safe in the current eav-server / 2sxc code paths reviewed, including the .NET Framework EF Core 2.1.1.
        // In case of "There is already an open DataReader..." failures enable MARS again.
        //if (!connectionString.ToLowerInvariant().Contains("multipleactiveresultsets")) // this was needed to allow querying data while preparing new data on the same DbContext
        //    connectionString += ";MultipleActiveResultSets=True";
        
        dbContextConfigurator.Configure(optionsBuilder, connectionString);

#if !NETFRAMEWORK
        optionsBuilder.ConfigureWarnings(w => w.Log(RelationalEventId.MultipleCollectionIncludeWarning));
#endif

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
        l.Done();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        logStore.Add("boot-log", Log);
        var l = Log.Fn(timer: true);

        modelBuilder.Entity<TsDynDataDimension>(dimension =>
        {
            dimension.HasKey(e => e.DimensionId)
                .HasName("PK_TsDynDataDimension");

            dimension.ToTable("TsDynDataDimension");

            dimension.Property(e => e.DimensionId);

            dimension.Property(e => e.Active)/*.HasDefaultValueSql("1")*/.ValueGeneratedNever();

            dimension.Property(e => e.EnvironmentKey).HasColumnName("ExternalKey").HasMaxLength(100);

            dimension.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            dimension.Property(e => e.Key).HasColumnName("SystemKey").HasMaxLength(100);

            dimension.Property(e => e.ZoneId);

            dimension.HasOne(d => d.ParentNavigation)
                .WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("FK_TsDynDataDimension_TsDynDataDimension");

            dimension.HasOne(d => d.Zone)
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

            // TODO: @STV see if we can start using this; requires detailed review of each use case
            //entity.HasQueryFilter(e => e.TransDeletedId == null);

#pragma warning disable CS0618 // Type or member is obsolete
            entity.HasIndex(e => e.KeyNumber)
                .HasName("IX_TsDynDataEntity_KeyNumber");
#pragma warning restore CS0618 // Type or member is obsolete

            entity.Property(e => e.EntityId);

            entity.Property(e => e.TargetTypeId);

            entity.Property(e => e.ContentTypeId);

            entity.Property(e => e.ContentType).HasMaxLength(250);

            entity.Property(e => e.EntityGuid);

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

        modelBuilder.Entity<TsDynDataRelationship>(relationship =>
        {
            relationship.HasKey(e => new { e.AttributeId, e.ParentEntityId, e.SortOrder })
                .HasName("PK_TsDynDataRelationship");

            relationship.ToTable("TsDynDataRelationship");

            relationship.Property(e => e.AttributeId);

            relationship.Property(e => e.ParentEntityId);

            relationship.Property(e => e.ChildEntityId);

            relationship.Property(e => e.ChildExternalId);

            relationship.HasOne(d => d.Attribute)
                .WithMany(p => p.TsDynDataRelationships)
                .HasForeignKey(d => d.AttributeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TsDynDataRelationship_TsDynDataAttribute");

            relationship.HasOne(d => d.ChildEntity)
                .WithMany(p => p.RelationshipsWithThisAsChild)
                .HasForeignKey(d => d.ChildEntityId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataRelationship_TsDynDataEntityChild");

            relationship.HasOne(d => d.ParentEntity)
                .WithMany(p => p.RelationshipsWithThisAsParent)
                .HasForeignKey(d => d.ParentEntityId)
                // Commented for efcore 2.1.1 to fix DbUpdateConcurrencyException
                // "Database operation expected to affect 1 row(s) but actually affected 0 row(s)."
                //.OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataRelationship_TsDynDataEntityParent");
        });

        modelBuilder.Entity<TsDynDataValue>(value =>
        {
            value.HasKey(e => e.ValueId)
                .HasName("PK_TsDynDataValue");

            value.ToTable("TsDynDataValue");

            value.Property(e => e.ValueId);

            value.Property(e => e.AttributeId);

            value.Property(e => e.EntityId);

            value.Property(e => e.Value)
                .IsRequired();

            value.HasOne(d => d.Attribute)
                .WithMany(p => p.TsDynDataValues)
                .HasForeignKey(d => d.AttributeId)
                .HasConstraintName("FK_TsDynDataValue_TsDynDataAttribute");

            value.HasOne(d => d.Entity)
                .WithMany(p => p.TsDynDataValues)
                .HasForeignKey(d => d.EntityId)
                //.OnDelete(DeleteBehavior.Restrict)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TsDynDataValue_TsDynDataEntity");
        });

        modelBuilder.Entity<TsDynDataValueDimension>(valDim =>
        {
            valDim.HasKey(e => new { e.ValueId, e.DimensionId })
                .HasName("PK_TsDynDataValueDimension");

            valDim.ToTable("TsDynDataValueDimension");

            valDim.Property(e => e.ValueId);

            valDim.Property(e => e.DimensionId);

            valDim.Property(e => e.ReadOnly)/*.HasDefaultValueSql("0")*/.ValueGeneratedNever();

            valDim.HasOne(d => d.Dimension)
                .WithMany(p => p.TsDynDataValueDimensions)
                .HasForeignKey(d => d.DimensionId)
                .OnDelete(DeleteBehavior.Cascade)// DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataValueDimension_TsDynDataDimension");

            valDim.HasOne(d => d.Value)
                .WithMany(p => p.TsDynDataValueDimensions)
                .HasForeignKey(d => d.ValueId)
                .OnDelete(DeleteBehavior.Cascade)// DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataValueDimension_TsDynDataValue");
        });

        modelBuilder.Entity<TsDynDataApp>(app =>
        {
            app.HasKey(e => e.AppId)
                .HasName("PK_TsDynDataApp");

            app.ToTable("TsDynDataApp");

            // New 2025-12-17, so far never been used yes, so no issues expected
            // Note that it's actually not in use yet, we just introduced the fields but never used them
            app.HasQueryFilter(e => e.TransDeletedId == null);

#pragma warning disable CS0618 // Type or member is obsolete
            app.HasIndex(e => new { e.Name, e.ZoneId })
                .HasName("UQ_TsDynDataApp_Name_ZoneId")
                .IsUnique();
#pragma warning restore CS0618 // Type or member is obsolete

            app.Property(e => e.AppId);

            app.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            app.Property(e => e.ZoneId);

            app.HasOne(d => d.Zone)
                .WithMany(p => p.TsDynDataApps)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataZone");

            app.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataAppsTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataTransactionCreated");

            app.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataAppsTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataTransactionModified");

            app.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataAppsTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataApp_TsDynDataTransactionDeleted");
        });

        modelBuilder.Entity<TsDynDataAttribute>(attribute =>
        {
            attribute.HasKey(e => e.AttributeId)
                .HasName("PK_TsDynDataAttribute");

            attribute.ToTable("TsDynDataAttribute");

            // TODO: @STV see if we can start using this; requires detailed review of each use case
            // New 2025-12-17, we THINK that we recently introduced this field, and it's not used yet, so we can always apply the filter for now
            attribute.HasQueryFilter(e => e.TransDeletedId == null);

            attribute.Property(e => e.AttributeId);

            attribute.Property(e => e.StaticName)
                .IsRequired()
                .HasMaxLength(50);

            attribute.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);

            attribute.Property(e => e.Guid);

            attribute.Property(e => e.SysSettings);

            attribute.Property(e => e.ContentTypeId);

            attribute.Property(e => e.IsTitle)/*.HasDefaultValueSql("0")*/.ValueGeneratedNever();

            attribute.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataAttributesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataTransactionCreated");

            attribute.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataAttributesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataTransactionModified");

            attribute.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataAttributesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataTransactionDeleted");

            attribute.HasOne(d => d.TypeNavigation)
                .WithMany(p => p.TsDynDataAttributes)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataAttributeType");

            attribute.HasOne(d => d.ContentType)
                .WithMany(p => p.TsDynDataAttributes)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataAttribute_TsDynDataContentType");
        });

        modelBuilder.Entity<TsDynDataAttributeType>(attributeType =>
        {
            attributeType.HasKey(e => e.Type)
                .HasName("PK_TsDynDataAttributeType");

            attributeType.ToTable("TsDynDataAttributeType");

            attributeType.Property(e => e.Type)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<TsDynDataContentType>(contentType =>
        {
            contentType.HasKey(e => e.ContentTypeId)
                .HasName("PK_TsDynDataContentType");

            contentType.ToTable("TsDynDataContentType");

            // New 2025-12-17, all 8 use cases reviewed by 2dm
            contentType.HasQueryFilter(e => e.TransDeletedId == null);

            contentType.Property(e => e.ContentTypeId);

            contentType.Property(e => e.IsGlobal)/*.HasDefaultValueSql("0")*/.ValueGeneratedNever();

            contentType.Property(e => e.AppId);

            contentType.Property(e => e.Name).HasMaxLength(150);

            contentType.Property(e => e.Scope).HasMaxLength(50);

            contentType.Property(e => e.StaticName)
                .HasMaxLength(150);

            contentType.HasOne(d => d.App)
                .WithMany(p => p.TsDynDataContentTypes)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataApp");

            contentType.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataContentTypesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataTransactionCreated");

            contentType.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataContentTypesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataTransactionModified");

            contentType.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataContentTypesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataTransactionDeleted");

            contentType.HasOne(d => d.InheritContentTypeNavigation)
                .WithMany(p => p.InverseInheritContentTypesNavigation)
                .HasForeignKey(d => d.InheritContentTypeId)
                .HasConstraintName("FK_TsDynDataContentType_TsDynDataContentType");

            //entity.Property(e => e.SysSettings)
            //    .HasColumnName("SysSettings")
            //    .HasColumnType("nvarchar(MAX)");
        });

        modelBuilder.Entity<TsDynDataHistory>(history =>
        {
            history.HasKey(e => e.HistoryId)
                .HasName("PK_TsDynDataHistory");

            history.ToTable("TsDynDataHistory");

            history.Property(e => e.HistoryId);

            history.Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(1)
                .IsFixedLength();

            history.Property(e => e.SourceId);

            history.Property(e => e.SourceTable)
                .IsRequired()
                .HasMaxLength(250);

            history.Property(e => e.Timestamp);

            history.Property(e => e.TransactionId);

            history.Property(e => e.ParentRef)
                .HasMaxLength(250);

#pragma warning disable CS0618 // Type or member is obsolete
            history.HasIndex(e => e.SourceId)
                .HasName("IX_TsDynDataHistory_SourceId");

            history.HasIndex(e => e.SourceGuid)
                .HasName("IX_TsDynDataHistory_SourceGuid");

            history.HasIndex(e => e.ParentRef)
                .HasName("IX_TsDynDataHistory_ParentRef");
#pragma warning restore CS0618 // Type or member is obsolete

            history.HasOne(d => d.Transaction)
                .WithMany(p => p.TsDynDataHistories)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_TsDynDataHistory_TsDynDataTransaction");
        });

        modelBuilder.Entity<TsDynDataTargetType>(targetType =>
        {
            targetType.HasKey(e => e.TargetTypeId)
                .HasName("PK_TsDynDataTargetType");

            targetType.ToTable("TsDynDataTargetType");

#pragma warning disable CS0618 // Type or member is obsolete
            targetType.HasIndex(e => e.Name)
                .HasName("IX_TsDynDataTargetType_Name");
#pragma warning restore CS0618 // Type or member is obsolete

            targetType.Property(e => e.TargetTypeId);

            targetType.Property(e => e.Description).IsRequired();

            targetType.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<TsDynDataTransaction>(transaction =>
        {
            transaction.HasKey(e => e.TransactionId)
                .HasName("PK_TsDynDataTransaction");

            transaction.ToTable("TsDynDataTransaction");

            transaction.Property(e => e.TransactionId);

            transaction.Property(e => e.Timestamp);

            transaction.Property(e => e.User).HasMaxLength(255);
        });

        modelBuilder.Entity<TsDynDataZone>(zone =>
        {
            zone.HasKey(e => e.ZoneId)
                .HasName("PK_TsDynDataZone");

            zone.ToTable("TsDynDataZone");

            // TODO: @STV see if we can start using this; requires detailed review of each use case
            // Note that it's actually not in use yet, we just introduced the fields but never used them
            zone.HasQueryFilter(e => e.TransDeletedId == null);

            zone.Property(e => e.ZoneId);

            zone.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            zone.HasOne(d => d.TransCreated)
                .WithMany(p => p.TsDynDataZonesTransCreated)
                .HasForeignKey(d => d.TransCreatedId)
                .HasConstraintName("FK_TsDynDataZone_TsDynDataTransactionCreated");

            zone.HasOne(d => d.TransModified)
                .WithMany(p => p.TsDynDataZonesTransModified)
                .HasForeignKey(d => d.TransModifiedId)
                .HasConstraintName("FK_TsDynDataZone_TsDynDataTransactionModified");

            zone.HasOne(d => d.TransDeleted)
                .WithMany(p => p.TsDynDataZonesTransDeleted)
                .HasForeignKey(d => d.TransDeletedId)
                .HasConstraintName("FK_TsDynDataZone_TsDynDataTransactionDeleted");
        });

        l.Done("done with own code");

        l = Log.Fn($"calling internal code {nameof(OnModelCreatingPartial)}", timer: true);
        OnModelCreatingPartial(modelBuilder);
#if !NETFRAMEWORK
        RewriteDatabaseNames(modelBuilder);
#endif
        l.Done();
    }

#if !NETFRAMEWORK
    private void RewriteDatabaseNames(ModelBuilder modelBuilder)
    {
        if (dbContextConfigurator.RewriteName("Name") == "Name")
            return;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName == null)
                continue;

            var table = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
            var columns = entityType.GetProperties()
                .Select(property => (Property: property, Name: property.GetColumnName(table)))
                .ToArray();
            entityType.SetTableName(dbContextConfigurator.RewriteName(tableName));

            foreach (var (property, columnName) in columns)
                if (columnName != null)
                    property.SetColumnName(dbContextConfigurator.RewriteName(columnName));

            foreach (var key in entityType.GetKeys())
                if (key.GetName() is { } keyName)
                    key.SetName(dbContextConfigurator.RewriteName(keyName));

            foreach (var index in entityType.GetIndexes())
                if (index.GetDatabaseName() is { } indexName)
                    index.SetDatabaseName(dbContextConfigurator.RewriteName(indexName));

            foreach (var foreignKey in entityType.GetForeignKeys())
                if (foreignKey.GetConstraintName() is { } constraintName)
                    foreignKey.SetConstraintName(dbContextConfigurator.RewriteName(constraintName));
        }
    }
#endif

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
