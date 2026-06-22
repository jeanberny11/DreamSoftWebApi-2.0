using DreamSoft.Application.Features.Apps.LandingApp.Municipality.GetMunicipalityByProvince;
using DreamSoft.Application.Features.Apps.LandingApp.Municipality.GetMunicipalities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class MunicipalityController : LandingControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetMunicipalityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetMunicipalities(
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMunicipalitiesQuery(), cancellationToken));

    [HttpGet("by-province")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetMunicipalityByProvinceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetMunicipalitiesByProvince(
        [FromQuery] int provinceId,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMunicipalityByProvinceQuery(provinceId), cancellationToken));
}
