using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Cafe2.Models;

namespace Cafe2;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DishInOrder> DishInOrders { get; set; }

    public virtual DbSet<EmployeeStatus> EmployeeStatuses { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<HistoryUser> HistoryUsers { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PymentMethod> PymentMethods { get; set; }

    public virtual DbSet<PymentStatus> PymentStatuses { get; set; }

    public virtual DbSet<ReadyStatus> ReadyStatuses { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    public virtual DbSet<TypeWorksShift> TypeWorksShifts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDocument> UserDocuments { get; set; }

    public virtual DbSet<WorkersShift> WorkersShifts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=2583");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_catalog", "adminpack");

        modelBuilder.Entity<DishInOrder>(entity =>
        {
            entity.HasKey(e => e.IddishInOrder).HasName("DishInOrder_pkey");

            entity.ToTable("DishInOrder", "cafe");

            entity.Property(e => e.IddishInOrder)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDDishInOrder");
            entity.Property(e => e.Iddish).HasColumnName("IDDish");
            entity.Property(e => e.Idorder).HasColumnName("IDOrder");

            entity.HasOne(d => d.IddishNavigation).WithMany(p => p.DishInOrders)
                .HasForeignKey(d => d.Iddish)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IDDish_fkay");

            entity.HasOne(d => d.IdorderNavigation).WithMany(p => p.DishInOrders)
                .HasForeignKey(d => d.Idorder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IDOrder_fkay");
        });

        modelBuilder.Entity<EmployeeStatus>(entity =>
        {
            entity.HasKey(e => e.IdemployeeStatus).HasName("EmployeeStatus_pkey");

            entity.ToTable("EmployeeStatus", "cafe");

            entity.Property(e => e.IdemployeeStatus)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDEmployeeStatus");
            entity.Property(e => e.NameStatus).HasMaxLength(100);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Idgroup).HasName("Group_pkey");

            entity.ToTable("Group", "cafe");

            entity.Property(e => e.Idgroup)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDGroup");
            entity.Property(e => e.NameGroup).HasMaxLength(100);
        });

        modelBuilder.Entity<HistoryUser>(entity =>
        {
            entity.HasKey(e => e.IdhistoryUsers).HasName("HistoryUsers_pkey");

            entity.ToTable("HistoryUsers", "cafe");

            entity.Property(e => e.IdhistoryUsers)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDHistoryUsers");
            entity.Property(e => e.Iduser).HasColumnName("IDUser");

            entity.HasOne(d => d.IduserNavigation).WithMany(p => p.HistoryUsers)
                .HasForeignKey(d => d.Iduser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserId_fkey");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.HistoryUsers)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EmployeeStatus_fkey");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Iddish).HasName("Menu_pkey");

            entity.ToTable("Menu", "cafe");

            entity.Property(e => e.Iddish)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDDish");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.NameDish).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("money");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Idorder).HasName("Order_pkey");

            entity.ToTable("Order", "cafe");

            entity.Property(e => e.Idorder)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDOrder");
           // entity.Property(e => e.IddetailOrderId).HasColumnName("IDDetailOrderId");
            entity.Property(e => e.IdpaymentMethod).HasColumnName("IDPaymentMethod");
            entity.Property(e => e.IdpaymentStatus).HasColumnName("IDPaymentStatus");
            entity.Property(e => e.IdreadyStatus).HasColumnName("IDReadyStatus");
            entity.Property(e => e.IdtableNumber).HasColumnName("IDTableNumber");

            entity.HasOne(d => d.IdpaymentMethodNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdpaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentMethod_fkey");

            entity.HasOne(d => d.IdpaymentStatusNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdpaymentStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentStatus_fkey");

            entity.HasOne(d => d.IdreadyStatusNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdreadyStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ReadyStatus_fkey");

            entity.HasOne(d => d.IdtableNumberNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdtableNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TableNumber_fkey");
        });

        modelBuilder.Entity<PymentMethod>(entity =>
        {
            entity.HasKey(e => e.IdpymentMethods).HasName("PymentMethods_pkey");

            entity.ToTable("PymentMethods", "cafe");

            entity.Property(e => e.IdpymentMethods)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDPymentMethods");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PymentStatus>(entity =>
        {
            entity.HasKey(e => e.IdpymentStatus).HasName("PymentStatus_pkey");

            entity.ToTable("PymentStatus", "cafe");

            entity.Property(e => e.IdpymentStatus)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDPymentStatus");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<ReadyStatus>(entity =>
        {
            entity.HasKey(e => e.IdreadyStatus).HasName("ReadyStatus_pkey");

            entity.ToTable("ReadyStatus", "cafe");

            entity.Property(e => e.IdreadyStatus)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDReadyStatus");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Idtable).HasName("Tables_pkey");

            entity.ToTable("Tables", "cafe");

            entity.Property(e => e.Idtable)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDTable");
            entity.Property(e => e.NameTable).HasColumnType("character varying");
            entity.Property(e => e.UserTableId).HasColumnName("UserTableID");

            entity.HasOne(d => d.UserTable).WithMany(p => p.Tables)
                .HasForeignKey(d => d.UserTableId)
                .HasConstraintName("UserTableId_fkey");
        });

        modelBuilder.Entity<TypeWorksShift>(entity =>
        {
            entity.HasKey(e => e.IdtypeWorksShift).HasName("TypeWorksShift_pkey");

            entity.ToTable("TypeWorksShift", "cafe");

            entity.Property(e => e.IdtypeWorksShift)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDTypeWorksShift");
            entity.Property(e => e.WorkingRate).HasColumnType("money");
            entity.Property(e => e.StartWorksShift).HasColumnType("timestamp with time zone");
            entity.Property(e => e.EndWorksShift).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Iduser).HasName("Users_pkey");

            entity.ToTable("Users", "cafe");

            entity.Property(e => e.Iduser)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDUser");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Iddocuments).HasColumnName("IDDocuments");
            entity.Property(e => e.Idgroup).HasColumnName("IDGroup");
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.SecondName).HasMaxLength(100);

            entity.HasOne(d => d.IddocumentsNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Iddocuments)
                .HasConstraintName("UserDocument_fkey");

            entity.HasOne(d => d.IdgroupNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Idgroup)
                .HasConstraintName("Group_fkey");
        });

        modelBuilder.Entity<UserDocument>(entity =>
        {
            entity.HasKey(e => e.IduserDocument).HasName("UserDocument_pkey");

            entity.ToTable("UserDocument", "cafe");

            entity.Property(e => e.IduserDocument)
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDUserDocument");
        });

        modelBuilder.Entity<WorkersShift>(entity =>
        {
            entity.HasKey(e => e.IdworkersShift).HasName("WorkersShift_pkey");

            entity.ToTable("WorkersShift", "cafe");

            entity.Property(e => e.IdworkersShift)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("IDWorkersShift");
            entity.Property(e => e.IdtypeWorkShift).HasColumnName("IDTypeWorkShift");
            entity.Property(e => e.Iduser).HasColumnName("IDUser");

            entity.HasOne(d => d.IduserNavigation).WithMany(p => p.WorkersShifts)
                .HasForeignKey(d => d.Iduser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IDUser_fkey");

            entity.HasOne(d => d.IdworkersShiftNavigation).WithOne(p => p.WorkersShift)
                .HasForeignKey<WorkersShift>(d => d.IdworkersShift)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IDTypeWorkShift_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
