﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VitalityProgramOnline.Data;

#nullable disable

namespace VitalityProgramOnline.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230923045205_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUser");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.FeedbackResponse", b =>
                {
                    b.Property<int>("ResponseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ResponseId"));

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("ResponseDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("ResponseId");

                    b.HasIndex("Id");

                    b.ToTable("FeedbackResponses");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.QuestionAnswerPair", b =>
                {
                    b.Property<int>("PairId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PairId"));

                    b.Property<string>("Answer")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Question")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ResponseDataId")
                        .HasColumnType("int");

                    b.HasKey("PairId");

                    b.HasIndex("ResponseDataId");

                    b.ToTable("QuestionAnswerPair");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.ResponseData", b =>
                {
                    b.Property<int>("ResponseDataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ResponseDataId"));

                    b.Property<int>("ResponseId")
                        .HasColumnType("int");

                    b.HasKey("ResponseDataId");

                    b.HasIndex("ResponseId")
                        .IsUnique();

                    b.ToTable("ResponseData");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.FoodDiary.FoodDiaryEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<double?>("CarbohydrateAmount")
                        .HasColumnType("float");

                    b.Property<double>("Cost")
                        .HasColumnType("float");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("DishName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EatPurpose")
                        .HasColumnType("int");

                    b.Property<double?>("FatAmount")
                        .HasColumnType("float");

                    b.Property<int>("MealTimeHours")
                        .HasColumnType("int");

                    b.Property<int>("MealTimeMinutes")
                        .HasColumnType("int");

                    b.Property<int>("MealType")
                        .HasColumnType("int");

                    b.Property<double?>("ProteinAmount")
                        .HasColumnType("float");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<string>("UserIdForeignKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<double?>("WaterAmount")
                        .HasColumnType("float");

                    b.Property<double?>("Weight")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("UserIdForeignKey");

                    b.ToTable("FoodDiary");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.User.ProgressUsers", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CurrentDay")
                        .HasColumnType("int");

                    b.Property<int>("CurrentStep")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateNextDay")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateTimeOfTheNextStep")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsTheNextDaysUpdateIsCompleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTheNextStepSheduledInTime")
                        .HasColumnType("bit");

                    b.Property<int>("UpdateState")
                        .HasColumnType("int");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("ProgressUsers");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.User.Settings.UserBotSettings", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<TimeSpan?>("EveningTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan?>("MorningTime")
                        .HasColumnType("time");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("UserBotSettings");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.User.ApplicationUser", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUser");

                    b.Property<string>("Allergic")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Diet")
                        .HasColumnType("int");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FitnessGoals")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("HasMedicalConditions")
                        .HasColumnType("bit");

                    b.Property<int?>("Height")
                        .HasColumnType("int");

                    b.Property<string>("InstagramUsername")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsAllergic")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsSmoker")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MedicalConditions")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumberTwo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TelegramUsername")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<double?>("Weight")
                        .HasColumnType("float");

                    b.HasDiscriminator().HasValue("ApplicationUser");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.FeedbackResponse", b =>
                {
                    b.HasOne("VitalityProgramOnline.Models.User.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.QuestionAnswerPair", b =>
                {
                    b.HasOne("VitalityProgramOnline.Models.Feedback.ResponseData", "ResponseData")
                        .WithMany("Responses")
                        .HasForeignKey("ResponseDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ResponseData");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.ResponseData", b =>
                {
                    b.HasOne("VitalityProgramOnline.Models.Feedback.FeedbackResponse", "FeedbackResponse")
                        .WithOne("ResponseData")
                        .HasForeignKey("VitalityProgramOnline.Models.Feedback.ResponseData", "ResponseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FeedbackResponse");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.FoodDiary.FoodDiaryEntry", b =>
                {
                    b.HasOne("VitalityProgramOnline.Models.User.ApplicationUser", "User")
                        .WithMany("FoodDiaryEntries")
                        .HasForeignKey("UserIdForeignKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.User.ProgressUsers", b =>
                {
                    b.HasOne("VitalityProgramOnline.Models.User.ApplicationUser", "User")
                        .WithOne("Progress")
                        .HasForeignKey("VitalityProgramOnline.Models.User.ProgressUsers", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.User.Settings.UserBotSettings", b =>
                {
                    b.HasOne("VitalityProgramOnline.Models.User.ApplicationUser", "User")
                        .WithOne("UserBotSettings")
                        .HasForeignKey("VitalityProgramOnline.Models.User.Settings.UserBotSettings", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.FeedbackResponse", b =>
                {
                    b.Navigation("ResponseData")
                        .IsRequired();
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.Feedback.ResponseData", b =>
                {
                    b.Navigation("Responses");
                });

            modelBuilder.Entity("VitalityProgramOnline.Models.User.ApplicationUser", b =>
                {
                    b.Navigation("FoodDiaryEntries");

                    b.Navigation("Progress")
                        .IsRequired();

                    b.Navigation("UserBotSettings")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}