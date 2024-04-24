using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var prod = _db.Products.FirstOrDefault(u => u.Id == product.Id);
            if(prod != null)
            {
                prod.Title = product.Title;
                prod.ISBN = product.ISBN;
                prod.Price = product.Price;
                prod.Price50 = product.Price50;
                prod.Price100 = product.Price100;
                prod.Description = product.Description;
                prod.CategoryId = product.CategoryId;
                prod.Author = product.Author;
                if(product.ImageUrl  != null)
                {
                    prod.ImageUrl = product.ImageUrl;
                }
                
            }
        }
    }
}
