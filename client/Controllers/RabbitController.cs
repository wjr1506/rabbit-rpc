using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using rabbitmq_rpc_client;
using rabbitmq_rpc_client.Model;

namespace client.Controllers;

[ApiController]
[Route("controller")]
public class RabbitController : ControllerBase
{
    [HttpPost("QueueRPC")]
    public async Task<IActionResult> Post([FromBody] Product product)
    {

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var replayQueue = $"{nameof(Order)}_return";
            var correlationId = Guid.NewGuid().ToString();

            channel.QueueDeclare(queue: replayQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: nameof(Order), durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            var tcs = new TaskCompletionSource<string>();

            consumer.Received += (model, ea) =>
                {
                    if (correlationId == ea.BasicProperties.CorrelationId)
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        tcs.SetResult(message);
                    }
                    else
                    {
                        tcs.SetResult(null); //se o id de correlação não corresponder será null
                    }
                };

            channel.BasicConsume(queue: replayQueue, autoAck: true, consumer: consumer);

            var pros = channel.CreateBasicProperties();

            pros.CorrelationId = correlationId;
            pros.ReplyTo = replayQueue;

            var amount = Convert.ToDecimal(product.Id);
            var order = new Order(amount: amount, toName: product.ToName);
            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: nameof(Order), basicProperties: pros, body: body);


            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30)); // Aguarda 30sec pelo processamento do servidor
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            if (completedTask == tcs.Task)
            {
                var result = tcs.Task.Result;
                Console.WriteLine(result + "\n");
                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Tempo limite atingido ao aguardar a resposta do RabbitMQ.");
            }
        }
    }
}
