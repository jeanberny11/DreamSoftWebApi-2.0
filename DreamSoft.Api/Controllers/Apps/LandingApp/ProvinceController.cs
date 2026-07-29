using DreamSoft.Application.Features.Apps.LandingApp.Province.GetProvinceByCountry;
using DreamSoft.Application.Features.Apps.LandingApp.Province.GetProvinces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class ProvinceController : LandingControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetProvinceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetProvinces(
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetProvincesQuery(), cancellationToken));

    [HttpGet("by-country")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetProvinceByCountryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetProvincesByCountry(
        [FromQuery] int countryId,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetProvinceByCountryQuery(countryId), cancellationToken));
}
