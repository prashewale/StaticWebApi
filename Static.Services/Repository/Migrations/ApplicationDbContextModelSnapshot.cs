﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Static.Services.Repository.DbContexts;

#nullable disable

namespace Static.Services.Repository.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Static.Services.Models.Instrument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Exchange")
                        .HasColumnType("int");

                    b.Property<string>("InstrumentName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Instruments");
                });

            modelBuilder.Entity("Static.Services.Models.OptionCandle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ClosePrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("Delta")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<decimal?>("Gamma")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("ImpliedFuture")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("ImpliedValume")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("OptionType")
                        .HasColumnType("int");

                    b.Property<decimal?>("Rho")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("StrikeId")
                        .HasColumnType("int");

                    b.Property<decimal?>("Theta")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("StrikeId");

                    b.ToTable("OptionCandles");
                });

            modelBuilder.Entity("Static.Services.Models.OptionExpiry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("InstrumentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InstrumentId");

                    b.ToTable("OptionExpiries");
                });

            modelBuilder.Entity("Static.Services.Models.Strike", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("OptionExpiryId")
                        .HasColumnType("int");

                    b.Property<long>("StrikePrice")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("OptionExpiryId");

                    b.ToTable("Strikes");
                });

            modelBuilder.Entity("Static.Services.Models.OptionCandle", b =>
                {
                    b.HasOne("Static.Services.Models.Strike", "MyStrike")
                        .WithMany("ListOfOptionCadles")
                        .HasForeignKey("StrikeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MyStrike");
                });

            modelBuilder.Entity("Static.Services.Models.OptionExpiry", b =>
                {
                    b.HasOne("Static.Services.Models.Instrument", "MyInstrument")
                        .WithMany("ListOfOptionExpiries")
                        .HasForeignKey("InstrumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MyInstrument");
                });

            modelBuilder.Entity("Static.Services.Models.Strike", b =>
                {
                    b.HasOne("Static.Services.Models.OptionExpiry", "OptionExpiry")
                        .WithMany("ListOfStrikes")
                        .HasForeignKey("OptionExpiryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OptionExpiry");
                });

            modelBuilder.Entity("Static.Services.Models.Instrument", b =>
                {
                    b.Navigation("ListOfOptionExpiries");
                });

            modelBuilder.Entity("Static.Services.Models.OptionExpiry", b =>
                {
                    b.Navigation("ListOfStrikes");
                });

            modelBuilder.Entity("Static.Services.Models.Strike", b =>
                {
                    b.Navigation("ListOfOptionCadles");
                });
#pragma warning restore 612, 618
        }
    }
}
