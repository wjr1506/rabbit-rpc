using rabbitmq_rpc_client.Domain;

namespace rabbitmq_rpc_client
{
    public sealed class Order
    {
        public long Id { get; set; }
        public string? ToName { get; set; }
        public decimal Amount { get; set; }
        public string Status => OrderStatus.ToString();
        private OrderStatus OrderStatus { get; set; }

        public Order(decimal amount, string toName)
        {
            Id = DateTime.Now.Ticks;
            ToName = toName;
            OrderStatus = OrderStatus.Processing;
            Amount = amount;
        }
    }
}
