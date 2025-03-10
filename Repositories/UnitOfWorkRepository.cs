using Higertech.Interfaces;

namespace Higertech.Repositories
{
  public class UnitOfWorkRepository : IUnitOfWorkRepository
  {
    public UnitOfWorkRepository(
      IUserRepository userRepository,
      IProductRepository productRepository,
      IArticleRepository articleRepository,
      IProjectRepository projectRepository,
      IMainRepository mainRepository,
      IActivitiesRepository activitiesRepository,
      IFooterRepository footerRepository,
      ITutorialRepository tutorialRepository
    )
    {
      User = userRepository;
      Product = productRepository;
      Article = articleRepository;
      Project = projectRepository;
      Main = mainRepository;
      Activities = activitiesRepository;
      Footer = footerRepository;
      Tutorial = tutorialRepository;
    }

    public IUserRepository User { get; }
    public IProductRepository Product { get; }
    public IArticleRepository Article { get; }
    public IProjectRepository Project { get; }
    public IMainRepository Main { get; }
    public IActivitiesRepository Activities { get; }
    public IFooterRepository Footer { get; }
    public ITutorialRepository Tutorial { get; }
  }
}
