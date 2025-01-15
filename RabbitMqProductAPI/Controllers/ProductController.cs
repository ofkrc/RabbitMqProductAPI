using Microsoft.AspNetCore.Mvc;
using RabbitMqProductAPI.Models;
using RabbitMqProductAPI.RabbitMQ;
using RabbitMqProductAPI.Services;
using System;

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
                _rabitMQProducer.SendProductMessage(product);

                return Ok("Ürün başarıyla RabbitMQ kuyruğuna gönderildi.");
            }
            catch (Exception ex)
            {
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
        [HttpPost("upload-image")]
        public IActionResult UploadImage([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Dosya yüklenmedi");

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    byte[] imageBytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    
                    return Ok(new { imageData = base64String });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata oluştu: {ex.Message}");
            }
        }
    }
}