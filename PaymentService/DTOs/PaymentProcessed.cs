namespace Sales.PaymentService.DTOs
{
    public class PaymentProcessed
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
