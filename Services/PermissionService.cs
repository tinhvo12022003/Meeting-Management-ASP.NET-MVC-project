using MeetingManagement.Common;
using MeetingManagement.Constant;
using MeetingManagement.Helper;
using MeetingManagement.Interface.IService;
using MeetingManagement.Interface.IUnitOfWork;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace MeetingManagement.Service;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserHelper _helper;
    private readonly IMemoryCache _cache;

    public PermissionService(IUnitOfWork unitOfWork, UserHelper helper, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _helper = helper;
        _cache = cache;
    }

    public async Task Create (PermissionCreateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.UserId) ||
            string.IsNullOrWhiteSpace(model.Controller) || 
            string.IsNullOrWhiteSpace(model.Action) 
        )
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var isExisted = await _unitOfWork.Permissions.IsExisted(model.UserId, model.Controller, model.Action);
        if (isExisted == true)
        {
            throw new Exception(MessageConstant.EXISTED);
        }

        var permission = new PermissionModel
        {
            Controller = model.Controller,
            Action = model.Action,
            FullPermission = model.FullPermission,
            View = model.View,
            Edit = model.Edit,
            Delete = model.Delete,
            Insert = model.Insert,
            EditAll = model.EditAll,
            DeleteAll = model.DeleteAll,
            InsertAll = model.InsertAll,
            UserId = model.UserId
        };
        await _unitOfWork.Permissions.Add(permission);
        await _unitOfWork.CommitAsync();
        _cache.Remove($"permissions_{model.UserId}");
    }

    public async Task Update (PermissionUpdateModel model)
    {
        if (
            string.IsNullOrWhiteSpace(model.UserId) ||
            string.IsNullOrWhiteSpace(model.Controller) || 
            string.IsNullOrWhiteSpace(model.Action) 
        )
        {
            throw new Exception(MessageConstant.EMPTY_STRING);
        }
        var permission = await _unitOfWork.Permissions.GetPermission(model.UserId, model.Controller, model.Action);
        if (permission == null)
        {
            throw new Exception(MessageConstant.NOT_EXISTED);
        }

        permission.UserId = model.UserId;
        permission.Controller = model.Controller;
        permission.Action = model.Action;
        permission.FullPermission = model.FullPermission;
        permission.View = model.View;
        permission.Edit = model.Edit;
        permission.Delete = model.Delete;
        permission.Insert = model.Insert;
        permission.EditAll = model.EditAll;
        permission.DeleteAll = model.DeleteAll;
        permission.InsertAll = model.InsertAll;

        await _unitOfWork.Permissions.Update(permission);
        await _unitOfWork.CommitAsync();
        _cache.Remove($"permissions_{model.UserId}");
    }


    public async Task<PaginatedResponse<PermissionViewModel>> Find (PaginatedRequest request)
    {
        var paginatedResult = await _unitOfWork.Permissions.GetPaginated(
            request,
            includes: new [] {"User"},
            searchFields: "UserId,Controller,Action"
        );

        var viewModels = paginatedResult.Items.Select(x => new PermissionViewModel
        {
            Username = x.User?.Username ?? string.Empty,
            Controller = x.Controller,
            Action = x.Action,
            FullPermission = x.FullPermission,
            View = x.View,
            Edit = x.Edit,
            Delete = x.Delete,
            Insert = x.Insert,
            EditAll = x.EditAll,
            DeleteAll = x.DeleteAll,
            InsertAll = x.InsertAll
        }).ToList();

        return new PaginatedResponse<PermissionViewModel>
        {
            Items = viewModels,
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };
    }

    public async Task AddBulkPermissions (PermissionCreateBulkModel model)
    {
        var user = await _unitOfWork.Users.GetById(model.UserId);
        if (user == null)
            throw new Exception("User không tồn tại.");
        if (model.Permissions == null || model.Permissions.Count == 0)
            throw new Exception("Danh sách permission không được rỗng.");

        // 1. Xóa tất cả permission cũ của user — filter trực tiếp ở DB, không load all vào RAM
        var oldPermissions = await _unitOfWork.Permissions.GetByUserId(model.UserId);
        _unitOfWork.Permissions.DeleteRange(oldPermissions);

        // 2. Map sang entity
        var permissionEntities = model.Permissions.Select(p => new PermissionModel
        {
            Controller     = p.Controller,
            Action         = p.Action,
            FullPermission  = p.FullPermission,
            View           = p.FullPermission || p.View,
            Edit           = p.FullPermission || p.Edit,
            Delete         = p.FullPermission || p.Delete,
            Insert         = p.FullPermission || p.Insert,
            EditAll        = p.FullPermission || p.EditAll,
            DeleteAll      = p.FullPermission || p.DeleteAll,
            InsertAll      = p.FullPermission || p.InsertAll,
            UserId         = model.UserId
        }).ToList();

        // 3. AddRange một lần duy nhất → commit 1 transaction
        await _unitOfWork.Permissions.AddRange(permissionEntities);
        await _unitOfWork.CommitAsync();
        _cache.Remove($"permissions_{model.UserId}");
    }

    public async Task<List<string>> GetPermissionsForUser(string userId)
    {
        var permissions = await _unitOfWork.Permissions.GetByUserId(userId);
        var permissionList = new List<string>();

        foreach (var p in permissions)
        {
            var controller = string.IsNullOrWhiteSpace(p.Controller) ? "*" : p.Controller;
            var action = string.IsNullOrWhiteSpace(p.Action) ? "*" : p.Action;

            if (p.FullPermission)
            {
                permissionList.Add($"{controller}.*");
                continue;
            }

            if (p.View)
                permissionList.Add($"{controller}.{action}.View");
            if (p.Insert)
                permissionList.Add($"{controller}.{action}.Insert");
            if (p.Edit)
                permissionList.Add($"{controller}.{action}.Edit");
            if (p.Delete)
                permissionList.Add($"{controller}.{action}.Delete");
            if (p.InsertAll)
                permissionList.Add($"{controller}.{action}.InsertAll");
            if (p.EditAll)
                permissionList.Add($"{controller}.{action}.EditAll");
            if (p.DeleteAll)
                permissionList.Add($"{controller}.{action}.DeleteAll");
        }

        return permissionList;
    }
}