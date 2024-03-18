using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
namespace RabbitMqProductAPI.RabbitMQ
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        public void SendProductMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "product-create-example",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "product-create-example",
                                         basicProperties: properties,
                                         body: body);

                    // ACK alınması beklenmese de, gönderimde bir hata olduğunda bu try-catch bloğu kullanılır
                    Console.WriteLine("Mesaj başarıyla RabbitMQ kuyruğuna gönderildi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }
    }
}