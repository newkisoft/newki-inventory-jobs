using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace newki_inventory_jobs.Services
{
    public interface IInventoryService 
    {
        void Run(int threahHold);
    }
    public class InventoryService:IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Run(int threshold)
        {
            _context.AlertDataView.RemoveRange(_context.AlertDataView);
            _context.SaveChanges();
            var alertList = new List<string>();
            Console.WriteLine("Starting the process ...");      
            var tekstilkentPallets = _context.Pallet
                        .Where(p=>!p.Sold && p.Warehouse == "Tekstilkent" && p.YarnType == "FDY")
                        .GroupBy(p=>p.ColorCode)
                        .Select(p=>new{p.Key, Value = p.Sum(s=>s.RemainWeight)})
                        .OrderByDescending(p=>p.Value);

            var warehousePallets = _context.Pallet
                        .Where(p=>!p.Sold && p.Warehouse != "Tekstilkent" && p.YarnType == "FDY")
                        .GroupBy(p=>p.ColorCode)
                        .Select(p=>new{p.Key, Value = p.Sum(s=>s.RemainWeight)})
                        .OrderBy(p=>p.Value);

            var allSoldColors = warehousePallets.Where(p=>!tekstilkentPallets.Any(w=>w.Key == p.Key)).Select(p=>p.Key);

            var lowInTekstilkent =  tekstilkentPallets.Where(p=>p.Value < threshold).Select(p=>p.Key);
            
            
            alertList.AddRange(allSoldColors);
            alertList.AddRange(lowInTekstilkent);
            foreach(var alert in alertList){
                var newAlert = new AlertDataView();
                newAlert.Id = int.Parse(alert);
                newAlert.AlertType = AlertTypes.Tekstilkent.ToString();
                _context.AlertDataView.Add(newAlert);
                _context.SaveChanges();
            }            
            Console.WriteLine("Done!");
        }
    }    
}