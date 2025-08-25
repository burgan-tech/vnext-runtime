using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting.Functions;

public class AddToCartMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        if (httpTask == null)
            throw new ArgumentException("Task must be of type HttpTask");
            
        var headers = new Dictionary<string, string?>
        {
            ["Authorization"] = $"Berear {context.Instance.Data?.login?.currentLogin?.accessToken}"
        };
        httpTask.SetHeaders(headers);
        
        // Check if cart already exists in context.Instance.Data, if not create it
        List<ProductDto> existingProducts = new List<ProductDto>();
        var isAddded = true;
        if (HasProperty(context.Instance.Data, "card"))
        {
            var cardData = GetPropertyValue(context.Instance.Data, "card");
            if (HasProperty(cardData, "products"))
            {
                isAddded = false;
                // Convert existing products to ProductDto list
                var productsData = GetPropertyValue(cardData, "products");
                if (productsData is IEnumerable<dynamic> products)
                {
                    foreach (var product in products)
                    {
                        existingProducts.Add(new ProductDto()
                        {
                            id = Convert.ToInt32(product?.id ?? 0),
                            quantity = Convert.ToInt32(product?.quantity ?? 1)
                        });
                    }
                }
            }
        }

        if (isAddded)
        {
            httpTask.Url = httpTask.Url.Replace("{CARD_ADD_OR_UPDATE_PATH}", "carts/add");
            httpTask.Method = "POST";
        }
        else
        {
            // WARN: It will simulate a POST request and will return the new created cart with a new id 
            // httpTask.Url = httpTask.Url.Replace("{CARD_ADD_OR_UPDATE_PATH}", $"carts/{context.Instance.Data?.card?.id}");
            httpTask.Url = httpTask.Url.Replace("{CARD_ADD_OR_UPDATE_PATH}", $"carts/10");
            httpTask.Method = "PUT";    
        }
        
        // Add the new product to the cart
        var newProduct = new ProductDto()
        {
            id = Convert.ToInt32(context.Body?.id ?? 0),
            quantity = 1
        };
        
        // Check if product already exists in cart, if so update quantity
        var existingProduct = existingProducts.FirstOrDefault(p => p.id == newProduct.id);
        if (existingProduct != null)
        {
            existingProduct.quantity += newProduct.quantity;
        }
        else
        {
            existingProducts.Add(newProduct);
        }

        var cardPayload = new
        {
            userId = Convert.ToInt32(context.Instance.Data?.login?.currentLogin?.id ?? 0),
            merge = true,
            products = existingProducts
        };

        httpTask.SetBody(cardPayload);

        // Store the updated cart in context.Instance.Data for persistence
        var updatedData = context.Instance.Data;
        if (updatedData != null)
        {
            updatedData.card = new
            {
                userId = Convert.ToInt32(context.Instance.Data?.login?.currentLogin?.id ?? 0),
                products = existingProducts
            };
        }

        return Task.FromResult(new ScriptResponse
        {
            Data = updatedData,
            Headers = null
        });
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Store authentication token for future requests
        var response = new ScriptResponse();

        var statusCode = context.Body?.statusCode ?? 500;
        if (statusCode >= 200 || statusCode <= 300)
        {
            response.Data = new
            {
                success = true,
                card = new
                {
                    success = true,
                    id = context.Body?.data?.id,
                    products = context.Body?.data?.products
                }
            };
        }
        else if (statusCode == 404)
        {
            // Not found - handle specifically
            response.Data = new
            {
                card = new
                {
                    success = false,
                    error = "Resource not found",
                    shouldRetry = false
                }
            };
        }
        else
        {
            // Server error - might want to retry
            response.Data = new
            {
                card = new
                {
                    success = false,
                    error = "Server error occurred",
                    shouldRetry = true,
                    retryAfter = 30 // seconds
                }
            };
        }

        return Task.FromResult(response);
    }

    internal class ProductDto
    {
        public int id { get; set; }
        public int quantity { get; set; }
    }
}