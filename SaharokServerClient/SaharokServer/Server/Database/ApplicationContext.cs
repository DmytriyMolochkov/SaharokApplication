using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Castle.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Internal;
using SaharokServer.Server.Database.Views;

namespace SaharokServer.Server.Database
{
    class ApplicationContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<SessionUser> SessionUser { get; set; }
        public DbSet<SessionAdmin> SessionAdmin { get; set; }
        public DbSet<RequestResponse> RequestResponse { get; set; }
        public DbSet<BannedResponse> AnswerBanned { get; set; }
        public DbSet<ErrorUser> ErrorUser { get; set; }
        public DbSet<ErrorAdmin> ErrorAdmin { get; set; }
        public DbSet<ErrorServerObject> ErrorServerObject { get; set; }
        public DbSet<ViewSessionUser> ViewSessionUsers { get; set; }
        public DbSet<ViewSessionAdmin> ViewSessionAdmins { get; set; }
        public DbSet<ViewUser> ViewUser { get; set; }
        public DbSet<ViewAdmin> ViewAdmin { get; set; }
        public DbSet<ViewRequestResponse> ViewRequestResponse { get; set; }
        public DbSet<ViewErrorUser> ViewErrorUser { get; set; }
        public DbSet<ViewErrorAdmin> ViewErrorAdmin { get; set; }
        public DbSet<ViewErrorServerObject> ViewErrorServerObject { get; set; }
        public DbSet<ViewSessionAdmin> ViewErrorResponse { get; set; }
        public DbSet<ViewSessionAdmin> ViewBannedResponse { get; set; }

        public string Name { get; set; }

        public ApplicationContext(string name)
        {
            Name = name;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseLazyLoadingProxies()
            //.UseMySQL("server=localhost;UserId=qwe;Password=qwe;database=saharok;");
            .UseMySQL($"server=185.104.114.109;UserId=root;Password=3J*3h1[vFeF(03_+z;database={Name};");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ViewSessionUser>((e =>
            {
                e.HasNoKey();
                e.ToView("view_session_user");
            }));

            modelBuilder.Entity<ViewSessionAdmin>((e =>
            {
                e.HasNoKey();
                e.ToView("view_session_admin");
            }));
            modelBuilder.Entity<ViewUser>((e =>
            {
                e.HasNoKey();
                e.ToView("view_user");
            }));
            modelBuilder.Entity<ViewAdmin>((e =>
            {
                e.HasNoKey();
                e.ToView("view_admin");
            }));
            modelBuilder.Entity<ViewRequestResponse>((e =>
            {
                e.HasNoKey();
                e.ToView("view_request_response");
            }));
            modelBuilder.Entity<ViewErrorUser>((e =>
            {
                e.HasNoKey();
                e.ToView("view_error_user");
            }));
            modelBuilder.Entity<ViewErrorAdmin>((e =>
            {
                e.HasNoKey();
                e.ToView("view_error_admin");
            }));
            modelBuilder.Entity<ViewErrorServerObject>((e =>
            {
                e.HasNoKey();
                e.ToView("view_error_server_object");
            }));
            modelBuilder.Entity<ViewErrorResponse>((e =>
            {
                e.HasNoKey();
                e.ToView("view_error_response");
            }));
            modelBuilder.Entity<ViewBannedResponse>((e =>
            {
                e.HasNoKey();
                e.ToView("view_error_bannes_response");
            }));

            //modelBuilder.Entity<Department>()
            //    .HasAlternateKey(d => d.MyDepartmentID)
            //    ;

            //modelBuilder.Entity<Course>()
            //    .HasOne(p => p.Department)
            //    .WithMany(t => t.Courses)
            //    .HasForeignKey(p => p.DepartmentID)
            //    .HasPrincipalKey(t => t.MyDepartmentID)
            //    ;


            //modelBuilder.Entity<Department>()
            //    .HasPrincipalKey(t => t.DepartmentID);
        }
    }
}
