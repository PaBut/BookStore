using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepo { get; }
        ICoverTypeRepository CoverTypeRepo { get; }
        IProductRepository ProductRepo { get; }
        ICompanyRepository CompanyRepo { get; }
        IShoppingCartRepository ShoppingCartRepo { get; }
        IApplicationUserRepository ApplicationUserRepo { get; }
        IOrderDetailsRepository OrderDetailsRepo { get; }
        IOrderHeaderRepository OrderHeaderRepo { get; } 

        void Save();
    }
}
