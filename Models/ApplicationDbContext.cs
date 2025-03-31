using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace nhom5_webAPI.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet cho các bảng
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetCategory> PetCategories { get; set; }
        public DbSet<PetImages> PetImages { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceImage> ServiceImages { get; set; }
        public DbSet<PetSupplyCategory> PetSupplyCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Quan hệ giữa Appointment và User
            builder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Appointment và Service
            //builder.Entity<Appointment>()
            //    .HasOne(a => a.Service)
            //    .WithMany(s => s.Appointments)
            //    .HasForeignKey(a => a.ServiceId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ giữa Order và User
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ giữa OrderDetail và Order
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Product và ProductImages
            builder.Entity<ProductImages>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Pet và PetCategory
            builder.Entity<Pet>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Pets)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ giữa Pet và PetImages
            builder.Entity<PetImages>()
                .HasOne(pi => pi.Pet)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Product và PetSupplyCategory
            builder.Entity<Product>()
                .HasOne(p => p.SupplyCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(p => p.SupplyCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ giữa Service và ServiceImage
            builder.Entity<ServiceImage>()
                .HasOne(si => si.Service)
                .WithMany(s => s.Images)
                .HasForeignKey(si => si.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Chỉ mục tối ưu hóa
            builder.Entity<Pet>()
                .HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Pet_CategoryId");

            builder.Entity<Product>()
                .HasIndex(p => p.SupplyCategoryId)
                .HasDatabaseName("IX_Product_SupplyCategoryId");

            // Seed Data: Pet Categories
            builder.Entity<PetCategory>().HasData(
                new PetCategory { CategoryId = 1, Name = "Dogs" },
                new PetCategory { CategoryId = 2, Name = "Cats" },
                new PetCategory { CategoryId = 3, Name = "Birds" },
                new PetCategory { CategoryId = 4, Name = "Fish" },
                new PetCategory { CategoryId = 5, Name = "Hamsters" }
            );

            // Seed Data: Pet Supply Categories
            builder.Entity<PetSupplyCategory>().HasData(
                new PetSupplyCategory { SupplyCategoryId = 1, Name = "Đồ ăn" },
                new PetSupplyCategory { SupplyCategoryId = 2, Name = "Đồ chơi" },
                new PetSupplyCategory { SupplyCategoryId = 3, Name = "Phụ kiện chăm sóc và vệ sinh" },
                new PetSupplyCategory { SupplyCategoryId = 4, Name = "Sản phẩm làm đẹp và chăm sóc sức khỏe" },
                new PetSupplyCategory { SupplyCategoryId = 5, Name = "Dụng cụ vận chuyển và giải trí ngoài trời" }
            );
        }
    }
}
