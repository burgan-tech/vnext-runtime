using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;
using System.Text.Json;
using BBT.Workflow.Scripting.Functions;
using System.Net.Http.Headers;
public class SendSignalrMapping :ScriptBase, IMapping
{
    public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = (task as HttpTask)!;
        
        // Dikkat! Signalr Url bilgisini set etmeniz gerekmektedir.
        var signalRUrl = string.Empty;

        httpTask.SetUrl(signalRUrl);
        var currentState = context.Workflow.GetState(context.Instance.CurrentState!);
        List<string> transitionList=currentState.TransitionKeys();
        
        // Transitions listesini oluştur
        var transitionItems = transitionList.Select(transitionKey => new TransitionItem
        {
            Name = transitionKey,
            Href = $"/{context.Runtime.Domain}/workflows/{context.Workflow.Key}/instances/{context.Instance.Id}/transitions/{transitionKey}"
        }).ToList();
        
        // Active Correlations listesini oluştur
        // Not: Context'te correlation bilgisi varsa kullanabilirsiniz
        var activeCorrelations = new List<ActiveCorrelationHref>();
        
        // Eğer context.Instance'ta Correlations varsa:
        var correlations = context.Instance.ChildCorrelations; // veya başka bir property
        if (correlations != null)
        {
            foreach (var correlation in correlations)
            {
                activeCorrelations.Add(new ActiveCorrelationHref
                {
                    CorrelationId = correlation.Id,
                    ParentState = correlation.ParentState,
                    SubFlowInstanceId = correlation.SubFlowInstanceId,
                    SubFlowType = correlation.SubFlowType,
                    SubFlowDomain = correlation.SubFlowDomain,
                    SubFlowName = correlation.SubFlowName,
                    SubFlowVersion = correlation.SubFlowVersion,
                    IsCompleted = correlation.IsCompleted,
                    Href = $"/{correlation.SubFlowDomain}/workflows/{correlation.SubFlowName}/instances/{correlation.SubFlowInstanceId}/data"
                });
            }
        }
        
        // Instance state output oluştur
        var dataHref = new DataHref
        {
            Href = $"/{context.Runtime.Domain}/workflows/{context.Workflow.Key}/instances/{context.Instance.Id}/data"
        };

        var viewHref = new ViewHref
        {
            Href = $"/{context.Runtime.Domain}/workflows/{context.Workflow.Key}/instances/{context.Instance.Id}/view",
            LoadData = true
        };

        var instanceOutput = new GetInstanceStateOutput
        {
            Data = dataHref,
            View = viewHref,
            State = context.Instance.CurrentState ?? string.Empty,
            Status = context.Instance.Status,
            ActiveCorrelations = activeCorrelations,
            Transitions = transitionItems,
            ETag = context.Instance.LatestData?.ETag ?? string.Empty
        };
        
        // SignalR request oluştur
        var signalRRequest = new SignalRRequest
        {
            Id = context.Instance.Id.ToString(),
            Source = "vnext",
            Type = "vnext.workflow",
            Subject = "workflow-completed",
            Data = instanceOutput
        };
        
        // Body'yi ayarla
        httpTask.SetBody(signalRRequest);
        
        // Headers'ı ayarla
        httpTask.SetHeaders(new Dictionary<string, string>
        {
            { "X-Device-Id",context.Headers?["x-device-id"] ?? string.Empty },
            { "X-Installation-Id",context.Headers?["x-installation-id"] ?? string.Empty }
        });
        
        return new ScriptResponse()
        {
            Data = transitionList,
            Headers =context.Headers
        };
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse()
        {
            Data = context.Body
        });
    }
}

// GetInstanceStateOutput model
public class GetInstanceStateOutput
{
    public DataHref Data { get; set; }
    public ViewHref View { get; set; }
    public string State { get; set; }
    public InstanceStatus? Status { get; set; }
    public List<ActiveCorrelationHref> ActiveCorrelations { get; set; }
    public List<TransitionItem> Transitions { get; set; }
    public string ETag { get; set; }
}

// DataHref model
public class DataHref
{
    public string Href { get; set; }
}

// ViewHref model
public class ViewHref
{
    public string Href { get; set; }
    public bool LoadData { get; set; }
}

// ActiveCorrelationHref model
public class ActiveCorrelationHref
{
    public Guid CorrelationId { get; set; }
    public string ParentState { get; set; }
    public Guid SubFlowInstanceId { get; set; }
    public SubFlowType SubFlowType { get; set; }
    public string SubFlowDomain { get; set; }
    public string SubFlowName { get; set; }
    public string SubFlowVersion { get; set; }
    public bool IsCompleted { get; set; }
    public string Href { get; set; }
}

// TransitionItem model
public class TransitionItem
{
    public string Name { get; set; }
    public string Href { get; set; }
}

// SignalR Request model
public class SignalRRequest
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string Type { get; set; }
    public string Subject { get; set; }
    public dynamic  Data { get; set; }
}

