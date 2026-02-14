using AutoMapper;
using MeetingManagement.Models;
using MeetingManagement.Models.DTOs;

namespace MeetingManagement.Config.Mapper;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<AccountModel, AccountViewModel>();
        CreateMap<CompanyModel, CompanyViewModel>();
        CreateMap<DepartmentModel, DepartmentViewModel>().ForMember(
            des => des.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : "")
        );
        CreateMap<MeetingModel, MeetingViewModel>().ForMember(
            des => des.RoomName, opt => opt.MapFrom(s => s.MeetingRoom != null ? s.MeetingRoom.Name : "")
        ).ForMember(
            des => des.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : "")
        ).ForMember(
            des => des.DepartmentName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : "")
        );
        CreateMap<MeetingRoomModel, MeetingRoomViewModel>().ForMember(
            des => des.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : "")
        );
        CreateMap<MeetingUserModel, MeetingUserViewModel>().ForMember(
            des => des.Title, opt => opt.MapFrom(s => s.Meeting != null ? s.Meeting.Title : "")
        ).ForMember(
            des => des.StartAt, opt => opt.MapFrom(s => s.Meeting != null ? s.Meeting.StartAt : DateTime.MinValue)
        ).ForMember(
            des => des.EndAt, opt => opt.MapFrom(s => s.Meeting != null ? s.Meeting.EndAt : DateTime.MinValue)
        ).ForMember(
            des => des.RoomName, opt => opt.MapFrom(s => s.Meeting != null && s.Meeting.MeetingRoom != null ? s.Meeting.MeetingRoom.Name : "")
        );

        CreateMap<UserModel, UserViewModel>().ForMember(
            des => des.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : "")
        ).ForMember(
            des => des.DepartmentName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : "")
        );
    }
}