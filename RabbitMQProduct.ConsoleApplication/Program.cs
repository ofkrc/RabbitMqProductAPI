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
    HostName = "localhost",
    UserName = "admin",     
    Password = "123456",    
    Port = 5672
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

        var product = JsonConvert.DeserializeObject<Product>(message);


        using (var dbContext = new DbContextClass(configBuilder)) 
        {
            dbContext.Products.Add(product);
            dbContext.SaveChanges(); 
        }

        Console.WriteLine("Product added to the database.");

        channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
    };


    channel.BasicConsume(queue: "product-create-example", autoAck: false, consumer: consumer);

    Console.WriteLine("Consumer started. Press any key to exit.");
    Console.ReadKey();
}
