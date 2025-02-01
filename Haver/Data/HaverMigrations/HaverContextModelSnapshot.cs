﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using haver.Data;

#nullable disable

namespace haver.Data.HaverMigrations
{
    [DbContext(typeof(HaverContext))]
    partial class HaverContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("haver.Models.Customer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Phone")
                        .IsUnique();

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("haver.Models.Engineer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Engineers");
                });

            modelBuilder.Entity("haver.Models.Machine", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal?>("ActualAssemblyHours")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AirSeal")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Base")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("BudgetedHours")
                        .HasColumnType("TEXT");

                    b.Property<string>("Class")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("CoatingLining")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Disassembly")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Media")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Nameplate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PreOrder")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProductionOrderNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Quantity")
                        .HasColumnType("INTEGER");

                    b.Property<decimal?>("ReworkHours")
                        .HasColumnType("TEXT");

                    b.Property<int>("SalesOrderID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Scope")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<string>("SerialNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Size")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SizeDeck")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("SparePMedia")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SpareParts")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ProductionOrderNumber")
                        .IsUnique();

                    b.HasIndex("SalesOrderID");

                    b.HasIndex("SerialNumber")
                        .IsUnique();

                    b.ToTable("Machines");
                });

            modelBuilder.Entity("haver.Models.PackageRelease", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PReleaseDateA")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PReleaseDateP")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SalesOrderID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("SalesOrderID")
                        .IsUnique();

                    b.HasIndex("Name", "SalesOrderID")
                        .IsUnique();

                    b.ToTable("PackageReleases");
                });

            modelBuilder.Entity("haver.Models.Procurement", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DeliveryDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpDueDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("PONumber")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SalesOrderID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VendorID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("SalesOrderID");

                    b.HasIndex("VendorID");

                    b.ToTable("Procurements");
                });

            modelBuilder.Entity("haver.Models.SalesOrder", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AppDwgRcd")
                        .HasColumnType("TEXT");

                    b.Property<string>("Comments")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("CustomerID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DwgIsDt")
                        .HasColumnType("TEXT");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("ShippingTerms")
                        .HasMaxLength(800)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SoDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("CustomerID");

                    b.HasIndex("OrderNumber")
                        .IsUnique();

                    b.ToTable("SalesOrders");
                });

            modelBuilder.Entity("haver.Models.SalesOrderEngineer", b =>
                {
                    b.Property<int>("SalesOrderID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EngineerID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("ID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("SalesOrderID", "EngineerID");

                    b.HasIndex("EngineerID");

                    b.ToTable("SalesOrderEngineers");
                });

            modelBuilder.Entity("haver.Models.Vendor", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Vendors");
                });

            modelBuilder.Entity("haver.Models.Machine", b =>
                {
                    b.HasOne("haver.Models.SalesOrder", "SalesOrder")
                        .WithMany("Machines")
                        .HasForeignKey("SalesOrderID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("SalesOrder");
                });

            modelBuilder.Entity("haver.Models.PackageRelease", b =>
                {
                    b.HasOne("haver.Models.SalesOrder", "SalesOrder")
                        .WithOne("PackageRelease")
                        .HasForeignKey("haver.Models.PackageRelease", "SalesOrderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SalesOrder");
                });

            modelBuilder.Entity("haver.Models.Procurement", b =>
                {
                    b.HasOne("haver.Models.SalesOrder", "SalesOrder")
                        .WithMany("Procurements")
                        .HasForeignKey("SalesOrderID");

                    b.HasOne("haver.Models.Vendor", "Vendor")
                        .WithMany("Procurements")
                        .HasForeignKey("VendorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SalesOrder");

                    b.Navigation("Vendor");
                });

            modelBuilder.Entity("haver.Models.SalesOrder", b =>
                {
                    b.HasOne("haver.Models.Customer", "Customer")
                        .WithMany("SalesOrders")
                        .HasForeignKey("CustomerID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("haver.Models.SalesOrderEngineer", b =>
                {
                    b.HasOne("haver.Models.Engineer", "Engineer")
                        .WithMany("SalesOrderEngineers")
                        .HasForeignKey("EngineerID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("haver.Models.SalesOrder", "SalesOrder")
                        .WithMany("SalesOrderEngineers")
                        .HasForeignKey("SalesOrderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Engineer");

                    b.Navigation("SalesOrder");
                });

            modelBuilder.Entity("haver.Models.Customer", b =>
                {
                    b.Navigation("SalesOrders");
                });

            modelBuilder.Entity("haver.Models.Engineer", b =>
                {
                    b.Navigation("SalesOrderEngineers");
                });

            modelBuilder.Entity("haver.Models.SalesOrder", b =>
                {
                    b.Navigation("Machines");

                    b.Navigation("PackageRelease");

                    b.Navigation("Procurements");

                    b.Navigation("SalesOrderEngineers");
                });

            modelBuilder.Entity("haver.Models.Vendor", b =>
                {
                    b.Navigation("Procurements");
                });
#pragma warning restore 612, 618
        }
    }
}
