using RetailOps.Core.Crm.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Crm;

public class CrmValueObjectsTests
{
    private const string ValidCpf = "52998224725";
    private const string ValidCnpj = "12345678000195";

    [Fact]
    public void Cpf_Create_WithValidValue_ReturnsSuccess()
    {
        var result = Cpf.Create(ValidCpf);
        Assert.True(result.IsSuccess);
        Assert.Equal(ValidCpf, result.Value!.Value);
    }

    [Fact]
    public void Cpf_Create_WithInvalidValue_ReturnsFailure()
    {
        var result = Cpf.Create("11111111111");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TaxDocument_Create_ForIndividual_UsesCpfValidation()
    {
        var result = TaxDocument.Create(ValidCpf, PersonType.Individual);
        Assert.True(result.IsSuccess);
        Assert.Equal(PersonType.Individual, result.Value!.PersonType);
    }

    [Fact]
    public void TaxDocument_Create_ForCompany_UsesCnpjValidation()
    {
        var result = TaxDocument.Create(ValidCnpj, PersonType.Company);
        Assert.True(result.IsSuccess);
        Assert.Equal(PersonType.Company, result.Value!.PersonType);
    }

    [Fact]
    public void TaxDocument_Create_ForCompany_WithInvalidCnpj_ReturnsFailure()
    {
        var result = TaxDocument.Create("11111111111111", PersonType.Company);
        Assert.True(result.IsFailure);
    }
}
