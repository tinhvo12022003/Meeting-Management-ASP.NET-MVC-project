using MeetingManagement.Common;
using MeetingManagement.Constant;
using MeetingManagement.Enum;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Service;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserHelper _helper;
    private const string ALL_ACTIVE_DEPTS_CACHE_KEY = "AllActiveDepartments";

    public DepartmentService(IUnitOfWork unitOfWork, UserHelper helper)
    {
        _unitOfWork = unitOfWork;
        _helper = helper;
    }

    public async Task Create(DepartmentCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var isExisted = await _unitOfWork.Departments.Existed(model.CompanyId, model.Name);
        if (isExisted == true)
        {
            throw new Exception(MessageConstant.EXISTED);
        }
        var department = new DepartmentModel
        {
            Name = model.Name,
            Location = model.Location,
            CompanyId = model.CompanyId,
            RowStatus = RowStatus.ACTIVE,
            CreateAt = DateTime.Now,
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.Departments.Add(department);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_DEPTS_CACHE_KEY);
    }

    public async Task Update(DepartmentUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var department = await _unitOfWork.Departments.GetById(model.Id);
        if (department == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }
        if (department.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }

        department.CompanyId = model.CompanyId;
        department.Name = model.Name;
        department.Location = model.Location;
        department.UpdateAt = DateTime.Now;
        department.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Departments.Update(department);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_DEPTS_CACHE_KEY);
    }

    public async Task Delete (string DepartmentId)
    {
        if (string.IsNullOrWhiteSpace(DepartmentId))
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var department = await _unitOfWork.Departments.GetById(DepartmentId);
        if (department == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        if(department.RowStatus == RowStatus.INACTIVE)
        {
            throw new Exception(MessageConstant.INACTIVE);
        }
        department.RowStatus = RowStatus.INACTIVE;
        department.UpdateAt = DateTime.Now;
        department.UpdateBy = _helper.GetCurrentUser();
        
        await _unitOfWork.Departments.Update(department);
        await _unitOfWork.CommitAsync();
        CacheHelper.Remove(ALL_ACTIVE_DEPTS_CACHE_KEY);
    }

    public async Task<PaginatedResponse<DepartmentViewModel>> Find(PaginatedRequest request, string? companyId = null)
    {
        var paginatedResult = await _unitOfWork.Departments.GetPaginated(
            request,
            baseFilter: x => x.RowStatus == RowStatus.ACTIVE && (string.IsNullOrEmpty(companyId) || x.CompanyId == companyId),
            searchFields: "Name,Location",
            includes: new [] {"Company", "Users"}
        );
        var viewModels = paginatedResult.Items.Select(x => new DepartmentViewModel
        {
            Id = x.Id,
            Name = x.Name,
            CompanyId = x.CompanyId,
            CompanyName = x.Company?.Name ?? string.Empty,
            TotalStaff = x.Users?.Count ?? 0,
            Location = x.Location,
            ManagerName = x.Users?.FirstOrDefault(u => u.userType == UserType.MANAGER)?.FullName ?? "Chưa cập nhật"

        }).ToList();

        return new PaginatedResponse<DepartmentViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };
    }

    public async Task<DepartmentUpdateModel?> GetById(string id)
    {
        var dept = await _unitOfWork.Departments.GetById(id);
        if (dept == null) return null;

        return new DepartmentUpdateModel
        {
            Id = dept.Id,
            Name = dept.Name ?? string.Empty,
            Location = dept.Location,
            CompanyId = dept.CompanyId,
            RowStatus = dept.RowStatus
        };
    }

    public async Task<List<DepartmentViewModel>> GetAllActive()
    {
        var cachedData = CacheHelper.Get<List<DepartmentViewModel>>(ALL_ACTIVE_DEPTS_CACHE_KEY);
        if (cachedData != null) return cachedData;

        var depts = await _unitOfWork.Departments.GetAllActive();
        var result = depts.Select(x => new DepartmentViewModel
        {
            Id = x.Id,
            Name = x.Name,
            CompanyId = x.CompanyId,
            Location = x.Location
        }).ToList();

        CacheHelper.Set(ALL_ACTIVE_DEPTS_CACHE_KEY, result, 30);
        return result;
    }
}