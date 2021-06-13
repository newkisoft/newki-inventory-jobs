using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace newki_inventory_jobs.Services
{
    public interface ICustomerService 
    {
        void Run();
    }
    public class CustomerService:ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Run()
        {
            Console.WriteLine("Starting the process ...");
           var customers = _context.Customer.ToList();
           foreach(var customer in customers)
           {
               var totalDebt = _context.Invoice.Where(p=>p.Customer.CustomerId == customer.CustomerId).Sum(w=>w.TotalUsd);
               var totalUsdPaid = _context.Invoice.Where(p=>p.Customer.CustomerId == customer.CustomerId && p.Currency == "Usd").Sum(w=>w.Paid);
               var totalLiraPaid = _context.Invoice.Where(p=>p.Customer.CustomerId == customer.CustomerId && p.Currency == "Lira").Sum(w=>w.Paid/w.ExchangeRate);
               var difference = totalDebt - totalUsdPaid + totalLiraPaid;
           }           
            Console.WriteLine("Done!");
        }

    }
    
}