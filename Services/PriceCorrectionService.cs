using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace newki_inventory_jobs.Services
{
    public interface IPriceCorrectionService
    {
        void Run();
    }
    public class PriceCorrectionService : IPriceCorrectionService
    {
        private readonly WebsiteDbContext _context;

        public PriceCorrectionService(WebsiteDbContext context)
        {
            _context = context;
        }

        public void Run()
        {
            Console.WriteLine("Starting the process ...");

            CleanDty();
            CleanBcf();
            CleanFdy();
            CleanShw();
            CleanSpandex();
            CleanTfo();

            Console.WriteLine("Done!");
        }

        private void CleanDty()
        {
            var pricesGroupedByDate = _context.DtyPriceArchive.ToList().GroupBy(p => p.RequestNumber).Where(g => g.Count() > 1);

            foreach (var pricesOnDate in pricesGroupedByDate)
            {
                var redundants = _context.DtyPriceArchive.Where(p => p.RequestNumber == pricesOnDate.Key).OrderByDescending(p => p.InsertDate).Skip(1);
                _context.DtyPriceArchive.RemoveRange(redundants);
                _context.SaveChanges();
            }
        }

        private void CleanFdy()
        {
            var pricesGroupedByDate = _context.FdyPriceArchive.ToList().GroupBy(p => p.RequestNumber).Where(g => g.Count() > 1);

            foreach (var pricesOnDate in pricesGroupedByDate)
            {
                var redundants = _context.FdyPriceArchive.ToList().Where(p => p.RequestNumber == pricesOnDate.Key).OrderByDescending(p => p.InsertDate).Skip(1);
                _context.FdyPriceArchive.RemoveRange(redundants);
                _context.SaveChanges();
            }
        }

        private void CleanBcf()
        {
            var pricesGroupedByDate = _context.BcfPriceArchive.ToList().GroupBy(p => p.RequestNumber).Where(g => g.Count() > 1);

            foreach (var pricesOnDate in pricesGroupedByDate)
            {
                var redundants = _context.BcfPriceArchive.Where(p => p.RequestNumber == pricesOnDate.Key).OrderByDescending(p => p.InsertDate).Skip(1);
                _context.BcfPriceArchive.RemoveRange(redundants);
                _context.SaveChanges();
            }
        }

        private void CleanTfo()
        {
            var pricesGroupedByDate = _context.TfoPriceArchive.ToList().GroupBy(p => p.RequestNumber).Where(g => g.Count() > 1);

            foreach (var pricesOnDate in pricesGroupedByDate)
            {
                var redundants = _context.TfoPriceArchive.Where(p => p.RequestNumber == pricesOnDate.Key).OrderByDescending(p => p.InsertDate).Skip(1);
                _context.TfoPriceArchive.RemoveRange(redundants);
                _context.SaveChanges();
            }
        }

        private void CleanShw()
        {
            var pricesGroupedByDate = _context.ShwPriceArchive.ToList().GroupBy(p => p.RequestNumber).Where(g => g.Count() > 1);

            foreach (var pricesOnDate in pricesGroupedByDate)
            {
                var redundants = _context.ShwPriceArchive.Where(p => p.RequestNumber == pricesOnDate.Key).OrderByDescending(p => p.InsertDate).Skip(10);
                _context.ShwPriceArchive.RemoveRange(redundants);
                _context.SaveChanges();
            }
        }

        private void CleanSpandex()
        {
            var pricesGroupedByDate = _context.SpandexPriceArchive.ToList().GroupBy(p => p.RequestNumber).Where(g => g.Count() > 1);

            foreach (var pricesOnDate in pricesGroupedByDate)
            {
                var redundants = _context.SpandexPriceArchive.Where(p => p.RequestNumber == pricesOnDate.Key).OrderByDescending(p => p.InsertDate).Skip(20);
                _context.SpandexPriceArchive.RemoveRange(redundants);
                _context.SaveChanges();
            }
        }
    }

}