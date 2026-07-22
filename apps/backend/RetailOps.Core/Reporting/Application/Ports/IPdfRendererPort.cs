using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.ValueObjects;

namespace RetailOps.Core.Reporting.Application.Ports;

public interface IPdfRendererPort
{
    Task<byte[]> RenderAsync<TModel>(string templateId, TModel model, ReportOutputFormat format);
}
