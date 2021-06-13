using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using newkilibraries;

namespace newki_inventory_jobs.Model
{
    public interface IInvoiceTable
    {
        void Insert(WebsiteInvoice Invoice);
        Task<List<WebsiteInvoice>> GetItems(string gmail);
        Task<WebsiteInvoice> GetInvoice(string code);
        void CreateTable();
        void Delete(WebsiteInvoice Invoice);
        void DeleteTable();
    }

    public class InvoiceTable : IInvoiceTable
    {
        IDynamoDbContext _context;
        Table _priceTable;
        public InvoiceTable(IDynamoDbContext context)
        {
            _context = context;
            CreateTable();
            _priceTable = Table.LoadTable(_context.GetClient(), "Invoice");
        }

        public void CreateTable()
        {
            var client = _context.GetClient();
            List<string> currentTables = client.ListTablesAsync().Result.TableNames;
            if (!currentTables.Contains("Invoice"))
            {
                var request = new CreateTableRequest
                {
                    TableName = "Invoice",
                    AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Email",
                        AttributeType = "S"
                    }
                },
                    KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "Email",
                        KeyType = "RANGE"
                    },
                },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 5,
                        WriteCapacityUnits = 5
                    }
                };
                try
                {
                    client.CreateTableAsync(request).Wait();
                }
                catch (ArgumentException ex1)
                {
                    // Log and re-throw
                }
                catch (AggregateException ex2)
                {
                    // Log and swallow
                }

            }
        }
        public void Insert(WebsiteInvoice Invoice)
        {
            var document = new Document();

            foreach (var prop in GetProperties())
            {
                var value = Invoice.GetType()
                                      .GetProperty(prop.Name).GetValue(Invoice);

                document[prop.Name] = value != null ? value.ToString() : string.Empty;
            }

            _priceTable.PutItemAsync(document);
        }

        public async Task<List<WebsiteInvoice>> GetItems(string gmail)
        {
            var Invoices = new List<WebsiteInvoice>();
            var filter = new ScanFilter();
            filter.AddCondition("InvoiceCode", ScanOperator.IsNotNull);
            var search = _priceTable.Scan(filter);
            do
            {
                var documents = await search.GetNextSetAsync();
                var cnt = 0;
                foreach (var document in documents)
                {
                    var Invoice = new WebsiteInvoice();
                    if (document.Keys.Contains("Id"))
                        Invoice.Id = document["Id"];
                    if (document.Keys.Contains("Email"))
                        Invoice.Email = document["Email"];
                    if (document.Keys.Contains("InvoiceDate"))
                        Invoice.InvoiceDate = document["InvoiceDate"];
                    if (document.Keys.Contains("Currency"))
                        Invoice.Currency = document["Currency"];
                    if (document.Keys.Contains("TotalUsd"))
                        Invoice.TotalUsd = document["TotalUsd"];
                    if (document.Keys.Contains("ExchangeRate"))
                        Invoice.ExchangeRate = document["ExchangeRate"];
                    if (document.Keys.Contains("Files"))
                        Invoice.Files = document["Files"];
                    Invoices.Add(Invoice);
                    cnt++;
                }


            } while (!search.IsDone);
            return Invoices;
        }

        public void Delete(WebsiteInvoice Invoice)
        {
            var document = new Document();
            document["Id"] = Invoice.Id;
            document["Email"] = Invoice.Email;
            document["InvoiceDate"] = Invoice.InvoiceDate;
            document["Currency"] = Invoice.Currency;
            document["TotalUsd"] = Invoice.TotalUsd;
            document["ExchangeRate"] = Invoice.ExchangeRate;
            _priceTable.DeleteItemAsync(document);

        }
        public void DeleteTable()
        {
            var request = new DeleteTableRequest
            {
                TableName = _priceTable.TableName
            };

            var result = _context.GetClient().DeleteTableAsync(request).GetAwaiter().GetResult();
        }

        private PropertyInfo[] GetProperties()
        {
            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(WebsiteInvoice).GetProperties();
            return propertyInfos;
        }

        public async Task<WebsiteInvoice> GetInvoice(string code)
        {
            var filter = new ScanFilter();
            filter.AddCondition("InvoiceCode", ScanOperator.Equal, code);
            var search = _priceTable.Scan(filter);
            var documents = await search.GetNextSetAsync();
            var Invoice = new WebsiteInvoice();
            foreach (var document in documents)
            {
                if (document.Keys.Contains("Id"))
                    Invoice.Id = document["Id"];
                if (document.Keys.Contains("Email"))
                    Invoice.Email = document["Email"];
                if (document.Keys.Contains("InvoiceDate"))
                    Invoice.InvoiceDate = document["InvoiceDate"];
                if (document.Keys.Contains("Currency"))
                    Invoice.Currency = document["Currency"];
                if (document.Keys.Contains("TotalUsd"))
                    Invoice.TotalUsd = document["TotalUsd"];
                if (document.Keys.Contains("ExchangeRate"))
                    Invoice.ExchangeRate = document["ExchangeRate"];
                if (document.Keys.Contains("Files"))
                    Invoice.Files = document["Files"];
                break;
            }
            return Invoice;
        }
    }
}