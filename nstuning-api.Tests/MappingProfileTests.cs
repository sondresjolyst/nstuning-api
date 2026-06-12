using MapsterMapper;
using nstuning_api.Features.DynoRuns;
using nstuning_api.Models;
using Xunit;

namespace nstuning_api.Tests;

public class MappingProfileTests : TestBase
{
    private readonly IMapper _mapper = RealMapper;

    [Fact]
    public void DynoRun_To_Dto_SetsHasReport_True_WhenReportPresent()
    {
        var run = new DynoRun
        {
            Title = "Volvo 242 Turbo",
            Slug = "volvo-242-turbo",
            Report = new DynoRunReport { FileName = "r.pdf", ContentType = "application/pdf", StoredPath = "x.pdf", DynoRunId = 1 }
        };

        var dto = _mapper.Map<DynoRunDto>(run);

        Assert.True(dto.HasReport);
        Assert.Equal("volvo-242-turbo", dto.Slug);
    }

    [Fact]
    public void DynoRun_To_Dto_SetsHasReport_False_WhenNoReport()
    {
        var run = new DynoRun { Title = "Volvo 242 Turbo", Slug = "volvo-242-turbo" };
        var dto = _mapper.Map<DynoRunDto>(run);
        Assert.False(dto.HasReport);
    }
}
