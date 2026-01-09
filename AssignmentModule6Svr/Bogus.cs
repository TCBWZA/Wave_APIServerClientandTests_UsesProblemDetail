using AssignmentModule6Svr.Classes;
using Bogus;
using System.Threading;

namespace AssignmentModule6Svr
{
    /// <summary>
    /// Provides fake data generation using the Bogus library for testing and development.
    /// Generates realistic customer, invoice, and phone number data with globally unique IDs.
    /// </summary>
    public static class Bogus
    {
        // Global ID counters to ensure uniqueness across all customers
        private static long _invoiceIdCounter = 0;
        private static long _phoneNumberIdCounter = 0;

        /// <summary>
        /// Generates a list of fake customers with realistic company names and email addresses.
        /// Each customer is assigned a unique sequential ID starting from 1.
        /// </summary>
        /// <param name="count">The number of customers to generate</param>
        /// <returns>A list of Customer objects with generated data</returns>
        public static List<Customer> GenerateCustomers(int count)
        {
            long id = 0;

            Faker<Customer> faker = new Faker<Customer>("en_GB")
                .CustomInstantiator(f => new Customer
                {
                    Id = Interlocked.Increment(ref id),
                    Name = f.Company.CompanyName(),
                    Email = f.Internet.Email()
                });

            List<Customer> list = [];
            for (int i = 0; i < count; i++)
            {
                list.Add(faker.Generate());
            }

            return list;
        }

        /// <summary>
        /// Generates a list of fake invoices for a specific customer.
        /// Invoice IDs are globally unique across all customers using a shared counter.
        /// Generates invoices with random amounts between 10 and 5000, dated within the past 2 years.
        /// </summary>
        /// <param name="custId">The customer ID to associate with the generated invoices</param>
        /// <param name="count">The number of invoices to generate (passed by reference for efficiency)</param>
        /// <returns>A list of Invoice objects with generated data</returns>
        public static List<Invoice> GenerateInvoices(long custId, in int count)
        {
            Faker<Invoice> faker = new Faker<Invoice>("en_GB")
                .CustomInstantiator(f =>
                {
                    string invoiceNumber = "INV-" + f.Random.AlphaNumeric(8).ToUpper();
                    DateTime invoiceDate = f.Date.Past(2);
                    decimal amount = f.Finance.Amount(10, 5000);

                    Invoice? invoice = new()
                    {
                        Id = Interlocked.Increment(ref _invoiceIdCounter),
                        InvoiceNumber = invoiceNumber,
                        InvoiceDate = invoiceDate,
                        Amount = amount,
                        CustomerId = custId
                    };

                    return invoice;
                });

            List<Invoice> list = [];
            for (int i = 0; i < count; i++)
            {
                list.Add(faker.Generate());
            }

            return list;
        }

        // Valid phone number types for random selection
        private static readonly string[] NumberTypes = new[] { "DirectDial", "Work" };
        private static readonly List<string> MobilePrefixes = new() { "071", "072", "073", "074", "075", "076", "077", "078", "079" };
        private static readonly List<string> landLinePrefix = new()
        {
            "+44 20",   // London
            "+44 121",  // Birmingham
            "+44 131",  // Edinburgh
            "+44 141",  // Glasgow
            "+44 151",  // Liverpool
            "+44 161",  // Manchester
            "+44 191"   // Tyneside
        };


    /// <summary>
    /// Generates a list of fake phone numbers for a specific customer.
    /// Phone number IDs are globally unique across all customers using a shared counter.
    /// Generates UK-formatted phone numbers with random types (Mobile, DirectDial, or Work).
    /// </summary>
    /// <param name="custId">The customer ID to associate with the generated phone numbers</param>
    /// <param name="count">The number of phone numbers to generate</param>
    /// <returns>A list of PhoneNumber objects with generated data</returns>
    public static List<PhoneNumber> GeneratePhoneNumbers(long custId, int count)
        {
            Faker<PhoneNumber> fakerphone = new Faker<PhoneNumber>("en_GB")
                .CustomInstantiator(f => new PhoneNumber
                {
                    Id = Interlocked.Increment(ref _phoneNumberIdCounter),
                    CustomerId = custId,
                    Number = f.Phone.PhoneNumber(),
                    Type = f.PickRandom(NumberTypes)
                });
            List<PhoneNumber> listPhone = [];
            for (int i = 0; i < count; i++)
            {
                listPhone.Add(fakerphone.Generate());
            }
            return listPhone;
        }
    }
}
