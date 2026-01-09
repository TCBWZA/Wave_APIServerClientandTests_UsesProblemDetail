namespace AssignmentModule6Client.Models
{
    /// <summary>
    /// Represents a customer with associated invoices and phone numbers.
    /// </summary>
    public class Customer
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public List<Invoice>? Invoices { get; set; } = new List<Invoice>();
        public decimal Balance { get; set; }
        public List<PhoneNumber>? PhoneNumbers { get; set; } = new List<PhoneNumber>();
    }
}
