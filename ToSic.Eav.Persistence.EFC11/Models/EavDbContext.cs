using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class EavDbContext : DbContext
    {
        public virtual DbSet<ToSicEavApps> ToSicEavApps { get; set; }
        public virtual DbSet<ToSicEavAssignmentObjectTypes> ToSicEavAssignmentObjectTypes { get; set; }
        public virtual DbSet<ToSicEavAttributeGroups> ToSicEavAttributeGroups { get; set; }
        public virtual DbSet<ToSicEavAttributeSets> ToSicEavAttributeSets { get; set; }
        public virtual DbSet<ToSicEavAttributeTypes> ToSicEavAttributeTypes { get; set; }
        public virtual DbSet<ToSicEavAttributes> ToSicEavAttributes { get; set; }
        public virtual DbSet<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }
        public virtual DbSet<ToSicEavChangeLog> ToSicEavChangeLog { get; set; }
        public virtual DbSet<ToSicEavDataTimeline> ToSicEavDataTimeline { get; set; }
        public virtual DbSet<ToSicEavDimensions> ToSicEavDimensions { get; set; }
        public virtual DbSet<ToSicEavEntities> ToSicEavEntities { get; set; }
        public virtual DbSet<ToSicEavEntityRelationships> ToSicEavEntityRelationships { get; set; }
        public virtual DbSet<ToSicEavValues> ToSicEavValues { get; set; }
        public virtual DbSet<ToSicEavValuesDimensions> ToSicEavValuesDimensions { get; set; }
        public virtual DbSet<ToSicEavZones> ToSicEavZones { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //    optionsBuilder.UseSqlServer(@"Data Source=NBK-DEV-04\SQLEXPRESS;Initial Catalog=""2flex 2Sexy Content"";Integrated Security=True;");
        //}

        public bool DebugMode = false;

        public EavDbContext(DbContextOptions<EavDbContext> options) : base(options)
        {
            
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToSicEavApps>(entity =>
            {
                entity.HasKey(e => e.AppId)
                    .HasName("PK_ToSIC_EAV_Apps");

                entity.ToTable("ToSIC_EAV_Apps");

                entity.HasIndex(e => new { e.Name, e.ZoneId })
                    .HasName("ToSIC_EAV_Apps_PreventDuplicates")
                    .IsUnique();

                entity.Property(e => e.AppId).HasColumnName("AppID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ZoneId).HasColumnName("ZoneID");

                entity.HasOne(d => d.Zone)
                    .WithMany(p => p.ToSicEavApps)
                    .HasForeignKey(d => d.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Apps_ToSIC_EAV_Zones");
            });

            modelBuilder.Entity<ToSicEavAssignmentObjectTypes>(entity =>
            {
                entity.HasKey(e => e.AssignmentObjectTypeId)
                    .HasName("PK_ToSIC_EAV_AssignmentObjectTypes");

                entity.ToTable("ToSIC_EAV_AssignmentObjectTypes");

                entity.HasIndex(e => e.Name)
                    .HasName("IX_ToSIC_EAV_AssignmentObjectTypes");

                entity.Property(e => e.AssignmentObjectTypeId).HasColumnName("AssignmentObjectTypeID");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ToSicEavAttributeGroups>(entity =>
            {
                entity.HasKey(e => e.AttributeGroupId)
                    .HasName("PK_ToSIC_EAV_AttributeGroups");

                entity.ToTable("ToSIC_EAV_AttributeGroups");

                entity.Property(e => e.AttributeGroupId).HasColumnName("AttributeGroupID");

                entity.Property(e => e.AttributeSetId).HasColumnName("AttributeSetID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.AttributeSet)
                    .WithMany(p => p.ToSicEavAttributeGroups)
                    .HasForeignKey(d => d.AttributeSetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_AttributeGroups_ToSIC_EAV_AttributeSets");
            });

            modelBuilder.Entity<ToSicEavAttributeSets>(entity =>
            {
                entity.HasKey(e => e.AttributeSetId)
                    .HasName("PK_ToSIC_EAV_AttributeSets");

                entity.ToTable("ToSIC_EAV_AttributeSets");

                entity.Property(e => e.AttributeSetId).HasColumnName("AttributeSetID");

                entity.Property(e => e.AlwaysShareConfiguration).HasDefaultValueSql("0");

                entity.Property(e => e.AppId).HasColumnName("AppID");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Name).HasMaxLength(150);

                entity.Property(e => e.Scope).HasMaxLength(50);

                entity.Property(e => e.StaticName)
                    .HasMaxLength(150)
                    .HasDefaultValueSql("newid()");

                entity.HasOne(d => d.App)
                    .WithMany(p => p.ToSicEavAttributeSets)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_AttributeSets_ToSIC_EAV_Apps");

                entity.HasOne(d => d.ChangeLogCreatedNavigation)
                    .WithMany(p => p.ToSicEavAttributeSetsChangeLogCreatedNavigation)
                    .HasForeignKey(d => d.ChangeLogCreated)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_AttributeSets_ToSIC_EAV_ChangeLogCreated");

                entity.HasOne(d => d.ChangeLogDeletedNavigation)
                    .WithMany(p => p.ToSicEavAttributeSetsChangeLogDeletedNavigation)
                    .HasForeignKey(d => d.ChangeLogDeleted)
                    .HasConstraintName("FK_ToSIC_EAV_AttributeSets_ToSIC_EAV_ChangeLogDeleted");

                entity.HasOne(d => d.UsesConfigurationOfAttributeSetNavigation)
                    .WithMany(p => p.InverseUsesConfigurationOfAttributeSetNavigation)
                    .HasForeignKey(d => d.UsesConfigurationOfAttributeSet)
                    .HasConstraintName("FK_ToSIC_EAV_AttributeSets_ToSIC_EAV_AttributeSets");

            });

            modelBuilder.Entity<ToSicEavAttributeTypes>(entity =>
            {
                entity.HasKey(e => e.Type)
                    .HasName("PK_ToSIC_EAV_AttributeTypes");

                entity.ToTable("ToSIC_EAV_AttributeTypes");

                entity.Property(e => e.Type).HasMaxLength(50);
            });

            modelBuilder.Entity<ToSicEavAttributes>(entity =>
            {
                entity.HasKey(e => e.AttributeId)
                    .HasName("PK_ToSIC_EAV_Attributes");

                entity.ToTable("ToSIC_EAV_Attributes");

                entity.Property(e => e.AttributeId).HasColumnName("AttributeID");

                entity.Property(e => e.StaticName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.ChangeLogCreatedNavigation)
                    .WithMany(p => p.ToSicEavAttributesChangeLogCreatedNavigation)
                    .HasForeignKey(d => d.ChangeLogCreated)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Attributes_ToSIC_EAV_ChangeLogCreated");

                entity.HasOne(d => d.ChangeLogDeletedNavigation)
                    .WithMany(p => p.ToSicEavAttributesChangeLogDeletedNavigation)
                    .HasForeignKey(d => d.ChangeLogDeleted)
                    .HasConstraintName("FK_ToSIC_EAV_Attributes_ToSIC_EAV_ChangeLogDeleted");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.ToSicEavAttributes)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Attributes_ToSIC_EAV_Types");
            });

            modelBuilder.Entity<ToSicEavAttributesInSets>(entity =>
            {
                entity.HasKey(e => new { e.AttributeId, e.AttributeSetId })
                    .HasName("PK_ToSIC_EAV_AttributesInSets");

                entity.ToTable("ToSIC_EAV_AttributesInSets");

                entity.Property(e => e.AttributeId).HasColumnName("AttributeID");

                entity.Property(e => e.AttributeSetId).HasColumnName("AttributeSetID");

                entity.Property(e => e.AttributeGroupId).HasColumnName("AttributeGroupID");

                entity.Property(e => e.IsTitle).HasDefaultValueSql("0");

                entity.HasOne(d => d.AttributeGroup)
                    .WithMany(p => p.ToSicEavAttributesInSets)
                    .HasForeignKey(d => d.AttributeGroupId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_AttributesInSets_ToSIC_EAV_AttributeGroups");

                entity.HasOne(d => d.Attribute)
                    .WithMany(p => p.ToSicEavAttributesInSets)
                    .HasForeignKey(d => d.AttributeId)
                    .HasConstraintName("FK_ToSIC_EAV_AttributesInSets_ToSIC_EAV_Attributes");

                entity.HasOne(d => d.AttributeSet)
                    .WithMany(p => p.ToSicEavAttributesInSets)
                    .HasForeignKey(d => d.AttributeSetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_AttributesInSets_ToSIC_EAV_AttributeSets");
            });

            modelBuilder.Entity<ToSicEavChangeLog>(entity =>
            {
                entity.HasKey(e => e.ChangeId)
                    .HasName("PK_ToSIC_EAV_ChangeLog");

                entity.ToTable("ToSIC_EAV_ChangeLog");

                entity.Property(e => e.ChangeId).HasColumnName("ChangeID");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.User).HasMaxLength(255);
            });

            modelBuilder.Entity<ToSicEavDataTimeline>(entity =>
            {
                entity.ToTable("ToSIC_EAV_DataTimeline");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.NewData)
                    .IsRequired()
                    .HasColumnType("xml");

                entity.Property(e => e.Operation)
                    .IsRequired()
                    .HasColumnType("nchar(1)")
                    .HasDefaultValueSql("N'I'");

                entity.Property(e => e.SourceId).HasColumnName("SourceID");

                entity.Property(e => e.SourceTable)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.SourceTextKey).HasMaxLength(250);

                entity.Property(e => e.SysCreatedDate).HasColumnType("datetime");

                entity.Property(e => e.SysLogId).HasColumnName("SysLogID");
            });

            modelBuilder.Entity<ToSicEavDimensions>(entity =>
            {
                entity.HasKey(e => e.DimensionId)
                    .HasName("PK_ToSIC_EAV_Dimensions");

                entity.ToTable("ToSIC_EAV_Dimensions");

                entity.Property(e => e.DimensionId).HasColumnName("DimensionID");

                entity.Property(e => e.Active).HasDefaultValueSql("1");

                entity.Property(e => e.ExternalKey).HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SystemKey).HasMaxLength(100);

                entity.Property(e => e.ZoneId).HasColumnName("ZoneID");

                entity.HasOne(d => d.ParentNavigation)
                    .WithMany(p => p.InverseParentNavigation)
                    .HasForeignKey(d => d.Parent)
                    .HasConstraintName("FK_ToSIC_EAV_Dimensions_ToSIC_EAV_Dimensions1");

                entity.HasOne(d => d.Zone)
                    .WithMany(p => p.ToSicEavDimensions)
                    .HasForeignKey(d => d.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Dimensions_ToSIC_EAV_Zones");
            });

            modelBuilder.Entity<ToSicEavEntities>(entity =>
            {
                entity.HasKey(e => e.EntityId)
                    .HasName("PK_ToSIC_EAV_Entities");

                entity.ToTable("ToSIC_EAV_Entities");

                entity.HasIndex(e => e.KeyNumber)
                    .HasName("IX_KeyNumber");

                entity.Property(e => e.EntityId).HasColumnName("EntityID");

                entity.Property(e => e.AssignmentObjectTypeId).HasColumnName("AssignmentObjectTypeID");

                entity.Property(e => e.AttributeSetId).HasColumnName("AttributeSetID");

                entity.Property(e => e.EntityGuid)
                    .HasColumnName("EntityGUID")
                    .HasDefaultValueSql("newid()");

                // 2dm: this was autogenerated, but it causes a big bug - see https://github.com/aspnet/EntityFramework/issues/7089
                //entity.Property(e => e.IsPublished).HasDefaultValueSql("1");

                entity.Property(e => e.KeyString).HasMaxLength(100);

                entity.Property(e => e.Owner).HasMaxLength(250);

                entity.HasOne(d => d.AssignmentObjectType)
                    .WithMany(p => p.ToSicEavEntities)
                    .HasForeignKey(d => d.AssignmentObjectTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Entities_ToSIC_EAV_AssignmentObjectTypes");

                entity.HasOne(d => d.AttributeSet)
                    .WithMany(p => p.ToSicEavEntities)
                    .HasForeignKey(d => d.AttributeSetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Entities_ToSIC_EAV_AttributeSets");

                entity.HasOne(d => d.ChangeLogCreatedNavigation)
                    .WithMany(p => p.ToSicEavEntitiesChangeLogCreatedNavigation)
                    .HasForeignKey(d => d.ChangeLogCreated)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Entities_ToSIC_EAV_ChangeLogCreated");

                entity.HasOne(d => d.ChangeLogDeletedNavigation)
                    .WithMany(p => p.ToSicEavEntitiesChangeLogDeletedNavigation)
                    .HasForeignKey(d => d.ChangeLogDeleted)
                    .HasConstraintName("FK_ToSIC_EAV_Entities_ToSIC_EAV_ChangeLogDeleted");

                entity.HasOne(d => d.ChangeLogModifiedNavigation)
                    .WithMany(p => p.ToSicEavEntitiesChangeLogModifiedNavigation)
                    .HasForeignKey(d => d.ChangeLogModified)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Entities_ToSIC_EAV_ChangeLog_Modified");

                entity.HasOne(d => d.ConfigurationSetNavigation)
                    .WithMany(p => p.InverseConfigurationSetNavigation)
                    .HasForeignKey(d => d.ConfigurationSet)
                    .HasConstraintName("FK_ToSIC_EAV_Entities_ToSIC_EAV_Entities");
            });

            modelBuilder.Entity<ToSicEavEntityRelationships>(entity =>
            {
                entity.HasKey(e => new { e.AttributeId, e.ParentEntityId, e.SortOrder })
                    .HasName("PK_ToSIC_EAV_EntityRelationships");

                entity.ToTable("ToSIC_EAV_EntityRelationships");

                entity.Property(e => e.AttributeId).HasColumnName("AttributeID");

                entity.Property(e => e.ParentEntityId).HasColumnName("ParentEntityID");

                entity.Property(e => e.ChildEntityId).HasColumnName("ChildEntityID");

                entity.HasOne(d => d.Attribute)
                    .WithMany(p => p.ToSicEavEntityRelationships)
                    .HasForeignKey(d => d.AttributeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_EntityRelationships_ToSIC_EAV_Attributes");

                entity.HasOne(d => d.ChildEntity)
                    .WithMany(p => p.RelationshipsWithThisAsChild)
                    .HasForeignKey(d => d.ChildEntityId)
                    .HasConstraintName("FK_ToSIC_EAV_EntityRelationships_ToSIC_EAV_ChildEntities");

                entity.HasOne(d => d.ParentEntity)
                    .WithMany(p => p.RelationshipsWithThisAsParent)
                    .HasForeignKey(d => d.ParentEntityId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_EntityRelationships_ToSIC_EAV_ParentEntities");
            });

            modelBuilder.Entity<ToSicEavValues>(entity =>
            {
                entity.HasKey(e => e.ValueId)
                    .HasName("PK_ToSIC_EAV_Values");

                entity.ToTable("ToSIC_EAV_Values");

                entity.HasIndex(e => new { e.AttributeId, e.EntityId, e.ChangeLogDeleted })
                    .HasName("IX_EAV_Values1");

                // 2017-04-28 disabled e.Value in this index - on one hand it's useless, but it also seems to affect sql queries to 450 chars on that field!
                entity.HasIndex(e => new { /*e.Value,*/ e.ChangeLogCreated, e.EntityId, e.ChangeLogDeleted, e.AttributeId, e.ValueId })
                    .HasName("IX_EAV_Values2");

                entity.Property(e => e.ValueId).HasColumnName("ValueID");

                entity.Property(e => e.AttributeId).HasColumnName("AttributeID");

                entity.Property(e => e.EntityId).HasColumnName("EntityID");

                entity.Property(e => e.Value).IsRequired();

                entity.HasOne(d => d.Attribute)
                    .WithMany(p => p.ToSicEavValues)
                    .HasForeignKey(d => d.AttributeId)
                    .HasConstraintName("FK_ToSIC_EAV_Values_ToSIC_EAV_Attributes");

                entity.HasOne(d => d.ChangeLogCreatedNavigation)
                    .WithMany(p => p.ToSicEavValuesChangeLogCreatedNavigation)
                    .HasForeignKey(d => d.ChangeLogCreated)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Values_ToSIC_EAV_ChangeLogCreated");

                entity.HasOne(d => d.ChangeLogDeletedNavigation)
                    .WithMany(p => p.ToSicEavValuesChangeLogDeletedNavigation)
                    .HasForeignKey(d => d.ChangeLogDeleted)
                    .HasConstraintName("FK_ToSIC_EAV_Values_ToSIC_EAV_ChangeLogDeleted");

                entity.HasOne(d => d.ChangeLogModifiedNavigation)
                    .WithMany(p => p.ToSicEavValuesChangeLogModifiedNavigation)
                    .HasForeignKey(d => d.ChangeLogModified)
                    .HasConstraintName("FK_ToSIC_EAV_Values_ToSIC_EAV_ChangeLogModified");

                entity.HasOne(d => d.Entity)
                    .WithMany(p => p.ToSicEavValues)
                    .HasForeignKey(d => d.EntityId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_Values_ToSIC_EAV_Entities");
            });

            modelBuilder.Entity<ToSicEavValuesDimensions>(entity =>
            {
                entity.HasKey(e => new { e.ValueId, e.DimensionId })
                    .HasName("PK_ToSIC_EAV_ValuesDimensions");

                entity.ToTable("ToSIC_EAV_ValuesDimensions");

                entity.Property(e => e.ValueId).HasColumnName("ValueID");

                entity.Property(e => e.DimensionId).HasColumnName("DimensionID");

                entity.Property(e => e.ReadOnly).HasDefaultValueSql("0");

                entity.HasOne(d => d.Dimension)
                    .WithMany(p => p.ToSicEavValuesDimensions)
                    .HasForeignKey(d => d.DimensionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_ValuesDimensions_ToSIC_EAV_Dimensions");

                entity.HasOne(d => d.Value)
                    .WithMany(p => p.ToSicEavValuesDimensions)
                    .HasForeignKey(d => d.ValueId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ToSIC_EAV_ValuesDimensions_ToSIC_EAV_Values");
            });

            modelBuilder.Entity<ToSicEavZones>(entity =>
            {
                entity.HasKey(e => e.ZoneId)
                    .HasName("PK_ToSIC_EAV_Zones");

                entity.ToTable("ToSIC_EAV_Zones");

                entity.Property(e => e.ZoneId).HasColumnName("ZoneID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });
        }
    }
}