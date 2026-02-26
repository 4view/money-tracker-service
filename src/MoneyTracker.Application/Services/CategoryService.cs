namespace MoneyTracker.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    public async Task<List<CategoryDto>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        var categories = await _categoryRepo.GetAllAsync(userId, ct);

        return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var category =
            await _categoryRepo.GetByIdAsync(userId, id, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<CategoryDto> AddAsync(Guid userId, CategoryDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ResponseException(
                ErrorType.Validation,
                "Название категории не может быть пустым"
            );

        var nameExists = await _categoryRepo.ExistsWithNameAsync(
            userId,
            dto.Name,
            excludeId: null,
            ct
        );
        if (nameExists)
            throw new ResponseException(
                ErrorType.Conflict,
                $"Категория '{dto.Name}' уже существует"
            );

        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            UserId = userId,
        };

        var created = await _categoryRepo.AddAsync(category, ct);

        return new CategoryDto { Id = created.Id, Name = created.Name };
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid userId,
        Guid id,
        CategoryDto dto,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ResponseException(
                ErrorType.Validation,
                "Название категории не может быть пустым"
            );

        var category =
            await _categoryRepo.GetByIdAsync(userId, id, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        var nameExists = await _categoryRepo.ExistsWithNameAsync(
            userId,
            dto.Name,
            excludeId: id,
            ct
        );
        if (nameExists)
            throw new ResponseException(
                ErrorType.Conflict,
                $"Категория '{dto.Name}' уже существует"
            );

        category.Name = dto.Name.Trim();

        await _categoryRepo.UpdateAsync(category, ct);

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var category =
            await _categoryRepo.GetByIdAsync(userId, id, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        var hasExpenses = await _categoryRepo.HasExpensesAsync(userId, id, ct);
        if (hasExpenses)
        {
            var count = await _categoryRepo.CountExpensesAsync(userId, id, ct);
            throw new ResponseException(
                ErrorType.Validation,
                $"Нельзя удалить категорию '{category.Name}', она используется в {count} тратах"
            );
        }

        await _categoryRepo.DeleteAsync(category, ct);
    }
}
