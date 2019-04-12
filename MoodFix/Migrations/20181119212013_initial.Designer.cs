﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MoodFix.Models;

namespace MoodFix.Migrations
{
    [DbContext(typeof(MoodFixContext))]
    [Migration("20181119212013_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MoodFix.Models.Fix", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("backvalue")
                        .HasMaxLength(100);

                    b.Property<string>("heading")
                        .HasMaxLength(1000);

                    b.Property<string>("image")
                        .HasMaxLength(1000);

                    b.Property<string>("maintext")
                        .HasMaxLength(1000);

                    b.Property<string>("nextvalue")
                        .HasMaxLength(100);

                    b.HasKey("ID");

                    b.ToTable("Fix");
                });
#pragma warning restore 612, 618
        }
    }
}
