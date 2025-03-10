using Higertech.Models;
using Higertech.ViewModels;
using Dapper;
using Npgsql;
using Serilog;
using Higertech.Models.Datatables;

namespace Higertech.Repositories;

public interface ITutorialRepository
{
    Task<List<Tutorial>> GetListTutorialAsync();
    Task<Tutorial?> GetTutorialBySlugAsync(string slug);
    Task<List<Tutorial>> GetAllAsync();
    Task<(IReadOnlyList<dynamic>, int)> GetDataTutorial(JqueryDataTableRequest request);
    Task<TutorialVM?> GetTutorialByIdAsync(Guid id);
    Task<AjaxResponse> SaveAsync(TutorialVM model);
    Task<bool> DeleteAsync(Guid id);
}

public class TutorialRepository : ITutorialRepository
{
    private readonly string _connectionString;
    public TutorialRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<AjaxResponse> SaveAsync(TutorialVM model)
    {
        AjaxResponse result = new();
        try
        {
            string query = "";
            string status = "Tambah";

            if (model.id == Guid.Empty)
            {
                query = @"
                    INSERT INTO 
                    tutorials ( title, description, img_url, yt_url)
                    VALUES (@title, @description, @img_url, @yt_url)
                    RETURNING *;";
            }
            else
            {
                status = "Memperbarui";
                model.updated_at = DateTime.Now;
                query = @"
                        UPDATE tutorials
                        SET title = @title,
                            description = @description,
                            img_url = @img_url,
                            yt_url = @yt_url,
                            updated_at = @updated_at
                        WHERE id = @Id
                        RETURNING *;";

            }


            using (var connection = new NpgsqlConnection(_connectionString))
            {

                var data = await connection.ExecuteAsync(query, model);

                result.Code = 200;
                result.Message = $"{status} data berhasil";
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving tutorials: {ex.Message}");
            result.Code = 500;
            result.Message = "Terjadi Kesalahan saat menyimpan data";
        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string query = @"
            UPDATE tutorials
            SET deleted_at = @Date_now
            WHERE id = @Id;";

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                int affectedRows = await connection.ExecuteAsync(query, new { Id = id, Date_now = DateTime.Now });
                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting tutorials: {ex.Message}");
            return false;
        }
    }


    public async Task<TutorialVM?> GetTutorialByIdAsync(Guid id)
    {
        const string query = @"
            SELECT * FROM tutorials WHERE id = @Id;";

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                TutorialVM? model = await connection.QuerySingleOrDefaultAsync<TutorialVM>(query, new { Id = id });
                return model;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching tutorials: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Tutorial>> GetAllAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var query = @$"
                        SELECT
                            id AS ""Id"",
                            name AS ""Name"",
                            value AS ""Value"",
                            created_at AS ""CreatedAt"",
                            updated_at AS ""UpdatedAt"";
                        FROM tutorials
                        WHERE deleted_at IS NULL
                        LIMIT 10
                        ORDER BY created_at DESC 
                        ;";

            var result = await connection.QueryAsync<Tutorial>(query);
            return result.ToList();
        }
        catch (NpgsqlException ex)
        {
            Log.Error(ex, "PostgreSQL Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace });
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace });
            throw;
        }
    }

    public async Task<(IReadOnlyList<dynamic>, int)> GetDataTutorial(JqueryDataTableRequest request)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            List<dynamic> result = new List<dynamic>();

            var query = $@"SELECT 
                    *
                    FROM 
                    tutorials 
                    ";

            var parameters = new DynamicParameters();
            var whereConditions = new List<string>();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                if (request.SearchValue.Contains('\''))
                {
                    request.SearchValue = request.SearchValue.Replace("'", "''");
                }


                whereConditions.Add(@"
                    (LOWER(title) LIKE @SearchValue OR
                    LOWER(description) LIKE @SearchValue)");
                parameters.Add("@SearchValue", "%" + request.SearchValue.ToLower() + "%");
            }

            // if (!string.IsNullOrEmpty(request.Status))
            // {
            //     whereConditions.Add(@"
            //     status = @Status");
            //     parameters.Add("@Status", request.Status);
            // }

            whereConditions.Add(@" deleted_at IS NULL");

            var whereClause = whereConditions.Count > 0 ? "WHERE" + string.Join(" AND ", whereConditions) : "";

            query += whereClause;

            query += @" ORDER BY updated_at DESC";

            int total = 0;
            var sql_count = $"SELECT COUNT(*) FROM ({query}) as total";
            total = connection.ExecuteScalar<int>(sql_count, parameters);

            query += @" 
                OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;";
            parameters.Add("@Skip", request.Skip);
            parameters.Add("@PageSize", request.PageSize);

            result = (await connection.QueryAsync<dynamic>(query, parameters)).ToList();

            return (result, total);
        }
        catch (Npgsql.NpgsqlException ex)
        {
            Log.Error(ex, "Sql Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, Desc = "Error while get data to table petak" });
            throw;
        }
        catch (System.Exception ex)
        {
            Log.Error(ex, "General Exception: {@ExceptionDetails}", new { ex.Message, ex.StackTrace, Desc = "Error while get data to table petak" });
            throw;
        }
    }

    public async Task<List<Tutorial>> GetListTutorialAsync()
    {
        const string query = @"
            SELECT 
                id AS ""Id"",
                title AS ""Title"",
                description AS ""Description"",
                yt_url AS ""Youtube"",
                img_url AS ""Image""
            FROM tutorials
            WHERE deleted_at IS NULL
            ORDER BY created_at DESC;";

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var model = await connection.QueryAsync<Tutorial>(query);
                return model.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Tutorial: {ex.Message}");
            return null;
        }
    }

    public async Task<Tutorial?> GetTutorialBySlugAsync(string id)
    {
        const string query = @"
            SELECT 
                id AS ""Id"",
                title AS ""Title"",
                description AS ""Description"",
                yt_url AS ""Youtube"",
                img_url AS ""Image"",
                created_at AS ""CreatedAt""
            FROM tutorials
            WHERE id = @Id
            AND deleted_at IS NULL;";

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Tutorial? model = await connection.QuerySingleOrDefaultAsync<Tutorial>(query, new { Id = id });
                return model;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching tutorials: {ex.Message}");
            return null;
        }
    }
}