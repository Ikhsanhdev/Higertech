using System;
using System.Collections.Generic;

namespace Higertech.Models;

public partial class Tutorial
{
    public Guid Id { get; set; }
    public string? Title { get; set; } 
    public string? Description { get; set; } 

    public string? Category { get; set; }
    public string? Image { get; set; }
    public string? Youtube { get; set; } 
    public DateTime CreatedAt { get; set; }
}