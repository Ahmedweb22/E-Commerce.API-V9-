using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace E_Commerce.API_V9_.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }  
        public DbSet<ProductSubImg> ProductSubImgs { get; set; } 
        public DbSet<ProductColor> ProductColors { get; set; } 
        public DbSet<Brand> Brands { get; set; } 
        public DbSet<Catgeory> Categories { get; set; } 
            public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }
     public DbSet<Cart> Carts { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImg> ReviewImgs { get; set; }

    }
}