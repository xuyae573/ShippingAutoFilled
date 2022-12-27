using System;
using System.Collections.Generic;
using System.Linq;

namespace ShippingAuto
{
	public class ExpressResult
	{
		public ExpressResult()
		{
			Address = new List<Address>();
		}

        public List<Address> Address { get; set; }

        public bool isSuccess { get; set; }
		public string? ExpressId { get; set; }
		public string? ExpressName { get; set; }


        public string GetShippingStatus()
        {
            if (Address.Any())
            {
                string? place = Address.First().Place;
                if (place==null)
                {
                    return "";
                }
                return place.Length < 20 ? place : place.Substring(0, 20);
            }

            return string.Empty;
        }

    }

	public class Address {
        public string? Place { get; set; }
        public string? Time { get; set; }
    }

}

