namespace MoneyTracker.Tests.Integration.Repositories;

public class ExpenseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    private readonly ExpenseRepository _expenseRepository;

    public ExpenseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        _db = new ApplicationDbContext(options);
        _expenseRepository = new ExpenseRepository(_db);
    }

    public void Dispose()
    {
        _db?.Dispose();
    }

    [Fact]
    public async Task GetExpenseByTimeAsync_WithValidData_ShouldGetExpenses()
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);
        var firstExpense = await CreateExpenseAsync(_db, category, 1757064405, "Купил молочка");
        var secondExpense = await CreateExpenseAsync(_db, category, 1754386005, "Купил кокосик");

        //Act
        var result = await _expenseRepository.GetExpenseByTimeAsync(
            1756666800,
            1759172400,
            CancellationToken.None
        );

        //Assert
        Assert.NotEmpty(result);

        var getedExpense = result.FirstOrDefault();
        Assert.NotNull(getedExpense);
        Assert.Equal("Купил молочка", getedExpense.Description);
    }

    [Fact]
    public async Task AddExpenseAsync_WithValidData_ShouldAddExpense()
    {
        //Arrange
        var existCategory = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var passedExpenseDto = new ExpenseDto
        {
            CategoryId = existCategory.Id,
            Time = 1748846796,
            Description = "Купил булку",
            Sum = 89,
        };

        //Act
        var result = await _expenseRepository.AddExpenseAsync(
            passedExpenseDto,
            CancellationToken.None
        );

        //Assert
        Assert.NotNull(result);
        Assert.Equal(existCategory.Id, passedExpenseDto.CategoryId);

        var expenseInDB = await _db.Set<ExpenseEntity>().FirstOrDefaultAsync();
        Assert.NotNull(expenseInDB);
    }

    [Fact]
    public async Task AddExpenseAsync_WithWrongCategory_ShouldThrowValidationException()
    {
        //Arrange
        var passedExpenseDto = new ExpenseDto
        {
            CategoryId = Guid.NewGuid(),
            Time = 1748846796,
            Description = "Купил булку",
            Sum = 89,
        };

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _expenseRepository.AddExpenseAsync(passedExpenseDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddExpenseAsync_WithIncorrectSum_ShouldThrowValidationException()
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var passedExpenseDto = new ExpenseDto
        {
            CategoryId = category.Id,
            Time = 1748846796,
            Description = "Купил булку",
            Sum = -100,
        };

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _expenseRepository.AddExpenseAsync(passedExpenseDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task DeleteExpenseAsync_WithValidData_ShouldDeleteExpense()
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);
        var existExpense = await CreateExpenseAsync(_db, category);

        //Act
        await _expenseRepository.DeleteExpenseAsync(existExpense.Id, CancellationToken.None);

        //Assert
        var expenses = _db.Set<ExpenseEntity>().FirstOrDefault();

        Assert.Null(expenses);
    }

    [Fact]
    public async Task DeleteExpenseAsync_WithWrongExpenseId_ShouldThrowValidationException()
    {
        //Arrange
        var passedId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _expenseRepository.DeleteExpenseAsync(passedId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateExpenseAsync_WithValidData_ShouldUpdateExpense()
    {
        //Arrange
        var existcategory1 = await CategoryRepositoryTest.CreateCategoryAsync(_db, "Продукты");
        var existcategory2 = await CategoryRepositoryTest.CreateCategoryAsync(_db, "Транспорт");

        var existExpense = await CreateExpenseAsync(
            _db,
            existcategory1,
            1748846796,
            "Rahat Lukum",
            154
        );

        var updatedExpenseDto = new ExpenseDto
        {
            Id = existExpense.Id,
            CategoryId = existcategory2.Id,
            Time = 1757295426,
            Description = "Поехал за рахатом",
            Sum = 250,
        };

        //Act
        var result = await _expenseRepository.UpdateExpenseAsync(
            existExpense.Id,
            updatedExpenseDto,
            CancellationToken.None
        );

        //Assert
        Assert.True(result.Equals(updatedExpenseDto));

        var expenseInDB = _db.Set<ExpenseEntity>().FirstOrDefault();

        Assert.NotNull(expenseInDB);
        Assert.Equal(result.Id, expenseInDB.Id);
    }

    public static async Task<ExpenseEntity> CreateExpenseAsync(
        ApplicationDbContext context,
        CategoryEntity category,
        long? timeUnix = null,
        string? description = null,
        decimal? sum = null
    )
    {
        var expense = new ExpenseEntity
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            TimeUnix = timeUnix ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Description = description ?? Guid.NewGuid().ToString(),
            Sum = sum ?? (decimal)((new Random().NextDouble() + 1) * 1000),
        };

        await context.Set<ExpenseEntity>().AddAsync(expense);
        await context.SaveChangesAsync();

        return expense;
    }
}
