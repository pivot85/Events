﻿// <auto-generated />
using System;
using Events.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Events.Data.Migrations
{
    [DbContext(typeof(EventsDbContext))]
    [Migration("20211119203825_SwitchedToDiscordEventIds")]
    partial class SwitchedToDiscordEventIds
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("Events.Data.Models.Event", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("AttendeeRole")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Category")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ControlChannel")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ControlPanel")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("CosmeticRole")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<ulong>("EventPanel")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Guild")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<ulong>("Organiser")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("ShortName")
                        .HasColumnType("longtext");

                    b.Property<ulong>("SpeakerRole")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("Start")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("StewardRole")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("TextChannel")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.Property<ulong>("VoiceChannel")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Events.Data.Models.PermittedRole", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("PermittedRoles");
                });
#pragma warning restore 612, 618
        }
    }
}