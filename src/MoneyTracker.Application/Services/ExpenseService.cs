namespace MoneyTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;

    public ExpenseService(IExpenseRepository expenseRepo, ICategoryRepository categoryRepo)
    {
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<List<ExpenseDto>> GetByPeriodAsync(
        Guid userId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        var expenses = await _expenseRepo.GetByPeriodAsync(userId, from, to, ct);

        return expenses
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                Time = e.TimeUnix,
                Description = e.Description,
                Sum = e.Sum,
            })
            .ToList();
    }

    public async Task<ExpenseDto> AddAsync(Guid userId, ExpenseDto dto, CancellationToken ct)
    {
        if (dto.Sum <= 0)
            throw new ResponseException(ErrorType.Validation, "Сумма должна быть больше нуля");

        var category =
            await _categoryRepo.GetByIdAsync(userId, dto.CategoryId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        var expense = new ExpenseEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = category.Id,
            Category = category,
            Sum = dto.Sum,
            Description = dto.Description,
            TimeUnix = dto.Time,
        };

        var created = await _expenseRepo.AddAsync(expense, ct);

        return ToDto(created);
    }

    public async Task<ExpenseDto> AddFromQrAsync(
        Guid userId,
        ExpenseQrDto dto,
        CancellationToken ct
    )
    {
        if (dto.Sum <= 0)
            throw new ResponseException(ErrorType.Validation, "Сумма должна быть больше нуля");

        // Пытаемся найти категорию по имени из QR
        CategoryEntity? category = null;

        if (!string.IsNullOrEmpty(dto.CategoryName))
            category = await _categoryRepo.GetByNameAsync(userId, dto.CategoryName, ct);

        // Если не нашли — берём или создаём "Другое"
        if (category == null)
            category = await GetOrCreateDefaultCategoryAsync(userId, ct);

        var expense = new ExpenseEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = category.Id,
            Category = category,
            Sum = dto.Sum,
            Description = dto.Description,
            TimeUnix = dto.Time,
        };

        var created = await _expenseRepo.AddAsync(expense, ct);

        return ToDto(created);
    }

    public async Task<ExpenseDto> UpdateAsync(
        Guid userId,
        Guid id,
        ExpenseDto dto,
        CancellationToken ct
    )
    {
        if (dto.Sum <= 0)
            throw new ResponseException(ErrorType.Validation, "Сумма должна быть больше нуля");

        var expense =
            await _expenseRepo.GetByIdAsync(userId, id, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Трата не найдена");

        var category =
            await _categoryRepo.GetByIdAsync(userId, dto.CategoryId, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Категория не найдена");

        expense.Sum = dto.Sum;
        expense.Description = dto.Description;
        expense.CategoryId = category.Id;
        expense.Category = category;
        expense.TimeUnix = dto.Time;

        await _expenseRepo.UpdateAsync(expense, ct);

        return ToDto(expense);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var expense =
            await _expenseRepo.GetByIdAsync(userId, id, ct)
            ?? throw new ResponseException(ErrorType.NotFound, "Трата не найдена");

        await _expenseRepo.DeleteAsync(expense, ct);
    }

    // ─── Приватные вспомогательные ───────────────────────────────────────────

    private async Task<CategoryEntity> GetOrCreateDefaultCategoryAsync(
        Guid userId,
        CancellationToken ct
    )
    {
        const string defaultName = "Другое";

        var existing = await _categoryRepo.GetByNameAsync(userId, defaultName, ct);
        if (existing != null)
            return existing;

        var newCategory = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = defaultName,
            UserId = userId,
        };

        return await _categoryRepo.AddAsync(newCategory, ct);
    }

    private static ExpenseDto ToDto(ExpenseEntity e) =>
        new()
        {
            Id = e.Id,
            CategoryId = e.CategoryId,
            Time = e.TimeUnix,
            Description = e.Description,
            Sum = e.Sum,
        };
}
