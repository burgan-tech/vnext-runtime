using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Device Registered Push Rule - Device registered and push supported condition mapping
// This rule checks whether the device is registered and supports push notifications.
/// </summary>
public class DeviceRegisteredPushRule : IConditionMapping
{
    /// <summary>
    /// Check if the device is registered and supports push notifications
    /// </summary>
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            // Null check
            if (context?.Instance?.Data == null)
            {
                return false;
            }

            // Get device registration data
            var deviceRegistration = context.Instance.Data.deviceRegistration;
            
            if (deviceRegistration == null)
            {
                return false;
            }

            // Basic registration check
            var isRegistered = deviceRegistration.isRegistered;
            if (isRegistered != true)
            {
                return false;
            }

            // Push support check
            var supportsPush = deviceRegistration.supportsPush;
            if (supportsPush != true)
            {
                return false;
            }

            // Additional security checks
            // Device ID check
            var deviceId = deviceRegistration.deviceId;
            if (string.IsNullOrEmpty(deviceId?.ToString()))
            {
                return false;
            }

            // Last seen time check (30 days)
            // var lastSeenAt = deviceRegistration.lastSeenAt;
            // if (lastSeenAt != null)
            // {
            //     if (DateTime.TryParse(lastSeenAt.ToString(), out DateTime lastSeen))
            //     {
            //         if (DateTime.UtcNow.Subtract(lastSeen).TotalDays > 30)
            //         {
            //             return false;
            //         }
            //     }
            // }

            // // Registration time check (6 months)
            // var registeredAt = deviceRegistration.registeredAt;
            // if (registeredAt != null)
            // {
            //     if (DateTime.TryParse(registeredAt.ToString(), out DateTime regTime))
            //     {
            //         if (DateTime.UtcNow.Subtract(regTime).TotalDays > 180)
            //         {
            //             return false;
            //         }
            //     }
            // }

            return true;
        }
        catch (Exception ex)
        {
            // If an exception occurs, return false and write to audit log
            if (context?.MetaData != null)
            {
                context.MetaData["device_registered_push_rule_error"] = new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    timestamp = DateTime.UtcNow
                };
            }

            return false;
        }
    }
}