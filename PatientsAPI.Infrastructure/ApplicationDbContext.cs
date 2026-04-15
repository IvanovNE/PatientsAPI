using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PatientsAPI.Domain.Common;
using PatientsAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PatientsAPI.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Patient> Patients => Set<Patient>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");

                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedNever();

                entity.Property(p => p.Gender)
                    .HasConversion<string>()
                    .HasMaxLength(10);

                entity.Property(p => p.BirthDate)
                    .HasColumnName("birth_date")
                    .HasColumnType("timestamp")  
                    .IsRequired();

                entity.Property(p => p.Active)
                    .HasColumnName("active")
                    .IsRequired();

                entity.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.OwnsOne(p => p.Name, name =>
                {
                    name.Property(n => n.Id)
                        .HasColumnName("name_id")
                        .IsRequired();

                    name.Property(n => n.Use)
                        .HasColumnName("name_use")
                        .HasConversion<string>()
                        .HasMaxLength(20);

                    name.Property(n => n.Family)
                        .HasColumnName("name_family")
                        .IsRequired()
                        .HasMaxLength(100);

                    var stringListComparer = new ValueComparer<IReadOnlyList<string>>(
                        (c1, c2) => c1 != null && c2 != null
                            ? c1.SequenceEqual(c2)
                            : c1 == null && c2 == null,
                        c => c != null
                            ? c.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()))
                            : 0,
                        c => c != null ? c.ToList() : new List<string>());

                    name.Property(n => n.Given)
                        .HasColumnName("name_given")
                        .HasColumnType("jsonb")
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                            v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>(),
                            stringListComparer
                        );
                });

                entity.HasIndex(p => p.BirthDate)
                    .HasDatabaseName("ix_patients_birth_date");
            });

            modelBuilder.Entity<Patient>().OwnsOne(p => p.Name)
                .HasIndex(n => n.Family)
                .HasDatabaseName("ix_patients_name_family");
        }
    }
}
