using Moq;
using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.UseCases;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Crm;

public class CreateSupplierUseCaseTests
{
    private const string ValidCnpj = "12345678000195";

    [Fact]
    public async Task Execute_WhenTaxDocumentExists_ReturnsFailure()
    {
        var repo = new Mock<ISupplierRepository>();
        repo.Setup(r => r.TaxDocumentExists(It.IsAny<TenantId>(), It.IsAny<TaxDocument>(), null))
            .ReturnsAsync(Result<bool>.Success(true));

        var sut = new CreateSupplierUseCase(repo.Object);
        var result = await sut.Execute((1, new CreateSupplierInputDto
        {
            Name = "Fornecedor",
            PersonType = PersonType.Company,
            TaxDocument = ValidCnpj,
        }));

        Assert.True(result.IsFailure);
        Assert.Contains("already registered", result.Error);
    }

    [Fact]
    public async Task Execute_WithValidInput_SavesAndReturnsDto()
    {
        var repo = new Mock<ISupplierRepository>();
        repo.Setup(r => r.TaxDocumentExists(It.IsAny<TenantId>(), It.IsAny<TaxDocument>(), null))
            .ReturnsAsync(Result<bool>.Success(false));
        repo.Setup(r => r.Save(It.IsAny<Supplier>()))
            .ReturnsAsync(Result.Success());

        var sut = new CreateSupplierUseCase(repo.Object);
        var result = await sut.Execute((1, new CreateSupplierInputDto
        {
            Name = "Fornecedor",
            PersonType = PersonType.Company,
            TaxDocument = ValidCnpj,
            IsActive = true,
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Fornecedor", result.Value.Name);
        repo.Verify(r => r.Save(It.IsAny<Supplier>()), Times.Once);
    }
}

public class DeactivateSupplierUseCaseTests
{
    [Fact]
    public async Task Execute_DeactivatesSupplier()
    {
        var supplier = Supplier.Create(
            TenantId.Create(1).Value,
            PersonName.Create("Fornecedor").Value,
            PersonType.Individual,
            TaxDocument.Create("39053344705", PersonType.Individual).Value).Value;

        var repo = new Mock<ISupplierRepository>();
        repo.Setup(r => r.GetById(supplier.Id)).ReturnsAsync(Result<Supplier>.Success(supplier));
        repo.Setup(r => r.Save(It.IsAny<Supplier>())).ReturnsAsync(Result.Success());

        var sut = new DeactivateSupplierUseCase(repo.Object);
        var result = await sut.Execute((1, supplier.Id));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }
}
