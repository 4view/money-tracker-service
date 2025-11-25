namespace MoneyTracker.Tests.Integration.Repositories;

public class CategoryRepositoryTest : IDisposable
{
    private readonly ApplicationDbContext _db;

    private readonly CategoryRepository _categoryRepository;

    public CategoryRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        _db = new ApplicationDbContext(options);
        _categoryRepository = new CategoryRepository(_db);
    }

    public void Dispose()
    {
        _db?.Dispose();
    }

    [Fact]
    public async Task AddCategoryAsync_WithValidData_ShouldAddCategory()
    {
        // Arrange
        var categoryDto = new CategoryDto { Name = "Food" };

        // Act
        var result = await _categoryRepository.AddCategoryAsync(
            categoryDto,
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Food", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);

        // Проверяем, что категория действительно сохранена в базе
        var categoryInDb = await _db.Set<CategoryEntity>().FirstOrDefaultAsync();
        Assert.NotNull(categoryInDb);
        Assert.Equal("Food", categoryInDb.Name);
    }

    [Fact]
    public async Task AddCategoryAsync_WithEmptyName_ShouldThrowValidationException()
    {
        //Arrange
        var categoryDto = new CategoryDto { Name = " " };

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryRepository.AddCategoryAsync(categoryDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddCategoryAsync_WhenCategoryExist_ShouldThrowValidationException()
    {
        //Arrange
        var categoryDto = await CreateCategoryAsync(_db, "Продукты");

        var sameCategoryDto = new CategoryDto { Name = "Продукты" };

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryRepository.AddCategoryAsync(sameCategoryDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithValidData_ShouldUpdateCategory()
    {
        //Arrange
        var existCategory = await CreateCategoryAsync(_db, "Продукты");

        var updateDto = new CategoryDto { Name = "Транспорт" };

        // Act
        var result = await _categoryRepository.UpdateCategoryAsync(
            existCategory.Id,
            updateDto,
            CancellationToken.None
        );

        //Assert
        Assert.NotNull(result);
        Assert.Equal(existCategory.Id, result.Id);
        Assert.Equal("Транспорт", result.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithWrongCategoryId_ShouldThrowValidationException()
    {
        //Arrange
        var existCategory = await CreateCategoryAsync(_db, "Продукты");

        var passedId = Guid.NewGuid();

        var updateCategoryDto = new CategoryDto { Name = "Транспорт" };

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryRepository.UpdateCategoryAsync(
                passedId,
                updateCategoryDto,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryExist_ShouldThrowValidationException()
    {
        //Arrange
        var existCategory1 = await CreateCategoryAsync(_db, "Продукты");

        var existCategory2 = await CreateCategoryAsync(_db, "Транспорт");

        var updateCategoryDto = new CategoryDto { Name = "Транспорт" };

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryRepository.UpdateCategoryAsync(
                existCategory1.Id,
                updateCategoryDto,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithValidData_ShouldDeleteCategory()
    {
        //Arrange
        var existCategory = await CreateCategoryAsync(_db);

        //Act
        await _categoryRepository.DeleteCategoryAsync(existCategory.Id, CancellationToken.None);
        var result = await _db.Set<CategoryEntity>().AnyAsync(CancellationToken.None);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithWrongCategoryId_ShouldThrowValidationException()
    {
        //Arrange
        var passedId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryRepository.DeleteCategoryAsync(passedId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryHasExpense_ShouldThrowValidationException()
    {
        //Arrange
        var existCategory = await CreateCategoryAsync(_db);

        var existExpense = await ExpenseRepositoryTests.CreateExpenseAsync(_db, existCategory);

        //Act & Assert
        await Assert.ThrowsAsync<ResponseException>(() =>
            _categoryRepository.DeleteCategoryAsync(existExpense.Id, CancellationToken.None)
        );
    }

    public static async Task<CategoryEntity> CreateCategoryAsync(
        ApplicationDbContext context,
        string? name = null
    )
    {
        if (name == null)
            name = Guid.NewGuid().ToString();

        var existCategory = new CategoryEntity { Id = Guid.NewGuid(), Name = name };

        await context.Set<CategoryEntity>().AddAsync(existCategory);
        await context.SaveChangesAsync();

        return existCategory;
    }
}
