using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

namespace ASP_201MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public RatesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public object Get([FromQuery] String data) // GET
        {
            return new { result = $"Запит оброблено методом  GET {data}" };
        }
        [HttpDelete]
        public object Delete([FromBody] BodyData data)
        {
            int statusCode;
            String result;

            if (data == null || data.Data == null || data.ItemId == null || data.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Не всі дані передані: Data={data.Data} ItemId={data.ItemId} UserId={data.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(data.ItemId);
                    Guid userId = Guid.Parse(data.UserId);
                    int rating = Convert.ToInt32(data.Data);
                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (rate is not null)
                    {
                        _dataContext.Rates.Remove(rate);
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status200OK;
                        result = $"Всі дані видалено: Data ={data.Data}ItemId ={data.ItemId} UserId ={data.UserId}";
                    }
                    else
                    {
                        statusCode = StatusCodes.Status406NotAcceptable;
                        result = $"Всі дані відсутні: Data ={data.Data}ItemId ={data.ItemId} UserId ={data.UserId}";
                       
                    }
                }
                catch (Exception ex)
                {

                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Не всі дані передані({ex.Message}): Data={data.Data} ItemId={data.ItemId} UserId={data.UserId}";
                }
            }
            HttpContext.Response.StatusCode = statusCode;
            return new { result };
        }
        [HttpPost]
        public object Post([FromBody] BodyData data)
        {
            int statusCode;
            String result;

            if (data == null || data.Data == null || data.ItemId == null || data.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Не всі дані передані: Data={data.Data} ItemId={data.ItemId} UserId={data.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(data.ItemId);
                    Guid userId = Guid.Parse(data.UserId);
                    int rating = Convert.ToInt32(data.Data);
                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);

                    if (rate is not null)
                    {
                        if(rate.Rating == rating)
                        {
                            statusCode = StatusCodes.Status406NotAcceptable;
                            result = $"Всі дані передані: Data ={data.Data}ItemId ={data.ItemId} UserId ={data.UserId}";
                        }
                        else
                        {
                            rate.Rating = rating;
                            _dataContext.SaveChanges();
                            statusCode = StatusCodes.Status202Accepted;
                            result = $"Всі дані оновлено: Data ={data.Data}ItemId ={data.ItemId} UserId ={data.UserId}";
                        }
                    }
                    else
                    {
                        _dataContext.Rates.Add(new()
                        {
                            ItemId = itemId,
                            UserId = userId,
                            Rating = rating
                        });
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status201Created;
                        result = $"Всі дані внесено: Data ={data.Data}ItemId ={data.ItemId} UserId ={data.UserId}";
                    }
                }
                catch(Exception ex)
                {
                    
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Не всі дані передані({ex.Message}): Data={data.Data} ItemId={data.ItemId} UserId={data.UserId}";
                }
            }
            HttpContext.Response.StatusCode = statusCode;
            return new { result };
        }
        public object Default()
        {
            switch(HttpContext.Request.Method)
            {
                case "LINK": return Link();
                default: throw new NotImplementedException();
            }
            return new { result = $"Запит оброблено методом {HttpContext.Request.Method} і прийняті дані -- " };
        }
        private object Link()
        {
            return new
            {
                result = $"Запит оброблено методом LINK і прийнято дані --"
            };
        }
    }
    public class BodyData
    {
        public String? Data { get; set; } = null!;
        public String? ItemId { get; set; } = null!;
        public String? UserId { get; set; } = null!;
    }
}
