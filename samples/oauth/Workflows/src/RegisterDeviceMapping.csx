using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class RegisterDeviceMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var deviceData = new
        {
            userId = context.Instance.Data?.authentication?.userId,
            deviceId = context.Instance?.Data?.deviceId,
            deviceName = context.Body?.deviceName ?? "Unknown Device",
            deviceType = context.Body?.deviceType ?? "mobile",
            supportsPush = true
        };
        httpTask.SetBody(deviceData);
        var headers = new Dictionary<string, string?>
        {
            ["Content-Type"] = "application/json"
        };
        httpTask.SetHeaders(headers);
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data,
            Headers = null
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        var statusCode = context.Body.statusCode ?? 500;

        if (statusCode == 200 || statusCode == 201)
        {
            response.Data = new
            {
                deviceRegistration = new
                {
                    success = true,
                    deviceId = context.Body.data?.deviceId,
                    isRegistered = true,
                    supportsPush = true
                }
            };
        }
        else
        {
            response.Data = new
            {
                deviceRegistration = new
                {
                    success = false,
                    error = "Device registration failed"
                }
            };
        }

        return response;
    }
}