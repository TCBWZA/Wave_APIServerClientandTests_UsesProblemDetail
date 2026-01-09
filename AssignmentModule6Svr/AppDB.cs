using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssignmentModule6Svr.Classes;
using AssignmentModule6Svr;

namespace AssignmentModule6Svr;

/// <summary>
/// In-memory database for managing customers, invoices, and phone numbers.
/// Provides CRUD operations with auto-generated IDs and maintains referential integrity.
/// Thread-safe operations using Interlocked for ID generation.
/// </summary>
public class AppDB
{
    // In-memory collections for storing application data
    private static List<Customer> _customers = new List<Customer>();
    private static List<Invoice> _invoices = new List<Invoice>();
    private static List<PhoneNumber> _phones = new List<PhoneNumber>();

    // Thread synchronization primitives (currently not actively used but available for future enhancements)
    private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    // Auto-incrementing ID counters for each entity type (thread-safe via Interlocked)
    private static long _nextCustomerId = 1;
    private static long _nextInvoiceId = 1;
    private static long _nextPhoneNumberId = 1;

    /// <summary>
    /// Finds a customer by their unique ID.
    /// </summary>
    /// <param name="Id">The customer ID to search for</param>
    /// <returns>The Customer object if found, null otherwise</returns>
    public static Customer? FindCustomer(long Id)
    {
        if (Id > 0)
        {
            return _customers.Find(i => i.Id == Id);
        }

        return null;
    }

    /// <summary>
    /// Finds a customer by their name (case-insensitive).
    /// </summary>
    /// <param name="name">The customer name to search for</param>
    /// <returns>The Customer object if found, null otherwise</returns>
    public static Customer? FindCustomerByName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            return _customers.FirstOrDefault(c => 
                !string.IsNullOrWhiteSpace(c.Name) && 
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        return null;
    }

    /// <summary>
    /// Finds a customer by their email (case-insensitive).
    /// </summary>
    /// <param name="email">The customer email to search for</param>
    /// <returns>The Customer object if found, null otherwise</returns>
    public static Customer? FindCustomerByEmail(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            return _customers.FirstOrDefault(c => 
                !string.IsNullOrWhiteSpace(c.Email) && 
                c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        return null;
    }

    /// <summary>
    /// Checks if a customer with the given name or email already exists.
    /// </summary>
    /// <param name="name">The customer name to check</param>
    /// <param name="email">The customer email to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check (for updates)</param>
    /// <returns>True if a duplicate exists, false otherwise</returns>
    public static bool IsDuplicateCustomer(string? name, string? email, long excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return _customers.Any(c => 
            c.Id != excludeId && 
            ((!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(c.Name) && 
              c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
             (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(c.Email) && 
              c.Email.Equals(email, StringComparison.OrdinalIgnoreCase))));
    }

    /// <summary>
    /// Initializes the customer database with fake data using the Bogus library.
    /// </summary>
    /// <param name="bogus">If true, generates fake data; if false, does nothing</param>
    /// <param name="NoCust">The number of customers to generate</param>
    /// <returns>True if customers were created, false otherwise</returns>
    public static bool CreateCustomers(bool bogus, int NoCust)
    {
        if (bogus)
        {
            _customers = Bogus.GenerateCustomers(NoCust);
            // Update the next customer ID based on generated customers
            if (_customers.Count > 0)
            {
                _nextCustomerId = _customers.Max(c => c.Id) + 1;
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Generates fake invoices for an existing list of customers using the Bogus library.
    /// Each customer receives 1-5 random invoices with globally unique IDs.
    /// </summary>
    /// <param name="customers">The list of customers to generate invoices for</param>
    /// <returns>False (always returns false, likely intended for future use)</returns>
    public static bool CreateInvoices(List<Customer> customers)
    {
        if (customers != null)
        {
            if (customers.Count > 0)
            {
                foreach (Customer cust in customers)
                {
                    // Create a random number of Invoices for the given customer ID
                    List<Invoice> invoices = Bogus.GenerateInvoices(cust.Id, Random.Shared.Next(1, 5));
                    foreach (Invoice inv in invoices)
                    {
                        _invoices.Add(inv);
                    }
                }
                
                // Update the next invoice ID based on generated invoices
                if (_invoices.Count > 0)
                {
                    _nextInvoiceId = _invoices.Max(i => i.Id) + 1;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Generates fake phone numbers for an existing list of customers using the Bogus library.
    /// Each customer receives 1-3 random phone numbers with globally unique IDs.
    /// </summary>
    /// <param name="customers">The list of customers to generate phone numbers for</param>
    /// <returns>False (always returns false, likely intended for future use)</returns>
    public static bool CreatePhoneNumbers(List<Customer> customers)
    {
        if (customers != null)
        {
            if (customers.Count > 0)
            {
                foreach (Customer cust in customers)
                {
                    // Create a random number of phone numbers for the given customer ID
                    List<PhoneNumber> Phones = Bogus.GeneratePhoneNumbers(cust.Id, Random.Shared.Next(1, 3));
                    foreach (PhoneNumber ph in Phones)
                    {
                        _phones.Add(ph);
                    }
                }
                
                // Update the next phone number ID based on generated phone numbers
                if (_phones.Count > 0)
                {
                    _nextPhoneNumberId = _phones.Max(p => p.Id) + 1;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Adds a customer with a manually specified ID. 
    /// The customer ID must be unique and greater than 0.
    /// For auto-generated IDs, use AddCustomerWithAutoId instead.
    /// </summary>
    /// <param name="customer">Customer to add (must have a valid, unique ID)</param>
    /// <returns>True if successfully added, false if ID is invalid or already exists</returns>
    public static bool AddCustomer(Customer customer)
    {
        if (customer == null || customer.Id <= 0)
        {
            return false;
        }

        // Check if customer ID already exists
        if (FindCustomer(customer.Id) != null)
        {
            return false;
        }

        _customers.Add(customer);

        // Add the customer's invoices to the _invoices collection
        if (customer.Invoices != null && customer.Invoices.Count > 0)
        {
            foreach (var invoice in customer.Invoices)
            {
                // Ensure the invoice has the correct CustomerId (override any mismatched value)
                invoice.CustomerId = customer.Id;
                _invoices.Add(invoice);
            }
        }

        // Add the customer's phone numbers to the _phones collection
        if (customer.PhoneNumbers != null && customer.PhoneNumbers.Count > 0)
        {
            foreach (var phoneNumber in customer.PhoneNumbers)
            {
                // Ensure the phone number has the correct CustomerId (override any mismatched value)
                phoneNumber.CustomerId = customer.Id;
                _phones.Add(phoneNumber);
            }
        }

        return true;
    }

    /// <summary>
    /// Adds a customer with an auto-generated ID.
    /// This is the recommended method for adding new customers.
    /// Related invoices and phone numbers will also receive auto-generated IDs if not set.
    /// </summary>
    /// <param name="customer">Customer to add (ID will be auto-generated)</param>
    /// <returns>The customer with assigned ID, or null if customer is invalid</returns>
    public static Customer? AddCustomerWithAutoId(Customer customer)
    {
        if (customer == null)
        {
            return null;
        }

        // Auto-generate customer ID (this overwrites any provided ID)
        customer.Id = Interlocked.Increment(ref _nextCustomerId);
        
        _customers.Add(customer);

        // Add the customer's invoices to the _invoices collection with auto IDs
        if (customer.Invoices != null && customer.Invoices.Count > 0)
        {
            foreach (var invoice in customer.Invoices)
            {
                // Auto-generate invoice ID if not set
                if (invoice.Id == 0)
                {
                    invoice.Id = Interlocked.Increment(ref _nextInvoiceId);
                }
                // Ensure the invoice has the correct CustomerId (override any provided value)
                invoice.CustomerId = customer.Id;
                _invoices.Add(invoice);
            }
        }

        // Add the customer's phone numbers to the _phones collection with auto IDs
        if (customer.PhoneNumbers != null && customer.PhoneNumbers.Count > 0)
        {
            foreach (var phoneNumber in customer.PhoneNumbers)
            {
                // Auto-generate phone number ID if not set
                if (phoneNumber.Id == 0)
                {
                    phoneNumber.Id = Interlocked.Increment(ref _nextPhoneNumberId);
                }
                // Ensure the phone number has the correct CustomerId (override any provided value)
                phoneNumber.CustomerId = customer.Id;
                _phones.Add(phoneNumber);
            }
        }

        return customer;
    }

    /// <summary>
    /// Removes a customer and all associated invoices and phone numbers (cascade delete).
    /// </summary>
    /// <param name="id">The ID of the customer to remove</param>
    /// <returns>True if customer was found and removed, false otherwise</returns>
    public static bool RemoveCustomer(long id)
    {
        if (id > 0)
        {
            Customer? customer = FindCustomer(id);

            if (customer != null)
            {
                _customers.Remove(customer);

                // Also remove any Invoices belonging to the deleted customer
                _invoices.RemoveAll(i => i.CustomerId == id);
                _phones.RemoveAll(p => p.CustomerId == id);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Retrieves all customers with their associated invoices and phone numbers populated.
    /// Returns copies of customer objects to prevent unintended modifications.
    /// </summary>
    /// <returns>A list of Customer objects with fully populated relationships</returns>
    public static List<Customer> GetCustomerList()
    {
        // Create a new list with fresh customer data to avoid modifying original objects
        var customerList = new List<Customer>();
        
        foreach (var customer in _customers)
        {
            // Create a copy with populated related data
            var customerCopy = new Customer
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Invoices = GetInvoicesByCustomer(customer.Id),
                PhoneNumbers = GetPhoneNumbersByCustomer(customer.Id)
            };
            customerList.Add(customerCopy);
        }

        return customerList;
    }
    
    /// <summary>
    /// Finds an invoice by its invoice number (e.g., "INV-12345678").
    /// </summary>
    /// <param name="invoiceNumber">The invoice number to search for</param>
    /// <returns>The Invoice object if found, null otherwise</returns>
    public static Invoice? FindInvoice(string invoiceNumber)
    {
        if (!String.IsNullOrWhiteSpace(invoiceNumber) && _invoices != null && _invoices.Count > 0)
        {
            Invoice? invoice = _invoices.Where(i => i.InvoiceNumber == invoiceNumber).FirstOrDefault();

            if (invoice != null)
            {
                return (invoice);
            }
        }

        return null;
    }
    
    /// <summary>
    /// Finds an invoice by its unique ID.
    /// </summary>
    /// <param name="invoiceId">The invoice ID to search for</param>
    /// <returns>The Invoice object if found, null otherwise</returns>
    public static Invoice? FindInvoice(long invoiceId)
    {
        if (invoiceId>0 && _invoices != null && _invoices.Count > 0)
        {
            Invoice? invoice = _invoices.Where(i => i.Id == invoiceId).FirstOrDefault();

            if (invoice != null)
            {
                return (invoice);
            }
        }

        return null;
    }

    /// <summary>
    /// Adds an invoice with a manually specified ID.
    /// The invoice ID must be unique and the customer must exist.
    /// For auto-generated IDs, use AddInvoiceWithAutoId instead.
    /// </summary>
    /// <param name="invoice">Invoice to add (must have valid ID, CustomerId, and InvoiceNumber)</param>
    /// <returns>True if successfully added, false if validation fails</returns>
    public static bool AddInvoice(Invoice invoice)
    {
        if (invoice == null || invoice.CustomerId <= 0 || string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            return false;
        }

        _invoices.Add(invoice);
        return true;
    }

    /// <summary>
    /// Adds an invoice with an auto-generated ID.
    /// Invoice number must be provided by the caller and will be saved as received.
    /// This is the recommended method for adding new invoices.
    /// </summary>
    /// <param name="invoice">Invoice to add (ID will be auto-generated, InvoiceNumber must be provided)</param>
    /// <returns>The invoice with assigned ID, or null if invoice is invalid</returns>
    public static Invoice? AddInvoiceWithAutoId(Invoice invoice)
    {
        if (invoice == null || invoice.CustomerId <= 0 || string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            return null;
        }

        // Auto-generate invoice ID (this overwrites any provided ID)
        invoice.Id = Interlocked.Increment(ref _nextInvoiceId);
        
        // Invoice number is saved as received (no auto-generation)
        // Caller is responsible for providing a valid invoice number
        
        _invoices.Add(invoice);
        return invoice;
    }

    /// <summary>
    /// Updates an existing invoice's properties. 
    /// ID and CustomerId cannot be changed to maintain data integrity.
    /// </summary>
    /// <param name="id">The ID of the invoice to update</param>
    /// <param name="updatedInvoice">Invoice object with new values</param>
    /// <returns>True if update successful, false if invoice not found or invalid parameters</returns>
    public static bool UpdateInvoice(long id, Invoice updatedInvoice)
    {
        if (id <= 0 || updatedInvoice == null)
        {
            return false;
        }

        var existingInvoice = FindInvoice(id);
        if (existingInvoice == null)
        {
            return false;
        }

        // IMPORTANT: Only update mutable properties. 
        // Id and CustomerId are NEVER updated to maintain referential integrity.
        // Even if updatedInvoice contains different Id/CustomerId values, they are ignored.
        existingInvoice.InvoiceNumber = updatedInvoice.InvoiceNumber;
        existingInvoice.InvoiceDate = updatedInvoice.InvoiceDate;
        existingInvoice.Amount = updatedInvoice.Amount;

        return true;
    }

    /// <summary>
    /// Removes all invoices associated with a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID whose invoices should be removed</param>
    /// <returns>True if the operation completed, false if customer ID is invalid</returns>
    public static bool RemoveInvoicesbyCustomer(long customerId)
    {
        if (customerId > 0)
        {

            _invoices.RemoveAll(i => i.CustomerId == customerId);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes all phone numbers associated with a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID whose phone numbers should be removed</param>
    /// <returns>True if the operation completed, false if customer ID is invalid</returns>
    public static bool RemovePhonesbyCustomer(long customerId)
    {
        if (customerId > 0)
        {

            _phones.RemoveAll(i => i.CustomerId == customerId);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes an invoice by its invoice number.
    /// </summary>
    /// <param name="invoiceNumber">The invoice number to remove</param>
    /// <returns>True if invoice was found and removed, false otherwise</returns>
    public static bool RemoveInvoice(string invoiceNumber)
    {
        if (!String.IsNullOrWhiteSpace(invoiceNumber))
        {
            Invoice? invoice = FindInvoice(invoiceNumber);
            if (invoice != null)
            {
                _invoices.Remove(invoice);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes an invoice by its unique ID.
    /// </summary>
    /// <param name="invoiceId">The invoice ID to remove</param>
    /// <returns>True if invoice was found and removed, false otherwise</returns>
    public static bool RemoveInvoice(long invoiceId)
    {
        if (invoiceId > 0)
        {
            Invoice? invoice = FindInvoice(invoiceId);
            if (invoice != null)
            {
                _invoices.Remove(invoice);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Retrieves all invoices for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID to get invoices for</param>
    /// <returns>A list of Invoice objects, or an empty list if customer ID is invalid or has no invoices</returns>
    public static List<Invoice> GetInvoicesByCustomer(long customerId)
    {
        if (customerId > 0)
        {
            return _invoices.Where(i => i.CustomerId == customerId).ToList();
        }
        return new List<Invoice>();
    }

    /// <summary>
    /// Retrieves all invoices in the database.
    /// </summary>
    /// <returns>A list of all Invoice objects</returns>
    public static List<Invoice> GetInvoiceList()
    {
        return _invoices;
    }
    
    /// <summary>
    /// Clears all data from the in-memory database (customers, invoices, and phone numbers).
    /// Primarily used for testing purposes.
    /// </summary>
    public static void ClearDatabase()
    {
        _customers.Clear();
        _invoices.Clear();
        _phones.Clear();
    }
    
    /// <summary>
    /// Gets the total number of customers in the database.
    /// </summary>
    /// <returns>The count of customers</returns>
    public static int GetCustomerCount()
    {
        return _customers.Count;
    }
    
    /// <summary>
    /// Gets the total number of invoices in the database.
    /// </summary>
    /// <returns>The count of invoices</returns>
    public static int GetInvoiceCount()
    {
        return _invoices.Count;
    }
    
    /// <summary>
    /// Gets the total number of phone numbers in the database.
    /// </summary>
    /// <returns>The count of phone numbers</returns>
    public static int GetPhoneNumberCount()
    {
        return _phones.Count;
    }

    /// <summary>
    /// Serializes a list of customers to JSON format with camelCase properties and indentation.
    /// Returns an empty JSON array "[]" when the input is null or empty.
    /// </summary>
    /// <param name="customers">The list of customers to serialize</param>
    /// <returns>A JSON string representation of the customers</returns>
    public static string SerializeCustomersToJson(List<Customer>? customers)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (customers == null || customers.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(customers, options);
    }
    
    /// <summary>
    /// Serializes a list of invoices to JSON format with camelCase properties and indentation.
    /// Returns an empty JSON array "[]" when the input is null or empty.
    /// </summary>
    /// <param name="invoices">The list of invoices to serialize</param>
    /// <returns>A JSON string representation of the invoices</returns>
    public static string SerializeInvoicesToJson(List<Invoice>? invoices)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (invoices == null || invoices.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(invoices, options);
    }
    
    /// <summary>
    /// Serializes a single customer to JSON format with camelCase properties and indentation.
    /// Returns an empty JSON array "[]" when the input is null.
    /// </summary>
    /// <param name="customer">The customer to serialize</param>
    /// <returns>A JSON string representation of the customer</returns>
    public static string SerializeCustomerToJson(Customer? customer)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (customer == null)
            return "[]";

        return JsonSerializer.Serialize(customer, options);
    }
    
    /// <summary>
    /// Serializes a single invoice to JSON format with camelCase properties and indentation.
    /// Returns an empty JSON array "[]" when the input is null.
    /// </summary>
    /// <param name="invoice">The invoice to serialize</param>
    /// <returns>A JSON string representation of the invoice</returns>
    public static string SerializeInvoiceToJson(Invoice? invoice)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (invoice == null)
            return "[]";

        return JsonSerializer.Serialize(invoice, options);
    }
    
    /// <summary>
    /// Retrieves all phone numbers for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID to get phone numbers for</param>
    /// <returns>A list of PhoneNumber objects, or an empty list if customer ID is invalid or has no phone numbers</returns>
    public static List<PhoneNumber> GetPhoneNumbersByCustomer(long customerId)
    {
        if (customerId > 0)
        {
            return _phones.Where(p => p.CustomerId == customerId).ToList();
        }
        return new List<PhoneNumber>();
    }
    
    /// <summary>
    /// Retrieves all phone numbers in the database.
    /// </summary>
    /// <returns>A list of all PhoneNumber objects</returns>
    public static List<PhoneNumber> GetPhoneNumbersList()
    {
        return _phones;
    }

    /// <summary>
    /// Finds a phone number by its unique ID.
    /// </summary>
    /// <param name="id">The phone number ID to search for</param>
    /// <returns>The PhoneNumber object if found, null otherwise</returns>
    public static PhoneNumber? FindPhoneNumber(long id)
    {
        if (id > 0 && _phones != null && _phones.Count > 0)
        {
            PhoneNumber? phoneNumber = _phones.Where(p => p.Id == id).FirstOrDefault();

            if (phoneNumber != null)
            {
                return phoneNumber;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds a phone number with a manually specified ID.
    /// The phone number ID must be unique and the customer must exist.
    /// For auto-generated IDs, use AddPhoneNumberWithAutoId instead.
    /// </summary>
    /// <param name="phoneNumber">Phone number to add (must have valid ID and CustomerId)</param>
    /// <returns>True if successfully added, false if validation fails</returns>
    public static bool AddPhoneNumber(PhoneNumber phoneNumber)
    {
        if (phoneNumber == null || phoneNumber.CustomerId <= 0)
        {
            return false;
        }

        _phones.Add(phoneNumber);
        return true;
    }

    /// <summary>
    /// Adds a phone number with an auto-generated ID.
    /// This is the recommended method for adding new phone numbers.
    /// </summary>
    /// <param name="phoneNumber">Phone number to add (ID will be auto-generated)</param>
    /// <returns>The phone number with assigned ID, or null if phone number is invalid</returns>
    public static PhoneNumber? AddPhoneNumberWithAutoId(PhoneNumber phoneNumber)
    {
        if (phoneNumber == null || phoneNumber.CustomerId <= 0)
        {
            return null;
        }

        // Auto-generate phone number ID (this overwrites any provided ID)
        phoneNumber.Id = Interlocked.Increment(ref _nextPhoneNumberId);
        _phones.Add(phoneNumber);
        return phoneNumber;
    }

    /// <summary>
    /// Updates an existing phone number's properties.
    /// ID and CustomerId cannot be changed to maintain data integrity.
    /// </summary>
    /// <param name="id">The ID of the phone number to update</param>
    /// <param name="updatedPhoneNumber">PhoneNumber object with new values</param>
    /// <returns>True if update successful, false if phone number not found or invalid parameters</returns>
    public static bool UpdatePhoneNumber(long id, PhoneNumber updatedPhoneNumber)
    {
        if (id <= 0 || updatedPhoneNumber == null)
        {
            return false;
        }

        var existingPhoneNumber = FindPhoneNumber(id);
        if (existingPhoneNumber == null)
        {
            return false;
        }

        // IMPORTANT: Only update mutable properties.
        // Id and CustomerId are NEVER updated to maintain referential integrity.
        // Even if updatedPhoneNumber contains different Id/CustomerId values, they are ignored.
        existingPhoneNumber.Type = updatedPhoneNumber.Type;
        existingPhoneNumber.Number = updatedPhoneNumber.Number;

        return true;
    }

    /// <summary>
    /// Removes a phone number by its unique ID.
    /// </summary>
    /// <param name="id">The phone number ID to remove</param>
    /// <returns>True if phone number was found and removed, false otherwise</returns>
    public static bool RemovePhoneNumber(long id)
    {
        if (id > 0)
        {
            PhoneNumber? phoneNumber = FindPhoneNumber(id);
            if (phoneNumber != null)
            {
                _phones.Remove(phoneNumber);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Updates an existing customer's basic properties.
    /// ID, Invoices, and PhoneNumbers cannot be changed through this method.
    /// Use specific invoice/phone number methods to manage those relationships.
    /// </summary>
    /// <param name="id">The ID of the customer to update</param>
    /// <param name="updatedCustomer">Customer object with new values</param>
    /// <returns>True if update successful, false if customer not found or invalid parameters</returns>
    public static bool UpdateCustomer(long id, Customer updatedCustomer)
    {
        if (id <= 0 || updatedCustomer == null)
        {
            return false;
        }

        var existingCustomer = FindCustomer(id);
        if (existingCustomer == null)
        {
            return false;
        }

        // IMPORTANT: Only update basic mutable properties (Name and Email).
        // Id is NEVER updated to maintain data integrity.
        // Invoices and PhoneNumbers are NOT updated through this method to prevent
        // accidental relationship corruption. Use AddInvoice/AddPhoneNumber methods instead.
        // Even if updatedCustomer contains different Id/Invoices/PhoneNumbers, they are ignored.
        existingCustomer.Name = updatedCustomer.Name;
        existingCustomer.Email = updatedCustomer.Email;

        return true;
    }
}
