using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public class OrderMoney
    {
        public decimal SubTotal { get; set; }     // Сумма всех позиций
        public decimal DeliveryFee { get; set; }  // Стоимость доставки
        public decimal Tax { get; set; }           // Налоги
        public decimal Discount { get; set; }      // Скидка
        public decimal TotalAmount { get; set; }   // Итого к оплате
    }
}
