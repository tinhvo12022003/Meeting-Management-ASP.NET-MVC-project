using MeetingManagement.Common;
using MeetingManagement.Constant;
using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserHelper _helper;
    private const string ALL_ACTIVE_COMPANIES_CACHE_KEY = "AllActiveCompanies";
    public CompanyService(IUnitOfWork unitOfWork, UserHelper helper)
    {
        _unitOfWork = unitOfWork;
        _helper = helper;
    }

    public async Task Create(CompanyCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Address))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }

        var isExisted = await _unitOfWork.Companies.GetByName(model.Name);
        if (isExisted != null)
        {
            throw new Exception(MessageConstant.EXISTED);
        }
        var company = new CompanyModel
        {
            Name = model.Name,
            Address = model.Address,
            Phone = model.Phone,
            Email = model.Email,
            TaxCode = model.TaxCode,
            RowStatus = RowStatus.ACTIVE,
            CreateAt = DateTime.UtcNow,
            CreateBy =  _helper.GetCurrentUser()
        };
        await _unitOfWork.Companies.Add(company);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_COMPANIES_CACHE_KEY);
    }

    public async Task Update(CompanyUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Address))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var company = await _unitOfWork.Companies.GetById(model.Id);
        if (company == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }
        if (company.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        company.Name = model.Name;
        company.Address = model.Address;
        company.Phone = model.Phone;
        company.Email = model.Email;
        company.TaxCode = model.TaxCode;
        company.UpdateAt = DateTime.UtcNow;
        company.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Companies.Update(company);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_COMPANIES_CACHE_KEY);
    }

    public async Task Delete(string CompanyId)
    {
        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var company = await _unitOfWork.Companies.GetById(CompanyId);
        if (company == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }
        if (company.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }
        company.RowStatus = RowStatus.INACTIVE;
        company.UpdateAt = DateTime.UtcNow;
        company.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Companies.Update(company);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_COMPANIES_CACHE_KEY);
    }

    public async Task<PaginatedResponse<CompanyViewModel>> Find(PaginatedRequest request)
    {
        // Xây dựng filter theo ColumnFilters["status"], mặc định chỉ lấy ACTIVE
        System.Linq.Expressions.Expression<Func<CompanyModel, bool>> baseFilter =
            x => x.RowStatus == RowStatus.ACTIVE;

        if (request.ColumnFilters != null &&
            request.ColumnFilters.TryGetValue("status", out var statusValue) &&
            !string.IsNullOrWhiteSpace(statusValue))
        {
            if (System.Enum.TryParse<RowStatus>(statusValue, ignoreCase: true, out var parsedStatus))
            {
                baseFilter = x => x.RowStatus == parsedStatus;
            }
        }

        var paginatedResult = await _unitOfWork.Companies.GetPaginated(
            request,
            baseFilter: baseFilter,  // Dùng filter động thay vì hardcode ACTIVE
            searchFields: "Name,Address"
        );
        var viewModels = paginatedResult.Items.Select(x => new CompanyViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Address = x.Address,
            Phone = x.Phone,
            Email = x.Email,
            TaxCode = x.TaxCode,
            rowStatus = x.RowStatus
        }).ToList();
        return new PaginatedResponse<CompanyViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };

    }

    public async Task<CompanyUpdateModel?> GetById(string id)
    {
        var company = await _unitOfWork.Companies.GetById(id);
        if (company == null) return null;

        return new CompanyUpdateModel
        {
            Id = company.Id,
            Name = company.Name ?? string.Empty,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
            TaxCode = company.TaxCode,
            RowStatus = company.RowStatus
        };
    }

    public async Task<List<CompanyViewModel>> GetAllActive()
    {
        var cachedData = CacheHelper.Get<List<CompanyViewModel>>(ALL_ACTIVE_COMPANIES_CACHE_KEY);
        if (cachedData != null) return cachedData;

        // Dùng repository method có DB-level filter — không load toàn bộ bảng vào RAM
        var activeCompanies = await _unitOfWork.Companies.GetAllActive();
        var result = activeCompanies.Select(x => new CompanyViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Address = x.Address,
            Phone = x.Phone,
            Email = x.Email,
            TaxCode = x.TaxCode,
            rowStatus = x.RowStatus
        }).ToList();

        CacheHelper.Set(ALL_ACTIVE_COMPANIES_CACHE_KEY, result, 30); // Cache trong 30 phút
        return result;
    }
}
