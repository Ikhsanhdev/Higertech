using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Higertech.Models;
using Higertech.ViewModels;
using Higertech.Interfaces;
using Higertech.Models.Datatables;
using Serilog;
using Higertech.Repositories;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Higertech.Controllers;

public class MainController : Controller
{
    private readonly ILogger<MainController> _logger;
    private readonly IUnitOfWorkRepository _unitOfWorkRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IActivitiesRepository _activitiesRepository;
    private readonly IArticleRepository _articleRepository;

    public MainController(ILogger<MainController> logger, IUnitOfWorkRepository unitOfWorkRepository, IProjectRepository projectRepository,
    IActivitiesRepository activitiesRepository, IArticleRepository articleRepository)
    {
        this._unitOfWorkRepository = unitOfWorkRepository;
        this._projectRepository = projectRepository;
        this._activitiesRepository = activitiesRepository;
        this._articleRepository = articleRepository;
        this._logger = logger;
    }

    public class ApiResponse
    {
        public IEnumerable<Maps> Data { get; set; }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult PageNotFound()
    {
        return View("~/Views/404/PageNotFound.cshtml");
    }

    public IActionResult Article()
    {
        return View();
    }

    public IActionResult MapGoogle()
    {
        return View();
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var mains = await _unitOfWorkRepository.Main.GetAllAsync();
            var projects = await _projectRepository.GetListProjectAsync();
            var activity = await _activitiesRepository.GetListActivityAsync();
            var article = await _articleRepository.GetListArticleAsync();

            if (mains == null || !mains.Any())
            {
                _logger.LogWarning("No data found");
                return View(new MainViewModel());
            }

            var viewModel = new MainViewModel
            {
                Posters = mains.Where(m => m.Category == "poster").ToList(),
                Tombol = mains.Where(m => m.Category == "tombol").ToList(),
                Layanan = mains.Where(m => m.Category == "layanan").ToList(),
                // Projects = projects.OrderByDescending(p => p.UpdatedAt).Take(6).ToList(),
                Klien = mains.Where(m => m.Category == "klien").ToList(),
                Activity = activity.Select(m => new ActivityModel
                {
                    Title = m.Title,
                    Description = m.Description,
                    Image = m.Image,
                    ClientName = m.ClientName,
                    DateProject = m.DateProject,
                    DateActivity = m.DateActivity
                }).Take(4).OrderByDescending(m => m.DateProject).ToList(),
                Articles = article.Select(m => new Article
                {
                    Title = m.Title,
                    Description = m.Description,
                    Image = m.Image,
                    Category = m.Category,
                    Author = m.Author,
                    Slug = m.Slug,
                    CreatedAt = m.CreatedAt
                }).Take(6).OrderByDescending(m => m.CreatedAt).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading data: {Message}", ex.Message);
            return View(new MainViewModel());
        }
    }


    [Route("/main/indexsecond")]
    public async Task<IActionResult> IndexSecond()
    {
        try
        {
            var mains = await _unitOfWorkRepository.Main.GetAllAsync();
            var projects = await _projectRepository.GetListProjectAsync();
            var activity = await _activitiesRepository.GetListActivityAsync();
            var article = await _articleRepository.GetListArticleAsync();

            if (mains == null || !mains.Any())
            {
                _logger.LogWarning("No data found");
                return View("~/Views/Main/IndexSecond.cshtml", new MainViewModel());
            }

            var viewModel = new MainViewModel
            {
                Posters = mains.Where(m => m.Category == "poster").ToList(),
                Tombol = mains.Where(m => m.Category == "tombol").ToList(),
                Layanan = mains.Where(m => m.Category == "layanan").ToList(),
                // Projects = projects.OrderByDescending(p => p.UpdatedAt).Take(6).ToList(),
                Klien = mains.Where(m => m.Category == "klien").ToList(),
                Activity = activity.Select(m => new ActivityModel
                {
                    Title = m.Title,
                    Description = m.Description,
                    Image = m.Image,
                    ClientName = m.ClientName,
                    DateProject = m.DateProject,
                    DateActivity = m.DateActivity
                }).Take(4).OrderByDescending(m => m.DateProject).ToList(),
                Articles = article.Select(m => new Article
                {
                    Title = m.Title,
                    Description = m.Description,
                    Image = m.Image,
                    Category = m.Category,
                    Author = m.Author,
                    Slug = m.Slug,
                    CreatedAt = m.CreatedAt
                }).Take(6).OrderByDescending(m => m.CreatedAt).ToList()
            };

            return View("~/Views/Main/IndexSecond.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading data: {Message}", ex.Message);
            return View("~/Views/Main/IndexSecond.cshtml", new MainViewModel());
        }
    }


    private async Task<dynamic> GetDataApi(string endPoint)
    {

        string apiUrl = $"http://localhost:5000/{endPoint}";
        string username = "m0n1tor_st4tion";
        string password = "H1gertech.1dua3";

        using (HttpClient client = new HttpClient())
        {
            // Set up basic authentication credentials
            string authHeaderValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    // Deserialize the response data to a JSON string
                    string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                    return responseData; // Output the JSON string
                }
                else
                {
                    return response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return ex.Message;
            }
        }
    }

    public async Task<JsonResult> GetStationAll()
    {
        string endPoint = $"LastReading/all/";
        var data = await GetDataApi(endPoint);
        return Json(data);
    }

    private async Task<List<Maps>> GetDataFromApi()
    {
        string apiUrl = "http://localhost:5000/LastReading/all";
        string username = "m0n1tor_st4tion";
        string password = "H1gertech.1dua3";

        using (HttpClient client = new HttpClient())
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseData);
                return apiResponse?.Data?.ToList() ?? new List<Maps>();
            }
            else
            {
                // Handle errors appropriately
                return new List<Maps>();
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetTotalData(string totalType)
    {
        try
        {
            var apiResponse = await GetDataFromApi();

            if (apiResponse == null)
            {
                // Jika respons API null, kembalikan total 0
                return Json(new { Total = 0 });
            }

            int total = 0;
            var today = DateTime.Today; // Menyimpan tanggal hari ini

            switch (totalType.ToLower())
            {
                case "totalpos":
                    total = apiResponse.Count();
                    break;
                case "totalinstansi":
                    total = apiResponse.Select(a => a.subDomain).Distinct().Count();
                    break;
                case "totaldugaair":
                    total = apiResponse.Count(a => a.stationType == "AWLR" || a.stationType == "AWLR_ARR");
                    break;
                case "totalcurahhujan":
                    total = apiResponse.Count(a => a.stationType == "ARR" || a.stationType == "AWLR_ARR");
                    break;
                case "totalklimatologi":
                    total = apiResponse.Count(a => a.stationType == "AWS");
                    break;
                default:
                    // Jika totalType tidak valid, kembalikan total 0
                    total = 0;
                    break;
            }

            return Json(new { Total = total });
        }
        catch (Exception ex)
        {
            // Handle exception
            Console.WriteLine(ex.Message);
            return Json(new { Total = 0 });
        }
    }
}
