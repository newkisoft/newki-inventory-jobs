using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using newki_inventory_jobs.Model;
using newki_inventory_jobs.Model;
using newkilibraries;

namespace newki_inventory_jobs
{
    public interface ISyncInvoiceService 
    {
        void StartInvoice();
        void GetWebsiteInvoices();
    }
    public class SyncInvoiceService  :ISyncInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly WebsiteDbContext _websiteDbContext;
        private IInvoiceTable _invoiceTable;

        public SyncInvoiceService (ApplicationDbContext context,
                              WebsiteDbContext websiteDbContext,
                              IInvoiceTable invoiceTable)
        {
            _context = context;
            _websiteDbContext = websiteDbContext;
            _invoiceTable = invoiceTable;
        }

        public void StartInvoice()
        {
            Console.WriteLine("Start syncing invoices");
            var invoices = _context.Invoice
            .Include(p => p.Files).ThenInclude(w => w.File)
            .Include(p => p.Customer);
            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrEmpty(invoice.Customer.Gmail))
                {
                    var websiteInvoice = new WebsiteInvoice();
                    websiteInvoice.Id = invoice.InvoiceId.ToString();
                    websiteInvoice.Email = invoice.Customer.Gmail;
                    websiteInvoice.InvoiceDate = invoice.InvoiceDate.ToString();
                    websiteInvoice.InvoiceDueDate = invoice.InvoiceDueDate.ToString();
                    websiteInvoice.Kdv = invoice.Kdv.ToString();
                    websiteInvoice.Tax = invoice.Tax.ToString();
                    websiteInvoice.Paid = invoice.Paid.ToString();
                    websiteInvoice.Comment = invoice.Comment;
                    websiteInvoice.Discount = invoice.Discount.ToString();
                    websiteInvoice.TotalUsd = invoice.TotalUsd.ToString();
                    websiteInvoice.ExchangeRate = invoice.ExchangeRate.ToString();
                    websiteInvoice.Currency = invoice.Currency;
                    websiteInvoice.Files = JsonSerializer.Serialize(invoice.Files.Select(p => p.File.FileName));
                    _invoiceTable.Insert(websiteInvoice);
                }
            }
            Console.WriteLine("Done!");

        }

        public void GetWebsiteInvoices()
        {
            Console.WriteLine("Start syncing orders");
            var orders = _websiteDbContext.Orders.Include(p => p.Products).ThenInclude(w => w.Product);
            foreach (var order in orders)
            {
                var customer = _context.Customer.FirstOrDefault(p => p.Email == order.Email);
                var invoice = _context.Invoice.FirstOrDefault(p => p.OrderId == order.OrderId);
                if (customer == null)
                {
                    customer = new Customer();
                    customer.Email = order.Email;
                    customer.Phone = order.ContactNumber;
                    customer.CustomerName = order.FirstName + order.LastName;
                }
                if (invoice == null)
                {
                    invoice = new Invoice();
                    invoice.OrderId = order.OrderId;
                    invoice.InvoiceDate = order.CreatedDate;
                    invoice.ExchangeRate = order.ExchangeRate;
                    invoice.Currency = "USD";
                    invoice.Customer = customer;
                    _context.Invoice.Add(invoice);
                    _context.SaveChanges();

                    var invoicePallets = new List<InvoicePallet>();
                    foreach (var orderProduct in order.Products)
                    {
                        var invoicePallet = new InvoicePallet();
                        var palletid = orderProduct.Product.Key.Split("-")[1];
                        invoicePallet.PalletId = int.Parse(palletid);
                        invoicePallet.InvoiceId = invoice.InvoiceId;
                        invoicePallet.Weight = orderProduct.Quantity;
                        var price = orderProduct.Product.Price * orderProduct.Quantity;
                        invoice.TotalUsd += price;
                        _context.InvoicePallet.Add(invoicePallet);
                    }
                    _context.SaveChanges();

                }
                else
                {
                    foreach (var orderProduct in order.Products)
                    {
                        var palletid = int.Parse(orderProduct.Product.Key.Split("-")[1]);
                        var invoicePallet = _context.InvoicePallet.FirstOrDefault(p => p.InvoiceId == invoice.InvoiceId && p.PalletId == palletid);
                        if (invoicePallet == null)
                        {
                            var newInvoicePallet = new InvoicePallet();
                            newInvoicePallet.PalletId = palletid;
                            newInvoicePallet.InvoiceId = invoice.InvoiceId;
                            newInvoicePallet.Weight = orderProduct.Quantity;
                            _context.InvoicePallet.Add(newInvoicePallet);
                            _context.SaveChanges();
                        }
                    }
                }

            }
            Console.WriteLine("Done!");

        }
    }
}