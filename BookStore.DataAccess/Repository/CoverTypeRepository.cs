using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        public CoverTypeRepository(DbContextApp context) : base(context)
        {

        }

        public void Update(CoverType obj)
        {
            base._context.CoverTypes.Update(obj);
        }
    }
}
