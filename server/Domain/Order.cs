namespace rabbitmq_rpc_server.Domain
{
    public sealed class Order
    {
        public long Id { get; set; }
        public string? ToName { get; set; }
        public decimal Amount { get; set; }
        public string Status => OrderStatus.ToString();
        private OrderStatus OrderStatus { get; set; }

        public void SetStatus(OrderStatus orderStatus)
        {
            OrderStatus = orderStatus;
        }
    }
}