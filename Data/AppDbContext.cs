using medical.Models;
using Microsoft.EntityFrameworkCore;

namespace medical.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DossierMedical> DossiersMedical { get; set; }
        public DbSet<Analyse> Analyses { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply check constraints for User
            //modelBuilder.Entity<User>()
            //    .HasCheckConstraint("CK_User_Genre", "\"Genre\" IN ('male', 'female', '')");

            //modelBuilder.Entity<User>()
            //    .HasCheckConstraint("CK_User_Role", "\"Role\" IN ('admin', 'doctor', 'patient')");

            //// Configure RendezVous table
            //modelBuilder.Entity<Appointment>()
            //    .HasCheckConstraint("CK_RendezVous_Status", "\"Status\" IN ('pending', 'approved', 'cancelled')");

            // Configure relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(r => r.Patient)
                .WithMany() // No inverse navigation in User for simplicity
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of User if referenced

            modelBuilder.Entity<Appointment>()
                .HasOne(r => r.Doctor)
                .WithMany() // No inverse navigation in User
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Analyse>()
            .HasOne(a => a.DossierMedical)
            .WithMany()
            .HasForeignKey(a => a.DossierMedicalId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}