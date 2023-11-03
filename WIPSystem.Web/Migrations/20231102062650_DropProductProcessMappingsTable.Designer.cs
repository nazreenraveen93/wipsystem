﻿// <auto-generated />
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
    [Migration("20231102062650_DropProductProcessMappingsTable")]
    partial class DropProductProcessMappingsTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

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
#pragma warning restore 612, 618
        }
    }
}