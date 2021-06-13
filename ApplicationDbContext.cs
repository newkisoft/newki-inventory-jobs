using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace newki_inventory_jobs
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AgentCustomer>().HasKey(sc => new { sc.CustomerId, sc.AgentId });
            builder.Entity<InvoicePallet>().HasKey(sc => new { sc.InvoiceId, sc.PalletId });
            builder.Entity<InvoiceBox>().HasKey(sc => new { sc.InvoiceId, sc.BoxId });
            builder.Entity<InvoiceDocumentFile>().HasKey(sc => new { sc.InvoiceId, sc.DocumentFileId });
        }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<Pallet> Pallet { get; set; }
        public DbSet<InvoicePallet> InvoicePallet { get; set; }
        public DbSet<InvoiceBox> InvoiceBox { get; set; }
        public DbSet<Box> Box { get; set; }
        public DbSet<Customer> Customer{get;set;}      
        public DbSet<AlertDataView> AlertDataView{get;set;}      
    }

}