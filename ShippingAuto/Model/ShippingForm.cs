using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingAuto.Model
{
    public class ShippingForm
    {
        public string OrderShipmentExpressName { get; set; }
        public string OrderShipmentExpressId { get; set; }
        public string ProductName { get; set; }
        public Decimal Amount { get; set; }
        public string ShipmentStatus { get; set; }
        public string Remark { get; set; }



        public ShippingForm(OrderHistory orderHistory)
        {
            OrderShipmentExpressName = orderHistory.OrderShipmentExpressName;
            if (long.TryParse(orderHistory.OrderShipmentExpressId, out long res))
            {
                OrderShipmentExpressId = "'" + orderHistory.OrderShipmentExpressId;
            }
            else
            {
                OrderShipmentExpressId = orderHistory.OrderShipmentExpressId;
            }
            
            ProductName = orderHistory.ProductName;
            Amount = Math.Truncate((decimal)0.5 * orderHistory.Amount) ;
            ShipmentStatus  = orderHistory.ShippingStaus;
            Remark = orderHistory.Remark;
        }
    }
}
