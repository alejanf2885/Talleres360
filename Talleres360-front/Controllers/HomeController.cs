using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Dtos.Clientes;
using Talleres360.Dtos.Trabajos;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
public class HomeController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        Task<ApiResult<ClienteStatsResponse>> statsTask =
            api.GetResultAsync<ClienteStatsResponse>("api/v1/clientes/stats");

        Task<ApiResult<PagedResponse<TrabajoDto>>> ordenesAbiertasTask =
            api.GetResultAsync<PagedResponse<TrabajoDto>>("api/v1/trabajos?estado=ABIERTO&pageSize=1");

        Task<ApiResult<PagedResponse<TrabajoDto>>> ultimasOrdenesTask =
            api.GetResultAsync<PagedResponse<TrabajoDto>>("api/v1/trabajos?pageSize=5");

        Task<ApiResult<PagedResponse<CitaDto>>> citasTask =
            api.GetResultAsync<PagedResponse<CitaDto>>("api/v1/citas?estado=PENDIENTE&pageSize=5");

        await Task.WhenAll(statsTask, ordenesAbiertasTask, ultimasOrdenesTask, citasTask);

        ApiResult<ClienteStatsResponse>      statsResult    = statsTask.Result;
        ApiResult<PagedResponse<TrabajoDto>> abiertasResult = ordenesAbiertasTask.Result;
        ApiResult<PagedResponse<TrabajoDto>> ordenesResult  = ultimasOrdenesTask.Result;
        ApiResult<PagedResponse<CitaDto>>    citasResult    = citasTask.Result;

        DashboardViewModel vm = new()
        {
            TotalClientes         = statsResult.Success    ? (statsResult.Data?.TotalClientes ?? 0)         : 0,
            ClientesNuevosEsteMes = statsResult.Success    ? (statsResult.Data?.ClientesNuevosEsteMes ?? 0) : 0,
            OrdenesAbiertas       = abiertasResult.Success ? (abiertasResult.Data?.TotalCount ?? 0)         : 0,
            CitasPendientes       = citasResult.Success    ? (citasResult.Data?.TotalCount ?? 0)            : 0,
            UltimasOrdenes        = ordenesResult.Success  ? (ordenesResult.Data?.Data?.ToList()  ?? new()) : new(),
            ProximasCitas         = citasResult.Success    ? (citasResult.Data?.Data?.ToList()    ?? new()) : new()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
