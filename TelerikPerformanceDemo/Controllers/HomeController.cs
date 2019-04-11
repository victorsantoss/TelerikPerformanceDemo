using System.Diagnostics;
using System.Linq;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelerikPerformanceDemo.Data;
using TelerikPerformanceDemo.Models;
using TelerikPerformanceDemo.Utils.Extensions;

namespace TelerikPerformanceDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetData([DataSourceRequest] DataSourceRequest request, bool custom = false)
        {            
            using (var db = new AdventureWorks2017Context())
            {
                IQueryable<OrderDetailViewModel> orders =
                    db.SalesOrderDetail
                        .Include(o => o.SalesOrder)
                        .Include(o => o.SpecialOfferProduct)
                            .ThenInclude(p => p.Product)
                    .Select(o => new OrderDetailViewModel
                    {
                        OrderDetailID = o.SalesOrderDetailId,
                        OrderTrackingNumber = o.CarrierTrackingNumber,
                        OrderDate = o.SalesOrder.OrderDate,
                        AccountNumber = o.SalesOrder.AccountNumber,
                        LineTotal = o.LineTotal,
                        OrderQty = o.OrderQty,
                        UnitPrice = o.UnitPrice,
                        ProductName = o.SpecialOfferProduct.Product.Name,
                        SpecialOfferID = o.SpecialOfferId
                    });


                var watch = Stopwatch.StartNew();
               
                var result = custom ? orders.ToCustomDataSourceResul(request) : orders.ToDataSourceResult(request);


                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                Debug.WriteLine("============================================================");
                Debug.WriteLine(" > HomeController: " + elapsedMs + " ms");
                Debug.WriteLine("============================================================");

                var t = result.Data;

                return Json(result);
            }            
        }
    }
}