﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SeleniumResults.Repository;

namespace SeleniumResults.Migrations
{
    [DbContext(typeof(CollectorContext))]
    partial class CollectorContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7");

            modelBuilder.Entity("SeleniumResults.Repository.Models.TestResultDao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("FlytApplicationType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SubtestsJson")
                        .HasColumnType("TEXT");

                    b.Property<int>("TestResultType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TestRunId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TestRunType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Time")
                        .HasColumnType("TEXT");

                    b.Property<int>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TestResults");
                });

            modelBuilder.Entity("SeleniumResults.Repository.Models.TestRunDao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BuildNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FlytApplicationType")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastRun")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalFileName")
                        .HasColumnType("TEXT");

                    b.Property<int>("TestRunType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UniqueId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TestRuns");
                });
#pragma warning restore 612, 618
        }
    }
}
