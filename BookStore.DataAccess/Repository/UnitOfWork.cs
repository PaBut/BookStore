using BookStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        DbContextApp _context;

        public UnitOfWork(DbContextApp context)
        {
            _context = context;
            CategoryRepo = new CategoryRepository(_context);
            CoverTypeRepo = new CoverTypeRepository(_context);
            ProductRepo = new ProductRepository(_context);
            CompanyRepo = new CompanyRepository(_context);
            ShoppingCartRepo = new ShoppingCartRepository(_context);
            ApplicationUserRepo = new ApplicationUserRepository(_context);
            OrderDetailsRepo = new OrderDetailsRepository(_context);
            OrderHeaderRepo = new OrderHeaderRepository(_context);
        }

        public ICategoryRepository CategoryRepo { get; private set; }

        public ICoverTypeRepository CoverTypeRepo { get; private set; }

        public IProductRepository ProductRepo { get; private set; }

        public ICompanyRepository CompanyRepo { get; private set; }
        public IShoppingCartRepository ShoppingCartRepo { get; private set; }
        public IApplicationUserRepository ApplicationUserRepo { get; private set; }
        public IOrderDetailsRepository OrderDetailsRepo { get; private set; }
        public IOrderHeaderRepository OrderHeaderRepo { get; private set; }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
