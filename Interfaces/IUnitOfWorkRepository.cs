using Higertech.Repositories;

namespace Higertech.Interfaces
{
  public interface IUnitOfWorkRepository
  {
    IUserRepository User { get; }
    IProductRepository Product { get; }
    IArticleRepository Article { get; }
    IProjectRepository Project { get; }
    IMainRepository Main { get; }
    IActivitiesRepository Activities { get; }
    IFooterRepository Footer { get; }

    ITutorialRepository Tutorial { get; }
  }
}
