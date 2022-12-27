using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShippingAuto.Model
{
    public sealed class OrderHistoryMap : ClassMap<OrderHistory>
    {
        public OrderHistoryMap()
        {
            Map(m => m.OrderNumber).Index(0);
            Map(m => m.BizNumber).Index(1);
            Map(m => m.OrderDate).Index(2);
            Map(m => m.OrderPlatform).Index(5);
            Map(m => m.Seller).Index(7);
            Map(m => m.ProductName).Index(8);
            Map(m => m.Amount).Index(9);
            Map(m => m.OrderStatus).Index(11);
            Map(m => m.PaymentStatus).Index(15);
        }
    }
    public class OrderHistory : INotifyPropertyChanged
    {
        public string OrderNumber { get; set; }
        public string BizNumber { get; set; }
 
        public string OrderDate { get; set; }

        public string OrderPlatform { get; set; }
  
        public string Seller { get; set; }
     
        public string ProductName { get; set; }

        public Decimal Amount { get; set; }
         
        public string OrderStatus { get; set; }
   
        public string PaymentStatus { get; set; }
        
        [Ignore]
        public string OrderShipmentExpressName
        {
            get
            {
                return expressName;
            }
            set
            {
                this.expressName = value;

                RaiseProperChanged(nameof(OrderShipmentExpressName));
            }
        }


        private string expressId { get; set; }
        private string expressStatus { get; set; }
        private string expressName { get; set; }

        [Ignore]
        public string OrderShipmentExpressId
        {
            get
            {
                return expressId;
            }
            set
            {
                this.expressId = value;

                RaiseProperChanged(nameof(OrderShipmentExpressId));
            }
        }

        [Ignore]        
        public string ShippingStaus
        {
            get
            {
                return expressStatus;
            }
            set
            {
                this.expressStatus = value;

                RaiseProperChanged(nameof(ShippingStaus));
            }
        }

        [Ignore]
        public string Remark { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaiseProperChanged([CallerMemberName] string caller = "")
        {

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

    }
}
