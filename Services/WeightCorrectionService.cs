using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace newki_inventory_jobs.Services
{
    public interface IWeightCorrectionService 
    {
        void Run();
    }
    public class WeightCorrectionService:IWeightCorrectionService
    {
        private readonly ApplicationDbContext _context;

        public WeightCorrectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Run()
        {
            Console.WriteLine("Starting the process ...");
            var errorList = new List<Pallet>();
            List<InvoiceBox> invoiceBoxes =  _context.InvoiceBox.Include(p => p.Box).ToList();
            List<InvoicePallet> invoicePallets = _context.InvoicePallet.Include(p => p.Pallet).ToList();
            var boxPallets = new Dictionary<int, double>();
            Console.WriteLine("Calculate all the boxes weight");
            foreach (var invoiceBox in invoiceBoxes)
            {
                if (!boxPallets.Keys.Contains(invoiceBox.Box.PalletId))
                {
                    boxPallets.Add(invoiceBox.Box.PalletId, 0);
                }
                boxPallets[invoiceBox.Box.PalletId] += invoiceBox.Weight;
            }
            Console.WriteLine("Compare the calculated weight with the pallet weight");
            foreach (var boxPallet in boxPallets)
            {              
                var pallet = _context.Pallet.FirstOrDefault(p => p.PalletId == boxPallet.Key);
                if (pallet.Weight == Math.Round(boxPallet.Value,3) && pallet.Sold == false)
                {
                    errorList.Add(pallet);
                    pallet.RemainWeight = 0;
                    pallet.Sold = true;
                    _context.SaveChanges();
                }
                else
                {
                    var remainingWeight = Math.Round(pallet.Weight - boxPallet.Value, 3);
                    if (remainingWeight != pallet.RemainWeight)
                    {
                        pallet.RemainWeight = remainingWeight;
                        _context.SaveChanges();
                        errorList.Add(pallet);
                    }
                }

            }
            Console.WriteLine("Calculate all the pallets weight");
            var palletTotalSold = new Dictionary<int, double>();
            foreach (var invoicePallet in invoicePallets)
            {
                if (!palletTotalSold.Keys.Contains(invoicePallet.PalletId))
                {
                    palletTotalSold.Add(invoicePallet.PalletId, 0);
                }
                palletTotalSold[invoicePallet.PalletId] += invoicePallet.Weight;
            }
            Console.WriteLine("Compare the sold weight with the calculated weight");
            foreach (var palletSoldWeight in palletTotalSold)
            {

                var boxPalletWeight = 0.0;
                if (boxPallets.ContainsKey(palletSoldWeight.Key))
                {
                    boxPalletWeight = boxPallets[palletSoldWeight.Key];
                }
                var pallet = _context.Pallet.FirstOrDefault(p => p.PalletId == palletSoldWeight.Key);
                if (pallet.Weight == Math.Round(palletSoldWeight.Value,3) && pallet.Sold == false)
                {
                    errorList.Add(pallet);
                    pallet.RemainWeight = 0;
                    pallet.Sold = true;
                    _context.SaveChanges();
                }
                else
                {
                    var remainingWeight = Math.Round(pallet.Weight - palletSoldWeight.Value - boxPalletWeight, 3);
                    if (remainingWeight != pallet.RemainWeight)
                    {
                        pallet.RemainWeight = remainingWeight;
                        _context.SaveChanges();
                        errorList.Add(pallet);
                    }
                }

            }
            Console.WriteLine("Done!");


        }


    }
    
}