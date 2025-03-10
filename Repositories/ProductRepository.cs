using Higertech.Models;
using Higertech.ViewModels;
using Dapper;
using Npgsql;
using Serilog;
using Higertech.Models.Datatables;

namespace Higertech.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<(IReadOnlyList<dynamic>, int)> GetDataProduct(JqueryDataTableRequest request);
    Task<ProductVM?> GetProductByIdAsync(Guid id);
    Task<AjaxResponse> SaveAsync(ProductVM product);
    Task<Product?> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(Guid id);
    Task<bool> IsProductNameExistsAsync(string productName, Guid? excludeId = null);
    Task<List<ImageProductVM>> GetProductImagesAsync(Guid productId);
    Task<bool> SaveProductImageAsync(Guid productId, string imageUrl);
    Task<bool> UpdateMainProductImageAsync(Guid productId, string imageUrl);
    Task<bool> DeleteProductImageAsync(int imageId);
    Task<bool> DeleteProductImageByUrlAsync(Guid productId, string imageUrl);
}

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;
    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<List<Product>> GetAllAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var query = @$"
                    SELECT
                        p.id AS ""Id"",
                        p.nama_produk AS ""NamaProduk"",
                        p.kategori AS ""Kategori"",
                        p.deskripsi AS ""Deskripsi"",
                        p.fitur AS ""Fitur"",
                        p.spesifikasi AS ""Spesifikasi"",
                        p.aplikasi AS ""Aplikasi"",
                        p.tkdn AS ""Tkdn"",
                        p.bmp AS ""Bmp"",
                        p.created_at AS ""CreatedAt"",
                        p.updated_at AS ""UpdatedAt"",
                        (SELECT img.gambar_url FROM image_products img WHERE img.id_product = p.id LIMIT 1) AS ""GambarUrl""
                    FROM products p
                    WHERE p.deleted_at IS NULL
                    ORDER BY p.created_at DESC;";

            var result = await connection.QueryAsync<Product>(query);
            return result.ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace });
            throw;
        }
    }

    public async Task<(IReadOnlyList<dynamic>, int)> GetDataProduct(JqueryDataTableRequest request)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            List<dynamic> result = new List<dynamic>();

            var query = @"
                SELECT 
                    p.*,
                    (SELECT img.gambar_url FROM image_products img 
                     WHERE img.id_product = p.id ORDER BY img.id LIMIT 1) AS main_image_url 
                FROM products p ";

            var parameters = new DynamicParameters();
            var whereConditions = new List<string>();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                if (request.SearchValue.Contains('\''))
                {
                    request.SearchValue = request.SearchValue.Replace("'", "''");
                }

                whereConditions.Add(@"
                    (LOWER(p.nama_produk) LIKE @SearchValue OR
                    LOWER(p.kategori) LIKE @SearchValue OR
                    LOWER(p.deskripsi) LIKE @SearchValue)");
                parameters.Add("@SearchValue", "%" + request.SearchValue.ToLower() + "%");
            }
            
            whereConditions.Add(@" p.deleted_at IS NULL");

            var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            query += whereClause;

            int total = 0;
            var sql_count = $"SELECT COUNT(*) FROM ({query}) as total";
            total = connection.ExecuteScalar<int>(sql_count, parameters);

            query += @" 
            ORDER BY p.created_at DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;";
            parameters.Add("@Skip", request.Skip);
            parameters.Add("@PageSize", request.PageSize);

            result = (await connection.QueryAsync<dynamic>(query, parameters)).ToList();

            return (result, total);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting product data: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<ProductVM?> GetProductByIdAsync(Guid id)
    {
        const string query = @"
            SELECT * FROM products WHERE id = @Id;";

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Get product data
                ProductVM? product = await connection.QuerySingleOrDefaultAsync<ProductVM>(query, new { Id = id });
                
                if (product != null)
                {
                    // Get product images
                    product.product_images = (await GetProductImagesAsync(id)).ToList();
                }
                
                return product;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching product: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<AjaxResponse> SaveAsync(ProductVM product)
    {
        AjaxResponse result = new();
        
        // Create transaction to ensure all operations succeed or fail together
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Check for duplicate name first
            var isDuplicate = await IsProductNameExistsAsync(product.nama_produk, product.id);
            if (isDuplicate)
            {
                result.Code = 400;
                result.Message = "Nama produk sudah digunakan. Silakan gunakan nama lain.";
                return result;
            }

            string status = "Tambah";
            Guid productId = product.id;

            if (product.id == Guid.Empty)
            {
                // Insert new product
                productId = Guid.NewGuid();
                product.id = productId;
                product.created_at = DateTime.Now;
                
                const string insertQuery = @"
                    INSERT INTO products 
                    (id, nama_produk, kategori, deskripsi, fitur, spesifikasi, aplikasi, tkdn, bmp, created_at)
                    VALUES 
                    (@id, UPPER(@nama_produk), @kategori, @deskripsi, @fitur, @spesifikasi, @aplikasi, @tkdn, @bmp, @created_at);";
                
                await connection.ExecuteAsync(insertQuery, product, transaction);
            }
            else
            {
                // Update existing product
                status = "Memperbarui";
                product.updated_at = DateTime.Now;
                
                const string updateQuery = @"
                    UPDATE products
                    SET nama_produk = UPPER(@nama_produk), 
                        kategori = @kategori, 
                        deskripsi = @deskripsi,
                        fitur = @fitur, 
                        spesifikasi = @spesifikasi, 
                        aplikasi = @aplikasi, 
                        tkdn = @tkdn,
                        bmp = @bmp, 
                        updated_at = @updated_at
                    WHERE id = @id;";
                
                await connection.ExecuteAsync(updateQuery, product, transaction);
            }
            
            // Process removed images if any
            if (product.removed_images != null && product.removed_images.Count > 0)
            {
                foreach (var imageUrl in product.removed_images)
                {
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        const string deleteImageQuery = @"
                            DELETE FROM image_products 
                            WHERE id_product = @productId AND gambar_url = @imageUrl;";
                        
                        await connection.ExecuteAsync(deleteImageQuery, 
                            new { productId, imageUrl }, 
                            transaction);
                    }
                }
            }
            
            // Commit the transaction
            transaction.Commit();
            
            result.Code = 200;
            result.Message = $"{status} data berhasil";
            result.Data = productId; // Return product ID for further processing if needed
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            result.Code = 500;
            result.Message = $"Terjadi Kesalahan.\nError: {ex.Message}";
            Log.Error(ex, "Error saving product: {Message}", ex.Message);
        }
        
        return result;
    }

    public async Task<Product?> UpdateProductAsync(Product product)
    {
        const string query = @"
            UPDATE products
                    SET nama_produk = @NamaProduk, 
                        gambar_url = @GambarUrl, 
                        kategori = @Kategori, 
                        deskripsi = @Deskripsi,
                        fitur = @Fitur, 
                        spesifikasi = @Spesifikasi, 
                        aplikasi = @Aplikasi, 
                        tkdn = @Tkdn,
                        bmp = @Bmp, 
                        updated_at = @UpdatedAt
                    WHERE id = @Id
            RETURNING *;";

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<Product>(query, product);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating product: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Soft delete the product
            const string productQuery = @"
                UPDATE products
                SET deleted_at = @Date_now
                WHERE id = @Id;";
            
            await connection.ExecuteAsync(productQuery, new { Id = id, Date_now = DateTime.Now }, transaction);
            
            // Don't delete the images - soft delete only the product
            // This helps maintain data integrity if needed for backup/recovery
            
            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Log.Error(ex, "Error deleting product: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<bool> IsProductNameExistsAsync(string productName, Guid? excludeId = null)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var query = @"
                SELECT COUNT(*)
                FROM products 
                WHERE LOWER(nama_produk) = LOWER(@productName) 
                AND deleted_at IS NULL";

            var parameters = new DynamicParameters();
            parameters.Add("@productName", productName);

            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
            {
                query += " AND id != @excludeId";
                parameters.Add("@excludeId", excludeId.Value);
            }

            var count = await connection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking product name existence: {Message}", ex.Message);
            throw;
        }
    }
    
    public async Task<List<ImageProductVM>> GetProductImagesAsync(Guid productId)
    {
        const string query = @"
            SELECT 
                id,
                id_product,
                gambar_url
            FROM image_products 
            WHERE id_product = @ProductId 
            ORDER BY id;";
            
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var images = await connection.QueryAsync<ImageProductVM>(query, new { ProductId = productId });
            return images.ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting product images: {Message}", ex.Message);
            return new List<ImageProductVM>();
        }
    }
    
    public async Task<bool> SaveProductImageAsync(Guid productId, string imageUrl)
    {
        // Check if image already exists to prevent duplicates
        const string checkQuery = @"
            SELECT COUNT(*) FROM image_products 
            WHERE id_product = @productId AND gambar_url = @imageUrl";
            
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Check if image already exists
                int imageCount = await connection.ExecuteScalarAsync<int>(checkQuery, new { productId, imageUrl });
                
                // Only insert if image doesn't exist
                if (imageCount == 0)
                {
                    const string insertQuery = @"
                        INSERT INTO image_products
                        (id_product, gambar_url)
                        VALUES
                        (@productId, @imageUrl);";
                        
                    await connection.ExecuteAsync(insertQuery, new { 
                        productId, 
                        imageUrl
                    }, transaction);
                    
                    // Check if this is the first image for the product
                    const string countQuery = @"
                        SELECT COUNT(*) FROM image_products 
                        WHERE id_product = @productId";
                    
                    int totalImages = await connection.ExecuteScalarAsync<int>(countQuery, new { productId }, transaction);
                    
                    // If this is the first image, update the gambar_url in the products table
                    if (totalImages == 1)
                    {
                        const string updateMainImageQuery = @"
                            UPDATE products 
                            SET gambar_url = @imageUrl
                            WHERE id = @productId";
                            
                        await connection.ExecuteAsync(updateMainImageQuery, new { productId, imageUrl }, transaction);
                    }
                    
                    transaction.Commit();
                    Log.Information("Gambar {ImageUrl} berhasil disimpan ke database untuk produk {ProductId}", imageUrl, productId);
                    return true;
                }
                else
                {
                    // Image already exists, but we don't consider it an error
                    transaction.Commit();
                    Log.Information("Gambar {ImageUrl} sudah ada untuk produk {ProductId}", imageUrl, productId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, "Error menyimpan gambar ke database: {Message}", ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error connecting to database: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<bool> UpdateMainProductImageAsync(Guid productId, string imageUrl)
    {
        const string query = @"
            UPDATE products
            SET gambar_url = @imageUrl
            WHERE id = @productId;";
            
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { productId, imageUrl });
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating main product image: {Message}", ex.Message);
            return false;
        }
    }
    
    public async Task<bool> DeleteProductImageAsync(int imageId)
    {
        const string query = @"
            DELETE FROM image_products
            WHERE id = @imageId;";
            
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { imageId });
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product image: {Message}", ex.Message);
            return false;
        }
    }
    
    public async Task<bool> DeleteProductImageByUrlAsync(Guid productId, string imageUrl)
    {
        const string query = @"
            DELETE FROM image_products
            WHERE id_product = @productId AND gambar_url = @imageUrl;";
            
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { productId, imageUrl });
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product image by URL: {Message}", ex.Message);
            return false;
        }
    }
}