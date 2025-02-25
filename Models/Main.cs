using System;
using System.Collections.Generic;

using System;
using System.ComponentModel.DataAnnotations;

namespace Higertech.Models
{
    public class Main
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Img_Url { get; set; } = string.Empty;
        public string Category { get; set; }
        public string Hide { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class MainViewModel
    {
        public List<Main> Mains { get; set; } = new List<Main>();
        public List<Article> Articles { get; set; } = new();
        public List<Main> Posters { get; set; } = new();
        public List<Main> Tombol { get; set; } = new();
        public List<Main> Layanan { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Main> Klien { get; set; } = new();
        public List<ActivityModel> Activity { get; set; } = new();
    }

    public class Maps
    {
        public string? deviceId { get; set; }
        public string? slug { get; set; }
        public string stationType { get; set;}
        public string? type {get; set;}
        public DateTime? lastReadingAt { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? installedDate { get; set; }
        public string? deviceStatus { get; set; }
        public float? waterLevel {get; set;}
        public string? warningStatus { get; set; }

    // Nested objects for different station readings
        public ArrLastReading? arrLastReading { get; set; }    // For "arr" station type
        public AwlrLastReading? awlrLastReading { get; set; }  // For "awlr" station type
        public AwsLastReading? awsLastReading { get; set; }    // For "aws" station type
        public AwlrArrLastReading? awlrArrLastReading { get; set; }  // For "awlrarr" station type

        public string? unitDisplay {get; set;}
        public string? unitSensor {get; set;}
        public string? intensityLastHour {get; set;}
        public string? subDomainOld {get; set;}
        public float? rainfallLastHour {get; set;}
        public float? rainfall {get; set;}

        [Required]
        public string balaiName { get; set; }
        public string? organizationCode { get; set; }
        public string subDomain { get; set;}
        public string name { get; set;}

        // Adjust the properties below based on the sorting logic used
        public int nomor { get; set; }
        public int jumlahPos { get; set; }
        public int jumlahPosOnline { get; set; }
        public int jumlahPosOffline { get; set; }
    }

    public class ArrLastReading
    {
        public string deviceId { get; set; }
        public string deviceStatus { get; set; }
        public DateTime? readingAt { get; set; }
    }
    // Model for AwlrLastReading (similar to ArrLastReading)
    public class AwlrLastReading
    {
        public string deviceId { get; set; }
        public string deviceStatus { get; set; }
        public DateTime? readingAt { get; set; }
    }

    // Model for AwsLastReading (similar to ArrLastReading)
    public class AwsLastReading
    {
        public string deviceId { get; set; }
        public string deviceStatus { get; set; }
        public DateTime? readingAt { get; set; }
    }

    // Model for AwlrArrLastReading (combination of Awlr and Arr)
    public class AwlrArrLastReading
    {
        public string deviceId { get; set; }
        public string deviceStatus { get; set; }
        public DateTime? readingAt { get; set; }
    }
}

