﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WIPSystem.Web.Data;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    [DbContext(typeof(WIPDbContext))]
    [Migration("20231207114704_AddJigs")]
    partial class AddJigs
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WIPSystem.Web.Models.Jig", b =>
                {
                    b.Property<int>("JigId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("JigId"));

                    b.Property<int>("JigLifeSpan")
                        .HasColumnType("int");

                    b.Property<string>("JigName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MachineId")
                        .HasColumnType("int");

                    b.HasKey("JigId");

                    b.HasIndex("MachineId");

                    b.ToTable("Jigs");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.LotTraveller", b =>
                {
                    b.Property<int>("LotTravellerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LotTravellerId"));

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("LotNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("LotTravellerId");

                    b.HasIndex("ProductId");

                    b.ToTable("LotTravellers");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Machine", b =>
                {
                    b.Property<int>("MachineId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MachineId"));

                    b.Property<string>("MachineName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MachineQty")
                        .HasColumnType("int");

                    b.Property<int>("ProcessId")
                        .HasColumnType("int");

                    b.HasKey("MachineId");

                    b.HasIndex("ProcessId");

                    b.ToTable("Machines");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Process", b =>
                {
                    b.Property<int>("ProcessId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProcessId"));

                    b.Property<int>("ProcessCode")
                        .HasColumnType("int");

                    b.Property<string>("ProcessName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ProcessId");

                    b.ToTable("Process");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductId"));

                    b.Property<string>("CustName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PackageSize")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PartNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PiecesPerBlank")
                        .HasColumnType("int");

                    b.HasKey("ProductId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.ProductProcessMapping", b =>
                {
                    b.Property<int>("ProductProcessMappingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductProcessMappingId"));

                    b.Property<int>("ProcessCode")
                        .HasColumnType("int");

                    b.Property<int>("ProcessId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Sequence")
                        .HasColumnType("int");

                    b.HasKey("ProductProcessMappingId");

                    b.HasIndex("ProcessId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductProcessMappings");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.SplitDetail", b =>
                {
                    b.Property<int>("SplitDetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SplitDetailId"));

                    b.Property<string>("Camber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LotNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int>("SplitLotId")
                        .HasColumnType("int");

                    b.HasKey("SplitDetailId");

                    b.HasIndex("SplitLotId");

                    b.ToTable("SplitDetails");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.SplitLot", b =>
                {
                    b.Property<int>("SplitLotId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SplitLotId"));

                    b.Property<string>("Camber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmpNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LotNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OriginalLot")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PartNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("SplitSuffix")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SplitLotId");

                    b.ToTable("SplitLot");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Jig", b =>
                {
                    b.HasOne("WIPSystem.Web.Models.Machine", "Machine")
                        .WithMany("Jigs")
                        .HasForeignKey("MachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Machine");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.LotTraveller", b =>
                {
                    b.HasOne("WIPSystem.Web.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Machine", b =>
                {
                    b.HasOne("WIPSystem.Web.Models.Process", "Process")
                        .WithMany("Machines")
                        .HasForeignKey("ProcessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Process");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.ProductProcessMapping", b =>
                {
                    b.HasOne("WIPSystem.Web.Models.Process", "Process")
                        .WithMany("ProductProcessMappings")
                        .HasForeignKey("ProcessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WIPSystem.Web.Models.Product", "Product")
                        .WithMany("ProductProcessMappings")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Process");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.SplitDetail", b =>
                {
                    b.HasOne("WIPSystem.Web.Models.SplitLot", "SplitLot")
                        .WithMany("SplitDetails")
                        .HasForeignKey("SplitLotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SplitLot");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Machine", b =>
                {
                    b.Navigation("Jigs");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Process", b =>
                {
                    b.Navigation("Machines");

                    b.Navigation("ProductProcessMappings");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.Product", b =>
                {
                    b.Navigation("ProductProcessMappings");
                });

            modelBuilder.Entity("WIPSystem.Web.Models.SplitLot", b =>
                {
                    b.Navigation("SplitDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
