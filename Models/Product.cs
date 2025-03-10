using System;
using System.Collections.Generic;

namespace Higertech.Models;

public partial class Product
{
    public Guid Id { get; set; }
    public string NamaProduk { get; set; } = null!;
    public string? GambarUrl { get; set; }
    public string? Kategori { get; set; }
    public string? Deskripsi { get; set; }
    public string? Fitur { get; set; }
    public string? Spesifikasi { get; set; }
    public string? Aplikasi { get; set; }
    public string? Tkdn { get; set; }
    public string? Bmp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ImageProduct
{
    public int id { get; set; }
    public Guid id_product { get; set; }
    public string gambar_url { get; set; }
}
