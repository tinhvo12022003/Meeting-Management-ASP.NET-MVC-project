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
            CompanyId = model.CompanyId,
            RowStatus = model.RowStatus,
            CreateAt = DateTime.UtcNow,
            CreateBy = _helper.GetCurrentUser()
        };

        await _unitOfWork.Departments.Add(department);
        await _unitOfWork.CommitAsync();
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
        department.UpdateAt = DateTime.UtcNow;
        department.UpdateBy = _helper.GetCurrentUser();

        await _unitOfWork.Departments.Update(department);
        await _unitOfWork.CommitAsync();
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
        department.UpdateAt = DateTime.UtcNow;
        department.UpdateBy = _helper.GetCurrentUser();
        
        await _unitOfWork.Departments.Update(department);
        await _unitOfWork.CommitAsync();
    }

    public async Task<PaginatedResponse<DepartmentViewModel>> Find(PaginatedRequest request, string? companyId = null)
    {
        var paginatedResult = await _unitOfWork.Departments.GetPaginated(
            request,
            baseFilter: x => x.RowStatus == RowStatus.ACTIVE && (string.IsNullOrEmpty(companyId) || x.CompanyId == companyId),
            searchFields: "Name,Location,TotalStaff", 
            includes: new [] {"Company,Users"}
        );
        var viewModels = paginatedResult.Items.Select(x => new DepartmentViewModel
        {
            Id = x.Id,
            Name = x.Name,
            CompanyName = x.Company?.Name ?? string.Empty,
            TotalStaff = x.Users?.Count ?? 0,
            ManagerName = x.Users?.FirstOrDefault(u => u.userType == UserType.HEAD || u.userType == UserType.MANAGER)?.FullName ?? "Chưa cập nhật"

        }).ToList();

        return new PaginatedResponse<DepartmentViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };
    }
}