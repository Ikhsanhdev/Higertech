using System;
using System.Collections.Generic;

namespace Higertech.ViewModels;

public partial class ProductVM
{
    public Guid id { get; set; }
    public string? nama_produk { get; set; }
    public string? gambar_url { get; set; }
    public string? kategori { get; set; }
    public string? deskripsi { get; set; }
    public string? fitur { get; set; }
    public string? spesifikasi { get; set; }
    public string? aplikasi { get; set; }
    public string? tkdn { get; set; }
    public string? bmp { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public List<IFormFile>? files { get; set; } // Untuk upload multiple files
    public List<string> removed_images { get; set; } = new List<string>(); // Track images to remove
    
    // List dari image_products yang dimiliki
    public List<ImageProductVM>? product_images { get; set; }
    
    // Helper method untuk mendapatkan URLs dari gambar_url (kompatibilitas)
    public List<string> GetImageUrls()
    {
        if (product_images != null && product_images.Count > 0)
            return product_images.Select(img => img.gambar_url).ToList();
            
        if (string.IsNullOrEmpty(gambar_url))
            return new List<string>();
            
        return gambar_url.Split(',').ToList();
    }
    
    // Helper property to get the first image URL for thumbnails
    public string GetMainImageUrl()
    {
        var urls = GetImageUrls();
        return urls.Count > 0 ? urls[0] : "";
    }
}

public class ImageProductVM
{
    public int id { get; set; }
    public Guid id_product { get; set; }
    public string gambar_url { get; set; }
}