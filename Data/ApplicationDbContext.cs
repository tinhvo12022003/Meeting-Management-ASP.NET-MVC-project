using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Models;
using MeetingManagement.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace MeetingManagement.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    public DbSet<AccountModel> Account { get; set; }
    public DbSet<CompanyModel> Company { get; set; }
    public DbSet<DepartmentModel> Department { get; set; }
    public DbSet<MeetingModel> Meeting { get; set; }
    public DbSet<MeetingRoomModel> MeetingRoom { get; set; }
    public DbSet<MeetingUserModel> MeetingUser { get; set; }
    public DbSet<PermissionModel> Permission { get; set; }
    public DbSet<UserModel> User { get; set; }
    public DbSet<RefreshTokenModel> RefreshToken { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseModel>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.BaseType == typeof(BaseModel) ||
                (entityType.ClrType.BaseType != null && entityType.ClrType.BaseType.IsGenericType && entityType.ClrType.BaseType.GetGenericTypeDefinition() == typeof(BaseModel)))
            {

                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreateAt")
                    .HasColumnType(typeName: "DATETIME2")
                    .HasColumnName(name: "CreateAt")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("UpdateAt")
                    .HasColumnType("DATETIME2")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");


                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("CreateBy")
                    .HasColumnType("NVARCHAR(100)")
                    .HasDefaultValue("SYSTEM");

                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("UpdateBy")
                    .HasColumnType(typeName: "NVARCHAR(100)")
                    .HasColumnName(name: "UpdateBy")
                    .HasDefaultValue("SYSTEM");
                
                // modelBuilder.Entity(entityType.ClrType)
                //     .Property<int>("RowStatus")
                //     .HasColumnType(typeName: "BIT")
                //     .HasColumnName(name: "RowStatus")
                //     .HasDefaultValue(true);
            }

        }

        modelBuilder.Entity<AccountModel>(entity =>
        {
            entity.ToTable(name: "Accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.Username).IsRequired().HasColumnName(name: "Username").HasColumnType(typeName: "NVARCHAR(100)");
            entity.Property(e => e.HashPassword).IsRequired().HasColumnName(name: "Password").HasMaxLength(150).HasColumnType(typeName: "NVARCHAR(150)");
            entity.Property(e => e.UserId).HasColumnName(name: "UserId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.User).WithOne(p => p.Account).HasForeignKey<AccountModel>(a => a.UserId).OnDelete(DeleteBehavior.NoAction).OnDelete(DeleteBehavior.NoAction);
        });


        modelBuilder.Entity<CompanyModel>(entity =>
        {
            entity.ToTable(name: "Companies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.Address).IsRequired().HasColumnName(name: "Address").HasColumnType(typeName: "NVARCHAR(255)");
            entity.Property(e => e.Name).IsRequired().HasColumnName(name: "Name").HasColumnType(typeName: "NVARCHAR(255)");
        });


        modelBuilder.Entity<DepartmentModel>(entity =>
        {
            entity.ToTable(name: "Departments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasColumnName(name: "Name").HasColumnType(typeName: "NVARCHAR(255)");
            entity.Property(e => e.CompanyId).IsRequired().HasColumnName(name: "CompanyId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Company).WithMany(p => p.Departments).HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.NoAction);
        });


        modelBuilder.Entity<MeetingModel>(entity =>
        {
            entity.ToTable(name: "Meetings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasColumnName(name: "Title").HasColumnType(typeName: "NVARCHAR(255)");
            entity.Property(e => e.StartAt).IsRequired().HasColumnName(name: "StartAt").HasColumnType(typeName: "DATETIME2");
            entity.Property(e => e.EndAt).IsRequired().HasColumnName(name: "EndAt").HasColumnType(typeName: "DATETIME2");
            entity.Property(e => e.Type).HasColumnName(name: "Type").HasColumnType(typeName: "TINYINT").HasConversion<int>();
            entity.Property(e => e.Description).HasColumnName(name: "Description").HasColumnType(typeName: "NVARCHAR(255)");
            entity.Property(e => e.Organization).HasColumnName(name: "Organization").HasColumnType(typeName: "NVARCHAR(255)").HasDefaultValue(null);
            entity.Property(e => e.RoomId).IsRequired().HasColumnName(name: "RoomId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.MeetingRoom).WithMany(p => p.Meetings).HasForeignKey(a => a.RoomId).OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.CompanyId).IsRequired().HasColumnName(name: "CompanyId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Company).WithMany(p => p.Meetings).HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.DepartmentId).IsRequired().HasColumnName(name: "DepartmentId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Department).WithMany(p => p.Meetings).HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.NoAction);
            entity.Property(x => x.Url).HasColumnName(name: "Url").HasColumnType(typeName: "NVARCHAR(255)");
        });

        modelBuilder.Entity<MeetingRoomModel>(entity =>
        {
            entity.ToTable(name: "MeetingRooms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasColumnName(name: "Name").HasColumnType(typeName: "NVARCHAR(255)");
            entity.Property(e => e.CompanyId).HasColumnName(name: "CompanyId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Company).WithMany(p => p.Rooms).HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.NoAction);

        });


        modelBuilder.Entity<MeetingUserModel>(entity =>
        {
            entity.ToTable(name: "MeetingUsers");
            entity.HasKey(mu => new { mu.UserId, mu.MeetingId });
            entity.Property(e => e.UserId).IsRequired().HasColumnName(name: "UserId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.User).WithMany(p => p.MeetingUser).HasForeignKey(a => a.UserId);
            entity.Property(e => e.MeetingId).IsRequired().HasColumnName(name: "MeetingId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Meeting).WithMany(p => p.MeetingUser).HasForeignKey(a => a.MeetingId).OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.Role).HasColumnName(name: "Role").HasColumnType(typeName: "TINYINT").HasConversion<int>();
            entity.Property(e => e.IsConfirmed).HasColumnName(name: "IsConfirmed").HasColumnType(typeName: "BIT").HasDefaultValue(true);
        });


        modelBuilder.Entity<PermissionModel>(entity =>
        {
            entity.ToTable(name: "Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id").HasColumnType("NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.Controller).HasColumnName(name: "Controller").HasColumnType(typeName: "NVARCHAR(50)");
            entity.Property(e => e.Action).HasColumnName(name: "Action").HasColumnType(typeName: "NVARCHAR(50)");
            entity.Property(e => e.FullPermission).HasColumnName(name: "FullPermission").HasColumnType(typeName: "BIT");
            entity.Property(e => e.View).HasColumnName(name: "View").HasColumnType(typeName: "BIT");
            entity.Property(e => e.Edit).HasColumnName(name: "Edit").HasColumnType(typeName: "BIT");
            entity.Property(e => e.Insert).HasColumnName(name: "Insert").HasColumnType(typeName: "BIT");
            entity.Property(e => e.Delete).HasColumnName(name: "Delete").HasColumnType(typeName: "BIT");
            entity.Property(e => e.EditAll).HasColumnName(name: "EditAll").HasColumnType(typeName: "BIT");
            entity.Property(e => e.InsertAll).HasColumnName(name: "InsertAll").HasColumnType(typeName: "BIT");
            entity.Property(e => e.DeleteAll).HasColumnName(name: "DeleteAll").HasColumnType(typeName: "BIT");
            entity.Property(e => e.UserId).HasColumnName(name: "UserId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.User).WithMany(p => p.Permissions).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();
            entity.Property(e => e.FullName).HasColumnName(name: "FullName").HasColumnType(typeName: "NVARCHAR(50)");
            entity.Property(e => e.Address).HasColumnName(name: "Address").HasColumnType(typeName: "NVARCHAR(255)");
            entity.Property(e => e.Phone).HasColumnName(name: "Phone").HasColumnType(typeName: "NVARCHAR(50)");
            entity.Property(e => e.Birthday).HasColumnName(name: "Birthday").HasColumnType(typeName: "DATE");
            entity.Property(e => e.Gender).HasColumnName(name: "Gender").HasColumnType(typeName: "TINYINT").HasConversion<int>();
            entity.Property(e => e.DepartmentId).HasColumnName(name: "DepartmentId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Department).WithMany(p => p.Users).HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.CompanyId).HasColumnName(name: "CompanyId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Company).WithMany(p => p.Users).HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RefreshTokenModel>(entity =>
        {
            entity.ToTable(name: "RefreshToken");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).IsRequired().HasColumnName(name: "Id").HasColumnType(typeName: "NVARCHAR(100)").HasValueGenerator<PrefixStringIdGenerator>().ValueGeneratedOnAdd();;
            entity.Property(e => e.TokenHash).HasColumnName(name: "TokenHash").HasColumnType(typeName: "NVARCHAR(1000)");
            entity.Property(e => e.ExpiresAt).HasColumnName(name: "ExpiresAt").HasColumnType(typeName: "DATETIME2");
            entity.Property(e => e.LoginAt).HasColumnName(name: "LoginAt").HasColumnType(typeName: "DATETIME2");
            entity.Property(e => e.RevokedAt).HasColumnName(name: "RevokedAt").HasColumnType(typeName: "DATETIME2");
            entity.Property(e => e.ReplacedByToken).HasColumnName(name: "ReplacedByToken").HasColumnType(typeName: "NVARCHAR(1000)");
            entity.Property(e => e.AccountId).IsRequired().HasColumnName(name: "AccountId").HasColumnType(typeName: "NVARCHAR(100)");
            entity.HasOne(a => a.Account).WithMany(p => p.RefreshTokens).HasForeignKey(a => a.AccountId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.AccountId);
        });

        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<BaseModel>().HasQueryFilter(e => e.rowStatus == RowStatus.ACTIVE);
    }
}
