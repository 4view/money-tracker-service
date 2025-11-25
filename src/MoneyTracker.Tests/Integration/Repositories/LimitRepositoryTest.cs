using Xunit.Sdk;

namespace MoneyTracker.Tests.Integration.Repositories;

[Trait("Limit", "Integration")]
public class LimitRepositoryTest : IDisposable
{
    private readonly ApplicationDbContext _db;

    private readonly CategoryLimitRepository _categoryLimitRepository;

    public LimitRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        _db = new ApplicationDbContext(options);
        _categoryLimitRepository = new CategoryLimitRepository(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task GetAllLimitsAsnc_WithValidData_ShouldGetAllLimits()
    {
        //Arrange
        var existCategory = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var existLimit = await CreateLimitAsync(_db, existCategory);
        //Act
        var result = await _categoryLimitRepository.GetAllLimitsAsync(CancellationToken.None);

        //Assert
        Assert.NotNull(result);

        var limitInDB = _db.Set<CategoryLimitEntity>().ToList();
        Assert.Contains(existLimit, limitInDB);
    }

    [Fact]
    public async Task GetLimitAsync_WithValidData_ShuoldGetCategoryLimit()
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var limit = await CreateLimitAsync(_db, category, 5000);

        var existExpense = await ExpenseRepositoryTests.CreateExpenseAsync(
            _db,
            category,
            1754636400,
            "smth",
            150
        );

        var expectedLimitDto = new ReturnedLimitDto
        {
            Id = limit.Id,
            CategoryId = category.Id,
            Limit = limit.Limit,
            Remaining = limit.Limit - existExpense.Sum,
        };

        //Act
        var result = await _categoryLimitRepository.GetCategoryLimitAsync(
            limit.Id,
            category.Id,
            1754031600,
            1756580400,
            CancellationToken.None
        );

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Equals(expectedLimitDto));
    }

    [Fact]
    public async Task GetLimitAsync_WithNotFoundCategory_ShouldThrowValidationException()
    {
        //Arrange
        var passedCategoryId = Guid.NewGuid();
        var passedLimitId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.GetCategoryLimitAsync(
                passedLimitId,
                passedCategoryId,
                1754636400,
                1754982000,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task GetLimitAsync_WithNotFoundLimit_ShouldThrowValidationException()
    {
        //Act
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var passedLimitId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.GetCategoryLimitAsync(
                passedLimitId,
                category.Id,
                1754636400,
                1754982000,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task AddLimitAsync_WithValidData_ShouldAddCategoryLimit()
    {
        //Arrange
        var existCategory = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var receivedLimitDto = new BaseLimitDto { CategoryId = existCategory.Id, Limit = 5000 };

        //Act
        var result = await _categoryLimitRepository.AddCategoryLimitAsync(
            receivedLimitDto,
            CancellationToken.None
        );

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Equals(receivedLimitDto));

        var savedCategoryLimit = _db.Set<CategoryLimitEntity>().First();

        Assert.NotNull(savedCategoryLimit);
        Assert.Equal(result.Id, savedCategoryLimit.Id);
    }

    [Fact]
    public async Task AddLimitAsync_WithAlreadyExistLimit_ShouldThrowValidationException()
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var limit = await CreateLimitAsync(_db, category);

        var limitToAdd = new BaseLimitDto { CategoryId = category.Id, Limit = 5000 };

        //Act
        var result = await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.AddCategoryLimitAsync(limitToAdd, CancellationToken.None)
        );

        //Assert
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task AddLimitAsync_WithNotfoundCategory_ShouldThrowValidationException()
    {
        //Arrange
        var fakeCategoryId = Guid.NewGuid();

        var limitToAdd = new BaseLimitDto { CategoryId = fakeCategoryId, Limit = 5000 };

        //Act
        var result = await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.AddCategoryLimitAsync(limitToAdd, CancellationToken.None)
        );

        //Assert
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public async Task AddLimitAsync_WithInvalidLimit_ShouldThrowValidationException(
        decimal InvalidLimit
    )
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var limitToAdd = new BaseLimitDto { CategoryId = category.Id, Limit = InvalidLimit };

        //Act
        var result = await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.AddCategoryLimitAsync(limitToAdd, CancellationToken.None)
        );

        //Assert
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.0)]
    [InlineData(1000.0)]
    [InlineData(999999.99)]
    [InlineData(1000000.0)]
    public async Task UpdateLimitAsync_WithValidData_ShouldUpdateCategoryLimit(decimal validLimit)
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var limit = await CreateLimitAsync(_db, category, 5000);

        var updatedLimit = new BaseLimitDto
        {
            Id = limit.Id,
            CategoryId = category.Id,
            Limit = validLimit,
        };

        //Act
        var result = await _categoryLimitRepository.UpdateCategoryLimitAsync(
            limit.Id,
            updatedLimit,
            CancellationToken.None
        );

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Equals(updatedLimit));
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0.0)]
    [InlineData(-0.1)]
    [InlineData(-999999.99)]
    public async Task UpdateLimitAsync_WithInValidLimit_ShouldThrowValidationException(
        decimal invalidLimit
    )
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var limit = await CreateLimitAsync(_db, category);

        var updatedLimit = new BaseLimitDto
        {
            Id = limit.Id,
            CategoryId = category.Id,
            Limit = invalidLimit,
        };

        //Act
        var result = await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.UpdateCategoryLimitAsync(
                limit.Id,
                updatedLimit,
                CancellationToken.None
            )
        );

        //Assert
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task UpdateLimitAsync_WhenCategoryDontHaveLimit_ShouldThrowValidationException()
    {
        //Arrange
        var existCategory = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var passedLimitId = Guid.NewGuid();

        var updatedLimit = new BaseLimitDto
        {
            Id = passedLimitId,
            CategoryId = existCategory.Id,
            Limit = 5000,
        };

        //Act
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.UpdateCategoryLimitAsync(
                passedLimitId,
                updatedLimit,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task DeleteLimitAsync_WithValidData_ShouldDeleteLimit()
    {
        //Arrange
        var category = await CategoryRepositoryTest.CreateCategoryAsync(_db);

        var limit = await CreateLimitAsync(_db, category);

        //Act
        await _categoryLimitRepository.DeleteCategoryLimitAsync(limit.Id, CancellationToken.None);

        //Assert
        var isLimitExist = await _db.Set<CategoryLimitEntity>().AnyAsync();

        Assert.False(isLimitExist);
    }

    [Fact]
    public async Task DeleteLimitAsync_WhenLimitNotFound_ShouldThrowNotFoundException()
    {
        //Arrange
        var passedLimitId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryLimitRepository.DeleteCategoryLimitAsync(passedLimitId, CancellationToken.None)
        );
    }

    public static async Task<CategoryLimitEntity> CreateLimitAsync(
        ApplicationDbContext context,
        CategoryEntity category,
        decimal? limit = null
    )
    {
        var categoryLimit = new CategoryLimitEntity
        {
            Id = Guid.NewGuid(),
            Category = category,
            Limit = limit ?? new Random().Next(1, 5000),
        };

        await context.Set<CategoryLimitEntity>().AddAsync(categoryLimit);
        await context.SaveChangesAsync();

        return categoryLimit;
    }
}
