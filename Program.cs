using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using newki_inventory_jobs.Model;
using newki_inventory_jobs.Services;

namespace newki_inventory_jobs
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
              var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true);
            var Configuration = builder.Build();
            

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var websiteConnectionString = Configuration.GetConnectionString("WebsiteConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);      

             var websiteOptionsBuilder = new DbContextOptionsBuilder<WebsiteDbContext>();
            websiteOptionsBuilder.UseNpgsql(websiteConnectionString); 
            

            services.AddTransient<IDynamoDbContext, DynamoDbContext>();
            services.AddTransient<IInvoiceTable, InvoiceTable>();

            
            var serviceProvider = services.BuildServiceProvider();            
            var invoiceTable = serviceProvider.GetService<IInvoiceTable>();            

            using(var webDbContext = new WebsiteDbContext(websiteOptionsBuilder.Options))
            {
                var priceCorrection = new PriceCorrectionService(webDbContext);
                priceCorrection.Run();
            }
            
            using(var dbContext = new ApplicationDbContext(optionsBuilder.Options))
            {
                var correctionService = new WeightCorrectionService(dbContext);
                correctionService.Run();
            }

            using(var webDbContext = new WebsiteDbContext(websiteOptionsBuilder.Options))            
            using(var dbContext = new ApplicationDbContext(optionsBuilder.Options))
            {
                var exportService = new ExportPalletService(dbContext,webDbContext);
                exportService.Export();   
            }
            using(var webDbContext = new WebsiteDbContext(websiteOptionsBuilder.Options))            
            using(var dbContext = new ApplicationDbContext(optionsBuilder.Options))
            {
                var sync = new SyncInvoiceService(dbContext,webDbContext,invoiceTable);
                sync.StartInvoice();
                sync.GetWebsiteInvoices();         
            }

            using(var webDbContext = new WebsiteDbContext(websiteOptionsBuilder.Options))            
            using(var dbContext = new ApplicationDbContext(optionsBuilder.Options))
            {
                var inventoryService = new InventoryService(dbContext);
                inventoryService.Run(150);
            }
            
        }
    }
}
