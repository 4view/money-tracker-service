namespace MoneyTracker.Application.Services;

public class LimitService : ILimitService
{
    private readonly ICategoryLimitRepository _limitRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IExpenseRepository _expenseRepo;

    public LimitService(
        ICategoryLimitRepository limitRepo,
        ICategoryRepository categoryRepo,
        IExpenseRepository expenseRepo
    )
    {
        _limitRepo = limitRepo;
        _categoryRepo = categoryRepo;
        _expenseRepo = expenseRepo;
    }

    public async Task<List<AddedLimitDto>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        var limits = await _limitRepo.GetAllAsync(userId, ct);

        return limits
            .Select(l => new AddedLimitDto
            {
                Id = l.Id,
                CategoryId = l.Category.Id,
                Limit = l.Limit,
            })
            .ToList();
    }

    public async Task<ReturnedLimitDto> GetWithCalculationAsync(
        Guid userId,
        Guid limitId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        var category =
            await _categoryRepo.GetByIdAsync(userId, categoryId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        var limit =
            await _limitRepo.GetByIdAsync(userId, limitId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Лимит не найден");

        var spent = await _expenseRepo.GetSumByCategoryAndPeriodAsync(
            userId,
            categoryId,
            from,
            to,
            ct
        );

        return new ReturnedLimitDto
        {
            Id = limit.Id,
            CategoryId = categoryId,
            Limit = limit.Limit,
            Remaining = limit.Limit - spent,
        };
    }

    public async Task<AddedLimitDto> AddAsync(Guid userId, BaseLimitDto dto, CancellationToken ct)
    {
        if (dto.Limit <= 0)
            throw new ResponseException(ErrorType.Validation, "Лимит должен быть больше нуля");

        var category =
            await _categoryRepo.GetByIdAsync(userId, dto.CategoryId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        var alreadyExists = await _limitRepo.ExistsForCategoryAsync(userId, dto.CategoryId, ct);
        if (alreadyExists)
            throw new ResponseException(ErrorType.Conflict, "Для этой категории уже задан лимит");

        var limit = new CategoryLimitEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = category,
            Limit = dto.Limit,
        };

        var created = await _limitRepo.AddAsync(limit, ct);

        return new AddedLimitDto
        {
            Id = created.Id,
            CategoryId = category.Id,
            Limit = created.Limit,
        };
    }

    public async Task UpdateAsync(Guid userId, Guid limitId, BaseLimitDto dto, CancellationToken ct)
    {
        if (dto.Limit <= 0)
            throw new ResponseException(ErrorType.Validation, "Лимит должен быть больше нуля");

        var limit =
            await _limitRepo.GetByIdAsync(userId, limitId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Лимит не найден");

        limit.Limit = dto.Limit;

        await _limitRepo.UpdateAsync(limit, ct);
    }

    public async Task DeleteAsync(Guid userId, Guid limitId, CancellationToken ct)
    {
        var limit =
            await _limitRepo.GetByIdAsync(userId, limitId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Лимит не найден");

        await _limitRepo.DeleteAsync(limit, ct);
    }
}
