using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqProductAPI.Data;
using RabbitMqProductAPI.Models;
using System;
using System.Text;

var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var factory = new ConnectionFactory
{
    HostName = "localhost"
};

var connection = factory.CreateConnection();

using (var channel = connection.CreateModel())
{
    channel.QueueDeclare(queue: "product-create-example",
                         durable: true,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, eventArgs) =>
    {
        var body = eventArgs.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($"product-create-example message received: {message}");

        // Gelen JSON mesajını Product nesnesine dönüştür
        var product = JsonConvert.DeserializeObject<Product>(message);

        // Veritabanına kayıt işlemi
        using (var dbContext = new DbContextClass(configBuilder)) // Veritabanı bağlantısı
        {
            dbContext.Products.Add(product); // Ürünü veritabanına ekle
            dbContext.SaveChanges(); // Değişiklikleri kaydet
        }

        Console.WriteLine("Product added to the database.");
        // Mesajı başarıyla işledikten sonra ACK gönder
        channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
    };

    // ACK mekanizmasını etkinleştirerek mesajları al
    channel.BasicConsume(queue: "product-create-example", autoAck: false, consumer: consumer);

    Console.WriteLine("Consumer started. Press any key to exit.");
    Console.ReadKey();
}
