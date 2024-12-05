﻿// <auto-generated />
using EnglishVietnameseDictionaryApi.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EnglishVietnameseDictionaryApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Example", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MeaningId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TranslatedText")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MeaningId");

                    b.ToTable("Examples");
                });

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Meaning", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PartOfSpeech")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("VietnameseText")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("WordId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("WordId");

                    b.ToTable("Meanings");
                });

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Word", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Audio")
                        .HasColumnType("TEXT");

                    b.Property<string>("EnglishText")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Phonetic")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EnglishText")
                        .IsUnique();

                    b.ToTable("Words");
                });

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Example", b =>
                {
                    b.HasOne("EnglishVietnameseDictionaryApi.Models.Meaning", "Meaning")
                        .WithMany("Examples")
                        .HasForeignKey("MeaningId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Meaning");
                });

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Meaning", b =>
                {
                    b.HasOne("EnglishVietnameseDictionaryApi.Models.Word", "Word")
                        .WithMany("Meanings")
                        .HasForeignKey("WordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Word");
                });

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Meaning", b =>
                {
                    b.Navigation("Examples");
                });

            modelBuilder.Entity("EnglishVietnameseDictionaryApi.Models.Word", b =>
                {
                    b.Navigation("Meanings");
                });
#pragma warning restore 612, 618
        }
    }
}
