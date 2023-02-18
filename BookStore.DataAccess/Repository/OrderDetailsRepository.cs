using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IRepository.IOrderDetailsRepository
    {
        
        public OrderDetailsRepository(DbContextApp context) : base(context)
        {
        }

        public void Update(OrderDetails obj)
        {
            base._context.OrderDetails.Update(obj);
        }
    }
}
