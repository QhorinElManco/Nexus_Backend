using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Customers;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Customers;

namespace Nexus.Application.UseCases.Customers;

public class CustomerService(
    ICustomerRepository customerRepository,
    ICompanyRepository companyRepository,
    IValidator<CreateCustomerDto> createValidator,
    IValidator<UpdateCustomerDto> updateValidator,
    IValidator<CustomerSearchRequest> searchValidator,
    ILogger<CustomerService> logger) : ICustomerService
{
    public async Task<Response<CustomerDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdWithAssignmentsAsync(id, ct);

        return customer is null
            ? Response<CustomerDto>.Fail("Customer not found", ErrorCode.NotFound)
            : Response<CustomerDto>.Ok(MapToDto(customer));
    }

    public async Task<Response<IReadOnlyList<CustomerDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await customerRepository.GetAllAsync(ct);
        return Response<IReadOnlyList<CustomerDto>>.Ok(customers.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<CustomerDto>> SearchAsync(CustomerSearchRequest request, CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<CustomerDto>();
        }

        var (items, totalCount) = await customerRepository.SearchAsync(request, ct);

        return ResponsePagination<CustomerDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<CustomerDto>> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<CustomerDto>();
        }

        var companyExists = await companyRepository.GetByIdAsync(dto.CompanyId, ct);
        if (companyExists is null)
        {
            logger.LogWarning("Create customer failed: company not found [{CompanyId}]", dto.CompanyId);
            return Response<CustomerDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        if (await customerRepository.ExistsByTaxIdAsync(dto.TaxId, ct: ct))
        {
            logger.LogWarning("Create customer failed: TaxId already exists [{TaxId}]", dto.TaxId);
            return Response<CustomerDto>.Fail("TaxId already exists", ErrorCode.Conflict);
        }

        var customer = new Customer
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            TradeName = dto.TradeName,
            TaxId = dto.TaxId,
            Lat = dto.Lat,
            Lng = dto.Lng,
            Status = dto.Status
        };

        var created = await customerRepository.AddAsync(customer, ct);
        var customerWithRelations = await customerRepository.GetByIdWithAssignmentsAsync(created.Id, ct);

        logger.LogInformation("Customer created [{CustomerId}] [{Name}] [{CompanyId}]", created.Id, created.Name, created.CompanyId);

        return Response<CustomerDto>.Ok(MapToDto(customerWithRelations!));
    }

    public async Task<Response<CustomerDto>> UpdateAsync(long id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<CustomerDto>();
        }

        var customer = await customerRepository.GetByIdAsync(id, ct);
        if (customer is null)
        {
            logger.LogWarning("Update customer failed: customer not found [{CustomerId}]", id);
            return Response<CustomerDto>.Fail("Customer not found", ErrorCode.NotFound);
        }

        customer.Name = dto.Name;
        customer.TradeName = dto.TradeName;
        customer.Lat = dto.Lat;
        customer.Lng = dto.Lng;
        customer.Status = dto.Status;

        await customerRepository.UpdateAsync(customer, ct);

        var customerWithRelations = await customerRepository.GetByIdWithAssignmentsAsync(id, ct);

        logger.LogInformation("Customer updated [{CustomerId}] [{Name}]", id, customer.Name);

        return Response<CustomerDto>.Ok(MapToDto(customerWithRelations!));
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, ct);
        if (customer is null)
        {
            logger.LogWarning("Delete customer failed: customer not found [{CustomerId}]", id);
            return Response<bool>.Fail("Customer not found", ErrorCode.NotFound);
        }

        await customerRepository.DeleteAsync(id, ct);

        logger.LogInformation("Customer deleted (soft-delete) [{CustomerId}] [{Name}]", id, customer.Name);

        return Response<bool>.Ok(true);
    }

    public async Task<Response<CustomerDto>> GetByTaxIdAsync(string taxId, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByTaxIdAsync(taxId, ct);

        return customer is null
            ? Response<CustomerDto>.Fail("Customer not found", ErrorCode.NotFound)
            : Response<CustomerDto>.Ok(MapToDto(customer));
    }

    public async Task<Response<IReadOnlyList<CustomerDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        var customers = await customerRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<CustomerDto>>.Ok(customers.Select(MapToDto).ToList());
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.CompanyId,
            customer.Company?.Name ?? string.Empty,
            customer.Name,
            customer.TradeName,
            customer.TaxId,
            customer.Lat,
            customer.Lng,
            customer.Status,
            customer.CustomerAssignments.Select(ca => new CustomerAssignmentDto(
                ca.Id,
                ca.CustomerId,
                ca.UserId,
                ca.User.FullName,
                ca.DayOfWeek,
                ca.SequenceOrder
            )).ToList(),
            customer.CreatedAt,
            customer.UpdatedAt
        );
    }
}
