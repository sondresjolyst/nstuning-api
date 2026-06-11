using Mapster;
using nstuning_api.Dtos.Auth;
using nstuning_api.Dtos.DynoRun;
using nstuning_api.Models;

namespace nstuning_api.Profiles
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegisterUserDto, User>()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Ignore(dest => dest.Id!);

            config.NewConfig<DynoRun, DynoRunDto>()
                .Map(dest => dest.HasReport, src => src.Report != null);
        }
    }
}
