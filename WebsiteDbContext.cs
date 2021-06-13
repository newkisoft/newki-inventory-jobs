using Microsoft.EntityFrameworkCore;
using newkilibraries;
using newkilibraries.website;

namespace newki_inventory_jobs
{
    public class WebsiteDbContext :DbContext
    {
        public WebsiteDbContext(DbContextOptions options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Order>().ToTable("Order");
            builder.Entity<WebsitePallet>().ToTable("WebsitePallet");
            builder.Entity<OrderProduct>().ToTable("OrderProduct").HasKey(p=>new {p.ProductId, p.OrderId});

        }
        
        public DbSet<News> News{get;set;}
        public DbSet<Product> Product{get;set;}
        public DbSet<Order> Orders{get;set;}
        public DbSet<OrderProduct> OrderProducts{get;set;}
        public DbSet<OnlineProduct> OnlineProduct{get;set;}
        public DbSet<newkilibraries.WebsitePallet> WebsitePallet{get;set;}
        public DbSet<DtyPriceArchive> DtyPriceArchive{get;set;}
        public DbSet<FdyPriceArchive> FdyPriceArchive{get;set;}
        public DbSet<BcfPriceArchive> BcfPriceArchive{get;set;}
        public DbSet<TfoPriceArchive> TfoPriceArchive{get;set;}
        public DbSet<ShwPriceArchive> ShwPriceArchive{get;set;}
        public DbSet<SpandexPriceArchive> SpandexPriceArchive{get;set;}

    }

}