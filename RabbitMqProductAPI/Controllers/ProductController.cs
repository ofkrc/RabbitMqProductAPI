using Microsoft.AspNetCore.Mvc;
using RabbitMqProductAPI.Models;
using RabbitMqProductAPI.RabbitMQ;
using RabbitMqProductAPI.Services;
namespace RabbitMqProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly IRabbitMQProducer _rabitMQProducer;
        public ProductController(IProductService _productService, IRabbitMQProducer rabitMQProducer)
        {
            productService = _productService;
            _rabitMQProducer = rabitMQProducer;
        }
        [HttpGet("productlist")]
        public IEnumerable<Product> ProductList()
        {
            var productList = productService.GetProductList();
            return productList;
        }
        [HttpGet("getproductbyid")]
        public Product GetProductById(int Id)
        {
            return productService.GetProductById(Id);
        }
        [HttpPost("addproduct")]
        public IActionResult AddProduct(Product product)
        {
            try
            {
                // Product nesnesini RabbitMQ kuyruğuna gönder
                _rabitMQProducer.SendProductMessage(product);

                // Başarı durumunu döndür
                return Ok("Ürün başarıyla RabbitMQ kuyruğuna gönderildi.");
            }
            catch (Exception ex)
            {
                // Hata durumunu döndür
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }
        [HttpPut("updateproduct")]
        public Product UpdateProduct(Product product)
        {
            return productService.UpdateProduct(product);
        }
        [HttpDelete("deleteproduct")]
        public bool DeleteProduct(int Id)
        {
            return productService.DeleteProduct(Id);
        }
    }
}