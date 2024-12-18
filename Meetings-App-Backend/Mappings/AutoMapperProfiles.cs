
//namespace Meetings_App_Backend.AutoMapper
//{
//    public class MappingProfile : 
//    {
//    }
//}
using System;
using AutoMapper;
using Meetings_App_Backend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Meetings_App_Backend.AutoMapper;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Meetings, MeetingResponse>()
            .ForMember(dest => dest.Attendees, opt => opt.MapFrom(src => src.MeetingAttendees.Select(ma => new AttendeeModel
            {
                UserId = ma.UserId,  // Map UserId from MeetingAttendee to AttendeeModel
                // If you want to map more fields from User, you can map them here
                Email = ma.User.Email ?? "Unknown" // Optionally map Email
            }).ToList()));
        CreateMap<MeetingAttendee, AttendeeModel>().ReverseMap();
        CreateMap<User, AttendeeModel>();
    }
}