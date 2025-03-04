using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Higertech.Models;
using Higertech.ViewModels;
using Higertech.Interfaces;
using Higertech.Models.Datatables;
using Serilog;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Higertech.Controllers;

// [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)
[Authorize]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly IUnitOfWorkRepository _unitOfWorkRepository;
    private readonly IUnitOfWorkService _unitOfWorkService;



    public AdminController(IUnitOfWorkRepository unitOfWorkRepository, IUnitOfWorkService unitOfWorkService)
    {
        this._unitOfWorkRepository = unitOfWorkRepository;
        this._unitOfWorkService = unitOfWorkService;
    }

    public IActionResult Index()
    {
        return View();
    }


    #region  <=================================== Article ========================================>

    public IActionResult Article()
    {
        return View("~/Views/Admin/Article/Index.cshtml");
    }

    [HttpGet("article/create")]
    public IActionResult CreateEditArticle(Guid id)
    {
        ArticleVM model = new ArticleVM();

        return View("~/Views/Admin/Article/CreateEdit.cshtml", model);
    }


    [HttpGet("article/edit/{id}")]
    public IActionResult EditArticle(Guid id)
    {

        var model = _unitOfWorkRepository.Article.GetArticleByIdAsync(id).Result;
        if (model == null)
        {
            return View("~/Views/404/PageNotFound.cshtml");
        }
        return View("~/Views/Admin/Article/CreateEdit.cshtml", model);
    }

    [HttpPost]
    [Route("/Admin/Article")]
    public async Task<IActionResult> SaveArticle(ArticleVM model, IFormFile file)
    {
        AjaxResponse response = new();
        if (file != null)
        {
            model.img_url = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "articles");
        }
        response = await _unitOfWorkRepository.Article.SaveAsync(model);
        return Json(response);
    }

    [HttpDelete]
    [Route("/article/delete/{id}")]
    public async Task<IActionResult> DeleteArticle(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Article.DeleteArticleAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    public async Task<IActionResult> GetArticleData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Article.GetDataArticle(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    }
    #endregion
    #region  <=================================== Project ========================================>
    public IActionResult Project()
    {
        return View("~/Views/Admin/Project/Index.cshtml");
    }

    [HttpGet("project/create")]
    public IActionResult CreateEditProject()
    {
        ProjectVM model = new ProjectVM();

        return View("~/Views/Admin/Project/CreateEdit.cshtml", model);
    }

    [HttpGet("project/edit/{id}")]
    public IActionResult EditProject(Guid id)
    {

        var model = _unitOfWorkRepository.Project.GetProjectByIdAsync(id).Result;

        if (model == null)
        {
            return View("~/Views/404/PageNotFound.cshtml");
        }
        return View("~/Views/Admin/Project/CreateEdit.cshtml", model);
    }

    [HttpPost]
    [Route("/Admin/Project")]
    public async Task<IActionResult> SaveProject(ProjectVM model, IFormFile file)
    {
        AjaxResponse response = new();

        if (file != null)
        {
            model.img_url = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "projects");
        }

        response = await _unitOfWorkRepository.Project.SaveAsync(model);
        return Json(response);
    }

    [HttpDelete]
    [Route("/project/delete/{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Project.DeleteAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    public async Task<IActionResult> GetProjectData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Project.GetDataProject(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    } 
    #endregion
    
#region  <=================================== Activity ========================================>
    public IActionResult Activities()
    {
        return View("~/Views/Admin/Activity/Index.cshtml");
    }

    [HttpGet("activity/create")]
    public IActionResult CreateEditActivities()
    {
        ActivityVM model = new ActivityVM();

        return View("~/Views/Admin/Activity/CreateEdit.cshtml", model);
    }

    [HttpGet("activity/edit/{id}")]
    public IActionResult EditActivity(Guid id)
    {

        var model = _unitOfWorkRepository.Activities.GetActivityByIdAsync(id).Result;

        if (model == null)
        {
            return View("~/Views/404/PageNotFound.cshtml");
        }
        return View("~/Views/Admin/Activity/CreateEdit.cshtml", model);
    }

    [HttpPost]
    [Route("/Admin/Activity")]
    public async Task<IActionResult> SaveActivity(ActivityVM model, IFormFile file)
    {
        AjaxResponse response = new();

        if (file != null)
        {
            model.img_url = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "activities");
        }

        response = await _unitOfWorkRepository.Activities.SaveAsync(model);
        return Json(response);
    }

    [HttpDelete]
    [Route("/activity/delete/{id}")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Activities.DeleteAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    public async Task<IActionResult> GetActiviyData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Activities.GetDataActivity(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    }
    #endregion

    #region  <=================================== Product ========================================>
    public IActionResult Product()
    {
        return View("~/Views/Admin/Product/Index.cshtml");
    }

    public async Task<IActionResult> GetProductData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Product.GetDataProduct(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    }

    [HttpGet("product/create")]
    public IActionResult CreateEditProduct()
    {
        ProductVM model = new ProductVM();
        return View("~/Views/Admin/Product/CreateEdit.cshtml", model);
    }

    [HttpPost]
    [Route("/Admin/Product")]
    public async Task<IActionResult> SaveProduct([FromForm] ProductVM model)
    {
        AjaxResponse response = new();
        
        try
        {
            Console.WriteLine("Menerima request penyimpanan produk...");
            
            // First save the product to get ID (for new products)
            response = await _unitOfWorkRepository.Product.SaveAsync(model);
            
            if (response.Code == 200)
            {
                Guid productId = model.id != Guid.Empty ? model.id : (Guid)response.Data;
                
                Console.WriteLine($"Produk berhasil disimpan dengan ID: {productId}");
                
                string firstImageUrl = null;

                // Pastikan file dikirim
                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                    foreach (var file in Request.Form.Files)
                    {
                        Console.WriteLine($"Mengupload file: {file.FileName}");
                        
                        // Upload each image
                        string imageUrl = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "products");
                        
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            Console.WriteLine("Gagal mengunggah gambar!");
                        }
                        else
                        {
                            Console.WriteLine($"Gambar diunggah ke: {imageUrl}");
                            
                            // Save the first image URL to update the main product later
                            if (firstImageUrl == null)
                            {
                                firstImageUrl = imageUrl;
                            }
                            
                            // Save to image_products table
                            bool isSaved = await _unitOfWorkRepository.Product.SaveProductImageAsync(productId, imageUrl);
                            if (!isSaved)
                            {
                                Console.WriteLine($"Gagal menyimpan gambar {imageUrl} ke database!");
                            }
                            else
                            {
                                Console.WriteLine($"Gambar {imageUrl} berhasil disimpan ke database!");
                            }
                        }
                    }
                }
                
                // If there are no existing images and we've uploaded at least one image, 
                // update the main product gambar_url field
                if (firstImageUrl != null)
                {
                    // Check if the product already has a main image
                    var product = await _unitOfWorkRepository.Product.GetProductByIdAsync(productId);
                    if (product != null && string.IsNullOrEmpty(product.gambar_url))
                    {
                        // Update the main product with the first image
                        var productToUpdate = new Product
                        {
                            Id = productId,
                            NamaProduk = product.nama_produk,
                            GambarUrl = firstImageUrl,
                            Kategori = product.kategori,
                            Deskripsi = product.deskripsi,
                            Fitur = product.fitur,
                            Spesifikasi = product.spesifikasi,
                            Aplikasi = product.aplikasi,
                            Tkdn = product.tkdn,
                            Bmp = product.bmp,
                            UpdatedAt = DateTime.Now
                        };
                        
                        await _unitOfWorkRepository.Product.UpdateProductAsync(productToUpdate);
                        Console.WriteLine($"Gambar utama produk diperbarui dengan: {firstImageUrl}");
                    }
                }
            }
            
            return Json(response);
        }
        catch (Exception ex)
        {
            response.Code = 500;
            response.Message = $"Terjadi kesalahan: {ex.Message}";
            Console.WriteLine($"Error saat menyimpan produk: {ex.Message}");
            return Json(response);
        }
    }

    [HttpPost]
    [Route("/Admin/Product/UploadProductImage")]
    public async Task<IActionResult> UploadProductImage(IFormFile file, Guid productId)
    {
        AjaxResponse response = new();
        
        try
        {
            if (file != null)
            {
                // Upload the image
                string imageUrl = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "products");
                
                // Save to image_products table
                bool success = await _unitOfWorkRepository.Product.SaveProductImageAsync(productId, imageUrl);
                
                if (success)
                {
                    response.Code = 200;
                    response.Message = "Image uploaded successfully";
                    response.Data = imageUrl;
                }
                else
                {
                    response.Code = 500;
                    response.Message = "Failed to save image record";
                }
            }
            else
            {
                response.Code = 400;
                response.Message = "No file uploaded";
            }
            
            return Json(response);
        }
        catch (Exception ex)
        {
            response.Code = 500;
            response.Message = $"Error uploading image: {ex.Message}";
            return Json(response);
        }
    }

    [HttpGet("product/edit/{id}")]
    public IActionResult EditProduct(Guid id)
    {
        ProductVM model = new ProductVM();

        model = _unitOfWorkRepository.Product.GetProductByIdAsync(id).Result;

        return View("~/Views/Admin/Product/CreateEdit.cshtml", model);
    }

    [HttpDelete]
    [Route("/product/delete/{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Product.DeleteProductAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    [HttpDelete]
    [Route("/Admin/Product/DeleteImage/{imageId}")]
    public async Task<IActionResult> DeleteProductImage(int imageId)
    {
        AjaxResponse response = new();
        
        try
        {
            bool success = await _unitOfWorkRepository.Product.DeleteProductImageAsync(imageId);
            
            if (success)
            {
                response.Code = 200;
                response.Message = "Image deleted successfully";
            }
            else
            {
                response.Code = 500;
                response.Message = "Failed to delete image";
            }
            
            return Json(response);
        }
        catch (Exception ex)
        {
            response.Code = 500;
            response.Message = $"Error deleting image: {ex.Message}";
            return Json(response);
        }
    }
    #endregion
    
    #region <=================================== Home ========================================>
    public IActionResult Main()
    {
        return View("~/Views/Admin/Main/Index.cshtml");
    }

    public async Task<IActionResult> GetMainData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Main.GetDataMain(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    }

    [HttpGet("main/create")]
    public IActionResult CreateEditMain(Guid id)
    {
        MainVM model = new MainVM();

        return View("~/Views/Admin/Main/CreateEdit.cshtml", model);
    }
    
    [HttpPost]
    [Route("/Admin/Main")]
    public async Task<IActionResult> SaveMain([FromForm] MainVM model, IFormFile file)
    {
        try 
        {
            if (file != null)
            {
                model.img_url = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "main");
            }
            
            var response = await _unitOfWorkRepository.Main.SaveAsync(model);
            return Json(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving main data: {@ExceptionDetails}", 
                new { ex.Message, ex.StackTrace });
            return Json(new AjaxResponse 
            { 
                Code = 500, 
                Message = "Terjadi kesalahan saat menyimpan data" 
            });
        }
    }

    [HttpGet("main/edit/{id}")]
    public IActionResult EditMain(Guid id)
    {

        var model = _unitOfWorkRepository.Main.GetMainByIdAsync(id).Result;
        if (model == null)
        {
            return View("~/Views/404/PageNotFound.cshtml");
        }
        return View("~/Views/Admin/Main/CreateEdit.cshtml", model);
    }

    [HttpDelete]
    [Route("/main/delete/{id}")]
    public async Task<IActionResult> DeleteMain(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Main.DeleteMainAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    [HttpPost]
    [Route("/main/toggle-hide/{id}")]
    public async Task<IActionResult> ToggleHideMain(Guid id)
    {
        var response = await _unitOfWorkRepository.Main.ToggleHideAsync(id);
        return Json(response);
    }
    #endregion
    #region  <=================================== Tutorial ========================================>
    public IActionResult Tutorial()
    {
        return View("~/Views/Admin/Tutorial/Index.cshtml");
    }

    [HttpGet("tutorial/create")]
    public IActionResult CreateEditTutorial()
    {
        TutorialVM model = new TutorialVM();

        return View("~/Views/Admin/Tutorial/CreateEdit.cshtml", model);
    }

    [HttpGet("tutorial/edit/{id}")]
    public IActionResult EditTutorial(Guid id)
    {

        var model = _unitOfWorkRepository.Tutorial.GetTutorialByIdAsync(id).Result;

        if (model == null)
        {
            return View("~/Views/404/PageNotFound.cshtml");
        }
        return View("~/Views/Admin/Tutorial/CreateEdit.cshtml", model);
    }

    [HttpPost]
    [Route("/Admin/Tutorial")]
    public async Task<IActionResult> SaveTutorial(TutorialVM model, IFormFile file)
    {
        AjaxResponse response = new();
        if (file != null)
        {
            model.img_url = await _unitOfWorkService.ImageUploads.UploadImageAsync(file, "tutorials");
        }
        response = await _unitOfWorkRepository.Tutorial.SaveAsync(model);
        return Json(response);
    }

    [HttpDelete]
    [Route("/tutorial/delete/{id}")]
    public async Task<IActionResult> DeleteTutorial(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Tutorial.DeleteAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    public async Task<IActionResult> GetTutorialData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Tutorial.GetDataTutorial(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    } 
    #endregion
   
     #region  <=================================== Footer ========================================>
    public IActionResult Footer()
    {
        return View("~/Views/Admin/Footer/Index.cshtml");
    }

    [HttpGet("footer/create")]
    public IActionResult CreateEditFooter()
    {
        FooterVM model = new FooterVM();

        return View("~/Views/Admin/Footer/CreateEdit.cshtml", model);
    }

    [HttpGet("footer/edit/{id}")]
    public IActionResult EditFooter(Guid id)
    {

        var model = _unitOfWorkRepository.Footer.GetFooterByIdAsync(id).Result;

        if (model == null)
        {
            return View("~/Views/404/PageNotFound.cshtml");
        }
        return View("~/Views/Admin/Footer/CreateEdit.cshtml", model);
    }

    [HttpPost]
    [Route("/Admin/Footer")]
    public async Task<IActionResult> SaveFooter(FooterVM model)
    {
        AjaxResponse response = new();

       
        response = await _unitOfWorkRepository.Footer.SaveAsync(model);
        return Json(response);
    }

    [HttpDelete]
    [Route("/footer/delete/{id}")]
    public async Task<IActionResult> DeleteFooter(Guid id)
    {
        AjaxResponse response = new();
        var msg = await _unitOfWorkRepository.Footer.DeleteAsync(id);

        if (msg)
        {
            response.Message = "Data berhasil dihapus";
            response.Code = 200;
        }
        else
        {
            response.Message = "Data gagal dihapus";
            response.Code = 500;
        }

        return Json(response);
    }

    public async Task<IActionResult> GetFooterData()
    {
        var ModelRequest = new JqueryDataTableRequest
        {
            Draw = Request.Form["draw"].FirstOrDefault() ?? "",
            Start = Request.Form["start"].FirstOrDefault() ?? "",
            Length = Request.Form["length"].FirstOrDefault() ?? "25",
            SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "",
            SortColumnDirection = Request.Form["order[0]dir"].FirstOrDefault() ?? "",
            SearchValue = Request.Form["search_value"].FirstOrDefault() ?? "",
            Status = Request.Form["status"].FirstOrDefault() ?? ""

        };

        try
        {
            if (ModelRequest.Length == "-1")
            {
                ModelRequest.PageSize = int.MaxValue;
            }
            else
            {
                ModelRequest.PageSize = ModelRequest.PageSize != null ? Convert.ToInt32(ModelRequest.Length) : 0;
            }

            ModelRequest.Skip = ModelRequest.Start != null ? Convert.ToInt32(ModelRequest.Start) : 0;

            var (rekomendasi, recordsTotal) = await _unitOfWorkRepository.Footer.GetDataFooter(ModelRequest);
            var jsonData = new { draw = ModelRequest.Draw, recordsFiltered = recordsTotal, recordsTotal, data = rekomendasi };
            return Json(jsonData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, DatatableRequest = ModelRequest });
            throw;
        }
    } 
    #endregion
   
}

