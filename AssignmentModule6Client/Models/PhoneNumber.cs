namespace AssignmentModule6Client.Models
{
    /// <summary>
    /// Represents a phone number belonging to a customer.
    /// </summary>
    public class PhoneNumber
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string? Type { get; set; }
        public string? Number { get; set; }
    }
}
