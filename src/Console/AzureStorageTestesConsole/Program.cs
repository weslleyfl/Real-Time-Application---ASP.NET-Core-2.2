using System;
using System.Threading.Tasks;
using AzureStorageTestesConsole.Entity;
// Azure Storage Namespaces
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;

namespace AzureStorageTestesConsole
{
    public static class Program
    {
        
        static void Main(string[] args)
        {
            
            Task.Run(async () =>
            {
                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    Book book = new Book() { Author = "Rami", BookName = "ASP.NET Core With Azure", Publisher = "APress" };
                    book.BookId = 1;
                    book.RowKey = book.BookId.ToString();
                    book.PartitionKey = book.Publisher;

                    var data = await bookRepository.AddAsync(book);
                    Console.WriteLine(data);

                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);

                    data.Author = "Rami Vemula";
                    var updatedData = await bookRepository.UpdateAsync(data);
                    Console.WriteLine(updatedData);

                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);

                    await bookRepository.DeleteAsync(data);
                    Console.WriteLine("Deleted");

                    // Throw an exception to test rollback actions
                    //  throw new Exception();

                    _unitOfWork.CommitTransaction();
                }

            }).GetAwaiter().GetResult();



        }

        private static async Task UpdateEntity()
        {
            using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
            {
                var bookRepository = _unitOfWork.Repository<Book>();
                await bookRepository.CreateTableAsync();
                var data = await bookRepository.FindAsync("APress", "1");
                Console.WriteLine(data);
                data.Author = "Rami Vemula";
                var updatedData = await bookRepository.UpdateAsync(data);
                Console.WriteLine(updatedData);
                _unitOfWork.CommitTransaction();
            }
        }

        private async static void CreateNewEntity()
        {

            using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
            {
                var bookRepository = _unitOfWork.Repository<Book>();
                // await bookRepository.CreateTableAsync();

                Book book = new Book
                {
                    Author = "Jose",
                    BookName = "ASP.NET Azure",
                    Publisher = "APress Jose"
                };

                book.BookId = 3;
                book.RowKey = book.BookId.ToString();
                book.PartitionKey = book.Publisher;

                var data = await bookRepository.AddAsync(book);

                Console.WriteLine(data);

                _unitOfWork.CommitTransaction();
            }

        }

        private static void CreateBookInstanceTest()
        {
            // Azure Storage Account and Table Service Instances
            CloudStorageAccount storageAccount;
            CloudTableClient tableClient;

            // Connnect to Storage Account
            storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            // Create the Table 'Book', if it not exists
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Book");
            table.CreateIfNotExistsAsync();
            // Create a Book instance
            Book book = new Book()
            {
                Author = "Rami",
                BookName = "ASP.NET Core With Azure",
                Publisher = "APress"
            };
            book.BookId = 1;
            book.RowKey = book.BookId.ToString();
            book.PartitionKey = book.Publisher;
            book.CreatedDate = DateTime.UtcNow;
            book.UpdatedDate = DateTime.UtcNow;
            // Insert and execute operations
            TableOperation insertOperation = TableOperation.Insert(book);
            table.ExecuteAsync(insertOperation);
            // Console.ReadLine();
        }
        
        public static async Task<CloudTable> CreateTableAsync(string tableName)
        {
            string storageConnectionString = "UseDevelopmentStorage=true;"; // AppSettings.LoadAppSettings().StorageConnectionString;

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }

        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }     
    }

    public class TableClientConfiguration : TableEntity
    {
        public TableClientConfiguration()
        {
        }

        public TableClientConfiguration(string lastName, string firstName)
        {
            PartitionKey = lastName;
            RowKey = firstName;
        }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    //public class AppSettings
    //{
    //    public string StorageConnectionString { get; set; }
    //    public static AppSettings LoadAppSettings()
    //    {
    //        IConfigurationRoot configRoot = new ConfigurationBuilder()
    //            .AddJsonFile("Settings.json")
    //            .Build();
    //        AppSettings appSettings = configRoot.Get<AppSettings>();
    //        return appSettings;
    //    }
    //}
}
